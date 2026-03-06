using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetPartActive : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool active;

	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	public NetPackageEntitySetPartActive Setup(Entity entity, string partName, bool active)
	{
		id = entity.entityId;
		this.active = active;
		this.partName = partName;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		id = _br.ReadInt32();
		active = _br.ReadBoolean();
		partName = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(id);
		_bw.Write(active);
		_bw.Write(partName ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			Entity entity = _world.GetEntity(id);
			if (entity == null)
			{
				Log.Out("Discarding " + GetType().Name);
			}
			else if (string.IsNullOrEmpty(partName))
			{
				Log.Out("Discarding " + GetType().Name + " unexpected no part name");
			}
			else
			{
				entity.SetTransformActive(partName, active);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
