using eShopCloudNative.Architecture.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests;
public class EnterpriseApplicationLogTests
{

    [Fact]
    public void EnterpriseApplicationLogTest() {

        bool pass = false;

        try
        {
            EnterpriseApplicationLog.SetGlobalContext("a");

            EnterpriseApplicationLog.SetGlobalContext("a", it => it.Add(new Tag("a", TagType.None, "b")));

            pass = true;
        }
        catch (Exception)
        {
            pass = false;
            throw;
        }
        
        Assert.True(pass);
    }
}
