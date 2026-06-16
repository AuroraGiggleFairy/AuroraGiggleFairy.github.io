using UnityEngine.Scripting;

[Preserve]
public class NetPackageSignDataResponse : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLastBatch;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public override bool Compress => true;

	public NetPackageSignDataResponse Setup(byte[] _data, bool _isLastBatch)
	{
		data = _data;
		isLastBatch = _isLastBatch;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		isLastBatch = _reader.ReadBoolean();
		int num = _reader.ReadInt32();
		data = ((num > 0) ? _reader.ReadBytes(num) : null);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(isLastBatch);
		int num = ((data != null) ? data.Length : 0);
		_writer.Write(num);
		if (num > 0)
		{
			_writer.Write(data);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SignDataManager.Instance.ProcessSignDataBatchReceived(data, isLastBatch);
	}

	public override int GetLength()
	{
		return 0;
	}
}
