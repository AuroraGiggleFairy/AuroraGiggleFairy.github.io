public class TileEntitySleeper : TileEntity
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float priorityMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sightAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hearingPercent;

	public TileEntitySleeper(Chunk _chunk)
		: base(_chunk)
	{
		priorityMultiplier = 1f;
		sightAngle = -1;
		sightRange = -1;
		hearingPercent = 1f;
	}

	public override TileEntity Clone()
	{
		return new TileEntitySleeper(chunk)
		{
			localChunkPos = base.localChunkPos,
			priorityMultiplier = priorityMultiplier,
			sightAngle = sightAngle,
			sightRange = sightRange,
			hearingPercent = hearingPercent
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntitySleeper tileEntitySleeper = (TileEntitySleeper)_other;
		priorityMultiplier = tileEntitySleeper.priorityMultiplier;
		sightAngle = tileEntitySleeper.sightAngle;
		sightRange = tileEntitySleeper.sightRange;
		hearingPercent = tileEntitySleeper.hearingPercent;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Sleeper;
	}

	public void SetPriorityMultiplier(float _priorityMultiplier)
	{
		priorityMultiplier = _priorityMultiplier;
		setModified();
	}

	public float GetPriorityMultiplier()
	{
		return priorityMultiplier;
	}

	public void SetSightAngle(int _sightAngle)
	{
		sightAngle = _sightAngle;
		setModified();
	}

	public int GetSightAngle()
	{
		return sightAngle;
	}

	public void SetSightRange(int _sightRange)
	{
		sightRange = _sightRange;
		setModified();
	}

	public int GetSightRange()
	{
		return sightRange;
	}

	public void SetHearingPercent(float _hearingPercent)
	{
		hearingPercent = _hearingPercent;
		setModified();
	}

	public float GetHearingPercent()
	{
		return hearingPercent;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		priorityMultiplier = _br.ReadSingle();
		sightRange = _br.ReadInt16();
		hearingPercent = _br.ReadSingle();
		sightAngle = _br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(priorityMultiplier);
		_bw.Write((short)sightRange);
		_bw.Write(hearingPercent);
		_bw.Write((short)sightAngle);
	}
}
