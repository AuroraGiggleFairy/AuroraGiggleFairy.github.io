using System;
using System.Collections.Generic;

public static class GameSparksCollector
{
	public enum GSDataKey
	{
		HoursPlayedAtLevel15,
		HoursPlayedAtLevel30,
		HoursPlayedAtLevel50,
		SkillsPurchasedAtLevel15,
		SkillsPurchasedAtLevel30,
		SkillsPurchasedAtLevel50,
		PlayerLevelAtHour,
		XpEarnedBy,
		PlayerDeathCauses,
		ZombiesKilledBy,
		CraftedItems,
		TraderItemsBought,
		VendingItemsBought,
		TraderMoneySpentOn,
		VendingMoneySpentOn,
		TotalMoneySpentOn,
		PeakConcurrentClients,
		PeakConcurrentPlayers,
		QuestTraderToTraderDistance,
		QuestAcceptedDistance,
		QuestOfferedDistance,
		QuestStarterTraderDistance,
		PlayerProfileIsCustom,
		PlayerArchetypeName,
		UsedTwitchIntegration
	}

	public enum GSDataCollection
	{
		SessionTotal,
		SessionUpdates
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData dataUpdates = new GSRequestData();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData dataSessionTotal = new GSRequestData();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool CollectGamePlayData { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData GetObject(string _keyString, GSRequestData _collection)
	{
		lock (lockObject)
		{
			GSRequestData gSData = _collection.GetGSData(_keyString);
			if (gSData != null)
			{
				return gSData;
			}
			gSData = new GSRequestData();
			_collection.Add(_keyString, gSData);
			return gSData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static (GSRequestData, string) GetRequestDataAndKey(GSDataCollection _collectionType, GSDataKey _key, string _subKey = null)
	{
		string text = _key.ToStringCached();
		GSRequestData gSRequestData = ((_collectionType == GSDataCollection.SessionUpdates) ? dataUpdates : dataSessionTotal);
		if (_subKey == null)
		{
			return (gSRequestData, text);
		}
		return (GetObject(text, gSRequestData), _subKey);
	}

	public static void SetValue(GSDataKey _key, string _subKey, int _value, bool _isGamePlay = true, GSDataCollection _collectionType = GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || CollectGamePlayData)
		{
			lock (lockObject)
			{
				var (gSRequestData, key) = GetRequestDataAndKey(_collectionType, _key, _subKey);
				gSRequestData.AddNumber(key, _value);
			}
		}
	}

	public static void SetValue(GSDataKey _key, string _subKey, string _value, bool _isGamePlay = true, GSDataCollection _collectionType = GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || CollectGamePlayData)
		{
			lock (lockObject)
			{
				var (gSRequestData, key) = GetRequestDataAndKey(_collectionType, _key, _subKey);
				gSRequestData.AddString(key, _value);
			}
		}
	}

	public static void IncrementCounter(GSDataKey _key, string _subKey, int _increment, bool _isGamePlay = true, GSDataCollection _collectionType = GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || CollectGamePlayData)
		{
			lock (lockObject)
			{
				(GSRequestData, string) requestDataAndKey = GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				int valueOrDefault = item.GetInt(item2).GetValueOrDefault();
				valueOrDefault += _increment;
				item.AddNumber(item2, valueOrDefault);
			}
		}
	}

	public static void IncrementCounter(GSDataKey _key, string _subKey, float _increment, bool _isGamePlay = true, GSDataCollection _collectionType = GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || CollectGamePlayData)
		{
			lock (lockObject)
			{
				(GSRequestData, string) requestDataAndKey = GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				float valueOrDefault = item.GetFloat(item2).GetValueOrDefault();
				valueOrDefault += _increment;
				item.AddNumber(item2, valueOrDefault);
			}
		}
	}

	public static void SetMax(GSDataKey _key, string _subKey, int _currentValue, bool _isGamePlay = true, GSDataCollection _collectionType = GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || CollectGamePlayData)
		{
			lock (lockObject)
			{
				(GSRequestData, string) requestDataAndKey = GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				int val = item.GetInt(item2) ?? int.MinValue;
				val = Math.Max(val, _currentValue);
				item.AddNumber(item2, val);
			}
		}
	}

	public static GSRequestData GetSessionUpdateDataAndReset()
	{
		lock (lockObject)
		{
			GSRequestData result = dataUpdates;
			dataUpdates = new GSRequestData();
			return result;
		}
	}

	public static GSRequestData GetSessionTotalData(bool _reset)
	{
		if (!_reset)
		{
			return dataSessionTotal;
		}
		lock (lockObject)
		{
			GSRequestData result = dataSessionTotal;
			dataSessionTotal = new GSRequestData();
			return result;
		}
	}

	public static void PlayerLevelUp(EntityPlayerLocal _localPlayer, int _level)
	{
		switch (_level)
		{
		case 15:
			SendSaveTimePlayed(GSDataKey.HoursPlayedAtLevel15, _localPlayer);
			SendSkillStats(GSDataKey.SkillsPurchasedAtLevel15, _localPlayer);
			break;
		case 30:
			SendSaveTimePlayed(GSDataKey.HoursPlayedAtLevel30, _localPlayer);
			SendSkillStats(GSDataKey.SkillsPurchasedAtLevel30, _localPlayer);
			break;
		case 50:
			SendSaveTimePlayed(GSDataKey.HoursPlayedAtLevel50, _localPlayer);
			SendSkillStats(GSDataKey.SkillsPurchasedAtLevel50, _localPlayer);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SendSkillStats(GSDataKey _key, EntityPlayerLocal _localPlayer)
	{
		foreach (KeyValuePair<int, ProgressionValue> item in _localPlayer.Progression.GetDict())
		{
			ProgressionClass progressionClass = item.Value.ProgressionClass;
			for (int i = progressionClass.MinLevel + 1; i <= item.Value.Level; i++)
			{
				string subKey;
				if (progressionClass.Parent == null || progressionClass.Parent == progressionClass)
				{
					subKey = $"{progressionClass.Name}_{i}";
				}
				else
				{
					ProgressionClass parent = progressionClass.Parent;
					while (parent.Parent != null && parent.Parent != parent)
					{
						parent = parent.Parent;
					}
					subKey = $"{parent.Name}_{progressionClass.Name}_{i}";
				}
				SetValue(_key, subKey, 1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SendSaveTimePlayed(GSDataKey _key, EntityPlayerLocal _localPlayer)
	{
		int num = (int)(_localPlayer.totalTimePlayed / 60f);
		if (num > 0)
		{
			SetValue(_key, null, num);
		}
	}
}
