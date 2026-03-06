using System;
using System.Collections.Generic;
using System.IO;

public class RegionFileV2 : RegionFileSectorBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedDictionary<int, int> usedSectors = new SortedDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ReservedSectors = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ReservedBytesPerEntry = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] emptyBytes = new byte[4096];

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream optimizerMemoryStream = new MemoryStream(16789504);

	public RegionFileV2(string _fullFilePath, int _rX, int _rZ, Stream _fileStream, int _version)
		: base(_fullFilePath, _rX, _rZ, _version)
	{
		try
		{
			if (_fileStream == null)
			{
				_fileStream = SdFile.Open(_fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				byte[] array = new byte[12288];
				array[0] = RegionFileSectorBased.FileHeaderMagicBytes[0];
				array[1] = RegionFileSectorBased.FileHeaderMagicBytes[1];
				array[2] = RegionFileSectorBased.FileHeaderMagicBytes[2];
				array[3] = (byte)_version;
				_fileStream.Write(array, 0, array.Length);
				_fileStream.Seek(0L, SeekOrigin.Begin);
			}
			_fileStream.Seek(4096L, SeekOrigin.Begin);
			_fileStream.Read(regionLocationHeader, 0, 4096);
			_fileStream.Read(regionTimestampHeader, 0, 4096);
			initSectorInfo();
			base.Length = _fileStream.Length;
		}
		finally
		{
			_fileStream?.Dispose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initSectorInfo()
	{
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				GetLocationInfo(j, i, out var _sectorOffset, out var _sectorLength);
				if (_sectorLength > 0)
				{
					usedSectors[_sectorOffset] = _sectorLength;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findFreeSectorOfSize(int _sectorLength)
	{
		lock (this)
		{
			int num = 3;
			foreach (KeyValuePair<int, int> usedSector in usedSectors)
			{
				if (usedSector.Key - num >= _sectorLength)
				{
					return num;
				}
				num = usedSector.Key + usedSector.Value;
			}
			return num;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetLocationInfo(int _cX, int _cZ, short _sectorOffset, byte _sectorLength)
	{
		lock (this)
		{
			base.SetLocationInfo(_cX, _cZ, _sectorOffset, _sectorLength);
			usedSectors[_sectorOffset] = _sectorLength;
		}
	}

	public override void SaveHeaderData()
	{
		lock (this)
		{
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			stream.Seek(4096L, SeekOrigin.Begin);
			stream.Write(regionLocationHeader, 0, 4096);
			stream.Write(regionTimestampHeader, 0, 4096);
			base.Length = stream.Length;
		}
	}

	public override void ReadData(int _cX, int _cZ, Stream _targetStream)
	{
		lock (this)
		{
			GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			long num = _sectorOffset * 4096;
			int num2 = _sectorLength * 4096;
			stream.Seek(num, SeekOrigin.Begin);
			int num3 = StreamUtils.ReadInt32(stream);
			stream.Seek(12L, SeekOrigin.Current);
			long num4 = num2 + num;
			long position = _targetStream.Position;
			if (num3 > 0)
			{
				_targetStream.SetLength(0L);
			}
			if (_sectorOffset < 3)
			{
				Log.Error($"ChunkRead: R={regionX}/{regionZ}, C={_cX}/{_cZ}, Off={_sectorOffset}, Len={_sectorLength}, DataLen={num3}, TotalLen={num3 + 16}, FOStart={num}, FSize={num2}, FOEndExp={num4}, FOEndRead={stream.Position}, TSPosBefore={position}");
			}
			else
			{
				try
				{
					StreamUtils.StreamCopy(stream, _targetStream, num3, tempReadBuffer);
				}
				catch (NotSupportedException)
				{
					Log.Error($"ChunkRead: R={regionX}/{regionZ}, C={_cX}/{_cZ}, Off={_sectorOffset}, Len={_sectorLength}, DataLen={num3}, TotalLen={num3 + 16}, FOStart={num}, FSize={num2}, FOEndExp={num4}, FOEndRead={stream.Position}, TSPosBefore={position}");
					throw;
				}
			}
			base.Length = stream.Length;
		}
	}

	public override void WriteData(int _cX, int _cZ, int _dataLength, byte _compression, byte[] _data, bool _saveHeaderToFile)
	{
		lock (this)
		{
			uint timeStamp = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
			int num = _dataLength + 16;
			byte b = (byte)Math.Ceiling((double)num / 4096.0);
			using (Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
				if (_sectorOffset == 0 || b > _sectorLength)
				{
					if (_sectorOffset > 0)
					{
						usedSectors.Remove(_sectorOffset);
					}
					_sectorOffset = (short)findFreeSectorOfSize(b);
					SetLocationInfo(_cX, _cZ, _sectorOffset, b);
				}
				if (_sectorOffset < 3)
				{
					Log.Error($"Sector offset < 3: R={regionX}/{regionZ}, C={_cX}/{_cZ}, Off={_sectorOffset}, Len={b}, DataLen={_dataLength}");
				}
				SetTimestampInfo(_cX, _cZ, timeStamp);
				long num2 = _sectorOffset * 4096;
				int num3 = b * 4096;
				long num4 = num3 + num2;
				stream.Seek(num2, SeekOrigin.Begin);
				StreamUtils.Write(stream, _dataLength);
				stream.Seek(12L, SeekOrigin.Current);
				stream.Write(_data, 0, _dataLength);
				stream.Write(emptyBytes, 0, num3 - num);
				long position = stream.Position;
				if (position != num4)
				{
					Log.Error($"Wrong write end: R={regionX}/{regionZ}, C={_cX}/{_cZ}, Off={_sectorOffset}, Len={b}, DataLen={_dataLength}, TotalLen={num}, FOStart={num2}, FSize={num3}, FOEndExp={num4}, FOEndFound={position}");
				}
				base.Length = stream.Length;
			}
			if (_saveHeaderToFile)
			{
				SaveHeaderData();
			}
		}
	}

	public override void RemoveChunk(int _cX, int _cZ)
	{
		lock (this)
		{
			GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
			if (_sectorOffset > 0 && _sectorLength > 0)
			{
				usedSectors.Remove(_sectorOffset);
			}
			base.RemoveChunk(_cX, _cZ);
		}
	}

	public override void OptimizeLayout()
	{
		using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		int num = 3;
		foreach (int value in usedSectors.Values)
		{
			num += value;
		}
		int num2 = num * 4096;
		usedSectors.Clear();
		optimizerMemoryStream.Position = 0L;
		stream.Seek(0L, SeekOrigin.Begin);
		StreamUtils.StreamCopy(stream, optimizerMemoryStream, 12288);
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				GetLocationInfo(j, i, out var _sectorOffset, out var _sectorLength);
				if (_sectorOffset > 0 && _sectorLength > 0)
				{
					stream.Seek(_sectorOffset * 4096, SeekOrigin.Begin);
					short sectorOffset = (short)(optimizerMemoryStream.Position / 4096);
					StreamUtils.StreamCopy(stream, optimizerMemoryStream, _sectorLength * 4096);
					SetLocationInfo(j, i, sectorOffset, _sectorLength);
				}
			}
		}
		optimizerMemoryStream.Seek(4096L, SeekOrigin.Begin);
		optimizerMemoryStream.Write(regionLocationHeader, 0, 4096);
		stream.SetLength(num2);
		stream.Position = 0L;
		optimizerMemoryStream.Position = 0L;
		StreamUtils.StreamCopy(optimizerMemoryStream, stream, num2);
		base.Length = stream.Length;
	}

	public override int ChunkCount()
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
