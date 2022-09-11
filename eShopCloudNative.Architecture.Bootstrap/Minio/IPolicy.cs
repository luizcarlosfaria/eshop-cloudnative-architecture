using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Minio;
public interface IPolicy
{
    string GetJsonPolicy();
}
