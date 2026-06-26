using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetBlockResponse : NetPackage
{
	public eSetBlockResponse response;

	public NetPackageSetBlockResponse Setup(eSetBlockResponse _response)
	{
		response = _response;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		response = (eSetBlockResponse)_reader.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((ushort)response);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		switch (response)
		{
		case eSetBlockResponse.PowerBlockLimitExceeded:
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddpowerblock");
			break;
		case eSetBlockResponse.StorageBlockLimitExceeded:
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddstorageblock");
			break;
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
