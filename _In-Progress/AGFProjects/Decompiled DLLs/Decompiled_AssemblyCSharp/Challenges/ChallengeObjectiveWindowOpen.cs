using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveWindowOpen : BaseChallengeObjective
{
	public string WindowName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentOpenWindow = "";

	public RequirementObjectiveGroupWindowOpen Parent;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.WindowOpen;

	public override string DescriptionText => string.Format(Localization.Get("ObjectiveOpenWindow_keyword"), string.Format("[DECEA3]{0}[-]", Localization.Get("xui" + WindowName)));

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.WindowChanged -= Current_WindowChanged;
		QuestEventManager.Current.WindowChanged += Current_WindowChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WindowChanged(string _windowName)
	{
		if (!(_windowName == "windowpaging") && !CheckBaseRequirements())
		{
			currentOpenWindow = _windowName;
			HandleUpdatingCurrent();
			CheckObjectiveComplete();
			Parent.CheckPrerequisites();
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.WindowChanged -= Current_WindowChanged;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleUpdatingCurrent()
	{
		base.HandleUpdatingCurrent();
		if (Owner != null)
		{
			base.Current = ((currentOpenWindow == WindowName) ? 1 : 0);
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveWindowOpen
		{
			WindowName = WindowName
		};
	}

	public override void CompleteObjective(bool handleComplete = true)
	{
		base.Current = MaxCount;
		base.Complete = true;
		if (handleComplete)
		{
			Owner.HandleComplete();
		}
	}
}
