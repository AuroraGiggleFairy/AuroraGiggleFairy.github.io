using System.Collections.Generic;

namespace DoomInfighting;

public static class Threshold
{
	public const ulong BaseThreshold = 57uL;

	private static readonly Dictionary<int, ulong> Expiry = new Dictionary<int, ulong>();

	public static bool Running(EntityAlive entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (!Expiry.TryGetValue(entity.entityId, out var until))
		{
			return false;
		}
		EntityAlive target = entity.GetAttackTarget();
		if (target == null || !target.IsAlive() || GameTimer.Instance.ticks >= until)
		{
			Expiry.Remove(entity.entityId);
			return false;
		}
		return true;
	}

	public static void Set(EntityAlive entity)
	{
		if (entity != null)
		{
			Expiry[entity.entityId] = GameTimer.Instance.ticks + 57;
		}
	}

	public static void Clear()
	{
		Expiry.Clear();
	}
}
