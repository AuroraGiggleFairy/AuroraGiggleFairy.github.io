using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockDamage : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageReceived = "Damage_received";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageType = "DamageType";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damage;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damageReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageTypes damageType = EnumDamageTypes.Piercing;

	public BlockDamage()
	{
		IsCheckCollideWithEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(Block.PropDamage))
		{
			int.TryParse(base.Properties.Values[Block.PropDamage], out damage);
		}
		else
		{
			Log.Error("Block " + GetBlockName() + " is a BlockDamage but does not specify a damage value");
			damage = 0;
		}
		if (base.Properties.Values.ContainsKey(PropDamageReceived))
		{
			int.TryParse(base.Properties.Values[PropDamageReceived], out damageReceived);
		}
		else
		{
			damageReceived = 0;
		}
		base.Properties.ParseEnum(PropDamageType, ref damageType);
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		base.GetCollisionAABB(_blockValue, _x, _y, _z, _distortedY, _result);
		Vector3 vector = new Vector3(0.05f, 0.05f, 0.05f);
		for (int i = 0; i < _result.Count; i++)
		{
			Bounds value = _result[i];
			value.SetMinMax(value.min - vector, value.max + vector);
			_result[i] = value;
		}
	}

	public override IList<Bounds> GetClipBoundsList(BlockValue _blockValue, Vector3 _blockPos)
	{
		Block.staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, (int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z, 0f, Block.staticList_IntersectRayWithBlockList);
		return Block.staticList_IntersectRayWithBlockList;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (!(_targetEntity is EntityAlive))
		{
			return false;
		}
		EntityAlive entityAlive = (EntityAlive)_targetEntity;
		if (entityAlive.IsDead())
		{
			return false;
		}
		DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, damageType, -1);
		damageSourceEntity.AttackingItem = _blockValue.ToItemValue();
		damageSourceEntity.BlockPosition = _blockPos;
		damageSourceEntity.SetIgnoreConsecutiveDamages(_b: true);
		bool flag;
		if (entityAlive is EntityHuman)
		{
			damageSourceEntity.hitTransformName = entityAlive.emodel.GetHitTransform(BodyPrimaryHit.Torso).name;
			flag = _targetEntity.DamageEntity(damageSourceEntity, damage, _criticalHit: false) > 0;
		}
		else
		{
			flag = _targetEntity.DamageEntity(damageSourceEntity, damage, _criticalHit: false) > 0;
		}
		bool bypassMaxDamage = false;
		int num = entityAlive.CalculateBlockDamage(this, damageReceived, out bypassMaxDamage);
		if (MovementFactor != 1f)
		{
			entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, MovementFactor, entityAlive));
		}
		if (flag && num > 0 && !((World)_world).IsWithinTraderArea(_blockPos))
		{
			DamageBlock(_world, _clrIdx, _blockPos, _blockValue, num, (_targetEntity != null) ? _targetEntity.entityId : (-1), null, _bUseHarvestTool: false, bypassMaxDamage);
		}
		return flag;
	}
}
