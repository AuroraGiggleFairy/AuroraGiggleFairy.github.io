public class PrefabVolumeManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabVolumeManager instance;

	public static PrefabVolumeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PrefabVolumeManager();
			}
			return instance;
		}
	}

	public void Cleanup()
	{
		GUIWindowDynamicPrefabMenu.Cleanup();
	}

	public void AddTeleportVolumeServer(Vector3i _startPos, Vector3i _size)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return;
			}
			PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _startPos + new Vector3i(_size.x / 2, 0, _size.z / 2));
			if (prefabInstance != null)
			{
				if (!prefabInstance.prefab.bTraderArea)
				{
					(((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowMessage(Localization.Get("failed"), Localization.Get("xuiPrefabEditorTraderTeleportError"), XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, null, _openMainMenuOnClose: false);
					return;
				}
				int num = prefabInstance.prefab.AddTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _startPos - prefabInstance.boundingBoxPosition, _size);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.TeleportVolumes[num]));
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddTeleportVolume>().Setup(_startPos, _size));
		}
	}

	public void UpdateTeleportPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdateTeleportPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdateTeleportPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
		}
		else
		{
			prefabInstance.prefab.SetTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
		}
	}

	public void AddInfoVolumeServer(Vector3i _startPos, Vector3i _size)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _startPos + new Vector3i(_size.x / 2, 0, _size.z / 2));
				if (prefabInstance != null)
				{
					int num = prefabInstance.prefab.AddInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _startPos - prefabInstance.boundingBoxPosition, _size);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.InfoVolumes[num]));
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddInfoVolume>().Setup(_startPos, _size));
		}
	}

	public void UpdateInfoPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdateInfoPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdateInfoPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
		}
		else
		{
			prefabInstance.prefab.SetInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
		}
	}

	public void AddWallVolumeServer(Vector3i _startPos, Vector3i _size)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _startPos + new Vector3i(_size.x / 2, 0, _size.z / 2));
				if (prefabInstance != null)
				{
					int num = prefabInstance.prefab.AddWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _startPos - prefabInstance.boundingBoxPosition, _size);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.WallVolumes[num]));
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddWallVolume>().Setup(_startPos, _size));
		}
	}

	public void UpdateWallPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabWallVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdateWallPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup((!remove) ? NetPackageEditorSleeperVolume.EChangeType.Changed : NetPackageEditorSleeperVolume.EChangeType.Removed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdateWallPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabWallVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
		}
		else
		{
			prefabInstance.prefab.SetWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
		}
	}
}
