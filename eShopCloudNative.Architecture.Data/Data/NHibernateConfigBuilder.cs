using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using eShopCloudNative.Architecture.Extensions;

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
    private bool showSQL = false;
    private bool formatSql = false;

    public NHibernateConfigBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public NHibernateConfigBuilder AddMappingsFromAssemblyOf<T>()
        => this.Fluent(() => this.typesToFindMapping.Add(typeof(T)));

    public NHibernateConfigBuilder Schema(string schema)
        => this.Fluent(() => this.schema = schema);

    public NHibernateConfigBuilder ConnectionStringKey(string connectionStringKey)
        => this.Fluent(() => this.connectionStringKey = connectionStringKey);

    public NHibernateConfigBuilder RegisterSession()
        => this.Fluent(() => this.registerSession = true);

    public NHibernateConfigBuilder ShowSQL(bool showSQL = true)
        => this.Fluent(() => this.showSQL = showSQL);

    public NHibernateConfigBuilder FormatSql(bool formatSql = true)
        => this.Fluent(() => this.formatSql = formatSql);

    public NHibernateConfigBuilder RegisterStatelessSession()
        => this.Fluent(() => this.registerStatelessSession = true);

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
                     .If(it => this.showSQL, it => it.ShowSql())
                     .If(it => this.formatSql, it => it.FormatSql())
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
