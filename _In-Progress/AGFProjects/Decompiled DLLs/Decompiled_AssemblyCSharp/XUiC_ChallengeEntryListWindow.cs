using System.Collections.Generic;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryListWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupList challengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeGroupList> challengeGroupList = new List<XUiC_ChallengeGroupList>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayKey = "";

	public override void Init()
	{
		base.Init();
		categoryList = GetChildByType<XUiC_CategoryList>();
		XUiC_ChallengeGroupList[] childrenByType = GetChildrenByType<XUiC_ChallengeGroupList>();
		if (childrenByType != null)
		{
			foreach (XUiC_ChallengeGroupList xUiC_ChallengeGroupList in childrenByType)
			{
				xUiC_ChallengeGroupList.ChallengeEntryListWindow = this;
				xUiC_ChallengeGroupList.CategoryList = categoryList;
				challengeGroupList.Add(xUiC_ChallengeGroupList);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupList GetListFromKey(string key)
	{
		XUiC_ChallengeGroupList result = null;
		for (int i = 0; i < challengeGroupList.Count; i++)
		{
			XUiC_ChallengeGroupList xUiC_ChallengeGroupList = challengeGroupList[i];
			if (xUiC_ChallengeGroupList.DisplayKey.EqualsCaseInsensitive(key))
			{
				result = xUiC_ChallengeGroupList;
				xUiC_ChallengeGroupList.ViewComponent.IsVisible = true;
			}
			else
			{
				xUiC_ChallengeGroupList.ViewComponent.IsVisible = false;
			}
		}
		return result;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			challengeList = GetListFromKey(displayKey);
			challengeList.SetChallengeGroupEntryList(player.challengeJournal.ChallengeGroups, categoryChange);
			IsDirty = false;
			categoryChange = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		if (categoryList != null)
		{
			categoryList.SetupCategoriesBasedOnChallengeCategories(ChallengeCategory.s_ChallengeCategories.Values.ToList());
		}
		if (categoryList != null)
		{
			categoryList.CategoryChanged += CategoryList_CategoryChanged;
		}
		RefreshBindings();
		if (categoryList != null && (categoryList.CurrentCategory == null || categoryList.CurrentCategory.SpriteName == ""))
		{
			categoryList.SetCategoryToFirst();
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		if (categoryList != null)
		{
			categoryList.CategoryChanged -= CategoryList_CategoryChanged;
		}
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		displayKey = ChallengeCategory.s_ChallengeCategories[_categoryEntry.CategoryName].DisplayKey;
		IsDirty = true;
		categoryChange = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSelectedByUnlockData(RecipeUnlockData unlockData)
	{
		string category = "";
		switch (unlockData.UnlockType)
		{
		case RecipeUnlockData.UnlockTypes.Challenge:
			displayKey = ChallengeCategory.s_ChallengeCategories[unlockData.Challenge.ChallengeGroup.Category].DisplayKey;
			category = unlockData.Challenge.ChallengeGroup.Category;
			break;
		case RecipeUnlockData.UnlockTypes.ChallengeGroup:
			displayKey = ChallengeCategory.s_ChallengeCategories[unlockData.ChallengeGroup.Category].DisplayKey;
			category = unlockData.ChallengeGroup.Category;
			break;
		}
		if (categoryList != null)
		{
			categoryList.CategoryChanged -= CategoryList_CategoryChanged;
			categoryList.SetCategory(category);
			categoryList.CategoryChanged += CategoryList_CategoryChanged;
		}
		IsDirty = true;
		categoryChange = true;
	}
}
