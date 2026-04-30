using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionServiceExtensions
{
	public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		return Add(services, serviceType, implementationType, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Transient);
	}

	public static IServiceCollection AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddTransient(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddTransient(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		return services.AddTransient(serviceType, serviceType);
	}

	public static IServiceCollection AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddTransient(typeof(TService));
	}

	public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddTransient(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddTransient(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		return Add(services, serviceType, implementationType, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddScoped(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddScoped(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		return services.AddScoped(serviceType, serviceType);
	}

	public static IServiceCollection AddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddScoped(typeof(TService));
	}

	public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddScoped(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddScoped(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		return Add(services, serviceType, implementationType, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Add(services, serviceType, implementationFactory, ServiceLifetime.Singleton);
	}

	public static IServiceCollection AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddSingleton(typeof(TService), typeof(TImplementation));
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		return services.AddSingleton(serviceType, serviceType);
	}

	public static IServiceCollection AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		return services.AddSingleton(typeof(TService));
	}

	public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddSingleton(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return services.AddSingleton(typeof(TService), implementationFactory);
	}

	public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationInstance == null)
		{
			throw new ArgumentNullException("implementationInstance");
		}
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationInstance);
		services.Add(item);
		return services;
	}

	public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance) where TService : class
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (implementationInstance == null)
		{
			throw new ArgumentNullException("implementationInstance");
		}
		return services.AddSingleton(typeof(TService), implementationInstance);
	}

	private static IServiceCollection Add(IServiceCollection collection, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationType, lifetime);
		collection.Add(item);
		return collection;
	}

	private static IServiceCollection Add(IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
	{
		ServiceDescriptor item = new ServiceDescriptor(serviceType, implementationFactory, lifetime);
		collection.Add(item);
		return collection;
	}
}
