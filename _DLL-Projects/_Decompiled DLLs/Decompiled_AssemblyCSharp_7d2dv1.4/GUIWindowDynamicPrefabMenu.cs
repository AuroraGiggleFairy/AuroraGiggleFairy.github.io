using UnityEngine;

public class GUIWindowDynamicPrefabMenu : ISelectionBoxCallback
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIWindowDynamicPrefabMenu instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance selectedPrefab;

	public static PrefabInstance selectedPrefabInstance
	{
		get
		{
			if (instance != null)
			{
				return instance.selectedPrefab;
			}
			return null;
		}
	}

	public static int selectedVolumeIndex
	{
		get
		{
			if (instance != null && instance.selectedPrefab != null)
			{
				return instance.selIdx;
			}
			return -1;
		}
	}

	public GUIWindowDynamicPrefabMenu(GameManager _gm)
	{
		gameManager = _gm;
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").SetCallback(this);
		SelectionBoxManager.Instance.GetCategory("InfoVolume").SetCallback(this);
		SelectionBoxManager.Instance.GetCategory("WallVolume").SetCallback(this);
		instance = this;
	}

	public static void Cleanup()
	{
		if (SelectionBoxManager.Instance != null && instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("TraderTeleport").SetCallback(null);
		}
		if (SelectionBoxManager.Instance != null && instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("InfoVolume").SetCallback(null);
		}
		if (SelectionBoxManager.Instance != null && instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("WallVolume").SetCallback(null);
		}
		instance = null;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
			{
				selIdx = _volumeId;
			}
		}
		else
		{
			selectedPrefab = null;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefabIdAndVolumeId(string _name, out int _prefabInstanceId, out int _volumeId)
	{
		_prefabInstanceId = (_volumeId = 0);
		string[] array = _name.Split('.');
		if (array.Length > 1)
		{
			string[] array2 = array[1].Split('_');
			if (array2.Length > 1 && int.TryParse(array2[1], out _volumeId) && int.TryParse(array2[0], out _prefabInstanceId))
			{
				selectedPrefab = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				return true;
			}
		}
		return false;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (selectedPrefab != null)
		{
			switch (_category)
			{
			case "TraderTeleport":
			{
				Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume(selectedPrefab.prefab.TeleportVolumes[selIdx]);
				prefabTeleportVolume.startPos += new Vector3i(_moveVector);
				PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(selectedPrefab.id, selIdx, prefabTeleportVolume);
				break;
			}
			case "InfoVolume":
			{
				Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume(selectedPrefab.prefab.InfoVolumes[selIdx]);
				prefabInfoVolume.startPos += new Vector3i(_moveVector);
				PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(selectedPrefab.id, selIdx, prefabInfoVolume);
				break;
			}
			case "WallVolume":
			{
				Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume(selectedPrefab.prefab.WallVolumes[selIdx]);
				prefabWallVolume.startPos += new Vector3i(_moveVector);
				PrefabVolumeManager.Instance.UpdateWallPropertiesServer(selectedPrefab.id, selIdx, prefabWallVolume);
				break;
			}
			}
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (selectedPrefab == null)
		{
			return;
		}
		switch (_category)
		{
		case "TraderTeleport":
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume(selectedPrefab.prefab.TeleportVolumes[selIdx]);
			prefabTeleportVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabTeleportVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size2 = prefabTeleportVolume.size;
			if (size2.x < 2)
			{
				size2 = new Vector3i(1, size2.y, size2.z);
			}
			if (size2.y < 2)
			{
				size2 = new Vector3i(size2.x, 1, size2.z);
			}
			if (size2.z < 2)
			{
				size2 = new Vector3i(size2.x, size2.y, 1);
			}
			prefabTeleportVolume.size = size2;
			PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(selectedPrefab.id, selIdx, prefabTeleportVolume);
			break;
		}
		case "InfoVolume":
		{
			Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume(selectedPrefab.prefab.InfoVolumes[selIdx]);
			prefabInfoVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabInfoVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size3 = prefabInfoVolume.size;
			if (size3.x < 2)
			{
				size3 = new Vector3i(1, size3.y, size3.z);
			}
			if (size3.y < 2)
			{
				size3 = new Vector3i(size3.x, 1, size3.z);
			}
			if (size3.z < 2)
			{
				size3 = new Vector3i(size3.x, size3.y, 1);
			}
			prefabInfoVolume.size = size3;
			PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(selectedPrefab.id, selIdx, prefabInfoVolume);
			break;
		}
		case "WallVolume":
		{
			Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume(selectedPrefab.prefab.WallVolumes[selIdx]);
			prefabWallVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabWallVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size = prefabWallVolume.size;
			if (size.x < 2)
			{
				size = new Vector3i(1, size.y, size.z);
			}
			if (size.y < 2)
			{
				size = new Vector3i(size.x, 1, size.z);
			}
			if (size.z < 2)
			{
				size = new Vector3i(size.x, size.y, 1);
			}
			prefabWallVolume.size = size;
			PrefabVolumeManager.Instance.UpdateWallPropertiesServer(selectedPrefab.id, selIdx, prefabWallVolume);
			break;
		}
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		foreach (LocalPlayerUI playerUI in LocalPlayerUI.PlayerUIs)
		{
			if (playerUI.windowManager.IsModalWindowOpen())
			{
				SelectionBoxManager.Instance.SetActive(_category, _name, _bActive: true);
				return false;
			}
		}
		if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
		{
			switch (_category)
			{
			case "TraderTeleport":
			{
				Prefab.PrefabTeleportVolume volumeSettings3 = new Prefab.PrefabTeleportVolume(selectedPrefab.prefab.TeleportVolumes[_volumeId]);
				PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(selectedPrefab.id, _volumeId, volumeSettings3, remove: true);
				break;
			}
			case "InfoVolume":
			{
				Prefab.PrefabInfoVolume volumeSettings2 = new Prefab.PrefabInfoVolume(selectedPrefab.prefab.InfoVolumes[_volumeId]);
				PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(selectedPrefab.id, _volumeId, volumeSettings2, remove: true);
				break;
			}
			case "WallVolume":
			{
				Prefab.PrefabWallVolume volumeSettings = new Prefab.PrefabWallVolume(selectedPrefab.prefab.WallVolumes[_volumeId]);
				PrefabVolumeManager.Instance.UpdateWallPropertiesServer(selectedPrefab.id, _volumeId, volumeSettings, remove: true);
				break;
			}
			}
			return true;
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}
}
