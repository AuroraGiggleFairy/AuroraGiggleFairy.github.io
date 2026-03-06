using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTrapDoor : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerDelay = "TriggerDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerSound = "TriggerSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public float TriggerDelay = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string TriggerSound = "trapdoor_trigger";

	public override bool AllowBlockTriggers => true;

	public BlockTrapDoor()
	{
		IsCheckCollideWithEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(PropTriggerDelay))
		{
			TriggerDelay = StringParsers.ParseFloat(base.Properties.Values[PropTriggerDelay]);
		}
		if (base.Properties.Values.ContainsKey(PropTriggerSound))
		{
			TriggerSound = base.Properties.Values[PropTriggerSound];
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		cmds[0].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: true, _showTriggeredBy: true);
		return true;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (_targetEntity as EntityAlive == null)
		{
			return false;
		}
		EntityAlive entityAlive = (EntityAlive)_targetEntity;
		if (entityAlive.IsDead())
		{
			return false;
		}
		if (_blockValue.meta == 1)
		{
			return false;
		}
		Vector3 vector = _blockPos.ToVector3() + new Vector3(0.5f, 0f, 0.5f);
		vector.y = 0f;
		Vector3 position = entityAlive.position;
		position.y = 0f;
		float num = Utils.FastAbs(vector.x - position.x);
		float num2 = Utils.FastAbs(vector.z - position.z);
		float num3 = ((_blockValue.Block != null && _blockValue.Block.isMultiBlock) ? ((float)_blockValue.Block.multiBlockPos.dim.x / 2f * 0.45f) : 0.45f);
		if (num > num3 || num2 > num3)
		{
			return false;
		}
		_blockValue.meta = 1;
		_world.SetBlockRPC(_blockPos, _blockValue);
		float value = EffectManager.GetValue(PassiveEffects.TrapDoorTriggerDelay, null, TriggerDelay, _targetEntity as EntityAlive);
		if (value > 0f)
		{
			GameManager.Instance.PlaySoundAtPositionServer(_blockPos.ToVector3(), TriggerSound, AudioRolloffMode.Linear, 5, _targetEntity.entityId);
			GameManager.Instance.StartCoroutine(damageBlock(value, _world, _clrIdx, _blockPos, _blockValue, _targetEntity as EntityPlayer, _targetEntity.entityId));
		}
		return true;
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (((World)_world).GetEntity(_entityId) is EntityPlayer player)
		{
			HandleTrigger(player, (World)_world, _clrIdx, _blockPos, _blockValue);
		}
		return base.OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityId, _bUseHarvestTool);
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		if (((World)_world).GetEntity(_playerThatStartedExpl) is EntityPlayer player)
		{
			HandleTrigger(player, (World)_world, _clrIdx, _blockPos, _blockValue);
		}
		return base.OnBlockDestroyedByExplosion(_world, _clrIdx, _blockPos, _blockValue, _playerThatStartedExpl);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator damageBlock(float time, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayer _player, int _entityId)
	{
		yield return new WaitForSeconds(time);
		if (_player != null)
		{
			HandleTrigger(_player, (World)_world, _clrIdx, _blockPos, _blockValue);
		}
		DamageBlock(_world, _clrIdx, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, _entityId);
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		DamageBlock(_world, _cIdx, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1);
	}
}
