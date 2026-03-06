using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationTargetType : UAIConsiderationBase
{
	public string[] type;

	public override void Init(Dictionary<string, string> parameters)
	{
		base.Init(parameters);
		if (parameters.ContainsKey("type"))
		{
			type = parameters["type"].Split(',');
			for (int i = 0; i < type.Length; i++)
			{
				type[i] = type[i].Trim();
			}
		}
	}

	public override float GetScore(Context _context, object target)
	{
		for (int i = 0; i < this.type.Length; i++)
		{
			Type type = Type.GetType(this.type[i]);
			if (type.IsAssignableFrom(target.GetType()))
			{
				return 1f;
			}
			if (target.GetType() == typeof(Vector3) && type.IsAssignableFrom(_context.World.GetBlock(new Vector3i((Vector3)target)).Block.GetType()))
			{
				return 1f;
			}
		}
		return 0f;
	}
}
