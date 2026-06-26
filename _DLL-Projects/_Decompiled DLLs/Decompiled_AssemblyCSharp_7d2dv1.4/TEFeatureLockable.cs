using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockable : TEFeatureAbs, ILockable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ILockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> allowedUserIds = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash = "";

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockpickFeature = base.Parent.GetFeature<ILockPickable>();
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ILockable feature = _other.GetFeature<ILockable>();
		if (feature != null)
		{
			locked = feature.IsLocked();
			allowedUserIds.AddRange(feature.GetUsers());
			passwordHash = feature.GetPassword();
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
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
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

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool _changed)
	{
		_changed = false;
		string text = _password.GetStableHashCode().ToString("X8");
		if (IsOwner(_userIdentifier))
		{
			if (text != passwordHash)
			{
				_changed = true;
				passwordHash = text;
				allowedUserIds.Clear();
				SetModified();
			}
			return true;
		}
		if (text == passwordHash)
		{
			allowedUserIds.Add(_userIdentifier);
			SetModified();
			return true;
		}
		return false;
	}

	public string GetPassword()
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
		if (lockpickFeature == null && !IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
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

	public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		HashSet<PlatformUserIdentifierAbs> hashSet = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(base.Parent.Owner)?.ACL;
		bool flag = !LocalPlayerIsOwner() && (hashSet?.Contains(internalLocalUserIdentifier) ?? false);
		if (CommandIs(_commandName, "lock"))
		{
			_command.enabled = !IsLocked() && (LocalPlayerIsOwner() || flag);
		}
		else if (CommandIs(_commandName, "unlock"))
		{
			_command.enabled = IsLocked() && LocalPlayerIsOwner();
		}
		else if (CommandIs(_commandName, "keypad"))
		{
			_command.enabled = (!IsUserAllowed(internalLocalUserIdentifier) && HasPassword() && IsLocked()) || LocalPlayerIsOwner();
		}
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

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
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
		_bw.Write(locked);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
		_bw.Write(passwordHash);
	}
}
