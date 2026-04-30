using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class ReflectionAttributeProvider : IAttributeProvider
{
	private readonly object _attributeProvider;

	public ReflectionAttributeProvider(object attributeProvider)
	{
		ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");
		_attributeProvider = attributeProvider;
	}

	public IList<Attribute> GetAttributes(bool inherit)
	{
		return ReflectionUtils.GetAttributes(_attributeProvider, null, inherit);
	}

	public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
	{
		return ReflectionUtils.GetAttributes(_attributeProvider, attributeType, inherit);
	}
}
