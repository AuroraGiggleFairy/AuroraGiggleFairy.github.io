using System.Collections.Generic;

public class PrefabSleeperVolumeManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabSleeperVolumeManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> clientPrefabs = new List<PrefabInstance>();

	public static PrefabSleeperVolumeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PrefabSleeperVolumeManager();
			}
			return instance;
		}
	}

	public void Cleanup()
	{
		clientPrefabs.Clear();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.OnPrefabLoaded -= PrefabLoadedServer;
			dynamicPrefabDecorator.OnPrefabChanged -= PrefabChangedServer;
			dynamicPrefabDecorator.OnPrefabRemoved -= PrefabRemovedServer;
		}
		PrefabEditModeManager.Instance.OnPrefabChanged -= PrefabChangedServer;
	}

	public void StartAsServer()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		dynamicPrefabDecorator.OnPrefabLoaded += PrefabLoadedServer;
		dynamicPrefabDecorator.OnPrefabChanged += PrefabChangedServer;
		dynamicPrefabDecorator.OnPrefabRemoved += PrefabRemovedServer;
		PrefabEditModeManager.Instance.OnPrefabChanged += PrefabChangedServer;
		GameManager.Instance.OnClientSpawned += SendAllPrefabs;
	}

	public void StartAsClient()
	{
		clientPrefabs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendAllPrefabs(ClientInfo _toClient)
	{
		if (_toClient == null)
		{
			return;
		}
		foreach (PrefabInstance dynamicPrefab in GameManager.Instance.GetDynamicPrefabDecorator().GetDynamicPrefabs())
		{
			_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, dynamicPrefab));
			for (int i = 0; i < dynamicPrefab.prefab.SleeperVolumes.Count; i++)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, dynamicPrefab.id, i, dynamicPrefab.prefab.SleeperVolumes[i]));
			}
			for (int j = 0; j < dynamicPrefab.prefab.TeleportVolumes.Count; j++)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, dynamicPrefab.id, j, dynamicPrefab.prefab.TeleportVolumes[j]));
			}
			for (int k = 0; k < dynamicPrefab.prefab.InfoVolumes.Count; k++)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, dynamicPrefab.id, k, dynamicPrefab.prefab.InfoVolumes[k]));
			}
			for (int l = 0; l < dynamicPrefab.prefab.WallVolumes.Count; l++)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, dynamicPrefab.id, l, dynamicPrefab.prefab.WallVolumes[l]));
			}
			for (int m = 0; m < dynamicPrefab.prefab.TriggerVolumes.Count; m++)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, dynamicPrefab.id, m, dynamicPrefab.prefab.TriggerVolumes[m]));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabLoadedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, _prefabInstance));
	}

	public void PrefabLoadedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename);
		PrefabInstance prefabInstance = new PrefabInstance(_prefabInstanceId, location, _boundingBoxPosition, 0, null, 0)
		{
			boundingBoxSize = _boundingBoxSize,
			name = _prefabInstanceName,
			prefab = new Prefab
			{
				size = _prefabSize,
				location = location,
				yOffset = _yOffset
			}
		};
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.CreateBoundingBox(_alsoCreateOtherBoxes: false);
		clientPrefabs.Add(prefabInstance);
		if (clientPrefabs.Count == 1)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabChangedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Changed, _prefabInstance));
	}

	public void PrefabChangedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PrefabInstance prefabInstance = GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
			return;
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename);
		prefabInstance.boundingBoxPosition = _boundingBoxPosition;
		prefabInstance.boundingBoxSize = _boundingBoxSize;
		prefabInstance.name = _prefabInstanceName;
		prefabInstance.prefab.size = _prefabSize;
		prefabInstance.prefab.location = location;
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.prefab.yOffset = _yOffset;
		prefabInstance.CreateBoundingBox(_alsoCreateOtherBoxes: false);
		if (clientPrefabs.IndexOf(prefabInstance) == 0)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabRemovedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Removed, _prefabInstance));
	}

	public void PrefabRemovedClient(int _prefabInstanceId)
	{
		for (int i = 0; i < clientPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = clientPrefabs[i];
			if (prefabInstance.id == _prefabInstanceId)
			{
				clientPrefabs.RemoveAt(i);
				for (int j = 0; j < prefabInstance.prefab.SleeperVolumes.Count; j++)
				{
					Prefab.PrefabSleeperVolume prefabSleeperVolume = prefabInstance.prefab.SleeperVolumes[j];
					prefabSleeperVolume.used = false;
					prefabInstance.prefab.SetSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, j, prefabSleeperVolume);
				}
				for (int k = 0; k < prefabInstance.prefab.TeleportVolumes.Count; k++)
				{
					Prefab.PrefabTeleportVolume volumeSettings = prefabInstance.prefab.TeleportVolumes[k];
					prefabInstance.prefab.SetTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, k, volumeSettings);
				}
				for (int l = 0; l < prefabInstance.prefab.InfoVolumes.Count; l++)
				{
					Prefab.PrefabInfoVolume volumeSettings2 = prefabInstance.prefab.InfoVolumes[l];
					prefabInstance.prefab.SetInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, l, volumeSettings2);
				}
				for (int m = 0; m < prefabInstance.prefab.WallVolumes.Count; m++)
				{
					Prefab.PrefabWallVolume volumeSettings3 = prefabInstance.prefab.WallVolumes[m];
					prefabInstance.prefab.SetWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, m, volumeSettings3);
				}
				for (int n = 0; n < prefabInstance.prefab.TriggerVolumes.Count; n++)
				{
					Prefab.PrefabTriggerVolume volumeSettings4 = prefabInstance.prefab.TriggerVolumes[n];
					prefabInstance.prefab.SetTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, n, volumeSettings4);
				}
				break;
			}
		}
	}

	public void AddSleeperVolumeServer(Vector3i _startPos, Vector3i _size)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _startPos + new Vector3i(_size.x / 2, 0, _size.z / 2));
				if (prefabInstance != null)
				{
					int num = prefabInstance.prefab.AddSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _startPos - prefabInstance.boundingBoxPosition, _size, 0, "GroupGenericZombie", 5, 6);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.SleeperVolumes[num]));
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddSleeperVolume>().Setup(_startPos, _size));
		}
	}

	public void UpdateSleeperPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volumeSettings)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdateSleeperPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdateSleeperPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volumeSettings)
	{
		PrefabInstance prefabInstance = GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
			return;
		}
		prefabInstance.prefab.SetSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings);
		XUiC_WoPropsSleeperVolume.SleeperVolumeChanged(_prefabInstanceId, _volumeId);
	}

	public PrefabInstance GetPrefabInstance(int _prefabId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (GameManager.Instance.GetDynamicPrefabDecorator() == null)
			{
				return null;
			}
			return GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(_prefabId);
		}
		foreach (PrefabInstance clientPrefab in clientPrefabs)
		{
			if (clientPrefab.id == _prefabId)
			{
				return clientPrefab;
			}
		}
		return null;
	}
}
