using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class EnumInfo
{
	public readonly bool IsFlags;

	public readonly ulong[] Values;

	public readonly string[] Names;

	public readonly string[] ResolvedNames;

	public EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
	{
		IsFlags = isFlags;
		Values = values;
		Names = names;
		ResolvedNames = resolvedNames;
	}
}
