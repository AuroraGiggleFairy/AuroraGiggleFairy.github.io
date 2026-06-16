using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DlcWindowNew : XUiController
{
	public static string ID = "";

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DlcList dlcList;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		registerForInputStyleChanges();
	}

	[XuiBindEvent("ListEntryClicked", "dlcList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void entryClicked(XUiC_List<XUiC_DlcList.DlcEntry> _list, XUiC_DlcList.DlcEntry _entry)
	{
		EntitlementManager.Instance.OpenStore(_entry.DlcSet, [PublicizedFrom(EAccessModifier.Private)] (EntitlementSetEnum _) =>
		{
			dlcList.RebuildList();
		});
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		updatePagingButtonIcons(PlatformManager.NativePlatform.Input.CurrentInputStyle);
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup, _fromEsc: true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			XUi.HandlePaging(xui, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				dlcList.ChangePage(1);
				return true;
			}, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				dlcList.ChangePage(-1);
				return true;
			});
			if (xui.playerUI.playerInput.GUIActions.Submit.WasReleased && !xui.playerUI.CursorController.CursorModeActive)
			{
				int currentBaseIndex = dlcList.CurrentBaseIndex;
				int pageLength = dlcList.PageLength;
				int index = currentBaseIndex + pageLength / 2;
				XUiC_DlcList.DlcEntry entryByIndex = dlcList.GetEntryByIndex(index);
				entryClicked(dlcList, entryByIndex);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		updatePagingButtonIcons(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePagingButtonIcons(PlayerInputManager.InputStyle _inputStyle)
	{
		bool flag = _inputStyle != PlayerInputManager.InputStyle.Keyboard && dlcList.AllEntries().Count > dlcList.PageLength;
		XUiView xUiView = GetChildById("LB_Icon").ViewComponent;
		bool isVisible = (GetChildById("RB_Icon").ViewComponent.IsVisible = flag);
		xUiView.IsVisible = isVisible;
	}
}
