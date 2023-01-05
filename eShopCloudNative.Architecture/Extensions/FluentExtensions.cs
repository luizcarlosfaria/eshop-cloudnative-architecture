using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class FluentExtensions
{
    public static T If<T>(this T target, Func<T, bool> condition, Func<T, T> actionWhenTrue, Func<T, T> actionWhenFalse = null)
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

    public static T2 IfTransform<T1, T2>(this T1 target, Func<T1, bool> condition, Func<T1, T2> actionWhenTrue, Func<T1, T2> actionWhenFalse = null)
    {
        Guard.Argument(condition).NotNull();
        Guard.Argument(actionWhenTrue).NotNull();
        Guard.Argument(actionWhenFalse).NotNull();

        bool conditionResult = condition(target);

        if (conditionResult)
            return actionWhenTrue(target);
        else
            return actionWhenFalse(target);
    }

    public static T If<T>(this T target, Func<T, bool> condition, Action<T> actionWhenTrue, Action<T> actionWhenFalse = null)
    {
        Guard.Argument(condition, nameof(condition)).NotNull();
        Guard.Argument(actionWhenTrue, nameof(actionWhenTrue)).NotNull();

        if (target == null)
            return target;

        bool conditionResult = condition(target);

        if (conditionResult)
            actionWhenTrue(target);
        else if (actionWhenFalse != null)
            actionWhenFalse(target);

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
}
