using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal abstract class NamingStrategy
{
	public bool ProcessDictionaryKeys { get; set; }

	public bool ProcessExtensionDataNames { get; set; }

	public bool OverrideSpecifiedNames { get; set; }

	public virtual string GetPropertyName(string name, bool hasSpecifiedName)
	{
		if (hasSpecifiedName && !OverrideSpecifiedNames)
		{
			return name;
		}
		return ResolvePropertyName(name);
	}

	public virtual string GetExtensionDataName(string name)
	{
		if (!ProcessExtensionDataNames)
		{
			return name;
		}
		return ResolvePropertyName(name);
	}

	public virtual string GetDictionaryKey(string key)
	{
		if (!ProcessDictionaryKeys)
		{
			return key;
		}
		return ResolvePropertyName(key);
	}

	protected abstract string ResolvePropertyName(string name);

	public override int GetHashCode()
	{
		return (((((GetType().GetHashCode() * 397) ^ ProcessDictionaryKeys.GetHashCode()) * 397) ^ ProcessExtensionDataNames.GetHashCode()) * 397) ^ OverrideSpecifiedNames.GetHashCode();
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public override bool Equals(object obj)
	{
		return Equals(obj as NamingStrategy);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	protected bool Equals(NamingStrategy other)
	{
		if (other == null)
		{
			return false;
		}
		if (GetType() == other.GetType() && ProcessDictionaryKeys == other.ProcessDictionaryKeys && ProcessExtensionDataNames == other.ProcessExtensionDataNames)
		{
			return OverrideSpecifiedNames == other.OverrideSpecifiedNames;
		}
		return false;
	}
}
