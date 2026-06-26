using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRespawnEntities : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string addToGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] AddToGroups;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string respawnSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAddToGroup = "add_to_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsMulti = "is_multi";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRespawnSound = "respawn_sound";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> entityList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float checkTime = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		AddToGroups = addToGroup.Split(',');
	}

	public override ActionCompleteStates OnPerformAction()
	{
		if (entityList == null)
		{
			entityList = new List<EntityAlive>();
			List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
			if (entityGroup == null)
			{
				Debug.LogWarning("ActionReviveEntities: Target Group " + targetGroup + " Does not exist!");
				return ActionCompleteStates.InCompleteRefund;
			}
			for (int i = 0; i < entityGroup.Count; i++)
			{
				EntityAlive entityAlive = entityGroup[i] as EntityAlive;
				if (entityAlive != null)
				{
					entityList.Add(entityAlive);
				}
			}
		}
		else if (entityList.Count > 0)
		{
			checkTime -= Time.deltaTime;
			if (!(checkTime <= 0f))
			{
				return ActionCompleteStates.Complete;
			}
			World world = GameManager.Instance.World;
			for (int j = 0; j < entityList.Count; j++)
			{
				if (!(entityList[j] != null) || entityList[j].IsAlive())
				{
					continue;
				}
				Entity entity = entityList[j];
				Entity entity2 = EntityFactory.CreateEntity(entityList[j].entityClass, entity.position, entity.rotation, base.Owner.Target.entityId, base.Owner.ExtraData);
				entity2.SetSpawnerSource(EnumSpawnerSource.Dynamic);
				world.SpawnEntityInWorld(entity2);
				world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);
				EntityAlive entityAlive2 = entity2 as EntityAlive;
				GameEventManager.Current.RegisterSpawnedEntity(entity2 as EntityAlive, base.Owner.Target, base.Owner.Requester, base.Owner);
				if (base.Owner.Requester != null)
				{
					if (base.Owner.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntitySpawned(base.Owner.Name, entityAlive2.entityId, base.Owner.Tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, base.Owner.Target.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner, entityAlive2.entityId), _onlyClientsAttachedToAnEntity: false, base.Owner.Requester.entityId);
					}
				}
				if (respawnSound != "")
				{
					Manager.BroadcastPlayByLocalPlayer(entity.position, respawnSound);
				}
				if (entity2 != null && AddToGroups != null)
				{
					for (int k = 0; k < AddToGroups.Length; k++)
					{
						if (AddToGroups[k] != "")
						{
							base.Owner.AddEntityToGroup(AddToGroups[k], entity2);
						}
					}
				}
				entityList.RemoveAt(j);
				return ActionCompleteStates.InComplete;
			}
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		entityList = null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropAddToGroup, ref addToGroup);
		properties.ParseString(PropTargetGroup, ref targetGroup);
		properties.ParseString(PropRespawnSound, ref respawnSound);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRespawnEntities
		{
			targetGroup = targetGroup,
			addToGroup = addToGroup,
			respawnSound = respawnSound
		};
	}
}
