using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Discord.Interactions;

internal class TypeMap<TConverter, TData> where TConverter : class, ITypeConverter<TData>
{
	private readonly ConcurrentDictionary<Type, TConverter> _concretes;

	private readonly ConcurrentDictionary<Type, Type> _generics;

	private readonly InteractionService _interactionService;

	public TypeMap(InteractionService interactionService, IDictionary<Type, TConverter> concretes = null, IDictionary<Type, Type> generics = null)
	{
		_interactionService = interactionService;
		_concretes = ((concretes != null) ? new ConcurrentDictionary<Type, TConverter>(concretes) : new ConcurrentDictionary<Type, TConverter>());
		_generics = ((generics != null) ? new ConcurrentDictionary<Type, Type>(generics) : new ConcurrentDictionary<Type, Type>());
	}

	internal TConverter Get(Type type, IServiceProvider services = null)
	{
		if (_concretes.TryGetValue(type, out var value))
		{
			return value;
		}
		if (_generics.Any((KeyValuePair<Type, Type> x) => x.Key.IsAssignableFrom(type) || (x.Key.IsGenericTypeDefinition && type.IsGenericType && x.Key.GetGenericTypeDefinition() == type.GetGenericTypeDefinition())))
		{
			if (services == null)
			{
				services = EmptyServiceProvider.Instance;
			}
			TConverter val = ReflectionUtils<TConverter>.CreateObject(GetMostSpecific(type).MakeGenericType(type).GetTypeInfo(), _interactionService, services);
			_concretes[type] = val;
			return val;
		}
		if (_concretes.Any((KeyValuePair<Type, TConverter> x) => x.Value.CanConvertTo(type)))
		{
			return _concretes.First((KeyValuePair<Type, TConverter> x) => x.Value.CanConvertTo(type)).Value;
		}
		throw new ArgumentException("No type " + typeof(TConverter).Name + " is defined for this " + type.FullName, "type");
	}

	public void AddConcrete<TTarget>(TConverter converter)
	{
		AddConcrete(typeof(TTarget), converter);
	}

	public void AddConcrete(Type type, TConverter converter)
	{
		if (!converter.CanConvertTo(type))
		{
			throw new ArgumentException("This " + converter.GetType().FullName + " cannot read " + type.FullName + " and cannot be registered as its TypeConverter");
		}
		_concretes[type] = converter;
	}

	public void AddGeneric<TTarget>(Type converterType)
	{
		AddGeneric(typeof(TTarget), converterType);
	}

	public void AddGeneric(Type targetType, Type converterType)
	{
		if (!converterType.IsGenericTypeDefinition)
		{
			throw new ArgumentException(converterType.FullName + " is not generic.");
		}
		Type[] genericArguments = converterType.GetGenericArguments();
		if (genericArguments.Length > 1)
		{
			throw new InvalidOperationException("Valid generic " + converterType.FullName + "s cannot have more than 1 generic type parameter");
		}
		if (!genericArguments.SelectMany((Type x) => x.GetGenericParameterConstraints()).Any((Type x) => x.IsAssignableFrom(targetType)))
		{
			throw new InvalidOperationException("This generic class does not support type " + targetType.FullName);
		}
		_generics[targetType] = converterType;
	}

	public bool TryRemoveConcrete<TTarget>(out TConverter converter)
	{
		return TryRemoveConcrete(typeof(TTarget), out converter);
	}

	public bool TryRemoveConcrete(Type type, out TConverter converter)
	{
		return _concretes.TryRemove(type, out converter);
	}

	public bool TryRemoveGeneric<TTarget>(out Type converterType)
	{
		return TryRemoveGeneric(typeof(TTarget), out converterType);
	}

	public bool TryRemoveGeneric(Type targetType, out Type converterType)
	{
		return _generics.TryRemove(targetType, out converterType);
	}

	private Type GetMostSpecific(Type type)
	{
		if (_generics.TryGetValue(type, out var value))
		{
			return value;
		}
		if (type.IsGenericType && _generics.TryGetValue(type.GetGenericTypeDefinition(), out var value2))
		{
			return value2;
		}
		Type[] typeInterfaces = type.GetInterfaces();
		return (from x in _generics
			where x.Key.IsAssignableFrom(type)
			orderby typeInterfaces.Count((Type y) => y.IsAssignableFrom(x.Key)) descending
			select x).First().Value;
	}
}
