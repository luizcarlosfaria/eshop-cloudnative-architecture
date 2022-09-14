using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace eShopCloudNative.Architecture.Data;

[ExcludeFromCodeCoverage]
public class NHibernateConfigBuilder
{
    private readonly IServiceCollection services;

    private readonly List<Type> typesToFindMapping = new();
    private string schema;
    private string connectionStringKey;
    private bool registerSession = false;
    private bool registerStatelessSession = false;

    public NHibernateConfigBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public NHibernateConfigBuilder AddMappingsFromAssemblyOf<T>()
    {
        this.typesToFindMapping.Add(typeof(T));
        return this;
    }

    public NHibernateConfigBuilder Schema(string schema)
    {
        this.schema = schema;
        return this;
    }

    public NHibernateConfigBuilder ConnectionStringKey(string connectionStringKey)
    {
        this.connectionStringKey = connectionStringKey;
        return this;
    }

    public NHibernateConfigBuilder RegisterSession()
    {
        this.registerSession = true;
        return this;
    }

    public NHibernateConfigBuilder RegisterStatelessSession()
    {
        this.registerStatelessSession = true;
        return this;
    }

    internal void Build()
    {
        this.services.AddSingleton(sp =>
        {
            var aspnetConfiguration = sp.GetRequiredService<IConfiguration>();

            return Fluently
             .Configure(new NHibernate.Cfg.Configuration().SetNamingStrategy(PostgresNamingStragegy.Instance))
             .Database(
                 PostgreSQLConfiguration.PostgreSQL82
                     .ConnectionString(aspnetConfiguration.GetConnectionString(this.connectionStringKey))
                     .ShowSql()
                     .DefaultSchema(this.schema)
                 )
             .Mappings(it =>
             {

                 foreach (var type in this.typesToFindMapping)
                 {
                     it.FluentMappings.AddFromAssembly(type.Assembly);
                 }
             })
             .ExposeConfiguration(it => it.SetProperty("hbm2ddl.keywords", "auto-quote"))
             .BuildSessionFactory();

        });
        if (this.registerSession) this.services.AddSession();
        if (this.registerStatelessSession) this.services.AddStatelessSession();

    }

}
