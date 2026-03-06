using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockDoor : BlockSecure
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlaceEverywhere = "PlaceEverywhere";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<string> tintableMaterials = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPlaceEverywhere;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("close", "door", _enabled: false),
		new BlockActivationCommand("open", "door", _enabled: false)
	};

	public override void Init()
	{
		if (base.Properties.Values[Block.PropMultiBlockDim] == null)
		{
			base.Properties.Values[Block.PropMultiBlockDim] = "1,2,1";
		}
		base.Init();
		if (base.Properties.Values.ContainsKey("TintableMaterials"))
		{
			string[] array = base.Properties.Values["TintableMaterials"].Split(',');
			foreach (string text in array)
			{
				tintableMaterials.Add(text + " (Instance)");
			}
		}
		if (base.Properties.Values.ContainsKey("OpenSound"))
		{
			openSound = base.Properties.Values["OpenSound"];
		}
		if (base.Properties.Values.ContainsKey("CloseSound"))
		{
			closeSound = base.Properties.Values["CloseSound"];
		}
		if (base.Properties.Values.ContainsKey(PropPlaceEverywhere))
		{
			StringParsers.TryParseBool(base.Properties.Values[PropPlaceEverywhere], out bPlaceEverywhere);
		}
	}

	public static bool IsDoorOpen(byte _metadata)
	{
		return (_metadata & 1) != 0;
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				string[] obj = new string[5] { "Door on position ", null, null, null, null };
				Vector3i vector3i = parentPos;
				obj[1] = vector3i.ToString();
				obj[2] = " with value ";
				BlockValue blockValue = block;
				obj[3] = blockValue.ToString();
				obj[4] = " should be a parent but is not! (2)";
				Log.Error(string.Concat(obj));
				return true;
			}
			return IsMovementBlocked(_world, parentPos, block, _face);
		}
		return !IsDoorOpen(_blockValue.meta);
	}

	public override bool IsSeeThrough(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				string[] obj = new string[5] { "Door on position ", null, null, null, null };
				Vector3i vector3i = parentPos;
				obj[1] = vector3i.ToString();
				obj[2] = " with value ";
				BlockValue blockValue = block;
				obj[3] = blockValue.ToString();
				obj[4] = " should be a parent but is not! (1)";
				Log.Error(string.Concat(obj));
				return true;
			}
			return IsSeeThrough(_world, _clrIdx, parentPos, block);
		}
		return IsDoorOpen(_blockValue.meta);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (shape is BlockShapeModelEntity && (_oldBlockValue.type != _newBlockValue.type || _oldBlockValue.meta != _newBlockValue.meta) && !_newBlockValue.ischild)
		{
			if (VehicleManager.Instance != null)
			{
				VehicleManager.Instance.PhysicsWakeNear(_blockPos.ToVector3());
			}
			BlockEntityData blockEntity = ((World)_world).ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
			bool flag = IsDoorOpen(_newBlockValue.meta);
			bool flag2 = IsDoorOpen(_oldBlockValue.meta);
			if (flag != flag2)
			{
				updateAnimState(blockEntity, flag);
			}
		}
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _clrIdx, parentPos, block, _player);
		}
		if (!(_commandName == "close"))
		{
			if (_commandName == "open")
			{
				_blockValue.meta = (byte)(_blockValue.meta & -2);
				return OnBlockActivated(_world, _clrIdx, _blockPos, _blockValue, _player);
			}
			return false;
		}
		_blockValue.meta |= 1;
		return OnBlockActivated(_world, _clrIdx, _blockPos, _blockValue, _player);
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		bool flag = !IsDoorOpen(_blockValue.meta);
		updateOpenCloseState(flag, _world, _blockPos, _cIdx, _blockValue, _bOnlyLocal: false);
		if (_player != null)
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, flag ? openSound : closeSound);
		}
		return true;
	}

	public void HandleOpenCloseSound(bool isOpen, Vector3i currentPos)
	{
		if (isOpen)
		{
			Manager.BroadcastPlayByLocalPlayer(currentPos.ToVector3() + Vector3.one * 0.5f, openSound);
		}
		else
		{
			Manager.BroadcastPlayByLocalPlayer(currentPos.ToVector3() + Vector3.one * 0.5f, closeSound);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimState(BlockEntityData _ebcd, bool _bOpen)
	{
		if (_ebcd == null || !_ebcd.bHasTransform)
		{
			return;
		}
		Animator[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>();
		if (componentsInChildren != null)
		{
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				Animator obj = componentsInChildren[num];
				obj.enabled = true;
				obj.SetBool(AnimatorDoorState.IsOpenHash, _bOpen);
				obj.SetTrigger(AnimatorDoorState.OpenTriggerHash);
			}
		}
	}

	public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (_ebcd == null || !_ebcd.bHasTransform)
		{
			return;
		}
		Animator[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>();
		if (componentsInChildren != null)
		{
			bool flag = IsDoorOpen(_blockValue.meta);
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				Animator obj = componentsInChildren[num];
				obj.enabled = true;
				obj.keepAnimatorStateOnDisable = true;
				obj.SetBool(AnimatorDoorState.IsOpenHash, flag);
				obj.Play(flag ? AnimatorDoorState.OpenHash : AnimatorDoorState.CloseHash, 0, 1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateOpenCloseState(bool _bOpen, WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, bool _bOnlyLocal)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster != null)
		{
			_blockValue.meta = (byte)((_bOpen ? 1 : 0) | (_blockValue.meta & -2));
			if (!_bOnlyLocal)
			{
				_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
			}
			else
			{
				chunkCluster.SetBlockRaw(_blockPos, _blockValue);
			}
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			updateOpenCloseState(IsDoorOpen(_blockValue.meta), _world, _blockPos, _chunk.ClrIdx, _blockValue, _bOnlyLocal: true);
		}
	}

	public override float GetStepHeight(IBlockAccess world, Vector3i blockPos, BlockValue blockDef, BlockFace stepFace)
	{
		return 0f;
	}

	public bool IsOpen(IBlockAccess _blockAccess, int _x, int _y, int _z)
	{
		return IsDoorOpen(_blockAccess.GetBlock(_x, _y, _z).meta);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return GetBlockActivationCommands(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		cmds[0].enabled = IsDoorOpen(_blockValue.meta);
		cmds[1].enabled = !IsDoorOpen(_blockValue.meta);
		return cmds;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		if (_blockValue.Block.HasTag(BlockTags.Window))
		{
			return true;
		}
		if (bPlaceEverywhere)
		{
			return true;
		}
		if ((_world.GetBlock(_clrIdx, _blockPos.x - 1, _blockPos.y, _blockPos.z).Block.shape.IsSolidCube && _world.GetBlock(_blockPos.x + 1, _blockPos.y, _blockPos.z).Block.shape.IsSolidCube) || (_world.GetBlock(_clrIdx, _blockPos.x, _blockPos.y, _blockPos.z - 1).Block.shape.IsSolidCube && _world.GetBlock(_blockPos.x, _blockPos.y, _blockPos.z + 1).Block.shape.IsSolidCube))
		{
			if (!_world.GetBlock(_clrIdx, _blockPos.x, _blockPos.y - 1, _blockPos.z).Block.shape.IsSolidCube)
			{
				return false;
			}
			if (_world.GetBlock(_clrIdx, _blockPos.x, _blockPos.y + 1, _blockPos.z).isair)
			{
				return true;
			}
		}
		return false;
	}

	public override void RenderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		if (!(shape is BlockShapeModelEntity) || (_blockValue.meta & 2) == 0)
		{
			shape.renderDecorations(_worldPos, _blockValue, _drawPos, _vertices, _lightingAround, _textureFullArray, _meshes, _nBlocks);
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		return string.Format(Localization.Get("useBlock"), arg, _blockValue.Block.GetLocalizedBlockName());
	}

	public override bool IsExplosionAffected()
	{
		return false;
	}
}
