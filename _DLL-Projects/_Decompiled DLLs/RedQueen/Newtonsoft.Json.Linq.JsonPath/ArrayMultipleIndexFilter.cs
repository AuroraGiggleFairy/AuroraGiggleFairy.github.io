using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class ArrayMultipleIndexFilter : PathFilter
{
	internal List<int> Indexes;

	public ArrayMultipleIndexFilter(List<int> indexes)
	{
		Indexes = indexes;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		foreach (JToken t in current)
		{
			foreach (int index in Indexes)
			{
				JToken tokenIndex = PathFilter.GetTokenIndex(t, settings, index);
				if (tokenIndex != null)
				{
					yield return tokenIndex;
				}
			}
		}
	}
}
