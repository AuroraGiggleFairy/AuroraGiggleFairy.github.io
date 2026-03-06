using UnityEngine;

public class TileEntityLight(Chunk _chunk) : TileEntity(_chunk)
{
	public LightType LightType = LightType.Point;

	public LightShadows LightShadows;

	public float LightIntensity = 1f;

	public float LightRange = 10f;

	public float LightAngle = 45f;

	public Color LightColor = Color.white;

	public LightStateType LightState;

	public float Rate = 1f;

	public float Delay = 1f;

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Light;
	}

	public override TileEntity Clone()
	{
		return new TileEntityLight(chunk)
		{
			localChunkPos = base.localChunkPos,
			LightType = LightType,
			LightIntensity = LightIntensity,
			LightRange = LightRange,
			LightColor = LightColor,
			LightAngle = LightAngle,
			LightShadows = LightShadows,
			LightState = LightState,
			Rate = Rate,
			Delay = Delay
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntityLight tileEntityLight = (TileEntityLight)_other;
		base.localChunkPos = tileEntityLight.localChunkPos;
		LightType = tileEntityLight.LightType;
		LightIntensity = tileEntityLight.LightIntensity;
		LightRange = tileEntityLight.LightRange;
		LightColor = tileEntityLight.LightColor;
		LightAngle = tileEntityLight.LightAngle;
		LightShadows = tileEntityLight.LightShadows;
		LightState = tileEntityLight.LightState;
		Rate = tileEntityLight.Rate;
		Delay = tileEntityLight.Delay;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		LightIntensity = _br.ReadSingle();
		LightRange = _br.ReadSingle();
		LightColor = StreamUtils.ReadColor32(_br);
		if (_eStreamMode != StreamModeRead.Persistency || readVersion > 4)
		{
			LightType = (LightType)_br.ReadByte();
			LightAngle = _br.ReadSingle();
			LightShadows = (LightShadows)_br.ReadByte();
		}
		if (_eStreamMode != StreamModeRead.Persistency || readVersion > 5)
		{
			LightState = (LightStateType)_br.ReadByte();
		}
		if (_eStreamMode != StreamModeRead.Persistency || readVersion > 6)
		{
			Rate = _br.ReadSingle();
		}
		if (_eStreamMode != StreamModeRead.Persistency || readVersion > 7)
		{
			Delay = _br.ReadSingle();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(LightIntensity);
		_bw.Write(LightRange);
		StreamUtils.WriteColor32(_bw, LightColor);
		_bw.Write((byte)LightType);
		_bw.Write(LightAngle);
		_bw.Write((byte)LightShadows);
		_bw.Write((byte)LightState);
		_bw.Write(Rate);
		_bw.Write(Delay);
	}
}
