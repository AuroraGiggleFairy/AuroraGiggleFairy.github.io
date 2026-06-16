using System.Collections.Generic;
using PrefabVolumes;

public class PrefabInstanceClientManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabInstanceClientManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> clientPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> dynamicPrefabBuffer = new List<PrefabInstance>();

	public static PrefabInstanceClientManager Instance => instance ?? (instance = new PrefabInstanceClientManager());

	public void Cleanup()
	{
		clientPrefabs.Clear();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.OnPrefabLoaded -= prefabLoadedServer;
			dynamicPrefabDecorator.OnPrefabChanged -= prefabChangedServer;
			dynamicPrefabDecorator.OnPrefabRemoved -= prefabRemovedServer;
		}
		PrefabEditModeManager.Instance.OnPrefabChanged -= prefabChangedServer;
		GameManager.Instance.OnClientSpawned -= sendAllPrefabs;
	}

	public void StartAsServer()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		dynamicPrefabDecorator.OnPrefabLoaded += prefabLoadedServer;
		dynamicPrefabDecorator.OnPrefabChanged += prefabChangedServer;
		dynamicPrefabDecorator.OnPrefabRemoved += prefabRemovedServer;
		PrefabEditModeManager.Instance.OnPrefabChanged += prefabChangedServer;
		GameManager.Instance.OnClientSpawned += sendAllPrefabs;
	}

	public void StartAsClient()
	{
		clientPrefabs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendAllPrefabs(ClientInfo _toClient)
	{
		if (_toClient == null)
		{
			return;
		}
		dynamicPrefabBuffer.Clear();
		GameManager.Instance.GetDynamicPrefabDecorator().GetWorldPrefabs(dynamicPrefabBuffer);
		foreach (PrefabInstance item in dynamicPrefabBuffer)
		{
			_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, item));
			foreach (PrefabVolumeListAbs allVolumeList in item.prefab.AllVolumeLists)
			{
				allVolumeList.SendAllVolumesToClient(_toClient, item.id);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void prefabLoadedServer(PrefabInstance _prefabInstance)
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
	public void prefabChangedServer(PrefabInstance _prefabInstance)
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
	public void prefabRemovedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Removed, _prefabInstance));
	}

	public void PrefabRemovedClient(int _prefabInstanceId)
	{
		for (int i = 0; i < clientPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = clientPrefabs[i];
			if (prefabInstance.id != _prefabInstanceId)
			{
				continue;
			}
			clientPrefabs.RemoveAt(i);
			{
				foreach (PrefabVolumeListAbs allVolumeList in prefabInstance.prefab.AllVolumeLists)
				{
					allVolumeList.RemoveVolumes(prefabInstance);
				}
				break;
			}
		}
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
