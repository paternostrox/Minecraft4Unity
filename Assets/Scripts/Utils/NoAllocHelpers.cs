using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

 /// <summary>
 /// Provides access to the internal UnityEngine.NoAllocHelpers methods.
 /// </summary>
 public static class NoAllocHelpers
 {
     private static readonly Dictionary<Type, Delegate> ExtractArrayFromListTDelegates = new Dictionary<Type, Delegate>();
     private static readonly Dictionary<Type, Delegate> ResizeListDelegates = new Dictionary<Type, Delegate>();

     /// <summary>
     /// Extract the internal array from a list.
     /// </summary>
     /// <typeparam name="T"><see cref="List{T}"/>.</typeparam>
     /// <param name="list">The <see cref="List{T}"/> to extract from.</param>
     /// <returns>The internal array of the list.</returns>
     public static T[] ExtractArrayFromListT<T>(List<T> list)
     {
         if (!ExtractArrayFromListTDelegates.TryGetValue(typeof(T), out var obj))
         {
             var ass = Assembly.GetAssembly(typeof(Mesh)); // any class in UnityEngine
             var type = ass.GetType("UnityEngine.NoAllocHelpers");
             var methodInfo = type.GetMethod("ExtractArrayFromListT", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeof(T));

             obj = ExtractArrayFromListTDelegates[typeof(T)] = Delegate.CreateDelegate(typeof(Func<List<T>, T[]>), methodInfo);
         }

         var func = (Func<List<T>, T[]>) obj;
         return func.Invoke(list);
     }

     /// <summary>
     /// Resize a list.
     /// </summary>
     /// <typeparam name="T"><see cref="List{T}"/>.</typeparam>
     /// <param name="list">The <see cref="List{T}"/> to resize.</param>
     /// <param name="size">The new length of the <see cref="List{T}"/>.</param>
     public static void ResizeList<T>(List<T> list, int size)
     {
         if (!ResizeListDelegates.TryGetValue(typeof(T), out var obj))
         {
             var ass = Assembly.GetAssembly(typeof(Mesh)); // any class in UnityEngine
             var type = ass.GetType("UnityEngine.NoAllocHelpers");
             var methodInfo = type.GetMethod("ResizeList", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeof(T));
             obj = ResizeListDelegates[typeof(T)] = Delegate.CreateDelegate(typeof(Action<List<T>, int>), methodInfo);
         }

         var action = (Action<List<T>, int>) obj;
         action.Invoke(list, size);
     }
 }