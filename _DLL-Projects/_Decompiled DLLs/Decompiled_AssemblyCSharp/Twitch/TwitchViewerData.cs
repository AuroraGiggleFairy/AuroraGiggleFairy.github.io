using System.Collections.Generic;
using System.IO;
using Challenges;
using UnityEngine;

namespace Twitch;

public class TwitchViewerData
{
	public TwitchManager Owner;

	public static float ChattingAddedTimeAmount = 300f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pointRate = 1f;

	public float PointRateSubs = 2f;

	public float NextActionTime;

	public int StartingPoints = 100;

	public float NonSubPointCap = 1000f;

	public float SubPointCap = 2000f;

	public int SubPointAddTier1 = 500;

	public int SubPointAddTier2 = 1000;

	public int SubPointAddTier3 = 2500;

	public int GiftSubPointAddTier1 = 500;

	public int GiftSubPointAddTier2 = 1000;

	public int GiftSubPointAddTier3 = 2500;

	public float ActionSpamDelay = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_GiftedSubs;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_AddPPAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_AddSPAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_ErrorAddingBitCredits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_ErrorAddingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_GiftedSubs;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ViewerEntry> ViewerEntries = new Dictionary<string, ViewerEntry>();

	public Dictionary<int, string> IdToUsername = new Dictionary<int, string>();

	public List<GiftSubEntry> SubEntries = new List<GiftSubEntry>();

	public static char[] UsernameExcludeCharacters = new char[3] { ';', '\\', ':' };

	public float PointRate
	{
		get
		{
			return pointRate;
		}
		set
		{
			pointRate = value;
			PointRateSubs = value * 2f;
		}
	}

	public TwitchViewerData(TwitchManager owner)
	{
		Owner = owner;
	}

	public int GetSubTierPoints(TwitchSubEventEntry.SubTierTypes tier)
	{
		return tier switch
		{
			TwitchSubEventEntry.SubTierTypes.Tier2 => SubPointAddTier2, 
			TwitchSubEventEntry.SubTierTypes.Tier3 => SubPointAddTier3, 
			_ => SubPointAddTier1, 
		};
	}

	public string GetRandomActiveViewer()
	{
		string userName = Owner.Authentication.userName;
		List<string> list = new List<string>();
		foreach (string key in ViewerEntries.Keys)
		{
			if (ViewerEntries[key].IsActive && key != userName)
			{
				list.Add(key);
			}
		}
		if (list.Count > 0)
		{
			return list[GameEventManager.Current.Random.RandomRange(list.Count)];
		}
		return "";
	}

	public int GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes tier)
	{
		return tier switch
		{
			TwitchSubEventEntry.SubTierTypes.Tier2 => GiftSubPointAddTier2, 
			TwitchSubEventEntry.SubTierTypes.Tier3 => GiftSubPointAddTier3, 
			_ => GiftSubPointAddTier1, 
		};
	}

	public void SetupLocalization()
	{
		chatOutput_AddPPAll = Localization.Get("TwitchChat_AddPPAll");
		chatOutput_AddSPAll = Localization.Get("TwitchChat_AddSPAll");
		chatOutput_ErrorAddingBitCredits = Localization.Get("TwitchChat_ErrorAddingBitCredit");
		chatOutput_ErrorAddingPoints = Localization.Get("TwitchChat_ErrorAddingPoints");
		chatOutput_GiftedSubs = Localization.Get("TwitchChat_GiftedSubs");
		ingameOutput_GiftedSubs = Localization.Get("TwitchInGame_GiftedSubs");
	}

	public void Update(float deltaTime)
	{
		NextActionTime -= deltaTime;
		if (NextActionTime <= 0f)
		{
			IncrementViewerEntries();
			NextActionTime = 10f;
		}
		for (int num = SubEntries.Count - 1; num >= 0; num--)
		{
			if (SubEntries[num].Update(deltaTime))
			{
				GiftSubEntry giftSubEntry = SubEntries[num];
				ViewerEntry viewerEntry = GetViewerEntry(giftSubEntry.UserName);
				viewerEntry.UserID = giftSubEntry.UserID;
				int num2 = GetGiftSubTierPoints(giftSubEntry.Tier) * giftSubEntry.SubCount * Owner.GiftSubPointModifier;
				if (num2 > 0)
				{
					viewerEntry.SpecialPoints += num2;
					Owner.ircClient.SendChannelMessage(string.Format(chatOutput_GiftedSubs, giftSubEntry.UserName, viewerEntry.CombinedPoints, giftSubEntry.SubCount, Owner.GetTierName(giftSubEntry.Tier), num2), useQueue: true);
					SubEntries.RemoveAt(num);
					string message = string.Format(ingameOutput_GiftedSubs, giftSubEntry.UserName, giftSubEntry.SubCount, Owner.GetTierName(giftSubEntry.Tier), num2);
					XUiC_ChatOutput.AddMessage(Owner.LocalPlayerXUi, EnumGameMessages.PlainTextLocal, message, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
				}
				Owner.HandleGiftSubEvent(giftSubEntry.UserName, giftSubEntry.SubCount, giftSubEntry.Tier);
			}
		}
	}

	public void AddGiftSubEntry(string userName, int userID, TwitchSubEventEntry.SubTierTypes tier, int total)
	{
		for (int i = 0; i < SubEntries.Count; i++)
		{
			if (SubEntries[i].UserName == userName)
			{
				SubEntries[i].AddSub();
				return;
			}
		}
		SubEntries.Add(new GiftSubEntry(userName, userID, tier, total));
	}

	public ViewerEntry AddCredit(string name, int credit, bool displayNewTotal)
	{
		ViewerEntry viewerEntry = AddToViewerEntry(name, credit, TwitchAction.PointTypes.Bits);
		if (viewerEntry == null)
		{
			Owner.ircClient.SendChannelMessage(string.Format(chatOutput_ErrorAddingBitCredits, name), useQueue: true);
		}
		else if (displayNewTotal)
		{
			Owner.SendChannelCreditOutputMessage(name, viewerEntry);
		}
		return viewerEntry;
	}

	public void AddPoints(string name, int points, bool isSpecial, bool displayNewTotal)
	{
		if (name == "")
		{
			foreach (string key in ViewerEntries.Keys)
			{
				if (!ViewerEntries[key].IsActive)
				{
					continue;
				}
				if (isSpecial)
				{
					ViewerEntries[key].SpecialPoints += points;
					if (ViewerEntries[key].SpecialPoints < 0f)
					{
						ViewerEntries[key].SpecialPoints = 0f;
					}
				}
				else
				{
					ViewerEntries[key].StandardPoints += points;
					if (ViewerEntries[key].StandardPoints < 0f)
					{
						ViewerEntries[key].StandardPoints = 0f;
					}
				}
			}
			if (isSpecial)
			{
				Owner.ircClient.SendChannelMessage(string.Format(chatOutput_AddSPAll, points), useQueue: true);
			}
			else
			{
				Owner.ircClient.SendChannelMessage(string.Format(chatOutput_AddPPAll, points), useQueue: true);
			}
		}
		else
		{
			ViewerEntry viewerEntry = AddToViewerEntry(name, points, isSpecial ? TwitchAction.PointTypes.SP : TwitchAction.PointTypes.PP);
			if (viewerEntry == null)
			{
				Owner.ircClient.SendChannelMessage(string.Format(chatOutput_ErrorAddingPoints, name), useQueue: true);
			}
			else if (displayNewTotal)
			{
				Owner.SendChannelPointOutputMessage(name, viewerEntry);
			}
		}
	}

	public void AddPointsAll(int standardPoints, int specialPoints, bool announceToChat = true)
	{
		foreach (string key in ViewerEntries.Keys)
		{
			if (!ViewerEntries[key].IsActive)
			{
				continue;
			}
			if (standardPoints != 0)
			{
				ViewerEntries[key].StandardPoints += standardPoints;
				if (ViewerEntries[key].StandardPoints < 0f)
				{
					ViewerEntries[key].StandardPoints = 0f;
				}
			}
			if (specialPoints != 0)
			{
				ViewerEntries[key].SpecialPoints += specialPoints;
				if (ViewerEntries[key].SpecialPoints < 0f)
				{
					ViewerEntries[key].SpecialPoints = 0f;
				}
			}
		}
		if (announceToChat)
		{
			if (standardPoints != 0)
			{
				Owner.ircClient.SendChannelMessage(string.Format(chatOutput_AddPPAll, standardPoints), useQueue: true);
			}
			if (specialPoints != 0)
			{
				Owner.ircClient.SendChannelMessage(string.Format(chatOutput_AddSPAll, specialPoints), useQueue: true);
			}
		}
	}

	public void Write(BinaryWriter bw)
	{
		int num = 0;
		foreach (string key in ViewerEntries.Keys)
		{
			if (key.IndexOfAny(UsernameExcludeCharacters) == -1 && (ViewerEntries[key].StandardPoints > 0f || ViewerEntries[key].SpecialPoints > 0f))
			{
				num++;
			}
		}
		bw.Write(num);
		foreach (string key2 in ViewerEntries.Keys)
		{
			if (key2.IndexOfAny(UsernameExcludeCharacters) == -1)
			{
				ViewerEntry viewerEntry = ViewerEntries[key2];
				if (viewerEntry.StandardPoints > 0f || viewerEntry.SpecialPoints > 0f)
				{
					bw.Write(key2);
					bw.Write(viewerEntry.UserID);
					bw.Write(viewerEntry.StandardPoints);
				}
			}
		}
	}

	public void WriteSpecial(BinaryWriter bw)
	{
		int num = 0;
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntry viewerEntry = ViewerEntries[key];
			if (key.IndexOfAny(UsernameExcludeCharacters) == -1 && (viewerEntry.SpecialPoints > 0f || viewerEntry.BitCredits > 0))
			{
				num++;
			}
		}
		bw.Write(num);
		foreach (string key2 in ViewerEntries.Keys)
		{
			if (key2.IndexOfAny(UsernameExcludeCharacters) == -1)
			{
				ViewerEntry viewerEntry2 = ViewerEntries[key2];
				if (viewerEntry2.SpecialPoints > 0f || viewerEntry2.BitCredits > 0)
				{
					bw.Write(key2);
					bw.Write(viewerEntry2.UserID);
					bw.Write(viewerEntry2.SpecialPoints);
					bw.Write(viewerEntry2.BitCredits);
				}
			}
		}
	}

	public void WriteExport(string savePath)
	{
		using StreamWriter tw = SdFile.CreateText(savePath);
		WriteExport(tw);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteExport(TextWriter tw)
	{
		tw.WriteLine("Name|UserID|PP|SP|Bit Credit");
		foreach (string key in ViewerEntries.Keys)
		{
			if (key.IndexOfAny(UsernameExcludeCharacters) == -1)
			{
				ViewerEntry viewerEntry = ViewerEntries[key];
				tw.WriteLine($"{key}|{viewerEntry.UserID}|{viewerEntry.StandardPoints}|{viewerEntry.SpecialPoints}|{viewerEntry.BitCredits}");
			}
		}
	}

	public void LoadExport(TextReader tr)
	{
		tr.ReadLine();
		Dictionary<string, ViewerEntry> dictionary = new Dictionary<string, ViewerEntry>();
		while (tr.Peek() >= 0)
		{
			string[] array = tr.ReadLine().Split('|');
			if (array.Length == 5)
			{
				ViewerEntry viewerEntry = null;
				viewerEntry = ((!ViewerEntries.ContainsKey(array[0])) ? new ViewerEntry() : ViewerEntries[array[0]]);
				viewerEntry.StandardPoints = StringParsers.ParseSInt32(array[2]);
				viewerEntry.SpecialPoints = StringParsers.ParseSInt32(array[3]);
				viewerEntry.BitCredits = StringParsers.ParseSInt32(array[4]);
				dictionary.Add(array[0], viewerEntry);
			}
		}
		ViewerEntries.Clear();
		dictionary.CopyTo(ViewerEntries, _overwriteExisting: true);
	}

	public void Read(BinaryReader br, byte currentVersion)
	{
		ViewerEntries.Clear();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = br.ReadString();
			int num2 = -1;
			if (currentVersion > 14)
			{
				num2 = br.ReadInt32();
			}
			float standardPoints = br.ReadSingle();
			if (text.IndexOfAny(UsernameExcludeCharacters) == -1)
			{
				if (num2 != -1)
				{
					AddToIDLookup(num2, text);
				}
				ViewerEntries.Add(text, new ViewerEntry
				{
					UserID = num2,
					StandardPoints = standardPoints
				});
			}
		}
	}

	public void ReadSpecial(BinaryReader br, byte currentVersion)
	{
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = br.ReadString();
			int num2 = -1;
			if (currentVersion > 1)
			{
				num2 = br.ReadInt32();
			}
			float specialPoints = br.ReadSingle();
			int bitCredits = 0;
			if (currentVersion > 2)
			{
				bitCredits = br.ReadInt32();
			}
			if (text.IndexOfAny(UsernameExcludeCharacters) == -1)
			{
				if (num2 != -1)
				{
					AddToIDLookup(num2, text);
				}
				ViewerEntry viewerEntry = GetViewerEntry(text);
				viewerEntry.UserID = num2;
				viewerEntry.SpecialPoints = specialPoints;
				if (currentVersion > 2)
				{
					viewerEntry.BitCredits = bitCredits;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveStandardToSpecialPoints()
	{
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntries[key].SpecialPoints += ViewerEntries[key].StandardPoints;
			ViewerEntries[key].StandardPoints = 0f;
		}
	}

	public void ResetAllStandardPoints()
	{
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntries[key].StandardPoints = 0f;
		}
	}

	public void ResetAllSpecialPoints()
	{
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntries[key].StandardPoints = 0f;
		}
	}

	public void Cleanup()
	{
		List<string> list = new List<string>();
		foreach (string key in ViewerEntries.Keys)
		{
			string text = key.ToLower();
			if (text != key)
			{
				ViewerEntry viewerEntry = ViewerEntries[key];
				if (ViewerEntries.ContainsKey(text))
				{
					ViewerEntry viewerEntry2 = ViewerEntries[text];
					viewerEntry2.StandardPoints += viewerEntry.StandardPoints;
					viewerEntry2.SpecialPoints += viewerEntry.SpecialPoints;
					viewerEntry2.BitCredits += viewerEntry.BitCredits;
				}
				else
				{
					ViewerEntries.Add(text, viewerEntry);
				}
				list.Add(key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			ViewerEntries.Remove(list[i]);
		}
	}

	public void ResetAllPoints()
	{
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntries[key].SpecialPoints = 0f;
			ViewerEntries[key].StandardPoints = 0f;
		}
	}

	public string GetPointTotals()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntry viewerEntry = ViewerEntries[key];
			num2 += (int)viewerEntry.SpecialPoints;
			num += (int)viewerEntry.StandardPoints;
			num3 += viewerEntry.BitCredits;
		}
		return $"PP: {num} SP: {num2} BC: {num3}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearDisplayViewers()
	{
		ViewerEntries.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncrementViewerEntries()
	{
		float value = EffectManager.GetValue(PassiveEffects.TwitchViewerPointRate, null, PointRate, TwitchManager.Current.LocalPlayer);
		float num = value * 2f;
		float value2 = EffectManager.GetValue(PassiveEffects.TwitchViewerPointRate, null, NonSubPointCap, TwitchManager.Current.LocalPlayer);
		float value3 = EffectManager.GetValue(PassiveEffects.TwitchViewerPointRate, null, SubPointCap, TwitchManager.Current.LocalPlayer);
		bool allowPointGeneration = Owner.CurrentActionPreset.AllowPointGeneration;
		foreach (string key in ViewerEntries.Keys)
		{
			ViewerEntry viewerEntry = ViewerEntries[key];
			if (!viewerEntry.IsActive)
			{
				continue;
			}
			Owner.HasDataChanges = true;
			if (allowPointGeneration)
			{
				if (viewerEntry.IsSub)
				{
					if (viewerEntry.StandardPoints < value3)
					{
						viewerEntry.StandardPoints += num;
						if (viewerEntry.StandardPoints > value3)
						{
							viewerEntry.StandardPoints = value3;
						}
					}
				}
				else if (viewerEntry.StandardPoints < value2)
				{
					viewerEntry.StandardPoints += value;
					if (viewerEntry.StandardPoints > value2)
					{
						viewerEntry.StandardPoints = value2;
					}
				}
			}
			if (viewerEntry.addPointsUntil < Time.time)
			{
				viewerEntry.IsActive = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddToIDLookup(int viewerID, string viewerName, bool sendNewInChat = false)
	{
		if (IdToUsername.ContainsKey(viewerID))
		{
			IdToUsername[viewerID] = viewerName;
			return;
		}
		IdToUsername.Add(viewerID, viewerName);
		if (sendNewInChat && TwitchManager.Current.extensionManager != null)
		{
			TwitchManager.Current.extensionManager.PushViewerChatState(viewerID.ToString(), hasChatted: true);
		}
	}

	public ViewerEntry UpdateViewerEntry(int viewerID, string name, string color, bool isSub)
	{
		AddToIDLookup(viewerID, name, sendNewInChat: true);
		if (ViewerEntries.ContainsKey(name))
		{
			ViewerEntry viewerEntry = ViewerEntries[name];
			viewerEntry.UserColor = color;
			viewerEntry.UserID = viewerID;
			viewerEntry.addPointsUntil = Time.time + ChattingAddedTimeAmount;
			if (!viewerEntry.IsActive)
			{
				Owner.PushBalanceToExtensionQueue(viewerID.ToString(), viewerEntry.BitCredits);
			}
			viewerEntry.IsActive = true;
			viewerEntry.IsSub = isSub;
			return viewerEntry;
		}
		ViewerEntry viewerEntry2 = new ViewerEntry
		{
			UserColor = color,
			UserID = viewerID,
			StandardPoints = StartingPoints,
			addPointsUntil = Time.time + ChattingAddedTimeAmount,
			IsActive = true,
			IsSub = isSub
		};
		ViewerEntries.Add(name, viewerEntry2);
		return viewerEntry2;
	}

	public bool HasViewerEntry(string name)
	{
		return ViewerEntries.ContainsKey(name);
	}

	public ViewerEntry GetViewerEntry(string name)
	{
		if (ViewerEntries.ContainsKey(name))
		{
			return ViewerEntries[name];
		}
		ViewerEntry viewerEntry = new ViewerEntry
		{
			StandardPoints = 0f,
			addPointsUntil = 0f,
			IsActive = false,
			IsSub = false
		};
		ViewerEntries.Add(name, viewerEntry);
		return viewerEntry;
	}

	public bool RemoveViewerEntry(string name)
	{
		if (ViewerEntries.ContainsKey(name))
		{
			ViewerEntries.Remove(name);
			return true;
		}
		return false;
	}

	public ViewerEntry GetViewerEntry(string name, bool isSub)
	{
		if (ViewerEntries.ContainsKey(name))
		{
			return ViewerEntries[name];
		}
		ViewerEntry viewerEntry = new ViewerEntry
		{
			StandardPoints = StartingPoints,
			addPointsUntil = 0f,
			IsActive = true,
			IsSub = isSub
		};
		ViewerEntries.Add(name, viewerEntry);
		return viewerEntry;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ViewerEntry AddToViewerEntry(string name, int points, TwitchAction.PointTypes pointType)
	{
		name = ((!name.StartsWith("@")) ? name.ToLower() : name.Substring(1).ToLower());
		if (ViewerEntries.ContainsKey(name))
		{
			ViewerEntry viewerEntry = ViewerEntries[name];
			switch (pointType)
			{
			case TwitchAction.PointTypes.PP:
				viewerEntry.StandardPoints += points;
				if (viewerEntry.StandardPoints < 0f)
				{
					viewerEntry.StandardPoints = 0f;
				}
				break;
			case TwitchAction.PointTypes.SP:
				viewerEntry.SpecialPoints += points;
				if (viewerEntry.SpecialPoints < 0f)
				{
					viewerEntry.SpecialPoints = 0f;
				}
				break;
			case TwitchAction.PointTypes.Bits:
				viewerEntry.BitCredits += points;
				if (viewerEntry.BitCredits < 0)
				{
					viewerEntry.BitCredits = 0;
				}
				Owner.PushBalanceToExtensionQueue(viewerEntry.UserID.ToString(), viewerEntry.BitCredits);
				break;
			}
			return ViewerEntries[name];
		}
		return null;
	}

	public bool HasPointsForAction(string username, TwitchAction action)
	{
		ViewerEntry viewerEntry = ViewerEntries[username];
		if ((action.SpecialOnly && viewerEntry.SpecialPoints >= (float)action.CurrentCost) || (!action.SpecialOnly && viewerEntry.CombinedPoints >= (float)action.CurrentCost))
		{
			return true;
		}
		return false;
	}

	public bool HandleInitialActionEntrySetup(string username, TwitchAction action, bool isRerun, bool isBitAction, out TwitchActionEntry actionEntry)
	{
		ViewerEntry viewerEntry = ViewerEntries[username];
		bool flag = isRerun || isBitAction;
		if ((flag || viewerEntry.LastAction == -1f || ActionSpamDelay == 0f || Time.time - viewerEntry.LastAction > ActionSpamDelay) && (flag || (action.SpecialOnly && viewerEntry.SpecialPoints >= (float)action.CurrentCost) || (!action.SpecialOnly && viewerEntry.CombinedPoints >= (float)action.CurrentCost)))
		{
			actionEntry = action.SetupActionEntry();
			actionEntry.UserName = username;
			if (!isRerun)
			{
				viewerEntry.RemovePoints(action.CurrentCost, action.PointType, actionEntry);
				if (username != Owner.Authentication.userName)
				{
					TwitchLeaderboardStats leaderboardStats = TwitchManager.LeaderboardStats;
					int num = ((action.PointType != TwitchAction.PointTypes.Bits) ? 1 : 2);
					if (action.IsPositive)
					{
						leaderboardStats.TotalGood += num;
						leaderboardStats.CheckTopGood(leaderboardStats.AddGoodActionUsed(username, viewerEntry.UserColor, action.PointType == TwitchAction.PointTypes.Bits));
						QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.GoodAction, action.DisplayCategory.Name);
					}
					else
					{
						leaderboardStats.TotalBad += num;
						leaderboardStats.CheckTopBad(leaderboardStats.AddBadActionUsed(username, viewerEntry.UserColor, action.PointType == TwitchAction.PointTypes.Bits));
						QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.BadAction, action.DisplayCategory.Name);
					}
					leaderboardStats.TotalActions += num;
				}
			}
			viewerEntry.LastAction = Time.time;
			return true;
		}
		actionEntry = null;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ReimburseAction(TwitchActionEntry twitchActionEntry)
	{
		ViewerEntry viewerEntry = ViewerEntries[twitchActionEntry.UserName];
		viewerEntry.StandardPoints += twitchActionEntry.StandardPointsUsed;
		viewerEntry.SpecialPoints += twitchActionEntry.SpecialPointsUsed;
		viewerEntry.BitCredits += twitchActionEntry.BitsUsed;
		Owner.PushBalanceToExtensionQueue(viewerEntry.UserID.ToString(), viewerEntry.BitCredits);
	}

	public void ReimburseAction(string userName, int pointsSpent, TwitchAction action)
	{
		ViewerEntry viewerEntry = ViewerEntries[userName];
		switch (action.PointType)
		{
		case TwitchAction.PointTypes.PP:
		case TwitchAction.PointTypes.SP:
			viewerEntry.SpecialPoints += pointsSpent;
			break;
		case TwitchAction.PointTypes.Bits:
			viewerEntry.BitCredits += pointsSpent;
			break;
		}
	}
}
