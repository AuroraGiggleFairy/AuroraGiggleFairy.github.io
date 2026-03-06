using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillListWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string totalItems = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "Intellect";

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string pointsAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string skillsTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string booksTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string craftingTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int> totalSkillsFormatter = new CachedStringFormatter<string, int>([PublicizedFrom(EAccessModifier.Internal)] (string _s, int _i) => string.Format(_s, _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int> skillPointsAvailableFormatter = new CachedStringFormatter<string, int>([PublicizedFrom(EAccessModifier.Internal)] (string _s, int _i) => $"{_s} {_i}");

	public override void Init()
	{
		base.Init();
		totalItems = Localization.Get("lblTotalItems");
		pointsAvailable = Localization.Get("xuiPointsAvailable");
		skillsTitle = Localization.Get("xuiSkills");
		booksTitle = Localization.Get("lblCategoryBooks");
		craftingTitle = Localization.Get("xuiCrafting");
		skillList = GetChildByType<XUiC_SkillList>();
		XUiController childByType = GetChildByType<XUiC_CategoryList>();
		if (childByType != null)
		{
			categoryList = (XUiC_CategoryList)childByType;
			categoryList.CategoryChanged += CategoryList_CategoryChanged;
		}
		skillList.CategoryList = categoryList;
		skillList.SkillListWindow = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		categoryName = _categoryEntry.CategoryDisplayName;
		categoryIcon = _categoryEntry.SpriteName;
		count = skillList.GetActiveCount();
		RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoSpendPoints", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "totalskills":
			value = "";
			if (skillList != null)
			{
				value = totalSkillsFormatter.Format(totalItems, skillList.GetActiveCount());
			}
			return true;
		case "titlename":
			value = "";
			if (skillList != null)
			{
				switch (skillList.DisplayType)
				{
				case ProgressionClass.DisplayTypes.Standard:
					value = skillsTitle;
					break;
				case ProgressionClass.DisplayTypes.Book:
					value = booksTitle;
					break;
				case ProgressionClass.DisplayTypes.Crafting:
					value = craftingTitle;
					break;
				}
			}
			return true;
		case "categoryicon":
			value = categoryIcon;
			return true;
		case "isnormal":
			if (skillList != null)
			{
				value = (skillList.DisplayType == ProgressionClass.DisplayTypes.Standard).ToString();
			}
			return true;
		case "isbook":
			if (skillList != null)
			{
				value = (skillList.DisplayType == ProgressionClass.DisplayTypes.Book).ToString();
			}
			return true;
		case "iscrafting":
			if (skillList != null)
			{
				value = (skillList.DisplayType == ProgressionClass.DisplayTypes.Crafting).ToString();
			}
			return true;
		case "skillpointsavailable":
		{
			string v = pointsAvailable;
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			if (XUi.IsGameRunning() && entityPlayer != null)
			{
				value = skillPointsAvailableFormatter.Format(v, entityPlayer.Progression.SkillPoints);
			}
			return true;
		}
		case "paging_visible":
			if (skillList != null)
			{
				value = (skillList.PageCount > 0).ToString();
			}
			return true;
		default:
			return false;
		}
	}
}
