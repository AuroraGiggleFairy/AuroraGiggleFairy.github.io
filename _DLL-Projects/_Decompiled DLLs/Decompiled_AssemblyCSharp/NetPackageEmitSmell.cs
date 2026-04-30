using UnityEngine.Scripting;

[Preserve]
public class NetPackageEmitSmell : NetPackage
{
	public int EntityId;

	public string SmellName;

	public NetPackageEmitSmell Setup(int entityId, string smellName)
	{
		EntityId = entityId;
		SmellName = smellName;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		EntityId = _reader.ReadInt32();
		SmellName = _reader.ReadString();
		if (string.IsNullOrEmpty(SmellName))
		{
			SmellName = null;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(EntityId);
		_writer.Write((SmellName != null) ? SmellName : "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
	}

	public override int GetLength()
	{
		return 10;
	}
}
