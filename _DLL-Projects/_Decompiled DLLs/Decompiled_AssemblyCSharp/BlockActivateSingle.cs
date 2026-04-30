using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockActivateSingle : Block
{
	public const int cMetaOn = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activateSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedBool = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedTrigger = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedState = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: true),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateSound = "ActivateSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimBool = "ActivatedAnimBool";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimTrigger = "ActivatedAnimTrigger";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimState = "ActivatedAnimState";

	public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(PropActivateSound, ref activateSound);
		base.Properties.ParseString(PropActivatedAnimBool, ref AnimActivatedBool);
		base.Properties.ParseString(PropActivatedAnimTrigger, ref AnimActivatedTrigger);
		base.Properties.ParseString(PropActivatedAnimState, ref AnimActivatedState);
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
		if (!_world.IsEditor() && (_blockValue.meta & 2) != 0)
		{
			return "";
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		_chunk.GetBlockTrigger(World.toBlock(_blockPos));
		Refresh(_world, _chunk, _chunk.ClrIdx, _blockPos, _blockValue);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!_world.IsEditor())
		{
			if ((_blockValue.meta & 2) != 0)
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
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _clrIdx, _blockPos, _showTriggers: true, _showTriggeredBy: false);
			}
		}
		else if (!_world.IsEditor())
		{
			bool flag = (_blockValue.meta & 2) != 0;
			if (!flag)
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
		BlockUtilityNavIcon.UpdateNavIcon(!flag, _blockPos);
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
			BlockSwitchSingleController component = blockEntity.transform.GetComponent<BlockSwitchSingleController>();
			if ((bool)component)
			{
				component.SetState(flag);
			}
			updateAnimState(blockEntity, flag);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimState(BlockEntityData _ebcd, bool _bOpen)
	{
		if (AnimActivatedBool == "" || AnimActivatedTrigger == "")
		{
			return;
		}
		Animator[] array = null;
		if (_ebcd != null && _ebcd.bHasTransform && (array = _ebcd.transform.GetComponentsInChildren<Animator>()) != null)
		{
			Animator[] array2 = array;
			foreach (Animator obj in array2)
			{
				obj.SetBool(AnimActivatedBool, _bOpen);
				obj.SetTrigger(AnimActivatedTrigger);
			}
		}
	}

	public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (AnimActivatedState == "" || AnimActivatedBool == "")
		{
			return;
		}
		Animator[] array = null;
		if (_ebcd == null || !_ebcd.bHasTransform || (array = _ebcd.transform.GetComponentsInChildren<Animator>(includeInactive: false)) == null)
		{
			return;
		}
		bool flag = (_blockValue.meta & 2) != 0;
		Animator[] array2 = array;
		foreach (Animator animator in array2)
		{
			animator.SetBool(AnimActivatedBool, flag);
			if (flag)
			{
				animator.CrossFade(AnimActivatedState, 0f);
			}
			else
			{
				animator.CrossFade(AnimActivatedState, 0f);
			}
		}
	}
}
