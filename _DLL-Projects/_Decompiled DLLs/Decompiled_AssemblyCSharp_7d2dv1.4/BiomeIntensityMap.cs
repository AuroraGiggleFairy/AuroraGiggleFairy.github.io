using System;
using System.IO;

public class BiomeIntensityMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayWithOffset<BiomeIntensity> intensities;

	public BiomeIntensityMap()
	{
	}

	public BiomeIntensityMap(int _w, int _h)
	{
		intensities = new ArrayWithOffset<BiomeIntensity>(_w, _h);
	}

	public void Load(string _worldName)
	{
		try
		{
			string path = PathAbstractions.WorldsSearchPaths.GetLocation(_worldName).FullPath + "/biomeintensity.dat";
			if (!SdFile.Exists(path))
			{
				intensities = null;
				return;
			}
			using Stream baseStream = SdFile.Open(path, FileMode.Open);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			pooledBinaryReader.ReadByte();
			pooledBinaryReader.ReadByte();
			pooledBinaryReader.ReadByte();
			pooledBinaryReader.ReadByte();
			pooledBinaryReader.ReadByte();
			int num = pooledBinaryReader.ReadUInt16();
			int num2 = pooledBinaryReader.ReadUInt16();
			intensities = new ArrayWithOffset<BiomeIntensity>(num, num2);
			num /= 2;
			num2 /= 2;
			for (int i = -num; i < num; i++)
			{
				for (int j = -num2; j < num2; j++)
				{
					BiomeIntensity value = default(BiomeIntensity);
					value.Read(pooledBinaryReader);
					intensities[i, j] = value;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Reading biome intensity map: " + ex.Message);
		}
	}

	public void Save(string _worldName)
	{
		try
		{
			using Stream baseStream = SdFile.Open(PathAbstractions.WorldsSearchPaths.GetLocation(_worldName).FullPath + "/biomeintensity.dat", FileMode.Create);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write((byte)66);
			pooledBinaryWriter.Write((byte)73);
			pooledBinaryWriter.Write((byte)73);
			pooledBinaryWriter.Write((byte)0);
			pooledBinaryWriter.Write((byte)1);
			int dimX = intensities.DimX;
			int dimY = intensities.DimY;
			pooledBinaryWriter.Write((ushort)dimX);
			pooledBinaryWriter.Write((ushort)dimY);
			dimX /= 2;
			dimY /= 2;
			for (int i = -dimX; i < dimX; i++)
			{
				for (int j = -dimY; j < dimY; j++)
				{
					intensities[i, j].Write(pooledBinaryWriter);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Writing biome intensity map: " + ex.Message);
		}
	}

	public void SetBiomeIntensity(int _x, int _y, BiomeIntensity _bi)
	{
		if (intensities != null && intensities.Contains(_x, _y))
		{
			intensities[_x, _y] = _bi;
		}
	}

	public BiomeIntensity GetBiomeIntensity(int _x, int _y)
	{
		if (intensities != null && intensities.Contains(_x, _y))
		{
			return intensities[_x, _y];
		}
		return BiomeIntensity.Default;
	}
}
