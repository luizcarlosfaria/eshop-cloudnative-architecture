using eShopCloudNative.Architecture.Bootstrap.Postgres;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests;
public class PostgresBootstrapperServiceTests
{
    /*
     public System.Net.NetworkCredential SysAdminUser { get; set; }
    public System.Net.DnsEndPoint ServerEndpoint { get; set; }
    public System.Net.NetworkCredential AppUser { get; set; }
    public string DatabaseToCreate { get; set; }
    public string InitialDatabase { get; set; }

    public string SchemaToSetPermissions { get; set; }


    public IConfiguration Configuration { get; set; }

    public Type MigrationType { get; set; }
     */

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
            .Returns(new FakeIConfigurationSection()
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

        var createAppUserIDbCommandMock = new Mock<IDbCommand>();
        createAppUserIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(1l);

        var createDatabaseIDbCommandMock = new Mock<IDbCommand>();
        createDatabaseIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(1l);

        var setPermissionsIDbCommandMock = new Mock<IDbCommand>();

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

        var createAppUserIDbCommandMock = new Mock<IDbCommand>();
        createAppUserIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0l);

        var createDatabaseIDbCommandMock = new Mock<IDbCommand>();
        createDatabaseIDbCommandMock.Setup(it => it.ExecuteScalar()).Returns(0l);

        var setPermissionsIDbCommandMock = new Mock<IDbCommand>();

        svc.DbConnectionMock.SetupSequence(it => it.CreateCommand())
            .Returns(createAppUserIDbCommandMock.Object)
            .Returns(createDatabaseIDbCommandMock.Object)
            .Returns(setPermissionsIDbCommandMock.Object);

        await svc.ExecuteAsync();

        createAppUserIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        createDatabaseIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Once());
        setPermissionsIDbCommandMock.Verify(it => it.ExecuteNonQuery(), Times.Exactly(2));

    }

    private static IConfiguration BuildConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("boostrap:postgres"))
            .Returns(new FakeIConfigurationSection()
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
    public Mock<IDbConnection> DbConnectionMock { get; set; } = new Mock<IDbConnection>();

    protected override IDbConnection BuildConnection(string connectionString) => dbConnection;

    private IDbConnection dbConnection;

    public PostgresTestService()
    {
        this.dbConnection = this.DbConnectionMock.Object;
    }
}