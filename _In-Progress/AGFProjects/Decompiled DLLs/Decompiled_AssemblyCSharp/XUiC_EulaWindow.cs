using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EulaWindow : XUiController
{
	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDecline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect footerContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public int defaultEulaVersion = -1;

	public static string retrievedEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> pages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageFormatted;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool viewMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAlphanumericPageCharacterLimit = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cScriptPageCharacterLimit = 1000;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnAccept = (XUiC_SimpleButton)GetChildById("btnAccept");
		btnDecline = (XUiC_SimpleButton)GetChildById("btnDecline");
		lblContent = GetChildById("lblContent").ViewComponent as XUiV_Label;
		footerContainer = GetChildById("footer").ViewComponent as XUiV_Rect;
		background = GetChildById("background");
		btnPageUp = GetChildById("btnPageUp");
		btnPageDown = GetChildById("btnPageDown");
		btnAccept.OnPressed += btnAccept_OnPressed;
		btnDecline.OnPressed += btnDecline_OnPressed;
		btnPageDown.OnPress += btnPageDown_OnPressed;
		btnPageUp.OnPress += btnPageUp_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDone")).OnPressed += btnDecline_OnPressed;
		TextAsset textAsset = Resources.Load<TextAsset>($"Data/EULA/eula_{Localization.language.ToLower()}");
		if (textAsset != null)
		{
			LoadDefaultXML(textAsset.bytes);
		}
		else
		{
			Log.Error("Could not load default EULA text asset");
		}
	}

	public static void Open(XUi _xui, bool _viewMode = false)
	{
		viewMode = _viewMode;
		_xui.playerUI.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		pageFormatted = false;
		currentPage = 0;
		SetVisibility(_visible: false);
		btnAccept.Enabled = false;
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		lblContent.Label.ResetAndUpdateAnchors();
		RefreshBindings(_forceAll: true);
		if (viewMode)
		{
			GetChildById("btnDone").SelectCursorElement(_withDelay: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		SetVisibility(_visible: false);
		base.xui.playerUI.windowManager.Close(ID);
		if (viewMode)
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, _bModal: true);
		}
		else
		{
			XUiC_MainMenu.Open(base.xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadDefaultXML(byte[] _data)
	{
		XmlFile xmlFile;
		try
		{
			xmlFile = new XmlFile(_data, _throwExc: true);
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading default EULA XML: {0}", ex.Message);
			return;
		}
		XElement root = xmlFile.XmlDoc.Root;
		if (root != null)
		{
			defaultEulaVersion = int.Parse(root.GetAttribute("version").Trim());
			defaultEula = root.Value;
			if (defaultEulaVersion > GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion))
			{
				GamePrefs.Set(EnumGamePrefs.EulaLatestVersion, defaultEulaVersion);
			}
			Log.Out("Loaded default EULA");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayLocalEulaOrClose()
	{
		if (GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) < defaultEulaVersion || viewMode)
		{
			SetVisibility(_visible: true);
			FormatPages(defaultEula);
			ShowGamepadCallouts();
		}
		else
		{
			Close();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FormatPages(string content)
	{
		content = content.Replace("\t", "  ");
		pages.Clear();
		string[] array = content.Split('\n');
		int num;
		switch (Localization.language)
		{
		case "japanese":
		case "koreana":
		case "schinese":
		case "tchinese":
			num = 1000;
			break;
		default:
			num = 2000;
			break;
		}
		int num2 = 0;
		while (num2 < array.Length)
		{
			string text = array[num2];
			if (string.IsNullOrWhiteSpace(text))
			{
				num2++;
				continue;
			}
			num2++;
			while (text.Length < num && num2 < array.Length)
			{
				text += "\n\n";
				text += array[num2];
				num2++;
			}
			pages.Add(text);
		}
		SetPage(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPage(int page)
	{
		if (page >= 0 && page < pages.Count)
		{
			currentPage = page;
			lblContent.Text = pages[page];
			UpdatePageButtonVisibility();
			if (!viewMode && currentPage == pages.Count - 1 && !btnAccept.Enabled)
			{
				base.xui.playerUI.CursorController.SetNavigationTarget(btnDecline.ViewComponent);
				btnAccept.Enabled = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowGamepadCallouts()
	{
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (viewMode)
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStickUpDown, "igcoScroll", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, _enabled: true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!pageFormatted)
		{
			if (GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded)
			{
				return;
			}
			pageFormatted = true;
			if (string.IsNullOrEmpty(retrievedEula))
			{
				DisplayLocalEulaOrClose();
			}
			else if (GameManager.HasAcceptedLatestEula() && !viewMode)
			{
				Close();
			}
			else
			{
				SetVisibility(_visible: true);
				FormatPages(retrievedEula);
				ShowGamepadCallouts();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			float value = base.xui.playerUI.playerInput.GUIActions.scroll.Value;
			if (value == 0f)
			{
				return;
			}
			if (value > 0f)
			{
				btnPageUp_OnPressed(null, 0);
			}
			else if (value < 0f)
			{
				btnPageDown_OnPressed(null, 0);
			}
		}
		else
		{
			XUi.HandlePaging(base.xui, TryPageUp, TryPageDown, useVerticalAxis: true);
		}
		if (viewMode && base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			Close();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePageButtonVisibility()
	{
		bool flag;
		bool flag2;
		if (!background.ViewComponent.IsVisible || !pageFormatted || GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded || pages == null)
		{
			flag = false;
			flag2 = false;
		}
		else
		{
			flag = currentPage > 0;
			flag2 = currentPage < pages.Count - 1;
		}
		if (btnPageUp.ViewComponent.IsVisible != flag)
		{
			btnPageUp.ViewComponent.IsVisible = flag;
		}
		if (btnPageDown.ViewComponent.IsVisible != flag2)
		{
			btnPageDown.ViewComponent.IsVisible = flag2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.EulaVersionAccepted, GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion));
		GamePrefs.Instance.Save();
		Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPressed(XUiController _sender, int _mouseButton)
	{
		Close();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageUpAction()
	{
		btnPageUp_OnPressed(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageDownAction()
	{
		btnPageDown_OnPressed(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryPageUp()
	{
		int num = currentPage - 1;
		if (num < 0)
		{
			return false;
		}
		SetPage(num);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryPageDown()
	{
		int num = currentPage + 1;
		if (num >= pages.Count)
		{
			return false;
		}
		SetPage(num);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPageUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		TryPageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPageDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		TryPageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisibility(bool _visible)
	{
		background.ViewComponent.IsVisible = _visible;
		footerContainer.IsVisible = _visible;
		UpdatePageButtonVisibility();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "viewmode")
		{
			_value = viewMode.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
