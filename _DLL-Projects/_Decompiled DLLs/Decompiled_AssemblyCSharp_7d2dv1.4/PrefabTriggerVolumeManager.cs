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

	public void AddTriggerVolumeServer(Vector3i _hitPointBlockPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _hitPointBlockPos);
				if (prefabInstance != null)
				{
					Vector3i size = new Vector3i(15, 3, 15);
					int num = prefabInstance.prefab.AddTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(size.x / 2, 0, size.z / 2), size);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.TriggerVolumes[num]));
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddTriggerVolume>().Setup(_hitPointBlockPos));
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
