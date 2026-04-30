using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class Challenge
{
	public enum ChallengeStates : byte
	{
		Active,
		Completed,
		Redeemed
	}

	public static byte FileVersion = 1;

	public ChallengeStates ChallengeState;

	public bool IsTracked;

	public bool UIDirty;

	public ChallengeClass ChallengeClass;

	public ChallengeGroup ChallengeGroup;

	public BaseRequirementObjectiveGroup RequirementObjectiveGroup;

	public List<BaseChallengeObjective> ObjectiveList = new List<BaseChallengeObjective>();

	public ChallengeTrackingHandler TrackingHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needsPrerequisites;

	public ChallengeJournal Owner;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	public bool IsActive => ChallengeState == ChallengeStates.Active;

	public bool NeedsPreRequisites
	{
		get
		{
			return needsPrerequisites;
		}
		set
		{
			needsPrerequisites = value;
			if (this.OnChallengeStateChanged != null)
			{
				this.OnChallengeStateChanged(this);
			}
		}
	}

	public bool ReadyToComplete
	{
		get
		{
			if (ChallengeState != ChallengeStates.Completed)
			{
				if (ChallengeClass.RedeemAlways)
				{
					return ChallengeState == ChallengeStates.Active;
				}
				return false;
			}
			return true;
		}
	}

	public float FillAmount
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < ObjectiveList.Count; i++)
			{
				num += ObjectiveList[i].FillAmount;
			}
			return num / (float)ObjectiveList.Count;
		}
	}

	public int ActiveObjectives
	{
		get
		{
			if (!NeedsPreRequisites)
			{
				return ObjectiveList.Count;
			}
			return RequirementObjectiveGroup.Count;
		}
	}

	public bool NeedsUIUpdate
	{
		get
		{
			if (ChallengeState == ChallengeStates.Active)
			{
				return ChallengeClass.NeedsConstantUIUpdate;
			}
			return false;
		}
	}

	public event ChallengeStateChanged OnChallengeStateChanged;

	public void SetRequirementGroup(BaseRequirementObjectiveGroup requirementObjectiveGroup)
	{
		requirementObjectiveGroup.Owner = this;
		RequirementObjectiveGroup = requirementObjectiveGroup;
	}

	public virtual void Read(BinaryReader _br)
	{
		CurrentFileVersion = _br.ReadByte();
		string key = _br.ReadString();
		ChallengeState = (ChallengeStates)_br.ReadByte();
		byte currentVersion = _br.ReadByte();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ChallengeObjectiveType type = (ChallengeObjectiveType)_br.ReadByte();
			ObjectiveList.Add(BaseChallengeObjective.ReadObjective(currentVersion, type, _br));
		}
		if (ChallengeClass.s_Challenges.ContainsKey(key))
		{
			ChallengeClass = ChallengeClass.s_Challenges[key];
			ChallengeGroup = ChallengeClass.ChallengeGroup;
		}
	}

	public ChallengeObjectiveChallengeComplete GetChallengeCompleteObjective()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			if (ObjectiveList[i] is ChallengeObjectiveChallengeComplete result)
			{
				return result;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public Recipe GetRecipeFromRequirements()
	{
		if (RequirementObjectiveGroup != null)
		{
			return RequirementObjectiveGroup.GetItemRecipe();
		}
		return null;
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(FileVersion);
		_bw.Write(ChallengeClass.Name);
		_bw.Write((byte)ChallengeState);
		_bw.Write(BaseChallengeObjective.FileVersion);
		_bw.Write(ObjectiveList.Count);
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			ObjectiveList[i].WriteObjective(_bw);
		}
	}

	public bool ResetToChallengeClass()
	{
		if (ChallengeClass == null)
		{
			return false;
		}
		return ChallengeClass.ResetObjectives(this);
	}

	public List<BaseChallengeObjective> GetObjectiveList()
	{
		if (!NeedsPreRequisites)
		{
			return ObjectiveList;
		}
		return RequirementObjectiveGroup.CurrentObjectiveList;
	}

	public List<Recipe> CraftedRecipes()
	{
		List<Recipe> list = null;
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			if (ObjectiveList[i].Complete)
			{
				continue;
			}
			Recipe[] recipeItems = ObjectiveList[i].GetRecipeItems();
			if (recipeItems != null)
			{
				if (list == null)
				{
					list = new List<Recipe>();
				}
				list.AddRange(recipeItems);
			}
		}
		return list;
	}

	public void StartChallenge()
	{
		if (IsActive)
		{
			for (int i = 0; i < ObjectiveList.Count; i++)
			{
				ObjectiveList[i].HandleAddHooks();
			}
		}
	}

	public void EndChallenge(bool isCompleted)
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			ObjectiveList[i].HandleRemoveHooks();
		}
		if (RequirementObjectiveGroup != null)
		{
			RequirementObjectiveGroup.HandleRemoveHooks();
		}
		if (isCompleted)
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Player);
			if (uIForPlayer.xui.QuestTracker.TrackedChallenge == this)
			{
				uIForPlayer.xui.QuestTracker.TrackedChallenge = Owner.GetNextChallenge(this);
			}
		}
	}

	public void HandleComplete(bool showTooltip = true)
	{
		bool flag = false;
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			if (!ObjectiveList[i].Complete)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (IsTracked)
			{
				CheckPrerequisites();
			}
			return;
		}
		if (ChallengeState == ChallengeStates.Active)
		{
			ChallengeState = ChallengeStates.Completed;
		}
		EndChallenge(isCompleted: true);
		QuestEventManager.Current.ChallengeCompleted(ChallengeClass, isRedeemed: false);
		if (ChallengeClass.ChallengeGroup.IsVisible(Owner.Player) && showTooltip)
		{
			GameManager.ShowTooltip(Owner.Player, string.Format(Localization.Get("challengeMessageComplete"), ChallengeClass.Title), "", "ui_challenge_complete");
		}
	}

	public void Redeem()
	{
		GameEventManager.Current.HandleAction(ChallengeClass.RewardEvent, null, Owner.Player, twitchActivated: false);
		Owner.HandleChallengeRedeemed(this);
		Owner.HandleChallengeGroupComplete(ChallengeGroup);
	}

	public void HandleTrackingStarted()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			if (!ObjectiveList[i].Complete)
			{
				ObjectiveList[i].HandleTrackingStarted();
			}
		}
	}

	public void HandleTrackingEnded()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			ObjectiveList[i].HandleTrackingEnded();
		}
	}

	public ChallengeTrackingHandler GetTrackingHelper()
	{
		if (TrackingHandler == null)
		{
			TrackingHandler = new ChallengeTrackingHandler
			{
				Owner = this,
				LocalPlayer = Owner.Player
			};
		}
		return TrackingHandler;
	}

	public void AddTrackingEntry(TrackingEntry entry)
	{
		if (TrackingHandler == null)
		{
			TrackingHandler = new ChallengeTrackingHandler
			{
				Owner = this,
				LocalPlayer = Owner.Player
			};
		}
		TrackingHandler.AddTrackingEntry(entry);
	}

	public void RemoveTrackingEntry(TrackingEntry entry)
	{
		if (TrackingHandler != null)
		{
			TrackingHandler.RemoveTrackingEntry(entry);
		}
	}

	public Challenge Clone()
	{
		Challenge challenge = new Challenge();
		challenge.ChallengeClass = ChallengeClass;
		challenge.ChallengeState = ChallengeState;
		challenge.IsTracked = IsTracked;
		if (RequirementObjectiveGroup != null)
		{
			BaseRequirementObjectiveGroup requirementObjectiveGroup = RequirementObjectiveGroup;
			BaseRequirementObjectiveGroup baseRequirementObjectiveGroup = requirementObjectiveGroup.Clone();
			baseRequirementObjectiveGroup.Owner = this;
			baseRequirementObjectiveGroup.ClonePhases(requirementObjectiveGroup);
			challenge.RequirementObjectiveGroup = baseRequirementObjectiveGroup;
		}
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			BaseChallengeObjective baseChallengeObjective = ObjectiveList[i].Clone();
			baseChallengeObjective.CopyValues(ObjectiveList[i], ChallengeClass.ObjectiveList[i]);
			baseChallengeObjective.Owner = challenge;
			challenge.ObjectiveList.Add(baseChallengeObjective);
		}
		return challenge;
	}

	public void RemovePrerequisiteHooks()
	{
		if (RequirementObjectiveGroup != null)
		{
			RequirementObjectiveGroup.HandleRemoveHooks();
		}
	}

	public void CheckPrerequisites()
	{
		bool flag = false;
		if (RequirementObjectiveGroup != null && RequirementObjectiveGroup.HasPrerequisiteCondition())
		{
			if (RequirementObjectiveGroup.HandleCheckStatus())
			{
				flag = true;
				UIDirty = true;
			}
			RequirementObjectiveGroup.UpdateStatus();
		}
		if (NeedsPreRequisites != flag)
		{
			NeedsPreRequisites = flag;
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Player);
			if (uIForPlayer.xui.QuestTracker.TrackedChallenge == this)
			{
				uIForPlayer.xui.QuestTracker.HandleTrackedChallengeChanged();
			}
		}
	}

	public void AddPrerequisiteHooks()
	{
		if (RequirementObjectiveGroup != null)
		{
			RequirementObjectiveGroup.HandleAddHooks();
		}
		CheckPrerequisites();
	}

	public BaseChallengeObjective GetNavObjective()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			BaseChallengeObjective baseChallengeObjective = ObjectiveList[i];
			if (baseChallengeObjective.NavType != ChallengeClass.UINavTypes.None)
			{
				return baseChallengeObjective;
			}
		}
		return null;
	}

	public void CompleteChallenge(bool forceRedeem = false)
	{
		if (!IsActive && (!forceRedeem || ChallengeState != ChallengeStates.Completed))
		{
			return;
		}
		foreach (BaseChallengeObjective objective in ObjectiveList)
		{
			objective.CompleteObjective(handleComplete: false);
		}
		HandleComplete(showTooltip: false);
		if (forceRedeem)
		{
			ChallengeState = ChallengeStates.Redeemed;
			Redeem();
			QuestEventManager.Current.ChallengeCompleted(ChallengeClass, isRedeemed: true);
		}
	}
}
