using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleJson2.Reflection;

[GeneratedCode("reflection-utils", "1.0.0")]
[PublicizedFrom(EAccessModifier.Internal)]
public class ReflectionUtils
{
	public delegate object GetDelegate(object source);

	public delegate void SetDelegate(object source, object value);

	public delegate object ConstructorDelegate(params object[] args);

	public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);

	[PublicizedFrom(EAccessModifier.Private)]
	public static class Assigner<T>
	{
		public static T Assign(ref T left, T right)
		{
			return left = right;
		}
	}

	public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object _lock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<TKey, TValue> _dictionary;

		public ICollection<TKey> Keys => _dictionary.Keys;

		public ICollection<TValue> Values => _dictionary.Values;

		public TValue this[TKey key]
		{
			get
			{
				return Get(key);
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int Count => _dictionary.Count;

		public bool IsReadOnly
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ThreadSafeDictionary(ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
		{
			_valueFactory = valueFactory;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TValue Get(TKey key)
		{
			if (_dictionary == null)
			{
				return AddValue(key);
			}
			if (!_dictionary.TryGetValue(key, out var value))
			{
				return AddValue(key);
			}
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TValue AddValue(TKey key)
		{
			TValue val = _valueFactory(key);
			lock (_lock)
			{
				if (_dictionary == null)
				{
					_dictionary = new Dictionary<TKey, TValue>();
					_dictionary[key] = val;
				}
				else
				{
					if (_dictionary.TryGetValue(key, out var value))
					{
						return value;
					}
					Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
					dictionary[key] = val;
					_dictionary = dictionary;
				}
			}
			return val;
		}

		public void Add(TKey key, TValue value)
		{
			throw new NotImplementedException();
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			value = this[key];
			return true;
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object[] EmptyObjects = new object[0];

	public static Type GetTypeInfo(Type type)
	{
		return type;
	}

	public static Attribute GetAttribute(MemberInfo info, Type type)
	{
		if (info == null || type == null || !Attribute.IsDefined(info, type))
		{
			return null;
		}
		return Attribute.GetCustomAttribute(info, type);
	}

	public static Type GetGenericListElementType(Type type)
	{
		foreach (Type item in (IEnumerable<Type>)type.GetInterfaces())
		{
			if (IsTypeGeneric(item) && item.GetGenericTypeDefinition() == typeof(IList<>))
			{
				return GetGenericTypeArguments(item)[0];
			}
		}
		return GetGenericTypeArguments(type)[0];
	}

	public static Attribute GetAttribute(Type objectType, Type attributeType)
	{
		if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
		{
			return null;
		}
		return Attribute.GetCustomAttribute(objectType, attributeType);
	}

	public static Type[] GetGenericTypeArguments(Type type)
	{
		return type.GetGenericArguments();
	}

	public static bool IsTypeGeneric(Type type)
	{
		return GetTypeInfo(type).IsGenericType;
	}

	public static bool IsTypeGenericeCollectionInterface(Type type)
	{
		if (!IsTypeGeneric(type))
		{
			return false;
		}
		Type genericTypeDefinition = type.GetGenericTypeDefinition();
		if (!(genericTypeDefinition == typeof(IList<>)) && !(genericTypeDefinition == typeof(ICollection<>)))
		{
			return genericTypeDefinition == typeof(IEnumerable<>);
		}
		return true;
	}

	public static bool IsAssignableFrom(Type type1, Type type2)
	{
		return GetTypeInfo(type1).IsAssignableFrom(GetTypeInfo(type2));
	}

	public static bool IsTypeDictionary(Type type)
	{
		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			return true;
		}
		if (!GetTypeInfo(type).IsGenericType)
		{
			return false;
		}
		return type.GetGenericTypeDefinition() == typeof(IDictionary<, >);
	}

	public static bool IsNullableType(Type type)
	{
		if (GetTypeInfo(type).IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	public static object ToNullableType(object obj, Type nullableType)
	{
		if (obj != null)
		{
			return Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture);
		}
		return null;
	}

	public static bool IsValueType(Type type)
	{
		return GetTypeInfo(type).IsValueType;
	}

	public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
	{
		return type.GetConstructors();
	}

	public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
	{
		foreach (ConstructorInfo constructor in GetConstructors(type))
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			if (argsType.Length != parameters.Length)
			{
				continue;
			}
			int num = 0;
			bool flag = true;
			ParameterInfo[] parameters2 = constructor.GetParameters();
			for (int i = 0; i < parameters2.Length; i++)
			{
				if (parameters2[i].ParameterType != argsType[num])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return constructor;
			}
		}
		return null;
	}

	public static IEnumerable<PropertyInfo> GetProperties(Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static IEnumerable<FieldInfo> GetFields(Type type)
	{
		return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo)
	{
		return propertyInfo.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo)
	{
		return propertyInfo.GetSetMethod(nonPublic: true);
	}

	public static ConstructorDelegate GetContructor(ConstructorInfo constructorInfo)
	{
		return GetConstructorByExpression(constructorInfo);
	}

	public static ConstructorDelegate GetContructor(Type type, params Type[] argsType)
	{
		return GetConstructorByExpression(type, argsType);
	}

	public static ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo)
	{
		return [PublicizedFrom(EAccessModifier.Internal)] (object[] args) => constructorInfo.Invoke(args);
	}

	public static ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
	{
		ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
		if (!(constructorInfo == null))
		{
			return GetConstructorByReflection(constructorInfo);
		}
		return null;
	}

	public static ConstructorDelegate GetConstructorByExpression(ConstructorInfo constructorInfo)
	{
		ParameterInfo[] parameters = constructorInfo.GetParameters();
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
		Expression[] array = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			Expression index = Expression.Constant(i);
			Type parameterType = parameters[i].ParameterType;
			Expression expression = Expression.Convert(Expression.ArrayIndex(parameterExpression, index), parameterType);
			array[i] = expression;
		}
		Expression<Func<object[], object>> expression2 = Expression.Lambda<Func<object[], object>>(Expression.New(constructorInfo, array), new ParameterExpression[1] { parameterExpression });
		Func<object[], object> compiledLambda = expression2.Compile();
		return [PublicizedFrom(EAccessModifier.Internal)] (object[] args) => compiledLambda(args);
	}

	public static ConstructorDelegate GetConstructorByExpression(Type type, params Type[] argsType)
	{
		ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
		if (!(constructorInfo == null))
		{
			return GetConstructorByExpression(constructorInfo);
		}
		return null;
	}

	public static GetDelegate GetGetMethod(PropertyInfo propertyInfo)
	{
		return GetGetMethodByExpression(propertyInfo);
	}

	public static GetDelegate GetGetMethod(FieldInfo fieldInfo)
	{
		return GetGetMethodByExpression(fieldInfo);
	}

	public static GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
	{
		MethodInfo methodInfo = GetGetterMethodInfo(propertyInfo);
		return [PublicizedFrom(EAccessModifier.Internal)] (object source) => methodInfo.Invoke(source, EmptyObjects);
	}

	public static GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
	{
		return [PublicizedFrom(EAccessModifier.Internal)] (object source) => fieldInfo.GetValue(source);
	}

	public static GetDelegate GetGetMethodByExpression(PropertyInfo propertyInfo)
	{
		MethodInfo getterMethodInfo = GetGetterMethodInfo(propertyInfo);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
		UnaryExpression instance = ((!IsValueType(propertyInfo.DeclaringType)) ? Expression.TypeAs(parameterExpression, propertyInfo.DeclaringType) : Expression.Convert(parameterExpression, propertyInfo.DeclaringType));
		Func<object, object> compiled = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instance, getterMethodInfo), typeof(object)), new ParameterExpression[1] { parameterExpression }).Compile();
		return [PublicizedFrom(EAccessModifier.Internal)] (object source) => compiled(source);
	}

	public static GetDelegate GetGetMethodByExpression(FieldInfo fieldInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
		MemberExpression expression = Expression.Field(Expression.Convert(parameterExpression, fieldInfo.DeclaringType), fieldInfo);
		GetDelegate compiled = Expression.Lambda<GetDelegate>(Expression.Convert(expression, typeof(object)), new ParameterExpression[1] { parameterExpression }).Compile();
		return [PublicizedFrom(EAccessModifier.Internal)] (object source) => compiled(source);
	}

	public static SetDelegate GetSetMethod(PropertyInfo propertyInfo)
	{
		return GetSetMethodByExpression(propertyInfo);
	}

	public static SetDelegate GetSetMethod(FieldInfo fieldInfo)
	{
		return GetSetMethodByExpression(fieldInfo);
	}

	public static SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
	{
		MethodInfo methodInfo = GetSetterMethodInfo(propertyInfo);
		return [PublicizedFrom(EAccessModifier.Internal)] (object source, object value) =>
		{
			methodInfo.Invoke(source, new object[1] { value });
		};
	}

	public static SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo)
	{
		return [PublicizedFrom(EAccessModifier.Internal)] (object source, object value) =>
		{
			fieldInfo.SetValue(source, value);
		};
	}

	public static SetDelegate GetSetMethodByExpression(PropertyInfo propertyInfo)
	{
		MethodInfo setterMethodInfo = GetSetterMethodInfo(propertyInfo);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
		UnaryExpression instance = ((!IsValueType(propertyInfo.DeclaringType)) ? Expression.TypeAs(parameterExpression, propertyInfo.DeclaringType) : Expression.Convert(parameterExpression, propertyInfo.DeclaringType));
		UnaryExpression unaryExpression = ((!IsValueType(propertyInfo.PropertyType)) ? Expression.TypeAs(parameterExpression2, propertyInfo.PropertyType) : Expression.Convert(parameterExpression2, propertyInfo.PropertyType));
		Action<object, object> compiled = Expression.Lambda<Action<object, object>>(Expression.Call(instance, setterMethodInfo, unaryExpression), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
		return [PublicizedFrom(EAccessModifier.Internal)] (object source, object val) =>
		{
			compiled(source, val);
		};
	}

	public static SetDelegate GetSetMethodByExpression(FieldInfo fieldInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
		Action<object, object> compiled = Expression.Lambda<Action<object, object>>(Assign(Expression.Field(Expression.Convert(parameterExpression, fieldInfo.DeclaringType), fieldInfo), Expression.Convert(parameterExpression2, fieldInfo.FieldType)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
		return [PublicizedFrom(EAccessModifier.Internal)] (object source, object val) =>
		{
			compiled(source, val);
		};
	}

	public static BinaryExpression Assign(Expression left, Expression right)
	{
		MethodInfo method = typeof(Assigner<>).MakeGenericType(left.Type).GetMethod("Assign");
		return Expression.Add(left, right, method);
	}
}
