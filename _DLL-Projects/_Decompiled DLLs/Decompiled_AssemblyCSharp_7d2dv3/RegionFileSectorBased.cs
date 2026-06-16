using System;
using System.IO;
using System.Text;

public abstract class RegionFileSectorBased : RegionFile
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int CURRENT_VERSION = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static readonly byte[] FileHeaderMagicBytes = Encoding.ASCII.GetBytes("7rg");

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int FileHeaderMagicBytesLength = 3;

	public const int ChunksPerRegionPerDimension = 32;

	public const int ChunksPerRegion = 1024;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int SECTOR_SIZE = 4096;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly byte[] regionLocationHeader = new byte[4096];

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly byte[] regionTimestampHeader = new byte[4096];

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly int Version;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RegionFileSectorBased(string _fullFilePath, int _rX, int _rZ, int _version)
		: base(_fullFilePath, _rX, _rZ)
	{
		Version = _version;
	}

	public static RegionFile Get(string dir, int rX, int rZ, string ext)
	{
		string text = RegionFile.ConstructFullFilePath(dir, rX, rZ, ext);
		if (!SdFile.Exists(text))
		{
			SdFile.Create(text).Close();
			return new RegionFileV2(text, rX, rZ, null, 1);
		}
		Stream stream = SdFile.Open(text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		byte[] array = new byte[3];
		stream.Read(array, 0, 3);
		for (int i = 0; i < 3; i++)
		{
			if (array[i] != FileHeaderMagicBytes[i])
			{
				throw new Exception("Incorrect region file header! " + text);
			}
		}
		int num = (byte)stream.ReadByte();
		if (num < 1)
		{
			return new RegionFileV1(text, rX, rZ, stream, num);
		}
		return new RegionFileV2(text, rX, rZ, stream, num);
	}

	public override void GetTimestampInfo(int _cX, int _cZ, out uint _timeStamp)
	{
		lock (this)
		{
			long offsetFromXz = GetOffsetFromXz(_cX, _cZ);
			_timeStamp = BitConverter.ToUInt32(regionTimestampHeader, (int)offsetFromXz);
		}
	}

	public override void SetTimestampInfo(int _cX, int _cZ, uint _timeStamp)
	{
		lock (this)
		{
			long offsetFromXz = GetOffsetFromXz(_cX, _cZ);
			Utils.GetBytes(_timeStamp, regionTimestampHeader, (int)offsetFromXz);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void GetLocationInfo(int _cX, int _cZ, out short _sectorOffset, out byte _sectorLength)
	{
		lock (this)
		{
			long offsetFromXz = GetOffsetFromXz(_cX, _cZ);
			_sectorOffset = RegionFile.ToShort(regionLocationHeader[offsetFromXz], regionLocationHeader[offsetFromXz + 1]);
			_sectorLength = regionLocationHeader[offsetFromXz + 3];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetLocationInfo(int _cX, int _cZ, short _sectorOffset, byte _sectorLength)
	{
		lock (this)
		{
			long offsetFromXz = GetOffsetFromXz(_cX, _cZ);
			RegionFile.FromShort(_sectorOffset, out regionLocationHeader[offsetFromXz], out regionLocationHeader[offsetFromXz + 1]);
			regionLocationHeader[offsetFromXz + 3] = _sectorLength;
		}
	}

	public override bool HasChunk(int _cX, int _cZ)
	{
		GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
		if (_sectorOffset > 0)
		{
			return _sectorLength > 0;
		}
		return false;
	}

	public override int GetChunkByteCount(int _cX, int _cZ)
	{
		GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
		if (_sectorOffset <= 0 || _sectorLength <= 0)
		{
			return 0;
		}
		return _sectorLength * 4096;
	}

	public override void RemoveChunk(int _cX, int _cZ)
	{
		SetLocationInfo(_cX, _cZ, 0, 0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual long GetOffsetFromXz(int _cX, int _cZ)
	{
		int num = _cX % 32;
		int num2 = _cZ % 32;
		if (_cX < 0)
		{
			num += 31;
		}
		if (_cZ < 0)
		{
			num2 += 31;
		}
		return 4 * (num + num2 * 32);
	}
}
