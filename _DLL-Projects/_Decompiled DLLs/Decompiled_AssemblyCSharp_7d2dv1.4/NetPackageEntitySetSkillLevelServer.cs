using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetSkillLevelServer : NetPackageEntitySetSkillLevelClient
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public new NetPackageEntitySetSkillLevelServer Setup(int _entityId, string skill, int _experience)
	{
		base.Setup(_entityId, skill, _experience);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(entityId);
			if (!(entityPlayer == null) && entityPlayer.isEntityRemote)
			{
				entityPlayer.Progression.GetProgressionValue(skill).Level = level;
			}
		}
	}
}
