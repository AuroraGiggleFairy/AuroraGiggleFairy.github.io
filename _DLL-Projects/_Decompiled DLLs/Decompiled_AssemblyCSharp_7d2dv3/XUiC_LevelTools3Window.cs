using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools3Window : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class VolumeTypeDefinition
	{
		public readonly SelectionCategory SelectionCategory;

		public readonly PrefabVolumeAbs.EVolumeType VolumeType;

		public VolumeTypeDefinition(string _selectionCategoryName, PrefabVolumeAbs.EVolumeType _volumeType)
		{
			SelectionCategory = SelectionBoxManager.Instance.GetCategory(_selectionCategoryName);
			VolumeType = _volumeType;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<VolumeTypeDefinition> volumeTypeDefinitions = new List<VolumeTypeDefinition>
	{
		new VolumeTypeDefinition("SleeperVolume", PrefabVolumeAbs.EVolumeType.Sleeper),
		new VolumeTypeDefinition("TriggerVolume", PrefabVolumeAbs.EVolumeType.Trigger),
		new VolumeTypeDefinition("InfoVolume", PrefabVolumeAbs.EVolumeType.Info),
		new VolumeTypeDefinition("TraderTeleport", PrefabVolumeAbs.EVolumeType.Teleport),
		new VolumeTypeDefinition("WallVolume", PrefabVolumeAbs.EVolumeType.Wall)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList volumeTypeSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnVolumesCreate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnVolumesCreateFromSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleVolumesShow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnVolumesDupe;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleVolumesConfirmDelete;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		volumeTypeSelector = GetChildById("volumeTypeSelector")?.GetChildByType<XUiC_CategoryList>();
		if (volumeTypeSelector != null)
		{
			volumeTypeSelector.SetCategoryToFirst();
		}
		btnVolumesCreate = GetChildById("btnVolumesCreate")?.GetChildByType<XUiC_SimpleButton>();
		if (btnVolumesCreate != null)
		{
			btnVolumesCreate.OnPressed += BtnVolumesCreateOnOnPressed;
		}
		btnVolumesCreateFromSelection = GetChildById("btnVolumesCreateFromSelection")?.GetChildByType<XUiC_SimpleButton>();
		if (btnVolumesCreateFromSelection != null)
		{
			btnVolumesCreateFromSelection.OnPressed += BtnVolumesCreateFromSelectionOnOnPressed;
		}
		toggleVolumesShow = GetChildById("toggleVolumesShow")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleVolumesShow != null)
		{
			toggleVolumesShow.OnValueChanged += ToggleVolumesShowOnOnValueChanged;
		}
		btnVolumesDupe = GetChildById("btnVolumesDupe")?.GetChildByType<XUiC_SimpleButton>();
		if (btnVolumesDupe != null)
		{
			btnVolumesDupe.OnPressed += BtnVolumesDupeOnOnPressed;
		}
		toggleVolumesConfirmDelete = GetChildById("toggleVolumesConfirmDelete")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleVolumesConfirmDelete != null)
		{
			toggleVolumesConfirmDelete.OnValueChanged += ToggleVolumesConfirmDeleteOnValueChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory(name);
		btnVolumesCreateFromSelection.Enabled = BlockToolSelection.Instance?.SelectionActive ?? false;
		toggleVolumesShow.Value = category?.IsVisible() ?? false;
		btnVolumesDupe.Enabled = cloneableVolumeSelection(out var _, out var _, out var _);
		toggleVolumesConfirmDelete.Value = !GamePrefs.GetBool(EnumGamePrefs.OptionsPoiVolumesSkipDeleteConfirmation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool volumeTypeByName(string _name, out VolumeTypeDefinition _result)
	{
		_result = volumeTypeDefinitions.Find([PublicizedFrom(EAccessModifier.Internal)] (VolumeTypeDefinition _vtd) => _vtd.SelectionCategory.name == _name);
		return _result != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnVolumesCreateOnOnPressed(XUiController _sender, int _mouseButton)
	{
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		if (volumeTypeByName(name, out var _result))
		{
			addVolume(_result.VolumeType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnVolumesCreateFromSelectionOnOnPressed(XUiController _sender, int _mouseButton)
	{
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		if (volumeTypeByName(name, out var _result))
		{
			addVolumeFromSelection(_result.VolumeType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleVolumesShowOnOnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		if (volumeTypeByName(name, out var _result))
		{
			SelectionCategory selectionCategory = _result.SelectionCategory;
			selectionCategory.SetVisible(!selectionCategory.IsVisible());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cloneableVolumeSelection(out PrefabVolumeAbs _existingVolume, out int _prefabInstanceId, out int _volumeId)
	{
		_existingVolume = null;
		_prefabInstanceId = 0;
		_volumeId = 0;
		SelectionBox selection = SelectionBoxManager.Instance.Selection;
		if (selection == null)
		{
			return false;
		}
		_existingVolume = selection.UserData as PrefabVolumeAbs;
		if (_existingVolume == null)
		{
			return false;
		}
		if (!PrefabVolumeManager.GetPrefabIdAndVolumeId(selection.name, out _volumeId, out var _prefabInstance))
		{
			return false;
		}
		_prefabInstanceId = _prefabInstance.id;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnVolumesDupeOnOnPressed(XUiController _sender, int _mouseButton)
	{
		if (cloneableVolumeSelection(out var _existingVolume, out var _prefabInstanceId, out var _volumeId))
		{
			PrefabVolumeManager.Instance.CloneVolumeServer(_existingVolume.VolumeType, _prefabInstanceId, _volumeId, new Vector3i(0, _existingVolume.size.y + 1, 0));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleVolumesConfirmDeleteOnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsPoiVolumesSkipDeleteConfirmation, !_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addVolume(PrefabVolumeAbs.EVolumeType _volumeType)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i size = new Vector3i(5, 4, 5);
			Vector3i startPos = World.worldToBlockPos(raycastHitPoint) - new Vector3i(size.x / 2, 0, size.z / 2);
			PrefabVolumeManager.Instance.AddVolumeServer(_volumeType, startPos, size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addVolumeFromSelection(PrefabVolumeAbs.EVolumeType _volumeType)
	{
		BlockToolSelection instance = BlockToolSelection.Instance;
		if (instance != null && instance.SelectionActive)
		{
			PrefabVolumeManager.Instance.AddVolumeServer(_volumeType, instance.SelectionMin, instance.SelectionSize);
		}
	}

	public static Vector3 getRaycastHitPoint(float _maxDistance = 100f, float _offsetUp = 0f)
	{
		Camera finalCamera = GameManager.Instance.World.GetPrimaryPlayer().finalCamera;
		Ray ray = finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
		ray.origin += Origin.position;
		Transform transform = finalCamera.transform;
		ray.origin += transform.forward * 0.1f;
		ray.origin += transform.up * _offsetUp;
		if (Voxel.Raycast(GameManager.Instance.World, ray, _maxDistance, -555266061, 4095, 0f))
		{
			return Voxel.voxelRayHitInfo.hit.pos - ray.direction * 0.05f;
		}
		return Vector3.zero;
	}
}
