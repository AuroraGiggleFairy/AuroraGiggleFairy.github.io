using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class CamelCaseNamingStrategy : NamingStrategy
{
	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
	{
		base.ProcessDictionaryKeys = processDictionaryKeys;
		base.OverrideSpecifiedNames = overrideSpecifiedNames;
	}

	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
		: this(processDictionaryKeys, overrideSpecifiedNames)
	{
		base.ProcessExtensionDataNames = processExtensionDataNames;
	}

	public CamelCaseNamingStrategy()
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	protected override string ResolvePropertyName(string name)
	{
		return StringUtils.ToCamelCase(name);
	}
}
