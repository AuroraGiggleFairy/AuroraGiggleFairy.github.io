using UnityEngine.Scripting;

[Preserve]
public abstract class EAITarget : EAIBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxXZDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNeedToSee;

	[PublicizedFrom(EAccessModifier.Private)]
	public int seeCounter;

	public void Init(EntityAlive _theEntity, float _maxXZDistance, bool _bNeedToSee)
	{
		base.Init(_theEntity);
		seeCounter = 0;
		maxXZDistance = _maxXZDistance;
		bNeedToSee = _bNeedToSee;
	}

	public override void Start()
	{
		seeCounter = 0;
	}

	public override bool Continue()
	{
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if (attackTarget == null)
		{
			return false;
		}
		if (!attackTarget.IsAlive())
		{
			return false;
		}
		if (maxXZDistance > 0f && theEntity.GetDistanceSq(attackTarget) > maxXZDistance * maxXZDistance)
		{
			return false;
		}
		if (bNeedToSee)
		{
			if (!theEntity.CanSee(attackTarget))
			{
				if (++seeCounter > 600)
				{
					return false;
				}
			}
			else
			{
				seeCounter = 0;
			}
		}
		return true;
	}

	public override void Reset()
	{
		theEntity.SetAttackTarget(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool check(EntityAlive _e)
	{
		if (_e == null)
		{
			return false;
		}
		if (_e == theEntity)
		{
			return false;
		}
		if (!_e.IsAlive())
		{
			return false;
		}
		if (_e.IsIgnoredByAI())
		{
			return false;
		}
		Vector3i vector3i = World.worldToBlockPos(_e.position);
		if (!theEntity.isWithinHomeDistance(vector3i.x, vector3i.y, vector3i.z))
		{
			return false;
		}
		if (bNeedToSee && !theEntity.CanSee(_e))
		{
			return false;
		}
		EntityPlayer entityPlayer = _e as EntityPlayer;
		if (entityPlayer != null && !theEntity.CanSeeStealth(manager.GetSeeDistance(entityPlayer), entityPlayer.Stealth.lightLevel))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EAITarget()
	{
	}
}
