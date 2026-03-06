using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools3Window : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class VolumeTypeDefinition
	{
		public readonly SelectionCategory SelectionCategory;

		public readonly Action<Vector3i, Vector3i> AddVolumeHandler;

		public VolumeTypeDefinition(string _selectionCategoryName, Action<Vector3i, Vector3i> _addVolumeHandler)
		{
			SelectionCategory = SelectionBoxManager.Instance.GetCategory(_selectionCategoryName);
			AddVolumeHandler = _addVolumeHandler;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton buttonCopySleeperVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<VolumeTypeDefinition> volumeTypeDefinitions = new List<VolumeTypeDefinition>
	{
		new VolumeTypeDefinition("SleeperVolume", PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer),
		new VolumeTypeDefinition("TriggerVolume", PrefabTriggerVolumeManager.Instance.AddTriggerVolumeServer),
		new VolumeTypeDefinition("InfoVolume", PrefabVolumeManager.Instance.AddInfoVolumeServer),
		new VolumeTypeDefinition("TraderTeleport", PrefabVolumeManager.Instance.AddTeleportVolumeServer),
		new VolumeTypeDefinition("WallVolume", PrefabVolumeManager.Instance.AddWallVolumeServer)
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

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		buttonCopySleeperVolume = GetChildById("buttonCopySleeperVolume")?.GetChildByType<XUiC_SimpleButton>();
		if (buttonCopySleeperVolume != null && GameManager.Instance.GetActiveBlockTool() is BlockToolSelection blockToolSelection)
		{
			if (blockToolSelection.GetActions().TryGetValue("copySleeperVolume", out var action))
			{
				string text = action.GetText() + " " + action.GetHotkey().GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithParentheses);
				string tooltip = action.GetTooltip();
				buttonCopySleeperVolume.Text = text;
				buttonCopySleeperVolume.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
				{
					action.OnClick();
				};
				buttonCopySleeperVolume.Tooltip = tooltip;
			}
		}
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
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory(name);
		btnVolumesCreateFromSelection.Enabled = BlockToolSelection.Instance?.SelectionActive ?? false;
		toggleVolumesShow.Value = category?.IsVisible() ?? false;
		btnVolumesDupe.Enabled = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && false;
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
			addVolume(_result.AddVolumeHandler);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnVolumesCreateFromSelectionOnOnPressed(XUiController _sender, int _mouseButton)
	{
		string name = volumeTypeSelector.CurrentCategory?.CategoryName ?? "";
		if (volumeTypeByName(name, out var _result))
		{
			addVolumeFromSelection(_result.AddVolumeHandler);
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
	public void BtnVolumesDupeOnOnPressed(XUiController _sender, int _mouseButton)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addVolume(Action<Vector3i, Vector3i> _addVolumeCallback)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i arg = new Vector3i(5, 4, 5);
			Vector3i arg2 = World.worldToBlockPos(raycastHitPoint) - new Vector3i(arg.x / 2, 0, arg.z / 2);
			_addVolumeCallback(arg2, arg);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addVolumeFromSelection(Action<Vector3i, Vector3i> _addVolumeCallback)
	{
		BlockToolSelection instance = BlockToolSelection.Instance;
		if (instance != null && instance.SelectionActive)
		{
			_addVolumeCallback(instance.SelectionMin, instance.SelectionSize);
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
		if (Voxel.Raycast(GameManager.Instance.World, ray, _maxDistance, -555266053, 4095, 0f))
		{
			return Voxel.voxelRayHitInfo.hit.pos - ray.direction * 0.05f;
		}
		return Vector3.zero;
	}
}
