using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Discord.Commands;

internal static class ReflectionUtils
{
	private static readonly TypeInfo ObjectTypeInfo = typeof(object).GetTypeInfo();

	internal static T CreateObject<T>(TypeInfo typeInfo, CommandService commands, IServiceProvider services = null)
	{
		return CreateBuilder<T>(typeInfo, commands)(services);
	}

	internal static Func<IServiceProvider, T> CreateBuilder<T>(TypeInfo typeInfo, CommandService commands)
	{
		ConstructorInfo constructor = GetConstructor(typeInfo);
		System.Reflection.ParameterInfo[] parameters = constructor.GetParameters();
		PropertyInfo[] properties = GetProperties(typeInfo);
		return delegate(IServiceProvider services)
		{
			object[] array = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = GetMember(commands, services, parameters[i].ParameterType, typeInfo);
			}
			T val = InvokeConstructor<T>(constructor, array, typeInfo);
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				propertyInfo.SetValue(val, GetMember(commands, services, propertyInfo.PropertyType, typeInfo));
			}
			return val;
		};
	}

	private static T InvokeConstructor<T>(ConstructorInfo constructor, object[] args, TypeInfo ownerType)
	{
		try
		{
			return (T)constructor.Invoke(args);
		}
		catch (Exception innerException)
		{
			throw new Exception("Failed to create \"" + ownerType.FullName + "\".", innerException);
		}
	}

	private static ConstructorInfo GetConstructor(TypeInfo ownerType)
	{
		ConstructorInfo[] array = ownerType.DeclaredConstructors.Where((ConstructorInfo x) => !x.IsStatic).ToArray();
		if (array.Length == 0)
		{
			throw new InvalidOperationException("No constructor found for \"" + ownerType.FullName + "\".");
		}
		if (array.Length > 1)
		{
			throw new InvalidOperationException("Multiple constructors found for \"" + ownerType.FullName + "\".");
		}
		return array[0];
	}

	private static PropertyInfo[] GetProperties(TypeInfo ownerType)
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		while (ownerType != ObjectTypeInfo)
		{
			foreach (PropertyInfo declaredProperty in ownerType.DeclaredProperties)
			{
				MethodInfo setMethod = declaredProperty.SetMethod;
				if ((object)setMethod != null && !setMethod.IsStatic)
				{
					MethodInfo setMethod2 = declaredProperty.SetMethod;
					if ((object)setMethod2 != null && setMethod2.IsPublic && declaredProperty.GetCustomAttribute<DontInjectAttribute>() == null)
					{
						list.Add(declaredProperty);
					}
				}
			}
			ownerType = ownerType.BaseType.GetTypeInfo();
		}
		return list.ToArray();
	}

	private static object GetMember(CommandService commands, IServiceProvider services, Type memberType, TypeInfo ownerType)
	{
		if (memberType == typeof(CommandService))
		{
			return commands;
		}
		if (memberType == typeof(IServiceProvider) || memberType == services.GetType())
		{
			return services;
		}
		object service = services.GetService(memberType);
		if (service != null)
		{
			return service;
		}
		throw new InvalidOperationException("Failed to create \"" + ownerType.FullName + "\", dependency \"" + memberType.Name + "\" was not found.");
	}
}
