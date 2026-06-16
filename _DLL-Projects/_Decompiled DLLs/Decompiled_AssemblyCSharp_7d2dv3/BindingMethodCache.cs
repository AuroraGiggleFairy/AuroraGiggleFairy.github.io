using System;
using System.Collections.Generic;
using System.Reflection;

public class BindingMethodCache
{
	public class BindingMethodData
	{
		public readonly string DefiningController;

		public readonly XuiBindingDelegate Delegate;

		public BindingMethodData(string _definingController, XuiBindingDelegate _delegate)
		{
			DefiningController = _definingController;
			Delegate = _delegate;
		}
	}

	public class BindingMethodTypeCache
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, BindingMethodData> methods = new Dictionary<string, BindingMethodData>(64, StringComparer.Ordinal);

		public BindingMethodTypeCache(Type _controllerType)
		{
			controllerType = _controllerType;
			initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			RecurseTypeTree(controllerType);
			[PublicizedFrom(EAccessModifier.Private)]
			void MethodFoundCallback(MethodInfo _method, bool _hasMultiple, XuiXmlBindingAttribute _bindingAttribute)
			{
				XuiBindingDelegate _func;
				if (_method.IsStatic)
				{
					Log.Warning("[XUi] XML binding has to be non-static! " + _method.DeclaringType.FullName + "." + _method.Name);
				}
				else if (tryGetBindingObjectDelegate(_method, out _func))
				{
					BindingMethodData bindingMethodData = new BindingMethodData(_method.DeclaringType.FullName, _func);
					if (methods.TryGetValue(_bindingAttribute.BindingName, out var value))
					{
						Log.Warning("[XUi] Child class (" + bindingMethodData.DefiningController + ") overriding parent (" + value.DefiningController + ") binding definition of '" + _bindingAttribute.BindingName + "'");
					}
					methods[_bindingAttribute.BindingName] = bindingMethodData;
				}
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void PropertyFoundCallback(PropertyInfo _property, bool _hasMultiple, XuiXmlBindingAttribute _bindingAttribute)
			{
				MethodInfo getMethod = _property.GetGetMethod(nonPublic: true);
				if (getMethod == null)
				{
					Log.Error("[XUi] Failed creating binding property wrapper: Property has no getter (" + _property.DeclaringType.FullName + "." + _property.Name + ")");
				}
				MethodFoundCallback(getMethod, _hasMultiple, _bindingAttribute);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void RecurseTypeTree(Type _type)
			{
				if (!(_type == null))
				{
					RecurseTypeTree(_type.BaseType);
					ReflectionHelpers.GetMethodsWithAttribute<XuiXmlBindingAttribute>(_type, MethodFoundCallback);
					ReflectionHelpers.GetPropertiesWithAttribute<XuiXmlBindingAttribute>(_type, PropertyFoundCallback);
				}
			}
		}

		public bool TryGetBindingDelegate(string _bindingName, out XuiBindingDelegate _bindingDelegate)
		{
			if (methods.TryGetValue(_bindingName, out var value))
			{
				_bindingDelegate = value.Delegate;
				return true;
			}
			_bindingDelegate = null;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, BindingMethodTypeCache> cachePerType = new Dictionary<Type, BindingMethodTypeCache>(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public static BindingMethodCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxed0 = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxed1 = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedFalse = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedTrue = true;

	public static BindingMethodCache Instance => instance ?? (instance = new BindingMethodCache());

	public bool TryGetBindingDelegate(XUiController _controller, string _bindingName, out XuiBindingDelegate _bindingDelegate)
	{
		Type type = _controller.GetType();
		if (!cachePerType.TryGetValue(type, out var value))
		{
			value = new BindingMethodTypeCache(type);
			cachePerType[type] = value;
		}
		return value.TryGetBindingDelegate(_bindingName, out _bindingDelegate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetBindingObjectDelegate(MethodInfo _method, out XuiBindingDelegate _func)
	{
		_func = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating binding method wrapper: MethodInfo null");
			return false;
		}
		if (_method.GetParameters().Length != 0)
		{
			Log.Error("[XUi] Failed creating binding method wrapper: Method has parameters (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		Type returnType = _method.ReturnType;
		if (returnType == typeof(void))
		{
			Log.Error("[XUi] Failed creating binding method wrapper: Method has no return value (" + _method.DeclaringType.FullName + "." + _method.Name + ")");
			return false;
		}
		MethodInfo methodInfo = ((returnType == typeof(bool)) ? new Func<MethodInfo, XuiBindingDelegate>(GetBindingObjectDelegateCoreBool<XUiController>).Method.GetGenericMethodDefinition().MakeGenericMethod(_method.DeclaringType) : ((!(returnType == typeof(int))) ? new Func<MethodInfo, XuiBindingDelegate>(GetBindingObjectDelegateCoreGeneric<XUiController, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(_method.DeclaringType, returnType) : new Func<MethodInfo, XuiBindingDelegate>(GetBindingObjectDelegateCoreInt<XUiController>).Method.GetGenericMethodDefinition().MakeGenericMethod(_method.DeclaringType)));
		_func = (XuiBindingDelegate)methodInfo.Invoke(null, new object[1] { _method });
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiBindingDelegate GetBindingObjectDelegateCoreBool<TController>(MethodInfo _methodInner) where TController : XUiController
		{
			Func<TController, bool> typedFunc = (Func<TController, bool>)Delegate.CreateDelegate(typeof(Func<TController, bool>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (XUiController _instance) => (!typedFunc((TController)_instance)) ? boxedFalse : boxedTrue;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiBindingDelegate GetBindingObjectDelegateCoreGeneric<TController, TValue>(MethodInfo _methodInner) where TController : XUiController
		{
			Func<TController, TValue> typedFunc = (Func<TController, TValue>)Delegate.CreateDelegate(typeof(Func<TController, TValue>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (XUiController _instance) => typedFunc((TController)_instance);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static XuiBindingDelegate GetBindingObjectDelegateCoreInt<TController>(MethodInfo _methodInner) where TController : XUiController
		{
			Func<TController, int> typedFunc = (Func<TController, int>)Delegate.CreateDelegate(typeof(Func<TController, int>), _methodInner);
			return [PublicizedFrom(EAccessModifier.Internal)] (XUiController _instance) =>
			{
				int num = typedFunc((TController)_instance);
				return num switch
				{
					0 => boxed0, 
					1 => boxed1, 
					_ => num, 
				};
			};
		}
	}
}
