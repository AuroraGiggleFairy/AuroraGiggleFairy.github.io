using System.Collections.Generic;
using Challenges;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeEntry> entryList = new List<XUiC_ChallengeEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_ChallengeEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Challenge> challengeList;

	public XUiC_ChallengeEntryListWindow ChallengeEntryListWindow;

	public XUiC_ChallengeGroupEntry Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_ChallengeWindowGroup journalWindowGroup;

	public static XUiC_ChallengeEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (value != null && !value.IsChallengeVisible)
			{
				Debug.LogWarning("Here it is!");
				return;
			}
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
			}
			journalWindowGroup.SetEntry(selectedEntry);
		}
	}

	public override void Init()
	{
		base.Init();
		journalWindowGroup = (XUiC_ChallengeWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_ChallengeEntry)
			{
				XUiC_ChallengeEntry xUiC_ChallengeEntry = (XUiC_ChallengeEntry)children[i];
				xUiC_ChallengeEntry.Owner = this;
				xUiC_ChallengeEntry.JournalUIHandler = journalWindowGroup;
				entryList.Add(xUiC_ChallengeEntry);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			ChallengeObjectiveChallengeComplete challengeObjectiveChallengeComplete = null;
			string b = "";
			string b2 = "";
			bool flag = Owner != null && Owner.Entry != null && Owner.Entry.ChallengeGroup.IsVisible(xui.playerUI.entityPlayer);
			if (xui.QuestTracker.TrackedChallenge != null)
			{
				challengeObjectiveChallengeComplete = xui.QuestTracker.TrackedChallenge.GetChallengeCompleteObjective();
				if (challengeObjectiveChallengeComplete != null && challengeObjectiveChallengeComplete.IsRedeemed)
				{
					if (challengeObjectiveChallengeComplete.IsGroup)
					{
						b2 = challengeObjectiveChallengeComplete.ChallengeName;
					}
					else
					{
						b = challengeObjectiveChallengeComplete.ChallengeName;
					}
				}
				else
				{
					challengeObjectiveChallengeComplete = null;
				}
			}
			for (int i = 0; i < entryList.Count; i++)
			{
				XUiC_ChallengeEntry xUiC_ChallengeEntry = entryList[i];
				if (xUiC_ChallengeEntry == null)
				{
					continue;
				}
				xUiC_ChallengeEntry.OnPress -= OnPressEntry;
				xUiC_ChallengeEntry.Selected = selectedEntry == xUiC_ChallengeEntry || (selectedEntry == null && xUiC_ChallengeEntry.Tracked);
				if (i < challengeList.Count)
				{
					xUiC_ChallengeEntry.Entry = challengeList[i];
					if (xUiC_ChallengeEntry.IsChallengeVisible)
					{
						xUiC_ChallengeEntry.OnPress += OnPressEntry;
						xUiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = true;
						xUiC_ChallengeEntry.ViewComponent.SoundPlayOnHover = true;
					}
					else
					{
						xUiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = false;
						xUiC_ChallengeEntry.ViewComponent.SoundPlayOnHover = false;
					}
					xUiC_ChallengeEntry.IsRedeemBlinking = false;
					if (xUiC_ChallengeEntry.Entry.ChallengeState == Challenge.ChallengeStates.Completed && challengeObjectiveChallengeComplete != null && (xUiC_ChallengeEntry.Entry.ChallengeClass.Name.EqualsCaseInsensitive(b) || xUiC_ChallengeEntry.Entry.ChallengeGroup.Name.EqualsCaseInsensitive(b2)))
					{
						xUiC_ChallengeEntry.IsRedeemBlinking = true;
					}
					if (!flag || selectedEntry != null)
					{
						continue;
					}
					if (xUiC_ChallengeEntry.Entry.IsTracked)
					{
						if (selectedEntry == null)
						{
							Owner.Select();
							SelectedEntry = xUiC_ChallengeEntry;
							if (selectedEntry != null)
							{
								journalWindowGroup.SetEntry(selectedEntry);
								xUiC_ChallengeEntry.SelectCursorElement(_withDelay: true);
							}
							else
							{
								xUiC_ChallengeEntry.Entry.IsTracked = false;
							}
						}
						else if (!xUiC_ChallengeEntry.IsChallengeVisible)
						{
							xUiC_ChallengeEntry.Tracked = false;
							SelectedEntry = null;
						}
					}
					else
					{
						Owner.Select();
						SelectedEntry = xUiC_ChallengeEntry;
						if (selectedEntry != null)
						{
							journalWindowGroup.SetEntry(selectedEntry);
							xUiC_ChallengeEntry.SelectCursorElement(_withDelay: true);
						}
					}
				}
				else
				{
					xUiC_ChallengeEntry.Entry = null;
					xUiC_ChallengeEntry.ViewComponent.SoundPlayOnClick = false;
				}
			}
			IsDirty = false;
		}
		base.Update(_dt);
	}

	public void MarkDirty()
	{
		IsDirty = true;
	}

	public void UnSelect()
	{
		SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		if (!(_sender is XUiC_ChallengeEntry xUiC_ChallengeEntry))
		{
			return;
		}
		Owner.Select();
		SelectedEntry = xUiC_ChallengeEntry;
		SelectedEntry.JournalUIHandler.SetEntry(SelectedEntry);
		if (InputUtils.ShiftKeyPressed)
		{
			Challenge entry = xUiC_ChallengeEntry.Entry;
			if (entry.IsActive && !entry.IsTracked)
			{
				entry.IsTracked = true;
				xui.QuestTracker.TrackedChallenge = entry;
			}
		}
		IsDirty = true;
	}

	public void SetChallengeEntryList(List<Challenge> newChallengeList)
	{
		challengeList = newChallengeList;
		IsDirty = true;
	}

	public void SetEntryByChallenge(Challenge newChallenge)
	{
		for (int i = 0; i < entryList.Count; i++)
		{
			XUiC_ChallengeEntry xUiC_ChallengeEntry = entryList[i];
			if (xUiC_ChallengeEntry != null && xUiC_ChallengeEntry.Entry == newChallenge)
			{
				SelectedEntry = xUiC_ChallengeEntry;
				break;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = xui.playerUI.entityPlayer;
	}

	public override void OnClose()
	{
		base.OnClose();
	}
}
