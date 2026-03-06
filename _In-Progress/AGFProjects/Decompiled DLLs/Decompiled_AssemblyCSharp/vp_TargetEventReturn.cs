using System;
using UnityEngine;

public static class vp_TargetEventReturn<R>
{
	public static void Register(object target, string eventName, Func<R> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 4);
	}

	public static void Unregister(object target, string eventName, Func<R> callback)
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

	public static R Send(object target, string eventName, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 4, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<R>)callback)();
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}

	public static R SendUpwards(Component target, string eventName, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 4, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<R>)callback)();
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}
}
public static class vp_TargetEventReturn<T, R>
{
	public static void Register(object target, string eventName, Func<T, R> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 5);
	}

	public static void Unregister(object target, string eventName, Func<T, R> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static R Send(object target, string eventName, T arg, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 5, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, R>)callback)(arg);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}

	public static R SendUpwards(Component target, string eventName, T arg, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 5, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, R>)callback)(arg);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}
}
public static class vp_TargetEventReturn<T, U, R>
{
	public static void Register(object target, string eventName, Func<T, U, R> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 6);
	}

	public static void Unregister(object target, string eventName, Func<T, U, R> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static R Send(object target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 6, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, U, R>)callback)(arg1, arg2);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}

	public static R SendUpwards(Component target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 6, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, U, R>)callback)(arg1, arg2);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}
}
public static class vp_TargetEventReturn<T, U, V, R>
{
	public static void Register(object target, string eventName, Func<T, U, V, R> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 7);
	}

	public static void Unregister(object target, string eventName, Func<T, U, V, R> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target);
	}

	public static R Send(object target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: false, 7, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, U, V, R>)callback)(arg1, arg2, arg3);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}

	public static R SendUpwards(Component target, string eventName, T arg1, U arg2, V arg3, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		while (true)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, upwards: true, 7, options);
			if ((object)callback == null)
			{
				break;
			}
			try
			{
				return ((Func<T, U, V, R>)callback)(arg1, arg2, arg3);
			}
			catch
			{
				eventName += "_";
			}
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		return default(R);
	}
}
