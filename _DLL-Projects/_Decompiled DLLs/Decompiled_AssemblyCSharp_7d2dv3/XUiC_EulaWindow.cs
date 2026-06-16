using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EulaWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int AlphanumericPageCharacterLimit = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ScriptPageCharacterLimit = 1000;

	public static string RetrievedEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[XuiBindComponent("btnAccept", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnAccept;

	[XuiBindComponent("btnDecline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDecline;

	[XuiBindComponent("btnDone", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDone;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView scrollView;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollBar scrollBar;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Table textTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultEula;

	[PublicizedFrom(EAccessModifier.Private)]
	public int defaultEulaVersion = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, string> pagesDict = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageFormatted;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool viewMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiVisible;

	[XuiXmlBinding("pages")]
	public Dictionary<string, string> PagesDict
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return pagesDict;
		}
	}

	[XuiXmlBinding("pagecount")]
	public int PageCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return pagesDict.Count;
		}
	}

	[XuiXmlBinding("viewmode")]
	public bool ViewMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return viewMode;
		}
	}

	[XuiXmlBinding("allowaccept")]
	public bool AllowAccept
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return allowAccept;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (allowAccept != value)
			{
				allowAccept = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("uivisible")]
	public bool UIVisible
	{
		get
		{
			return uiVisible;
		}
		set
		{
			if (uiVisible != value)
			{
				uiVisible = value;
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		id = base.WindowGroup.Id;
		TextAsset textAsset = Resources.Load<TextAsset>("Data/EULA/eula_" + Localization.ActiveLanguage.ToLower());
		if (textAsset != null)
		{
			loadDefaultXML(textAsset.bytes);
		}
		else
		{
			Log.Error("Could not load default EULA text asset");
		}
	}

	public static void Open(XUi _xui, bool _viewMode = false)
	{
		viewMode = _viewMode;
		_xui.playerUI.windowManager.Open(id, _bModal: true, _bIsNotEscClosable: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		scrollBar.ScrollPosition = 0f;
		pageFormatted = false;
		UIVisible = false;
		AllowAccept = false;
		if (viewMode)
		{
			btnDone.SelectCursorElement(_withDelay: true);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		scrollBar.ScrollPosition = 0f;
		scrollView.ResetPosition();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		if (viewMode)
		{
			xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, _bModal: true);
		}
		else
		{
			XUiC_MainMenu.Open(xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadDefaultXML(byte[] _data)
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

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (!pageFormatted)
		{
			if (GameManager.UpdatingRemoteResources || !GameManager.RemoteResourcesLoaded)
			{
				return;
			}
			pageFormatted = true;
			displayEulaOrClose();
		}
		if (!viewMode && (double)scrollBar.ScrollPosition >= 0.98 && !AllowAccept)
		{
			xui.playerUI.CursorController.SetNavigationTarget(btnDecline.ViewComponent);
			AllowAccept = true;
		}
		if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			close();
		}
		if (!viewMode && xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			btnAccept_OnPressed(this, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void displayEulaOrClose()
	{
		bool flag = string.IsNullOrEmpty(RetrievedEula);
		bool flag2 = (flag ? (GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) >= defaultEulaVersion) : GameManager.HasAcceptedLatestEula());
		if (!viewMode && flag2)
		{
			close();
			return;
		}
		UIVisible = true;
		string content = (flag ? defaultEula : RetrievedEula);
		formatPages(content);
		ThreadManager.StartCoroutine(updateScrolling());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator updateScrolling()
	{
		textTable.Reposition();
		yield return null;
		textTable.Reposition();
		scrollView.ResetPosition();
		for (int i = 0; i < 5; i++)
		{
			yield return null;
			scrollView.UpdatePosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void formatPages(string _content)
	{
		_content = _content.Replace("\t", "  ");
		pagesDict.Clear();
		string[] array = _content.Split('\n');
		int num;
		switch (Localization.ActiveLanguage)
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
		int num2 = num;
		StringBuilder stringBuilder = new StringBuilder();
		int num3 = 0;
		while (num3 < array.Length)
		{
			stringBuilder.AppendLine(array[num3]);
			stringBuilder.AppendLine();
			num3++;
			while (stringBuilder.Length < num2 && num3 < array.Length)
			{
				stringBuilder.AppendLine(array[num3]);
				stringBuilder.AppendLine();
				num3++;
			}
			stringBuilder.Length--;
			pagesDict.Add(pagesDict.Count.ToString(), stringBuilder.ToString());
			stringBuilder.Clear();
		}
	}

	[XuiBindEvent("OnPress", "btnAccept")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (btnAccept.ViewComponent.Enabled)
		{
			GamePrefs.Set(EnumGamePrefs.EulaVersionAccepted, GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion));
			GamePrefs.Instance.Save();
			close();
		}
	}

	[XuiBindEvent("OnPress", "btnDecline")]
	[XuiBindEvent("OnPress", "btnDone")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPressed(XUiController _sender, int _mouseButton)
	{
		close();
	}
}
