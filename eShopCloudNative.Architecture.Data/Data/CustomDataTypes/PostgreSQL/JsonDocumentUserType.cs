using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NHibernate;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Data.CustomDataTypes.PostgreSQL;
public class JsonDocumentUserType : IUserType
{
    public virtual SqlType[] SqlTypes => new SqlType[] {
        new NpgSqlType(DbType.Object, NpgsqlDbType.Json)
    };

    public Type ReturnedType => typeof(JsonDocument);

    public bool IsMutable => true;

    public object Assemble(object cached, object owner)
    {
        return cached;
    }

    public object DeepCopy(object value)
    {
        if (value == null)
            return null;

        string json = JsonSerializer.Serialize(value);
        object obj = JsonDocument.Parse(json);
        return obj;
    }

    public object Disassemble(object value)
    {
        return value;
    }

    public new bool Equals(object x, object y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;
        if (x is JsonDocument token1 && y is JsonDocument token2)
            return token1.Equals(token2);
        return x.Equals(y);
    }

    public int GetHashCode(object x)
    {
        return x?.GetHashCode() ?? 0;
    }

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var json = (string)NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner);
        return string.IsNullOrEmpty(json) ? null : JsonDocument.Parse(json);
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value == null)
        {
            NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
        }
        else
        {
            var json = ((JsonDocument)value);
            NHibernateUtil.String.NullSafeSet(cmd, json, index, session);
        }
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }
}
