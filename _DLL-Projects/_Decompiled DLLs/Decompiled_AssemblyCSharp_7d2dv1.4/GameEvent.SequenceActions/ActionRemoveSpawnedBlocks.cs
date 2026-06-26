using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveSpawnedBlocks : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool targetOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool despawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetOnly = "target_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDespawn = "despawn";

	public override ActionCompleteStates OnPerformAction()
	{
		for (int i = 0; i < GameEventManager.Current.blockEntries.Count; i++)
		{
			GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = GameEventManager.Current.blockEntries[i];
			if (!targetOnly || spawnedBlocksEntry.Target == base.Owner.Target)
			{
				spawnedBlocksEntry.TimeAlive = 1f;
				spawnedBlocksEntry.IsDespawn = despawn;
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropTargetOnly))
		{
			targetOnly = StringParsers.ParseBool(properties.Values[PropTargetOnly]);
		}
		properties.ParseBool(PropDespawn, ref despawn);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveSpawnedBlocks
		{
			targetOnly = targetOnly,
			despawn = despawn
		};
	}
}
