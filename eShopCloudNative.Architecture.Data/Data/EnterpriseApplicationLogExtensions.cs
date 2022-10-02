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
    public static List<Tag> AddDataOperation(this List<Tag> tags, DataOperation operation)
    {
        Guard.Argument(tags, nameof(tags)).NotNull();
        tags.Add(new Tag("Operation", TagType.None, operation.ToString()));
        return tags;
    }
}