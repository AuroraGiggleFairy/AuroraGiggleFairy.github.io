using System.IO;
using System.Text;

public static class PlatformUserIdentifierExtensions
{
	public static void ToStream(this PlatformUserIdentifierAbs _instance, Stream _targetStream, bool _inclCustomData = false)
	{
		if (_targetStream == null)
		{
			return;
		}
		if (_instance == null)
		{
			_targetStream.WriteByte(0);
			return;
		}
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
		pooledBinaryWriter.SetBaseStream(_targetStream);
		_instance.ToStream(pooledBinaryWriter, _inclCustomData);
	}

	public static void ToStream(this PlatformUserIdentifierAbs _instance, BinaryWriter _writer, bool _inclCustomData = false)
	{
		if (_writer == null)
		{
			return;
		}
		if (_instance == null)
		{
			_writer.Write((byte)0);
			return;
		}
		_writer.Write((byte)1);
		_writer.Write((byte)1);
		_writer.Write(_instance.PlatformIdentifierString);
		_writer.Write(_instance.ReadablePlatformUserIdentifier);
		if (_inclCustomData)
		{
			_instance.WriteCustomData(_writer);
		}
	}

	public static int GetToStreamLength(this PlatformUserIdentifierAbs _instance, Encoding encoding, bool _inclCustomData = false)
	{
		if (_instance == null)
		{
			return 1;
		}
		return 0 + 1 + 1 + _instance.PlatformIdentifierString.GetBinaryWriterLength(encoding) + _instance.ReadablePlatformUserIdentifier.GetBinaryWriterLength(encoding) + _instance.GetCustomDataLengthEstimate();
	}
}
