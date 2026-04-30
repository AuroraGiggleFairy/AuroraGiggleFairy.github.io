using System;
using UnityEngine;

namespace Audio;

public class Client : IDisposable
{
	public int entityId;

	public Client(int _entityId)
	{
		entityId = _entityId;
	}

	public void Dispose()
	{
	}

	public void Play(int playOnEntityId, string soundGoupName, float _occlusion)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(playOnEntityId, soundGoupName, _occlusion, _play: true);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
	}

	public void Play(Vector3 position, string soundGoupName, float _occlusion, int entityId = -1)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGoupName, _occlusion, _play: true, entityId);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, this.entityId);
	}

	public void Stop(int stopOnEntityId, string soundGroupName)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(stopOnEntityId, soundGroupName, 0f, _play: false);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
	}

	public void Stop(Vector3 position, string soundGroupName)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, 0f, _play: false);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
	}
}
