using System.Text;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillBookLevel : XUiController
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
	public ProgressionValue perk;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volume;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool completionReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxEntriesWithoutPaging = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

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

	public ProgressionValue Perk
	{
		get
		{
			return perk;
		}
		set
		{
			if (value != perk)
			{
				perk = value;
				IsDirty = true;
			}
		}
	}

	public int Volume
	{
		set
		{
			if (value != volume)
			{
				volume = value;
				IsDirty = true;
			}
		}
	}

	public bool CompletionReward
	{
		get
		{
			return completionReward;
		}
		set
		{
			completionReward = value;
			IsDirty = true;
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
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.selectedSkill == null || !base.xui.selectedSkill.ProgressionClass.IsBookGroup)
			{
				return null;
			}
			return base.xui.selectedSkill;
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
		bool flag = CurrentSkill != null && perk != null;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		bool flag2 = false;
		if (flag)
		{
			flag2 = perk != null && perk.Level > 0;
		}
		switch (_bindingName)
		{
		case "nothiddenbypager":
			_value = (CurrentSkill == null || CurrentSkill.ProgressionClass.MaxLevel <= maxEntriesWithoutPaging || listIndex < maxEntriesWithoutPaging - hiddenEntriesWithPaging).ToString();
			return true;
		case "hasentry":
			_value = flag.ToString();
			return true;
		case "color_bg":
			if (flag2)
			{
				_value = color_bg_bought;
			}
			else
			{
				_value = color_bg_locked;
			}
			return true;
		case "color_fg":
			if (flag2)
			{
				_value = color_lbl_available;
			}
			else
			{
				_value = color_lbl_locked;
			}
			return true;
		case "level":
			if (perk != null)
			{
				_value = (completionReward ? "" : volume.ToString());
			}
			else
			{
				_value = "";
			}
			return true;
		case "text":
		{
			effectsStringBuilder.Length = 0;
			if (perk == null)
			{
				_value = "";
			}
			int num = 1;
			if (flag && perk.ProgressionClass != null)
			{
				if (!string.IsNullOrEmpty(perk.ProgressionClass.DescKey))
				{
					_value = Localization.Get(perk.ProgressionClass.DescKey);
					return true;
				}
				if (perk.ProgressionClass.Effects != null && perk.ProgressionClass.Effects.EffectGroups != null)
				{
					foreach (MinEffectGroup effectGroup in perk.ProgressionClass.Effects.EffectGroups)
					{
						if (effectGroup.EffectDescriptions != null)
						{
							for (int i = 0; i < effectGroup.EffectDescriptions.Count; i++)
							{
								if (num >= effectGroup.EffectDescriptions[i].MinLevel && num <= effectGroup.EffectDescriptions[i].MaxLevel)
								{
									_value = effectGroup.EffectDescriptions[i].Description;
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
							float num2 = _base_value;
							passiveEffect.ModifyValue(entityPlayer, num, ref _base_value, ref _perc_value, passiveEffect.Tags);
							if (_base_value != num2 || _perc_value != 1f)
							{
								if (effectsStringBuilder.Length > 0)
								{
									effectsStringBuilder.Append(", ");
								}
								if (_base_value == num2)
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
			}
			_value = effectsStringBuilder.ToString();
			return true;
		}
		case "buyvisible":
			_value = flag.ToString();
			return true;
		case "buyicon":
			if (flag2)
			{
				_value = "ui_game_symbol_check";
			}
			else
			{
				_value = "ui_game_symbol_lock";
			}
			return true;
		case "iscomplete":
			_value = completionReward.ToString();
			return true;
		case "buycolor":
			if (flag2)
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

	public override void Init()
	{
		base.Init();
		viewComponent.IsNavigatable = true;
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
