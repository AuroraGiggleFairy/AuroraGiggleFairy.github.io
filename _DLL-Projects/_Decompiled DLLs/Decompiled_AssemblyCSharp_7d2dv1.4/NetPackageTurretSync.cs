using UnityEngine.Scripting;

[Preserve]
public class NetPackageTurretSync : NetPackage
{
	public int entityId;

	public int targetEntityId;

	public bool isOn;

	public ItemValue itemValue;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageTurretSync Setup(int _entityId, int _targetEntityId, bool _isOn, ItemValue _originalItemValue)
	{
		entityId = _entityId;
		targetEntityId = _targetEntityId;
		isOn = _isOn;
		itemValue = _originalItemValue;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		targetEntityId = _reader.ReadInt32();
		isOn = _reader.ReadBoolean();
		itemValue = ItemValue.None.Clone();
		itemValue.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(targetEntityId);
		_writer.Write(isOn);
		itemValue.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityTurret entityTurret = GameManager.Instance.World.GetEntity(entityId) as EntityTurret;
			if (entityTurret != null)
			{
				entityTurret.TargetEntityId = targetEntityId;
				entityTurret.OriginalItemValue = itemValue;
				entityTurret.IsOn = isOn;
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
