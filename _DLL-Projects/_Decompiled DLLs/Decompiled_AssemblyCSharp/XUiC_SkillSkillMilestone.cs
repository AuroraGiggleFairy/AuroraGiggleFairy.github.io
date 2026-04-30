using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillSkillMilestone : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> effectLines = new List<string>();

	public int LevelStart;

	public int LevelGoal;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeMultiplicationFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => string.Format("{0}: {1}%", _s, (_f < 0f) ? _f.ToCultureInvariantString("0.#") : ("+" + _f.ToCultureInvariantString("0.#"))));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeDivisionFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => string.Format("{0}: {1}%", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeAdditionFormatter = new CachedStringFormatter<string, float, bool>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f, bool _b) => string.Format("{0}: +{1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f, bool _b) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> attributeLockedFormatter = new CachedStringFormatter<string, string>([PublicizedFrom(EAccessModifier.Internal)] (string _s1, string _s2) => $"{_s1}: {_s2}");

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

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		effectLines.Clear();
		if (CurrentSkill == null)
		{
			return;
		}
		int levelGoal = LevelGoal;
		foreach (MinEffectGroup effectGroup in CurrentSkill.ProgressionClass.Effects.EffectGroups)
		{
			if (effectGroup.EffectDescriptions != null)
			{
				for (int i = 0; i < effectGroup.EffectDescriptions.Count; i++)
				{
					if (levelGoal >= effectGroup.EffectDescriptions[i].MinLevel && levelGoal <= effectGroup.EffectDescriptions[i].MaxLevel)
					{
						effectLines.Add(effectGroup.EffectDescriptions[i].Description);
						return;
					}
				}
			}
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
				passiveEffect.ModifyValue(base.xui.playerUI.entityPlayer, levelGoal, ref _base_value, ref _perc_value, passiveEffect.Tags);
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

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "color_lbl_available"))
		{
			if (_name == "color_lbl_locked")
			{
				color_lbl_locked = _value;
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		color_lbl_available = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		bool flag = CurrentSkill != null && CurrentSkill.ProgressionClass.MaxLevel >= LevelGoal;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (_bindingName)
		{
		case "hasentry":
			_value = flag.ToString();
			return true;
		case "progress":
			if (flag)
			{
				if (CurrentSkill.CalculatedLevel(entityPlayer) < LevelStart)
				{
					_value = "0.0";
				}
				else if (CurrentSkill.CalculatedLevel(entityPlayer) >= LevelGoal)
				{
					_value = "1.0";
				}
				else
				{
					float num = (float)CurrentSkill.CalculatedLevel(entityPlayer) + CurrentSkill.PercToNextLevel;
					_value = ((num - (float)LevelStart) / (float)(LevelGoal - LevelStart)).ToCultureInvariantString();
				}
			}
			else
			{
				_value = "0.0";
			}
			return true;
		case "color_fg":
			if (flag && CurrentSkill.CalculatedLevel(entityPlayer) >= LevelGoal && !CurrentSkill.IsLocked(entityPlayer))
			{
				_value = color_lbl_available;
			}
			else
			{
				_value = color_lbl_locked;
			}
			return true;
		case "level":
			_value = LevelGoal.ToString();
			return true;
		case "effectsCol1":
		{
			int num2 = effectLines.Count;
			if (effectLines.Count > 3)
			{
				num2 = effectLines.Count / 2 + effectLines.Count % 2;
			}
			_value = "";
			for (int i = 0; i < num2; i++)
			{
				if (_value.Length > 0)
				{
					_value += "\n";
				}
				_value += effectLines[i];
			}
			return true;
		}
		case "effectsCol2":
		{
			int num2 = effectLines.Count;
			if (effectLines.Count > 3)
			{
				num2 = effectLines.Count / 2 + effectLines.Count % 2;
			}
			_value = "";
			for (int j = num2; j < effectLines.Count; j++)
			{
				if (_value.Length > 0)
				{
					_value += "\n";
				}
				_value += effectLines[j];
			}
			return true;
		}
		case "icon":
			if (flag && CurrentSkill.CalculatedLevel(entityPlayer) >= LevelGoal)
			{
				_value = "ui_game_symbol_check";
			}
			else
			{
				_value = "ui_game_symbol_lock";
			}
			return true;
		case "iconvisible":
			_value = (flag && (CurrentSkill.CalculatedLevel(entityPlayer) >= LevelGoal || CurrentSkill.CalculatedMaxLevel(entityPlayer) < LevelGoal)).ToString();
			return true;
		case "icontooltip":
			if (flag && CurrentSkill.CalculatedMaxLevel(entityPlayer) < LevelGoal)
			{
				ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue(CurrentSkill.ProgressionClass.ParentName);
				_value = string.Format(Localization.Get("xuiSkillRequirement"), Localization.Get(progressionValue.ProgressionClass.NameKey), Mathf.CeilToInt((float)LevelGoal / CurrentSkill.ProgressionClass.ParentMaxLevelRatio));
			}
			else
			{
				_value = "";
			}
			return true;
		default:
			return false;
		}
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
}
