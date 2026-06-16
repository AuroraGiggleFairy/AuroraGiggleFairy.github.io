using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationTargetHealth : UAIConsiderationBase
{
	public override float GetScore(Context _context, object target)
	{
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
		if (entityAlive != null)
		{
			return (float)entityAlive.Health / (float)entityAlive.GetMaxHealth();
		}
		if (target.GetType() == typeof(Vector3))
		{
			BlockValue block = _context.Self.world.GetBlock(new Vector3i((Vector3)target));
			Block block2 = block.Block;
			return (float)(block2.MaxDamage - block.damage) / (float)block2.MaxDamage;
		}
		return 0f;
	}
}
