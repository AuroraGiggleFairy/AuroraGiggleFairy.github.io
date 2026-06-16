using UnityEngine.Scripting;

[Preserve]
public abstract class NetPackageEntityTargeted : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Setup(int _entityId)
	{
		entityId = _entityId;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
	}

	public override bool ShouldProcess(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return true;
		}
		if (_world.netEntityPackageQueue == null || _world.entityAsyncManager == null)
		{
			return true;
		}
		if (!_world.netEntityPackageQueue.HasPackagesForEntity(entityId) && !_world.entityAsyncManager.IsEntityPending(entityId))
		{
			return true;
		}
		return false;
	}

	public override void HandleSkipped(World _world, GameManager _callbacks)
	{
		_world.netEntityPackageQueue.EnqueueNetPackageForEntity(entityId, this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public NetPackageEntityTargeted()
	{
	}
}
