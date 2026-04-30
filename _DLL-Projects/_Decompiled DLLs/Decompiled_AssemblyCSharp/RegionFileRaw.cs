using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class RegionFileRaw : RegionFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentVersion = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static readonly byte[] FileHeaderMagicBytes = Encoding.ASCII.GetBytes("7rr");

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int FileHeaderMagicBytesLength = 3;

	public const int ChunksPerRegionPerDimension = 8;

	public const int ChunksPerRegion = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int fileHeaderLength = 11;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int locationHeaderLength = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int timestampHeaderLength = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int sectorsStartOffset = 779;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int reservedBytesPerEntry = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] locationHeader = new int[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly uint[] timestampHeader = new uint[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int paddingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int version;

	[PublicizedFrom(EAccessModifier.Private)]
	public SortedDictionary<int, int> usedSectors = new SortedDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileRaw(string fullFilePath, int rX, int rZ, int paddingBytes, int version)
		: base(fullFilePath, rX, rZ)
	{
		this.paddingBytes = paddingBytes;
		this.version = version;
	}

	public static RegionFileRaw Load(string fullFilePath, int rX, int rZ)
	{
		using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		for (int i = 0; i < 3; i++)
		{
			if (FileHeaderMagicBytes[i] != stream.ReadByte())
			{
				throw new Exception("Incorrect header: " + fullFilePath);
			}
		}
		int num = StreamUtils.ReadInt32(stream);
		int num2 = StreamUtils.ReadInt32(stream);
		RegionFileRaw regionFileRaw = new RegionFileRaw(fullFilePath, rX, rZ, num2, num);
		ReadBytes(stream, regionFileRaw.locationHeader, 0, regionFileRaw.locationHeader.Length);
		ReadBytes(stream, regionFileRaw.timestampHeader, 0, regionFileRaw.timestampHeader.Length);
		regionFileRaw.InitUsedSectors();
		regionFileRaw.Length = stream.Length;
		return regionFileRaw;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitUsedSectors()
	{
		lock (this)
		{
			usedSectors.Clear();
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					GetLocationInfo(j, i, out var offset, out var length);
					if (offset > 0 && length > 0)
					{
						usedSectors.Add(offset, length);
					}
				}
			}
		}
	}

	public static RegionFileRaw New(string fullFilePath, int rX, int rZ, int paddingBytes)
	{
		using Stream stream = SdFile.Open(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		RegionFileRaw regionFileRaw = new RegionFileRaw(fullFilePath, rX, rZ, paddingBytes, 1);
		stream.Write(FileHeaderMagicBytes, 0, 3);
		StreamUtils.Write(stream, 1);
		StreamUtils.Write(stream, paddingBytes);
		if (stream.Position != 11)
		{
			throw new Exception($"Unexpected header length written. Expected: {11}, Actual: {stream.Position}");
		}
		WriteBytes(stream, regionFileRaw.locationHeader, 0, regionFileRaw.locationHeader.Length);
		WriteBytes(stream, regionFileRaw.timestampHeader, 0, regionFileRaw.timestampHeader.Length);
		regionFileRaw.Length = stream.Length;
		return regionFileRaw;
	}

	public override void ReadData(int cX, int cZ, Stream targetStream)
	{
		lock (this)
		{
			GetLocationInfo(cX, cZ, out var offset, out var length);
			if (offset < 779)
			{
				throw new Exception($"Reading outside of allowed bounds. R={regionX}/{regionZ}, C={cX}/{cZ}, Off={offset}, Len={length}");
			}
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			stream.Seek(offset, SeekOrigin.Begin);
			int num = StreamUtils.ReadInt32(stream);
			if (num > 0)
			{
				targetStream.SetLength(0L);
			}
			StreamUtils.StreamCopy(stream, targetStream, num, tempReadBuffer);
		}
	}

	public override void WriteData(int cX, int cZ, int dataLength, byte compression, byte[] data, bool saveHeaderToFile)
	{
		lock (this)
		{
			uint timeStamp = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
			int num = dataLength + 4;
			GetLocationInfo(cX, cZ, out var offset, out var length);
			if (offset == 0 || num > length)
			{
				num += paddingBytes;
				if (offset > 0)
				{
					usedSectors.Remove(offset);
				}
				offset = FindBestFreeSpace(num);
				SetLocationInfo(cX, cZ, offset, num);
			}
			else
			{
				num = length;
			}
			if (offset < 779)
			{
				throw new Exception($"Cannot write to protected offset: R={regionX}/{regionZ}, C={cX}/{cZ}, Off={offset}, Len={num}, DataLen={dataLength}");
			}
			SetTimestampInfo(cX, cZ, timeStamp);
			int num2 = offset + num;
			using (Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				stream.Seek(offset, SeekOrigin.Begin);
				StreamUtils.Write(stream, dataLength);
				stream.Write(data, 0, dataLength);
				int num3 = num - 4 - dataLength;
				if (num3 > 0)
				{
					stream.Seek(num3 - 1, SeekOrigin.Current);
					stream.WriteByte(0);
				}
				if (num2 != stream.Position)
				{
					throw new Exception($"Wrong write end: R={regionX}/{regionZ}, C={cX}/{cZ}, Off={offset}, Len={num}, DataLen={dataLength}, FOEndExp={num2}, FOEndFound={stream.Position}");
				}
				base.Length = stream.Length;
			}
			if (saveHeaderToFile)
			{
				SaveHeaderData();
			}
		}
	}

	public override void SaveHeaderData()
	{
		lock (this)
		{
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			stream.Seek(11L, SeekOrigin.Begin);
			WriteBytes(stream, locationHeader, 0, locationHeader.Length);
			WriteBytes(stream, timestampHeader, 0, timestampHeader.Length);
		}
	}

	public override bool HasChunk(int cX, int cZ)
	{
		GetLocationInfo(cX, cZ, out var offset, out var length);
		if (offset > 0)
		{
			return length > 0;
		}
		return false;
	}

	public override void RemoveChunk(int cX, int cZ)
	{
		lock (this)
		{
			GetLocationInfo(cX, cZ, out var offset, out var length);
			if (offset > 0 && length > 0)
			{
				usedSectors.Remove(offset);
			}
			SetLocationInfo(cX, cZ, 0, 0);
		}
	}

	public override int ChunkCount()
	{
		lock (this)
		{
			int num = 0;
			foreach (int value in usedSectors.Values)
			{
				if (value > 0)
				{
					num++;
				}
			}
			return num;
		}
	}

	public override int GetChunkByteCount(int cX, int cZ)
	{
		GetLocationInfo(cX, cZ, out var _, out var length);
		return length;
	}

	public override void GetTimestampInfo(int cX, int cZ, out uint timeStamp)
	{
		lock (this)
		{
			long offsetFromXz = GetOffsetFromXz(cX, cZ);
			timeStamp = timestampHeader[offsetFromXz];
		}
	}

	public override void SetTimestampInfo(int cX, int cZ, uint timeStamp)
	{
		long offsetFromXz = GetOffsetFromXz(cX, cZ);
		timestampHeader[offsetFromXz] = timeStamp;
	}

	public override void OptimizeLayout()
	{
		lock (this)
		{
			usedSectors.Clear();
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			pooledExpandableMemoryStream.Seek(779L, SeekOrigin.Begin);
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					GetLocationInfo(j, i, out var offset, out var length);
					if (offset > 0 && length > 0)
					{
						stream.Seek(offset, SeekOrigin.Begin);
						int offset2 = (int)pooledExpandableMemoryStream.Position;
						StreamUtils.StreamCopy(stream, pooledExpandableMemoryStream, length);
						SetLocationInfo(j, i, offset2, length);
					}
				}
			}
			stream.Seek(11L, SeekOrigin.Begin);
			WriteBytes(stream, locationHeader, 0, locationHeader.Length);
			WriteBytes(stream, timestampHeader, 0, timestampHeader.Length);
			pooledExpandableMemoryStream.Seek(779L, SeekOrigin.Begin);
			StreamUtils.StreamCopy(pooledExpandableMemoryStream, stream);
			stream.SetLength(pooledExpandableMemoryStream.Length);
			base.Length = stream.Length;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetLocationInfo(int cX, int cZ, out int offset, out int length)
	{
		lock (this)
		{
			long num = GetOffsetFromXz(cX, cZ) * 2;
			offset = locationHeader[num];
			length = locationHeader[num + 1];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLocationInfo(int cX, int cZ, int offset, int length)
	{
		lock (this)
		{
			long num = GetOffsetFromXz(cX, cZ) * 2;
			locationHeader[num] = offset;
			locationHeader[num + 1] = length;
			if (offset > 0)
			{
				usedSectors[offset] = length;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long GetOffsetFromXz(int _cX, int _cZ)
	{
		int num = _cX % 8;
		int num2 = _cZ % 8;
		if (_cX < 0)
		{
			num += 7;
		}
		if (_cZ < 0)
		{
			num2 += 7;
		}
		return num + num2 * 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindBestFreeSpace(int requiredLength)
	{
		lock (this)
		{
			int num = 779;
			int num2 = -1;
			int num3 = int.MaxValue;
			foreach (KeyValuePair<int, int> usedSector in usedSectors)
			{
				int num4 = usedSector.Key - num;
				int num5 = num4 - requiredLength;
				if (num5 == 0)
				{
					return num;
				}
				if (num4 > requiredLength && num5 < num3 - requiredLength)
				{
					num2 = num;
					num3 = num4;
				}
				num = usedSector.Key + usedSector.Value;
			}
			if (num2 > 0)
			{
				return num2;
			}
			return num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteBytes<T>(Stream stream, T[] buf, int offset, int length) where T : unmanaged
	{
		ReadOnlySpan<byte> buffer = MemoryMarshal.Cast<T, byte>(buf.AsSpan(offset, length));
		stream.Write(buffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReadBytes<T>(Stream stream, T[] buf, int offset, int length) where T : unmanaged
	{
		Span<byte> span = MemoryMarshal.Cast<T, byte>(buf.AsSpan(offset, length));
		while (span.Length > 0)
		{
			int num = stream.Read(span);
			if (num <= 0)
			{
				throw new EndOfStreamException();
			}
			Span<byte> span2 = span;
			int num2 = num;
			span = span2.Slice(num2, span2.Length - num2);
		}
	}
}
