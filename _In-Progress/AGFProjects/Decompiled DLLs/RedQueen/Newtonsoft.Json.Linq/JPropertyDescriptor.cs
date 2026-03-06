using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JPropertyDescriptor : PropertyDescriptor
{
	public override Type ComponentType => typeof(JObject);

	public override bool IsReadOnly => false;

	public override Type PropertyType => typeof(object);

	protected override int NameHashCode => base.NameHashCode;

	public JPropertyDescriptor(string name)
		: base(name, null)
	{
	}

	private static JObject CastInstance(object instance)
	{
		return (JObject)instance;
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public override object GetValue(object component)
	{
		return (component as JObject)?[Name];
	}

	public override void ResetValue(object component)
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public override void SetValue(object component, object value)
	{
		if (component is JObject jObject)
		{
			JToken value2 = (value as JToken) ?? new JValue(value);
			jObject[Name] = value2;
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		return false;
	}
}
