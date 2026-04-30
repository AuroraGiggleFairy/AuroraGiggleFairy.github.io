using UnityEngine;

public class TileEntityLandClaim : TileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	public Transform BoundsHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBounds;

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

	public TileEntityLandClaim(Chunk _chunk)
		: base(_chunk)
	{
		ownerID = null;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
		setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (_userIdentifier != null && _userIdentifier.Equals(ownerID))
		{
			return true;
		}
		return false;
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		showBounds = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(0);
		ownerID.ToStream(_bw);
		_bw.Write(showBounds);
	}

	public override TileEntity Clone()
	{
		return new TileEntityLandClaim(chunk)
		{
			localChunkPos = base.localChunkPos,
			ownerID = ownerID,
			showBounds = showBounds
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_other;
		base.localChunkPos = tileEntityLandClaim.localChunkPos;
		ownerID = tileEntityLandClaim.ownerID;
		showBounds = tileEntityLandClaim.ShowBounds;
	}

	public int GetEntityID()
	{
		return entityId;
	}

	public void SetEntityID(int _entityID)
	{
		entityId = _entityID;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (BoundsHelper != null)
		{
			BoundsHelper.localPosition = ToWorldPos().ToVector3() - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.LandClaim;
	}
}
