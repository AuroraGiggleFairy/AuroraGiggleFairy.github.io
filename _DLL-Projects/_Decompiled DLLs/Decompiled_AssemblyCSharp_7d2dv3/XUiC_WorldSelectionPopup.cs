using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldSelectionPopup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<PathAbstractions.AbstractedLocation> onConfirm;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldSelectionList worldList;

	[XuiXmlBinding("msgTitle")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MsgTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = string.Empty;

	[XuiXmlBinding("isWorldSelected")]
	public bool IsWorldSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return worldList?.HasSelection ?? false;
		}
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCancelButtonPressed(XUiController _sender, int _mouseButton)
	{
		cancel();
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnConfirmButtonPressed(XUiController _sender, int _mouseButton)
	{
		confirm();
	}

	[XuiBindEvent("SelectionChanged", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_List<XUiC_WorldSelectionList.Entry> _list, XUiC_WorldSelectionList.Entry _previousEntry, XUiC_WorldSelectionList.Entry _newEntry)
	{
		IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		id = windowGroup.Id;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void open(string _titleCaptionKey, string _worldName, Action _onCancel, Action<PathAbstractions.AbstractedLocation> _onConfirm)
	{
		MsgTitle = _titleCaptionKey;
		onCancel = _onCancel;
		onConfirm = _onConfirm;
		worldList.SetFilter(_worldName);
		if (worldList.EntryCount == 1)
		{
			_onConfirm?.Invoke(worldList.GetEntry(0).Location);
		}
		else
		{
			xui.playerUI.windowManager.Open(windowGroup, _bModal: false);
		}
	}

	public static void Open(XUi _xui, string _titleCaptionKey, string _worldName, Action _onCancel, Action<PathAbstractions.AbstractedLocation> _onConfirm)
	{
		((XUiC_WorldSelectionPopup)_xui.FindWindowGroupByName(id)).open(_titleCaptionKey, _worldName, _onCancel, _onConfirm);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent))
		{
			if (xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				cancel();
				return;
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				confirm();
				return;
			}
		}
		if (IsDirty)
		{
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancel()
	{
		onCancel?.Invoke();
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void confirm()
	{
		if (worldList.HasSelection)
		{
			onConfirm?.Invoke(worldList.SelectedEntryData.Location);
			xui.playerUI.windowManager.Close(windowGroup);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		onCancel = null;
		onConfirm = null;
	}
}
