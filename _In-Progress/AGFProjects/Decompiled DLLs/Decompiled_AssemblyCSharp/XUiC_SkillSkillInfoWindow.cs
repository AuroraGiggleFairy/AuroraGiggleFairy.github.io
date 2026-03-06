using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillSkillInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SkillSkillMilestone> levelEntries = new List<XUiC_SkillSkillMilestone>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> effectLines = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f, bool _b) => _s + ": " + _f.ToCultureInvariantString("0.#") + (_b ? "%" : ""));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => _s + ": " + _f.ToCultureInvariantString("0.#"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, float> groupLevelFormatter = new CachedStringFormatter<float, float>([PublicizedFrom(EAccessModifier.Internal)] (float _i1, float _i2) => _i1.ToCultureInvariantString() + "/" + _i2.ToCultureInvariantString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> groupPointCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => string.Format("{0} {1}", _i, (_i != 1) ? Localization.Get("xuiSkillPoints") : Localization.Get("xuiSkillPoint")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillPercentThisLevelFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillLevelFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat maxSkillLevelFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> buyCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + " " + Localization.Get("xuiSkillPoints"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> expCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + " " + Localization.Get("RewardExp_keyword"));

	public ProgressionValue CurrentSkill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.selectedSkill == null || !base.xui.selectedSkill.ProgressionClass.IsSkill)
			{
				return null;
			}
			return base.xui.selectedSkill;
		}
	}

	public override void Init()
	{
		base.Init();
		GetChildrenByType(levelEntries);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		effectLines.Clear();
		if (CurrentSkill != null)
		{
			int level = CurrentSkill.Level;
			foreach (MinEffectGroup effectGroup in CurrentSkill.ProgressionClass.Effects.EffectGroups)
			{
				foreach (PassiveEffect passiveEffect in effectGroup.PassiveEffects)
				{
					float _base_value = 0f;
					float _perc_value = 1f;
					int entityClass = base.xui.playerUI.entityPlayer.entityClass;
					if (EntityClass.list.ContainsKey(entityClass) && EntityClass.list[entityClass].Effects != null)
					{
						EntityClass.list[entityClass].Effects.ModifyValue(base.xui.playerUI.entityPlayer, passiveEffect.Type, ref _base_value, ref _perc_value, 0f, EntityClass.list[entityClass].Tags);
					}
					float num = _base_value;
					passiveEffect.ModifyValue(base.xui.playerUI.entityPlayer, level, ref _base_value, ref _perc_value, passiveEffect.Tags);
					if (_base_value != num || _perc_value != 1f)
					{
						if (_base_value == num)
						{
							effectLines.Add(attributeSubtractionFormatter.Format(passiveEffect.Type.ToStringCached(), 100f * _perc_value, _v3: true));
						}
						else
						{
							effectLines.Add(attributeSetValueFormatter.Format(passiveEffect.Type.ToStringCached(), _perc_value * _base_value));
						}
					}
				}
			}
		}
		if (CurrentSkill != null)
		{
			float num2 = 0f;
			float num3 = ((CurrentSkill.ProgressionClass.MaxLevel - CurrentSkill.ProgressionClass.MinLevel > 5) ? ((float)CurrentSkill.ProgressionClass.MaxLevel / 5f) : 1f);
			{
				foreach (XUiC_SkillSkillMilestone levelEntry in levelEntries)
				{
					if (num3 < 2f && num2 < (float)CurrentSkill.ProgressionClass.MinLevel)
					{
						num2 = CurrentSkill.ProgressionClass.MinLevel;
					}
					levelEntry.LevelStart = Mathf.RoundToInt(num2);
					if (levelEntry.LevelStart < CurrentSkill.ProgressionClass.MinLevel)
					{
						levelEntry.LevelStart = CurrentSkill.ProgressionClass.MinLevel;
					}
					float num4 = num2 + num3;
					if (Mathf.RoundToInt(num4) == Mathf.RoundToInt(num2))
					{
						num4 += 1f;
					}
					levelEntry.LevelGoal = Mathf.RoundToInt(num4);
					levelEntry.IsDirty = true;
					num2 = num4;
				}
				return;
			}
		}
		foreach (XUiC_SkillSkillMilestone levelEntry2 in levelEntries)
		{
			levelEntry2.LevelStart = 0;
			levelEntry2.LevelGoal = 1;
			levelEntry2.IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiEventManager.Instance.OnSkillExperienceAdded += Current_OnSkillExperienceAdded;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiEventManager.Instance.OnSkillExperienceAdded -= Current_OnSkillExperienceAdded;
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			IsDirty = false;
			UpdateSkill();
			RefreshBindings(IsDirty);
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_OnSkillExperienceAdded(ProgressionValue _changedSkill, int _newXp)
	{
		if (CurrentSkill == _changedSkill)
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (_bindingName)
		{
		case "groupicon":
			_value = ((CurrentSkill != null) ? CurrentSkill.ProgressionClass.Icon : "ui_game_symbol_skills");
			return true;
		case "groupname":
			_value = ((CurrentSkill != null) ? Localization.Get(CurrentSkill.ProgressionClass.NameKey) : "Skill Info");
			return true;
		case "groupdescription":
			_value = ((CurrentSkill != null) ? Localization.Get(CurrentSkill.ProgressionClass.DescKey) : "");
			return true;
		case "skillpercentthislevel":
			_value = ((CurrentSkill != null && CurrentSkill.CalculatedLevel(entityPlayer) < CurrentSkill.ProgressionClass.MaxLevel) ? skillPercentThisLevelFormatter.Format(CurrentSkill.PercToNextLevel) : "1");
			return true;
		case "skillLevel":
			_value = ((CurrentSkill != null) ? skillLevelFormatter.Format(CurrentSkill.GetCalculatedLevel(entityPlayer)) : "0");
			return true;
		case "nextSkillLevel":
			_value = ((CurrentSkill != null) ? skillLevelFormatter.Format(CurrentSkill.GetCalculatedLevel(entityPlayer) + 1f) : "0");
			return true;
		case "nextSkillLevelLocked":
			_value = ((CurrentSkill != null) ? (CurrentSkill.CalculatedLevel(entityPlayer) < CurrentSkill.ProgressionClass.MaxLevel && CurrentSkill.CalculatedMaxLevel(entityPlayer) < CurrentSkill.CalculatedLevel(entityPlayer) + 1).ToString() : "false");
			return true;
		case "nextSkillLevelRequirement":
			if (CurrentSkill != null)
			{
				ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue(CurrentSkill.ProgressionClass.ParentName);
				_value = string.Format(Localization.Get("xuiSkillRequirement"), Localization.Get(progressionValue.ProgressionClass.NameKey), Mathf.CeilToInt((float)CurrentSkill.Level / CurrentSkill.ProgressionClass.ParentMaxLevelRatio));
			}
			else
			{
				_value = "false";
			}
			return true;
		case "maxSkillLevel":
			_value = ((CurrentSkill != null) ? maxSkillLevelFormatter.Format(CurrentSkill.ProgressionClass.MaxLevel) : "0");
			return true;
		case "notmaxlevel":
			_value = ((CurrentSkill != null) ? (CurrentSkill.CalculatedLevel(entityPlayer) < CurrentSkill.ProgressionClass.MaxLevel).ToString() : "true");
			return true;
		case "currentlevel":
			_value = Localization.Get("xuiSkillLevel");
			return true;
		case "effectsCol1":
		{
			int num = effectLines.Count;
			if (effectLines.Count > 3)
			{
				num = effectLines.Count / 2 + effectLines.Count % 2;
			}
			_value = "";
			for (int j = 0; j < num; j++)
			{
				if (_value.Length > 0)
				{
					_value += "\n";
				}
				_value += effectLines[j];
			}
			return true;
		}
		case "effectsCol2":
		{
			int num = effectLines.Count;
			if (effectLines.Count > 3)
			{
				num = effectLines.Count / 2 + effectLines.Count % 2;
			}
			_value = "";
			for (int i = num; i < effectLines.Count; i++)
			{
				if (_value.Length > 0)
				{
					_value += "\n";
				}
				_value += effectLines[i];
			}
			return true;
		}
		default:
			return false;
		}
	}
}
