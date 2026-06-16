using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLandClaim : TEFeatureAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	public Transform BoundsHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inactivePrompt;

	public bool ShowBounds
	{
		get
		{
			return showBounds;
		}
		set
		{
			showBounds = value;
			SetModified();
		}
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		activePrompt = Localization.Get("activeBlockPrompt");
		inactivePrompt = Localization.Get("inactiveBlockPrompt");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (_other.TryGetSelfOrFeature<TEFeatureLandClaim>(out var _typedTe))
		{
			showBounds = _typedTe.showBounds;
		}
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
		if (IsPrimary())
		{
			string text = (base.Parent.LocalPlayerIsOwner ? ("\n" + Localization.Get("useWorkstation")) : "");
			return string.Format(activePrompt, _blockValue.Block.GetLocalizedBlockName()) + text;
		}
		return string.Format(inactivePrompt, _blockValue.Block.GetLocalizedBlockName());
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("show_bounds", "frames", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("hide_bounds", "frames", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("remove", "x", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		bool flag = base.Parent.LocalPlayerIsOwner && IsPrimary();
		if (!Equals(_module))
		{
			return true;
		}
		if (lockFeature != null && !GameManager.Instance.IsEditMode() && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return false;
		}
		if (CommandIs(_commandName, "show_bounds"))
		{
			if (flag)
			{
				return !ShowBounds;
			}
			return false;
		}
		if (CommandIs(_commandName, "hide_bounds"))
		{
			if (flag)
			{
				return ShowBounds;
			}
			return false;
		}
		CommandIs(_commandName, "remove");
		return flag;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "show_bounds") || CommandIs(_commandName, "hide_bounds"))
		{
			ShowBounds = !ShowBounds;
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(ToWorldPos());
			if (boundsHelper != null)
			{
				BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(ShowBounds);
			}
			return true;
		}
		if (CommandIs(_commandName, "remove"))
		{
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
			_player.PlayOneShot("keystone_destroyed");
			return true;
		}
		return false;
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		GameManager.Instance.persistentPlayers.PlaceLandProtectionBlock(ToWorldPos(), base.Parent.Owner);
		if (base.Parent.LocalPlayerIsOwner)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(ToWorldPos());
			if (boundsHelper != null)
			{
				BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(ShowBounds);
			}
		}
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
		showBounds = _br.ReadBoolean();
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		_bw.Write(showBounds);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (BoundsHelper != null)
		{
			BoundsHelper.localPosition = ToWorldPos().ToVector3() - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}
	}

	public override void OnLoad()
	{
		base.OnLoad();
		Vector3i vector3i = ToWorldPos();
		if (!IsPrimary())
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				HandleDeactivateLandClaim(vector3i);
			}
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(vector3i, "land_claim");
			if (GameManager.Instance.persistentPlayers.m_lpBlockMap.ContainsKey(vector3i))
			{
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers.m_lpBlockMap[vector3i];
				GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(vector3i);
				persistentPlayerData.LPBlocks.Remove(vector3i);
			}
			ShowBounds = false;
		}
		if (base.Parent.LocalPlayerIsOwner)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(vector3i);
			if (boundsHelper != null)
			{
				BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(ShowBounds);
			}
		}
	}

	public override void OnUnload(World _world)
	{
		base.OnUnload(_world);
		LandClaimBoundsHelper.RemoveBoundsHelper(ToWorldPos());
	}

	public override void OnRemove(World _world)
	{
		base.OnRemove(_world);
		LandClaimBoundsHelper.RemoveBoundsHelper(ToWorldPos());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		GameManager.Instance.persistentPlayers.RemoveLandProtectionBlock(ToWorldPos());
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, ToWorldPos()));
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!IsPrimary())
		{
			ShowBounds = false;
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(ToWorldPos());
			if (boundsHelper != null)
			{
				BoundsHelper = null;
				boundsHelper.gameObject.SetActive(value: false);
				LandClaimBoundsHelper.RemoveBoundsHelper(ToWorldPos());
			}
		}
	}

	public bool IsPrimary()
	{
		return GameManager.Instance.persistentPlayers.GetLandProtectionBlockOwner(ToWorldPos()) != null;
	}

	public static void HandleDeactivateLandClaim(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		if (!block.isair)
		{
			GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				block.damage = block.Block.MaxDamage - 1;
				world.SetBlockRPC(_blockPos, block);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _blockPos));
			}
		}
	}
}
