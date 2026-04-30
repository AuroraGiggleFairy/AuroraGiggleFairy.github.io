using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public static class vp_GlobalEventReturn<R>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	public static void Register(string name, vp_GlobalCallbackReturn<R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<R>> list = (List<vp_GlobalCallbackReturn<R>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallbackReturn<R>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	public static void Unregister(string name, vp_GlobalCallbackReturn<R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<R>> list = (List<vp_GlobalCallbackReturn<R>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static R Send(string name)
	{
		return Send(name, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static R Send(string name, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		List<vp_GlobalCallbackReturn<R>> list = (List<vp_GlobalCallbackReturn<R>>)m_Callbacks[name];
		if (list != null)
		{
			R result = default(R);
			{
				foreach (vp_GlobalCallbackReturn<R> item in list)
				{
					result = item();
				}
				return result;
			}
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
		return default(R);
	}
}
public static class vp_GlobalEventReturn<T, R>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	public static void Register(string name, vp_GlobalCallbackReturn<T, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, R>> list = (List<vp_GlobalCallbackReturn<T, R>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallbackReturn<T, R>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	public static void Unregister(string name, vp_GlobalCallbackReturn<T, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, R>> list = (List<vp_GlobalCallbackReturn<T, R>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static R Send(string name, T arg1)
	{
		return Send(name, arg1, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static R Send(string name, T arg1, vp_GlobalEventMode mode)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (arg1 == null)
		{
			throw new ArgumentNullException("arg1");
		}
		List<vp_GlobalCallbackReturn<T, R>> list = (List<vp_GlobalCallbackReturn<T, R>>)m_Callbacks[name];
		if (list != null)
		{
			R result = default(R);
			{
				foreach (vp_GlobalCallbackReturn<T, R> item in list)
				{
					result = item(arg1);
				}
				return result;
			}
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
		return default(R);
	}
}
public static class vp_GlobalEventReturn<T, U, R>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	public static void Register(string name, vp_GlobalCallbackReturn<T, U, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, U, R>> list = (List<vp_GlobalCallbackReturn<T, U, R>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallbackReturn<T, U, R>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	public static void Unregister(string name, vp_GlobalCallbackReturn<T, U, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, U, R>> list = (List<vp_GlobalCallbackReturn<T, U, R>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static R Send(string name, T arg1, U arg2)
	{
		return Send(name, arg1, arg2, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static R Send(string name, T arg1, U arg2, vp_GlobalEventMode mode)
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
		List<vp_GlobalCallbackReturn<T, U, R>> list = (List<vp_GlobalCallbackReturn<T, U, R>>)m_Callbacks[name];
		if (list != null)
		{
			R result = default(R);
			{
				foreach (vp_GlobalCallbackReturn<T, U, R> item in list)
				{
					result = item(arg1, arg2);
				}
				return result;
			}
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
		return default(R);
	}
}
[Preserve]
public static class vp_GlobalEventReturn<T, U, V, R>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Hashtable m_Callbacks = vp_GlobalEventInternal.Callbacks;

	[Preserve]
	public static void Register(string name, vp_GlobalCallbackReturn<T, U, V, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, U, V, R>> list = (List<vp_GlobalCallbackReturn<T, U, V, R>>)m_Callbacks[name];
		if (list == null)
		{
			list = new List<vp_GlobalCallbackReturn<T, U, V, R>>();
			m_Callbacks.Add(name, list);
		}
		list.Add(callback);
	}

	[Preserve]
	public static void Unregister(string name, vp_GlobalCallbackReturn<T, U, V, R> callback)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		List<vp_GlobalCallbackReturn<T, U, V, R>> list = (List<vp_GlobalCallbackReturn<T, U, V, R>>)m_Callbacks[name];
		if (list != null)
		{
			list.Remove(callback);
			return;
		}
		throw vp_GlobalEventInternal.ShowUnregisterException(name);
	}

	public static R Send(string name, T arg1, U arg2, V arg3)
	{
		return Send(name, arg1, arg2, arg3, vp_GlobalEventMode.DONT_REQUIRE_LISTENER);
	}

	public static R Send(string name, T arg1, U arg2, V arg3, vp_GlobalEventMode mode)
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
		List<vp_GlobalCallbackReturn<T, U, V, R>> list = (List<vp_GlobalCallbackReturn<T, U, V, R>>)m_Callbacks[name];
		if (list != null)
		{
			R result = default(R);
			{
				foreach (vp_GlobalCallbackReturn<T, U, V, R> item in list)
				{
					result = item(arg1, arg2, arg3);
				}
				return result;
			}
		}
		if (mode == vp_GlobalEventMode.REQUIRE_LISTENER)
		{
			throw vp_GlobalEventInternal.ShowSendException(name);
		}
		return default(R);
	}
}
