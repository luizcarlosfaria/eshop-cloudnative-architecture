using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Minio;
public class StaticPolicy : IPolicy
{
    public string PolicyText { get; set; }

    public string GetJsonPolicy() => this.PolicyText;
}
