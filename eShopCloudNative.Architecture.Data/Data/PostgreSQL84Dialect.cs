using NHibernate.Dialect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Data;
public class PostgreSQL84Dialect : PostgreSQL83Dialect
{
    public PostgreSQL84Dialect()
    {
        //this.RegisterColumnType(DbType.AnsiStringFixedLength, "character(255)");
        //this.RegisterColumnType(DbType.AnsiStringFixedLength, 8000, "character($l)");
        //this.RegisterColumnType(DbType.AnsiString, "character varying(255)");
        //this.RegisterColumnType(DbType.AnsiString, 8000, "character varying($l)");
        //this.RegisterColumnType(DbType.StringFixedLength, "character(255)");
        //this.RegisterColumnType(DbType.StringFixedLength, 4000, "character($l)");
        //this.RegisterColumnType(DbType.String, "character varying(255)");
        //this.RegisterColumnType(DbType.String, 4000, "character varying($l)");
    }
}
