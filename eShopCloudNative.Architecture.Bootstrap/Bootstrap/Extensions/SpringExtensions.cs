using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.Extensions;
public static class SpringExtensions
{
    public class ObjectContainer { public static object Define(object defined) => defined; }

    public static CodeConfigApplicationContext RegisterInstance(this CodeConfigApplicationContext context, string name, object instance)
    {
        Spring.Objects.Factory.Support.GenericObjectDefinition objectDefinition = new Spring.Objects.Factory.Support.GenericObjectDefinition();
        objectDefinition.ObjectType = typeof(ObjectContainer);
        objectDefinition.ConstructorArgumentValues.AddNamedArgumentValue("defined", instance);
        objectDefinition.FactoryMethodName = "Define";
        objectDefinition.IsSingleton = true;
        context.RegisterObjectDefinition(name, objectDefinition);
        return context;
    }


    public static XmlApplicationContext CreateChildContext(this CodeConfigApplicationContext context, params string[] configurationLocations)
    {
        XmlApplicationContext childContext = new XmlApplicationContext(context, configurationLocations);
        return childContext;
    }
}
