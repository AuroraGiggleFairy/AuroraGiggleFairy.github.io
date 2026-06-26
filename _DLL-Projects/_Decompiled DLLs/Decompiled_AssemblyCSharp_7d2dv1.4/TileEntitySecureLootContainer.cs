using System.Collections.Generic;
using Platform;

public class TileEntitySecureLootContainer : TileEntityLootContainer, ILockable, ILockPickable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	public float PickTimeLeft = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string password;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bPlayerPlaced;

	public override float LootStageMod => ((BlockSecureLoot)base.blockValue.Block).LootStageMod;

	public override float LootStageBonus => ((BlockSecureLoot)base.blockValue.Block).LootStageBonus;

	public new int EntityId
	{
		get
		{
			return entityId;
		}
		set
		{
			entityId = value;
		}
	}

	public TileEntitySecureLootContainer(Chunk _chunk)
		: base(_chunk)
	{
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		isLocked = true;
		ownerID = null;
		password = "";
		bPlayerPlaced = false;
	}

	public bool IsLocked()
	{
		return isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		isLocked = _isLocked;
		setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
		setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if ((_userIdentifier != null && _userIdentifier.Equals(ownerID)) || allowedUserIds.Contains(_userIdentifier))
		{
			return true;
		}
		return false;
	}

	public bool LocalPlayerIsOwner()
	{
		return IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(password);
	}

	public string GetPassword()
	{
		return password;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(ownerID))
		{
			if (Utils.HashString(_password) != password)
			{
				changed = true;
				password = Utils.HashString(_password);
				allowedUserIds.Clear();
				setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == password)
		{
			allowedUserIds.Add(_userIdentifier);
			setModified();
			return true;
		}
		return false;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.SecureLoot;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		bPlayerPlaced = _br.ReadBoolean();
		isLocked = _br.ReadBoolean();
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		password = _br.ReadString();
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(bPlayerPlaced);
		_bw.Write(isLocked);
		ownerID.ToStream(_bw);
		_bw.Write(password);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable)
		{
			ILockable lockable = _other as ILockable;
			EntityId = lockable.EntityId;
			SetLocked(lockable.IsLocked());
			SetOwner(lockable.GetOwner());
			allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			password = lockable.GetPassword();
			setModified();
		}
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return allowedUserIds;
	}

	public void ShowLockpickUi(EntityPlayerLocal _player)
	{
		if (_player != null && base.blockValue.Block is BlockSecureLoot blockSecureLoot)
		{
			blockSecureLoot.ShowLockpickUi(this, _player);
		}
	}
}
