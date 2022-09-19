using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Configuration;
using eShopCloudNative.Architecture.Bootstrap;
using Dawn;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using eShopCloudNative.Architecture.Extensions;

namespace eShopCloudNative.Architecture.Bootstrap.Postgres;

public class PostgresBootstrapperService : IBootstrapperService
{
    public System.Net.NetworkCredential SysAdminUser { get; set; }

    public System.Net.DnsEndPoint ServerEndpoint { get; set; }

    public System.Net.NetworkCredential AppUser { get; set; }

    public string DatabaseToCreate { get; set; }

    public string InitialDatabase { get; set; }

    public string SchemaToSetPermissions { get; set; }

    public IConfiguration Configuration { get; set; }

    public Type MigrationType { get; set; }

    public Task InitializeAsync()
    {
        Guard.Argument(this.SysAdminUser, nameof(this.SysAdminUser)).NotNull();
        Guard.Argument(this.SysAdminUser.UserName, nameof(this.SysAdminUser.UserName)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.SysAdminUser.Password, nameof(this.SysAdminUser.Password)).NotNull().NotEmpty().NotWhiteSpace();

        Guard.Argument(this.ServerEndpoint, nameof(this.ServerEndpoint)).NotNull();
        Guard.Argument(this.ServerEndpoint.Host, $"{nameof(this.ServerEndpoint)}.{nameof(this.ServerEndpoint.Host)}").NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.ServerEndpoint.Port, $"{nameof(this.ServerEndpoint)}.{nameof(this.ServerEndpoint.Port)}").NotZero();

        Guard.Argument(this.AppUser, nameof(this.AppUser)).NotNull();
        Guard.Argument(this.AppUser.UserName, $"{nameof(this.AppUser)}.{nameof(this.AppUser.UserName)}").NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.AppUser.Password, $"{nameof(this.AppUser)}.{nameof(this.AppUser.Password)}").NotNull().NotEmpty().NotWhiteSpace();

        Guard.Argument(this.DatabaseToCreate, nameof(this.DatabaseToCreate)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.InitialDatabase, nameof(this.InitialDatabase)).NotNull().NotEmpty().NotWhiteSpace();

        Guard.Argument(this.Configuration, nameof(this.Configuration)).NotNull();

        return Task.CompletedTask;
    }



    public Task ExecuteAsync()
    {
        if (this.Configuration.GetFlag("boostrap", "postgres"))
        {
            Log.Information("{svc} Iniciando... ", nameof(PostgresBootstrapperService));

            using var rootConnection = this.BuildConnection(this.BuildConnectionString(this.InitialDatabase, this.SysAdminUser));
            rootConnection.Open();

            this.CreateAppUser(rootConnection);
            this.CreateDatabase(rootConnection);
            this.ApplyMigrations();

            using var databaseConnection = this.BuildConnection(this.BuildConnectionString(this.DatabaseToCreate, this.SysAdminUser));
            databaseConnection.Open();
            this.SetPermissions(databaseConnection);

            Log.Information("{svc} Finalizado com sucesso!!! ", nameof(PostgresBootstrapperService));
        }
        else
        {
            Log.Information("{svc} Bootstrap ignorado por configuração ", nameof(PostgresBootstrapperService));
        }

        return Task.CompletedTask;
    }


    private void CreateAppUser(IDbConnection connection)
    {
        Log.Information("{svc} CreateAppUser... ", nameof(PostgresBootstrapperService));

        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(rolname) FROM pg_catalog.pg_roles WHERE  rolname = '{this.AppUser.UserName}'";

        long qtd = (long) command.ExecuteScalar();

        if (qtd == 0)
        {
            Log.Information("{svc} Criando usuário '{UserName}'...", nameof(PostgresBootstrapperService), this.AppUser.UserName);

            command.CommandText = @$"
                    CREATE ROLE {this.AppUser.UserName} WITH
	                    LOGIN
	                    NOSUPERUSER
	                    NOCREATEDB
	                    NOCREATEROLE
	                    INHERIT
	                    NOREPLICATION
	                    CONNECTION LIMIT -1
	                    PASSWORD '{this.AppUser.Password}'; ";

            command.ExecuteNonQuery();

            Log.Information("{svc} Usuário '{UserName}' criado com sucesso!!", nameof(PostgresBootstrapperService), this.AppUser.UserName);
        }
        else
        {
            Log.Information("{svc} Usuário '{UserName}' já existe, não foi criado!!", nameof(PostgresBootstrapperService), this.AppUser.UserName);
        }
    }

    private void CreateDatabase(IDbConnection connection)
    {
        Log.Information("{svc} CreateDatabase... ", nameof(PostgresBootstrapperService));

        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(datname) FROM pg_database WHERE datname = '{this.DatabaseToCreate}'";

        long qtd = (long) command.ExecuteScalar();

        if (qtd == 0)
        {
            Log.Information("{svc} Criando DATABASE '{DatabaseToCreate}'...", nameof(PostgresBootstrapperService), this.DatabaseToCreate);

            command.CommandText = @$"
                        CREATE DATABASE {this.DatabaseToCreate} 
                            WITH 
                            OWNER = {this.AppUser.UserName}
                            ENCODING = 'UTF8'
                            CONNECTION LIMIT = -1; ";

            command.ExecuteNonQuery();

            Log.Information("{svc} DATABASE '{DatabaseToCreate}' criado com sucesso!!", nameof(PostgresBootstrapperService), this.DatabaseToCreate);
        }
        else
        {
            Log.Information("{svc} DATABASE '{DatabaseToCreate}' já existe, não foi criado!!", nameof(PostgresBootstrapperService), this.DatabaseToCreate);
        }

    }

    private void SetPermissions(IDbConnection connection)
    {
        Log.Information("{svc} SetPermissions... ", nameof(PostgresBootstrapperService));

        using var command = connection.CreateCommand();

        command.CommandText = $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {this.SchemaToSetPermissions} TO {this.AppUser.UserName};";

        command.ExecuteNonQuery();

        command.CommandText = $"GRANT UPDATE, USAGE, SELECT ON ALL SEQUENCES IN SCHEMA {this.SchemaToSetPermissions} TO {this.AppUser.UserName};";

        command.ExecuteNonQuery();

        Log.Information("{svc} Permissões a Sequences e Tabelas concedidas ao usuário '{UserName}' no schema '{SchemaToSetPermissions}'... ", nameof(PostgresBootstrapperService), this.AppUser.UserName, this.SchemaToSetPermissions);

    }

    [ExcludeFromCodeCoverage]
    protected virtual IDbConnection BuildConnection(string connectionString)
        => new NpgsqlConnection(connectionString);

    protected virtual string BuildConnectionString(string database, System.Net.NetworkCredential credential)
        => $"server={this.ServerEndpoint.Host};Port={this.ServerEndpoint.Port};Database={database};User Id={credential.UserName};Password={credential.Password};";

    private void ApplyMigrations()
    {
        Log.Information("{svc} ApplyMigrations... ", nameof(PostgresBootstrapperService));

        if (this.MigrationType != null)
        {
            Log.Debug("{svc} Criando service provider... ", nameof(PostgresBootstrapperService));

            IServiceProvider serviceProvider = this.BuildServiceProviderForMigration();

            Log.Debug("{svc} Criando escopo... ", nameof(PostgresBootstrapperService));

            using (var scope = serviceProvider.CreateScope())
            {
                Log.Debug("{svc} obtendo IMigrationRunner do escopo... ", nameof(PostgresBootstrapperService));

                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                Log.Information("{svc} Executando IMigrationRunner.MigrateUp()...", nameof(PostgresBootstrapperService));

                runner.MigrateUp();

                Log.Information("{svc} IMigrationRunner.MigrateUp() finalizado com sucesso!!! ", nameof(PostgresBootstrapperService));

                Log.Information("{svc} ApplyMigrations finalizado com sucesso!!! ", nameof(PostgresBootstrapperService));
            }
        }
        else
        {
            Log.Information("{svc} ApplyMigrations ignorado por configuração (MigrationType é nulo)", nameof(PostgresBootstrapperService));
        }
    }

    [ExcludeFromCodeCoverage]
    protected virtual IServiceProvider BuildServiceProviderForMigration()
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(this.ConfigureRunner)
            .Configure<RunnerOptions>(this.ConfigureOptions)
            .BuildServiceProvider(false);
    }

    [ExcludeFromCodeCoverage]
    protected void ConfigureRunner(IMigrationRunnerBuilder migrationRunnerBuilder) =>
        migrationRunnerBuilder.AddPostgres11_0()
                .WithGlobalConnectionString(this.BuildConnectionString(this.DatabaseToCreate, this.SysAdminUser))
                .ScanIn(this.MigrationType.Assembly).For.Migrations();

    [ExcludeFromCodeCoverage]
    protected void ConfigureOptions(RunnerOptions runnerOptions) => runnerOptions.Tags = new[] { "blue" };
}
