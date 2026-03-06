using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockActivateSwitch : Block
{
	public const int cMetaPowered = 1;

	public const int cMetaOn = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool singleUse;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activateSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: true),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSingleUse = "SingleUse";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateSound = "ActivateSound";

	public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		base.Properties.ParseBool(PropSingleUse, ref singleUse);
		base.Properties.ParseString(PropActivateSound, ref activateSound);
	}

	public override void LateInit()
	{
		base.LateInit();
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		Refresh(_world, _chunk, _clrIdx, _blockPos, _newBlockValue);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		Refresh(_world, null, _cIdx, _blockPos, _blockValue);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockUtilityNavIcon.RemoveNavObject(_blockPos);
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockUtilityNavIcon.RemoveNavObject(_blockPos);
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsEditor())
		{
			if (singleUse && (_blockValue.meta & 2) != 0)
			{
				return "";
			}
			if ((_blockValue.meta & 1) == 0)
			{
				return "";
			}
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		BlockTrigger blockTrigger = _chunk.GetBlockTrigger(World.toBlock(_blockPos));
		if (blockTrigger != null)
		{
			bool flag = blockTrigger.HasAnyTriggeredBy();
			_blockValue.meta = (byte)((uint)(_blockValue.meta & -2) | ((!flag) ? 1u : 0u));
			_world.SetBlockRPC(_chunk.ClrIdx, _blockPos, _blockValue);
		}
		Refresh(_world, _chunk, _chunk.ClrIdx, _blockPos, _blockValue);
	}

	public override void OnTriggerAddedFromPrefab(BlockTrigger _trigger, Vector3i _blockPos, BlockValue _blockValue, FastTags<TagGroup.Global> _questTags)
	{
		if (!GameManager.Instance.World.IsEditor())
		{
			World world = GameManager.Instance.World;
			base.OnTriggerAddedFromPrefab(_trigger, _blockPos, _blockValue, _questTags);
			bool flag = _trigger.HasAnyTriggeredBy();
			_blockValue.meta = (byte)((uint)(_blockValue.meta & -2) | ((!flag) ? 1u : 0u));
			world.SetBlock(_trigger.Chunk.ClrIdx, _trigger.ToWorldPos(), _blockValue, bNotify: true, updateLight: false);
			Refresh(GameManager.Instance.World, _trigger.Chunk, _trigger.Chunk.ClrIdx, _trigger.LocalChunkPos, _blockValue);
		}
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!_world.IsEditor())
		{
			if (singleUse && (_blockValue.meta & 2) != 0)
			{
				return false;
			}
			if ((_blockValue.meta & 1) == 0)
			{
				return false;
			}
			if (_player.prefab == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return false;
			}
		}
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _clrIdx, _blockPos, _showTriggers: true, _showTriggeredBy: true);
			}
		}
		else if (!_world.IsEditor())
		{
			bool flag = (_blockValue.meta & 2) != 0;
			if ((_blockValue.meta & 1) != 0 && (!flag || !singleUse))
			{
				HandleTrigger(_player, (World)_world, _clrIdx, _blockPos, _blockValue);
				Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, activateSound);
				flag = !flag;
				_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
				_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
				Refresh(_world, null, _clrIdx, _blockPos, _blockValue);
			}
			return true;
		}
		return false;
	}

	public override void OnTriggerRefresh(BlockTrigger _trigger, BlockValue _bv, FastTags<TagGroup.Global> questTag)
	{
		GameManager gm = GameManager.Instance;
		gm.StartCoroutine(gm.World.triggerManager.CheckPowerState(_trigger, questTag, [PublicizedFrom(EAccessModifier.Internal)] (bool powered) =>
		{
			_bv.meta = (byte)((_bv.meta & -2) | (powered ? 1 : 0));
			gm.World.SetBlockRPC(_trigger.Chunk.ClrIdx, _trigger.ToWorldPos(), _bv);
		}));
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		cmds[0].enabled = !_world.IsEditor();
		cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override void Refresh(WorldBase _world, Chunk _chunk, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.Refresh(_world, _chunk, _cIdx, _blockPos, _blockValue);
		bool flag = (_blockValue.meta & 2) != 0;
		bool flag2 = (_blockValue.meta & 1) != 0;
		BlockUtilityNavIcon.UpdateNavIcon(flag2 && !flag, _blockPos);
		IChunk chunk = _chunk;
		if (chunk == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			chunk = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunk == null)
			{
				return;
			}
		}
		if (chunk == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
		if (blockEntity != null && blockEntity.bHasTransform)
		{
			BlockSwitchController component = blockEntity.transform.GetComponent<BlockSwitchController>();
			if ((bool)component)
			{
				component.SetState(flag2, flag);
			}
		}
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		_blockValue.meta = (byte)((_blockValue.meta & -2) | 1);
		_blockChanges.Add(new BlockChangeInfo(_cIdx, _blockPos, _blockValue));
	}
}
