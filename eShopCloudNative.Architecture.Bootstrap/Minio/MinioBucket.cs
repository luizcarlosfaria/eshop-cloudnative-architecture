using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Minio;
public class MinioBucket
{
    public string BucketName { get; set; }

    public IPolicy Policy { get; set; }

}
