public static class FireControllerUtils
{
	public static void SpawnParticleEffect(ParticleEffect _pe, int _entityId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!GameManager.IsDedicatedServer)
			{
				GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, _forceCreation: false, _worldSpawn: true);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation: false, _worldSpawn: true), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation: false, _worldSpawn: true));
		}
	}
}
