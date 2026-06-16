using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSwitch : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("light", "electric_switch", _enabled: false),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockSwitch()
	{
		HasTileEntity = true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if ((_blockValue.meta & 2) != 0)
		{
			return string.Format(Localization.Get("useSwitchLightOff"), arg);
		}
		return string.Format(Localization.Get("useSwitchLightOn"), arg);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "light"))
		{
			if (_commandName == "take")
			{
				takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
				return true;
			}
			return false;
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredTrigger))
		{
			return false;
		}
		updateState(_world, _blockPos, _blockValue, _bChangeState: true);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bChangeState = false)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		if (chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z)) == null)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 1) != 0;
		bool flag2 = (_blockValue.meta & 2) != 0;
		if (_bChangeState)
		{
			flag2 = !flag2;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag2 ? 2 : 0));
			_blockValue.meta = (byte)((_blockValue.meta & -2) | (flag ? 1 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
			if (flag2)
			{
				Manager.BroadcastPlay(_blockPos.ToVector3(), "switch_up");
			}
			else
			{
				Manager.BroadcastPlay(_blockPos.ToVector3(), "switch_down");
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _world.GetTileEntity(_blockPos) is TileEntityPoweredTrigger tileEntityPoweredTrigger)
		{
			tileEntityPoweredTrigger.IsTriggered = flag2;
		}
		BlockEntityData blockEntity = ((World)_world).ChunkCache.GetBlockEntity(_blockPos);
		if (blockEntity != null && blockEntity.transform != null && blockEntity.transform.gameObject != null)
		{
			Renderer[] componentsInChildren = blockEntity.transform.gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
					{
						componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
					}
					if (flag)
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", flag2 ? Color.green : Color.red);
					}
					else
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
					}
					componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
					componentsInChildren[i].material.EnableKeyword("_EMISSION");
				}
			}
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		updateState(_world, _blockPos, _blockValue);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		updateState(_world, _blockPos, _newBlockValue);
		BlockEntityData blockEntity = ((World)_world).ChunkCache.GetBlockEntity(_blockPos);
		updateAnimState(blockEntity, IsSwitchOn(_newBlockValue.meta), _newBlockValue);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = true;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		_blockValue.meta = (byte)((_blockValue.meta & -2) | (isPowered ? 1 : 0));
		_world.SetBlockRPC(_blockPos, _blockValue);
		updateState(_world, _blockPos, _blockValue);
		return true;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredTrigger(chunk);
	}

	public static bool IsSwitchOn(byte _metadata)
	{
		return (_metadata & 2) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimState(BlockEntityData _ebcd, bool _bOpen, BlockValue _blockValue)
	{
		Animator[] array = null;
		if (_ebcd != null && _ebcd.bHasTransform && (array = _ebcd.transform.GetComponentsInChildren<Animator>()) != null)
		{
			Animator[] array2 = array;
			foreach (Animator obj in array2)
			{
				obj.SetBool("SwitchActivated", _bOpen);
				obj.SetTrigger("SwitchTrigger");
			}
		}
	}

	public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
		Animator[] array = null;
		if (_ebcd == null || !_ebcd.bHasTransform || (array = _ebcd.transform.GetComponentsInChildren<Animator>(includeInactive: false)) == null)
		{
			return;
		}
		bool flag = IsSwitchOn(_blockValue.meta);
		Animator[] array2 = array;
		foreach (Animator animator in array2)
		{
			animator.SetBool("SwitchActivated", flag);
			if (flag)
			{
				animator.CrossFade("SwitchOnStatic", 0f);
			}
			else
			{
				animator.CrossFade("SwitchOffStatic", 0f);
			}
		}
	}
}
