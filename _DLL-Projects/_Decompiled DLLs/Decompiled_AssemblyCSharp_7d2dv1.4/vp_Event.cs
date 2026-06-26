using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class vp_Event
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string m_Name;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type m_ArgumentType;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type m_ReturnType;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public FieldInfo[] m_Fields;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type[] m_DelegateTypes;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MethodInfo[] m_DefaultMethods;

	[Preserve]
	public string[] InvokerFieldNames;

	[Preserve]
	public Dictionary<string, int> Prefixes;

	public string EventName => m_Name;

	[Preserve]
	public Type ArgumentType => m_ArgumentType;

	[Preserve]
	public Type ReturnType => m_ReturnType;

	[Preserve]
	public Type GetArgumentType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!GetType().IsGenericType)
			{
				return typeof(void);
			}
			return GetType().GetGenericArguments()[0];
		}
	}

	[Preserve]
	public Type GetGenericReturnType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!GetType().IsGenericType)
			{
				return typeof(void);
			}
			if (GetType().GetGenericArguments().Length != 2)
			{
				return typeof(void);
			}
			return GetType().GetGenericArguments()[1];
		}
	}

	[Preserve]
	public abstract void Register(object target, string method, int variant);

	[Preserve]
	public abstract void Unregister(object target);

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void InitFields();

	[Preserve]
	public vp_Event(string name = "")
	{
		m_ArgumentType = GetArgumentType;
		m_ReturnType = GetGenericReturnType;
		m_Name = name;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void StoreInvokerFieldNames()
	{
		InvokerFieldNames = new string[m_Fields.Length];
		for (int i = 0; i < m_Fields.Length; i++)
		{
			InvokerFieldNames[i] = m_Fields[i].Name;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Type MakeGenericType(Type type)
	{
		if (m_ReturnType == typeof(void))
		{
			return type.MakeGenericType(m_ArgumentType, m_ArgumentType);
		}
		return type.MakeGenericType(m_ArgumentType, m_ReturnType, m_ArgumentType, m_ReturnType);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetFieldToExternalMethod(object target, FieldInfo field, string method, Type type)
	{
		Delegate obj = Delegate.CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: false);
		if ((object)obj == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Failed to bind: " + target?.ToString() + " -> " + method + ".");
		}
		else
		{
			field.SetValue(this, obj);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddExternalMethodToField(object target, FieldInfo field, string method, Type type)
	{
		Delegate obj = Delegate.Combine((Delegate)field.GetValue(this), Delegate.CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: false));
		if ((object)obj == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Failed to bind: " + target?.ToString() + " -> " + method + ".");
		}
		else
		{
			field.SetValue(this, obj);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetFieldToLocalMethod(FieldInfo field, MethodInfo method, Type type)
	{
		if (!(method == null))
		{
			Delegate obj = Delegate.CreateDelegate(type, method);
			if ((object)obj == null)
			{
				Debug.LogError("Error (" + this?.ToString() + ") Failed to bind: " + method?.ToString() + ".");
			}
			else
			{
				field.SetValue(this, obj);
			}
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveExternalMethodFromField(object target, FieldInfo field)
	{
		List<Delegate> list = new List<Delegate>(((Delegate)field.GetValue(this)).GetInvocationList());
		if (list == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Failed to remove: " + target?.ToString() + " -> " + field.Name + ".");
			return;
		}
		for (int num = list.Count - 1; num > -1; num--)
		{
			if (list[num].Target == target)
			{
				list.Remove(list[num]);
			}
		}
		if (list != null)
		{
			field.SetValue(this, Delegate.Combine(list.ToArray()));
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MethodInfo GetStaticGenericMethod(Type e, string name, Type parameterType, Type returnType)
	{
		MethodInfo[] methods = e.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (MethodInfo methodInfo in methods)
		{
			if (!(methodInfo == null) && !(methodInfo.Name != name))
			{
				MethodInfo methodInfo2 = ((!(GetGenericReturnType == typeof(void))) ? methodInfo.MakeGenericMethod(m_ArgumentType, m_ReturnType) : methodInfo.MakeGenericMethod(m_ArgumentType));
				if (methodInfo2.GetParameters().Length <= 1 && (methodInfo2.GetParameters().Length != 1 || !(parameterType == typeof(void))) && (methodInfo2.GetParameters().Length != 0 || !(parameterType != typeof(void))) && (methodInfo2.GetParameters().Length != 1 || !(methodInfo2.GetParameters()[0].ParameterType != parameterType)) && !(returnType != methodInfo2.ReturnType))
				{
					return methodInfo2;
				}
			}
		}
		return null;
	}

	[Preserve]
	public Type GetParameterType(int index)
	{
		if (!GetType().IsGenericType)
		{
			return typeof(void);
		}
		if (index > m_Fields.Length - 1)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Event '" + EventName + "' only supports " + m_Fields.Length + " indices. 'GetParameterType' referenced index " + index + ".");
		}
		if (m_DelegateTypes[index].GetMethod("Invoke").GetParameters().Length == 0)
		{
			return typeof(void);
		}
		return m_ArgumentType;
	}

	[Preserve]
	public Type GetReturnType(int index)
	{
		if (index > m_Fields.Length - 1)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Event '" + EventName + "' only supports " + m_Fields.Length + " indices. 'GetReturnType' referenced index " + index + ".");
			return null;
		}
		if (GetType().GetGenericArguments().Length > 1)
		{
			return GetGenericReturnType;
		}
		Type returnType = m_DelegateTypes[index].GetMethod("Invoke").ReturnType;
		if (returnType.IsGenericParameter)
		{
			return m_ArgumentType;
		}
		return returnType;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Refresh()
	{
	}
}
