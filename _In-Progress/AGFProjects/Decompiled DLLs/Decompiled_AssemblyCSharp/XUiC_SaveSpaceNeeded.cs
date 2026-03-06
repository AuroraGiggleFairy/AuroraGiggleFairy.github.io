using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveSpaceNeeded : XUiController
{
	public enum ConfirmationResult
	{
		Pending,
		Cancelled,
		Discarded,
		Confirmed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultTitle = "xuiSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultBody = "xuiDmSavingBody";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultCancel = "xuiCancel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultDiscard = "xuiDiscard";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultConfirm = "xuiConfirm";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultManage = "xuiDmManageSaves";

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool m_openedProperly;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDiscard;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnManage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView m_previousLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] m_protectedPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_canCancel = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_canDiscard = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyTitle = "xuiSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyBody = "xuiDmSavingBody";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyCancel = "xuiCancel";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyDiscard = "xuiDiscard";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyConfirm = "xuiConfirm";

	[PublicizedFrom(EAccessModifier.Private)]
	public string m_langKeyManage = "xuiDmManageSaves";

	[PublicizedFrom(EAccessModifier.Private)]
	public ParentControllerState m_parentControllerState;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_wasCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_wasCursorLocked;

	public bool ShouldShowDataBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SaveInfoProvider.DataLimitEnabled;
		}
	}

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SaveInfoProvider.DataLimitEnabled)
			{
				return m_pendingBytes <= m_totalAvailableBytes;
			}
			return true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ConfirmationResult Result
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "canCancel":
			_value = m_canCancel.ToString();
			return true;
		case "canDiscard":
			_value = m_canDiscard.ToString();
			return true;
		case "langKeyTitle":
			_value = m_langKeyTitle;
			return true;
		case "langKeyBody":
			_value = m_langKeyBody;
			return true;
		case "langKeyCancel":
			_value = m_langKeyCancel;
			return true;
		case "langKeyDiscard":
			_value = m_langKeyDiscard;
			return true;
		case "langKeyConfirm":
			_value = m_langKeyConfirm;
			return true;
		case "langKeyManage":
			_value = m_langKeyManage;
			return true;
		case "shouldShowDataBar":
			_value = ShouldShowDataBar.ToString();
			return true;
		case "hasSufficientSpace":
			_value = HasSufficientSpace.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		labelTitle = (XUiV_Label)GetChildById("titleText").ViewComponent;
		labelBody = (XUiV_Label)GetChildById("bodyText").ViewComponent;
		btnCancel = (XUiC_SimpleButton)GetChildById("btnCancel");
		btnDiscard = (XUiC_SimpleButton)GetChildById("btnDiscard");
		btnManage = (XUiC_SimpleButton)GetChildById("btnManage");
		btnConfirm = (XUiC_SimpleButton)GetChildById("btnConfirm");
		btnCancel.OnPressed += BtnCancel_OnPressed;
		btnDiscard.OnPressed += BtnDiscard_OnPressed;
		btnManage.OnPressed += BtnManage_OnPressed;
		btnConfirm.OnPressed += BtnConfirm_OnPressed;
		dataManagementBar = GetChildById("data_bar_controller") as XUiC_DataManagementBar;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!m_canCancel)
		{
			Log.Error("[SaveSpaceNeeded] Cancel button was pressed even though cancel is hidden?");
			return;
		}
		Result = ConfirmationResult.Cancelled;
		base.xui.playerUI.windowManager.Close(ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDiscard_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!m_canDiscard)
		{
			Log.Error("[SaveSpaceNeeded] Discard button was pressed even though discard is hidden?");
			return;
		}
		Result = ConfirmationResult.Discarded;
		base.xui.playerUI.windowManager.Close(ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		(((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID))?.Controller?.GetChildByType<XUiC_LoadingScreen>())?.SetTipsVisible(visible: false);
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!HasSufficientSpace)
		{
			Log.Error("[SaveSpaceNeeded] Confirm button was pressed even though there isn't enough free space?");
			return;
		}
		Result = ConfirmationResult.Confirmed;
		base.xui.playerUI.windowManager.Close(ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		(((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID))?.Controller?.GetChildByType<XUiC_LoadingScreen>())?.SetTipsVisible(visible: true);
		UpdateBarValues();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!m_openedProperly)
		{
			Log.Error("[SaveSpaceNeeded] XUiC_SaveSpaceNeeded should be opened with the static Open method so that InitInternal is executed.");
			base.xui.playerUI.windowManager.Close(ID);
			return;
		}
		m_openedProperly = false;
		Result = ConfirmationResult.Pending;
		m_wasCursorHidden = base.xui.playerUI.CursorController.GetCursorHidden();
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		m_wasCursorLocked = base.xui.playerUI.CursorController.Locked;
		base.xui.playerUI.CursorController.Locked = false;
		m_previousLockView = base.xui.playerUI.CursorController.lockNavigationToView;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.CursorController.SetCursorHidden(m_wasCursorHidden);
		base.xui.playerUI.CursorController.SetNavigationLockView(m_previousLockView);
		base.xui.playerUI.CursorController.Locked = m_wasCursorLocked;
		if (m_protectedPaths != null)
		{
			string[] protectedPaths = m_protectedPaths;
			foreach (string text in protectedPaths)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, isProtected: false);
				}
			}
		}
		m_parentControllerState?.Restore();
		m_pendingBytes = 0L;
		m_protectedPaths = null;
		m_canCancel = true;
		m_canDiscard = true;
		m_langKeyTitle = "xuiSave";
		m_langKeyBody = "xuiDmSavingBody";
		m_langKeyCancel = "xuiCancel";
		m_langKeyDiscard = "xuiDiscard";
		m_langKeyConfirm = "xuiConfirm";
		m_langKeyManage = "xuiDmManageSaves";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitInternal(long pendingBytes, string[] protectedPaths, XUiController parentController, bool autoConfirm, bool canCancel, bool canDiscard, string langKeyTitle, string langKeyBody, string langKeyCancel, string langKeyDiscard, string langKeyConfirm, string langKeyManage)
	{
		m_pendingBytes = pendingBytes;
		m_protectedPaths = protectedPaths;
		m_canCancel = canCancel;
		m_canDiscard = canDiscard;
		m_langKeyTitle = (string.IsNullOrWhiteSpace(langKeyTitle) ? "xuiSave" : langKeyTitle);
		m_langKeyBody = (string.IsNullOrWhiteSpace(langKeyBody) ? "xuiDmSavingBody" : langKeyBody);
		m_langKeyCancel = (string.IsNullOrWhiteSpace(langKeyCancel) ? "xuiCancel" : langKeyCancel);
		m_langKeyDiscard = (string.IsNullOrWhiteSpace(langKeyDiscard) ? "xuiDiscard" : langKeyDiscard);
		m_langKeyConfirm = (string.IsNullOrWhiteSpace(langKeyConfirm) ? "xuiConfirm" : langKeyConfirm);
		m_langKeyManage = (string.IsNullOrWhiteSpace(langKeyManage) ? "xuiDmManageSaves" : langKeyManage);
		m_parentControllerState = new ParentControllerState(parentController);
		m_parentControllerState.Hide();
		if (m_protectedPaths != null)
		{
			string[] protectedPaths2 = m_protectedPaths;
			foreach (string text in protectedPaths2)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, isProtected: true);
				}
			}
		}
		if (!autoConfirm || SaveInfoProvider.DataLimitEnabled)
		{
			UpdateBarValues();
			if (m_pendingBytes != 0L)
			{
				Log.Out("[SaveSpaceNeeded] Pending Bytes: " + m_pendingBytes.FormatSize(includeOriginalBytes: true) + ", Total Available Bytes: " + m_totalAvailableBytes.FormatSize(includeOriginalBytes: true));
			}
		}
		if (autoConfirm && HasSufficientSpace)
		{
			if (m_pendingBytes != 0L)
			{
				Log.Out("[SaveSpaceNeeded] Auto-Confirming.");
			}
			BtnConfirm_OnPressed(btnConfirm, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarValues()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		dataManagementBar.ViewComponent.IsVisible = SaveInfoProvider.DataLimitEnabled;
		dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		dataManagementBar.SetPendingBytes(m_pendingBytes);
		m_totalAvailableBytes = instance.TotalAvailableBytes;
		RefreshBindings();
		if (HasSufficientSpace)
		{
			btnConfirm.SelectCursorElement(_withDelay: true);
		}
		else
		{
			btnManage.SelectCursorElement(_withDelay: true);
		}
	}

	public static XUiC_SaveSpaceNeeded Open(long pendingBytes, string protectedPath, XUiController parentController = null, bool autoConfirm = false, bool canCancel = true, bool canDiscard = true, string title = null, string body = null, string cancel = null, string discard = null, string confirm = null, string manage = null)
	{
		return Open(pendingBytes, new string[1] { protectedPath }, parentController, autoConfirm, canCancel, canDiscard, title, body, cancel, discard, confirm, manage);
	}

	public static XUiC_SaveSpaceNeeded Open(long pendingBytes, string[] protectedPaths, XUiController parentController = null, bool autoConfirm = false, bool canCancel = true, bool canDiscard = true, string title = null, string body = null, string cancel = null, string discard = null, string confirm = null, string manage = null)
	{
		GUIWindowManager windowManager = LocalPlayerUI.primaryUI.xui.playerUI.windowManager;
		m_openedProperly = true;
		windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true, _bCloseAllOpenWindows: false);
		m_openedProperly = false;
		XUiC_SaveSpaceNeeded xUiC_SaveSpaceNeeded = ((XUiWindowGroup)windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_SaveSpaceNeeded>();
		if (xUiC_SaveSpaceNeeded == null)
		{
			Log.Error("[SaveSpaceNeeded] Failed to retrieve reference to XUiC_SaveSpaceNeeded instance.");
		}
		else
		{
			xUiC_SaveSpaceNeeded.InitInternal(pendingBytes, protectedPaths, parentController, autoConfirm, canCancel, canDiscard, title, body, cancel, discard, confirm, manage);
		}
		return xUiC_SaveSpaceNeeded;
	}
}
