using eShopCloudNative.Architecture.Bootstrap;

namespace eShopCloudNative.Architecture.Tests;

public class BootstrapperServiceTests
{
    [Fact]
    public void ConstructorTests()
    {
        Assert.NotNull(new BootstrapperService());
    }


    [Fact]
    public async Task NullServiceTestsAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var bootstrapperService = new BootstrapperService();
            await bootstrapperService.InitializeAsync();
        });

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var bootstrapperService = new BootstrapperService();
            await bootstrapperService.ExecuteAsync();
        });

    }

    [Fact]
    public async Task NullServiceItemTestsAsync()
    {
        IBootstrapperService nullBootstrapperService = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var bootstrapperService = new BootstrapperService()
            {
                Services = new List<IBootstrapperService>()
                {
                    nullBootstrapperService,
                }
            };
            await bootstrapperService.InitializeAsync();
        });

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var bootstrapperService = new BootstrapperService()
            {
                Services = new List<IBootstrapperService>()
                {
                    nullBootstrapperService,
                }
            };
            await bootstrapperService.ExecuteAsync();
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
        await bootstrapperService.InitializeAsync();
            

        mock.Verify(m => m.InitializeAsync(), Times.Once());

        mock = new Mock<IBootstrapperService>();


        bootstrapperService = new BootstrapperService()
        {
            Services = new List<IBootstrapperService>()
                {
                    mock.Object,
                }
        };
        await bootstrapperService.ExecuteAsync();


        mock.Verify(m => m.ExecuteAsync(), Times.Once());
    }
}