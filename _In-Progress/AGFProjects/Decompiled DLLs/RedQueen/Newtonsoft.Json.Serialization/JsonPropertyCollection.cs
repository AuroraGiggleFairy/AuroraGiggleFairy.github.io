using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1, 1 })]
internal class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
{
	private readonly Type _type;

	private readonly List<JsonProperty> _list;

	public JsonPropertyCollection(Type type)
		: base((IEqualityComparer<string>)StringComparer.Ordinal)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		_type = type;
		_list = (List<JsonProperty>)base.Items;
	}

	protected override string GetKeyForItem(JsonProperty item)
	{
		return item.PropertyName;
	}

	public void AddProperty(JsonProperty property)
	{
		if (Contains(property.PropertyName))
		{
			if (property.Ignored)
			{
				return;
			}
			JsonProperty jsonProperty = base[property.PropertyName];
			bool flag = true;
			if (jsonProperty.Ignored)
			{
				Remove(jsonProperty);
				flag = false;
			}
			else if (property.DeclaringType != null && jsonProperty.DeclaringType != null)
			{
				if (property.DeclaringType.IsSubclassOf(jsonProperty.DeclaringType) || (jsonProperty.DeclaringType.IsInterface() && property.DeclaringType.ImplementInterface(jsonProperty.DeclaringType)))
				{
					Remove(jsonProperty);
					flag = false;
				}
				if (jsonProperty.DeclaringType.IsSubclassOf(property.DeclaringType) || (property.DeclaringType.IsInterface() && jsonProperty.DeclaringType.ImplementInterface(property.DeclaringType)) || (_type.ImplementInterface(jsonProperty.DeclaringType) && _type.ImplementInterface(property.DeclaringType)))
				{
					return;
				}
			}
			if (flag)
			{
				throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _type));
			}
		}
		Add(property);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public JsonProperty GetClosestMatchProperty(string propertyName)
	{
		JsonProperty property = GetProperty(propertyName, StringComparison.Ordinal);
		if (property == null)
		{
			property = GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);
		}
		return property;
	}

	private bool TryGetProperty(string key, [_003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhen(true)][_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out JsonProperty item)
	{
		if (base.Dictionary == null)
		{
			item = null;
			return false;
		}
		return base.Dictionary.TryGetValue(key, out item);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public JsonProperty GetProperty(string propertyName, StringComparison comparisonType)
	{
		if (comparisonType == StringComparison.Ordinal)
		{
			if (TryGetProperty(propertyName, out var item))
			{
				return item;
			}
			return null;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			JsonProperty jsonProperty = _list[i];
			if (string.Equals(propertyName, jsonProperty.PropertyName, comparisonType))
			{
				return jsonProperty;
			}
		}
		return null;
	}
}
