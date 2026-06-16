using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockable : TEFeatureAbs, ILockable, IFeatureTriggerCapability
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> allowedUserIds = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash = "";

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		if (_other.TryGetSelfOrFeature<TEFeatureLockable>(out var _typedTe))
		{
			locked = _typedTe.IsLocked();
			allowedUserIds.AddRange(_typedTe.GetUsers());
			passwordHash = _typedTe.GetPasswordHash();
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		TEFeatureLockable feature = _other.GetFeature<TEFeatureLockable>();
		if (feature != null)
		{
			locked = feature.IsLocked();
			allowedUserIds.AddRange(feature.GetUsers());
			passwordHash = feature.GetPasswordHash();
		}
	}

	public bool IsLocked()
	{
		return locked;
	}

	public void SetLocked(bool _isLocked)
	{
		locked = _isLocked;
		SetModified();
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (_triggeredBy != null && _triggeredBy.Unlock)
		{
			SetLocked(_isLocked: false);
		}
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return base.Parent.Owner;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		base.Parent.SetOwner(_userIdentifier);
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (IsOwner(_userIdentifier) || allowedUserIds.Contains(_userIdentifier))
		{
			return true;
		}
		if (!_userIdentifier.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return false;
		}
		if (!base.Parent.TryGetSelfOrFeature<TEFeatureDoor>(out var _) && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return true;
		}
		return false;
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return allowedUserIds;
	}

	public bool LocalPlayerIsOwner()
	{
		return IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(base.Parent.Owner) ?? false;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(passwordHash);
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (IsOwner(_userIdentifier) && _passwordHash != passwordHash)
		{
			passwordHash = _passwordHash;
			allowedUserIds.Clear();
			SetModified();
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (IsOwner(_userIdentifier) || !HasPassword())
		{
			return true;
		}
		if (_passwordHash == passwordHash)
		{
			allowedUserIds.Add(_userIdentifier);
			SetModified();
			return true;
		}
		return false;
	}

	public string GetPasswordHash()
	{
		return passwordHash;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (!IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (!IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("lock", "lock", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("unlock", "unlock", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("keypad", "keypad", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
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
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		bool flag = !LocalPlayerIsOwner() && GameManager.Instance.persistentPlayers.Allies.IsAlly(base.Parent.Owner, internalLocalUserIdentifier);
		if (CommandIs(_commandName, "lock"))
		{
			if (!IsLocked())
			{
				if (!(LocalPlayerIsOwner() || flag))
				{
					return _world.IsEditor();
				}
				return true;
			}
			return false;
		}
		if (CommandIs(_commandName, "unlock"))
		{
			if (IsLocked())
			{
				if (!LocalPlayerIsOwner())
				{
					return _world.IsEditor();
				}
				return true;
			}
			return false;
		}
		if (CommandIs(_commandName, "keypad"))
		{
			if (IsUserAllowed(internalLocalUserIdentifier) || !HasPassword() || !IsLocked())
			{
				return LocalPlayerIsOwner();
			}
			return true;
		}
		return true;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "lock"))
		{
			SetLocked(_isLocked: true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked");
			return true;
		}
		if (CommandIs(_commandName, "unlock"))
		{
			SetLocked(_isLocked: false);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			GameManager.ShowTooltip(_player, "containerUnlocked");
			return true;
		}
		if (CommandIs(_commandName, "keypad"))
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uIForPlayer != null)
			{
				XUiC_KeypadWindow.Open(uIForPlayer, this);
			}
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
		locked = _br.ReadBoolean();
		int num = _br.ReadInt32();
		allowedUserIds.Clear();
		for (int i = 0; i < num; i++)
		{
			allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
		passwordHash = _br.ReadString();
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		_bw.Write(locked);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
		_bw.Write(passwordHash);
	}
}
