using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIBreakBlock : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBoostPerAlly = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageBoostPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> allies = new List<Entity>();

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 8;
		executeDelay = 0.15f;
	}

	public override bool CanExecute()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		if (moveHelper.BlockedTime < 0.35f || !moveHelper.CanBreakBlocks)
		{
			return false;
		}
		if (theEntity.Jumping && !moveHelper.IsDestroyArea)
		{
			return false;
		}
		int num = ((theEntity.crouchType == 0 && theEntity.physicsHeight >= 1f) ? 7 : 5);
		if ((moveHelper.BlockedFlags & num) > 0)
		{
			Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
			if (theEntity.world.GetBlock(blockPos).isair)
			{
				return false;
			}
			float num2 = moveHelper.CalcBlockedDistanceSq();
			float num3 = theEntity.m_characterController.GetRadius() + 0.7f;
			if (num2 <= num3 * num3)
			{
				return true;
			}
		}
		return false;
	}

	public override void Start()
	{
		attackDelay = 1;
		Vector3i blockPos = theEntity.moveHelper.HitInfo.hit.blockPos;
		Block block = theEntity.world.GetBlock(blockPos).Block;
		if (block.HasTag(BlockTags.Door) || block.HasTag(BlockTags.ClosetDoor))
		{
			theEntity.IsBreakingDoors = true;
		}
	}

	public override bool Continue()
	{
		if (theEntity.onGround || theEntity.IsInElevator())
		{
			return CanExecute();
		}
		return false;
	}

	public override void Update()
	{
		_ = theEntity.moveHelper;
		if (attackDelay > 0)
		{
			attackDelay--;
		}
		if (attackDelay <= 0)
		{
			AttackBlock();
		}
	}

	public override void Reset()
	{
		theEntity.IsBreakingBlocks = false;
		theEntity.IsBreakingDoors = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackBlock()
	{
		theEntity.SetLookPosition(Vector3.zero);
		if (!(theEntity.inventory.holdingItemData.actionData[0] is ItemActionAttackData itemActionAttackData))
		{
			return;
		}
		damageBoostPercent = 0f;
		if (theEntity is EntityZombie)
		{
			Bounds bb = new Bounds(theEntity.position, new Vector3(1.7f, 1.5f, 1.7f));
			theEntity.world.GetEntitiesInBounds(typeof(EntityZombie), bb, allies);
			for (int num = allies.Count - 1; num >= 0; num--)
			{
				if ((EntityZombie)allies[num] != theEntity)
				{
					damageBoostPercent += 0.2f;
				}
			}
			allies.Clear();
		}
		if (theEntity.Attack(_isReleased: false))
		{
			theEntity.IsBreakingBlocks = true;
			float num2 = 0.25f + base.RandomFloat * 0.8f;
			if (theEntity.moveHelper.IsUnreachableAbove)
			{
				num2 *= 0.5f;
			}
			attackDelay = (int)((num2 + 0.75f) * 20f);
			itemActionAttackData.hitDelegate = GetHitInfo;
			theEntity.Attack(_isReleased: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetHitInfo(out float damageScale)
	{
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		damageScale = moveHelper.DamageScale + damageBoostPercent;
		return moveHelper.HitInfo;
	}
}
