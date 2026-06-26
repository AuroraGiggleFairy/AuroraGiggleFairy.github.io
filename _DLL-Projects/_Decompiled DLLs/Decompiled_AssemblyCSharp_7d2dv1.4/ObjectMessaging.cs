using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public sealed class ObjectMessaging
{
	public enum CacheDisposition
	{
		CacheTypeInfo,
		Uncached
	}

	public sealed class MethodSignature
	{
		public Type[] ArgumentTypes;

		public Type ReturnType;

		[PublicizedFrom(EAccessModifier.Private)]
		public int hash;

		public override int GetHashCode()
		{
			if (hash == 0)
			{
				hash = ReturnType.GetHashCode();
				if (ArgumentTypes != null && ArgumentTypes.Length >= 1)
				{
					for (int i = 0; i < ArgumentTypes.Length; i++)
					{
						hash ^= ArgumentTypes[i].GetHashCode();
					}
				}
			}
			return hash;
		}
	}

	public static ObjectMessaging Instance;

	public const int DYNAMIC_SIGNATURE = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, Dictionary<int, MethodInfo>> typeCache = new Dictionary<Type, Dictionary<int, MethodInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	static ObjectMessaging()
	{
		Instance = new ObjectMessaging();
	}

	public object CheckedSendMessage(Type _returnType, string methodName, MethodSignature _methodSignature, object _target, params object[] _arguments)
	{
		return CheckedSendMessage(_returnType, methodName, _methodSignature, _target, CacheDisposition.CacheTypeInfo, _arguments);
	}

	public object CheckedSendMessage(Type _returnType, string methodName, MethodSignature _methodSignature, object _target, CacheDisposition _cacheDispostion, params object[] _arguments)
	{
		object _returnValue = null;
		SendMessageEx(_returnType, methodName, _methodSignature, _target, _cacheDispostion, out _returnValue, checkedCall: true, _arguments);
		return _returnValue;
	}

	public object SendMessage(Type _returnType, string _methodName, MethodSignature _methodSignature, object _target, params object[] _arguments)
	{
		return SendMessage(_returnType, _methodName, _methodSignature, _target, CacheDisposition.CacheTypeInfo, _arguments);
	}

	public object SendMessage(Type _returnType, string _methodName, MethodSignature _methodSignature, object _target, CacheDisposition _cacheDispostion, params object[] _arguments)
	{
		object _returnValue = null;
		SendMessageEx(_returnType, _methodName, _methodSignature, _target, _cacheDispostion, out _returnValue, checkedCall: false, _arguments);
		return _returnValue;
	}

	public MethodSignature GenerateMethodSignature(Type _returnType, Type[] _types)
	{
		if (_returnType == null)
		{
			_returnType = typeof(void);
		}
		return new MethodSignature
		{
			ArgumentTypes = _types,
			ReturnType = _returnType
		};
	}

	public bool SendMessageEx(Type _returnType, string _methodName, MethodSignature _messageSignature, object _target, CacheDisposition _cacheDispostion, out object _returnValue, bool checkedCall, params object[] _arguments)
	{
		if (_returnType == null)
		{
			_returnType = typeof(void);
		}
		int num = 0;
		Type[] array = null;
		if (_messageSignature == null)
		{
			num = _returnType.GetHashCode();
			for (int i = 0; i < _arguments.Length; i++)
			{
				num ^= _arguments[i].GetType().GetHashCode();
			}
		}
		else
		{
			num = _messageSignature.GetHashCode();
			_returnType = _messageSignature.ReturnType;
			array = _messageSignature.ArgumentTypes;
		}
		Type type = _target.GetType();
		MethodInfo value = null;
		Dictionary<int, MethodInfo> value2 = null;
		if (_cacheDispostion == CacheDisposition.CacheTypeInfo)
		{
			int key = _methodName.GetHashCode() ^ num;
			if (typeCache.TryGetValue(type, out value2))
			{
				if (value2.TryGetValue(key, out value))
				{
					value2 = null;
				}
			}
			else
			{
				value2 = new Dictionary<int, MethodInfo>();
				typeCache[type] = value2;
			}
			if (value == null && value2 != null)
			{
				if (array == null)
				{
					array = new Type[_arguments.Length];
					for (int j = 0; j < _arguments.Length; j++)
					{
						array[j] = _arguments[j].GetType();
					}
				}
				value = (value2[key] = findMethod(_methodName, type, _returnType, array));
			}
		}
		else
		{
			if (array == null)
			{
				array = new Type[_arguments.Length];
				for (int k = 0; k < _arguments.Length; k++)
				{
					array[k] = _arguments[k].GetType();
				}
			}
			value = findMethod(_methodName, type, _returnType, array);
		}
		if (value != null)
		{
			_returnValue = value.Invoke(_target, _arguments);
		}
		else if (checkedCall)
		{
			throw new TargetInvocationException("Method signature '" + buildMethodSignature(_methodName, _returnType, array) + " does not exist in object type '" + type.FullName, null);
		}
		_returnValue = null;
		return false;
	}

	public void FlushCache()
	{
		typeCache.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildMethodSignature(string _methodName, Type _returnType, Type[] _argTypes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_returnType.Name);
		stringBuilder.Append(" ");
		stringBuilder.Append(_methodName);
		stringBuilder.Append("(");
		for (int i = 0; i < _argTypes.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(_argTypes[i].Name);
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MethodInfo findMethod(string _methodName, Type _target, Type _returnType, Type[] args)
	{
		MethodInfo methodInfo = null;
		Type type = _target;
		while (type != typeof(object))
		{
			methodInfo = _target.GetMethod(_methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding, null, args, null);
			if (methodInfo != null)
			{
				break;
			}
			type = type.BaseType;
		}
		if (methodInfo != null && _returnType != methodInfo.ReturnType && !methodInfo.ReturnType.IsSubclassOf(_returnType))
		{
			methodInfo = null;
		}
		return methodInfo;
	}
}
