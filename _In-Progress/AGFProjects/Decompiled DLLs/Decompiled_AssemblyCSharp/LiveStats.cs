using System;
using System.IO;

public class LiveStats
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int liveLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxLiveLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oversaturationLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturationLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float exhaustionLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timer;

	public LiveStats(int _maxLiveLevel, int _oversaturationLevel)
	{
		maxLiveLevel = _maxLiveLevel;
		oversaturationLevel = _oversaturationLevel;
		Reset();
	}

	public void Copy(LiveStats other)
	{
		liveLevel = other.liveLevel;
		maxLiveLevel = other.maxLiveLevel;
		oversaturationLevel = other.oversaturationLevel;
		saturationLevel = other.saturationLevel;
		exhaustionLevel = other.exhaustionLevel;
		timer = other.timer;
	}

	public void Reset()
	{
		timer = 0;
		liveLevel = maxLiveLevel;
		saturationLevel = 0f;
		exhaustionLevel = 0f;
	}

	public void AddStats(int _addLifeValue)
	{
		liveLevel += _addLifeValue;
		if (liveLevel > maxLiveLevel)
		{
			saturationLevel += liveLevel - maxLiveLevel;
			liveLevel = maxLiveLevel;
			saturationLevel = Utils.FastMin(saturationLevel, oversaturationLevel);
		}
		if (liveLevel < 0)
		{
			liveLevel = 0;
		}
	}

	public void OnUpdate(EntityPlayer _entityPlayer)
	{
		while (exhaustionLevel > 1f)
		{
			exhaustionLevel -= 1f;
			if (saturationLevel > 0f)
			{
				saturationLevel = Math.Max(saturationLevel - 1f, 0f);
			}
			else
			{
				liveLevel = Math.Max(liveLevel - 1, 0);
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		liveLevel = _br.ReadInt16();
		timer = _br.ReadInt16();
		saturationLevel = _br.ReadSingle();
		exhaustionLevel = _br.ReadSingle();
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)liveLevel);
		_bw.Write((ushort)timer);
		_bw.Write(saturationLevel);
		_bw.Write(exhaustionLevel);
	}

	public int GetLifeLevel()
	{
		return liveLevel;
	}

	public int GetMaxLifeLevel()
	{
		return maxLiveLevel;
	}

	public void SetLifeLevel(int _value)
	{
		liveLevel = _value;
	}

	public bool IsFilledUp()
	{
		return liveLevel >= maxLiveLevel;
	}

	public void AddExhaustion(float _v)
	{
		exhaustionLevel = Math.Min(exhaustionLevel + _v, 40f);
	}

	public float GetSaturationLevel()
	{
		return saturationLevel;
	}

	public void SetSaturationLevel(float _level)
	{
		saturationLevel = _level;
	}

	public float GetExhaustionLevel()
	{
		return exhaustionLevel;
	}

	public void SetExhaustionLevel(float _level)
	{
		exhaustionLevel = _level;
	}

	public float GetLifeLevelFraction()
	{
		return (float)liveLevel / (float)maxLiveLevel;
	}

	public LiveStats Clone()
	{
		LiveStats liveStats = new LiveStats(maxLiveLevel, oversaturationLevel);
		liveStats.SetSaturationLevel(saturationLevel);
		liveStats.SetExhaustionLevel(exhaustionLevel);
		liveStats.SetLifeLevel(liveLevel);
		return liveStats;
	}
}
