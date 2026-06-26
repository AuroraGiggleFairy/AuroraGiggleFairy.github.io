using System;
using System.Collections.Generic;
using System.IO;

namespace Platform;

public class AchievementData : Serializable
{
	public enum EnumUpdateType
	{
		Sum,
		Replace,
		Max
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct AchievementStatDecl(EnumAchievementDataStat _name, EnumStatType _type, EnumUpdateType _updateType, List<AchievementInfo> _achievementPairs)
	{
		public readonly EnumAchievementDataStat name = _name;

		public readonly EnumStatType type = _type;

		public readonly EnumUpdateType updateType = _updateType;

		public readonly List<AchievementInfo> achievementInfos = _achievementPairs;
	}

	public readonly struct AchievementInfo(object _triggerPoint, EnumAchievementManagerAchievement _achievement, float _progressContribution)
	{
		public readonly object triggerPoint = _triggerPoint;

		public readonly EnumAchievementManagerAchievement achievement = _achievement;

		public readonly float progressContribution = _progressContribution;
	}

	public const string cDataName = "achievements.bin";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly AchievementStatDecl[] propertyList = new AchievementStatDecl[19]
	{
		new AchievementStatDecl(EnumAchievementDataStat.StoneAxeCrafted, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.StoneAxe, 1.5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.BedrollPlaced, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.Bedroll, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.BleedOutStopped, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.BleedOut, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.WoodFrameCrafted, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.WoodFrame, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.LandClaimPlaced, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.LandClaim, 1.5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.ItemsCrafted, EnumStatType.Int, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(50, EnumAchievementManagerAchievement.Items50, 2f),
			new AchievementInfo(500, EnumAchievementManagerAchievement.Items500, 2f),
			new AchievementInfo(1500, EnumAchievementManagerAchievement.Items1500, 2f),
			new AchievementInfo(5000, EnumAchievementManagerAchievement.Items5000, 5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.ZombiesKilled, EnumStatType.Int, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(10, EnumAchievementManagerAchievement.Zombies10, 2f),
			new AchievementInfo(100, EnumAchievementManagerAchievement.Zombies100, 2f),
			new AchievementInfo(500, EnumAchievementManagerAchievement.Zombies500, 2f),
			new AchievementInfo(2500, EnumAchievementManagerAchievement.Zombies2500, 5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.PlayersKilled, EnumStatType.Int, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.Players1, 1f),
			new AchievementInfo(5, EnumAchievementManagerAchievement.Players5, 2f),
			new AchievementInfo(10, EnumAchievementManagerAchievement.Players10, 2f),
			new AchievementInfo(25, EnumAchievementManagerAchievement.Players25, 5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.KMTravelled, EnumStatType.Float, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(10, EnumAchievementManagerAchievement.Travel10, 0.5f),
			new AchievementInfo(50, EnumAchievementManagerAchievement.Travel50, 1f),
			new AchievementInfo(250, EnumAchievementManagerAchievement.Travel250, 2f),
			new AchievementInfo(1000, EnumAchievementManagerAchievement.Travel1000, 5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.LongestLifeLived, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(60, EnumAchievementManagerAchievement.Life60Minute, 1f),
			new AchievementInfo(180, EnumAchievementManagerAchievement.Life180Minute, 2f),
			new AchievementInfo(600, EnumAchievementManagerAchievement.Life600Minute, 2.5f),
			new AchievementInfo(1680, EnumAchievementManagerAchievement.Life1680Minute, 7.5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.Deaths, EnumStatType.Int, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.Die1, 1f),
			new AchievementInfo(7, EnumAchievementManagerAchievement.Die7, 1.5f),
			new AchievementInfo(14, EnumAchievementManagerAchievement.Die14, 2f),
			new AchievementInfo(28, EnumAchievementManagerAchievement.Die28, 2.5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.HeightAchieved, EnumStatType.Int, EnumUpdateType.Replace, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.Height255, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.DepthAchieved, EnumStatType.Int, EnumUpdateType.Replace, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.Height0, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.SubZeroNakedSwim, EnumStatType.Int, EnumUpdateType.Replace, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.SubZeroNaked, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.KilledWith44Magnum, EnumStatType.Int, EnumUpdateType.Sum, new List<AchievementInfo>
		{
			new AchievementInfo(44, EnumAchievementManagerAchievement.Kills44Mag, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.LegBroken, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(1, EnumAchievementManagerAchievement.LegBreak, 1f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.HighestFortitude, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(4, EnumAchievementManagerAchievement.Fortitude4, 1f),
			new AchievementInfo(6, EnumAchievementManagerAchievement.Fortitude6, 2f),
			new AchievementInfo(8, EnumAchievementManagerAchievement.Fortitude8, 2f),
			new AchievementInfo(10, EnumAchievementManagerAchievement.Fortitude10, 5f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.HighestGamestage, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(10, EnumAchievementManagerAchievement.Gamestage10, 0.5f),
			new AchievementInfo(25, EnumAchievementManagerAchievement.Gamestage25, 1f),
			new AchievementInfo(50, EnumAchievementManagerAchievement.Gamestage50, 2f),
			new AchievementInfo(100, EnumAchievementManagerAchievement.Gamestage100, 5f),
			new AchievementInfo(200, EnumAchievementManagerAchievement.Gamestage200, 10f)
		}),
		new AchievementStatDecl(EnumAchievementDataStat.HighestPlayerLevel, EnumStatType.Int, EnumUpdateType.Max, new List<AchievementInfo>
		{
			new AchievementInfo(7, EnumAchievementManagerAchievement.PlayerLevel7, 0.5f),
			new AchievementInfo(28, EnumAchievementManagerAchievement.PlayerLevel28, 1f),
			new AchievementInfo(70, EnumAchievementManagerAchievement.PlayerLevel70, 2f),
			new AchievementInfo(140, EnumAchievementManagerAchievement.PlayerLevel140, 5f),
			new AchievementInfo(300, EnumAchievementManagerAchievement.PlayerLevel300, 10f)
		})
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> achievementToStat = CreateAchievementToStat();

	[PublicizedFrom(EAccessModifier.Private)]
	public uint version;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object[] statValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumAchievementManagerAchievement, bool> achievementStatuses;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<EnumAchievementManagerAchievement> statCompleteCallback;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsDirty { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> CreateAchievementToStat()
	{
		EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat> enumDictionary = new EnumDictionary<EnumAchievementManagerAchievement, EnumAchievementDataStat>();
		AchievementStatDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			AchievementStatDecl achievementStatDecl = array[i];
			foreach (AchievementInfo achievementInfo in achievementStatDecl.achievementInfos)
			{
				enumDictionary.Add(achievementInfo.achievement, achievementStatDecl.name);
			}
		}
		return enumDictionary;
	}

	public static EnumStatType GetStatType(EnumAchievementDataStat _stat)
	{
		if (_stat != EnumAchievementDataStat.Last)
		{
			return propertyList[(int)_stat].type;
		}
		return EnumStatType.Invalid;
	}

	public static EnumUpdateType GetUpdateType(EnumAchievementDataStat _stat)
	{
		if (_stat != EnumAchievementDataStat.Last)
		{
			return propertyList[(int)_stat].updateType;
		}
		return EnumUpdateType.Replace;
	}

	public static List<AchievementInfo> GetAchievementInfos(EnumAchievementDataStat _stat)
	{
		return propertyList[(int)_stat].achievementInfos;
	}

	public static EnumAchievementDataStat GetStat(EnumAchievementManagerAchievement _achievement)
	{
		return achievementToStat[_achievement];
	}

	public AchievementData()
	{
		statValues = new object[propertyList.Length];
		achievementStatuses = new EnumDictionary<EnumAchievementManagerAchievement, bool>();
		AchievementStatDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			int name = (int)array[i].name;
			statValues[name] = 0;
		}
		for (int j = 0; j < 48; j++)
		{
			achievementStatuses[(EnumAchievementManagerAchievement)j] = false;
		}
	}

	public void UpdateAchievement(EnumAchievementDataStat _stat)
	{
		List<AchievementInfo> achievementInfos = GetAchievementInfos(_stat);
		object achievementStatValue = GetAchievementStatValue(_stat);
		EnumStatType statType = GetStatType(_stat);
		for (int i = 0; i < achievementInfos.Count; i++)
		{
			EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
			if (statType == EnumStatType.Int)
			{
				if ((int)achievementStatValue >= Convert.ToInt32(achievementInfos[i].triggerPoint) && !IsAchievementLocked(achievement))
				{
					LockAchievement(achievement);
				}
			}
			else if (Convert.ToSingle(achievementStatValue) >= Convert.ToSingle(achievementInfos[i].triggerPoint) && !IsAchievementLocked(achievement))
			{
				LockAchievement(achievement);
			}
		}
	}

	public void SetStatCompleteCallback(Action<EnumAchievementManagerAchievement> _statCompleteCallback)
	{
		statCompleteCallback = _statCompleteCallback;
	}

	public int GetIntStatValue(EnumAchievementDataStat _stat)
	{
		if (propertyList[(int)_stat].type == EnumStatType.Int)
		{
			return Convert.ToInt32(statValues[(int)_stat]);
		}
		return -1;
	}

	public float GetFloatStatValue(EnumAchievementDataStat _stat)
	{
		if (propertyList[(int)_stat].type == EnumStatType.Float)
		{
			return Convert.ToSingle(statValues[(int)_stat]);
		}
		return -1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetStatValue(EnumAchievementDataStat _stat, object _value)
	{
		EnumUpdateType updateType = propertyList[(int)_stat].updateType;
		EnumStatType type = propertyList[(int)_stat].type;
		lock (statValues)
		{
			switch (updateType)
			{
			case EnumUpdateType.Sum:
				if (type == EnumStatType.Int)
				{
					statValues[(int)_stat] = Convert.ToInt32(statValues[(int)_stat]) + Convert.ToInt32(_value);
				}
				else
				{
					statValues[(int)_stat] = Convert.ToSingle(statValues[(int)_stat]) + Convert.ToSingle(_value);
				}
				break;
			case EnumUpdateType.Replace:
				statValues[(int)_stat] = _value;
				break;
			case EnumUpdateType.Max:
				if (type == EnumStatType.Int)
				{
					statValues[(int)_stat] = ((Convert.ToInt32(_value) > Convert.ToInt32(statValues[(int)_stat])) ? _value : statValues[(int)_stat]);
				}
				else
				{
					statValues[(int)_stat] = ((Convert.ToSingle(_value) > Convert.ToSingle(statValues[(int)_stat])) ? _value : statValues[(int)_stat]);
				}
				break;
			}
		}
		IsDirty = true;
		UpdateAchievement(_stat);
	}

	public virtual void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
	{
		SetStatValue(_stat, _value);
	}

	public virtual void SetAchievementStat(EnumAchievementDataStat _stat, float _value)
	{
		SetStatValue(_stat, _value);
	}

	public object GetAchievementStatValue(EnumAchievementDataStat _stat)
	{
		if (_stat == EnumAchievementDataStat.Last)
		{
			return 0;
		}
		return statValues[(int)_stat];
	}

	public bool IsAchievementLocked(EnumAchievementManagerAchievement _achievement)
	{
		return achievementStatuses[_achievement];
	}

	public void LockAchievement(EnumAchievementManagerAchievement _achievement)
	{
		lock (achievementStatuses)
		{
			achievementStatuses[_achievement] = true;
		}
		statCompleteCallback?.Invoke(_achievement);
	}

	public float GetGameProgress()
	{
		float num = 0f;
		AchievementStatDecl[] array = propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			AchievementStatDecl achievementStatDecl = array[i];
			int count = achievementStatDecl.achievementInfos.Count;
			for (int j = 0; j < count; j++)
			{
				AchievementInfo achievementInfo = achievementStatDecl.achievementInfos[j];
				if (IsAchievementLocked(achievementInfo.achievement))
				{
					num += achievementInfo.progressContribution;
				}
			}
		}
		return num;
	}

	public void DebugPrintStats()
	{
		for (int i = 0; i < 19; i++)
		{
			string[] obj = new string[6]
			{
				"Stat: ",
				i.ToString(),
				", ",
				null,
				null,
				null
			};
			EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
			obj[3] = enumAchievementDataStat.ToString();
			obj[4] = " = ";
			obj[5] = statValues[i]?.ToString();
			string line = string.Concat(obj);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
		}
	}

	public byte[] Serialize()
	{
		byte[] result = null;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			try
			{
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write('t');
				binaryWriter.Write('t');
				binaryWriter.Write('w');
				binaryWriter.Write((byte)0);
				binaryWriter.Write(1u);
				binaryWriter.Write(Constants.cVersionInformation.LongString);
				for (int i = 0; i < 19; i++)
				{
					EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
					binaryWriter.Write(enumAchievementDataStat.ToString());
					binaryWriter.Write(propertyList[i].type.ToString());
					lock (statValues)
					{
						if (propertyList[i].type == EnumStatType.Int)
						{
							binaryWriter.Write(Convert.ToInt32(statValues[i]));
						}
						else
						{
							binaryWriter.Write(Convert.ToSingle(statValues[i]));
						}
					}
				}
				foreach (KeyValuePair<EnumAchievementManagerAchievement, bool> achievementStatus in achievementStatuses)
				{
					binaryWriter.Write(achievementStatus.Key.ToString());
					lock (achievementStatuses)
					{
						binaryWriter.Write(achievementStatus.Value);
					}
				}
				result = memoryStream.ToArray();
			}
			catch (Exception ex)
			{
				Log.Error("Writing header of achievement data: " + ex.Message);
			}
		}
		return result;
	}

	public void DeserializeBytes(byte[] _bytes)
	{
		Stream stream = null;
		try
		{
			stream = new MemoryStream(_bytes, writable: false);
			DeserializeFromStream(stream);
			stream.Close();
		}
		catch (Exception ex)
		{
			Log.Error("Reading header of achievements: " + ex.Message);
		}
		stream?.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DeserializeFromStream(Stream _stream)
	{
		BinaryReader binaryReader = new BinaryReader(_stream);
		long num = 0L;
		_ = binaryReader.BaseStream.Length;
		if (binaryReader.ReadChar() != 't' || binaryReader.ReadChar() != 't' || binaryReader.ReadChar() != 'w' || binaryReader.ReadChar() != 0)
		{
			return false;
		}
		num += 2;
		version = binaryReader.ReadUInt32();
		num += 4;
		if (version != 1)
		{
			return false;
		}
		string text = binaryReader.ReadString();
		num += text.Length * 2;
		if (text != Constants.cVersionInformation.LongString)
		{
			Log.Warning("Loaded achievement data from different version: '" + text + "'");
		}
		for (int i = 0; i < 19; i++)
		{
			string text2 = binaryReader.ReadString();
			num += text2.Length * 2;
			if ((EnumAchievementDataStat)Enum.Parse(typeof(EnumAchievementDataStat), text2) != (EnumAchievementDataStat)i)
			{
				return false;
			}
			string text3 = binaryReader.ReadString();
			num += text3.Length * 2;
			EnumStatType enumStatType = (EnumStatType)Enum.Parse(typeof(EnumStatType), text3);
			if (propertyList[i].type != enumStatType)
			{
				return false;
			}
			if (enumStatType == EnumStatType.Int)
			{
				statValues[i] = binaryReader.ReadInt32();
				num += 4;
			}
			else
			{
				statValues[i] = binaryReader.ReadSingle();
				num += 4;
			}
		}
		foreach (KeyValuePair<EnumAchievementManagerAchievement, bool> achievementStatus in achievementStatuses)
		{
			_ = achievementStatus;
			string text4 = binaryReader.ReadString();
			num += text4.Length * 2;
			EnumAchievementManagerAchievement key = (EnumAchievementManagerAchievement)Enum.Parse(typeof(EnumAchievementManagerAchievement), text4);
			binaryReader.ReadBoolean();
			num++;
			achievementStatuses[key] = false;
		}
		return true;
	}

	public static void Deserialize(byte[] _bytes, Action<AchievementData> _callback)
	{
		AchievementData achievementData = null;
		TaskManager.Schedule([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			achievementData = new AchievementData();
			try
			{
				achievementData.DeserializeBytes(_bytes);
			}
			catch (Exception)
			{
				achievementData = null;
			}
		}, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			_callback?.Invoke(achievementData);
		});
	}
}
