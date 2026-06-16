using System.Collections.Generic;
using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeGroupEntry> entryList = new List<XUiC_ChallengeGroupEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupEntry selectedGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;

	public string DisplayKey = "";

	public XUiC_CategoryList CategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChallengeGroupEntry> challengeGroupList;

	public XUiC_ChallengeEntryListWindow ChallengeEntryListWindow;

	public XUiC_ChallengeGroupEntry SelectedGroup
	{
		get
		{
			return selectedGroup;
		}
		set
		{
			if (selectedGroup != null)
			{
				selectedGroup.UnSelect();
			}
			selectedGroup = value;
		}
	}

	public override void Init()
	{
		base.Init();
		_ = (XUiC_ChallengeWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_ChallengeGroupEntry)
			{
				XUiC_ChallengeGroupEntry xUiC_ChallengeGroupEntry = (XUiC_ChallengeGroupEntry)children[i];
				xUiC_ChallengeGroupEntry.Owner = this;
				entryList.Add(xUiC_ChallengeGroupEntry);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (CategoryList.CurrentCategory != null && IsDirty)
		{
			updateChallengeEntries();
			IsDirty = false;
			categoryChange = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChallengeEntries()
	{
		if (challengeGroupList == null)
		{
			return;
		}
		string categoryName = CategoryList.CurrentCategory.CategoryName;
		int num = 0;
		for (int i = 0; i < challengeGroupList.Count; i++)
		{
			XUiC_ChallengeGroupEntry xUiC_ChallengeGroupEntry = entryList[num];
			if (xUiC_ChallengeGroupEntry != null && challengeGroupList[i].ChallengeGroup.Category.EqualsCaseInsensitive(categoryName))
			{
				xUiC_ChallengeGroupEntry.Entry = challengeGroupList[i];
				xUiC_ChallengeGroupEntry.ViewComponent.SoundPlayOnClick = true;
				if (categoryChange)
				{
					XUiC_ChallengeEntryList.SelectedEntry = null;
				}
				num++;
			}
			if (num >= entryList.Count)
			{
				break;
			}
		}
		for (int j = num; j < entryList.Count; j++)
		{
			XUiC_ChallengeGroupEntry xUiC_ChallengeGroupEntry2 = entryList[j];
			xUiC_ChallengeGroupEntry2.Entry = null;
			xUiC_ChallengeGroupEntry2.ViewComponent.SoundPlayOnClick = false;
		}
	}

	public void SetChallengeGroupEntryList(List<ChallengeGroupEntry> newChallengeGroupList, bool newCategoryChange)
	{
		challengeGroupList = newChallengeGroupList;
		if (CategoryList != null && CategoryList.CurrentCategory == null)
		{
			CategoryList.SetCategoryToFirst();
		}
		categoryChange = newCategoryChange;
		if (xui.QuestTracker.TrackedChallenge != null)
		{
			ChallengeGroup challengeGroup = xui.QuestTracker.TrackedChallenge.ChallengeGroup;
			for (int i = 0; i < challengeGroupList.Count && challengeGroupList[i].ChallengeGroup != challengeGroup; i++)
			{
			}
		}
		IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "display_key")
		{
			DisplayKey = value;
			return true;
		}
		return base.ParseAttribute(name, value);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = xui.playerUI.entityPlayer;
	}
}
