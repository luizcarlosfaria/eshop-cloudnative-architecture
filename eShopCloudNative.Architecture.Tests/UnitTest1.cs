using eShopCloudNative.Architecture.Bootstrap;

namespace eShopCloudNative.Architecture.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1Async()
    {
        new BootstrapperService();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService();
            await bootstrapperService.InitializeAsync(null);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var bootstrapperService = new BootstrapperService()
            {
                Services = new List<IBootstrapperService>()
                {
                    null
                }
            };
            await bootstrapperService.InitializeAsync(null);
        });
    }
}