using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonProperty
{
	internal Required? _required;

	internal bool _hasExplicitDefaultValue;

	private object _defaultValue;

	private bool _hasGeneratedDefaultValue;

	private string _propertyName;

	internal bool _skipPropertyNameEscape;

	private Type _propertyType;

	internal JsonContract PropertyContract { get; set; }

	public string PropertyName
	{
		get
		{
			return _propertyName;
		}
		set
		{
			_propertyName = value;
			_skipPropertyNameEscape = !JavaScriptUtils.ShouldEscapeJavaScriptString(_propertyName, JavaScriptUtils.HtmlCharEscapeFlags);
		}
	}

	public Type DeclaringType { get; set; }

	public int? Order { get; set; }

	public string UnderlyingName { get; set; }

	public IValueProvider ValueProvider { get; set; }

	public IAttributeProvider AttributeProvider { get; set; }

	public Type PropertyType
	{
		get
		{
			return _propertyType;
		}
		set
		{
			if (_propertyType != value)
			{
				_propertyType = value;
				_hasGeneratedDefaultValue = false;
			}
		}
	}

	public JsonConverter Converter { get; set; }

	[Obsolete("MemberConverter is obsolete. Use Converter instead.")]
	public JsonConverter MemberConverter
	{
		get
		{
			return Converter;
		}
		set
		{
			Converter = value;
		}
	}

	public bool Ignored { get; set; }

	public bool Readable { get; set; }

	public bool Writable { get; set; }

	public bool HasMemberAttribute { get; set; }

	public object DefaultValue
	{
		get
		{
			if (!_hasExplicitDefaultValue)
			{
				return null;
			}
			return _defaultValue;
		}
		set
		{
			_hasExplicitDefaultValue = true;
			_defaultValue = value;
		}
	}

	public Required Required
	{
		get
		{
			return _required.GetValueOrDefault();
		}
		set
		{
			_required = value;
		}
	}

	public bool IsRequiredSpecified => _required.HasValue;

	public bool? IsReference { get; set; }

	public NullValueHandling? NullValueHandling { get; set; }

	public DefaultValueHandling? DefaultValueHandling { get; set; }

	public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

	public ObjectCreationHandling? ObjectCreationHandling { get; set; }

	public TypeNameHandling? TypeNameHandling { get; set; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public Predicate<object> ShouldSerialize
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public Predicate<object> ShouldDeserialize
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public Predicate<object> GetIsSpecified
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
	public Action<object, object> SetIsSpecified
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
		set;
	}

	public JsonConverter ItemConverter { get; set; }

	public bool? ItemIsReference { get; set; }

	public TypeNameHandling? ItemTypeNameHandling { get; set; }

	public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

	internal object GetResolvedDefaultValue()
	{
		if (_propertyType == null)
		{
			return null;
		}
		if (!_hasExplicitDefaultValue && !_hasGeneratedDefaultValue)
		{
			_defaultValue = ReflectionUtils.GetDefaultValue(_propertyType);
			_hasGeneratedDefaultValue = true;
		}
		return _defaultValue;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public override string ToString()
	{
		return PropertyName ?? string.Empty;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	internal void WritePropertyName(JsonWriter writer)
	{
		string propertyName = PropertyName;
		if (_skipPropertyNameEscape)
		{
			writer.WritePropertyName(propertyName, escape: false);
		}
		else
		{
			writer.WritePropertyName(propertyName);
		}
	}
}
