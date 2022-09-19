using Dawn;
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
        Guard.Argument(this.Vhost, nameof(this.Vhost)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.UserName, nameof(this.UserName)).NotNull().NotEmpty().NotWhiteSpace();

        Guard.Argument(this.ConfigurePattern, nameof(this.ConfigurePattern)).NotNull();
        Guard.Argument(this.WritePattern, nameof(this.WritePattern)).NotNull();
        Guard.Argument(this.ReadPattern, nameof(this.ReadPattern)).NotNull();

        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IRabbitMQAdminApi api)
     => api.SetVhostPermissionsAsync(this.Vhost, this.UserName, new VhostPermission()
     {
         Configure = this.ConfigurePattern,
         Write = this.WritePattern,
         Read = this.ReadPattern,
     });

}
