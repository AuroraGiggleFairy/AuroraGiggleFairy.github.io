using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingListInfo : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName = "Craft_Icon_Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public string unlockedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoCategoryIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblUnlockedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	public string CategoryName
	{
		get
		{
			return categoryName;
		}
		set
		{
			isDirty |= categoryName != value;
			categoryName = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("windowicon");
		if (childById != null)
		{
			icoCategoryIcon = childById.ViewComponent as XUiV_Sprite;
		}
		XUiController childById2 = GetChildById("windowname");
		if (childById2 != null)
		{
			lblName = childById2.ViewComponent as XUiV_Label;
		}
		childById2 = GetChildById("unlockedcount");
		if (childById2 != null)
		{
			lblUnlockedCount = childById2.ViewComponent as XUiV_Label;
		}
		XUiController childById3 = GetChildById("categories");
		if (childById3 != null)
		{
			categoryList = (XUiC_CategoryList)childById3;
			categoryList.CategoryChanged += HandleCategoryChanged;
		}
		isDirty = true;
		IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		CategoryName = _categoryEntry.CategoryDisplayName;
		spriteName = _categoryEntry.SpriteName;
	}

	public override void Update(float _dt)
	{
		if (!windowGroup.isShowing)
		{
			return;
		}
		if (isDirty)
		{
			if (lblName != null)
			{
				lblName.Text = categoryName;
			}
			if (icoCategoryIcon != null)
			{
				icoCategoryIcon.SpriteName = spriteName;
			}
			if (lblUnlockedCount != null)
			{
				lblUnlockedCount.Text = $"{CraftingManager.GetUnlockedRecipeCount()}/{CraftingManager.GetLockedRecipeCount()}";
			}
			isDirty = false;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
		IsDormant = false;
		CraftingManager.RecipeUnlocked += CraftingManager_RecipeUnlocked;
	}

	public override void OnClose()
	{
		base.OnClose();
		IsDormant = true;
		CraftingManager.RecipeUnlocked -= CraftingManager_RecipeUnlocked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CraftingManager_RecipeUnlocked(string recipeName)
	{
		isDirty = true;
	}
}
