using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillPerkInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SkillPerkLevel> levelEntries = new List<XUiC_SkillPerkLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int skillsPerPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hoveredLevel = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillLevelFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat maxSkillLevelFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> buyCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + " " + Localization.Get("xuiSkillPoints"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> expCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + " " + Localization.Get("RewardExp_keyword"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f, bool _b) => _s + ": " + _f.ToCultureInvariantString("0.#") + (_b ? "%" : ""));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>([PublicizedFrom(EAccessModifier.Internal)] (string _s, float _f) => _s + ": " + _f.ToCultureInvariantString("0.#"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder effectsStringBuilder = new StringBuilder();

	public ProgressionValue CurrentSkill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.selectedSkill == null || !base.xui.selectedSkill.ProgressionClass.IsPerk)
			{
				return null;
			}
			return base.xui.selectedSkill;
		}
	}

	public int HoveredLevel
	{
		get
		{
			return hoveredLevel;
		}
		set
		{
			if (hoveredLevel != value)
			{
				hoveredLevel = value;
				RefreshBindings();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		GetChildrenByType(levelEntries);
		int num = 1;
		foreach (XUiC_SkillPerkLevel levelEntry in levelEntries)
		{
			levelEntry.ListIndex = num - 1;
			levelEntry.Level = num++;
			levelEntry.HiddenEntriesWithPaging = hiddenEntriesWithPaging;
			levelEntry.MaxEntriesWithoutPaging = levelEntries.Count;
			levelEntry.OnScroll += Entry_OnScroll;
			levelEntry.OnHover += Entry_OnHover;
			levelEntry.btnBuy.Controller.OnHover += Entry_OnHover;
		}
		actionItemList = GetChildByType<XUiC_ItemActionList>();
		skillsPerPage = levelEntries.Count - hiddenEntriesWithPaging;
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += Pager_OnPageChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Entry_OnHover(XUiController _sender, bool _isOver)
	{
		XUiC_SkillPerkLevel xUiC_SkillPerkLevel = _sender as XUiC_SkillPerkLevel;
		if (xUiC_SkillPerkLevel == null)
		{
			xUiC_SkillPerkLevel = _sender.Parent as XUiC_SkillPerkLevel;
		}
		if (_isOver && xUiC_SkillPerkLevel != null)
		{
			HoveredLevel = xUiC_SkillPerkLevel.Level;
		}
		else
		{
			HoveredLevel = -1;
		}
	}

	public void SkillChanged()
	{
		pager?.SetLastPageByElementsAndPageLength((CurrentSkill != null && CurrentSkill.ProgressionClass.MaxLevel > levelEntries.Count) ? (CurrentSkill.ProgressionClass.MaxLevel - 1) : 0, skillsPerPage);
		pager?.Reset();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		if (CurrentSkill != null && actionItemList != null)
		{
			actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Skill, this);
		}
		XUiC_SkillEntry entryForSkill = windowGroup.Controller.GetChildByType<XUiC_SkillList>().GetEntryForSkill(CurrentSkill);
		int num = (pager?.GetPage() ?? 0) * skillsPerPage + 1;
		foreach (XUiC_SkillPerkLevel levelEntry in levelEntries)
		{
			levelEntry.Level = num++;
			levelEntry.IsDirty = true;
			if (entryForSkill != null)
			{
				levelEntry.btnBuy.NavLeftTarget = entryForSkill.ViewComponent;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Pager_OnPageChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Entry_OnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (actionItemList != null)
		{
			actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Skill, this);
		}
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
		if (base.ViewComponent.UiTransform.gameObject.activeInHierarchy && CurrentSkill != null && !base.xui.playerUI.windowManager.IsInputActive() && ((PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed) || (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.DPad_Up.WasPressed)))
		{
			foreach (XUiC_SkillPerkLevel levelEntry in levelEntries)
			{
				if (levelEntry.CurrentSkill != null && levelEntry.Level == CurrentSkill.Level + 1)
				{
					levelEntry.btnBuy.Controller.Pressed(-1);
					break;
				}
			}
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

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "hidden_entries_with_paging")
		{
			hiddenEntriesWithPaging = StringParsers.ParseSInt32(_value);
			foreach (XUiC_SkillPerkLevel levelEntry in levelEntries)
			{
				if (levelEntry != null)
				{
					levelEntry.HiddenEntriesWithPaging = hiddenEntriesWithPaging;
				}
			}
			skillsPerPage = levelEntries.Count - hiddenEntriesWithPaging;
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (_bindingName)
		{
		case "alwaysfalse":
			_value = "false";
			return true;
		case "groupicon":
			_value = ((CurrentSkill != null) ? CurrentSkill.ProgressionClass.Icon : "ui_game_symbol_skills");
			return true;
		case "groupname":
			_value = ((CurrentSkill != null) ? Localization.Get(CurrentSkill.ProgressionClass.NameKey) : "Skill Info");
			return true;
		case "groupdescription":
			_value = ((CurrentSkill != null) ? Localization.Get(CurrentSkill.ProgressionClass.DescKey) : "");
			return true;
		case "detailsdescription":
			if (CurrentSkill != null && hoveredLevel != -1 && CurrentSkill.ProgressionClass.MaxLevel >= hoveredLevel)
			{
				foreach (MinEffectGroup effectGroup in CurrentSkill.ProgressionClass.Effects.EffectGroups)
				{
					if (effectGroup.EffectDescriptions == null)
					{
						continue;
					}
					for (int i = 0; i < effectGroup.EffectDescriptions.Count; i++)
					{
						if (hoveredLevel >= effectGroup.EffectDescriptions[i].MinLevel && hoveredLevel <= effectGroup.EffectDescriptions[i].MaxLevel)
						{
							_value = ((!string.IsNullOrEmpty(effectGroup.EffectDescriptions[i].LongDescription)) ? effectGroup.EffectDescriptions[i].LongDescription : effectGroup.EffectDescriptions[i].Description);
							return true;
						}
					}
				}
			}
			else
			{
				_value = "";
			}
			return true;
		case "skillLevel":
			_value = ((CurrentSkill != null) ? skillLevelFormatter.Format(CurrentSkill.GetCalculatedLevel(entityPlayer)) : "0");
			return true;
		case "maxSkillLevel":
			_value = ((CurrentSkill != null) ? maxSkillLevelFormatter.Format(ProgressionClass.GetCalculatedMaxLevel(entityPlayer, CurrentSkill)) : "0");
			return true;
		case "currentlevel":
			_value = Localization.Get("xuiSkillLevel");
			return true;
		case "buycost":
			_value = "-- PTS";
			if (CurrentSkill != null && CurrentSkill.CalculatedLevel(entityPlayer) < CurrentSkill.ProgressionClass.MaxLevel)
			{
				if (CurrentSkill.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
				{
					_value = buyCostFormatter.Format(CurrentSkill.ProgressionClass.CalculatedCostForLevel(CurrentSkill.CalculatedLevel(entityPlayer) + 1));
				}
				else
				{
					_value = expCostFormatter.Format((int)((1f - CurrentSkill.PercToNextLevel) * (float)CurrentSkill.ProgressionClass.CalculatedCostForLevel(CurrentSkill.CalculatedLevel(entityPlayer) + 1)));
				}
			}
			return true;
		case "showPaging":
			_value = (CurrentSkill != null && CurrentSkill.ProgressionClass.MaxLevel > levelEntries.Count).ToString();
			return true;
		default:
			return false;
		}
	}
}
