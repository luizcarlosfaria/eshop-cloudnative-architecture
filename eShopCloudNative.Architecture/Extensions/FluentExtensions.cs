using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class FluentExtensions
{
    public static T IfFunction<T>(this T target, Func<T, bool> condition, Func<T, T> actionWhenTrue, Func<T, T> actionWhenFalse = null)
    {
        Guard.Argument(condition, nameof(condition)).NotNull();
        Guard.Argument(actionWhenTrue, nameof(actionWhenTrue)).NotNull();

        if (target == null)
            return target;

        bool conditionResult = condition(target);

        if (conditionResult)
            target = actionWhenTrue(target);
        else if (actionWhenFalse != null)
            target = actionWhenFalse(target);

        return target;
    }

    public static T IfAction<T>(this T target, Func<T, bool> condition, Action<T> actionWhenTrue, Action<T> actionWhenFalse = null)
    {
        Guard.Argument(condition, nameof(condition)).NotNull();
        Guard.Argument(actionWhenTrue, nameof(actionWhenTrue)).NotNull();

        if (target == null)
            return target;

        bool conditionResult = condition(target);

        if (conditionResult)
            actionWhenTrue(target);
        else actionWhenFalse?.Invoke(target);

        return target;
    }


    public static T Fluent<T>(this T target, Action action)
        where T : class
    {
        Guard.Argument(target, nameof(target)).NotNull();
        Guard.Argument(action, nameof(action)).NotNull();

        action();

        return target;
    }

    public static T Fluent<T>(this T target, Func<T> func)
        where T : class
    {
        Guard.Argument(target, nameof(target)).NotNull();
        Guard.Argument(func, nameof(func)).NotNull();

        return func();

    }


}
