using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceProviderServiceExtensions
{
	public static T? GetService<T>(this IServiceProvider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return (T)provider.GetService(typeof(T));
	}

	public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (provider is ISupportRequiredService supportRequiredService)
		{
			return supportRequiredService.GetRequiredService(serviceType);
		}
		object service = provider.GetService(serviceType);
		if (service == null)
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.NoServiceRegistered, serviceType));
		}
		return service;
	}

	public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return (T)provider.GetRequiredService(typeof(T));
	}

	public static IEnumerable<T> GetServices<T>(this IServiceProvider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.GetRequiredService<IEnumerable<T>>();
	}

	public static IEnumerable<object?> GetServices(this IServiceProvider provider, Type serviceType)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		Type serviceType2 = typeof(IEnumerable<>).MakeGenericType(serviceType);
		return (IEnumerable<object>)provider.GetRequiredService(serviceType2);
	}

	public static IServiceScope CreateScope(this IServiceProvider provider)
	{
		return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
	}
}
