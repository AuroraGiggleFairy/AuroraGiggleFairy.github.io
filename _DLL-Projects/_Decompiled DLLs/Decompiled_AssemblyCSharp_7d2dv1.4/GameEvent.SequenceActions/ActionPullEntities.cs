using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPullEntities : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float minDistance = 7f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 9f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string pullSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinDistance = "min_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPullSound = "pull_sound";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityPullList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	public override bool CanPerform(Entity player)
	{
		if (!GameManager.Instance.World.CanPlaceBlockAt(new Vector3i(player.position), null))
		{
			return false;
		}
		return true;
	}

	public override ActionCompleteStates OnPerformAction()
	{
		if (targetGroup != "")
		{
			if (entityPullList == null)
			{
				entityPullList = new List<Entity>();
				List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
				if (entityGroup != null)
				{
					entityPullList.AddRange(entityGroup);
					index = 0;
				}
				if (entityPullList.Count == 0)
				{
					return ActionCompleteStates.InCompleteRefund;
				}
			}
			else
			{
				Entity entity = entityPullList[index];
				if (entity.IsDead() || entity.IsDespawned)
				{
					index++;
					if (index >= entityPullList.Count)
					{
						return ActionCompleteStates.Complete;
					}
				}
				Vector3 newPoint = Vector3.zero;
				if (ActionBaseSpawn.FindValidPosition(out newPoint, base.Owner.Target, minDistance, maxDistance, spawnInSafe: false))
				{
					entity.SetPosition(newPoint);
					if (entity is EntityAlive entityAlive && base.Owner.Target is EntityAlive attackTarget)
					{
						entityAlive.SetAttackTarget(attackTarget, 12000);
					}
					if (pullSound != "")
					{
						Manager.BroadcastPlayByLocalPlayer(newPoint, pullSound);
					}
					index++;
				}
				if (index >= entityPullList.Count)
				{
					return ActionCompleteStates.Complete;
				}
			}
			return ActionCompleteStates.InComplete;
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTargetGroup, ref targetGroup);
		properties.ParseString(PropPullSound, ref pullSound);
		properties.ParseFloat(PropMinDistance, ref minDistance);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPullEntities
		{
			targetGroup = targetGroup,
			pullSound = pullSound,
			minDistance = minDistance,
			maxDistance = maxDistance
		};
	}
}
