using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetSkillLevelClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string skill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int level;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntitySetSkillLevelClient Setup(int _entityId, string _skill, int _level)
	{
		entityId = _entityId;
		skill = _skill;
		level = _level;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		skill = _reader.ReadString();
		level = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(skill);
		_writer.Write(level);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(entityId);
			if (entityPlayer != null)
			{
				entityPlayer.Progression.GetProgressionValue(skill).Level = level;
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
