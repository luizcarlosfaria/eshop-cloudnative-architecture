using eShopCloudNative.Architecture.Bootstrap;

namespace eShopCloudNative.Architecture.Tests;

public class BootstrapperServiceTests
{
    [Fact]
    public void ConstructorTests()
    {
        new BootstrapperService();
    }


    [Fact]
    public async Task NullServiceTestsAsync()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService();
            await bootstrapperService.InitializeAsync(null);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService();
            await bootstrapperService.ExecuteAsync(null);
        });

    }

    [Fact]
    public async Task NullServiceItemTestsAsync()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService()
            {
                Services = new List<IBootstrapperService>()
                {
                    null,
                }
            };
            await bootstrapperService.InitializeAsync(null);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService()
            {
                Services = new List<IBootstrapperService>()
                {
                    null,
                }
            };
            await bootstrapperService.ExecuteAsync(null);
        });

    }

    [Fact]
    public async Task PropagationTestsAsync()
    {
        var mock = new Mock<IBootstrapperService>();


        var bootstrapperService = new BootstrapperService()
        {
            Services = new List<IBootstrapperService>()
                {
                    mock.Object,
                }
        };
        await bootstrapperService.InitializeAsync(null);


        mock.Verify(m => m.InitializeAsync(null), Times.Once());

        mock = new Mock<IBootstrapperService>();


        bootstrapperService = new BootstrapperService()
        {
            Services = new List<IBootstrapperService>()
                {
                    mock.Object,
                }
        };
        await bootstrapperService.ExecuteAsync(null);


        mock.Verify(m => m.ExecuteAsync(null), Times.Once());
    }
}