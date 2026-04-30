using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoiTeleportMenu : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PoiList list;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxFilterTier;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		list = GetChildByType<XUiC_PoiList>();
		list.ListEntryClicked += onListEntryClicked;
		XUiC_ToggleButton obj = GetChildById("filterSmall")?.GetChildByType<XUiC_ToggleButton>();
		obj.Value = list.FilterSmallPois;
		obj.OnValueChanged += FilterSmall_Changed;
		cbxFilterTier = GetChildById("cbxFilterTier")?.GetChildByType<XUiC_ComboBoxInt>();
		cbxFilterTier.Value = list.FilterTier;
		cbxFilterTier.OnValueChanged += CbxFilterTier_OnValueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxFilterTier_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		list.FilterTier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FilterSmall_Changed(XUiC_ToggleButton _sender, bool _newvalue)
	{
		list.FilterSmallPois = _newvalue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onListEntryClicked(XUiC_ListEntry<XUiC_PoiList.PoiListEntry> _entry)
	{
		if (_entry != null && _entry.GetEntry() != null)
		{
			XUiC_PoiList.PoiListEntry entry = _entry.GetEntry();
			EntryPressed(entry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryPressed(XUiC_PoiList.PoiListEntry _key)
	{
		base.xui.playerUI.entityPlayer.Teleport(_key.prefabInstance.boundingBoxPosition.ToVector3(), 45f);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		cbxFilterTier.Max = list.MaxTier;
	}
}
