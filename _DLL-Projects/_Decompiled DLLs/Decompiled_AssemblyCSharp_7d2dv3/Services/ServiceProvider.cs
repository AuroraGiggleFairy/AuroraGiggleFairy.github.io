using System;
using System.Collections.Generic;

namespace Services;

public class ServiceProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, object> _services;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ServiceProvider Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Init()
	{
		if (Instance != null)
		{
			Log.Out("ServiceProvider has already exists");
			return;
		}
		Instance = new ServiceProvider();
		Instance.InternalInit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalInit()
	{
		_services = new Dictionary<Type, object>();
	}

	public void Register(Type type, object obj)
	{
		_services.Add(type, obj);
	}

	public T Get<T>()
	{
		return (T)_services[typeof(T)];
	}
}
