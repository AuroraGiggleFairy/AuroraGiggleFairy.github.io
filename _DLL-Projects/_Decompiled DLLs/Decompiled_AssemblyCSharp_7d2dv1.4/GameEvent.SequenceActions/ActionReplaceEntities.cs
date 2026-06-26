using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionReplaceEntities : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string entityNames = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool singleChoice;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEntityNames = "entity_names";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSingleChoice = "single_choice";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAttackTarget = "attack_target";

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<int> entityIDs = new List<int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntityIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Entity> newList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool attackTarget = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		string[] array = entityNames.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
			{
				if (item.Value.entityClassName == array[i])
				{
					entityIDs.Add(item.Key);
					if (entityIDs.Count == array.Length)
					{
						break;
					}
				}
			}
		}
		if (singleChoice && selectedEntityIndex == -1)
		{
			selectedEntityIndex = Random.Range(0, entityIDs.Count);
		}
	}

	public override void StartTargetAction()
	{
		newList = new List<Entity>();
	}

	public override void EndTargetAction()
	{
		if (targetGroup != "")
		{
			base.Owner.AddEntitiesToGroup(targetGroup, newList, twitchNegative: false);
		}
	}

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		World world = GameManager.Instance.World;
		if (target != null && !(target is EntityPlayer))
		{
			int num = ((selectedEntityIndex == -1) ? Random.Range(0, entityIDs.Count) : selectedEntityIndex);
			Entity entity = EntityFactory.CreateEntity(entityIDs[num], target.position, target.rotation, (base.Owner.Target != null) ? base.Owner.Target.entityId : (-1), base.Owner.ExtraData);
			entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
			world.SpawnEntityInWorld(entity);
			newList.Add(entity);
			if (attackTarget && entity is EntityAlive entityAlive && base.Owner.Target is EntityAlive target2)
			{
				GameEventManager.Current.RegisterSpawnedEntity(entityAlive, target2, base.Owner.Requester, base.Owner);
				entityAlive.SetAttackTarget(target2, 12000);
				entityAlive.aiManager.SetTargetOnlyPlayers(100f);
				if (base.Owner.Requester != null)
				{
					GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence == null) ? base.Owner : base.Owner.OwnerSequence);
					if (base.Owner.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntitySpawned(gameEventActionSequence.Name, entity.entityId, gameEventActionSequence.Tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameEventActionSequence.Name, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData, gameEventActionSequence.Tag, NetPackageGameEventResponse.ResponseTypes.EntitySpawned, entity.entityId), _onlyClientsAttachedToAnEntity: false, gameEventActionSequence.Requester.entityId);
					}
				}
			}
			HandleRemoveData(target);
			GameManager.Instance.StartCoroutine(removeLater(target));
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRemoveData(Entity ent)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator removeLater(Entity e)
	{
		yield return new WaitForSeconds(0.25f);
		if (e is EntityVehicle)
		{
			(e as EntityVehicle).Kill();
		}
		GameManager.Instance.World.RemoveEntity(e.entityId, EnumRemoveEntityReason.Killed);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropEntityNames))
		{
			entityNames = properties.Values[PropEntityNames];
		}
		if (properties.Values.ContainsKey(PropSingleChoice))
		{
			singleChoice = StringParsers.ParseBool(properties.Values[PropSingleChoice]);
		}
		properties.ParseBool(PropAttackTarget, ref attackTarget);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionReplaceEntities
		{
			entityNames = entityNames,
			entityIDs = entityIDs,
			singleChoice = singleChoice,
			targetGroup = targetGroup,
			selectedEntityIndex = selectedEntityIndex,
			attackTarget = attackTarget
		};
	}
}
