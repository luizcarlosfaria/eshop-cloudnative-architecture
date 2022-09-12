using Ardalis.GuardClauses;
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
        Guard.Against.NullOrWhiteSpace(this.Name, nameof(this.Name));
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IRabbitMQAdminAPI api)
     => api.CreateVirtualHostAsync(this.Name);

}
