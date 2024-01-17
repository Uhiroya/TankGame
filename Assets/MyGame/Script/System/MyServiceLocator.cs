using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyServiceLocator 
{
    private static Dictionary<Type, List<object>> _IContainer;
    private static Dictionary<Type, List<object>> _container;
    static MyServiceLocator()
    {
        _IContainer = new();
        _container = new();
    }

    internal static List<object> IResolve<T>() 
    {
        if (_IContainer.ContainsKey(typeof(T)))
        {
            return _IContainer[typeof(T)];
        }
        else
        {
            return null;
        }
    }
    internal static void IRegister<T>(T instance) 
    {
        if (_IContainer.ContainsKey(typeof(T)))
        {
            _IContainer[typeof(T)].Add(instance);
        }
        else
        {
            _IContainer.Add(typeof(T), new List<object>(){instance});
        }
            
    }
    internal static void IUnRegister<T>(T instance) 
    {
        _IContainer[typeof(T)].Remove(instance);
    }
}
