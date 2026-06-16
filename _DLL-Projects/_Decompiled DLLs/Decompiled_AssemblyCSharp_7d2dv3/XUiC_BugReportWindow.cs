using System.Collections;
using Backtrace.Unity.Model;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastSubmissionTime = -1f;

	[XuiBindComponent("txtDescription", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtDescription;

	[XuiBindComponent("btnSubmit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSubmit;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("comboAttachScreenshot", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool comboAttachScreenshot;

	[XuiBindComponent("comboAttachSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool comboAttachSave;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_BugReportSaveSelect saveSelectWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo selectedSaveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uploading;

	[XuiXmlBinding("intimeout")]
	public bool InTimeout
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (lastSubmissionTime >= 0f)
			{
				return Time.time - lastSubmissionTime < 600f;
			}
			return false;
		}
	}

	[XuiXmlBinding("description")]
	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return txtDescription?.Text.Trim() ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			txtDescription.Text = value ?? "";
			IsDirty = true;
		}
	}

	[XuiXmlBinding("attach_saves_enabled")]
	public bool AttachSavesEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BacktraceUtils.BugReportAttachSaveFeature;
		}
	}

	[XuiXmlBinding("game_running")]
	public bool GameRunningBinding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameRunning;
		}
	}

	public static bool GameRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.gameStateManager.IsGameStarted();
		}
	}

	public override void Init()
	{
		base.Init();
		windowGroup.isEscClosable = false;
		saveSelectWindow.GetChildByType<XUiC_BugReportSavesList>().SelectionChanged += List_SelectionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void List_SelectionChanged(XUiC_List<XUiC_BugReportSavesList.ListEntry> _list, XUiC_BugReportSavesList.ListEntry _previousEntry, XUiC_BugReportSavesList.ListEntry _newEntry)
	{
		selectedSaveInfo = _newEntry?.SaveEntryInfo;
		IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "comboAttachSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachSave_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (!GameRunning)
		{
			saveSelectWindow.ViewComponent.IsVisible = _newValue;
			if (!_newValue)
			{
				selectedSaveInfo = null;
				saveSelectWindow?.List.ClearSelection();
			}
		}
		IsDirty = true;
	}

	public static void Open()
	{
		LocalPlayerUI primaryUI = LocalPlayerUI.primaryUI;
		XUiC_BugReportWindow childByType = primaryUI.xui.GetChildByType<XUiC_BugReportWindow>();
		primaryUI.windowManager.Open(childByType.windowGroup, GameRunning);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Description = "";
		selectedSaveInfo = null;
		uploading = false;
		comboAttachSave.Value = false;
		comboAttachScreenshot.Value = false;
		if (GameRunning)
		{
			GameManager.Instance.Pause(_bOn: true);
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		uploading = false;
		if (GameRunning)
		{
			GameManager.Instance.Pause(_bOn: false);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!GameRunning && saveSelectWindow != null && saveSelectWindow.ViewComponent.IsVisible != comboAttachSave.Value)
		{
			saveSelectWindow.ViewComponent.IsVisible = comboAttachSave.Value;
		}
		else if (GameRunning && saveSelectWindow != null && saveSelectWindow.ViewComponent.IsVisible)
		{
			saveSelectWindow.ViewComponent.IsVisible = false;
		}
		if (!uploading && (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased || xui.playerUI.playerInput.GUIActions.Cancel.WasReleased))
		{
			xui.playerUI.windowManager.Close(windowGroup);
		}
		handleDirtyUpdateDefault();
	}

	[XuiBindEvent("OnChangeHandler", "txtDescription")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtDescriptionOnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnSubmit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmitOnPressed(XUiController _sender, int _mouseButton)
	{
		if (btnSubmit.ViewComponent.Enabled)
		{
			lastSubmissionTime = Time.time;
			ThreadManager.StartCoroutine(submitRoutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator submitRoutine()
	{
		_ = PathAbstractions.AbstractedLocation.None;
		string saveName;
		string worldName;
		string saveDir;
		PathAbstractions.AbstractedLocation abstractedLocation;
		if (GameRunning)
		{
			saveName = GamePrefs.GetString(EnumGamePrefs.GameName);
			worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			saveDir = ((!GameManager.Instance.World.IsRemote()) ? GameIO.GetSaveGameDir() : GameIO.GetSaveGameLocalDir());
			abstractedLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
		}
		else
		{
			saveName = selectedSaveInfo.Name;
			saveDir = selectedSaveInfo.SaveDir;
			worldName = selectedSaveInfo.WorldEntry.Name;
			if (selectedSaveInfo.WorldEntry.Location.Type == PathAbstractions.EAbstractedLocationType.None && !selectedSaveInfo.WorldEntry.Type.Equals(SaveInfoProvider.DeletedWorldsType))
			{
				PathAbstractions.AbstractedLocation? userWorldSelection = null;
				XUiC_WorldSelectionPopup.Open(xui, "xuiWorldConflictBugReport", worldName, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					userWorldSelection = PathAbstractions.AbstractedLocation.None;
				}, [PublicizedFrom(EAccessModifier.Internal)] (PathAbstractions.AbstractedLocation _location) =>
				{
					userWorldSelection = _location;
				});
				while (!userWorldSelection.HasValue)
				{
					yield return null;
				}
				if (userWorldSelection.Value.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					yield break;
				}
				abstractedLocation = userWorldSelection.Value;
			}
			else
			{
				abstractedLocation = selectedSaveInfo.WorldEntry.Location;
			}
		}
		PathAbstractions.EAbstractedLocationType type = abstractedLocation.Type;
		if ((uint)(type - 1) > 1u)
		{
			abstractedLocation = PathAbstractions.AbstractedLocation.None;
		}
		string worldDir = ((abstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None) ? abstractedLocation.FullPath : null);
		uploading = true;
		string screenshotPath = null;
		if (comboAttachScreenshot.Value)
		{
			base.ViewComponent.IsVisible = false;
			GameManager.Instance.Pause(_bOn: false);
			for (int i = 0; i < 10; i++)
			{
				yield return null;
			}
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.File, PlatformApplicationManager.Application.temporaryCachePath + "/" + Application.productName), Log.Exception);
			GameManager.Instance.Pause(_bOn: true);
			yield return null;
			base.ViewComponent.IsVisible = true;
			screenshotPath = GameUtils.lastSavedScreenshotFilename;
		}
		yield return null;
		XUiC_ProgressWindow.Open(xui.playerUI, Localization.Get("xuiBugReportUploading"), null, _modal: false, _notEscClosable: true, _useShadow: true);
		yield return new WaitForSecondsRealtime(0.5f);
		BacktraceUtils.SendBugReport(Description, worldName, saveName, worldDir, saveDir, comboAttachSave.Value, screenshotPath, BugReportCallBack);
		[PublicizedFrom(EAccessModifier.Private)]
		void BugReportCallBack(BacktraceResult _result)
		{
			Log.Out("Bug Report Send callback: {0}", (_result == null) ? "null" : _result.message);
			handlePostSubmissionClose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePostSubmissionClose()
	{
		XUiC_ProgressWindow.Close(xui.playerUI);
		xui.playerUI.windowManager.Close(base.WindowGroup);
		XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiBugReportHeader"), Localization.Get("xuiBugReportSubmitted"), null, _openMainMenuOnClose: false, GameRunning);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelOnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(base.WindowGroup);
	}
}
