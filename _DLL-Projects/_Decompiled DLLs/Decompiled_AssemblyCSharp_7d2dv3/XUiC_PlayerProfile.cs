using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerProfile : XUiC_PlayGameDialogBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxUserProfiles = -1;

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ProfilesList profiles;

	[XuiBindComponent("btnProfileCreate", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileCreate;

	[XuiBindComponent("btnProfileDelete", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileDelete;

	[XuiBindComponent("btnProfileEdit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileEdit;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SDCSPreviewWindow previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseAction;

	[XuiXmlBinding("user_profile_count")]
	public int UserProfileCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (profiles == null)
			{
				return 0;
			}
			int num = 0;
			foreach (XUiC_ProfilesList.ListEntry item in profiles.AllEntries())
			{
				if (Archetype.GetArchetype(PlayerProfile.LoadProfile(item.Name).ProfileArchetype).CanCustomize)
				{
					num++;
				}
			}
			return num;
		}
	}

	[XuiXmlBinding("can_create_userprofile")]
	public bool MaxUserProfilesReached
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("can_modify_profile")]
	public bool CanModifyProfile
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (profiles == null)
			{
				return false;
			}
			bool num = !string.IsNullOrEmpty(ProfileSDF.CurrentProfileName());
			bool flag = profiles.SelectedEntryData != null;
			bool flag2 = num && flag;
			if (flag2)
			{
				PlayerProfile playerProfile = PlayerProfile.LoadLocalProfile();
				Archetype archetype = Archetype.GetArchetype(playerProfile.ProfileArchetype) ?? Archetype.GetArchetype(playerProfile.IsMale ? "BaseMale" : "BaseFemale");
				if (archetype != null)
				{
					flag2 &= archetype.CanCustomize;
				}
			}
			return flag2;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnProfileEdit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		openCustomCharacterWindow();
	}

	[XuiBindEvent("OnPress", "btnProfileDelete")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ProfilesList.ListEntry selectedEntry = profiles.SelectedEntryData;
		XUiC_MessageBoxWindowGroup.ShowCustom(xui, Localization.Get("xuiDeleteProfile"), string.Format(Localization.Get("xuiProfilesDeleteConfirmation"), Utils.EscapeBbCodes(selectedEntry.Name)), [PublicizedFrom(EAccessModifier.Internal)] (XUiC_MessageBoxWindowGroup _box) =>
		{
			_box.Buttons[0].DefaultConfirm("btnConfirm", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				btnProfileDelete.SelectCursorElement();
				ProfileSDF.DeleteProfile(selectedEntry.Name);
				ProfileSDF.SetSelectedProfile("");
				profiles.RebuildList();
				selectFirstProfile();
				IsDirty = true;
			}, _enabled: true, 0f, 1.5f);
			_box.Buttons[2].DefaultCancel("xuiCancel", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				btnProfileDelete.SelectCursorElement();
			});
		}, _openMainMenuOnClose: false, _modal: false);
	}

	[XuiBindEvent("OnPress", "btnProfileCreate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_PlayerProfileCreate.Open(xui, [PublicizedFrom(EAccessModifier.Private)] (string _name) =>
		{
			if (_name == null)
			{
				btnProfileCreate.SelectCursorElement();
			}
			else
			{
				ProfileSDF.SaveProfile(_name, "", _isMale: true, "White", 1, "Blue01", "", "", "", "", "");
				ProfileSDF.SetSelectedProfile(_name);
				ProfileSDF.Save();
				profiles.RebuildList();
				openCustomCharacterWindow();
			}
		});
		xui.playerUI.windowManager.Open("playerProfilesCreate", _bModal: false);
	}

	[XuiBindEvent("SelectionChanged", "profiles")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Profiles_OnSelectionChanged(XUiC_List<XUiC_ProfilesList.ListEntry> _list, XUiC_ProfilesList.ListEntry _previousEntry, XUiC_ProfilesList.ListEntry _newEntry)
	{
		if (_newEntry != null)
		{
			ProfileSDF.SetSelectedProfile(_newEntry.Name);
			ProfileSDF.Save();
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openCustomCharacterWindow()
	{
		previewWindow.ViewComponent.IsVisible = false;
		XUiC_CustomCharacterWindowGroup.Open(xui, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			previewWindow.ViewComponent.IsVisible = true;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectFirstProfile()
	{
		string text = ProfileSDF.CurrentProfileName();
		if (!string.IsNullOrEmpty(text))
		{
			return;
		}
		using (IEnumerator<XUiC_ProfilesList.ListEntry> enumerator = profiles.AllEntries().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				text = enumerator.Current.Name;
				profiles.SelectByName(text);
			}
		}
		ProfileSDF.SetSelectedProfile(text);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		selectFirstProfile();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (onCloseAction != null)
		{
			onCloseAction();
			onCloseAction = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	public static void Open(XUi _xuiInstance, Action _onCloseAction = null)
	{
		_xuiInstance.GetChildByType<XUiC_PlayerProfile>().onCloseAction = _onCloseAction;
		_xuiInstance.playerUI.windowManager.Open(ID, _bModal: true);
	}
}
