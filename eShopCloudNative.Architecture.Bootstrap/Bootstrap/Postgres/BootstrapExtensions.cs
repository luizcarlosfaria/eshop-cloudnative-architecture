using System.Data;

namespace eShopCloudNative.Architecture.Bootstrap.Postgres;

public static class BootstrapExtensions
{
    public static void CreateAndAddParameter(this IDbCommand command,DbType type,  string parameterName, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = "username";
        parameter.DbType = type;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}