using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillAttributeInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SkillAttributeLevel> levelEntries = new List<XUiC_SkillAttributeLevel>();

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

	public ProgressionValue CurrentSkill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.selectedSkill == null || !base.xui.selectedSkill.ProgressionClass.IsAttribute)
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
		foreach (XUiC_SkillAttributeLevel levelEntry in levelEntries)
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
		XUiC_SkillAttributeLevel xUiC_SkillAttributeLevel = _sender as XUiC_SkillAttributeLevel;
		if (xUiC_SkillAttributeLevel == null)
		{
			xUiC_SkillAttributeLevel = _sender.Parent as XUiC_SkillAttributeLevel;
		}
		if (_isOver && xUiC_SkillAttributeLevel != null)
		{
			HoveredLevel = xUiC_SkillAttributeLevel.Level;
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
		foreach (XUiC_SkillAttributeLevel levelEntry in levelEntries)
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
			foreach (XUiC_SkillAttributeLevel levelEntry in levelEntries)
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
			foreach (XUiC_SkillAttributeLevel levelEntry in levelEntries)
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
			if (CurrentSkill != null && hoveredLevel != -1)
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
			if (CurrentSkill != null && CurrentSkill.Level < CurrentSkill.ProgressionClass.MaxLevel)
			{
				if (CurrentSkill.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
				{
					_value = buyCostFormatter.Format(CurrentSkill.ProgressionClass.CalculatedCostForLevel(CurrentSkill.Level + 1));
				}
				else
				{
					_value = expCostFormatter.Format((int)((1f - CurrentSkill.PercToNextLevel) * (float)CurrentSkill.ProgressionClass.CalculatedCostForLevel(CurrentSkill.Level + 1)));
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
