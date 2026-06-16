using System.Collections.Generic;

namespace Challenges;

public class RequirementGroupPhase
{
	public List<BaseChallengeObjective> RequirementObjectiveList = new List<BaseChallengeObjective>();

	public bool IsComplete;

	public void AddChallengeObjective(BaseChallengeObjective obj)
	{
		RequirementObjectiveList.Add(obj);
	}

	public void AddHooks()
	{
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			for (int j = 0; j < RequirementObjectiveList.Count; j++)
			{
				RequirementObjectiveList[j].HandleAddHooks();
			}
		}
	}

	public bool HandleCheckStatus()
	{
		bool result = false;
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			if (!RequirementObjectiveList[i].HandleCheckStatus())
			{
				result = true;
			}
			RequirementObjectiveList[i].UpdateStatus();
		}
		return result;
	}

	public void HandleRemoveHooks()
	{
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			RequirementObjectiveList[i].HandleRemoveHooks();
		}
	}

	public void ResetComplete()
	{
		IsComplete = false;
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			RequirementObjectiveList[i].ResetComplete();
		}
	}

	public virtual void UpdateStatus()
	{
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			RequirementObjectiveList[i].UpdateStatus();
		}
	}

	public void Clone(RequirementGroupPhase phase)
	{
		for (int i = 0; i < phase.RequirementObjectiveList.Count; i++)
		{
			BaseChallengeObjective item = phase.RequirementObjectiveList[i].Clone();
			if (RequirementObjectiveList == null)
			{
				RequirementObjectiveList = new List<BaseChallengeObjective>();
			}
			RequirementObjectiveList.Add(item);
		}
	}

	public RequirementGroupPhase Clone()
	{
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			BaseChallengeObjective item = RequirementObjectiveList[i].Clone();
			if (RequirementObjectiveList == null)
			{
				RequirementObjectiveList = new List<BaseChallengeObjective>();
			}
			requirementGroupPhase.RequirementObjectiveList.Add(item);
		}
		return requirementGroupPhase;
	}

	public Recipe GetItemRecipe()
	{
		for (int i = 0; i < RequirementObjectiveList.Count; i++)
		{
			Recipe recipeItem = RequirementObjectiveList[i].GetRecipeItem();
			if (recipeItem != null)
			{
				return recipeItem;
			}
		}
		return null;
	}
}
