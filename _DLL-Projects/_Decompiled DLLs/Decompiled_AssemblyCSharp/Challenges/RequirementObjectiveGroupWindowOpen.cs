using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class RequirementObjectiveGroupWindowOpen : BaseRequirementObjectiveGroup
{
	public string WindowOpen = "";

	public RequirementObjectiveGroupWindowOpen(string windowOpen)
	{
		WindowOpen = windowOpen;
	}

	public override void CreateRequirements()
	{
		if (PhaseList == null)
		{
			PhaseList = new List<RequirementGroupPhase>();
		}
		RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
		ChallengeObjectiveWindowOpen challengeObjectiveWindowOpen = new ChallengeObjectiveWindowOpen();
		challengeObjectiveWindowOpen.WindowName = WindowOpen;
		challengeObjectiveWindowOpen.Parent = this;
		challengeObjectiveWindowOpen.Owner = Owner;
		challengeObjectiveWindowOpen.IsRequirement = true;
		challengeObjectiveWindowOpen.Init();
		requirementGroupPhase.AddChallengeObjective(challengeObjectiveWindowOpen);
		PhaseList.Add(requirementGroupPhase);
	}

	public override bool HasPrerequisiteCondition()
	{
		_ = Owner.Owner.Player;
		GUIWindow window = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).windowManager.GetWindow(WindowOpen);
		if (window != null)
		{
			return !window.isShowing;
		}
		return false;
	}

	public override BaseRequirementObjectiveGroup Clone()
	{
		return new RequirementObjectiveGroupWindowOpen(WindowOpen);
	}
}
