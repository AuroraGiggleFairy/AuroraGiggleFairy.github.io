using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillEntry[] skillEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillAttributeInfoWindow skillAttributeInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillSkillInfoWindow skillSkillInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillPerkInfoWindow skillPerkInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillBookInfoWindow skillBookInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillCraftingInfoWindow skillCraftingInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue currentSkill;

	public ProgressionValue CurrentSkill
	{
		get
		{
			return currentSkill;
		}
		set
		{
			currentSkill = value;
		}
	}

	public override void Init()
	{
		base.Init();
		skillList = GetChildByType<XUiC_SkillList>();
		skillAttributeInfoWindow = GetChildByType<XUiC_SkillAttributeInfoWindow>();
		skillSkillInfoWindow = GetChildByType<XUiC_SkillSkillInfoWindow>();
		skillPerkInfoWindow = GetChildByType<XUiC_SkillPerkInfoWindow>();
		skillBookInfoWindow = GetChildByType<XUiC_SkillBookInfoWindow>();
		skillCraftingInfoWindow = GetChildByType<XUiC_SkillCraftingInfoWindow>();
		XUiController[] childrenByType = GetChildrenByType<XUiC_SkillEntry>();
		XUiController[] array = childrenByType;
		skillEntries = new XUiC_SkillEntry[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			skillEntries[i] = (XUiC_SkillEntry)array[i];
			skillEntries[i].OnPress += XUiC_SkillEntry_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		skillList.Category = _categoryEntry.CategoryName;
		skillList.RefreshSkillList();
		skillList.SelectFirstEntry();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryClickChanged(XUiC_CategoryEntry _categoryEntry)
	{
		skillList.Category = _categoryEntry.CategoryName;
		skillList.SetFilterText("");
		skillList.RefreshSkillList();
		skillList.SelectFirstEntry();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SkillEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!IsDirty)
		{
			return;
		}
		CurrentSkill = base.xui.selectedSkill;
		skillAttributeInfoWindow.SkillChanged();
		skillSkillInfoWindow.IsDirty = true;
		skillPerkInfoWindow.SkillChanged();
		skillBookInfoWindow.SkillChanged();
		skillCraftingInfoWindow.SkillChanged();
		if (skillList.DisplayType == ProgressionClass.DisplayTypes.Book)
		{
			skillBookInfoWindow.ViewComponent.IsVisible = true;
		}
		else if (skillList.DisplayType == ProgressionClass.DisplayTypes.Crafting)
		{
			skillCraftingInfoWindow.ViewComponent.IsVisible = true;
		}
		else if (base.xui.selectedSkill != null)
		{
			if (base.xui.selectedSkill.ProgressionClass.IsAttribute)
			{
				skillAttributeInfoWindow.ViewComponent.IsVisible = true;
			}
			else if (base.xui.selectedSkill.ProgressionClass.IsSkill)
			{
				skillSkillInfoWindow.ViewComponent.IsVisible = true;
			}
			else
			{
				skillPerkInfoWindow.ViewComponent.IsVisible = true;
			}
		}
		IsDirty = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (categoryList == null)
		{
			XUiController childByType = GetChildByType<XUiC_CategoryList>();
			if (childByType != null)
			{
				categoryList = (XUiC_CategoryList)childByType;
				categoryList.SetupCategoriesByWorkstation("skills");
				categoryList.CategoryChanged += CategoryList_CategoryChanged;
				categoryList.CategoryClickChanged += CategoryList_CategoryClickChanged;
			}
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("skills");
		IsDirty = true;
		if (categoryList.CurrentCategory == null)
		{
			categoryList.SetCategoryToFirst();
		}
		skillList.Category = categoryList.CurrentCategory.CategoryName;
		skillList.RefreshSkillList();
		if (base.xui.selectedSkill == null)
		{
			skillList.SelectFirstEntry();
		}
		else
		{
			skillList.SelectedEntry.SelectCursorElement(_withDelay: true);
		}
		IsDirty = true;
	}
}
