using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal static class ReflectionUtils<T>
{
	private static readonly TypeInfo ObjectTypeInfo = typeof(object).GetTypeInfo();

	internal static T CreateObject(TypeInfo typeInfo, InteractionService commandService, IServiceProvider services = null)
	{
		return CreateBuilder(typeInfo, commandService)(services);
	}

	internal static Func<IServiceProvider, T> CreateBuilder(TypeInfo typeInfo, InteractionService commandService)
	{
		ConstructorInfo constructor = GetConstructor(typeInfo);
		ParameterInfo[] parameters = constructor.GetParameters();
		PropertyInfo[] properties = GetProperties(typeInfo);
		return delegate(IServiceProvider services)
		{
			object[] array = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = GetMember(commandService, services, parameters[i].ParameterType, typeInfo);
			}
			T val = InvokeConstructor(constructor, array, typeInfo);
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				propertyInfo.SetValue(val, GetMember(commandService, services, propertyInfo.PropertyType, typeInfo));
			}
			return val;
		};
	}

	private static T InvokeConstructor(ConstructorInfo constructor, object[] args, TypeInfo ownerType)
	{
		try
		{
			return (T)constructor.Invoke(args);
		}
		catch (Exception innerException)
		{
			throw new Exception("Failed to create \"" + ownerType.FullName + "\".", innerException);
		}
	}

	private static ConstructorInfo GetConstructor(TypeInfo ownerType)
	{
		ConstructorInfo[] array = ownerType.DeclaredConstructors.Where((ConstructorInfo x) => !x.IsStatic).ToArray();
		if (array.Length == 0)
		{
			throw new InvalidOperationException("No constructor found for \"" + ownerType.FullName + "\".");
		}
		if (array.Length > 1)
		{
			throw new InvalidOperationException("Multiple constructors found for \"" + ownerType.FullName + "\".");
		}
		return array[0];
	}

	private static PropertyInfo[] GetProperties(TypeInfo ownerType)
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		while (ownerType != ObjectTypeInfo)
		{
			foreach (PropertyInfo declaredProperty in ownerType.DeclaredProperties)
			{
				MethodInfo setMethod = declaredProperty.SetMethod;
				if ((object)setMethod != null && !setMethod.IsStatic)
				{
					MethodInfo setMethod2 = declaredProperty.SetMethod;
					if ((object)setMethod2 != null && setMethod2.IsPublic)
					{
						list.Add(declaredProperty);
					}
				}
			}
			ownerType = ownerType.BaseType.GetTypeInfo();
		}
		return list.ToArray();
	}

	private static object GetMember(InteractionService commandService, IServiceProvider services, Type memberType, TypeInfo ownerType)
	{
		if (memberType == typeof(InteractionService))
		{
			return commandService;
		}
		if (memberType == typeof(IServiceProvider) || memberType == services.GetType())
		{
			return services;
		}
		object service = services.GetService(memberType);
		if (service != null)
		{
			return service;
		}
		throw new InvalidOperationException("Failed to create \"" + ownerType.FullName + "\", dependency \"" + memberType.Name + "\" was not found.");
	}

	internal static Func<T, object[], Task> CreateMethodInvoker(MethodInfo methodInfo)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		Expression[] array = new Expression[parameters.Length];
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "instance");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "args");
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			ConstantExpression index = Expression.Constant(i);
			BinaryExpression expression = Expression.ArrayIndex(parameterExpression2, index);
			array[i] = Expression.Convert(expression, parameterInfo.ParameterType);
		}
		return Expression.Lambda<Func<T, object[], Task>>(Expression.Convert(Expression.Call(Expression.Convert(parameterExpression, methodInfo.ReflectedType), methodInfo, array), typeof(Task)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	internal static Func<IServiceProvider, T> CreateLambdaBuilder(TypeInfo typeInfo, InteractionService commandService)
	{
		ConstructorInfo constructor = GetConstructor(typeInfo);
		ParameterInfo[] parameters = constructor.GetParameters();
		PropertyInfo[] properties = GetProperties(typeInfo);
		Func<object[], object[], T> lambda = CreateLambdaMemberInit(typeInfo, constructor);
		return delegate(IServiceProvider services)
		{
			object[] array = new object[parameters.Length];
			object[] array2 = new object[properties.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = GetMember(commandService, services, parameters[i].ParameterType, typeInfo);
			}
			for (int j = 0; j < properties.Length; j++)
			{
				array2[j] = GetMember(commandService, services, properties[j].PropertyType, typeInfo);
			}
			return lambda(array, array2);
		};
	}

	internal static Func<object[], T> CreateLambdaConstructorInvoker(TypeInfo typeInfo)
	{
		ConstructorInfo constructor = GetConstructor(typeInfo);
		ParameterInfo[] parameters = constructor.GetParameters();
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
		Expression[] array = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ConstantExpression index = Expression.Constant(i);
			BinaryExpression expression = Expression.ArrayIndex(parameterExpression, index);
			array[i] = Expression.Convert(expression, parameters[i].ParameterType);
		}
		return Expression.Lambda<Func<object[], T>>(Expression.New(constructor, array), new ParameterExpression[1] { parameterExpression }).Compile();
	}

	internal static Action<T, object> CreateLambdaPropertySetter(PropertyInfo propertyInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "instance");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
		return Expression.Lambda<Action<T, object>>(Expression.Assign(Expression.Property(parameterExpression, propertyInfo), Expression.Convert(parameterExpression2, propertyInfo.PropertyType)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	internal static Func<object[], object[], T> CreateLambdaMemberInit(TypeInfo typeInfo, ConstructorInfo constructor, Predicate<PropertyInfo> propertySelect = null)
	{
		if (propertySelect == null)
		{
			propertySelect = (PropertyInfo x) => true;
		}
		ParameterInfo[] parameters = constructor.GetParameters();
		PropertyInfo[] array = (from x in GetProperties(typeInfo)
			where propertySelect(x)
			select x).ToArray();
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "props");
		Expression[] array2 = new Expression[parameters.Length];
		for (int num = 0; num < parameters.Length; num++)
		{
			ConstantExpression index = Expression.Constant(num);
			BinaryExpression expression = Expression.ArrayIndex(parameterExpression, index);
			array2[num] = Expression.Convert(expression, parameters[num].ParameterType);
		}
		NewExpression newExpression = Expression.New(constructor, array2);
		MemberAssignment[] array3 = new MemberAssignment[array.Length];
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			ConstantExpression index2 = Expression.Constant(num2);
			UnaryExpression expression2 = Expression.Convert(Expression.ArrayIndex(parameterExpression2, index2), array[num2].PropertyType);
			array3[num2] = Expression.Bind(array[num2], expression2);
		}
		MemberBinding[] bindings = array3;
		MemberInitExpression body = Expression.MemberInit(newExpression, bindings);
		Func<object[], object[], T> lambda = Expression.Lambda<Func<object[], object[], T>>(body, new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
		return (object[] args, object[] props) => lambda(args, props);
	}
}
