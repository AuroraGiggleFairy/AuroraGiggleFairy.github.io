using System;
using UnityEngine;

public static class vp_TargetEvent
{
	public static void Register(object target, string eventName, Action callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 0);
	}

	public static void Unregister(object target, string eventName, Action callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static void Unregister(Component component)
	{
		vp_TargetEventHandler.Unregister(component);
	}

	public static void Send(object target, string eventName, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 0, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action)callback)();
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}

	public static void SendUpwards(Component target, string eventName, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 0, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action)callback)();
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}
}
public static class vp_TargetEvent<T>
{
	public static void Register(object target, string eventName, Action<T> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 1);
	}

	public static void Unregister(object target, string eventName, Action<T> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static void Send(object target, string eventName, T arg, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 1, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T>)callback)(arg);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}

	public static void SendUpwards(Component target, string eventName, T arg, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 1, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T>)callback)(arg);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}
}
public static class vp_TargetEvent<T, U>
{
	public static void Register(object target, string eventName, Action<T, U> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 2);
	}

	public static void Unregister(object target, string eventName, Action<T, U> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static void Send(object target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 2, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U>)callback)(arg1, arg2);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}

	public static void SendUpwards(Component target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 2, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U>)callback)(arg1, arg2);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}
}
public static class vp_TargetEvent<T, U, V>
{
	public static void Register(object target, string eventName, Action<T, U, V> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 3);
	}

	public static void Unregister(object target, string eventName, Action<T, U, V> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static void Send(object target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 3, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U, V>)callback)(arg1, arg2, arg3);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}

	public static void SendUpwards(Component target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 3, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				((Action<T, U, V>)callback)(arg1, arg2, arg3);
				return;
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
	}
}
