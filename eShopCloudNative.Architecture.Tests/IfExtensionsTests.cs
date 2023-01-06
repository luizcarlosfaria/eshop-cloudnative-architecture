using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Extensions;
using eShopCloudNative.Architecture.Minio;
using Newtonsoft.Json.Linq;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static eShopCloudNative.Architecture.Extensions.SpringExtensions;

namespace eShopCloudNative.Architecture.Tests;
public class Extensions_IF_Tests
{



    [Fact]
    public void FluentIfTrueTests()
    {
        //Func<> 
        Assert.True(true.IfFunction(it => true, it => true, it => false));
        Assert.False(true.IfFunction(it => false, it => true, it => false));

        //Actions
        {
            bool? value1 = null;

            true.IfAction(it => true, it => { value1 = true; }, it => { value1 = false; }).Should().BeTrue();

            ((object)null).IfAction(it => true, it => { value1 = true; }, it => { value1 = false; }).Should().BeNull();

            Assert.True(value1);
        }
        //Actions
        {
            bool? value2 = null;

            true.IfAction(it => false, it => { value2 = true; }, it => { value2 = false; });

            Assert.False(value2);
        }
    }

    [Fact]
    public void FluentIfFalseTests()
    {
        bool?  value1 = null;
        new List<string>().IfAction(it => false, it => value1 = true, it => value1 = false);
        value1.Should().Be(false);

        bool? value2 = null;
        new List<string>().IfAction(it => false, it => value2 = true, it => value2 = false);
        value2.Should().Be(false);
    }

    [Fact]
    public void FluentExceptionsTests()
    {
        object objectNull = null;
        object objectNotNull = new object();

        Assert.Throws<ArgumentNullException>(() => objectNull.IfAction(null, null, null));

        Assert.Throws<ArgumentNullException>(() => objectNotNull.IfAction(null, null, null));

        Assert.Throws<ArgumentNullException>(() => objectNotNull.IfAction((it) => true, null, null));

    }

    [Fact]
    public void FluentNullTargetTests()
    {
        bool? value = null;
        IFormattable objectNull = null;

        objectNull.IfAction(it => false, it => value = true, it => value = false).Should().BeNull();

        value.Should().BeNull();

        ((object)null).IfFunction(it => false, it => "A", it => "B").Should().BeNull();
    }

    [Fact]
    public void FluentFunctionsTests()
    {
        object object_ = new();
        object objectA = new();
        object objectB = new();

        object_.IfFunction(it => false, it => objectA, it => objectB).Should().Be(objectB);
        object_.IfFunction(it => true, it => objectA, it => objectB).Should().Be(objectA);
    }

}
