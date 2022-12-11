using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace eShopCloudNative.Architecture.Data.CustomDataTypes.PostgreSQL;

public class Json : IUserType
{
    public virtual SqlType[] SqlTypes => new SqlType[] {
        new NpgSqlType(DbType.Object, NpgsqlDbType.Json)
    };

    public Type ReturnedType => typeof(JToken);

    public bool IsMutable => true;

    public object Assemble(object cached, object owner)
    {
        return cached;
    }

    public object DeepCopy(object value)
    {
        if (value == null)
            return null;
        string json = JsonConvert.SerializeObject(value);
        object obj = JsonConvert.DeserializeObject(json, value.GetType());
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
        if (x is JToken token1 && y is JToken token2)
            return token1.Equals(token2);
        return x.Equals(y);
    }

    public int GetHashCode(object x)
    {
        return x?.GetHashCode() ?? 0;
    }

    public object NullSafeGet(
        DbDataReader rs,
        string[] names,
        ISessionImplementor session,
        object owner
    )
    {
        if (names.Length != 1)
        {
            throw new InvalidOperationException(
                "Only expecting one column..."
            );
        }
        if (rs[names[0]] is string val)
            return JToken.Parse(val);
        return null;
    }

    public void NullSafeSet(
        DbCommand cmd,
        object value,
        int index,
        ISessionImplementor session
    )
    {
        var parameter = (NpgsqlParameter)cmd.Parameters[index];
        parameter.Value = value ?? DBNull.Value;
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }
}
