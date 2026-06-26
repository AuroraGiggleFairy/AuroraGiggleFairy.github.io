public class TileEntityTrader : TileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	public TraderData TraderData;

	public bool syncNeeded = true;

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

	public TileEntityTrader(Chunk _chunk)
		: base(_chunk)
	{
		TraderData = new TraderData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityTrader(TileEntityTrader _other)
		: base(null)
	{
		bUserAccessing = _other.bUserAccessing;
		TraderData = new TraderData(_other.TraderData);
	}

	public override TileEntity Clone()
	{
		return new TileEntityTrader(this);
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		if (TraderData == null)
		{
			TraderData = new TraderData();
		}
		TraderData.Read(0, _br);
		syncNeeded = false;
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		TraderData.Write(_bw);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Trader;
	}
}
