using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHibernate.Cfg.Mappings;

namespace eShopCloudNative.Architecture.Data;
public class PostgresNamingStragegy : INamingStrategy
{
    private static readonly INamingStrategy improvedNamingStrategy = NHibernate.Cfg.ImprovedNamingStrategy.Instance;

    private static PostgresNamingStragegy instance;

    public static INamingStrategy Instance { get { return instance ?? (instance = new PostgresNamingStragegy()); } }

    protected PostgresNamingStragegy() { }

    public string ClassToTableName(string className) => improvedNamingStrategy.ClassToTableName(className);

    public string ColumnName(string columnName) => columnName.UnderQuotes();

    public string TableName(string tableName) => tableName.UnderQuotes();

    public string LogicalColumnName(string columnName, string propertyName) => improvedNamingStrategy.LogicalColumnName(columnName, propertyName);

    public string PropertyToColumnName(string propertyName) => improvedNamingStrategy.PropertyToColumnName(propertyName);

    public string PropertyToTableName(string className, string propertyName) => improvedNamingStrategy.PropertyToTableName(className, propertyName);

}