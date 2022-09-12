using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
public class SetUserPermissionCommand : IAdminCommand
{
    public string Vhost { get; set; }
    public string UserName { get; set; }
    public string ConfigurePattern { get; set; }
    public string WritePattern { get; set; }
    public string ReadPattern { get; set; }

    public Task PrepareAsync()
    {
        Guard.Against.NullOrWhiteSpace(this.Vhost, nameof(this.Vhost));
        Guard.Against.NullOrWhiteSpace(this.UserName, nameof(this.UserName));

        Guard.Against.Null(this.ConfigurePattern, nameof(this.ConfigurePattern));
        Guard.Against.Null(this.WritePattern, nameof(this.WritePattern));
        Guard.Against.Null(this.ReadPattern, nameof(this.ReadPattern));

        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IRabbitMQAdminAPI api)
     => api.SetUserVirtualHostPermissionsAsync(this.Vhost, this.UserName, new VhostPermission()
     {
         Configure = this.ConfigurePattern,
         Write = this.WritePattern,
         Read = this.ReadPattern,
     });

}
