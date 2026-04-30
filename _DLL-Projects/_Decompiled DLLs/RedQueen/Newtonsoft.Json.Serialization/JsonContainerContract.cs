using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonContainerContract : JsonContract
{
	private JsonContract _itemContract;

	private JsonContract _finalItemContract;

	internal JsonContract ItemContract
	{
		get
		{
			return _itemContract;
		}
		set
		{
			_itemContract = value;
			if (_itemContract != null)
			{
				_finalItemContract = (_itemContract.UnderlyingType.IsSealed() ? _itemContract : null);
			}
			else
			{
				_finalItemContract = null;
			}
		}
	}

	internal JsonContract FinalItemContract => _finalItemContract;

	public JsonConverter ItemConverter { get; set; }

	public bool? ItemIsReference { get; set; }

	public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

	public TypeNameHandling? ItemTypeNameHandling { get; set; }

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	internal JsonContainerContract(Type underlyingType)
		: base(underlyingType)
	{
		JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(underlyingType);
		if (cachedAttribute != null)
		{
			if (cachedAttribute.ItemConverterType != null)
			{
				ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(cachedAttribute.ItemConverterType, cachedAttribute.ItemConverterParameters);
			}
			ItemIsReference = cachedAttribute._itemIsReference;
			ItemReferenceLoopHandling = cachedAttribute._itemReferenceLoopHandling;
			ItemTypeNameHandling = cachedAttribute._itemTypeNameHandling;
		}
	}
}
