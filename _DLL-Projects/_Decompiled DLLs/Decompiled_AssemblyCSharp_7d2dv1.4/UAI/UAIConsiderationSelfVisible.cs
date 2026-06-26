using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationSelfVisible : UAIConsiderationBase
{
	public override float GetScore(Context _context, object target)
	{
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
		if (entityAlive != null)
		{
			float seeDistance = _context.Self.GetSeeDistance();
			seeDistance *= seeDistance;
			float num = 1f - UAIUtils.DistanceSqr(_context.Self.getHeadPosition(), entityAlive.getHeadPosition()) / seeDistance;
			return (float)(entityAlive.CanEntityBeSeen(_context.Self) ? 1 : 0) * num;
		}
		return 0f;
	}
}
