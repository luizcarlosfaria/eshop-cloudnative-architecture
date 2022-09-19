using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
public class CreateVhostCommand : IAdminCommand
{
    public string Name { get; set; }

    public Task PrepareAsync()
    {
        Guard.Argument(this.Name, nameof(this.Name)).NotNull().NotEmpty().NotWhiteSpace();
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IRabbitMQAdminApi api)
     => api.CreateVirtualHostAsync(this.Name);

}
