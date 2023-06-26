using Dawn;
using eShopCloudNative.Architecture.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Data;

public enum DataOperation
{ 
    None,
    Insert,
    Update, 
    Upsert,
    Delete,
    Query
}


[ExcludeFromCodeCoverage]
public static class EnterpriseApplicationLogExtensions
{
    public static EnterpriseApplicationLogContext AddDataOperation(this EnterpriseApplicationLogContext context, DataOperation operation)
    {
        Guard.Argument(context, nameof(context)).NotNull();

        context.Add("Operation", TagType.None, operation.ToString());
        
        return context;
    }
}