using System.Collections.Generic;
using Platform;

public class TileEntitySecure : TileEntityLootContainer, ILockable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayerPlaced;

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

	public TileEntitySecure(Chunk _chunk)
		: base(_chunk)
	{
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		isLocked = false;
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

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if ((_userIdentifier != null && _userIdentifier.Equals(ownerID)) || allowedUserIds.Contains(_userIdentifier))
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
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(password);
	}

	public string GetPassword()
	{
		return password;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable lockable)
		{
			entityId = lockable.EntityId;
			isLocked = lockable.IsLocked();
			ownerID = lockable.GetOwner();
			allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			password = lockable.GetPassword();
		}
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

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_br.ReadInt32() > 0)
		{
			bPlayerPlaced = _br.ReadBoolean();
			isLocked = _br.ReadBoolean();
			ownerID = PlatformUserIdentifierAbs.FromStream(_br);
			int num = _br.ReadInt32();
			allowedUserIds = new List<PlatformUserIdentifierAbs>();
			for (int i = 0; i < num; i++)
			{
				allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
			}
			password = _br.ReadString();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(bPlayerPlaced);
		_bw.Write(isLocked);
		ownerID.ToStream(_bw);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
		_bw.Write(password);
	}
}
