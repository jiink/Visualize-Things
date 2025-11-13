using System;
using System.Collections.Generic;
using UnityEngine;

public static class Services
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogError($"Service of type {type.Name} is already registered.");
            return;
        }
        _services[type] = service;
    }

    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (!_services.TryGetValue(type, out var service))
        {
            Debug.LogError($"Service of type {type.Name} is not registered.");
            return null;
        }
        return service as T;
    }
}
