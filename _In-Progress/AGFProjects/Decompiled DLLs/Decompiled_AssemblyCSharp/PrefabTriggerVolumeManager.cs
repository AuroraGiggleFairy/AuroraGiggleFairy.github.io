public class PrefabTriggerVolumeManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabTriggerVolumeManager instance;

	public static PrefabTriggerVolumeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PrefabTriggerVolumeManager();
			}
			return instance;
		}
	}

	public void Cleanup()
	{
		GUIWindowDynamicPrefabMenu.Cleanup();
	}

	public void AddTriggerVolumeServer(Vector3i _startPos, Vector3i _size)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _startPos + new Vector3i(_size.x / 2, 0, _size.z / 2));
				if (prefabInstance != null)
				{
					int num = prefabInstance.prefab.AddTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _startPos - prefabInstance.boundingBoxPosition, _size);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.TriggerVolumes[num]));
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddTriggerVolume>().Setup(_startPos, _size));
		}
	}

	public void UpdateTriggerPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdateTriggerPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdateTriggerPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
		}
		else
		{
			prefabInstance.prefab.SetTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
		}
	}
}
