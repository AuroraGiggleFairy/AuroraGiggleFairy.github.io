using Audio;
using Platform;
using UnityEngine;
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
	public static bool mOpenedProperly;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnDiscard", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscard;

	[XuiBindComponent("btnManage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnManage;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public long mPendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] mProtectedPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataStorageType mStorageType;

	[PublicizedFrom(EAccessModifier.Private)]
	public long mTotalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mWasCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mWasCursorLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip mPromptReadyClip;

	[XuiXmlBinding("canCancel")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CanCancel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlBinding("canDiscard")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CanDiscard
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlBinding("langKeyTitle")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiSave";

	[XuiXmlBinding("langKeyBody")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyBody
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDmSavingBody";

	[XuiXmlBinding("langKeyCancel")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyCancel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiCancel";

	[XuiXmlBinding("langKeyDiscard")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyDiscard
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDiscard";

	[XuiXmlBinding("langKeyConfirm")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyConfirm
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiConfirm";

	[XuiXmlBinding("langKeyManage")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LangKeyManage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDmManageSaves";

	[XuiXmlBinding("shouldShowDataBar")]
	public bool ShouldShowDataBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SaveInfoProvider.DataLimitEnabled)
			{
				return mStorageType.UsesDataLimit();
			}
			return false;
		}
	}

	[XuiXmlBinding("hasSufficientSpace")]
	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SaveInfoProvider.DataLimitEnabled && mStorageType.UsesDataLimit())
			{
				return mPendingBytes <= mTotalAvailableBytes;
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

	[XuiXmlAttribute("prompt_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			mPromptReadyClip = _o;
		});
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!CanCancel)
		{
			Log.Error("[SaveSpaceNeeded] Cancel button was pressed even though cancel is hidden?");
			return;
		}
		Result = ConfirmationResult.Cancelled;
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("OnPress", "btnDiscard")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDiscard_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!CanDiscard)
		{
			Log.Error("[SaveSpaceNeeded] Discard button was pressed even though discard is hidden?");
			return;
		}
		Result = ConfirmationResult.Discarded;
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("OnPress", "btnManage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		(((XUiWindowGroup)xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID))?.Controller?.GetChildByType<XUiC_LoadingScreen>())?.SetTipsVisible(_visible: false);
		XUiC_DataManagement.OpenDataManagementWindow(this, OnDataManagementWindowClosed);
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!HasSufficientSpace)
		{
			Log.Error("[SaveSpaceNeeded] Confirm button was pressed even though there isn't enough free space?");
			return;
		}
		Result = ConfirmationResult.Confirmed;
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		(((XUiWindowGroup)xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID))?.Controller?.GetChildByType<XUiC_LoadingScreen>())?.SetTipsVisible(_visible: true);
		updateBarValues();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!mOpenedProperly)
		{
			Log.Error("[SaveSpaceNeeded] XUiC_SaveSpaceNeeded should be opened with the static Open method so that initInternal is executed.");
		}
		mOpenedProperly = false;
		if (mProtectedPaths != null)
		{
			string[] array = mProtectedPaths;
			foreach (string text in array)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, isProtected: true);
				}
			}
		}
		if (ShouldShowDataBar)
		{
			updateBarValues();
			if (mPendingBytes != 0L)
			{
				Log.Out("[SaveSpaceNeeded] Pending Bytes: " + mPendingBytes.FormatSize(includeOriginalBytes: true) + ", Total Available Bytes: " + mTotalAvailableBytes.FormatSize(includeOriginalBytes: true));
			}
		}
		Manager.PlayXUiSound(mPromptReadyClip, 1f);
		mWasCursorHidden = xui.playerUI.CursorController.GetCursorHidden();
		xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		mWasCursorLocked = xui.playerUI.CursorController.Locked;
		xui.playerUI.CursorController.Locked = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.CursorController.SetCursorHidden(mWasCursorHidden);
		xui.playerUI.CursorController.Locked = mWasCursorLocked;
		if (mProtectedPaths != null)
		{
			string[] array = mProtectedPaths;
			foreach (string text in array)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, isProtected: false);
				}
			}
		}
		mPendingBytes = 0L;
		mProtectedPaths = null;
		CanCancel = true;
		CanDiscard = true;
		LangKeyTitle = "xuiSave";
		LangKeyBody = "xuiDmSavingBody";
		LangKeyCancel = "xuiCancel";
		LangKeyDiscard = "xuiDiscard";
		LangKeyConfirm = "xuiConfirm";
		LangKeyManage = "xuiDmManageSaves";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initInternal(long _pendingBytes, string[] _protectedPaths, UserDataStorageType _storageType, bool _canCancel, bool _canDiscard, string _langKeyTitle, string _langKeyBody, string _langKeyCancel, string _langKeyDiscard, string _langKeyConfirm, string _langKeyManage)
	{
		mPendingBytes = _pendingBytes;
		mProtectedPaths = _protectedPaths;
		mStorageType = _storageType;
		CanCancel = _canCancel;
		CanDiscard = _canDiscard;
		LangKeyTitle = (string.IsNullOrWhiteSpace(_langKeyTitle) ? "xuiSave" : _langKeyTitle);
		LangKeyBody = (string.IsNullOrWhiteSpace(_langKeyBody) ? "xuiDmSavingBody" : _langKeyBody);
		LangKeyCancel = (string.IsNullOrWhiteSpace(_langKeyCancel) ? "xuiCancel" : _langKeyCancel);
		LangKeyDiscard = (string.IsNullOrWhiteSpace(_langKeyDiscard) ? "xuiDiscard" : _langKeyDiscard);
		LangKeyConfirm = (string.IsNullOrWhiteSpace(_langKeyConfirm) ? "xuiConfirm" : _langKeyConfirm);
		LangKeyManage = (string.IsNullOrWhiteSpace(_langKeyManage) ? "xuiDmManageSaves" : _langKeyManage);
		mTotalAvailableBytes = SaveInfoProvider.Instance.TotalAvailableBytes;
		Result = ConfirmationResult.Pending;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarValues()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		dataManagementBar.ViewComponent.IsVisible = SaveInfoProvider.DataLimitEnabled;
		dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		dataManagementBar.SetPendingBytes(mPendingBytes);
		mTotalAvailableBytes = instance.TotalAvailableBytes;
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

	public static XUiC_SaveSpaceNeeded Open(long _pendingBytes, string _protectedPath, UserDataStorageType _storageType, bool _onlyShowOnInsufficientSpace = false, bool _canCancel = true, bool _canDiscard = true, string _title = null, string _body = null, string _cancel = null, string _discard = null, string _confirm = null, string _manage = null)
	{
		return Open(_pendingBytes, new string[1] { _protectedPath }, _storageType, _onlyShowOnInsufficientSpace, _canCancel, _canDiscard, _title, _body, _cancel, _discard, _confirm, _manage);
	}

	public static XUiC_SaveSpaceNeeded Open(long _pendingBytes, string[] _protectedPaths, UserDataStorageType _storageType, bool _onlyShowOnInsufficientSpace = false, bool _canCancel = true, bool _canDiscard = true, string _title = null, string _body = null, string _cancel = null, string _discard = null, string _confirm = null, string _manage = null)
	{
		if (_onlyShowOnInsufficientSpace)
		{
			if (!SaveInfoProvider.DataLimitEnabled || !_storageType.UsesDataLimit())
			{
				return null;
			}
			if (_pendingBytes <= SaveInfoProvider.Instance.TotalAvailableBytes)
			{
				return null;
			}
		}
		GUIWindowManager windowManager = LocalPlayerUI.primaryUI.xui.playerUI.windowManager;
		XUiC_SaveSpaceNeeded xUiC_SaveSpaceNeeded = ((XUiWindowGroup)windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_SaveSpaceNeeded>();
		if (xUiC_SaveSpaceNeeded == null)
		{
			Log.Error("[SaveSpaceNeeded] Failed to retrieve reference to XUiC_SaveSpaceNeeded instance.");
			return null;
		}
		xUiC_SaveSpaceNeeded.initInternal(_pendingBytes, _protectedPaths, _storageType, _canCancel, _canDiscard, _title, _body, _cancel, _discard, _confirm, _manage);
		mOpenedProperly = true;
		windowManager.Open(ID, _bModal: false);
		mOpenedProperly = false;
		return xUiC_SaveSpaceNeeded;
	}
}
