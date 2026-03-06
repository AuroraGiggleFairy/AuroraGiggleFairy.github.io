using UnityEngine.Scripting;

[Preserve]
public class XUiC_TipWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string tipText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string tipTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string nextTip = "";

	public string TipText
	{
		get
		{
			return tipText;
		}
		set
		{
			tipText = value;
			IsDirty = true;
		}
	}

	public string TipTitle
	{
		get
		{
			return tipTitle;
		}
		set
		{
			tipTitle = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ToolTipEvent CloseEvent { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (!(bindingName == "tiptext"))
		{
			if (bindingName == "tiptitle")
			{
				value = TipTitle;
				return true;
			}
			return false;
		}
		value = TipText;
		return true;
	}

	public override void Init()
	{
		base.Init();
		((XUiV_Button)GetChildById("clickable").ViewComponent).Controller.OnPress += closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public static void ShowTip(string _tip, string _title, EntityPlayerLocal _localPlayer, ToolTipEvent _closeEvent)
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_localPlayer);
		if (uIForPlayer != null && uIForPlayer.xui != null && uIForPlayer.windowManager.IsHUDEnabled())
		{
			XUiC_TipWindow childByType = uIForPlayer.xui.FindWindowGroupByName("tipWindow").GetChildByType<XUiC_TipWindow>();
			childByType.TipText = Localization.Get(_tip);
			childByType.TipTitle = Localization.Get(_title);
			childByType.CloseEvent = _closeEvent;
			uIForPlayer.windowManager.Open("tipWindow", _bModal: true);
			uIForPlayer.windowManager.CloseIfOpen("windowpaging");
			uIForPlayer.windowManager.CloseIfOpen("toolbelt");
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (CloseEvent != null)
		{
			CloseEvent.HandleEvent();
			CloseEvent = null;
		}
	}
}
