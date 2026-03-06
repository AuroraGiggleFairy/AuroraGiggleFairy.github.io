using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ActivatorUtilities
{
	private struct ConstructorMatcher(ConstructorInfo constructor)
	{
		private readonly ConstructorInfo _constructor = constructor;

		private readonly ParameterInfo[] _parameters = _constructor.GetParameters();

		private readonly object[] _parameterValues = new object[_parameters.Length];

		public int Match(object[] givenParameters)
		{
			int num = 0;
			int result = 0;
			for (int i = 0; i != givenParameters.Length; i++)
			{
				TypeInfo typeInfo = givenParameters[i]?.GetType().GetTypeInfo();
				bool flag = false;
				int num2 = num;
				while (!flag && num2 != _parameters.Length)
				{
					if (_parameterValues[num2] == null && _parameters[num2].ParameterType.GetTypeInfo().IsAssignableFrom(typeInfo))
					{
						flag = true;
						_parameterValues[num2] = givenParameters[i];
						if (num == num2)
						{
							num++;
							if (num2 == i)
							{
								result = num2;
							}
						}
					}
					num2++;
				}
				if (!flag)
				{
					return -1;
				}
			}
			return result;
		}

		public object CreateInstance(IServiceProvider provider)
		{
			for (int i = 0; i != _parameters.Length; i++)
			{
				if (_parameterValues[i] != null)
				{
					continue;
				}
				object service = provider.GetService(_parameters[i].ParameterType);
				if (service == null)
				{
					if (!ParameterDefaultValue.TryGetDefaultValue(_parameters[i], out object defaultValue))
					{
						throw new InvalidOperationException($"Unable to resolve service for type '{_parameters[i].ParameterType}' while attempting to activate '{_constructor.DeclaringType}'.");
					}
					_parameterValues[i] = defaultValue;
				}
				else
				{
					_parameterValues[i] = service;
				}
			}
			try
			{
				return _constructor.Invoke(_parameterValues);
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}
	}

	private static readonly MethodInfo GetServiceInfo = GetMethodInfo<Func<IServiceProvider, Type, Type, bool, object>>((IServiceProvider sp, Type t, Type r, bool c) => GetService(sp, t, r, c));

	public static object CreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, params object[] parameters)
	{
		int num = -1;
		bool flag = false;
		ConstructorMatcher constructorMatcher = default(ConstructorMatcher);
		if (!instanceType.GetTypeInfo().IsAbstract)
		{
			ConstructorInfo[] constructors = instanceType.GetConstructors();
			foreach (ConstructorInfo constructorInfo in constructors)
			{
				if (constructorInfo.IsStatic)
				{
					continue;
				}
				ConstructorMatcher constructorMatcher2 = new ConstructorMatcher(constructorInfo);
				bool flag2 = constructorInfo.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), inherit: false);
				int num2 = constructorMatcher2.Match(parameters);
				if (flag2)
				{
					if (flag)
					{
						ThrowMultipleCtorsMarkedWithAttributeException();
					}
					if (num2 == -1)
					{
						ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
					}
				}
				if (flag2 || num < num2)
				{
					num = num2;
					constructorMatcher = constructorMatcher2;
				}
				flag = flag || flag2;
			}
		}
		if (num == -1)
		{
			string message = $"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
			throw new InvalidOperationException(message);
		}
		return constructorMatcher.CreateInstance(provider);
	}

	public static ObjectFactory CreateFactory([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes)
	{
		FindApplicableConstructor(instanceType, argumentTypes, out ConstructorInfo matchingConstructor, out int?[] matchingParameterMap);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(IServiceProvider), "provider");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "argumentArray");
		Expression body = BuildFactoryExpression(matchingConstructor, matchingParameterMap, parameterExpression, parameterExpression2);
		Expression<Func<IServiceProvider, object[], object>> expression = Expression.Lambda<Func<IServiceProvider, object[], object>>(body, new ParameterExpression[2] { parameterExpression, parameterExpression2 });
		Func<IServiceProvider, object[], object> func = expression.Compile();
		return func.Invoke;
	}

	public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider, params object[] parameters)
	{
		return (T)CreateInstance(provider, typeof(T), parameters);
	}

	public static T GetServiceOrCreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider)
	{
		return (T)GetServiceOrCreateInstance(provider, typeof(T));
	}

	public static object GetServiceOrCreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
	{
		return provider.GetService(type) ?? CreateInstance(provider, type);
	}

	private static MethodInfo GetMethodInfo<T>(Expression<T> expr) where T : notnull
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)expr.Body;
		return methodCallExpression.Method;
	}

	private static object GetService(IServiceProvider sp, Type type, Type requiredBy, bool isDefaultParameterRequired)
	{
		object service = sp.GetService(type);
		if (service == null && !isDefaultParameterRequired)
		{
			string message = $"Unable to resolve service for type '{type}' while attempting to activate '{requiredBy}'.";
			throw new InvalidOperationException(message);
		}
		return service;
	}

	private static Expression BuildFactoryExpression(ConstructorInfo constructor, int?[] parameterMap, Expression serviceProvider, Expression factoryArgumentArray)
	{
		ParameterInfo[] parameters = constructor.GetParameters();
		Expression[] array = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type parameterType = parameterInfo.ParameterType;
			object defaultValue;
			bool flag = ParameterDefaultValue.TryGetDefaultValue(parameterInfo, out defaultValue);
			if (parameterMap[i].HasValue)
			{
				array[i] = Expression.ArrayAccess(factoryArgumentArray, Expression.Constant(parameterMap[i]));
			}
			else
			{
				Expression[] arguments = new Expression[4]
				{
					serviceProvider,
					Expression.Constant(parameterType, typeof(Type)),
					Expression.Constant(constructor.DeclaringType, typeof(Type)),
					Expression.Constant(flag)
				};
				array[i] = Expression.Call(GetServiceInfo, arguments);
			}
			if (flag)
			{
				ConstantExpression right = Expression.Constant(defaultValue);
				array[i] = Expression.Coalesce(array[i], right);
			}
			array[i] = Expression.Convert(array[i], parameterType);
		}
		return Expression.New(constructor, array);
	}

	private static void FindApplicableConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, out ConstructorInfo matchingConstructor, out int?[] matchingParameterMap)
	{
		ConstructorInfo matchingConstructor2 = null;
		int?[] parameterMap = null;
		if (!TryFindPreferredConstructor(instanceType, argumentTypes, ref matchingConstructor2, ref parameterMap) && !TryFindMatchingConstructor(instanceType, argumentTypes, ref matchingConstructor2, ref parameterMap))
		{
			string message = $"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
			throw new InvalidOperationException(message);
		}
		matchingConstructor = matchingConstructor2;
		matchingParameterMap = parameterMap;
	}

	private static bool TryFindMatchingConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, [NotNullWhen(true)] ref ConstructorInfo matchingConstructor, [NotNullWhen(true)] ref int?[] parameterMap)
	{
		ConstructorInfo[] constructors = instanceType.GetConstructors();
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (!constructorInfo.IsStatic && TryCreateParameterMap(constructorInfo.GetParameters(), argumentTypes, out int?[] parameterMap2))
			{
				if (matchingConstructor != null)
				{
					throw new InvalidOperationException($"Multiple constructors accepting all given argument types have been found in type '{instanceType}'. There should only be one applicable constructor.");
				}
				matchingConstructor = constructorInfo;
				parameterMap = parameterMap2;
			}
		}
		if (matchingConstructor != null)
		{
			return true;
		}
		return false;
	}

	private static bool TryFindPreferredConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes, [NotNullWhen(true)] ref ConstructorInfo matchingConstructor, [NotNullWhen(true)] ref int?[] parameterMap)
	{
		bool flag = false;
		ConstructorInfo[] constructors = instanceType.GetConstructors();
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (!constructorInfo.IsStatic && constructorInfo.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), inherit: false))
			{
				if (flag)
				{
					ThrowMultipleCtorsMarkedWithAttributeException();
				}
				if (!TryCreateParameterMap(constructorInfo.GetParameters(), argumentTypes, out int?[] parameterMap2))
				{
					ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
				}
				matchingConstructor = constructorInfo;
				parameterMap = parameterMap2;
				flag = true;
			}
		}
		if (matchingConstructor != null)
		{
			return true;
		}
		return false;
	}

	private static bool TryCreateParameterMap(ParameterInfo[] constructorParameters, Type[] argumentTypes, out int?[] parameterMap)
	{
		parameterMap = new int?[constructorParameters.Length];
		for (int i = 0; i < argumentTypes.Length; i++)
		{
			bool flag = false;
			TypeInfo typeInfo = argumentTypes[i].GetTypeInfo();
			for (int j = 0; j < constructorParameters.Length; j++)
			{
				if (!parameterMap[j].HasValue && constructorParameters[j].ParameterType.GetTypeInfo().IsAssignableFrom(typeInfo))
				{
					flag = true;
					parameterMap[j] = i;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static void ThrowMultipleCtorsMarkedWithAttributeException()
	{
		throw new InvalidOperationException("Multiple constructors were marked with ActivatorUtilitiesConstructorAttribute.");
	}

	private static void ThrowMarkedCtorDoesNotTakeAllProvidedArguments()
	{
		throw new InvalidOperationException("Constructor marked with ActivatorUtilitiesConstructorAttribute does not accept all given argument types.");
	}
}
