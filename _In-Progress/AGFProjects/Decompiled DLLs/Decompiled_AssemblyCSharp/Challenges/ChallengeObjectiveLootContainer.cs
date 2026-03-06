using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveLootContainer : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lastLocation = Vector3i.zero;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.LootContainer;

	public override string DescriptionText => Localization.Get("ObjectiveLootContainer_keyword");

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.ContainerOpened -= Current_ContainerOpened;
		QuestEventManager.Current.ContainerOpened += Current_ContainerOpened;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (!(containerLocation == lastLocation))
		{
			lastLocation = containerLocation;
			if (!tileEntity.bWasTouched && !CheckBaseRequirements())
			{
				base.Current++;
				CheckObjectiveComplete();
			}
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.ContainerOpened -= Current_ContainerOpened;
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveLootContainer();
	}
}
