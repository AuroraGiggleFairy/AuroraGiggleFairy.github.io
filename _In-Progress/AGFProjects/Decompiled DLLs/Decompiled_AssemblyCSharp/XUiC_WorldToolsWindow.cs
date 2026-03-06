using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldToolsWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLevelStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxBoxSideTransparency;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxBoxSelectionCaptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxBoxPrefabPreviewLimit;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnLevelStartPoint = GetChildById("btnLevelStartPoint");
		btnLevelStartPoint.GetChildById("clickable").OnPress += BtnLevelStartPoint_Controller_OnPress;
		cbxBoxSideTransparency = GetChildById("cbxBoxSideTransparency").GetChildByType<XUiC_ComboBoxFloat>();
		cbxBoxSideTransparency.OnValueChanged += CbxBoxSideTransparency_OnValueChanged;
		cbxBoxSelectionCaptions = GetChildById("cbxBoxSelectionCaptions").GetChildByType<XUiC_ComboBoxBool>();
		cbxBoxSelectionCaptions.OnValueChanged += CbxBoxSelectionCaptions_OnValueChanged;
		cbxBoxSelectionCaptions.Value = true;
		cbxBoxPrefabPreviewLimit = GetChildById("cbxBoxPrefabPreviewLimit").GetChildByType<XUiC_ComboBoxInt>();
		cbxBoxPrefabPreviewLimit.OnValueChanged += CbxBoxPrefabPreviewLimit_OnValueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLevelStartPoint_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
			GameManager.Instance.GetSpawnPointList().Add(new SpawnPoint(vector3i));
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("StartPoint");
			Vector3i vector3i2 = vector3i;
			category.AddBox(vector3i2.ToString() ?? "", vector3i, Vector3i.one, _bDrawDirection: true);
			SelectionBoxManager instance = SelectionBoxManager.Instance;
			vector3i2 = vector3i;
			instance.SetActive("StartPoint", vector3i2.ToString() ?? "", _bActive: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxSelectionCaptions_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs")?.SetCaptionVisibility(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxSideTransparency_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SelectionBoxManager.Instance.AlphaMultiplier = (float)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxPrefabPreviewLimit_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		DynamicPrefabDecorator.PrefabPreviewLimit = (int)_newValue;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		cbxBoxSideTransparency.Value = SelectionBoxManager.Instance.AlphaMultiplier;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			btnLevelStartPoint.ViewComponent.IsVisible = false;
		}
	}
}
