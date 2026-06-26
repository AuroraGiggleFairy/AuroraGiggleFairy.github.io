using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class BaseRequirementObjectiveGroup
{
	public Challenge Owner;

	public List<RequirementGroupPhase> PhaseList = new List<RequirementGroupPhase>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentIndex;

	public bool NeedsPreRequisites;

	public int Count
	{
		get
		{
			if (PhaseList == null)
			{
				return 0;
			}
			return PhaseList[currentIndex].RequirementObjectiveList.Count;
		}
	}

	public List<BaseChallengeObjective> CurrentObjectiveList
	{
		get
		{
			if (PhaseList == null)
			{
				return null;
			}
			return PhaseList[currentIndex].RequirementObjectiveList;
		}
	}

	public virtual void CreateRequirements()
	{
	}

	public void HandleAddHooks()
	{
		if (PhaseList.Count == 0)
		{
			CreateRequirements();
		}
		for (int i = 0; i < PhaseList.Count; i++)
		{
			PhaseList[i].AddHooks();
		}
		CheckPrerequisites();
	}

	public void CheckPrerequisites()
	{
		if (PhaseList.Count == 0)
		{
			CreateRequirements();
		}
		NeedsPreRequisites = false;
		for (int i = 0; i < PhaseList.Count; i++)
		{
			if (PhaseList[i].HandleCheckStatus())
			{
				currentIndex = i;
				NeedsPreRequisites = true;
				break;
			}
		}
	}

	public void HandleRemoveHooks()
	{
		for (int i = 0; i < PhaseList.Count; i++)
		{
			PhaseList[i].HandleRemoveHooks();
		}
	}

	public virtual bool HasPrerequisiteCondition()
	{
		return false;
	}

	public void ResetObjectives()
	{
		if (PhaseList != null)
		{
			for (int i = 0; i < PhaseList.Count; i++)
			{
				PhaseList[i].ResetComplete();
			}
		}
	}

	public void ClonePhases(BaseRequirementObjectiveGroup group)
	{
		if (group.PhaseList != null)
		{
			for (int i = 0; i < group.PhaseList.Count; i++)
			{
				RequirementGroupPhase item = group.PhaseList[i].Clone();
				PhaseList.Add(item);
			}
		}
	}

	public virtual bool HandleCheckStatus()
	{
		if (PhaseList.Count == 0)
		{
			CreateRequirements();
		}
		ResetObjectives();
		for (int i = 0; i < PhaseList.Count; i++)
		{
			if (CheckPhaseStatus(i) && PhaseList[i].HandleCheckStatus())
			{
				currentIndex = i;
				NeedsPreRequisites = true;
				return true;
			}
			PhaseList[i].IsComplete = true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CheckPhaseStatus(int index)
	{
		return true;
	}

	public virtual void UpdateStatus()
	{
		for (int i = 0; i < PhaseList.Count; i++)
		{
			if (!PhaseList[i].IsComplete)
			{
				PhaseList[i].UpdateStatus();
			}
		}
	}

	public virtual BaseRequirementObjectiveGroup Clone()
	{
		return null;
	}

	public Recipe GetItemRecipe()
	{
		return PhaseList[currentIndex].GetItemRecipe();
	}
}
