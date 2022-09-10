using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap;

public interface IBootstrapperService
{
    Task InitializeAsync();

    Task ExecuteAsync();

}
