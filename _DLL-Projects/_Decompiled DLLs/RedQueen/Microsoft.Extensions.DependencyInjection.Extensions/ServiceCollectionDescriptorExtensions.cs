using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

internal static class ServiceCollectionDescriptorExtensions
{
	public static IServiceCollection Add(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		collection.Add(descriptor);
		return collection;
	}

	public static IServiceCollection Add(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (descriptors == null)
		{
			throw new ArgumentNullException("descriptors");
		}
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			collection.Add(descriptor);
		}
		return collection;
	}

	public static void TryAdd(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		if (!collection.Any((ServiceDescriptor d) => d.ServiceType == descriptor.ServiceType))
		{
			collection.Add(descriptor);
		}
	}

	public static void TryAdd(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (descriptors == null)
		{
			throw new ArgumentNullException("descriptors");
		}
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			collection.TryAdd(descriptor);
		}
	}

	public static void TryAddTransient(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Transient(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddTransient(typeof(TService), typeof(TService));
	}

	public static void TryAddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddTransient(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Transient(implementationFactory));
	}

	public static void TryAddScoped(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Scoped(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddScoped(typeof(TService), typeof(TService));
	}

	public static void TryAddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddScoped(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Scoped(implementationFactory));
	}

	public static void TryAddSingleton(this IServiceCollection collection, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, service);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton(this IServiceCollection collection, Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, implementationType);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton(this IServiceCollection collection, Type service, Func<IServiceProvider, object> implementationFactory)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(service, implementationFactory);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection collection) where TService : class
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddSingleton(typeof(TService), typeof(TService));
	}

	public static void TryAddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection collection) where TService : class where TImplementation : class, TService
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		collection.TryAddSingleton(typeof(TService), typeof(TImplementation));
	}

	public static void TryAddSingleton<TService>(this IServiceCollection collection, TService instance) where TService : class
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		ServiceDescriptor descriptor = ServiceDescriptor.Singleton(typeof(TService), instance);
		collection.TryAdd(descriptor);
	}

	public static void TryAddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		services.TryAdd(ServiceDescriptor.Singleton(implementationFactory));
	}

	public static void TryAddEnumerable(this IServiceCollection services, ServiceDescriptor descriptor)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		Type implementationType = descriptor.GetImplementationType();
		if (implementationType == typeof(object) || implementationType == descriptor.ServiceType)
		{
			throw new ArgumentException(System.SR.Format(System.SR.TryAddIndistinguishableTypeToEnumerable, implementationType, descriptor.ServiceType), "descriptor");
		}
		if (!services.Any((ServiceDescriptor d) => d.ServiceType == descriptor.ServiceType && d.GetImplementationType() == implementationType))
		{
			services.Add(descriptor);
		}
	}

	public static void TryAddEnumerable(this IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		if (descriptors == null)
		{
			throw new ArgumentNullException("descriptors");
		}
		foreach (ServiceDescriptor descriptor in descriptors)
		{
			services.TryAddEnumerable(descriptor);
		}
	}

	public static IServiceCollection Replace(this IServiceCollection collection, ServiceDescriptor descriptor)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		ServiceDescriptor serviceDescriptor = collection.FirstOrDefault((ServiceDescriptor s) => s.ServiceType == descriptor.ServiceType);
		if (serviceDescriptor != null)
		{
			collection.Remove(serviceDescriptor);
		}
		collection.Add(descriptor);
		return collection;
	}

	public static IServiceCollection RemoveAll<T>(this IServiceCollection collection)
	{
		return collection.RemoveAll(typeof(T));
	}

	public static IServiceCollection RemoveAll(this IServiceCollection collection, Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			ServiceDescriptor serviceDescriptor = collection[num];
			if (serviceDescriptor.ServiceType == serviceType)
			{
				collection.RemoveAt(num);
			}
		}
		return collection;
	}
}
