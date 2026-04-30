using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq;

internal class JTokenEqualityComparer : IEqualityComparer<JToken>
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public bool Equals(JToken x, JToken y)
	{
		return JToken.DeepEquals(x, y);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public int GetHashCode(JToken obj)
	{
		return obj?.GetDeepHashCode() ?? 0;
	}
}
