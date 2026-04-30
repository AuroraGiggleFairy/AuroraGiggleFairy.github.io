using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal static class TypeExtensions
{
	public static MethodInfo Method(this Delegate d)
	{
		return d.Method;
	}

	public static MemberTypes MemberType(this MemberInfo memberInfo)
	{
		return memberInfo.MemberType;
	}

	public static bool ContainsGenericParameters(this Type type)
	{
		return type.ContainsGenericParameters;
	}

	public static bool IsInterface(this Type type)
	{
		return type.IsInterface;
	}

	public static bool IsGenericType(this Type type)
	{
		return type.IsGenericType;
	}

	public static bool IsGenericTypeDefinition(this Type type)
	{
		return type.IsGenericTypeDefinition;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static Type BaseType(this Type type)
	{
		return type.BaseType;
	}

	public static Assembly Assembly(this Type type)
	{
		return type.Assembly;
	}

	public static bool IsEnum(this Type type)
	{
		return type.IsEnum;
	}

	public static bool IsClass(this Type type)
	{
		return type.IsClass;
	}

	public static bool IsSealed(this Type type)
	{
		return type.IsSealed;
	}

	public static bool IsAbstract(this Type type)
	{
		return type.IsAbstract;
	}

	public static bool IsVisible(this Type type)
	{
		return type.IsVisible;
	}

	public static bool IsValueType(this Type type)
	{
		return type.IsValueType;
	}

	public static bool IsPrimitive(this Type type)
	{
		return type.IsPrimitive;
	}

	public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)][_003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhen(true)] out Type match)
	{
		Type type2 = type;
		while (type2 != null)
		{
			if (string.Equals(type2.FullName, fullTypeName, StringComparison.Ordinal))
			{
				match = type2;
				return true;
			}
			type2 = type2.BaseType();
		}
		if (searchInterfaces)
		{
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (string.Equals(interfaces[i].Name, fullTypeName, StringComparison.Ordinal))
				{
					match = type;
					return true;
				}
			}
		}
		match = null;
		return false;
	}

	public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces)
	{
		Type match;
		return type.AssignableToTypeName(fullTypeName, searchInterfaces, out match);
	}

	public static bool ImplementInterface(this Type type, Type interfaceType)
	{
		Type type2 = type;
		while (type2 != null)
		{
			foreach (Type item in (IEnumerable<Type>)type2.GetInterfaces())
			{
				if (item == interfaceType || (item != null && item.ImplementInterface(interfaceType)))
				{
					return true;
				}
			}
			type2 = type2.BaseType();
		}
		return false;
	}
}
