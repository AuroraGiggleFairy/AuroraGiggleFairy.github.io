using System;
using System.IO;

public struct BiomeIntensity : IEquatable<BiomeIntensity>
{
	public const int cDataSize = 6;

	public static BiomeIntensity Default;

	public byte biomeId0;

	public byte biomeId1;

	public byte biomeId2;

	public byte biomeId3;

	public byte intensity0and1;

	public byte intensity2and3;

	public float intensity0
	{
		get
		{
			return (float)(intensity0and1 & 0xF) / 15f;
		}
		set
		{
			intensity0and1 = (byte)((intensity0and1 & 0xF0) | ((int)(value * 15f) & 0xF));
		}
	}

	public float intensity1
	{
		get
		{
			return (float)((intensity0and1 >> 4) & 0xF) / 15f;
		}
		set
		{
			intensity0and1 = (byte)((intensity0and1 & 0xF) | (((int)(value * 15f) << 4) & 0xF0));
		}
	}

	public float intensity2
	{
		get
		{
			return (float)(intensity2and3 & 0xF) / 15f;
		}
		set
		{
			intensity2and3 = (byte)((intensity2and3 & 0xF0) | ((int)(value * 15f) & 0xF));
		}
	}

	public float intensity3
	{
		get
		{
			return (float)((intensity2and3 >> 4) & 0xF) / 15f;
		}
		set
		{
			intensity2and3 = (byte)((intensity2and3 & 0xF) | (((int)(value * 15f) << 4) & 0xF0));
		}
	}

	public BiomeIntensity(byte _singleBiomeId)
	{
		biomeId0 = _singleBiomeId;
		biomeId1 = 0;
		biomeId2 = 0;
		biomeId3 = 0;
		intensity0and1 = 15;
		intensity2and3 = 0;
	}

	public BiomeIntensity(byte[] _chunkBiomeIntensityArray, int _offs)
	{
		biomeId0 = _chunkBiomeIntensityArray[_offs];
		biomeId1 = _chunkBiomeIntensityArray[_offs + 1];
		biomeId2 = _chunkBiomeIntensityArray[_offs + 2];
		biomeId3 = _chunkBiomeIntensityArray[_offs + 3];
		intensity0and1 = _chunkBiomeIntensityArray[_offs + 4];
		intensity2and3 = _chunkBiomeIntensityArray[_offs + 5];
	}

	public static BiomeIntensity FromArray(int[] _unsortedBiomeIdArray)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		BiomeIntensity result = default(BiomeIntensity);
		for (int i = 0; i < _unsortedBiomeIdArray.Length; i++)
		{
			if (num < _unsortedBiomeIdArray[i])
			{
				result.biomeId0 = (byte)i;
				num = _unsortedBiomeIdArray[i];
				if (num5 < num)
				{
					num5 = num;
				}
			}
		}
		_unsortedBiomeIdArray[result.biomeId0] = 0;
		for (int j = 0; j < _unsortedBiomeIdArray.Length; j++)
		{
			if (num2 < _unsortedBiomeIdArray[j])
			{
				result.biomeId1 = (byte)j;
				num2 = _unsortedBiomeIdArray[j];
				if (num5 < num2)
				{
					num5 = num2;
				}
			}
		}
		_unsortedBiomeIdArray[result.biomeId1] = 0;
		for (int k = 0; k < _unsortedBiomeIdArray.Length; k++)
		{
			if (num3 < _unsortedBiomeIdArray[k])
			{
				result.biomeId2 = (byte)k;
				num3 = _unsortedBiomeIdArray[k];
				if (num5 < num3)
				{
					num5 = num3;
				}
			}
		}
		_unsortedBiomeIdArray[result.biomeId2] = 0;
		for (int l = 0; l < _unsortedBiomeIdArray.Length; l++)
		{
			if (num4 < _unsortedBiomeIdArray[l])
			{
				result.biomeId3 = (byte)l;
				num4 = _unsortedBiomeIdArray[l];
				if (num5 < num4)
				{
					num5 = num4;
				}
			}
		}
		_unsortedBiomeIdArray[result.biomeId3] = 0;
		result.intensity0 = (float)num / (float)num5;
		result.intensity1 = (float)num2 / (float)num5;
		result.intensity2 = (float)num3 / (float)num5;
		result.intensity3 = (float)num4 / (float)num5;
		return result;
	}

	public void ToArray(byte[] _array, int offs)
	{
		_array[offs] = biomeId0;
		_array[1 + offs] = biomeId1;
		_array[2 + offs] = biomeId2;
		_array[3 + offs] = biomeId3;
		_array[4 + offs] = intensity0and1;
		_array[5 + offs] = intensity2and3;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(biomeId0);
		_bw.Write((byte)(intensity0 * 255f));
		_bw.Write(biomeId1);
		_bw.Write((byte)(intensity1 * 255f));
		_bw.Write(biomeId2);
		_bw.Write((byte)(intensity2 * 255f));
		_bw.Write(biomeId3);
		_bw.Write((byte)(intensity3 * 255f));
	}

	public void Read(BinaryReader _br)
	{
		biomeId0 = _br.ReadByte();
		intensity0 = (float)(int)_br.ReadByte() / 255f;
		biomeId1 = _br.ReadByte();
		intensity1 = (float)(int)_br.ReadByte() / 255f;
		biomeId2 = _br.ReadByte();
		intensity2 = (float)(int)_br.ReadByte() / 255f;
		biomeId3 = _br.ReadByte();
		intensity3 = (float)(int)_br.ReadByte() / 255f;
	}

	public bool Equals(BiomeIntensity other)
	{
		if (biomeId0 == other.biomeId0 && biomeId1 == other.biomeId1 && biomeId2 == other.biomeId2 && biomeId3 == other.biomeId3 && intensity0and1 == other.intensity0and1)
		{
			return intensity2and3 == other.intensity2and3;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is BiomeIntensity)
		{
			return Equals((BiomeIntensity)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((((biomeId0.GetHashCode() * 397) ^ biomeId1.GetHashCode()) * 397) ^ biomeId2.GetHashCode()) * 397) ^ biomeId3.GetHashCode()) * 397) ^ intensity0and1.GetHashCode()) * 397) ^ intensity2and3.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("[b0={0} b1={1} i0={2} i1={3}]", biomeId0, biomeId1, intensity0.ToCultureInvariantString("0.0"), intensity1.ToCultureInvariantString("0.0"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static BiomeIntensity()
	{
		Default = new BiomeIntensity(0);
	}
}
