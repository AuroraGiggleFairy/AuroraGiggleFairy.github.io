using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> skills = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> currentSkills = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillEntry[] skillEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pagingControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass attributeClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	public XUiC_SkillEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.IsSelected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.IsSelected = true;
				base.xui.selectedSkill = selectedEntry.Skill;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList CategoryList { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillListWindow SkillListWindow { get; set; }

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			if (category != value)
			{
				category = value;
				if (Progression.ProgressionClasses.ContainsKey(category))
				{
					attributeClass = Progression.ProgressionClasses[category];
				}
			}
		}
	}

	public ProgressionClass.DisplayTypes DisplayType
	{
		get
		{
			if (attributeClass == null)
			{
				return ProgressionClass.DisplayTypes.Standard;
			}
			return attributeClass.DisplayType;
		}
	}

	public int PageCount
	{
		get
		{
			if (pagingControl != null)
			{
				return pagingControl.LastPageNumber;
			}
			return 1;
		}
	}

	public override void Init()
	{
		base.Init();
		Category = "";
		XUiController xUiController = parent.Parent;
		skillEntries = GetChildrenByType<XUiC_SkillEntry>();
		pagingControl = xUiController.GetChildByType<XUiC_Paging>();
		if (pagingControl != null)
		{
			pagingControl.OnPageChanged += PagingControl_OnPageChanged;
		}
		txtInput = xUiController.GetChildByType<XUiC_TextInput>();
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += TxtInput_OnChangeHandler;
			txtInput.Text = "";
		}
		for (int i = 0; i < skillEntries.Length; i++)
		{
			skillEntries[i].skillList = this;
			skillEntries[i].OnPress += XUiC_SkillEntry_OnPress;
			skillEntries[i].OnScroll += HandleOnScroll;
		}
	}

	public void SetSelectedByUnlockData(RecipeUnlockData unlockData)
	{
		switch (unlockData.UnlockType)
		{
		case RecipeUnlockData.UnlockTypes.Perk:
			selectName = unlockData.Perk.Name;
			if (unlockData.Perk.IsPerk)
			{
				CategoryList.SetCategory(unlockData.Perk.Parent.ParentName);
			}
			break;
		case RecipeUnlockData.UnlockTypes.Book:
			selectName = unlockData.Perk.ParentName;
			if (unlockData.Perk.IsBook)
			{
				CategoryList.SetCategory(unlockData.Perk.Parent.ParentName);
			}
			break;
		case RecipeUnlockData.UnlockTypes.Skill:
			selectName = unlockData.Perk.Name;
			if (unlockData.Perk.IsCrafting)
			{
				CategoryList.SetCategory(unlockData.Perk.Parent.Name);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public int GetActiveCount()
	{
		return currentSkills.Count;
	}

	public void SetFilterText(string _text)
	{
		if (txtInput != null)
		{
			txtInput.OnChangeHandler -= TxtInput_OnChangeHandler;
			filterText = _text;
			txtInput.Text = _text;
			txtInput.OnChangeHandler += TxtInput_OnChangeHandler;
		}
	}

	public void SelectFirstEntry()
	{
		SelectedEntry = skillEntries[0];
		SelectedEntry.SelectCursorElement(_withDelay: true);
	}

	public void SetSelected(XUiC_SkillEntry _entry)
	{
		SelectedEntry = _entry;
		selectName = "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SkillEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_SkillEntry xUiC_SkillEntry = (XUiC_SkillEntry)_sender;
		if (xUiC_SkillEntry.Skill != null && xUiC_SkillEntry.Skill.ProgressionClass.Type != ProgressionType.Skill)
		{
			SetSelected(xUiC_SkillEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		filterText = _text.Trim();
		if (filterText == "")
		{
			if (attributeClass.DisplayType != ProgressionClass.DisplayTypes.Book)
			{
				CategoryList.SetCategoryToFirst();
			}
			else
			{
				CategoryList.SetCategory(Category);
			}
		}
		else if (attributeClass == null || attributeClass.DisplayType != ProgressionClass.DisplayTypes.Book)
		{
			CategoryList.SetCategory("");
		}
		else
		{
			CategoryList.SetCategory(Category);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PagingControl_OnPageChanged()
	{
		listSkills(pagingControl?.GetPage() ?? 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			HandlePageDown(this, new EventArgs());
		}
		else
		{
			HandlePageUp(this, new EventArgs());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageDown(XUiController _sender, EventArgs _e)
	{
		pagingControl?.PageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageUp(XUiController _sender, EventArgs _e)
	{
		pagingControl?.PageUp();
	}

	public void RefreshSkillList()
	{
		pagingControl?.Reset();
		updateFilteredList();
		PagingControl_OnPageChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFilteredList()
	{
		currentSkills.Clear();
		string text = Category.Trim();
		bool flag = filterText != "";
		foreach (ProgressionValue skill in skills)
		{
			ProgressionClass progressionClass = skill?.ProgressionClass;
			if (progressionClass == null || !progressionClass.ValidDisplay(DisplayType) || progressionClass.Name == null || progressionClass.IsBook || progressionClass.Hidden || (flag && !progressionClass.NameKey.ContainsCaseInsensitive(filterText) && !Localization.Get(progressionClass.NameKey).ContainsCaseInsensitive(filterText)))
			{
				continue;
			}
			if (text == "" || text.EqualsCaseInsensitive(progressionClass.Name))
			{
				currentSkills.Add(skill);
				continue;
			}
			ProgressionClass progressionClass2 = progressionClass.Parent;
			if (progressionClass2 != null && progressionClass2 != progressionClass && (((progressionClass.IsSkill || progressionClass.IsCrafting) && text.EqualsCaseInsensitive(progressionClass.Parent.Name)) || ((progressionClass.IsPerk || progressionClass.IsBookGroup) && text.EqualsCaseInsensitive(progressionClass.Parent.Parent.Name))))
			{
				currentSkills.Add(skill);
			}
		}
		currentSkills.Sort(ProgressionClass.ListSortOrderComparer.Instance);
		if (filterText == "")
		{
			for (int i = 0; i < currentSkills.Count; i++)
			{
				if (currentSkills[i].ProgressionClass.IsAttribute)
				{
					for (; i % skillEntries.Length != 0; i++)
					{
						currentSkills.Insert(i, null);
					}
				}
			}
		}
		pagingControl?.SetLastPageByElementsAndPageLength(currentSkills.Count, skillEntries.Length);
		if (string.IsNullOrEmpty(selectName))
		{
			return;
		}
		for (int j = 0; j < currentSkills.Count; j++)
		{
			if (currentSkills[j].Name == selectName)
			{
				pagingControl?.SetPage(j / skillEntries.Length);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void listSkills(int page)
	{
		int num = skillEntries.Length * page;
		SelectedEntry = null;
		for (int i = 0; i < skillEntries.Length; i++)
		{
			int num2 = i + num;
			XUiC_SkillEntry xUiC_SkillEntry = skillEntries[i];
			if (num2 < currentSkills.Count && currentSkills[num2] != null && Progression.ProgressionClasses.ContainsKey(currentSkills[num2].Name))
			{
				xUiC_SkillEntry.Skill = currentSkills[num2];
				if (selectName != "")
				{
					if (xUiC_SkillEntry.Skill.ProgressionClass.Name == selectName)
					{
						SelectedEntry = xUiC_SkillEntry;
						xUiC_SkillEntry.IsSelected = true;
						xUiC_SkillEntry.RefreshBindings();
						((XUiC_SkillWindowGroup)base.WindowGroup.Controller).CurrentSkill = xUiC_SkillEntry.Skill;
					}
				}
				else if (SelectedEntry == null && i == 0)
				{
					SelectedEntry = xUiC_SkillEntry;
					base.xui.selectedSkill = xUiC_SkillEntry.Skill;
					((XUiC_SkillWindowGroup)base.WindowGroup.Controller).CurrentSkill = selectedEntry.Skill;
					((XUiC_SkillWindowGroup)base.WindowGroup.Controller).IsDirty = true;
				}
				if (base.xui.selectedSkill != null)
				{
					xUiC_SkillEntry.IsSelected = xUiC_SkillEntry.Skill.Name == base.xui.selectedSkill.Name;
					xUiC_SkillEntry.RefreshBindings();
				}
				else
				{
					xUiC_SkillEntry.IsSelected = false;
					xUiC_SkillEntry.RefreshBindings();
				}
				xUiC_SkillEntry.DisplayType = DisplayType;
				xUiC_SkillEntry.ViewComponent.Enabled = true;
				if (xUiC_SkillEntry.Skill.ProgressionClass.IsAttribute)
				{
					XUiView navRightTarget = base.xui.GetWindow("windowSkillAttributeInfo").Controller.GetChildById("0").ViewComponent;
					xUiC_SkillEntry.ViewComponent.NavRightTarget = navRightTarget;
				}
				else if (xUiC_SkillEntry.Skill.ProgressionClass.IsPerk)
				{
					XUiView navRightTarget2 = base.xui.GetWindow("windowSkillPerkInfo").Controller.GetChildById("0").ViewComponent;
					xUiC_SkillEntry.ViewComponent.NavRightTarget = navRightTarget2;
				}
				else if (xUiC_SkillEntry.Skill.ProgressionClass.IsBookGroup)
				{
					XUiView navRightTarget3 = base.xui.GetWindow("windowSkillBookInfo").Controller.GetChildById("0").ViewComponent;
					xUiC_SkillEntry.ViewComponent.NavRightTarget = navRightTarget3;
				}
				else if (xUiC_SkillEntry.Skill.ProgressionClass.IsCrafting)
				{
					XUiView navRightTarget4 = base.xui.GetWindow("windowSkillCraftingInfo").Controller.GetChildById("0").ViewComponent;
					xUiC_SkillEntry.ViewComponent.NavRightTarget = navRightTarget4;
				}
			}
			else
			{
				xUiC_SkillEntry.Skill = null;
				xUiC_SkillEntry.IsSelected = false;
				xUiC_SkillEntry.ViewComponent.Enabled = false;
				xUiC_SkillEntry.DisplayType = ProgressionClass.DisplayTypes.Standard;
				xUiC_SkillEntry.ViewComponent.NavRightTarget = null;
				xUiC_SkillEntry.RefreshBindings();
			}
		}
		if (SelectedEntry == null)
		{
			SelectedEntry = skillEntries[0];
			SelectedEntry.IsSelected = true;
			SelectedEntry.RefreshBindings();
			((XUiC_SkillWindowGroup)base.WindowGroup.Controller).CurrentSkill = SelectedEntry.Skill;
			((XUiC_SkillWindowGroup)base.WindowGroup.Controller).IsDirty = true;
			selectName = "";
		}
		RefreshBindings();
		SkillListWindow.RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		skills.Clear();
		base.xui.playerUI.entityPlayer.Progression.GetDict().CopyValuesTo(skills);
		updateFilteredList();
		PagingControl_OnPageChanged();
	}

	public override void OnClose()
	{
		base.OnClose();
		selectName = "";
	}

	public XUiC_SkillEntry GetEntryForSkill(ProgressionValue _skill)
	{
		XUiC_SkillEntry[] array = skillEntries;
		foreach (XUiC_SkillEntry xUiC_SkillEntry in array)
		{
			if (xUiC_SkillEntry.Skill == _skill)
			{
				return xUiC_SkillEntry;
			}
		}
		return null;
	}
}
