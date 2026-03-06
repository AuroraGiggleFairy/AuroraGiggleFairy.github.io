using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsProfiles : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_USER_PROFILES = -1;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ProfilesList profiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerProfileCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileCreate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileDelete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProfileEdit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteProfilePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteProfileText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel createProfilePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput createProfileName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton createProfileConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action OnCloseAction;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		profiles = GetChildByType<XUiC_ProfilesList>();
		profiles.SelectionChanged += Profiles_OnSelectionChanged;
		btnOk = GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		btnOk.OnPressed += BtnOk_OnPressed;
		btnProfileCreate = GetChildById("btnProfileCreate").GetChildByType<XUiC_SimpleButton>();
		btnProfileCreate.OnPressed += BtnProfileCreate_OnPressed;
		btnProfileCreate.ViewComponent.NavDownTarget = btnOk.ViewComponent;
		btnProfileDelete = GetChildById("btnProfileDelete").GetChildByType<XUiC_SimpleButton>();
		btnProfileDelete.OnPressed += BtnProfileDelete_OnPressed;
		btnProfileDelete.ViewComponent.NavDownTarget = btnOk.ViewComponent;
		btnProfileEdit = GetChildById("btnProfileEdit").GetChildByType<XUiC_SimpleButton>();
		btnProfileEdit.OnPressed += BtnProfileEdit_OnPressed;
		btnProfileEdit.ViewComponent.NavDownTarget = btnOk.ViewComponent;
		deleteProfilePanel = (XUiV_Panel)GetChildById("deleteProfilePanel").ViewComponent;
		((XUiC_SimpleButton)deleteProfilePanel.Controller.GetChildById("btnCancel")).OnPressed += BtnCancelDelete_OnPressed;
		((XUiC_SimpleButton)deleteProfilePanel.Controller.GetChildById("btnConfirm")).OnPressed += BtnConfirmDelete_OnPressed;
		deleteProfileText = (XUiV_Label)deleteProfilePanel.Controller.GetChildById("deleteText").ViewComponent;
		createProfilePanel = (XUiV_Panel)GetChildById("createProfilePanel").ViewComponent;
		((XUiC_SimpleButton)createProfilePanel.Controller.GetChildById("btnCancel")).OnPressed += BtnCancelCreate_OnPressed;
		createProfileConfirm = (XUiC_SimpleButton)createProfilePanel.Controller.GetChildById("btnConfirm");
		createProfileConfirm.OnPressed += BtnConfirmCreate_OnPressed;
		createProfileName = (XUiC_TextInput)createProfilePanel.Controller.GetChildById("createProfileName");
		createProfileName.OnSubmitHandler += CreateProfileName_OnSubmitHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		OpenCustomCharacterWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteProfilePanel.IsVisible = true;
		deleteProfileText.Text = string.Format(Localization.Get("xuiProfilesDeleteConfirmation"), Utils.EscapeBbCodes(profiles.SelectedEntry.GetEntry().name));
		base.xui.playerUI.CursorController.SetNavigationLockView(deleteProfilePanel, deleteProfilePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		createProfilePanel.IsVisible = true;
		createProfileName.Text = "";
		base.xui.playerUI.CursorController.SetNavigationLockView(createProfilePanel);
		createProfileName.SelectOrVirtualKeyboard();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Profiles_OnSelectionChanged(XUiC_ListEntry<XUiC_ProfilesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ProfilesList.ListEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			ProfileSDF.SetSelectedProfile(_newEntry.GetEntry().name);
			ProfileSDF.Save();
		}
		updateButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnProfileDelete.SelectCursorElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		deleteProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnProfileDelete.SelectCursorElement();
		ProfileSDF.DeleteProfile(profiles.SelectedEntry.GetEntry().name);
		playerProfileCount--;
		string selectedProfile = "";
		string[] array = ProfileSDF.GetProfiles();
		if (array.Length != 0)
		{
			selectedProfile = array[0];
		}
		ProfileSDF.SetSelectedProfile(selectedProfile);
		profiles.RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		createProfilePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		btnProfileCreate.SelectCursorElement();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (createProfileConfirm.Enabled)
		{
			createProfilePanel.IsVisible = false;
			base.xui.playerUI.CursorController.SetNavigationLockView(null);
			btnProfileCreate.SelectCursorElement();
			string text = createProfileName.Text.Trim();
			ProfileSDF.SaveProfile(text, "", _isMale: true, "White", 1, "Blue01", "", "", "", "", "");
			ProfileSDF.SetSelectedProfile(text);
			ProfileSDF.Save();
			playerProfileCount++;
			profiles.RebuildList();
			OpenCustomCharacterWindow();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenCustomCharacterWindow()
	{
		Action onCloseAction = OnCloseAction;
		OnCloseAction = null;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		OnCloseAction = onCloseAction;
		base.xui.playerUI.windowManager.Open(XUiC_CustomCharacterWindowGroup.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnInputAbortedHandler(XUiController _sender)
	{
		BtnCancelCreate_OnPressed(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		ThreadManager.AddSingleTaskMainThread("OpenProfileEditorWindow", [PublicizedFrom(EAccessModifier.Private)] (object _func) =>
		{
			BtnConfirmCreate_OnPressed(this, -1);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateButtonStates()
	{
		bool flag = ProfileSDF.CurrentProfileName().Length != 0;
		bool flag2 = profiles.SelectedEntry != null;
		btnOk.Enabled = flag && flag2;
		bool flag3 = flag && flag2;
		PlayerProfile playerProfile = PlayerProfile.LoadLocalProfile();
		Archetype archetype = Archetype.GetArchetype(playerProfile.ProfileArchetype);
		if (archetype == null)
		{
			archetype = Archetype.GetArchetype(playerProfile.IsMale ? "BaseMale" : "BaseFemale");
		}
		if (archetype != null)
		{
			flag3 &= archetype.CanCustomize;
		}
		btnProfileEdit.Enabled = flag3;
		btnProfileDelete.Enabled = flag3;
		btnProfileCreate.Enabled = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		string text = ProfileSDF.CurrentProfileName();
		playerProfileCount = 0;
		foreach (XUiC_ProfilesList.ListEntry item in profiles.AllEntries())
		{
			ProfileSDF.SetSelectedProfile(item.name);
			if (string.IsNullOrEmpty(text))
			{
				text = item.name;
				profiles.SelectByName(text);
			}
			if (Archetype.GetArchetype(PlayerProfile.LoadLocalProfile().ProfileArchetype).CanCustomize)
			{
				playerProfileCount++;
			}
		}
		ProfileSDF.SetSelectedProfile(text);
		deleteProfilePanel.IsVisible = false;
		createProfilePanel.IsVisible = false;
		updateButtonStates();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (OnCloseAction != null)
		{
			OnCloseAction();
			OnCloseAction = null;
		}
		else
		{
			base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (createProfilePanel.IsVisible)
		{
			string text = createProfileName.Text.Trim();
			bool flag = text.Length > 0 && text.IndexOf('.') < 0 && !ProfileSDF.ProfileExists(text);
			createProfileConfirm.Enabled = flag;
			createProfileName.ActiveTextColor = (flag ? Color.white : Color.red);
		}
	}

	public static void Open(XUi _xuiInstance, Action _onCloseAction = null)
	{
		_xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_OptionsProfiles>().OnCloseAction = _onCloseAction;
		_xuiInstance.playerUI.windowManager.Open(ID, _bModal: true);
	}
}
