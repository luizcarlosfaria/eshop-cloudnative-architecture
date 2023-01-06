using eShopCloudNative.Architecture.Bootstrap.Postgres;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;
public class PostgresBootstrapperServiceTests
{
    [Fact]
    public async Task PostgresBootstrapperServiceBossalitiesAsync()
    {
        var sysAdminUser = new System.Net.NetworkCredential(){ UserName = "a", Password = "b" };
        var serverEndpoint = new System.Net.DnsEndPoint("127.00.0.1", 1);
        var appUser = new System.Net.NetworkCredential(){ UserName = "a", Password = "b" };
        var databaseToCreate = "a";
        var initialDatabase = "b";
        var schemaToSetPermissions = "c";

        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("boostrap:postgres"))
            .Returns(new FakeConfigurationSection()
            {
                Key = "boostrap:postgres",
                Value = "true"
            });
        var configurationInstance = configurationMock.Object;
        var migrationType = this.GetType();

        var svc = new PostgresBootstrapperService()
        {
            SysAdminUser = sysAdminUser,
            ServerEndpoint = serverEndpoint,
            AppUser = appUser,
            DatabaseToCreate = databaseToCreate,
            InitialDatabase = initialDatabase,
            SchemaToSetPermissions = schemaToSetPermissions,
            Configuration = configurationInstance,
            MigrationType = migrationType,
        };

        svc.SysAdminUser.Should().Be(sysAdminUser);
        svc.ServerEndpoint.Should().Be(serverEndpoint);
        svc.AppUser.Should().Be(appUser);
        svc.DatabaseToCreate.Should().Be(databaseToCreate);
        svc.InitialDatabase.Should().Be(initialDatabase);
        svc.SchemaToSetPermissions.Should().Be(schemaToSetPermissions);
        svc.Configuration.Should().Be(configurationInstance);
        svc.MigrationType.Should().Be(migrationType);

        await svc.InitializeAsync();
    }

    [Fact]
    public async Task EvictIfObjectsExists()
    {
        IConfiguration configurationInstance = BuildConfiguration();

        var migrationType = this.GetType();

        var svc = new PostgresTestService()
        {
            SysAdminUser = new System.Net.NetworkCredential(){ UserName = "a", Password = "b" },
            ServerEndpoint = new System.Net.DnsEndPoint("127.00.0.1", 1),
            AppUser = new System.Net.NetworkCredential() { UserName = "a", Password = "b" },
            DatabaseToCreate = "DatabaseToCreate",
            InitialDatabase = "InitialDatabase",
            SchemaToSetPermissions = "SchemaToSetPermissions",
            Configuration = configurationInstance,
            //MigrationType = migrationType,
        };

        var paramListMock = new Mock<IDataParameterCollection>();
        paramListMock.Setup(it => it.Add(It.IsAny<IDbDataParameter>())).Returns(1);

        var paramMock = new Mock<IDbDataParameter>();
        paramMock.SetupAllProperties();

        var createAppUserIDbCommandMock = new Mock<IDbCommand>();
        createAppUserIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(1L);
        createAppUserIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createAppUserIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        var createDatabaseIDbCommandMock = new Mock<IDbCommand>();
        createDatabaseIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(1L);
        createDatabaseIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createDatabaseIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        var setPermissionsIDbCommandMock = new Mock<IDbCommand>();
        setPermissionsIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        setPermissionsIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        svc.DbConnectionMock.SetupSequence(it => it.CreateCommand())
            .Returns(createAppUserIDbCommandMock.Object)
            .Returns(createDatabaseIDbCommandMock.Object)
            .Returns(setPermissionsIDbCommandMock.Object);

        await svc.ExecuteAsync();

        createAppUserIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Never());
        createDatabaseIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Never());
        setPermissionsIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Exactly(2));

    }


    [Fact]
    public async Task CompleteFlow()
    {
        IConfiguration configurationInstance = BuildConfiguration();

        var migrationType = this.GetType();

        var svc = new PostgresTestService()
        {
            SysAdminUser = new System.Net.NetworkCredential(){ UserName = "a", Password = "b" },
            ServerEndpoint = new System.Net.DnsEndPoint("127.00.0.1", 1),
            AppUser = new System.Net.NetworkCredential() { UserName = "a", Password = "b" },
            DatabaseToCreate = "DatabaseToCreate",
            InitialDatabase = "InitialDatabase",
            SchemaToSetPermissions = "SchemaToSetPermissions",
            Configuration = configurationInstance,
            //MigrationType = migrationType,
        };
        var paramListMock = new Mock<IDataParameterCollection>();
        paramListMock.Setup(it => it.Add(It.IsAny<IDbDataParameter>())).Returns(1);

        var paramMock = new Mock<IDbDataParameter>();
        paramMock.SetupAllProperties();


        var createAppUserIDbCommandMock = new Mock<IDbCommand>();
        createAppUserIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0L);
        createAppUserIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createAppUserIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        var createDatabaseIDbCommandMock = new Mock<IDbCommand>();
        createDatabaseIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0L);
        createDatabaseIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createDatabaseIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);


        var setPermissionsIDbCommandMock = new Mock<IDbCommand>();
        setPermissionsIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0L);
        setPermissionsIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        setPermissionsIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        svc.DbConnectionMock.SetupSequence(it => it.CreateCommand())
            .Returns(createAppUserIDbCommandMock.Object)
            .Returns(createDatabaseIDbCommandMock.Object)
            .Returns(setPermissionsIDbCommandMock.Object);

        await svc.ExecuteAsync();

        createAppUserIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        createDatabaseIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        setPermissionsIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Exactly(2));

    }


    [Fact]
    public async Task MigrationsTest()
    {
        var migrationRunnerMock = new Mock<IMigrationRunner>();

        var rootSP = new ServiceCollection()
            .AddScoped((sp) => migrationRunnerMock.Object)
            .AddScoped(sp => new CustomVersionTableMetaData())
            .BuildServiceProvider();

        var svc = new PostgresTestService(rootSP)
        {
            SysAdminUser = new System.Net.NetworkCredential(){ UserName = "a", Password = "b" },
            ServerEndpoint = new System.Net.DnsEndPoint("127.00.0.1", 1),
            AppUser = new System.Net.NetworkCredential() { UserName = "a", Password = "b" },
            DatabaseToCreate = "DatabaseToCreate",
            InitialDatabase = "InitialDatabase",
            SchemaToSetPermissions = "SchemaToSetPermissions",
            Configuration = BuildConfiguration(),
            MigrationType = this.GetType(),
        };

        var paramListMock = new Mock<IDataParameterCollection>();
        paramListMock.Setup(it => it.Add(It.IsAny<IDbDataParameter>())).Returns(1);

        var paramMock = new Mock<IDbDataParameter>();
        paramMock.SetupAllProperties();


        var createAppUserIDbCommandMock = new Mock<IDbCommand>();
        createAppUserIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0L);
        createAppUserIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createAppUserIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        var createDatabaseIDbCommandMock = new Mock<IDbCommand>();
        createDatabaseIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0L);
        createDatabaseIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        createDatabaseIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        var setPermissionsIDbCommandMock = new Mock<IDbCommand>();
        setPermissionsIDbCommandMock.Setup(it => it.CreateParameter()).Returns(paramMock.Object);
        setPermissionsIDbCommandMock.SetupGet(it => it.Parameters).Returns(paramListMock.Object);

        svc.DbConnectionMock.SetupSequence(it => it.CreateCommand())
            .Returns(createAppUserIDbCommandMock.Object)
            .Returns(createDatabaseIDbCommandMock.Object)
            .Returns(setPermissionsIDbCommandMock.Object);

        await svc.ExecuteAsync();

        createAppUserIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        createDatabaseIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        setPermissionsIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Exactly(2));

        migrationRunnerMock.Verify(it => it.MigrateUp(), Times.Once());

    }

    private static IConfiguration BuildConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("boostrap:postgres"))
            .Returns(new FakeConfigurationSection()
            {
                Key = "boostrap:postgres",
                Value = "true"
            });
        var configurationInstance = configurationMock.Object;
        return configurationInstance;
    }



}


public class PostgresTestService : PostgresBootstrapperService
{
    private IDbConnection dbConnection;
    public Mock<IDbConnection> DbConnectionMock { get; set; } = new Mock<IDbConnection>();
    protected override IDbConnection BuildConnection(string connectionString) => this.dbConnection;

    private IServiceProvider serviceProvider;
    protected override IServiceProvider BuildServiceProviderForMigration() => this.serviceProvider;


    public PostgresTestService(IServiceProvider serviceProvider = null)
    {
        this.dbConnection = this.DbConnectionMock.Object;
        this.serviceProvider = serviceProvider;
    }




}