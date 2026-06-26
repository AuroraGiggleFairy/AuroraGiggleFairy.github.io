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

	public override void Init()
	{
		base.Init();
		categoryList = GetChildByType<XUiC_CategoryList>();
		challengeList = GetChildByType<XUiC_ChallengeGroupList>();
		if (challengeList != null)
		{
			challengeList.ChallengeEntryListWindow = this;
			challengeList.CategoryList = categoryList;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
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
		IsDirty = true;
		categoryChange = true;
	}
}
