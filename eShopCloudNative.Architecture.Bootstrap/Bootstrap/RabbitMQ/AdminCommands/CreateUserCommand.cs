using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
public class CreateUserCommand : IAdminCommand
{
    public System.Net.NetworkCredential Credential { get; set; }
    public string Tags { get; set; }

    public Task PrepareAsync()
    {
        Guard.Against.Null(this.Credential, nameof(this.Credential));
        Guard.Against.NullOrWhiteSpace(this.Tags, nameof(this.Tags));
        
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IRabbitMQAdminApi api)
     => api.CreateUserAsync(this.Credential.UserName, new CreateUserRequest() { Password = this.Credential.Password, Tags = this.Tags });

}
