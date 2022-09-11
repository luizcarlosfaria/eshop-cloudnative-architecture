using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;

public static class ObjectContainer { public static object Define(object instance) => instance; }

public static class SpringExtensions
{
    public static CodeConfigApplicationContext RegisterInstance(this CodeConfigApplicationContext context, string name, object instance)
    {
        var objectDefinition = new Spring.Objects.Factory.Support.GenericObjectDefinition
        {
            ObjectType = typeof(ObjectContainer),
            FactoryMethodName = nameof(ObjectContainer.Define),
            IsSingleton = true
        };
        objectDefinition.ConstructorArgumentValues.AddNamedArgumentValue("instance", instance);
        context.RegisterObjectDefinition(name, objectDefinition);
        return context;
    }


    public static XmlApplicationContext CreateChildContext(this CodeConfigApplicationContext context, params string[] configurationLocations)
        => new(context, configurationLocations);
}
