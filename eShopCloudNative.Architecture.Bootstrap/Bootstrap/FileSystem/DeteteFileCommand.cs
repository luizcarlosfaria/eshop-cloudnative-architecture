using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.FileSystem;
public class DeteteFileCommand : IFileSystemCommand
{
    public string FileName { get; set; }

    public Task ExecuteAsync()
    {
        Guard.Argument(this.FileName, nameof(this.FileName)).NotNull().NotEmpty().NotWhiteSpace();

        File.Delete(this.FileName);

        return Task.CompletedTask;
    }
}
