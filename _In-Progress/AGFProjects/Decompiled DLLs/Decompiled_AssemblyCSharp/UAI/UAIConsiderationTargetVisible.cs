using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationTargetVisible : UAIConsiderationBase
{
	public override float GetScore(Context _context, object target)
	{
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
		if (entityAlive != null)
		{
			return _context.Self.CanEntityBeSeen(entityAlive) ? 1 : 0;
		}
		if (target.GetType() == typeof(Vector3))
		{
			return _context.Self.CanSee((Vector3)target) ? 1 : 0;
		}
		return 0f;
	}
}
