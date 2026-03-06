using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationTargetDistance : UAIConsiderationBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float min;

	[PublicizedFrom(EAccessModifier.Private)]
	public float max = 9126f;

	public override void Init(Dictionary<string, string> _parameters)
	{
		base.Init(_parameters);
		if (_parameters.ContainsKey("min"))
		{
			min = StringParsers.ParseFloat(_parameters["min"]);
			min *= min;
		}
		if (_parameters.ContainsKey("max"))
		{
			max = StringParsers.ParseFloat(_parameters["max"]);
			max *= max;
		}
	}

	public override float GetScore(Context _context, object target)
	{
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
		if (entityAlive != null)
		{
			float num = UAIUtils.DistanceSqr(_context.Self.position, entityAlive.position);
			return Mathf.Clamp01(Mathf.Max(0f, num - min) / (max - min));
		}
		if (target.GetType() == typeof(Vector3))
		{
			Vector3 pointB = (Vector3)target;
			float num2 = UAIUtils.DistanceSqr(_context.Self.position, pointB);
			return Mathf.Clamp01(Mathf.Max(0f, num2 - min) / (max - min));
		}
		return 0f;
	}
}
