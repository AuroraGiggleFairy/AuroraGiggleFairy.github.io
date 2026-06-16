public class TileEntityPoweredMeleeTrap(Chunk _chunk) : TileEntityPoweredBlock(_chunk)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ownerEntityID = -1;

	public int OwnerEntityID
	{
		get
		{
			if (ownerEntityID == -1)
			{
				SetOwnerEntityID();
			}
			return ownerEntityID;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			ownerEntityID = value;
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerMeleeTrap;
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
		SetOwnerEntityID();
		setModified();
	}

	public override void OnSetLocalChunkPosition()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOwnerEntityID()
	{
		ownerEntityID = -1;
		PersistentPlayerData persistentPlayerData = GameManager.Instance.GetPersistentPlayerList()?.GetPlayerData(ownerID);
		if (persistentPlayerData != null)
		{
			ownerEntityID = persistentPlayerData.EntityId;
		}
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode == StreamModeRead.Persistency)
		{
			if (UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				GetLegacyForkVersion();
			}
		}
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		SetOwnerEntityID();
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		ownerID.ToStream(_bw);
	}
}
