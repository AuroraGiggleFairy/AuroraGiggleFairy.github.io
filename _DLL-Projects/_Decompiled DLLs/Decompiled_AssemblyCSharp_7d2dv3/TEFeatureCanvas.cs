using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureCanvas : TEFeatureAbs, IFeatureSavedInPrefab
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas.CanvasState pendingCanvasState = new SignCanvas.CanvasState();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas Canvas
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static CanvasRotationMode GetUprightRotation(byte blockRotation)
	{
		int num = blockRotation & 3;
		switch ((blockRotation >> 2) & 7)
		{
		case 0:
			return CanvasRotationMode.None;
		case 1:
			return CanvasRotationMode.Rot180;
		case 2:
		case 4:
			if ((num & 1) != 0)
			{
				return (CanvasRotationMode)num;
			}
			return CanvasRotationMode.None;
		case 3:
			if ((num & 1) != 1)
			{
				return (CanvasRotationMode)((num + 3) % 4);
			}
			return CanvasRotationMode.None;
		case 5:
			if ((num & 1) != 1)
			{
				return (CanvasRotationMode)((num + 1) % 4);
			}
			return CanvasRotationMode.None;
		default:
			return CanvasRotationMode.None;
		}
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (!_other.TryGetSelfOrFeature<TEFeatureCanvas>(out var _typedTe))
		{
			Log.Error("CopyFrom failed to get Canvas feature from other entity.");
			return;
		}
		pendingCanvasState = ((_typedTe.Canvas != null) ? _typedTe.Canvas.State.Clone() : _typedTe.pendingCanvasState?.Clone());
		if (pendingCanvasState != null && PrefabEditModeManager.Instance != null && PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension != null)
		{
			string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
			if (pendingCanvasState.SignId.libraryId != "[D]" && pendingCanvasState.SignId.libraryId != fileNameNoExtension && SignDataManager.Instance.MoveSignToLibrary(pendingCanvasState.SignId, fileNameNoExtension, out var newId))
			{
				pendingCanvasState.SignId = newId;
			}
		}
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform || GameManager.IsDedicatedServer)
		{
			return;
		}
		Canvas = _blockEntityData.transform.GetComponentInChildren<SignCanvas>(includeInactive: true);
		if (Canvas == null)
		{
			Log.Error($"TEFeatureCanvas at {ToWorldPos()} has no SignCanvas component in its block prefab.");
			return;
		}
		Canvas.RenderingDataUpdated -= SetModified;
		Canvas.RenderingDataUpdated += SetModified;
		Canvas.State = pendingCanvasState;
		Canvas.Initialize();
		LODGroup componentInChildren = _blockEntityData.transform.GetComponentInChildren<LODGroup>();
		if ((bool)componentInChildren)
		{
			LOD[] lODs = componentInChildren.GetLODs();
			lODs[^1].screenRelativeTransitionHeight = 0.003f;
			componentInChildren.SetLODs(lODs);
		}
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		pendingCanvasState.CanvasRotation = GetUprightRotation(_blockValue.rotation);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Canvas?.Cleanup();
	}

	public override void OnUnload(World _world)
	{
		base.OnUnload(_world);
		Canvas?.Cleanup();
	}

	public override void OnRemove(World _world)
	{
		base.OnRemove(_world);
		Canvas?.Cleanup();
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (lockpickFeature != null && lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (lockFeature != null && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return Localization.Get("useWorkstation");
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("edit", "pen", _enabled: true), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!_world.IsEditor())
		{
			return false;
		}
		if (!Equals(_module))
		{
			return true;
		}
		if (CommandIs(_commandName, "edit"))
		{
			if (lockFeature != null && !GameManager.Instance.IsEditMode() && lockFeature.IsLocked())
			{
				return lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
			}
			return true;
		}
		CommandIs(_commandName, "report");
		return true;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "edit"))
		{
			if (_world.IsEditor())
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					if (PrefabEditModeManager.Instance != null && PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.LoadedPrefab.Type == PathAbstractions.EAbstractedLocationType.None)
					{
						XUiC_SaveDirtyPrefab.Show(_player.PlayerUI.xui, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_SaveDirtyPrefab.ESelectedAction _action) =>
						{
							if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Save)
							{
								XUiC_SignGalleryWindow.Open(_player.PlayerUI, this);
							}
						}, XUiC_SaveDirtyPrefab.EMode.ForceSave);
					}
					else
					{
						XUiC_SignGalleryWindow.Open(_player.PlayerUI, this);
					}
				}
				else
				{
					GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "Cannot edit signs as client");
				}
			}
			return true;
		}
		if (CommandIs(_commandName, "report"))
		{
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		int num = ((_eStreamMode != TileEntity.StreamModeRead.Persistency) ? 21 : ((!base.Parent.UseLocalVersioning()) ? base.Parent.GetLegacyForkVersion() : _br.ReadUInt16()));
		SignCanvas.CanvasState canvasState;
		if (num >= 21)
		{
			canvasState = SignCanvas.CanvasState.Read(_br);
		}
		else
		{
			canvasState = new SignCanvas.CanvasState();
			canvasState.SignId = GlobalSignId.FromStream(_br);
			if (num < 18)
			{
				_br.ReadByte();
			}
			if (num >= 19)
			{
				canvasState.BlendMode = (SignCanvas.SignBlendMode)_br.ReadByte();
			}
			if (num >= 20)
			{
				canvasState.CanvasRotation = (CanvasRotationMode)_br.ReadByte();
			}
		}
		if (Canvas != null)
		{
			Canvas.State = canvasState;
		}
		else
		{
			pendingCanvasState = canvasState;
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)21);
		}
		((Canvas != null) ? Canvas.State : pendingCanvasState).Write(_bw);
	}
}
