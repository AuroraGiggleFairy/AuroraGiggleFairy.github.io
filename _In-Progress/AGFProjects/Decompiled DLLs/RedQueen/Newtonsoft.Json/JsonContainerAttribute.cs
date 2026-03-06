using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal abstract class JsonContainerAttribute : Attribute
{
	internal bool? _isReference;

	internal bool? _itemIsReference;

	internal ReferenceLoopHandling? _itemReferenceLoopHandling;

	internal TypeNameHandling? _itemTypeNameHandling;

	private Type _namingStrategyType;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private object[] _namingStrategyParameters;

	public string Id { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }

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

	public Type NamingStrategyType
	{
		get
		{
			return _namingStrategyType;
		}
		set
		{
			_namingStrategyType = value;
			NamingStrategyInstance = null;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public object[] NamingStrategyParameters
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return _namingStrategyParameters;
		}
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set
		{
			_namingStrategyParameters = value;
			NamingStrategyInstance = null;
		}
	}

	internal NamingStrategy NamingStrategyInstance { get; set; }

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

	protected JsonContainerAttribute()
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	protected JsonContainerAttribute(string id)
	{
		Id = id;
	}
}
