using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_Message : vp_Event
{
	[Preserve]
	public delegate void Sender();

	[Preserve]
	public Sender Send;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty()
	{
	}

	[Preserve]
	public vp_Message(string name)
		: base(name)
	{
		InitFields();
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetType().GetMethod("Empty") };
		m_DelegateTypes = new Type[1] { typeof(Sender) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		Send = Empty;
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		Send = (Sender)Delegate.Combine(Send, (Sender)Delegate.CreateDelegate(m_DelegateTypes[v], t, m));
		Refresh();
	}

	[Preserve]
	public override void Unregister(object t)
	{
		RemoveExternalMethodFromField(t, m_Fields[0]);
		Refresh();
	}
}
[Preserve]
public class vp_Message<V> : vp_Message
{
	[Preserve]
	public new delegate void Sender<T>(T value);

	[Preserve]
	public new Sender<V> Send;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty<T>(T value)
	{
	}

	[Preserve]
	public vp_Message(string name)
		: base(name)
	{
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetStaticGenericMethod(GetType(), "Empty", m_ArgumentType, typeof(void)) };
		m_DelegateTypes = new Type[1] { typeof(Sender<>) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		Send = Empty;
		if (m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		if (m != null)
		{
			AddExternalMethodToField(t, m_Fields[v], m, MakeGenericType(m_DelegateTypes[v]));
			Refresh();
		}
	}

	[Preserve]
	public override void Unregister(object t)
	{
		RemoveExternalMethodFromField(t, m_Fields[0]);
		Refresh();
	}
}
public class vp_Message<V, VResult> : vp_Message
{
	[Preserve]
	public new delegate TResult Sender<T, TResult>(T value);

	[Preserve]
	public new Sender<V, VResult> Send;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static TResult Empty<T, TResult>(T value)
	{
		return default(TResult);
	}

	[Preserve]
	public vp_Message(string name)
		: base(name)
	{
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetStaticGenericMethod(GetType(), "Empty", m_ArgumentType, m_ReturnType) };
		m_DelegateTypes = new Type[1] { typeof(Sender<, >) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		if (m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
	}

	[Preserve]
	public override void Register(object t, string m, int v)
	{
		if (m != null)
		{
			AddExternalMethodToField(t, m_Fields[0], m, MakeGenericType(m_DelegateTypes[0]));
			Refresh();
		}
	}

	[Preserve]
	public override void Unregister(object t)
	{
		RemoveExternalMethodFromField(t, m_Fields[0]);
		Refresh();
	}
}
