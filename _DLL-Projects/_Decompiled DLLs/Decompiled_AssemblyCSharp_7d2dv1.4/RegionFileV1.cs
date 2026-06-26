using System;
using System.IO;

public class RegionFileV1 : RegionFileSectorBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_HEADER_SIZE = 4;

	public RegionFileV1(string _fullFilePath, int _rX, int _rZ, Stream _fileStream, int _version)
		: base(_fullFilePath, _rX, _rZ, _version)
	{
		try
		{
			if (_fileStream == null)
			{
				_fileStream = SdFile.Open(_fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				byte[] array = new byte[8196];
				array[0] = RegionFileSectorBased.FileHeaderMagicBytes[0];
				array[1] = RegionFileSectorBased.FileHeaderMagicBytes[1];
				array[2] = RegionFileSectorBased.FileHeaderMagicBytes[2];
				array[3] = 0;
				_fileStream.Write(array, 0, array.Length);
				_fileStream.Seek(0L, SeekOrigin.Begin);
			}
			_fileStream.Seek(4L, SeekOrigin.Begin);
			_fileStream.Read(regionLocationHeader, 0, 4096);
			_fileStream.Read(regionTimestampHeader, 0, 4096);
		}
		finally
		{
			_fileStream?.Dispose();
		}
	}

	public override void SaveHeaderData()
	{
		lock (this)
		{
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			stream.Write(RegionFileSectorBased.FileHeaderMagicBytes, 0, 3);
			stream.WriteByte(0);
			stream.Write(regionLocationHeader, 0, 4096);
			stream.Write(regionTimestampHeader, 0, 4096);
		}
	}

	public override void ReadData(int _cX, int _cZ, Stream _targetStream)
	{
		lock (this)
		{
			GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _);
			using Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			stream.Seek(_sectorOffset * 4096 + 4, SeekOrigin.Begin);
			int num = StreamUtils.ReadInt32(stream);
			stream.ReadByte();
			long position = _targetStream.Position;
			if (num > 0)
			{
				_targetStream.SetLength(0L);
			}
			try
			{
				StreamUtils.StreamCopy(stream, _targetStream, num, tempReadBuffer);
			}
			catch (NotSupportedException)
			{
				Log.Error("Chunk length: {0}, pos before: {1}", num, position);
				throw;
			}
		}
	}

	public override void WriteData(int _cX, int _cZ, int _dataLength, byte _compression, byte[] _data, bool _saveHeaderToFile)
	{
		lock (this)
		{
			uint timeStamp = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
			using (Stream stream = SdFile.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				stream.ReadByte();
				GetLocationInfo(_cX, _cZ, out var _sectorOffset, out var _sectorLength);
				if (_sectorOffset == 0)
				{
					_sectorOffset = (short)Math.Ceiling((double)stream.Length / 4096.0);
				}
				byte b = (byte)Math.Ceiling((decimal)_dataLength / 4096m);
				if (b <= _sectorLength)
				{
					SetLocationInfo(_cX, _cZ, _sectorOffset, b);
					SetTimestampInfo(_cX, _cZ, timeStamp);
					stream.Seek(_sectorOffset * 4096 + 4, SeekOrigin.Begin);
				}
				else if (_sectorOffset == (short)Math.Ceiling((double)stream.Length / 4096.0) - b)
				{
					SetLocationInfo(_cX, _cZ, _sectorOffset, b);
					SetTimestampInfo(_cX, _cZ, timeStamp);
					stream.Seek(_sectorOffset * 4096 + 4, SeekOrigin.Begin);
				}
				else
				{
					SetLocationInfo(_cX, _cZ, (short)Math.Ceiling((double)stream.Length / 4096.0), b);
					SetTimestampInfo(_cX, _cZ, timeStamp);
					stream.Seek((short)Math.Ceiling((double)stream.Length / 4096.0) * 4096 + 4, SeekOrigin.Begin);
				}
				StreamUtils.Write(stream, _dataLength);
				stream.WriteByte(0);
				stream.Write(_data, 0, _dataLength);
				for (int i = _dataLength; i < b * 4096; i++)
				{
					stream.WriteByte(0);
				}
			}
			if (_saveHeaderToFile)
			{
				SaveHeaderData();
			}
		}
	}

	public override void OptimizeLayout()
	{
	}

	public override int ChunkCount()
	{
		return -1;
	}
}
