#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Imperium.Util;

internal static class Reflection
{
    internal static void Invoke<T>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(instance, parameters);
    }

    internal static R Invoke<T, R>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        var value = typeof(T)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(instance, parameters);
        if (value is not R valueCasted) throw new ArgumentOutOfRangeException();

        return valueCasted;
    }

    internal static IEnumerator GetCoroutine<T>(
        T instance,
        string methodName
    )
    {
        return (IEnumerator)typeof(T)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(instance, []);
    }

    internal static R Get<T, R>(
        T instance,
        string fieldName,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        var value = typeof(T)
            .GetField(fieldName, bindingFlags)!
            .GetValue(instance);

        return (R)value;
    }

    internal static void Set<T, V>(
        T instance,
        string fieldName,
        V value,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        typeof(T).GetField(fieldName, bindingFlags)!.SetValue(instance, value);
    }

    internal static void SetProperty<T, V>(
        T instance,
        string propertyName,
        V value,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        typeof(T).InvokeMember(propertyName, bindingFlags | BindingFlags.SetProperty, null, instance, [value]);
    }

    internal static void CopyField<T>(
        T source,
        T target,
        string fieldName,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        var field = typeof(T).GetField(fieldName, bindingFlags)!;
        field.SetValue(target, field.GetValue(source));
    }

    private static readonly Dictionary<(Type, string, int), MethodInfo> optionalMethodCache = new();

    /// <summary>
    ///     Invokes a game method that gained new optional parameters in a later game version (e.g. v0.5.0).
    ///     A direct call compiled against older game libraries throws a MissingMethodException there, so the
    ///     method is looked up by name and any trailing optional parameters are filled with their defaults.
    /// </summary>
    internal static void InvokeWithOptionals<T>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        var key = (typeof(T), methodName, parameters.Length);
        if (!optionalMethodCache.TryGetValue(key, out var method))
        {
            method = typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == methodName)
                .Select(m => (method: m, args: m.GetParameters()))
                .Where(m => m.args.Length >= parameters.Length && m.args.Skip(parameters.Length).All(p => p.IsOptional))
                .OrderBy(m => m.args.Length)
                .Select(m => m.method)
                .First();
            optionalMethodCache[key] = method;
        }

        var methodParameters = method.GetParameters();
        var arguments = new object[methodParameters.Length];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = i < parameters.Length ? parameters[i] : methodParameters[i].DefaultValue;
        }

        method.Invoke(instance, arguments);
    }
}