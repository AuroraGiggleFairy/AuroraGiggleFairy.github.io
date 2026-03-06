using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

public static class vp_GlobalEvent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	public static void Register(string name, vp_GlobalCallback callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	public static void Unregister(string name, vp_GlobalCallback callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static void Send(string name)
	{
		Send(name, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		List<vp_GlobalCallback> list = (List<vp_GlobalCallback>)m_Callbacks[name];
		if (list != null)
		{
			foreach (vp_GlobalCallback item in list)
			{
				item();
			}
			return;
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
	}
}
public static class vp_GlobalEvent<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	[Preserve]
	public static void Register(string name, vp_GlobalCallback<T> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T>> list = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback<T>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	[Preserve]
	public static void Unregister(string name, vp_GlobalCallback<T> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T>> list = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static void Send(string name, T arg1)
	{
		Send(name, arg1, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, T arg1, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (arg1 == null)
		{
			throw new ArgumentNullException("arg1");
		}
		List<vp_GlobalCallback<T>> list = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
		if (list != null)
		{
			foreach (vp_GlobalCallback<T> item in list)
			{
				item(arg1);
			}
			return;
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
	}
}
public static class vp_GlobalEvent<T, U>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	[Preserve]
	public static void Register(string name, vp_GlobalCallback<T, U> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U>> list = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback<T, U>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	[Preserve]
	public static void Unregister(string name, vp_GlobalCallback<T, U> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U>> list = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static void Send(string name, T arg1, U arg2)
	{
		Send(name, arg1, arg2, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, T arg1, U arg2, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (arg1 == null)
		{
			throw new ArgumentNullException("arg1");
		}
		if (arg2 == null)
		{
			throw new ArgumentNullException("arg2");
		}
		List<vp_GlobalCallback<T, U>> list = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
		if (list != null)
		{
			foreach (vp_GlobalCallback<T, U> item in list)
			{
				item(arg1, arg2);
			}
			return;
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
	}
}
public static class vp_GlobalEvent<T, U, V>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	[Preserve]
	public static void Register(string name, vp_GlobalCallback<T, U, V> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallback<T, U, V>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	[Preserve]
	public static void Unregister(string name, vp_GlobalCallback<T, U, V> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	[Preserve]
	public static void Send(string name, T arg1, U arg2, V arg3)
	{
		Send(name, arg1, arg2, arg3, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static void Send(string name, T arg1, U arg2, V arg3, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (arg1 == null)
		{
			throw new ArgumentNullException("arg1");
		}
		if (arg2 == null)
		{
			throw new ArgumentNullException("arg2");
		}
		if (arg3 == null)
		{
			throw new ArgumentNullException("arg3");
		}
		List<vp_GlobalCallback<T, U, V>> list = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
		if (list != null)
		{
			foreach (vp_GlobalCallback<T, U, V> item in list)
			{
				item(arg1, arg2, arg3);
			}
			return;
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
	}
}
