using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DlcWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DlcList dlcList;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnBack", out var _child))
		{
			_child.OnPressed += BtnBack_OnPressed;
		}
		dlcList = GetChildByType<XUiC_DlcList>();
		if (dlcList != null)
		{
			dlcList.ListEntryClicked += entryClicked;
		}
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void entryClicked(XUiC_ListEntry<XUiC_DlcList.DlcEntry> _entry)
	{
		XUiC_DlcList.DlcEntry entry = _entry.GetEntry();
		Log.Out($"DLC list entry clicked: {entry.Name} : {entry.DlcSet}");
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		UpdatePagingButtonIcons(PlatformManager.NativePlatform.Input.CurrentInputStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup, _fromEsc: true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed)
		{
			dlcList.PageUp();
		}
		else if (base.xui.playerUI.playerInput.GUIActions.WindowPagingLeft.WasPressed)
		{
			dlcList.PageDown();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		UpdatePagingButtonIcons(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePagingButtonIcons(PlayerInputManager.InputStyle inputStyle)
	{
		bool flag = inputStyle != PlayerInputManager.InputStyle.Keyboard && dlcList.AllEntries().Count > dlcList.PageLength;
		XUiView xUiView = GetChildById("LB_Icon").ViewComponent;
		bool isVisible = (GetChildById("RB_Icon").ViewComponent.IsVisible = flag);
		xUiView.IsVisible = isVisible;
	}
}
