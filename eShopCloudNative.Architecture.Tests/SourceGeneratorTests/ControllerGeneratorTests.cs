using eShopCloudNative.Architecture.SourceGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace eShopCloudNative.Architecture.Tests.SourceGeneratorTests;

[UsesVerify] // 👈 Adds hooks for Verify into XUnit
public class ControllerGeneratorTests
{
    [Fact(Skip="Source Generator Tests")]
    //[Fact]
    public Task GenerateWebApiCorrectly()
    {
        // The source code to test
        var source = @"
using eShopCloudNative.Catalog.Dto;
using eShopCloudNative.Catalog.Entities;
using eShopCloudNative.Catalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopCloudNative.Catalog.Controllers;

public class DTOout{}
public class DTOin{}

public interface IAppService
{
    [Get(""/Public/Catalog/HomeCatalog"")]
    Task<IEnumerable<DTOout>> GetHomeCatalogAsync(DTOin data);
}

[ApiController]
[Route(""Public/Catalog"")]
public partial class AbcdeController : ControllerBase, IAppService, ITestService
{
    
}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify<ControllerGenerator>(source);
    }
}