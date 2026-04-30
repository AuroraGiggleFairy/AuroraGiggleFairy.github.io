using System.Collections.Generic;
using System.Text;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillAttributeLevel : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_bought;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_nerfed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_buffed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int level;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxEntriesWithoutPaging = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

	public XUiV_Button btnBuy;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> groupPointCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => string.Format("{0}: {1} {2}", Localization.Get("xuiSkillBuy"), _i, (_i != 1) ? Localization.Get("xuiSkillPoints") : Localization.Get("xuiSkillPoint")));

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

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder effectsStringBuilder = new StringBuilder();

	public int ListIndex
	{
		set
		{
			if (value != listIndex)
			{
				listIndex = value;
				IsDirty = true;
			}
		}
	}

	public int Level
	{
		get
		{
			return level;
		}
		set
		{
			if (value != level)
			{
				level = value;
				IsDirty = true;
			}
		}
	}

	public int MaxEntriesWithoutPaging
	{
		set
		{
			if (maxEntriesWithoutPaging != value)
			{
				maxEntriesWithoutPaging = value;
				IsDirty = true;
			}
		}
	}

	public int HiddenEntriesWithPaging
	{
		set
		{
			if (hiddenEntriesWithPaging != value)
			{
				hiddenEntriesWithPaging = value;
				IsDirty = true;
			}
		}
	}

	public ProgressionValue CurrentSkill
	{
		get
		{
			if (base.xui.selectedSkill == null || !base.xui.selectedSkill.ProgressionClass.IsAttribute)
			{
				return null;
			}
			return base.xui.selectedSkill;
		}
	}

	public override void Init()
	{
		base.Init();
		btnBuy = (XUiV_Button)GetChildById("btnBuy").ViewComponent;
		btnBuy.Controller.OnPress += btnBuy_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnBuy_OnPress(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (CurrentSkill.Level + 1 == level && CurrentSkill.CalculatedMaxLevel(entityPlayer) >= level && CurrentSkill.CanPurchase(entityPlayer, level) && entityPlayer.Progression.SkillPoints >= CurrentSkill.ProgressionClass.CalculatedCostForLevel(level) && CurrentSkill.CostForNextLevel > 0)
		{
			CurrentSkill.Level++;
			entityPlayer.Progression.SkillPoints -= CurrentSkill.ProgressionClass.CalculatedCostForLevel(level);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySetSkillLevelServer>().Setup(entityPlayer.entityId, CurrentSkill.Name, CurrentSkill.Level));
			}
			base.xui.Recipes.RefreshTrackedRecipe();
			QuestEventManager.Current.SpendSkillPoint(CurrentSkill);
			Manager.PlayInsidePlayerHead("ui_skill_purchase");
			base.WindowGroup.Controller.RefreshBindingsSelfAndChildren();
			base.WindowGroup.Controller.SetAllChildrenDirty();
			entityPlayer.bPlayerStatsChanged = true;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "color_bg_bought":
			color_bg_bought = _value;
			return true;
		case "color_bg_available":
			color_bg_available = _value;
			return true;
		case "color_bg_locked":
			color_bg_locked = _value;
			return true;
		case "color_lbl_available":
			color_lbl_available = _value;
			return true;
		case "color_lbl_locked":
			color_lbl_locked = _value;
			return true;
		case "color_lbl_nerfed":
			color_lbl_nerfed = _value;
			return true;
		case "color_lbl_buffed":
			color_lbl_buffed = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		bool flag = CurrentSkill != null && CurrentSkill.ProgressionClass.MaxLevel >= level;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (flag)
		{
			flag3 = CurrentSkill.Level >= level;
			flag2 = CurrentSkill.Level + 1 == level && CurrentSkill.Level + 1 <= CurrentSkill.CalculatedMaxLevel(entityPlayer);
			flag4 = !flag3 && CurrentSkill.CalculatedLevel(entityPlayer) >= level;
			flag5 = flag3 && CurrentSkill.CalculatedLevel(entityPlayer) < level;
		}
		switch (_bindingName)
		{
		case "nothiddenbypager":
			_value = (CurrentSkill == null || CurrentSkill.ProgressionClass.MaxLevel <= maxEntriesWithoutPaging || listIndex < maxEntriesWithoutPaging - hiddenEntriesWithPaging).ToString();
			return true;
		case "hasentry":
			_value = flag.ToString();
			return true;
		case "level":
			_value = level.ToString();
			return true;
		case "color_bg":
			if (flag3)
			{
				_value = color_bg_bought;
			}
			else if (flag2)
			{
				_value = color_bg_available;
			}
			else
			{
				_value = color_bg_locked;
			}
			return true;
		case "color_fg":
			if (flag3 || flag2)
			{
				_value = color_lbl_available;
			}
			else
			{
				_value = color_lbl_locked;
			}
			return true;
		case "text":
			effectsStringBuilder.Length = 0;
			if (flag && CurrentSkill.ProgressionClass != null && CurrentSkill.ProgressionClass.Effects != null && CurrentSkill.ProgressionClass.Effects.EffectGroups != null)
			{
				foreach (MinEffectGroup effectGroup in CurrentSkill.ProgressionClass.Effects.EffectGroups)
				{
					if (effectGroup.EffectDescriptions != null)
					{
						for (int j = 0; j < effectGroup.EffectDescriptions.Count; j++)
						{
							if (level >= effectGroup.EffectDescriptions[j].MinLevel && level <= effectGroup.EffectDescriptions[j].MaxLevel)
							{
								_value = effectGroup.EffectDescriptions[j].Description;
								return true;
							}
						}
					}
					foreach (PassiveEffect passiveEffect in effectGroup.PassiveEffects)
					{
						float _base_value = 0f;
						float _perc_value = 1f;
						int entityClass = entityPlayer.entityClass;
						if (EntityClass.list.ContainsKey(entityClass) && EntityClass.list[entityClass].Effects != null)
						{
							EntityClass.list[entityClass].Effects.ModifyValue(entityPlayer, passiveEffect.Type, ref _base_value, ref _perc_value, 0f, EntityClass.list[entityClass].Tags);
						}
						float num3 = _base_value;
						passiveEffect.ModifyValue(entityPlayer, level, ref _base_value, ref _perc_value, passiveEffect.Tags);
						if (_base_value != num3 || _perc_value != 1f)
						{
							if (effectsStringBuilder.Length > 0)
							{
								effectsStringBuilder.Append(", ");
							}
							if (_base_value == num3)
							{
								effectsStringBuilder.Append(attributeSubtractionFormatter.Format(passiveEffect.Type.ToStringCached(), 100f * _perc_value, _v3: true));
							}
							else
							{
								effectsStringBuilder.Append(attributeSetValueFormatter.Format(passiveEffect.Type.ToStringCached(), _perc_value * _base_value));
							}
						}
					}
				}
			}
			_value = effectsStringBuilder.ToString();
			return true;
		case "buytooltip":
			if (flag3 && flag5)
			{
				_value = Localization.Get("xuiSkillNerfedEffect");
			}
			else if (flag3)
			{
				_value = "";
			}
			else if (flag2)
			{
				int num = CurrentSkill.ProgressionClass.CalculatedCostForLevel(level);
				_value = ((num > 0) ? groupPointCostFormatter.Format(num) : "NA");
			}
			else if (flag)
			{
				_value = "";
				LevelRequirement requirementsForLevel = CurrentSkill.ProgressionClass.GetRequirementsForLevel(level);
				if (requirementsForLevel.Requirements != null)
				{
					List<string> list = new List<string>();
					requirementsForLevel.Requirements.GetInfoStrings(ref list);
					for (int i = 0; i < list.Count; i++)
					{
						if (i > 0)
						{
							_value += "\n";
						}
						_value += list[i];
					}
				}
				if (_value == "")
				{
					int num2 = CurrentSkill.ProgressionClass.CalculatedCostForLevel(level);
					_value = ((num2 > 0) ? groupPointCostFormatter.Format(num2) : "NA");
				}
			}
			else
			{
				_value = "";
			}
			return true;
		case "buyvisible":
			_value = flag.ToString();
			return true;
		case "buyicon":
			if (flag3)
			{
				_value = "ui_game_symbol_check";
			}
			else if (flag2)
			{
				_value = "ui_game_symbol_shopping_cart";
			}
			else
			{
				_value = "ui_game_symbol_lock";
			}
			return true;
		case "buycolor":
			if (flag5)
			{
				_value = color_lbl_nerfed;
			}
			else if (flag4)
			{
				_value = color_lbl_buffed;
			}
			else if (flag3 || flag2)
			{
				_value = color_lbl_available;
			}
			else
			{
				_value = color_lbl_locked;
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
			RefreshBindings(IsDirty);
		}
		base.Update(_dt);
	}
}
