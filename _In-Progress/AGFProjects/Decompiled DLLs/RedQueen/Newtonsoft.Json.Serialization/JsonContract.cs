using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal abstract class JsonContract
{
	internal bool IsNullable;

	internal bool IsConvertable;

	internal bool IsEnum;

	internal Type NonNullableUnderlyingType;

	internal ReadType InternalReadType;

	internal JsonContractType ContractType;

	internal bool IsReadOnlyOrFixedSize;

	internal bool IsSealed;

	internal bool IsInstantiable;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<SerializationCallback> _onDeserializedCallbacks;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<SerializationCallback> _onDeserializingCallbacks;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<SerializationCallback> _onSerializedCallbacks;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<SerializationCallback> _onSerializingCallbacks;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<SerializationErrorCallback> _onErrorCallbacks;

	private Type _createdType;

	public Type UnderlyingType { get; }

	public Type CreatedType
	{
		get
		{
			return _createdType;
		}
		set
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			_createdType = value;
			IsSealed = _createdType.IsSealed();
			IsInstantiable = !_createdType.IsInterface() && !_createdType.IsAbstract();
		}
	}

	public bool? IsReference { get; set; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public JsonConverter Converter
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set;
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public JsonConverter InternalConverter
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		internal set;
	}

	public IList<SerializationCallback> OnDeserializedCallbacks
	{
		get
		{
			if (_onDeserializedCallbacks == null)
			{
				_onDeserializedCallbacks = new List<SerializationCallback>();
			}
			return _onDeserializedCallbacks;
		}
	}

	public IList<SerializationCallback> OnDeserializingCallbacks
	{
		get
		{
			if (_onDeserializingCallbacks == null)
			{
				_onDeserializingCallbacks = new List<SerializationCallback>();
			}
			return _onDeserializingCallbacks;
		}
	}

	public IList<SerializationCallback> OnSerializedCallbacks
	{
		get
		{
			if (_onSerializedCallbacks == null)
			{
				_onSerializedCallbacks = new List<SerializationCallback>();
			}
			return _onSerializedCallbacks;
		}
	}

	public IList<SerializationCallback> OnSerializingCallbacks
	{
		get
		{
			if (_onSerializingCallbacks == null)
			{
				_onSerializingCallbacks = new List<SerializationCallback>();
			}
			return _onSerializingCallbacks;
		}
	}

	public IList<SerializationErrorCallback> OnErrorCallbacks
	{
		get
		{
			if (_onErrorCallbacks == null)
			{
				_onErrorCallbacks = new List<SerializationErrorCallback>();
			}
			return _onErrorCallbacks;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public Func<object> DefaultCreator
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public bool DefaultCreatorNonPublic { get; set; }

	internal JsonContract(Type underlyingType)
	{
		ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
		UnderlyingType = underlyingType;
		underlyingType = ReflectionUtils.EnsureNotByRefType(underlyingType);
		IsNullable = ReflectionUtils.IsNullable(underlyingType);
		NonNullableUnderlyingType = ((IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType);
		_createdType = (CreatedType = NonNullableUnderlyingType);
		IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);
		IsEnum = NonNullableUnderlyingType.IsEnum();
		InternalReadType = ReadType.Read;
	}

	internal void InvokeOnSerializing(object o, StreamingContext context)
	{
		if (_onSerializingCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback onSerializingCallback in _onSerializingCallbacks)
		{
			onSerializingCallback(o, context);
		}
	}

	internal void InvokeOnSerialized(object o, StreamingContext context)
	{
		if (_onSerializedCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback onSerializedCallback in _onSerializedCallbacks)
		{
			onSerializedCallback(o, context);
		}
	}

	internal void InvokeOnDeserializing(object o, StreamingContext context)
	{
		if (_onDeserializingCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback onDeserializingCallback in _onDeserializingCallbacks)
		{
			onDeserializingCallback(o, context);
		}
	}

	internal void InvokeOnDeserialized(object o, StreamingContext context)
	{
		if (_onDeserializedCallbacks == null)
		{
			return;
		}
		foreach (SerializationCallback onDeserializedCallback in _onDeserializedCallbacks)
		{
			onDeserializedCallback(o, context);
		}
	}

	internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
	{
		if (_onErrorCallbacks == null)
		{
			return;
		}
		foreach (SerializationErrorCallback onErrorCallback in _onErrorCallbacks)
		{
			onErrorCallback(o, context, errorContext);
		}
	}

	internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
	{
		return delegate(object o, StreamingContext context)
		{
			callbackMethodInfo.Invoke(o, new object[1] { context });
		};
	}

	internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
	{
		return delegate(object o, StreamingContext context, ErrorContext econtext)
		{
			callbackMethodInfo.Invoke(o, new object[2] { context, econtext });
		};
	}
}
