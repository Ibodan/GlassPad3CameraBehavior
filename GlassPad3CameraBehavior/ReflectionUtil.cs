using System;
using System.Reflection;
using UnityEngine;

internal static class ReflectionUtil
{
	public static void SetPrivateField(object obj, string fieldName, object value)
	{
		obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(obj, value);
	}

	public static T GetPrivateField<T>(object obj, string fieldName)
	{
		return (T)obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
	}

	public static void SetPrivateProperty(object obj, string propertyName, object value)
	{
		obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(obj, value, null);
	}

	public static T GetPrivateProperty<T>(object obj, string propertyName)
	{
		return (T)obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
	}

	public static void InvokePrivateMethod(object obj, string methodName, object[] methodParams)
	{
		obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, methodParams);
	}

	public static Component CopyComponent(Component original, Type overridingType, GameObject destination)
	{
		Component val = destination.AddComponent(overridingType);
		FieldInfo[] fields = ((object)original).GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(val, fieldInfo.GetValue(original));
		}
		return val;
	}
}
