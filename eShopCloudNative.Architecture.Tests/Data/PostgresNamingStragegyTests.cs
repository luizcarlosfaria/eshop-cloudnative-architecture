using eShopCloudNative.Architecture.Data;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Data;
public class PostgresNamingStragegyTests
{
    [Fact]
    public void ClassToTableName() => PostgresNamingStragegy.Instance.ClassToTableName("TesteClass").Should().Be("teste_class");
    
    [Fact]
    public void ColumnName() => PostgresNamingStragegy.Instance.ColumnName("NomeColuna").Should().Be("\"NomeColuna\"");
    
    [Fact]
    public void TableName() => PostgresNamingStragegy.Instance.TableName("NomeTabela").Should().Be("\"NomeTabela\"");
    
    [Fact]
    public void LogicalColumnName() => PostgresNamingStragegy.Instance.LogicalColumnName("teste_property", "TesteProperty").Should().Be("teste_property");
    
    [Fact]
    public void PropertyToColumnName() => PostgresNamingStragegy.Instance.PropertyToColumnName("TesteClass").Should().Be("teste_class");
    
    [Fact]
    public void PropertyToTableName() => PostgresNamingStragegy.Instance.PropertyToTableName("Categoria", "categoria").Should().Be("categoria");

}
