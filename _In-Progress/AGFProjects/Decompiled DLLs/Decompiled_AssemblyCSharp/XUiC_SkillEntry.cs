using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue currentSkill;

	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite groupIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ogIconPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ogNamePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i ogIconSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass.DisplayTypes displayType;

	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int, int> groupLevelFormatter = new CachedStringFormatter<int, int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i3, int _i1, int _i2) => (_i1 >= _i3) ? ((_i1 <= _i3) ? (_i1 + "/" + _i2) : ("[11cc11]" + _i1 + "[-]/" + _i2)) : ("[cc1111]" + _i1 + "[-]/" + _i2));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> groupPointCostFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => string.Format("{0} {1}", _i, (_i != 1) ? Localization.Get("xuiSkillPoints") : Localization.Get("xuiSkillPoint")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillPercentThisLevelFormatter = new CachedStringFormatterFloat();

	public ProgressionValue Skill
	{
		get
		{
			return currentSkill;
		}
		set
		{
			currentSkill = value;
			RefreshBindings(_forceAll: true);
			IsDirty = true;
			IsHovered = false;
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = value != null);
		}
	}

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			if (isSelected != value)
			{
				IsDirty = true;
				isSelected = value;
			}
		}
	}

	public ProgressionClass.DisplayTypes DisplayType
	{
		get
		{
			return displayType;
		}
		set
		{
			displayType = value;
		}
	}

	public override void Init()
	{
		base.Init();
		if (GetChildById("groupName") != null)
		{
			groupName = GetChildById("groupName").ViewComponent as XUiV_Label;
			if (groupName != null)
			{
				ogNamePos = groupName.UiTransform.localPosition;
			}
		}
		if (GetChildById("groupIcon") != null)
		{
			groupIcon = GetChildById("groupIcon").ViewComponent as XUiV_Sprite;
			if (groupIcon != null)
			{
				ogIconPos = groupIcon.UiTransform.localPosition;
				ogIconSize = groupIcon.Size;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (currentSkill != null && (currentSkill.ProgressionClass.Type != ProgressionType.Skill || DisplayType != ProgressionClass.DisplayTypes.Standard))
		{
			if (IsHovered != _isOver)
			{
				IsHovered = _isOver;
				RefreshBindings();
			}
		}
		else
		{
			IsHovered = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGroupLevel()
	{
		if (displayType == ProgressionClass.DisplayTypes.Standard && currentSkill != null && currentSkill.ProgressionClass.Type != ProgressionType.Skill)
		{
			return groupLevelFormatter.Format(currentSkill.Level, currentSkill.CalculatedLevel(base.xui.playerUI.entityPlayer), currentSkill.ProgressionClass.MaxLevel);
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGroupPointCost()
	{
		if (currentSkill != null)
		{
			if (currentSkill.ProgressionClass.IsAttribute || currentSkill.ProgressionClass.IsPerk)
			{
				if (currentSkill.ProgressionClass.CurrencyType != ProgressionCurrencyType.SP)
				{
					return "";
				}
				if (currentSkill.CostForNextLevel <= 0)
				{
					return "NA";
				}
				return groupPointCostFormatter.Format(currentSkill.CostForNextLevel);
			}
			if (currentSkill.ProgressionClass.IsBookGroup)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < currentSkill.ProgressionClass.Children.Count; i++)
				{
					num++;
					if (base.xui.playerUI.entityPlayer.Progression.GetProgressionValue(currentSkill.ProgressionClass.Children[i].Name).Level == 1)
					{
						num2++;
					}
				}
				num2 = Mathf.Min(num2, num - 1);
				return groupLevelFormatter.Format(num2, num2, num - 1);
			}
			if (currentSkill.ProgressionClass.IsCrafting)
			{
				return groupLevelFormatter.Format(currentSkill.Level, currentSkill.CalculatedLevel(base.xui.playerUI.entityPlayer), currentSkill.ProgressionClass.MaxLevel);
			}
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "groupicon":
			value = ((currentSkill == null) ? "" : ((currentSkill.ProgressionClass.Icon == null || currentSkill.ProgressionClass.Icon == "") ? "ui_game_filled_circle" : ((currentSkill.ProgressionClass.Icon != null) ? currentSkill.ProgressionClass.Icon : "ui_game_symbol_other")));
			return true;
		case "groupname":
			value = ((currentSkill != null) ? Localization.Get(currentSkill.ProgressionClass.NameKey) : "");
			return true;
		case "grouplevel":
			value = GetGroupLevel();
			return true;
		case "islocked":
			value = ((currentSkill != null) ? (currentSkill.CalculatedLevel(base.xui.playerUI.entityPlayer) > currentSkill.CalculatedMaxLevel(base.xui.playerUI.entityPlayer)).ToString() : "false");
			return true;
		case "isnotlocked":
			value = ((currentSkill != null) ? (currentSkill.CalculatedLevel(base.xui.playerUI.entityPlayer) <= currentSkill.CalculatedMaxLevel(base.xui.playerUI.entityPlayer)).ToString() : "true");
			return true;
		case "grouptypeicon":
			if (displayType == ProgressionClass.DisplayTypes.Standard)
			{
				value = ((currentSkill == null) ? "" : (currentSkill.ProgressionClass.IsPerk ? "ui_game_symbol_perk" : (currentSkill.ProgressionClass.IsSkill ? "ui_game_symbol_skills" : (currentSkill.ProgressionClass.IsAttribute ? "ui_game_symbol_hammer" : "ui_game_symbol_skills"))));
			}
			return true;
		case "grouppointcost":
			value = GetGroupPointCost();
			return true;
		case "canpurchase":
			if (displayType != ProgressionClass.DisplayTypes.Standard)
			{
				value = "true";
			}
			else
			{
				value = ((currentSkill != null && currentSkill.ProgressionClass.Type != ProgressionType.Skill) ? currentSkill.CanPurchase(base.xui.playerUI.entityPlayer, currentSkill.Level + 1).ToString() : "false");
			}
			return true;
		case "cannotpurchase":
			value = ((currentSkill != null && currentSkill.ProgressionClass.Type != ProgressionType.Skill && currentSkill.ProgressionClass.Type != ProgressionType.BookGroup && currentSkill.ProgressionClass.Type != ProgressionType.Crafting) ? (!currentSkill.CanPurchase(base.xui.playerUI.entityPlayer, currentSkill.Level + 1)).ToString() : "false");
			return true;
		case "requiredskill":
		{
			string text = "NA";
			if (currentSkill != null)
			{
				text = currentSkill.ProgressionClass.NameKey;
			}
			value = text;
			return true;
		}
		case "statuscolor":
			value = ((currentSkill == null) ? disabledColor : ((currentSkill.CalculatedMaxLevel(base.xui.playerUI.entityPlayer) == 0) ? disabledColor : enabledColor));
			return true;
		case "hasskill":
			value = (currentSkill != null).ToString();
			return true;
		case "ishighlighted":
			value = (IsHovered || IsSelected).ToString();
			return true;
		case "isnothighlighted":
			value = (!IsHovered && !IsSelected).ToString();
			return true;
		case "rowstatecolor":
			value = (IsSelected ? "255,255,255,255" : (IsHovered ? hoverColor : ((currentSkill != null && currentSkill.ProgressionClass.IsAttribute) ? "160,160,160,255" : rowColor)));
			return true;
		case "rowstatesprite":
			value = (IsSelected ? "ui_game_select_row" : "menu_empty");
			return true;
		case "skillpercentthislevel":
			value = ((currentSkill != null) ? skillPercentThisLevelFormatter.Format(currentSkill.PercToNextLevel) : "0");
			return true;
		case "skillpercentshouldshow":
			value = ((currentSkill != null) ? (currentSkill.ProgressionClass.Type == ProgressionType.Skill).ToString() : "false");
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "row_color":
			rowColor = value;
			return true;
		case "hover_color":
			hoverColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiEventManager.Instance.OnSkillExperienceAdded += Current_OnSkillExperienceAdded;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiEventManager.Instance.OnSkillExperienceAdded -= Current_OnSkillExperienceAdded;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_OnSkillExperienceAdded(ProgressionValue changedSkill, int newXP)
	{
		if (currentSkill == changedSkill)
		{
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		if (currentSkill != null)
		{
			if (displayType != ProgressionClass.DisplayTypes.Standard)
			{
				groupIcon.UiTransform.localPosition = ogIconPos;
				groupIcon.Size = ogIconSize;
			}
			else if (currentSkill.ProgressionClass.IsSkill)
			{
				Vector3 vector = new Vector3(32f, -4f, 0f);
				if (currentSkill.ProgressionClass.Parent.Hidden)
				{
					vector = new Vector3(0f, -4f, 0f);
				}
				groupIcon.UiTransform.localPosition = ogIconPos + vector;
				groupIcon.Size = ogIconSize;
				base.ViewComponent.IsNavigatable = false;
			}
			else if (currentSkill.ProgressionClass.IsPerk)
			{
				Vector3 vector2 = new Vector3(64f, -4f, 0f);
				if (currentSkill.ProgressionClass.Parent.Hidden && currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector2 = new Vector3(0f, -4f, 0f);
				}
				else if (currentSkill.ProgressionClass.Parent.Hidden || currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector2 = new Vector3(32f, -4f, 0f);
				}
				groupIcon.UiTransform.localPosition = ogIconPos + vector2;
				groupIcon.Size = ogIconSize;
				base.ViewComponent.IsNavigatable = true;
			}
			else
			{
				groupIcon.UiTransform.localPosition = ogIconPos;
				groupIcon.Size = ogIconSize;
				base.ViewComponent.IsNavigatable = true;
			}
		}
		else
		{
			groupIcon.UiTransform.localPosition = ogIconPos;
			groupIcon.Size = ogIconSize;
			base.ViewComponent.IsNavigatable = true;
		}
		if (currentSkill != null)
		{
			if (displayType != ProgressionClass.DisplayTypes.Standard)
			{
				groupName.UiTransform.localPosition = ogNamePos;
			}
			else if (currentSkill.ProgressionClass.IsSkill)
			{
				Vector3 vector3 = new Vector3(32f, -4f, 0f);
				if (currentSkill.ProgressionClass.Parent.Hidden)
				{
					vector3 = new Vector3(0f, -4f, 0f);
				}
				groupName.UiTransform.localPosition = ogNamePos + vector3;
				base.ViewComponent.IsNavigatable = false;
			}
			else if (currentSkill.ProgressionClass.IsPerk)
			{
				Vector3 vector4 = new Vector3(64f, 0f, 0f);
				if (currentSkill.ProgressionClass.Parent.Hidden && currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector4 = Vector3.zero;
				}
				else if (currentSkill.ProgressionClass.Parent.Hidden || currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector4 = new Vector3(32f, 0f, 0f);
				}
				groupName.UiTransform.localPosition = ogNamePos + vector4;
				base.ViewComponent.IsNavigatable = true;
			}
			else
			{
				groupName.UiTransform.localPosition = ogNamePos;
				base.ViewComponent.IsNavigatable = true;
			}
		}
		else
		{
			groupName.UiTransform.localPosition = ogNamePos;
			base.ViewComponent.IsNavigatable = true;
		}
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		skillList.SetSelected(this);
		((XUiC_SkillWindowGroup)windowGroup.Controller).IsDirty = true;
	}
}
