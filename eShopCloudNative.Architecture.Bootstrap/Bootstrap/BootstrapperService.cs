using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap;
public class BootstrapperService : IBootstrapperService
{

    public List<IBootstrapperService> Services { get; set; }

    public async Task InitializeAsync(IConfiguration configuration)
    {
        if (this.Services == null) throw new InvalidOperationException("BootstrapperService.Services can't be null");

        foreach (var service in this.Services)
        {
            if (service == null) throw new InvalidOperationException("A item of BootstrapperService.Services can't be null");

            await service.InitializeAsync(configuration);
        }
    }

    public async Task ExecuteAsync(IConfiguration configuration)
    {
        if (this.Services == null) throw new InvalidOperationException("BootstrapperService.Services can't be null");

        foreach (var service in this.Services)
        {
            if (service == null) throw new InvalidOperationException("A item of BootstrapperService.Services can't be null");

            await service.ExecuteAsync(configuration);
        }
    }

}
