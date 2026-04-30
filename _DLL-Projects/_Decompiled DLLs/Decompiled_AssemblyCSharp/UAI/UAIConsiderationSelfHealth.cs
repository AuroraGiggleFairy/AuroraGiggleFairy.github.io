using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationSelfHealth : UAIConsiderationBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float min;

	[PublicizedFrom(EAccessModifier.Private)]
	public float max;

	public override void Init(Dictionary<string, string> parameters)
	{
		base.Init(parameters);
		if (parameters.ContainsKey("min"))
		{
			min = StringParsers.ParseFloat(parameters["min"]);
		}
		else
		{
			min = 0f;
		}
		if (parameters.ContainsKey("max"))
		{
			max = StringParsers.ParseFloat(parameters["max"]);
		}
		else
		{
			max = float.NaN;
		}
	}

	public override float GetScore(Context _context, object _target)
	{
		if (float.IsNaN(max))
		{
			max = _context.Self.GetMaxHealth();
		}
		return ((float)_context.Self.Health - min) / (max - min);
	}
}
