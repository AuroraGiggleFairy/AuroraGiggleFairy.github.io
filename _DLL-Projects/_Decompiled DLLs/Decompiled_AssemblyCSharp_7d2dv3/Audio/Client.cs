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

	public void Play(int playOnEntityId, string soundGroupName, float occlusion, float volumeScale = 1f)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(playOnEntityId, soundGroupName, occlusion, _play: true, _signalOnly: false, volumeScale);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
	}

	public void Play(Vector3 position, string soundGroupName, float occlusion, int entityId = -1, float volumeScale = 1f)
	{
		NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, occlusion, _play: true, entityId, volumeScale);
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
