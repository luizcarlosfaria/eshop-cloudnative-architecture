using System.Runtime.CompilerServices;

namespace eShopCloudNative.Architecture.Tests.SourceGeneratorTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}