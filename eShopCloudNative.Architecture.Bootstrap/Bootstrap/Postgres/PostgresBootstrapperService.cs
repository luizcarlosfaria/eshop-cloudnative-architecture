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
using Ardalis.GuardClauses;

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
        Guard.Against.Null(this.SysAdminUser, nameof(this.SysAdminUser));
        Guard.Against.Null(this.ServerEndpoint, nameof(this.ServerEndpoint));
        Guard.Against.Null(this.AppUser, nameof(this.AppUser));
        Guard.Against.Null(this.DatabaseToCreate, nameof(this.DatabaseToCreate));
        Guard.Against.Null(this.InitialDatabase, nameof(this.InitialDatabase));
        Guard.Against.Null(this.Configuration, nameof(this.Configuration));

        return Task.CompletedTask;
    }

    public async Task ExecuteAsync()
    {
        if (this.Configuration.GetValue<bool>("boostrap:postgres"))
        {
            using var rootConnection = new NpgsqlConnection(this.BuildConnectionString(this.InitialDatabase, this.SysAdminUser));
            await rootConnection.OpenAsync();

            await this.CreateAppUser(rootConnection);
            await this.CreateDatabase(rootConnection);
            await this.ApplyMigrations();

            using var databaseConnection = new NpgsqlConnection(this.BuildConnectionString(this.DatabaseToCreate, this.SysAdminUser));
            await databaseConnection.OpenAsync();
            await this.SetPermissions(databaseConnection);
        }
        else
        {
            //TODO: Logar dizendo que está ignorando
        }
    }

    private async Task CreateAppUser(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(rolname) FROM pg_catalog.pg_roles WHERE  rolname = '{this.AppUser.UserName}'";

        long qtd = (long) await command.ExecuteScalarAsync();

        if (qtd == 0)
        {

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

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task CreateDatabase(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(datname) FROM pg_database WHERE datname = '{this.DatabaseToCreate}'";

        long qtd = (long) await command.ExecuteScalarAsync();

        if (qtd == 0)
        {
            command.CommandText = @$"
                        CREATE DATABASE {this.DatabaseToCreate} 
                            WITH 
                            OWNER = {this.AppUser.UserName}
                            ENCODING = 'UTF8'
                            CONNECTION LIMIT = -1; ";
            await command.ExecuteNonQueryAsync();
        }

    }

    private async Task SetPermissions(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {this.SchemaToSetPermissions} TO {this.AppUser.UserName};";
        await command.ExecuteNonQueryAsync();

        command.CommandText = $"GRANT UPDATE, USAGE, SELECT ON ALL SEQUENCES IN SCHEMA {this.SchemaToSetPermissions} TO {this.AppUser.UserName};";
        await command.ExecuteNonQueryAsync();

    }

    private string BuildConnectionString(string database, System.Net.NetworkCredential credential) => $"server={this.ServerEndpoint?.Host ?? "localhost"};Port={this.ServerEndpoint?.Port};Database={database};User Id={credential.UserName};Password={credential.Password};";

    private Task ApplyMigrations()
    {
        if (this.MigrationType != null)
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres11_0()
                    .WithGlobalConnectionString(this.BuildConnectionString(this.DatabaseToCreate, this.SysAdminUser))
                    .ScanIn(this.MigrationType.Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Tags = new[] { "blue" };
                })
                .BuildServiceProvider(false);

            using (var scope = serviceProvider.CreateScope())
            {
                // Instantiate the runner
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }
        else
        {
            //TODO: Logar informando que não foi executada a migration porque o Tipo estava nulo
        }
        return Task.CompletedTask;
    }

}
