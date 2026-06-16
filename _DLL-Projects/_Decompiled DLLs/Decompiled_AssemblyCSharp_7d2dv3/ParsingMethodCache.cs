using System;
using System.Collections.Generic;
using System.Reflection;

public class ParsingMethodCache
{
	public class ParsingMethodData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerOrViewType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string attributeName;

		public readonly string DefiningController;

		public readonly Type NativeParseType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiParsingDelegate nativeParserDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<Type, XuiParsingDelegate> sourceTypeDelegates = new Dictionary<Type, XuiParsingDelegate>();

		public ParsingMethodData(Type _controllerOrViewType, string _attributeName, string _definingController, Type _nativeParseType, XuiParsingDelegate _nativeParserDelegate)
		{
			controllerOrViewType = _controllerOrViewType;
			attributeName = _attributeName;
			DefiningController = _definingController;
			NativeParseType = _nativeParseType;
			nativeParserDelegate = _nativeParserDelegate;
		}

		public bool TryGetDelegateForSourceType(Type _sourceType, out XuiParsingDelegate _delegate)
		{
			if (_sourceType == NativeParseType)
			{
				_delegate = nativeParserDelegate;
				return true;
			}
			if (sourceTypeDelegates.TryGetValue(_sourceType, out _delegate))
			{
				return _delegate != null;
			}
			if (NativeParseType == typeof(string))
			{
				_delegate = [PublicizedFrom(EAccessModifier.Internal)] (IXUiElement _instance, object _value) =>
				{
					nativeParserDelegate(_instance, _value.ToString());
				};
				sourceTypeDelegates[_sourceType] = _delegate;
				return true;
			}
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Warning("[XUi] Trying to map from " + _sourceType.FullName + " to " + NativeParseType.FullName + " for attribute '" + attributeName + "' on " + controllerOrViewType.FullName);
			}
			if (Instance.TryGetTypeConverterFromType(NativeParseType, _sourceType, out var typeToTypeConverter))
			{
				_delegate = [PublicizedFrom(EAccessModifier.Internal)] (IXUiElement _instance, object _value) =>
				{
					if (!typeToTypeConverter(_value, out var _parsedValue))
					{
						Log.Error($"[XUi] Can not parse input ('{_value}', type {_value.GetType().FullName}) into target type {NativeParseType.FullName} for attribute '{attributeName}' on {controllerOrViewType.FullName}: Conversion to target type failed. --- hierarchy: {_instance.GetXuiHierarchy()}");
					}
					else
					{
						nativeParserDelegate(_instance, _parsedValue);
					}
				};
				sourceTypeDelegates[_sourceType] = _delegate;
				return true;
			}
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Conversion from source type (" + _sourceType.FullName + ") into target type (" + NativeParseType.FullName + ") requires parsing from string! (Attribute '" + attributeName + "' on " + controllerOrViewType.FullName + ")");
			}
			if (!Instance.TryGetTypeConverterFromString(NativeParseType, out var converter))
			{
				Log.Error("[XUi] Can not parse input type ('" + _sourceType.FullName + "') into target type " + NativeParseType.FullName + " for attribute '" + attributeName + "' on " + controllerOrViewType.FullName + ": No converter for target type");
				_delegate = null;
				sourceTypeDelegates[_sourceType] = _delegate;
				return false;
			}
			_delegate = [PublicizedFrom(EAccessModifier.Internal)] (IXUiElement _instance, object _value) =>
			{
				string inputValue = _value.ToString();
				if (!converter(inputValue, out var _parsedValue))
				{
					Log.Error($"[XUi] Can not parse input ('{_value}', type {_value.GetType().FullName}) into target type {NativeParseType.FullName} for attribute '{attributeName}' on {controllerOrViewType.FullName}: Conversion to target type failed. --- hierarchy: {_instance.GetXuiHierarchy()}");
				}
				else
				{
					nativeParserDelegate(_instance, _parsedValue);
				}
			};
			sourceTypeDelegates[_sourceType] = _delegate;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ParsingMethodTypeCache
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerOrViewType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, ParsingMethodData> methods = new Dictionary<string, ParsingMethodData>(64, StringComparer.Ordinal);

		public ParsingMethodTypeCache(Type _controllerOrViewType)
		{
			controllerOrViewType = _controllerOrViewType;
			initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			RecurseTypeTree(controllerOrViewType);
			[PublicizedFrom(EAccessModifier.Private)]
			void MethodFoundCallback(MethodInfo _method, bool _hasMultiple, XuiXmlAttributeAttribute _parsingAttribute)
			{
				if (tryGetParsingObjectDelegate(_method, out var _func, out var _inputType))
				{
					ParsingMethodData parsingMethodData = new ParsingMethodData(controllerOrViewType, _parsingAttribute.AttributeName, _method.DeclaringType.FullName, _inputType, _func);
					if (!_parsingAttribute.Override && methods.TryGetValue(_parsingAttribute.AttributeName, out var value))
					{
						Log.Warning("[XUi] Child class (" + parsingMethodData.DefiningController + ") overriding parent (" + value.DefiningController + ") parsing definition of '" + _parsingAttribute.AttributeName + "'");
					}
					methods[_parsingAttribute.AttributeName] = parsingMethodData;
				}
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void PropertyFoundCallback(PropertyInfo _property, bool _hasMultiple, XuiXmlAttributeAttribute _xmlParsingConverterAttribute)
			{
				MethodInfo setMethod = _property.GetSetMethod(nonPublic: true);
				if (setMethod == null)
				{
					Log.Error("[XUi] Failed creating parsing property wrapper: Property has no setter (" + _property.DeclaringType.FullName + "." + _property.Name + ")");
				}
				MethodFoundCallback(setMethod, _hasMultiple, _xmlParsingConverterAttribute);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void RecurseTypeTree(Type _type)
			{
				if (!(_type == null))
				{
					RecurseTypeTree(_type.BaseType);
					ReflectionHelpers.GetMethodsWithAttribute<XuiXmlAttributeAttribute>(_type, MethodFoundCallback, _declaredOnly: true, _allowInstance: true, _allowStatic: false);
					ReflectionHelpers.GetPropertiesWithAttribute<XuiXmlAttributeAttribute>(_type, PropertyFoundCallback, _declaredOnly: true, _allowInstance: true, _allowStatic: false);
				}
			}
		}

		public bool TryGetParsingDelegate(string _attributeName, out ParsingMethodData _parsingDelegate)
		{
			if (methods.TryGetValue(_attributeName, out var value))
			{
				_parsingDelegate = value;
				return true;
			}
			_parsingDelegate = null;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool RawConverterFromStringDelegate<T>(string _in, out T _out);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool RawConverterTypeToTypeDelegate<TOutput, TInput>(TInput _in, out TOutput _out);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, ParsingMethodTypeCache> cachePerType = new Dictionary<Type, ParsingMethodTypeCache>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, XuiParsingConverterFromStringDelegate> convertersFromString = new Dictionary<Type, XuiParsingConverterFromStringDelegate>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>> convertersTypeToType = new Dictionary<Type, Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MethodInfo genericEnumTryParseMethod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ParsingMethodCache instance;

	public static ParsingMethodCache Instance => instance ?? (instance = new ParsingMethodCache());

	[PublicizedFrom(EAccessModifier.Private)]
	public ParsingMethodCache()
	{
		genericEnumTryParseMethod = new RawConverterFromStringDelegate<int>(EnumUtils.TryParseIgnoreCase).Method.GetGenericMethodDefinition();
		ReflectionHelpers.FindTypesWithAttribute<XuiXmlAttributeConvertersClassAttribute>(TypeFoundCallback, _allowAbstract: true);
		[PublicizedFrom(EAccessModifier.Private)]
		void TypeFoundCallback(Type _type, bool _hasMultiple, XuiXmlAttributeConvertersClassAttribute _xmlParsingConvertersClassAttribute)
		{
			ReflectionHelpers.GetMethodsWithAttribute<XuiXmlAttributeConverterAttribute>(_type, MethodFoundCallback, _declaredOnly: true, _allowInstance: false);
			[PublicizedFrom(EAccessModifier.Private)]
			void MethodFoundCallback(MethodInfo _method, bool flag, XuiXmlAttributeConverterAttribute _xmlParsingConverterAttribute)
			{
				if (_method.GetParameters().Length >= 1 && _method.GetParameters()[0].ParameterType == typeof(string))
				{
					if (tryGetParsingConverterFromStringDelegate(_method, out var _func, out var _outputType))
					{
						convertersFromString[_outputType] = _func;
					}
				}
				else
				{
					tryGetParsingConverterTypeToTypeDelegate(_method, out var _func2, out var _outputType2, out var _inputType);
					if (!convertersTypeToType.TryGetValue(_outputType2, out var value))
					{
						value = new Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>();
						convertersTypeToType[_outputType2] = value;
					}
					if (!value.TryAdd(_inputType, _func2))
					{
						Log.Warning("[XUi] Input type (" + _inputType.FullName + ") -> output type (" + _outputType2.FullName + ") converter already defined before. New implementation: " + _method.DeclaringType.FullName + "." + _method.Name);
					}
				}
			}
		}
	}

	public bool TryGetParsingDelegate(IXUiElement _target, string _attributeName, out ParsingMethodData _parsingDelegate)
	{
		Type type = _target.GetType();
		if (!cachePerType.TryGetValue(type, out var value))
		{
			value = new ParsingMethodTypeCache(type);
			cachePerType[type] = value;
		}
		return value.TryGetParsingDelegate(_attributeName, out _parsingDelegate);
	}

	public bool TryParseDirect(IXUiElement _target, string _attributeName, string _value)
	{
		if (!TryGetParsingDelegate(_target, _attributeName, out var _parsingDelegate))
		{
			return false;
		}
		if (!_parsingDelegate.TryGetDelegateForSourceType(typeof(string), out var _delegate))
		{
			Log.Warning("[XUi] Found attribute parser but no suitable converter from string input. Attribute: " + _attributeName + ", target type: " + _parsingDelegate.NativeParseType.FullName + " on " + _target.GetType().FullName);
			return true;
		}
		try
		{
			_delegate(_target, _value);
		}
		catch (Exception e)
		{
			Log.Error("[XUi] Can not parse input ('" + _value + "') into target type " + _parsingDelegate.NativeParseType.FullName + " for attribute '" + _attributeName + "' on " + _target.GetType().FullName + ": Conversion to target type failed");
			Log.Exception(e);
		}
		return true;
	}

	public bool TryGetTypeConverterFromString(Type _outputType, out XuiParsingConverterFromStringDelegate _converterDelegate)
	{
		if (convertersFromString.TryGetValue(_outputType, out _converterDelegate))
		{
			return true;
		}
		if (!_outputType.IsEnum)
		{
			return false;
		}
		if (!tryGetParsingConverterFromStringDelegate(genericEnumTryParseMethod.MakeGenericMethod(_outputType), out _converterDelegate, out var _outputType2))
		{
			return false;
		}
		convertersFromString[_outputType2] = _converterDelegate;
		return true;
	}

	public bool TryGetTypeConverterFromType(Type _outputType, Type _inputType, out XuiParsingConverterTypeToTypeDelegate _converterDelegate)
	{
		if (!convertersTypeToType.TryGetValue(_outputType, out var value))
		{
			_converterDelegate = null;
			return false;
		}
		return value.TryGetValue(_inputType, out _converterDelegate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingObjectDelegate(MethodInfo _method, out XuiParsingDelegate _func, out Type _inputType)
	{
		_func = null;
		_inputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(void))
		{
			Log.Error("[XUi] Failed creating parsing method wrapper: Method has return value (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 1)
		{
			Log.Error("[XUi] Failed creating parsing method wrapper: Method has invalid number of parameters, must be 1 (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		_inputType = parameters[0].ParameterType;
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingDelegate>(GetParsingObjectDelegateCore<XUiController, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(_method.DeclaringType, _inputType);
		_func = (XuiParsingDelegate)methodInfo.Invoke(null, new object[1] { _method });
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiParsingDelegate GetParsingObjectDelegateCore<TInstance, TValue>(MethodInfo _methodInner) where TInstance : class
		{
			Action<TInstance, TValue> typedFunc = (Action<TInstance, TValue>)Delegate.CreateDelegate(typeof(Action<TInstance, TValue>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (IXUiElement _instance, object _value) =>
			{
				typedFunc((TInstance)_instance, (TValue)_value);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingConverterFromStringDelegate(MethodInfo _method, out XuiParsingConverterFromStringDelegate _func, out Type _outputType)
	{
		_func = null;
		_outputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(bool))
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method has no bool return value (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 2)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method has invalid number of parameters, must be 2 (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		if (parameters[0].ParameterType != typeof(string))
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method has invalid first parameter, must be string (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		if (!parameters[1].IsOut)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method second parameter must be declared 'out' (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		_outputType = parameters[1].ParameterType.GetElementType();
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingConverterFromStringDelegate>(GetParsingConverterDelegateCore<object>).Method.GetGenericMethodDefinition().MakeGenericMethod(_outputType);
		_func = (XuiParsingConverterFromStringDelegate)methodInfo.Invoke(null, new object[1] { _method });
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiParsingConverterFromStringDelegate GetParsingConverterDelegateCore<TValue>(MethodInfo _methodInner)
		{
			RawConverterFromStringDelegate<TValue> typedFunc = (RawConverterFromStringDelegate<TValue>)Delegate.CreateDelegate(typeof(RawConverterFromStringDelegate<TValue>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (string _inputValue, out object _outputValue) =>
			{
				TValue _out;
				bool result = typedFunc(_inputValue, out _out);
				_outputValue = _out;
				return result;
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingConverterTypeToTypeDelegate(MethodInfo _method, out XuiParsingConverterTypeToTypeDelegate _func, out Type _outputType, out Type _inputType)
	{
		_func = null;
		_outputType = null;
		_inputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(bool))
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method has no bool return value (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 2)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method has invalid number of parameters, must be 2 (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		if (!parameters[1].IsOut)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: Method second parameter must be declared 'out' (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		_inputType = parameters[0].ParameterType;
		_outputType = parameters[1].ParameterType.GetElementType();
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingConverterTypeToTypeDelegate>(GetParsingConverterDelegateCore<object, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(_outputType, _inputType);
		_func = (XuiParsingConverterTypeToTypeDelegate)methodInfo.Invoke(null, new object[1] { _method });
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiParsingConverterTypeToTypeDelegate GetParsingConverterDelegateCore<TOutput, TInput>(MethodInfo _methodInner)
		{
			RawConverterTypeToTypeDelegate<TOutput, TInput> typedFunc = (RawConverterTypeToTypeDelegate<TOutput, TInput>)Delegate.CreateDelegate(typeof(RawConverterTypeToTypeDelegate<TOutput, TInput>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (object _inputValue, out object _outputValue) =>
			{
				TOutput _out;
				bool result = typedFunc((TInput)_inputValue, out _out);
				_outputValue = _out;
				return result;
			};
		}
	}
}
