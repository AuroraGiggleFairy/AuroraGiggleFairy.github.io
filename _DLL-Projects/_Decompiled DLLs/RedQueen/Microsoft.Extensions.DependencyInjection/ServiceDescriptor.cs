using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

[DebuggerDisplay("Lifetime = {Lifetime}, ServiceType = {ServiceType}, ImplementationType = {ImplementationType}")]
internal class ServiceDescriptor
{
	public ServiceLifetime Lifetime { get; }

	public Type ServiceType { get; }

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public Type? ImplementationType { get; }

	public object? ImplementationInstance { get; }

	public Func<IServiceProvider, object>? ImplementationFactory { get; }

	public ServiceDescriptor(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
		: this(serviceType, lifetime)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		ImplementationType = implementationType;
	}

	public ServiceDescriptor(Type serviceType, object instance)
		: this(serviceType, ServiceLifetime.Singleton)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		ImplementationInstance = instance;
	}

	public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
		: this(serviceType, lifetime)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (factory == null)
		{
			throw new ArgumentNullException("factory");
		}
		ImplementationFactory = factory;
	}

	private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime)
	{
		Lifetime = lifetime;
		ServiceType = serviceType;
	}

	public override string ToString()
	{
		string text = string.Format("{0}: {1} {2}: {3} ", "ServiceType", ServiceType, "Lifetime", Lifetime);
		if (ImplementationType != null)
		{
			return text + string.Format("{0}: {1}", "ImplementationType", ImplementationType);
		}
		if (ImplementationFactory != null)
		{
			return text + string.Format("{0}: {1}", "ImplementationFactory", ImplementationFactory.Method);
		}
		return text + string.Format("{0}: {1}", "ImplementationInstance", ImplementationInstance);
	}

	internal Type GetImplementationType()
	{
		if (ImplementationType != null)
		{
			return ImplementationType;
		}
		if (ImplementationInstance != null)
		{
			return ImplementationInstance.GetType();
		}
		if (ImplementationFactory != null)
		{
			Type[] genericTypeArguments = ImplementationFactory.GetType().GenericTypeArguments;
			return genericTypeArguments[1];
		}
		return null;
	}

	public static ServiceDescriptor Transient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>() where TService : class where TImplementation : class, TService
	{
		return Describe<TService, TImplementation>(ServiceLifetime.Transient);
	}

	public static ServiceDescriptor Transient(Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		return Describe(service, implementationType, ServiceLifetime.Transient);
	}

	public static ServiceDescriptor Transient<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Transient);
	}

	public static ServiceDescriptor Transient<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Transient);
	}

	public static ServiceDescriptor Transient(Type service, Func<IServiceProvider, object> implementationFactory)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(service, implementationFactory, ServiceLifetime.Transient);
	}

	public static ServiceDescriptor Scoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>() where TService : class where TImplementation : class, TService
	{
		return Describe<TService, TImplementation>(ServiceLifetime.Scoped);
	}

	public static ServiceDescriptor Scoped(Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		return Describe(service, implementationType, ServiceLifetime.Scoped);
	}

	public static ServiceDescriptor Scoped<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
	}

	public static ServiceDescriptor Scoped<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
	}

	public static ServiceDescriptor Scoped(Type service, Func<IServiceProvider, object> implementationFactory)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(service, implementationFactory, ServiceLifetime.Scoped);
	}

	public static ServiceDescriptor Singleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>() where TService : class where TImplementation : class, TService
	{
		return Describe<TService, TImplementation>(ServiceLifetime.Singleton);
	}

	public static ServiceDescriptor Singleton(Type service, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (implementationType == null)
		{
			throw new ArgumentNullException("implementationType");
		}
		return Describe(service, implementationType, ServiceLifetime.Singleton);
	}

	public static ServiceDescriptor Singleton<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
	}

	public static ServiceDescriptor Singleton<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
	{
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
	}

	public static ServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationFactory == null)
		{
			throw new ArgumentNullException("implementationFactory");
		}
		return Describe(serviceType, implementationFactory, ServiceLifetime.Singleton);
	}

	public static ServiceDescriptor Singleton<TService>(TService implementationInstance) where TService : class
	{
		if (implementationInstance == null)
		{
			throw new ArgumentNullException("implementationInstance");
		}
		return Singleton(typeof(TService), implementationInstance);
	}

	public static ServiceDescriptor Singleton(Type serviceType, object implementationInstance)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (implementationInstance == null)
		{
			throw new ArgumentNullException("implementationInstance");
		}
		return new ServiceDescriptor(serviceType, implementationInstance);
	}

	private static ServiceDescriptor Describe<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(ServiceLifetime lifetime) where TService : class where TImplementation : class, TService
	{
		return Describe(typeof(TService), typeof(TImplementation), lifetime);
	}

	public static ServiceDescriptor Describe(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
	{
		return new ServiceDescriptor(serviceType, implementationType, lifetime);
	}

	public static ServiceDescriptor Describe(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
	{
		return new ServiceDescriptor(serviceType, implementationFactory, lifetime);
	}
}
