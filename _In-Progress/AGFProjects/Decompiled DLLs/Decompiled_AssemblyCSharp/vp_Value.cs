using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_Value<V> : vp_Event
{
	[Preserve]
	public delegate T Getter<T>();

	[Preserve]
	public delegate void Setter<T>(T o);

	[Preserve]
	public Getter<V> Get;

	[Preserve]
	public Setter<V> Set;

	[Preserve]
	public FieldInfo[] Fields
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_Fields;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static T Empty<T>()
	{
		return default(T);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty<T>(T value)
	{
	}

	[Preserve]
	public vp_Value(string name)
		: base(name)
	{
		InitFields();
	}

	public void DoNotCallAOTCompileFix()
	{
		Empty<V>();
		Empty(default(V));
		throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		m_Fields = new FieldInfo[2]
		{
			GetType().GetField("Get"),
			GetType().GetField("Set")
		};
		StoreInvokerFieldNames();
		m_DelegateTypes = new Type[2]
		{
			typeof(Getter<>),
			typeof(Setter<>)
		};
		m_DefaultMethods = new MethodInfo[2]
		{
			GetStaticGenericMethod(GetType(), "Empty", typeof(void), m_ArgumentType),
			GetStaticGenericMethod(GetType(), "Empty", m_ArgumentType, typeof(void))
		};
		Prefixes = new Dictionary<string, int>
		{
			{ "get_OnValue_", 0 },
			{ "set_OnValue_", 1 }
		};
		if (m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
		if (m_DefaultMethods[1] != null)
		{
			SetFieldToLocalMethod(m_Fields[1], m_DefaultMethods[1], MakeGenericType(m_DelegateTypes[1]));
		}
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		if (m != null)
		{
			SetFieldToExternalMethod(t, m_Fields[v], m, MakeGenericType(m_DelegateTypes[v]));
			Refresh();
		}
	}

	[Preserve]
	public override void Unregister(object t)
	{
		if (m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
		if (m_DefaultMethods[1] != null)
		{
			SetFieldToLocalMethod(m_Fields[1], m_DefaultMethods[1], MakeGenericType(m_DelegateTypes[1]));
		}
		Refresh();
	}
}
