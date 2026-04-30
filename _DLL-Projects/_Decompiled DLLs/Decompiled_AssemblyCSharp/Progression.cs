using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Progression
{
	public enum XPTypes
	{
		Kill,
		Harvesting,
		Upgrading,
		Crafting,
		Selling,
		Quest,
		Looting,
		Party,
		Other,
		Repairing,
		Debug,
		Max
	}

	public const byte cVersion = 3;

	public static int BaseExpToLevel;

	public static int ClampExpCostAtLevel;

	public static float ExpMultiplier;

	public static int MaxLevel;

	public static int SkillPointsPerLevel;

	public static float SkillPointMultiplier;

	public static Dictionary<string, ProgressionClass> ProgressionClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryNameIdMapping ProgressionNameIds = new DictionaryNameIdMapping();

	public bool bProgressionStatsChanged;

	public int ExpToNextLevel;

	public int ExpDeficit;

	public int Level = 1;

	public int SkillPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ExpDeficitGained;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryNameId<ProgressionValue> ProgressionValues = new DictionaryNameId<ProgressionValue>(ProgressionNameIds);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> ProgressionValueQuickList = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> eventList = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PassiveEffects, List<ProgressionValue>> passiveEffects = new Dictionary<PassiveEffects, List<ProgressionValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global>[] xpFastTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timer = 1f;

	public Progression()
	{
		ExpToNextLevel = getExpForLevel(Level + 1);
	}

	public Progression(EntityAlive _parent)
	{
		parent = _parent;
		ExpToNextLevel = getExpForLevel(Level + 1);
		SetupData();
	}

	public Dictionary<int, ProgressionValue> GetDict()
	{
		return ProgressionValues.Dict;
	}

	public static int CalcId(string _name)
	{
		return ProgressionNameIds.Add(_name);
	}

	public ProgressionValue GetProgressionValue(int _id)
	{
		return ProgressionValues.Get(_id);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getLevelFloat()
	{
		return (float)Level + (1f - (float)ExpToNextLevel / (float)GetExpForNextLevel());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int getExpForLevel(float _level)
	{
		return (int)Math.Min((float)BaseExpToLevel * Mathf.Pow(ExpMultiplier, _level), 2.1474836E+09f);
	}

	public int GetLevel()
	{
		return Level;
	}

	public int GetExpForNextLevel()
	{
		return getExpForLevel(Mathf.Clamp(Level + 1, 0, ClampExpCostAtLevel));
	}

	public float GetLevelProgressPercentage()
	{
		return getLevelFloat() - (float)Level;
	}

	public void ModifyValue(PassiveEffects _effect, ref float _baseVal, ref float _percVal, FastTags<TagGroup.Global> _tags)
	{
		if (_effect != PassiveEffects.AttributeLevel && _effect != PassiveEffects.SkillLevel && _effect != PassiveEffects.PerkLevel && passiveEffects.TryGetValue(_effect, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				ProgressionValue progressionValue = value[i];
				progressionValue.ProgressionClass?.ModifyValue(parent, progressionValue, _effect, ref _baseVal, ref _percVal, _tags);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, PassiveEffects _effect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> _tags)
	{
		if (_effect == PassiveEffects.AttributeLevel || _effect == PassiveEffects.SkillLevel || _effect == PassiveEffects.PerkLevel)
		{
			return;
		}
		for (int i = 0; i < ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = ProgressionValueQuickList[i];
			if (progressionValue != null)
			{
				ProgressionClass progressionClass = progressionValue.ProgressionClass;
				if (progressionClass != null && progressionClass.Effects != null && progressionClass.Effects.PassivesIndex != null && progressionClass.Effects.PassivesIndex.Contains(_effect))
				{
					progressionClass.GetModifiedValueData(_modValueSources, _sourceType, parent, progressionValue, _effect, ref _base_val, ref _perc_val, _tags);
				}
			}
		}
	}

	public void Update()
	{
		if (timer <= 0f)
		{
			FireEvent(MinEventTypes.onSelfProgressionUpdate, parent.MinEventContext);
			timer = 1f;
		}
		else
		{
			timer -= Time.deltaTime;
		}
		parent.Buffs.SetCustomVar("_expdeficit", ExpDeficit);
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (eventList != null)
		{
			for (int i = 0; i < eventList.Count; i++)
			{
				ProgressionValue progressionValue = eventList[i];
				ProgressionClass progressionClass = progressionValue.ProgressionClass;
				_params.ProgressionValue = progressionValue;
				progressionClass.FireEvent(_eventType, _params);
			}
		}
	}

	public int AddLevelExp(int _exp, string _cvarXPName = "_xpOther", XPTypes _xpType = XPTypes.Other, bool useBonus = true, bool notifyUI = true, int _instigatorID = -1)
	{
		if (parent as EntityPlayer == null)
		{
			return _exp;
		}
		parent.MinEventContext.Instigator = ((_instigatorID == -1) ? null : (parent.world.GetEntity(_instigatorID) as EntityAlive));
		float num = _exp;
		if (useBonus)
		{
			if (xpFastTags == null)
			{
				xpFastTags = new FastTags<TagGroup.Global>[11];
				for (int i = 0; i < 11; i++)
				{
					xpFastTags[i] = FastTags<TagGroup.Global>.Parse(((XPTypes)i).ToStringCached());
				}
			}
			num = num * (float)GameStats.GetInt(EnumGameStats.XPMultiplier) / 100f;
			num = EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num, parent, null, xpFastTags[(int)_xpType]);
		}
		if (num > 214748370f)
		{
			num = 214748370f;
		}
		if (_xpType != XPTypes.Debug)
		{
			GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.XpEarnedBy, _xpType.ToStringCached(), num);
		}
		int level = Level;
		EntityPlayerLocal entityPlayerLocal = parent as EntityPlayerLocal;
		if ((bool)entityPlayerLocal)
		{
			entityPlayerLocal.PlayerUI.xui.CollectedItemList.AddIconNotification("ui_game_symbol_xp", (int)num);
		}
		AddLevelExpRecursive((int)num, _cvarXPName, notifyUI);
		if (Level != level)
		{
			Log.Out("{0} made level {1} (was {2}), exp for next level {3}", parent.EntityName, Level, level, ExpToNextLevel);
		}
		return (int)num;
	}

	public void OnDeath()
	{
	}

	public void AddXPDeficit()
	{
		ExpDeficit += (int)((float)GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitPerDeathPercentage, null, 0.1f, parent));
		ExpDeficit = Mathf.Clamp(ExpDeficit, 0, (int)((float)GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, parent)));
		ExpDeficitGained = true;
	}

	public void OnRespawnFromDeath()
	{
		if (ExpDeficitGained)
		{
			EntityPlayerLocal player = parent as EntityPlayerLocal;
			if (ExpDeficit == (int)((float)GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, parent)))
			{
				GameManager.ShowTooltip(player, Localization.Get("ttResurrectMaxXPLost"));
			}
			else
			{
				GameManager.ShowTooltip(player, string.Format(Localization.Get("ttResurrectXPLost"), EffectManager.GetValue(PassiveEffects.ExpDeficitPerDeathPercentage, null, 0.1f, parent) * 100f));
			}
			ExpDeficitGained = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLevelExpRecursive(int exp, string _cvarXPName, bool notifyUI = true)
	{
		if (Level >= MaxLevel)
		{
			Level = MaxLevel;
			return;
		}
		parent.Buffs.IncrementCustomVar(_cvarXPName, exp);
		int num;
		if (ExpDeficit > 0)
		{
			num = exp - ExpDeficit;
			ExpDeficit -= exp;
			ExpDeficit = Mathf.Clamp(ExpDeficit, 0, (int)((float)GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, parent)));
		}
		else
		{
			num = exp - ExpToNextLevel;
			ExpToNextLevel -= exp;
		}
		EntityPlayerLocal entityPlayerLocal = parent as EntityPlayerLocal;
		if (ExpDeficit <= 0)
		{
			int level = Level;
			if (ExpToNextLevel <= 0)
			{
				Level++;
				if (SkillPointMultiplier == 0f)
				{
					SkillPoints += SkillPointsPerLevel;
				}
				else
				{
					SkillPoints += (int)Math.Min((float)SkillPointsPerLevel * Mathf.Pow(SkillPointMultiplier, Level), 2.1474836E+09f);
				}
				if ((bool)entityPlayerLocal)
				{
					GameSparksCollector.PlayerLevelUp(entityPlayerLocal, Level);
				}
				ExpToNextLevel = GetExpForNextLevel();
			}
			if ((ExpToNextLevel > num || Level == MaxLevel) && level != Level && (bool)entityPlayerLocal && notifyUI)
			{
				GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttLevelUp"), Level.ToString(), SkillPoints), string.Empty, "levelupplayer");
			}
		}
		if (num > 0)
		{
			AddLevelExpRecursive(num, _cvarXPName);
		}
	}

	public void SpendSkillPoints(int _points, string _progressionName)
	{
		ProgressionValue progressionValue = GetProgressionValue(_progressionName);
		if (progressionValue != null && progressionValue.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
		{
			addProgressionCurrency(_points, progressionValue);
		}
	}

	public ProgressionValue GetProgressionValue(string _progressionName)
	{
		return ProgressionValues.Get(_progressionName);
	}

	public void GetPerkList(List<ProgressionValue> perkList, string _skillName)
	{
		perkList.Clear();
		for (int i = 0; i < ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = ProgressionValueQuickList[i];
			if ((progressionValue.ProgressionClass.Type == ProgressionType.Perk || progressionValue.ProgressionClass.Type == ProgressionType.Book) && progressionValue.ProgressionClass.Parent.Name == _skillName)
			{
				perkList.Add(progressionValue);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addProgressionCurrency(int _currencyAmount, ProgressionValue _pv)
	{
		if (_pv == null)
		{
			return;
		}
		ProgressionClass progressionClass = _pv.ProgressionClass;
		if (_pv.Level >= progressionClass.MaxLevel)
		{
			if (_pv.Level > progressionClass.MaxLevel)
			{
				_pv.Level = progressionClass.MaxLevel;
			}
			return;
		}
		if (progressionClass.Type == ProgressionType.Skill)
		{
			_currencyAmount = (int)EffectManager.GetValue(PassiveEffects.SkillExpGain, null, _currencyAmount, parent);
		}
		int num = _currencyAmount - _pv.CostForNextLevel;
		_pv.CostForNextLevel -= _currencyAmount;
		if (_pv.CostForNextLevel <= 0)
		{
			_pv.Level++;
			_pv.CostForNextLevel = progressionClass.CalculatedCostForLevel(_pv.Level + 1);
		}
		if (num > 0)
		{
			addProgressionCurrency(num, _pv);
		}
	}

	public byte[] ToBytes(bool _IsNetwork = false)
	{
		byte[] array = null;
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
		MemoryStream memoryStream = new MemoryStream();
		pooledBinaryWriter.SetBaseStream(memoryStream);
		Write(pooledBinaryWriter, _IsNetwork);
		array = memoryStream.ToArray();
		memoryStream.Dispose();
		return array;
	}

	public void Write(BinaryWriter _bw, bool _IsNetwork = false)
	{
		_bw.Write((byte)3);
		_bw.Write((ushort)Level);
		_bw.Write(ExpToNextLevel);
		_bw.Write((ushort)SkillPoints);
		int count = ProgressionValues.Count;
		_bw.Write(count);
		foreach (KeyValuePair<int, ProgressionValue> item in ProgressionValues.Dict)
		{
			item.Value.Write(_bw, _IsNetwork);
		}
		_bw.Write(ExpDeficit);
	}

	public static Progression FromBytes(byte[] data, EntityAlive _parent)
	{
		Progression result = null;
		try
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			MemoryStream memoryStream = new MemoryStream(data);
			pooledBinaryReader.SetBaseStream(memoryStream);
			result = Read(pooledBinaryReader, _parent);
			memoryStream.Dispose();
		}
		catch
		{
			result = null;
		}
		return result;
	}

	public static Progression Read(BinaryReader _br, EntityAlive _parent)
	{
		Progression progression = _parent.Progression;
		if (progression == null)
		{
			Log.Warning("Progression Read {0}, new", _parent);
			progression = (_parent.Progression = new Progression(_parent));
		}
		byte b = _br.ReadByte();
		progression.Level = _br.ReadUInt16();
		progression.ExpToNextLevel = _br.ReadInt32();
		progression.SkillPoints = _br.ReadUInt16();
		int num = _br.ReadInt32();
		ProgressionValue progressionValue = new ProgressionValue();
		for (int i = 0; i < num; i++)
		{
			progressionValue.Read(_br);
			if (ProgressionClasses.ContainsKey(progressionValue.Name))
			{
				ProgressionValue progressionValue2 = progression.ProgressionValues.Get(progressionValue.Name);
				if (progressionValue2 != null)
				{
					progressionValue2.CopyFrom(progressionValue);
					continue;
				}
				Log.Error("ProgressionValues missing {0}", progressionValue.Name);
				progressionValue2 = new ProgressionValue();
				progressionValue2.CopyFrom(progressionValue);
				progression.ProgressionValues.Add(progressionValue.Name, progressionValue2);
			}
		}
		if (b > 2)
		{
			progression.ExpDeficit = _br.ReadInt32();
		}
		progression.SetupData();
		return progression;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupData()
	{
		foreach (KeyValuePair<string, ProgressionClass> progressionClass2 in ProgressionClasses)
		{
			string name = progressionClass2.Value.Name;
			if (!ProgressionValues.Contains(name))
			{
				ProgressionValue value = new ProgressionValue(name)
				{
					Level = progressionClass2.Value.MinLevel,
					CostForNextLevel = progressionClass2.Value.CalculatedCostForLevel(Level + 1)
				};
				ProgressionValues.Add(name, value);
			}
		}
		ProgressionValueQuickList.Clear();
		foreach (KeyValuePair<int, ProgressionValue> item in ProgressionValues.Dict)
		{
			ProgressionValueQuickList.Add(item.Value);
		}
		eventList.Clear();
		passiveEffects.Clear();
		for (int i = 0; i < ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			if (progressionClass.HasEvents())
			{
				eventList.Add(progressionValue);
			}
			MinEffectController effects = progressionClass.Effects;
			if (effects == null)
			{
				continue;
			}
			HashSet<PassiveEffects> passivesIndex = effects.PassivesIndex;
			if (passivesIndex == null)
			{
				continue;
			}
			foreach (PassiveEffects item2 in passivesIndex)
			{
				if (!passiveEffects.TryGetValue(item2, out var value2))
				{
					value2 = new List<ProgressionValue>();
					passiveEffects.Add(item2, value2);
				}
				value2.Add(progressionValue);
			}
		}
	}

	public void ClearProgressionClassLinks()
	{
		if (ProgressionValueQuickList == null)
		{
			return;
		}
		foreach (ProgressionValue progressionValueQuick in ProgressionValueQuickList)
		{
			progressionValueQuick?.ClearProgressionClassLink();
		}
		SetupData();
	}

	public static void Cleanup()
	{
		if (ProgressionClasses != null)
		{
			ProgressionClasses.Clear();
		}
	}

	public void ResetProgression(bool _resetSkills = true, bool _resetBooks = false, bool _resetCrafting = false)
	{
		int num = 0;
		for (int i = 0; i < ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			if (progressionClass.IsBook)
			{
				if (!_resetBooks)
				{
					continue;
				}
				progressionValue.Level = 0;
			}
			if (progressionClass.IsCrafting)
			{
				if (!_resetCrafting)
				{
					continue;
				}
				progressionValue.Level = 1;
			}
			if (!_resetSkills)
			{
				continue;
			}
			if (progressionClass.IsAttribute)
			{
				if (progressionValue.Level > 1)
				{
					for (int j = 2; j <= progressionValue.Level; j++)
					{
						num += progressionClass.CalculatedCostForLevel(j);
					}
					progressionValue.Level = 1;
				}
			}
			else if (progressionClass.IsPerk && progressionValue.Level > 0)
			{
				for (int k = 1; k <= progressionValue.Level; k++)
				{
					num += progressionClass.CalculatedCostForLevel(k);
				}
				progressionValue.Level = 0;
			}
		}
		if (parent is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.PlayerUI.xui.Recipes.RefreshTrackedRecipe();
		}
		SkillPoints += num;
	}

	public void RefreshPerks(string attribute)
	{
		for (int i = 0; i < ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			if (progressionClass.IsPerk && (attribute == "" || attribute.EqualsCaseInsensitive(progressionClass.ParentName)))
			{
				progressionValue.CalculatedLevel(parent);
			}
		}
	}
}
