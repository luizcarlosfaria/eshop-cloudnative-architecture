using Dawn;
using System.Text;

namespace eShopCloudNative.Architecture.Bootstrap.FileSystem;

public class CreateFileCommand : IFileSystemCommand
{
    public string FileName { get; set; }
    public string Content { get; set; }

    public Task ExecuteAsync()
    {
        Guard.Argument(this.FileName, nameof(this.FileName)).NotNull().NotEmpty().NotWhiteSpace();

        File.WriteAllText(this.FileName, this.Content, Encoding.UTF8);

        return Task.CompletedTask;
    }
}
