using System.Collections.Generic;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupEntry : XUiController
{
	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	public XUiC_ChallengeEntryList ChallengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Challenge> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroupEntry entry;

	public XUiC_ChallengeGroupList Owner;

	public ChallengeGroupEntry Entry
	{
		get
		{
			return entry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			entry = value;
			group = ((entry != null) ? entry.ChallengeGroup : null);
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		ChallengeList = GetChildByType<XUiC_ChallengeEntryList>();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = group != null;
		switch (bindingName)
		{
		case "groupname":
			value = "";
			if (flag)
			{
				value = group.Title;
			}
			return true;
		case "groupreward":
			value = (flag ? group.RewardText : "");
			return true;
		case "groupobjective":
			value = (flag ? group.ObjectiveText : "");
			return true;
		case "resetday":
			if (flag)
			{
				value = ((group.DayReset == -1) ? "" : entry.LastUpdateDay.ToString());
			}
			else
			{
				value = "";
			}
			return true;
		case "hasreset":
			value = (flag ? (group.DayReset != -1).ToString() : "false");
			return true;
		case "hasentry":
			value = (flag ? "true" : "false");
			return true;
		default:
			return false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (group != null && group.UIDirty)
		{
			IsDirty = true;
			group.UIDirty = false;
		}
		if (IsDirty)
		{
			currentItems = player.challengeJournal.Challenges.Where([PublicizedFrom(EAccessModifier.Private)] (Challenge item) => item.ChallengeGroup == group).ToList();
			if (ChallengeList != null)
			{
				ChallengeList.Owner = this;
				ChallengeList.SetChallengeEntryList(currentItems);
			}
			RefreshBindings();
			IsDirty = false;
		}
	}

	public void Select()
	{
		Owner.SelectedGroup = this;
	}

	public void UnSelect()
	{
		ChallengeList.UnSelect();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		IsDirty = true;
	}

	public void Refresh()
	{
		IsDirty = true;
	}
}
