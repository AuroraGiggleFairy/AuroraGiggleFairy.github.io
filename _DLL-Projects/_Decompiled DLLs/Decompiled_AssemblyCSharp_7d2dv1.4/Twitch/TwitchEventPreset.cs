using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Twitch;

public class TwitchEventPreset
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchEventEntry> BitEvents = new List<TwitchEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchSubEventEntry> SubEvents = new List<TwitchSubEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchSubEventEntry> GiftSubEvents = new List<TwitchSubEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchEventEntry> RaidEvents = new List<TwitchEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchEventEntry> CharityEvents = new List<TwitchEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchChannelPointEventEntry> ChannelPointEvents = new List<TwitchChannelPointEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchHypeTrainEventEntry> HypeTrainEvents = new List<TwitchHypeTrainEventEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchCreatorGoalEventEntry> CreatorGoalEvents = new List<TwitchCreatorGoalEventEntry>();

	public string Name;

	public bool IsDefault;

	public bool IsEmpty;

	public bool ChannelPointsSetup;

	public string Title;

	public string Description;

	public bool HasCustomEvents
	{
		get
		{
			if (BitEvents.Count <= 0 && SubEvents.Count <= 0 && GiftSubEvents.Count <= 0 && RaidEvents.Count <= 0 && CharityEvents.Count <= 0 && ChannelPointEvents.Count <= 0 && HypeTrainEvents.Count <= 0)
			{
				return CreatorGoalEvents.Count > 0;
			}
			return true;
		}
	}

	public bool HasBitEvents => BitEvents.Count > 0;

	public bool HasSubEvents => SubEvents.Count > 0;

	public bool HasGiftSubEvents => GiftSubEvents.Count > 0;

	public bool HasRaidEvents => RaidEvents.Count > 0;

	public bool HasCharityEvents => CharityEvents.Count > 0;

	public bool HasChannelPointEvents => ChannelPointEvents.Count > 0;

	public bool HasHypeTrainEvents => HypeTrainEvents.Count > 0;

	public bool HasCreatorGoalEvents => CreatorGoalEvents.Count > 0;

	public void AddBitEvent(TwitchEventEntry entry)
	{
		BitEvents.Add(entry);
	}

	public void AddSubEvent(TwitchSubEventEntry entry)
	{
		SubEvents.Add(entry);
	}

	public void AddGiftSubEvent(TwitchSubEventEntry entry)
	{
		GiftSubEvents.Add(entry);
	}

	public void AddRaidEvent(TwitchEventEntry entry)
	{
		RaidEvents.Add(entry);
	}

	public void AddCharityEvent(TwitchEventEntry entry)
	{
		CharityEvents.Add(entry);
	}

	public void AddChannelPointEvent(TwitchChannelPointEventEntry entry)
	{
		ChannelPointEvents.Add(entry);
	}

	public void AddHypeTrainEvent(TwitchHypeTrainEventEntry entry)
	{
		HypeTrainEvents.Add(entry);
	}

	public void AddCreatorGoalEvent(TwitchCreatorGoalEventEntry entry)
	{
		CreatorGoalEvents.Add(entry);
	}

	public TwitchSubEventEntry HandleSubEvent(int months, TwitchSubEventEntry.SubTierTypes tier)
	{
		for (int i = 0; i < SubEvents.Count; i++)
		{
			if (SubEvents[i].IsValid(months, "", tier))
			{
				return SubEvents[i];
			}
		}
		return null;
	}

	public TwitchSubEventEntry HandleGiftSubEvent(int giftCounts, TwitchSubEventEntry.SubTierTypes tier)
	{
		for (int i = 0; i < GiftSubEvents.Count; i++)
		{
			if (GiftSubEvents[i].IsValid(giftCounts, "", tier))
			{
				return GiftSubEvents[i];
			}
		}
		return null;
	}

	public TwitchEventEntry HandleBitRedeem(int bitAmount)
	{
		for (int i = 0; i < BitEvents.Count; i++)
		{
			if (BitEvents[i].IsValid(bitAmount))
			{
				return BitEvents[i];
			}
		}
		return null;
	}

	public TwitchChannelPointEventEntry HandleChannelPointsRedeem(string title)
	{
		for (int i = 0; i < ChannelPointEvents.Count; i++)
		{
			if (ChannelPointEvents[i].ChannelPointTitle == title)
			{
				return ChannelPointEvents[i];
			}
		}
		return null;
	}

	public TwitchEventEntry HandleRaid(int viewerAmount)
	{
		for (int i = 0; i < RaidEvents.Count; i++)
		{
			if (RaidEvents[i].IsValid(viewerAmount))
			{
				return RaidEvents[i];
			}
		}
		return null;
	}

	public TwitchEventEntry HandleCharityRedeem(int charityAmount)
	{
		for (int i = 0; i < CharityEvents.Count; i++)
		{
			if (CharityEvents[i].IsValid(charityAmount))
			{
				return CharityEvents[i];
			}
		}
		return null;
	}

	public TwitchEventEntry HandleHypeTrainRedeem(int hypeTrainLevel)
	{
		for (int i = 0; i < HypeTrainEvents.Count; i++)
		{
			if (HypeTrainEvents[i].IsValid(hypeTrainLevel))
			{
				return HypeTrainEvents[i];
			}
		}
		return null;
	}

	public TwitchCreatorGoalEventEntry HandleCreatorGoalEvent(string goalType)
	{
		for (int i = 0; i < HypeTrainEvents.Count; i++)
		{
			if (CreatorGoalEvents[i].IsValid(-1, goalType))
			{
				return CreatorGoalEvents[i];
			}
		}
		return null;
	}

	public void AddChannelPointRedemptions()
	{
		if (TwitchManager.Current.Authentication == null)
		{
			return;
		}
		string userID = TwitchManager.Current.Authentication.userID;
		for (int i = 0; i < ChannelPointEvents.Count; i++)
		{
			if (!(ChannelPointEvents[i].ChannelPointID == "") || !ChannelPointEvents[i].AutoCreate)
			{
				continue;
			}
			GameManager.Instance.StartCoroutine(TwitchChannelPointEventEntry.CreateCustomRewardPost(ChannelPointEvents[i].SetupRewardEntry(userID), [PublicizedFrom(EAccessModifier.Private)] (string res) =>
			{
				TwitchChannelPointEventEntry.CreateCustomRewardResponses createCustomRewardResponses = JsonConvert.DeserializeObject<TwitchChannelPointEventEntry.CreateCustomRewardResponses>(res);
				for (int j = 0; j < ChannelPointEvents.Count; j++)
				{
					if (ChannelPointEvents[j].ChannelPointTitle == createCustomRewardResponses.data[0].title)
					{
						ChannelPointEvents[j].ChannelPointID = createCustomRewardResponses.data[0].id;
						break;
					}
				}
			}, [PublicizedFrom(EAccessModifier.Internal)] (string err) =>
			{
				Log.Out(err);
			}));
		}
		ChannelPointsSetup = true;
	}

	public void RemoveChannelPointRedemptions(TwitchEventPreset newPreset = null)
	{
		for (int i = 0; i < ChannelPointEvents.Count; i++)
		{
			TwitchChannelPointEventEntry twitchChannelPointEventEntry = ChannelPointEvents[i];
			if (!(twitchChannelPointEventEntry.ChannelPointID == "") && ChannelPointEvents[i].AutoCreate && (newPreset == null || !newPreset.ChannelPointEvents.Contains(twitchChannelPointEventEntry)))
			{
				GameManager.Instance.StartCoroutine(TwitchChannelPointEventEntry.DeleteCustomRewardsDelete(ChannelPointEvents[i].ChannelPointID, [PublicizedFrom(EAccessModifier.Internal)] (string res) =>
				{
				}, [PublicizedFrom(EAccessModifier.Internal)] (string err) =>
				{
					Debug.LogWarning("Remove Channel Point Redeem Failed: " + err);
				}));
				ChannelPointEvents[i].ChannelPointID = "";
			}
		}
		ChannelPointsSetup = false;
	}
}
