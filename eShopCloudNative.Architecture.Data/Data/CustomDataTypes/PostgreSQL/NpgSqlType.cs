using NHibernate.SqlTypes;
using NpgsqlTypes;
using System.Data;

namespace eShopCloudNative.Architecture.Data.CustomDataTypes.PostgreSQL;

/// <summary>
/// https://github.com/beginor/nhibernate-extensions/tree/master/src/NHibernate.Extensions.NpgSql
/// </summary>
public class NpgSqlType : SqlType
{
    public NpgsqlDbType NpgDbType { get; }

    public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType)
        : base(dbType)
    {
        this.NpgDbType = npgDbType;
    }

    // ReSharper disable once UnusedMember.Global
    public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType, int length)
        : base(dbType, length)
    {
        this.NpgDbType = npgDbType;
    }

    // ReSharper disable once UnusedMember.Global
    public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale)
        : base(dbType, precision, scale)
    {
        this.NpgDbType = npgDbType;
    }
}