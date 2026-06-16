using System;
using PrefabVolumes;
using UnityEngine;

public class PrefabVolumeManager : ISelectionBoxCallback
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabVolumeManager instance;

	public static PrefabVolumeManager Instance => instance ?? (instance = new PrefabVolumeManager());

	public void AddVolumeServer(PrefabVolumeAbs.EVolumeType _volumeType, Vector3i _startPos, Vector3i _size)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddVolumeFromClient>().Setup(_volumeType, _startPos, _size));
			return;
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefabFromWorldPosInside = dynamicPrefabDecorator.GetPrefabFromWorldPosInside(_startPos.x + _size.x / 2, _startPos.z + _size.z / 2);
		if (prefabFromWorldPosInside == null)
		{
			return;
		}
		if (!prefabFromWorldPosInside.prefab.AllVolumeListsByType.TryGetValue(_volumeType, out var value))
		{
			throw new ArgumentException("VolumeList for Type " + _volumeType.ToStringCached() + " not found", "_volumeType");
		}
		if (value.CanCreateVolume(prefabFromWorldPosInside.name, prefabFromWorldPosInside.boundingBoxPosition, _startPos - prefabFromWorldPosInside.boundingBoxPosition, _size))
		{
			var (volumeId, volume, _) = value.AddNewVolume(prefabFromWorldPosInside.name, prefabFromWorldPosInside.boundingBoxPosition, _startPos - prefabFromWorldPosInside.boundingBoxPosition, _size);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorUpdateVolume>().Setup(NetPackageEditorUpdateVolume.EChangeType.Added, prefabFromWorldPosInside.id, volumeId, volume));
			if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.PrefabInstanceId == prefabFromWorldPosInside.id)
			{
				PrefabEditModeManager.Instance.NeedsSaving = true;
			}
		}
	}

	public void CloneVolumeServer(PrefabVolumeAbs.EVolumeType _volumeType, int _prefabInstanceId, int _existingIndex, Vector3i _offset)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddVolumeFromClient>().Setup(_volumeType, _prefabInstanceId, _existingIndex, _offset));
			return;
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefab = dynamicPrefabDecorator.GetPrefab(_prefabInstanceId);
		if (prefab != null)
		{
			if (!prefab.prefab.AllVolumeListsByType.TryGetValue(_volumeType, out var value))
			{
				throw new ArgumentException("VolumeList for Type " + _volumeType.ToStringCached() + " not found", "_volumeType");
			}
			var (volumeId, volume, _) = value.CloneVolume(prefab.name, prefab.boundingBoxPosition, _existingIndex, _offset);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorUpdateVolume>().Setup(NetPackageEditorUpdateVolume.EChangeType.Added, prefab.id, volumeId, volume));
		}
	}

	public void UpdatePropertiesServer(int _prefabInstanceId, int _volumeId, PrefabVolumeAbs _volumeSettings)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AddUpdatePropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorUpdateVolume>().Setup(NetPackageEditorUpdateVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorUpdateVolume>().Setup(NetPackageEditorUpdateVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings));
		}
	}

	public void AddUpdatePropertiesClient(int _prefabInstanceId, int _volumeId, PrefabVolumeAbs _volumeSettings)
	{
		PrefabInstance prefabInstance = PrefabInstanceClientManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId);
			return;
		}
		if (!prefabInstance.prefab.AllVolumeListsByType.TryGetValue(_volumeSettings.VolumeType, out var value))
		{
			throw new ArgumentException("VolumeList for Type " + _volumeSettings.VolumeType.ToStringCached() + " not found", "_volumeSettings");
		}
		value.SetVolume(prefabInstance, _volumeId, _volumeSettings);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.PrefabInstanceId == prefabInstance.id)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public static bool GetPrefabIdAndVolumeId(string _name, out int _volumeId, out PrefabInstance _prefabInstance)
	{
		_volumeId = -1;
		_prefabInstance = null;
		int num = _name.LastIndexOf('.');
		if (num < 0)
		{
			return false;
		}
		int num2 = _name.IndexOf('_', num + 2);
		if (num2 < 0 || num2 >= _name.Length - 1)
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(_name, out var _result, num + 1, num2 - 1))
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(_name, out _volumeId, num2 + 1))
		{
			_volumeId = -1;
			return false;
		}
		_prefabInstance = PrefabInstanceClientManager.Instance.GetPrefabInstance(_result);
		if (_prefabInstance == null)
		{
			_volumeId = -1;
		}
		return _prefabInstance != null;
	}

	public static bool TryGetSelectedVolume<TVolume>(SelectionCategory _selectionCategory, out SelectionBox _box, out TVolume _volume, out PrefabInstance _prefabInstance, out int _volumeIndex) where TVolume : PrefabVolumeAbs
	{
		_box = null;
		_volume = null;
		_prefabInstance = null;
		_volumeIndex = -1;
		SelectionBox selection = SelectionBoxManager.Instance.Selection;
		if (selection == null || selection.Category != _selectionCategory)
		{
			return false;
		}
		if (!(selection.UserData is TVolume val))
		{
			return false;
		}
		if (!GetPrefabIdAndVolumeId(selection.name, out _volumeIndex, out _prefabInstance))
		{
			return false;
		}
		_box = selection;
		_volume = val;
		return true;
	}

	public void SelectionBoxMoved(PrefabVolumeAbs.EVolumeType _volumeType, SelectionBox _box, Vector3 _moveVector)
	{
		if (GetPrefabIdAndVolumeId(_box.name, out var _volumeId, out var _prefabInstance))
		{
			if (!_prefabInstance.prefab.AllVolumeListsByType.TryGetValue(_volumeType, out var value))
			{
				throw new ArgumentException("VolumeList for Type " + _volumeType.ToStringCached() + " not found", "_volumeType");
			}
			PrefabVolumeAbs prefabVolumeAbs = value.Get(_volumeId).Clone();
			prefabVolumeAbs.Move(new Vector3i(_moveVector));
			UpdatePropertiesServer(_prefabInstance.id, _volumeId, prefabVolumeAbs);
		}
	}

	public void SelectionBoxSized(PrefabVolumeAbs.EVolumeType _volumeType, SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (GetPrefabIdAndVolumeId(_box.name, out var _volumeId, out var _prefabInstance))
		{
			if (!_prefabInstance.prefab.AllVolumeListsByType.TryGetValue(_volumeType, out var value))
			{
				throw new ArgumentException("VolumeList for Type " + _volumeType.ToStringCached() + " not found", "_volumeType");
			}
			PrefabVolumeAbs prefabVolumeAbs = value.Get(_volumeId).Clone();
			prefabVolumeAbs.Resize(_dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
			UpdatePropertiesServer(_prefabInstance.id, _volumeId, prefabVolumeAbs);
		}
	}

	public bool SelectionBoxDelete(PrefabVolumeAbs.EVolumeType _volumeType, SelectionBox _box, bool _checkCanDeleteOnly)
	{
		if (!GetPrefabIdAndVolumeId(_box.name, out var _volumeId, out var _prefabInstance))
		{
			return false;
		}
		if (_checkCanDeleteOnly)
		{
			return true;
		}
		if (!_prefabInstance.prefab.AllVolumeListsByType.TryGetValue(_volumeType, out var value))
		{
			throw new ArgumentException("VolumeList for Type " + _volumeType.ToStringCached() + " not found", "_volumeType");
		}
		PrefabVolumeAbs prefabVolumeAbs = value.Get(_volumeId).Clone();
		prefabVolumeAbs.MarkUnused();
		UpdatePropertiesServer(_prefabInstance.id, _volumeId, prefabVolumeAbs);
		return true;
	}

	public void Init()
	{
		SelectionBoxManager.Instance.CategoryTraderTeleport.SetCallback(this);
		SelectionBoxManager.Instance.CategoryInfoVolume.SetCallback(this);
		SelectionBoxManager.Instance.CategoryWallVolume.SetCallback(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabVolumeAbs.EVolumeType getBoxVolumeType(SelectionBox _box)
	{
		SelectionCategory category = _box.Category;
		if (category == SelectionBoxManager.Instance.CategoryTraderTeleport)
		{
			return PrefabVolumeAbs.EVolumeType.Teleport;
		}
		if (category == SelectionBoxManager.Instance.CategoryInfoVolume)
		{
			return PrefabVolumeAbs.EVolumeType.Info;
		}
		if (category == SelectionBoxManager.Instance.CategoryWallVolume)
		{
			return PrefabVolumeAbs.EVolumeType.Wall;
		}
		throw new ArgumentException("Invalid box category " + category.name);
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		SelectionBoxMoved(getBoxVolumeType(_box), _box, _moveVector);
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		SelectionBoxSized(getBoxVolumeType(_box), _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		return SelectionBoxDelete(getBoxVolumeType(_box), _box, _checkCanDeleteOnly);
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
	}
}
