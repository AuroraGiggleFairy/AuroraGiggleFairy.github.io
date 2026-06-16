using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BlockedPlayersList : XUiController
{
	[XuiBindComponent("blockList", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid blockedPlayerList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PlayersBlockedListEntry[] blockedEntries;

	[XuiBindComponent("blockedPager", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Paging blockedPager;

	[XuiBindComponent("recentList", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid recentPlayerList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PlayersRecentListEntry[] recentEntries;

	[XuiBindComponent("recentPager", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Paging recentPager;

	[XuiXmlBinding("blockedcount")]
	public int BlockedEntriesCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (BlockedPlayerList.Instance != null)
			{
				return BlockedPlayerList.Instance.EntryCount(_blocked: true, _resolveRequired: false);
			}
			return 0;
		}
	}

	[XuiXmlBinding("blockedmax")]
	public int MaxBlockedEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 500;
		}
	}

	[XuiXmlBinding("recentcount")]
	public int RecentEntriesCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (BlockedPlayerList.Instance != null)
			{
				return BlockedPlayerList.Instance.EntryCount(_blocked: false, _resolveRequired: false);
			}
			return 0;
		}
	}

	[XuiXmlBinding("recentmax")]
	public int MaxRecentEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 100;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (BlockedPlayerList.Instance != null)
		{
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

	public void DisplayMessage(string _header, string _message)
	{
		XUiC_MessageBoxWindowGroup.ShowOk(xui, _header, _message, null, _openMainMenuOnClose: false, _modal: false);
	}

	[XuiBindEvent("OnPageChanged", "blockedPager")]
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

	[XuiBindEvent("OnPageChanged", "recentPager")]
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
