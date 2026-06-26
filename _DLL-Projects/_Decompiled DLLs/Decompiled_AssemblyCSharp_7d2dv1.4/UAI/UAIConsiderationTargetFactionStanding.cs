using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationTargetFactionStanding : UAIConsiderationBase
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
			max = 255f;
		}
	}

	public override float GetScore(Context _context, object target)
	{
		if (target is EntityAlive)
		{
			EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
			return (FactionManager.Instance.GetRelationshipValue(_context.Self, targetEntity) - min) / (max - min);
		}
		return 0f;
	}
}
