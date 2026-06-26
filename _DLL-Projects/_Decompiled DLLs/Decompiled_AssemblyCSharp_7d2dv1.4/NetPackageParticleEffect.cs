using UnityEngine.Scripting;

[Preserve]
public class NetPackageParticleEffect : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleEffect pe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityThatCausedIt;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceCreation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldSpawn;

	public NetPackageParticleEffect Setup(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false)
	{
		pe = _pe;
		entityThatCausedIt = _entityThatCausedIt;
		worldSpawn = _worldSpawn;
		forceCreation = _forceCreation;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		pe = new ParticleEffect();
		pe.Read(_br);
		entityThatCausedIt = _br.ReadInt32();
		forceCreation = _br.ReadBoolean();
		worldSpawn = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		pe.Write(_bw);
		_bw.Write(entityThatCausedIt);
		_bw.Write(forceCreation);
		_bw.Write(worldSpawn);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				_world.GetGameManager().SpawnParticleEffectServer(pe, entityThatCausedIt, forceCreation, worldSpawn);
			}
			else
			{
				_world.GetGameManager().SpawnParticleEffectClient(pe, entityThatCausedIt, forceCreation, worldSpawn);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
