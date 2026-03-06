using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
internal sealed class JsonPropertyAttribute : Attribute
{
	internal NullValueHandling? _nullValueHandling;

	internal DefaultValueHandling? _defaultValueHandling;

	internal ReferenceLoopHandling? _referenceLoopHandling;

	internal ObjectCreationHandling? _objectCreationHandling;

	internal TypeNameHandling? _typeNameHandling;

	internal bool? _isReference;

	internal int? _order;

	internal Required? _required;

	internal bool? _itemIsReference;

	internal ReferenceLoopHandling? _itemReferenceLoopHandling;

	internal TypeNameHandling? _itemTypeNameHandling;

	public Type ItemConverterType { get; set; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public object[] ItemConverterParameters
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public Type NamingStrategyType { get; set; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public object[] NamingStrategyParameters
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public NullValueHandling NullValueHandling
	{
		get
		{
			return _nullValueHandling.GetValueOrDefault();
		}
		set
		{
			_nullValueHandling = value;
		}
	}

	public DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return _defaultValueHandling.GetValueOrDefault();
		}
		set
		{
			_defaultValueHandling = value;
		}
	}

	public ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return _referenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			_referenceLoopHandling = value;
		}
	}

	public ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return _objectCreationHandling.GetValueOrDefault();
		}
		set
		{
			_objectCreationHandling = value;
		}
	}

	public TypeNameHandling TypeNameHandling
	{
		get
		{
			return _typeNameHandling.GetValueOrDefault();
		}
		set
		{
			_typeNameHandling = value;
		}
	}

	public bool IsReference
	{
		get
		{
			return _isReference == true;
		}
		set
		{
			_isReference = value;
		}
	}

	public int Order
	{
		get
		{
			return _order.GetValueOrDefault();
		}
		set
		{
			_order = value;
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

	public string PropertyName { get; set; }

	public ReferenceLoopHandling ItemReferenceLoopHandling
	{
		get
		{
			return _itemReferenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			_itemReferenceLoopHandling = value;
		}
	}

	public TypeNameHandling ItemTypeNameHandling
	{
		get
		{
			return _itemTypeNameHandling.GetValueOrDefault();
		}
		set
		{
			_itemTypeNameHandling = value;
		}
	}

	public bool ItemIsReference
	{
		get
		{
			return _itemIsReference == true;
		}
		set
		{
			_itemIsReference = value;
		}
	}

	public JsonPropertyAttribute()
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public JsonPropertyAttribute(string propertyName)
	{
		PropertyName = propertyName;
	}
}
