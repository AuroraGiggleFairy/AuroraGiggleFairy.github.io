using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockMine : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerDelay = "TriggerDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerSound = "TriggerSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNoImmunity = "NoImmunity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public ExplosionData explosion;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TriggerDelay = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string TriggerSound = "landmine_trigger";

	[PublicizedFrom(EAccessModifier.Private)]
	public float BaseEntityDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NoImmunity;

	public ExplosionData Explosion => explosion;

	public override void Init()
	{
		base.Init();
		explosion = new ExplosionData(base.Properties);
		BaseEntityDamage = explosion.EntityDamage;
		if (base.Properties.Values.ContainsKey(PropTriggerDelay))
		{
			TriggerDelay = StringParsers.ParseFloat(base.Properties.Values[PropTriggerDelay]);
		}
		if (base.Properties.Values.ContainsKey(PropTriggerSound))
		{
			TriggerSound = base.Properties.Values[PropTriggerSound];
		}
		base.Properties.ParseBool(PropNoImmunity, ref NoImmunity);
	}

	public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
	{
		if (!NoImmunity && EffectManager.GetValue(PassiveEffects.LandMineImmunity, null, 0f, entity as EntityAlive) != 0f)
		{
			return;
		}
		if (entity as EntityPlayer != null)
		{
			if ((entity as EntityPlayer).IsSpectator)
			{
				return;
			}
			GameManager.Instance.PlaySoundAtPositionServer(new Vector3(_x, _y, _z), TriggerSound, AudioRolloffMode.Linear, 5, entity.entityId);
		}
		float num = TriggerDelay;
		if (entity as EntityAlive != null)
		{
			num = EffectManager.GetValue(PassiveEffects.LandMineTriggerDelay, null, TriggerDelay, entity as EntityAlive);
		}
		explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, BaseEntityDamage, entity as EntityAlive);
		_world.GetWBT().AddScheduledBlockUpdate(new Vector3i(_x, _y, _z), blockID, (ulong)(num * 20f));
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_damagePoints >= 0)
		{
			float num = Utils.FastClamp(_damagePoints, 1, _blockValue.Block.MaxDamage - 1);
			if (_world.GetGameRandom().RandomFloat <= num / (float)_blockValue.Block.MaxDamage)
			{
				explode(_world, _bvRef, _entityIdThatDamaged);
			}
		}
		else
		{
			base.OnBlockDamaged(_world, _bvRef, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
		}
		return _blockValue.damage;
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _playerIdx)
	{
		if (_world.GetGameRandom().RandomFloat < 0.33f)
		{
			explode(_world, _bvRef, _playerIdx);
			return DestroyedResult.Remove;
		}
		return DestroyedResult.Keep;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(WorldBase _world, BlockValueRef _bvRef, int _entityId)
	{
		Vector3 worldPos = _bvRef.ToVector3Center(_world);
		_world.GetGameManager().ExplosionServer(worldPos, World.worldToBlockPos(worldPos), Quaternion.identity, explosion, -1, 0.1f, _bRemoveBlockAtExplPosition: true);
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue blockDef, BlockFace face)
	{
		return false;
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		explode(_world, _blockPos, -1);
		return true;
	}

	public void TriggerMine(Entity _entity, WorldBase _world, Vector3i _blockPos, bool useTrigger)
	{
		if (useTrigger)
		{
			float num = TriggerDelay;
			if (_entity is EntityAlive entity)
			{
				GameManager.Instance.PlaySoundAtPositionServer(_blockPos, TriggerSound, AudioRolloffMode.Linear, 5);
				num = EffectManager.GetValue(PassiveEffects.LandMineTriggerDelay, null, TriggerDelay, entity);
				explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, BaseEntityDamage, entity);
			}
			else
			{
				explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, BaseEntityDamage);
			}
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, blockID, (ulong)(num * 20f));
		}
		else
		{
			explode(_world, _blockPos, -1);
		}
	}
}
