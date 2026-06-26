using System.Collections.Generic;
using UnityEngine;

public class EntitySeeCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive theEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> positiveCache = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> negativeCache = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksSinceLastClear;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeSeenAPlayer;

	public EntitySeeCache(EntityAlive _e)
	{
		theEntity = _e;
	}

	public bool CanSee(Entity _e)
	{
		if (_e == null)
		{
			return false;
		}
		if (positiveCache.Contains(_e.entityId))
		{
			return true;
		}
		if (negativeCache.Contains(_e.entityId))
		{
			return false;
		}
		bool num = theEntity.CanEntityBeSeen(_e);
		if (num)
		{
			positiveCache.Add(_e.entityId);
			if (_e.IsClientControlled())
			{
				lastTimeSeenAPlayer = Time.time;
				return num;
			}
		}
		else
		{
			negativeCache.Add(_e.entityId);
		}
		return num;
	}

	public float GetLastTimePlayerSeen()
	{
		return lastTimeSeenAPlayer;
	}

	public void SetLastTimePlayerSeen()
	{
		lastTimeSeenAPlayer = Time.time;
	}

	public void SetCanSee(Entity _e)
	{
		positiveCache.Add(_e.entityId);
	}

	public void Clear()
	{
		positiveCache.Clear();
		negativeCache.Clear();
	}

	public void ClearIfExpired()
	{
		if (++ticksSinceLastClear >= 30)
		{
			ticksSinceLastClear = 0;
			Clear();
		}
	}
}
