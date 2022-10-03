
using Dawn;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.FileSystem;
internal class FileSystemBootstrapperService : IBootstrapperService
{
    public IList<IFileSystemCommand> Commands { get; set; }

    public async Task ExecuteAsync()
    {
        foreach (var command in this.Commands)
        {
            await command.ExecuteAsync();
        }
    }

    public Task InitializeAsync()
    {
        Guard.Argument(this.Commands, nameof(this.Commands)).NotNull().NotEmpty();

        return Task.CompletedTask;
    }
}
