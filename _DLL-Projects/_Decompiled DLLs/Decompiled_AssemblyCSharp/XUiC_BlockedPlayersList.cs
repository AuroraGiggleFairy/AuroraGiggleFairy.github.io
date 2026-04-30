using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BlockedPlayersList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel noClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid blockedPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersBlockedListEntry[] blockedEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging blockedPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label blockedCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid recentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersRecentListEntry[] recentEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging recentPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label recentCounter;

	public override void Init()
	{
		base.Init();
		noClick = (XUiV_Panel)GetChildById("noClick").ViewComponent;
		blockedPlayerList = (XUiV_Grid)GetChildById("blockList").ViewComponent;
		blockedEntries = GetChildrenByType<XUiC_PlayersBlockedListEntry>();
		blockedPager = (XUiC_Paging)GetChildById("blockedPager");
		blockedPager.OnPageChanged += updateBlockedList;
		blockedCounter = (XUiV_Label)GetChildById("blockedCounter").ViewComponent;
		recentPlayerList = (XUiV_Grid)GetChildById("recentList").ViewComponent;
		recentEntries = GetChildrenByType<XUiC_PlayersRecentListEntry>();
		recentPager = (XUiC_Paging)GetChildById("recentPager");
		recentPager.OnPageChanged += updateRecentList;
		recentCounter = (XUiV_Label)GetChildById("recentCounter").ViewComponent;
		for (int i = 0; i < blockedEntries.Length; i++)
		{
			blockedEntries[i].BlockList = this;
			blockedEntries[i].IsAlternating = i % 2 == 0;
		}
		for (int j = 0; j < recentEntries.Length; j++)
		{
			recentEntries[j].BlockList = this;
			recentEntries[j].IsAlternating = j % 2 == 0;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (BlockedPlayerList.Instance != null)
		{
			noClick.Enabled = false;
			blockedPager.Reset();
			recentPager.Reset();
			BlockedPlayerList.Instance.UpdatePlayersSeenInWorld(GameManager.Instance.World);
			ThreadManager.StartCoroutine(BlockedPlayerList.Instance.ResolveUserDetails());
			updateBlockedList();
			updateRecentList();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (BlockedPlayerList.Instance != null && IsDirty)
		{
			IsDirty = false;
			updateBlockedList();
			updateRecentList();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "blockedCount"))
		{
			if (_bindingName == "recentCount")
			{
				if (BlockedPlayerList.Instance != null)
				{
					_value = $"{BlockedPlayerList.Instance.EntryCount(_blocked: false, _resolveRequired: false)}/{100}";
					return true;
				}
				return false;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		if (BlockedPlayerList.Instance != null)
		{
			_value = $"{BlockedPlayerList.Instance.EntryCount(_blocked: true, _resolveRequired: false)}/{500}";
			return true;
		}
		return false;
	}

	public void DisplayMessage(string _header, string _message)
	{
		noClick.Enabled = true;
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, _header, _message, XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, DisableNoClick, DisableNoClick, _openMainMenuOnClose: false, _modal: true, _bCloseAllOpenWindows: false);
		[PublicizedFrom(EAccessModifier.Private)]
		void DisableNoClick()
		{
			noClick.Enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockedList()
	{
		for (int i = 0; i < blockedPlayerList.Rows; i++)
		{
			blockedEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			blockedEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData"));
			IsDirty = true;
			return;
		}
		blockedPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(_blocked: true, _resolveRequired: true), blockedPlayerList.Rows);
		int num = blockedPlayerList.Rows * blockedPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry item in BlockedPlayerList.Instance.GetEntriesOrdered(_blocked: true, _resolveRequired: true))
		{
			if (num2 < num)
			{
				num2++;
				continue;
			}
			int num3 = num2 - num;
			if (num3 >= blockedPlayerList.Rows)
			{
				break;
			}
			blockedEntries[num3].UpdateEntry(item.PlayerData.PrimaryId);
			num2++;
		}
		for (int j = num2 - num; j < blockedPlayerList.Rows; j++)
		{
			blockedEntries[j].Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRecentList()
	{
		for (int i = 0; i < recentPlayerList.Rows; i++)
		{
			recentEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			recentEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData"));
			IsDirty = true;
			return;
		}
		recentPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(_blocked: false, _resolveRequired: true), recentPlayerList.Rows);
		int num = recentPlayerList.Rows * recentPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry item in BlockedPlayerList.Instance.GetEntriesOrdered(_blocked: false, _resolveRequired: true))
		{
			if (num2 < num)
			{
				num2++;
				continue;
			}
			int num3 = num2 - num;
			if (num3 >= recentPlayerList.Rows)
			{
				break;
			}
			recentEntries[num3].UpdateEntry(item.PlayerData.PrimaryId);
			num2++;
		}
		for (int j = num2 - num; j < recentPlayerList.Rows; j++)
		{
			recentEntries[j].Clear();
		}
	}
}
