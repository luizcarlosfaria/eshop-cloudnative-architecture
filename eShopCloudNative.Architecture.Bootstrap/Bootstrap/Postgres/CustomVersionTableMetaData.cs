using FluentMigrator.Runner.VersionTableInfo;

namespace eShopCloudNative.Architecture.Bootstrap.Postgres;

[VersionTableMetaData]
public class CustomVersionTableMetaData : IVersionTableMetaData
{
    public virtual string SchemaName { get; set; } = "";

    public virtual string TableName { get; set; } = "_VersionInfo";

    public virtual string ColumnName { get; set; } = "Version";

    public virtual string UniqueIndexName { get; set; } = "UC_Version";

    public virtual string AppliedOnColumnName { get; set; } = "AppliedOn";

    public virtual string DescriptionColumnName { get; set; } = "Description";

    public virtual bool OwnsSchema => false;

    public object ApplicationContext { get; set; }

}
