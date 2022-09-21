using eShopCloudNative.Architecture.Data;
using FluentMigrator;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Data;

[ExcludeFromCodeCoverage]
public static class FluentMigratorExtensions
{
    public static ICreateTableWithColumnSyntax Table(this ICreateExpressionRoot it, string tableName, string schema)
        => it.Table(tableName.UnderQuotes()).InSchema(schema.UnderQuotes());

    public static ICreateTableWithColumnSyntax NxNTable<T1, T2>(this ICreateExpressionRoot it, string tableName, string schema, Expression<Func<T1, int>> memberExpression1, Expression<Func<T2, int>> memberExpression2)
    {
        string LTable = typeof(T1).Name;
        string LFK = $"FK_{LTable.ToUpperInvariant()}_TO_{tableName.ToUpperInvariant()}";

        string RTable = typeof(T2).Name;
        string RFK = $"FK_{RTable.ToUpperInvariant()}_TO_{tableName.ToUpperInvariant()}";


        return it.Table(tableName.UnderQuotes()).InSchema(schema.UnderQuotes())

        .WithColumn(memberExpression1.GetPropertyName().UnderQuotes()).AsInt32().Nullable().PrimaryKey()
               .ForeignKey(LFK, schema.UnderQuotes(), LTable.UnderQuotes(), memberExpression1.GetPropertyName().UnderQuotes())

        .WithColumn(memberExpression2.GetPropertyName().UnderQuotes()).AsInt32().Nullable().PrimaryKey()
               .ForeignKey(RFK, schema.UnderQuotes(), RTable.UnderQuotes(), memberExpression2.GetPropertyName().UnderQuotes());

    }


    public static string UnderQuotes(this string text) => $"\"{text}\"";

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, int>> memberExpression)
        =>
        it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
        .AsInt32();

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, Guid>> memberExpression)
        =>
        it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
        .AsGuid();

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, long>> memberExpression)
         =>
         it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
         .AsInt64();

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, string>> memberExpression, int length)
        =>
        it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
        .AsString(length);

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, bool>> memberExpression)
        =>
        it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
        .AsBoolean();

    public static ICreateTableColumnOptionOrWithColumnSyntax Column<T>(this ICreateTableWithColumnSyntax it, Expression<Func<T, decimal>> memberExpression, int size, int precision)
       =>
       it.WithColumn(memberExpression.GetPropertyName().UnderQuotes())
       .AsDecimal(size, precision);

    private static string GetPropertyName<T1, T2>(this Expression<Func<T1, T2>> property)
    {
        var lambda = (LambdaExpression)property;

        MemberExpression memberExpression = 
            lambda.Body is UnaryExpression unaryExpression 
            ? (MemberExpression)unaryExpression.Operand 
            : (MemberExpression)lambda.Body;

        return ((PropertyInfo)memberExpression.Member).Name;
    }
}
