using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureDoor : TEFeatureAbs, IFeatureTriggerCapability, IFeaturePhysicalCapabilities, IFeatureSavedInPrefab
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockedSound = "LockedSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropIsDrawBridge = "IsDrawBridge";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropAutoCloseTime = "AutoCloseTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockedSound = "Misc/locked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<string> tintableMaterials = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDrawBridge;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool animateOnSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoCloseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong autoCloseAtTickTime;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		isDrawBridge = false;
		DynamicProperties props = _featureData.Props;
		if (base.Parent.TeData.CompositeProps.Values.ContainsKey(Block.PropMultiBlockDim))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " requires a " + Block.PropMultiBlockDim + " property when including a Door feature");
		}
		props.ParseString(PropOpenSound, ref openSound);
		props.ParseString(PropCloseSound, ref closeSound);
		props.ParseString(PropLockedSound, ref lockedSound);
		props.ParseBool(PropIsDrawBridge, ref isDrawBridge);
		props.ParseFloat(PropAutoCloseTime, ref autoCloseTime);
		if (props.Values.ContainsKey("TintableMaterials"))
		{
			string[] array = props.Values["TintableMaterials"].Split(',');
			foreach (string text in array)
			{
				tintableMaterials.Add(text + " (Instance)");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (_other.TryGetSelfOrFeature<TEFeatureDoor>(out var _typedTe))
		{
			isOpen = _typedTe.isOpen;
			openSound = _typedTe.openSound;
			closeSound = _typedTe.closeSound;
			lockedSound = _typedTe.lockedSound;
			isDrawBridge = _typedTe.isDrawBridge;
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		TEFeatureDoor feature = _other.GetFeature<TEFeatureDoor>();
		if (feature != null)
		{
			isOpen = feature.isOpen;
			openSound = feature.openSound;
			closeSound = feature.closeSound;
			lockedSound = feature.lockedSound;
			isDrawBridge = feature.isDrawBridge;
		}
	}

	public void HandleOpenCloseSound(Vector3i _blockPos)
	{
		if (ThreadManager.IsMainThread())
		{
			string soundGroupName = (isOpen ? openSound : closeSound);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, soundGroupName);
		}
	}

	public void UpdateAnimState(BlockEntityData _ebcd)
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
				obj.SetBool(AnimatorDoorState.IsOpenHash, isOpen);
				obj.SetTrigger(AnimatorDoorState.OpenTriggerHash);
			}
		}
	}

	public void ForceAnimationState(BlockEntityData _ebcd)
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
				obj.keepAnimatorStateOnDisable = true;
				obj.SetBool(AnimatorDoorState.IsOpenHash, isOpen);
				obj.Play(isOpen ? AnimatorDoorState.OpenHash : AnimatorDoorState.CloseHash, 0, 1f);
			}
		}
	}

	public bool IsOpen()
	{
		return isOpen;
	}

	public void SetOpen(bool _open, bool _animate = false)
	{
		if (isOpen != _open)
		{
			isOpen = _open;
			animateOnSync = _animate;
			if (isOpen && autoCloseTime > 0f)
			{
				autoCloseAtTickTime = GameTimer.Instance.ticks + (ulong)(autoCloseTime * 20f);
			}
			else
			{
				autoCloseAtTickTime = 0uL;
			}
			HandleDoorAnimation(_animate);
			if (_animate)
			{
				HandleOpenCloseSound(ToWorldPos());
			}
			GameManager.Instance.World.SetBlockRPC(ToWorldPos(), base.Parent.blockValue);
			SetModified();
		}
	}

	public bool CanOpen(out bool canPickToOpen)
	{
		canPickToOpen = false;
		if (IsOpen())
		{
			return true;
		}
		canPickToOpen = lockpickFeature?.NeedsLockpicking() ?? false;
		if (lockFeature != null && lockFeature.IsLocked())
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleDoorAnimation(bool _animate)
	{
		BlockEntityData blockEntity = GameManager.Instance.World.ChunkCache.GetBlockEntity(base.Parent.ToWorldPos());
		if (_animate)
		{
			UpdateAnimState(blockEntity);
		}
		else
		{
			ForceAnimationState(blockEntity);
		}
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData != null && _blockEntityData.bHasTransform)
		{
			ForceAnimationState(_blockEntityData);
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && isOpen && !(autoCloseTime <= 0f) && autoCloseAtTickTime != 0L && GameTimer.Instance.ticks >= autoCloseAtTickTime)
		{
			SetOpen(_open: false, _animate: true);
		}
	}

	public override void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (_newBlockValue.Block.shape is BlockShapeModelEntity && _oldBlockValue.type != _newBlockValue.type && !_newBlockValue.ischild)
		{
			if (VehicleManager.Instance != null)
			{
				VehicleManager.Instance.PhysicsWakeNear(_blockPos.ToVector3());
			}
			BlockEntityData blockEntity = GameManager.Instance.World.ChunkCache.GetBlockEntity(_blockPos);
			ForceAnimationState(blockEntity);
		}
	}

	public override void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		SetOpen(_open: false);
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (lockFeature != null && _triggeredBy != null && _triggeredBy.Unlock)
		{
			lockFeature.SetLocked(_isLocked: false);
		}
		SetOpen(!isOpen, _animate: true);
	}

	public bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (!isDrawBridge)
		{
			return !isOpen;
		}
		Block block = _blockValue.Block;
		if (block.isMultiBlock && _blockValue.ischild)
		{
			if (!isOpen)
			{
				return true;
			}
			Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			return _blockPos.y == parentPos.y;
		}
		return true;
	}

	public bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (!isDrawBridge)
		{
			return !isOpen;
		}
		Block block = _blockValue.Block;
		if (block.isMultiBlock && _blockValue.ischild)
		{
			if (!isOpen)
			{
				return true;
			}
			Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			return _blockPos.y == parentPos.y;
		}
		return true;
	}

	public bool IsSeeThrough(Vector3i _blockPos, BlockValue _blockValue)
	{
		return isOpen;
	}

	public float GetStepHeight(Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		if (!isDrawBridge)
		{
			return 0f;
		}
		if (!isOpen)
		{
			return 1f;
		}
		Block block = _blockValue.Block;
		if (block.isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			if (_blockPos.y == parentPos.y)
			{
				return 1f;
			}
		}
		return 0f;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		string arg = Localization.Get("door");
		if (lockpickFeature != null && lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, arg);
		}
		if (lockFeature != null && lockFeature.IsLocked())
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, arg);
		}
		return string.Format(Localization.Get("tooltipUnlocked"), _activateHotkeyMarkup, arg);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("open", "door", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("close", "door", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!Equals(_module))
		{
			return true;
		}
		if (CommandIs(_commandName, "open"))
		{
			return !isOpen;
		}
		if (CommandIs(_commandName, "close"))
		{
			return isOpen;
		}
		return true;
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		if (isDrawBridge)
		{
			isOpen = true;
			SetModified();
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "close") || CommandIs(_commandName, "open"))
		{
			bool flag = false;
			if (lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				flag = true;
			}
			if (flag)
			{
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, lockedSound);
				TraderArea traderAreaAt = ((World)_world).GetTraderAreaAt(_blockPos);
				if (traderAreaAt != null)
				{
					EntityTrader trader = traderAreaAt.GetTrader();
					GameManager.ShowTooltip(_player, trader.GetNextTimeMessage(), string.Empty, null, null, _showImmediately: true);
				}
				return false;
			}
			SetOpen(!isOpen, _animate: true);
			_blockValue.Block.HandleTrigger(_player, (World)_world, _blockPos, _blockValue);
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.Parent.GetLegacyForkVersion();
			}
		}
		bool flag = isOpen;
		isOpen = _br.ReadBoolean();
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency && autoCloseTime > 0f)
		{
			isOpen = false;
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			bool animate = _br.ReadBoolean();
			if (flag != isOpen)
			{
				HandleDoorAnimation(animate);
			}
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		_bw.Write(isOpen);
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(animateOnSync);
			animateOnSync = false;
		}
	}
}
