using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRespawnEntity : BaseAction
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
	public static string PropRespawnSound = "respawn_sound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDelay = "delay";

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldEntityClass = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay = 3f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		AddToGroups = addToGroup.Split(',');
	}

	public override ActionCompleteStates OnPerformAction()
	{
		if (oldEntityClass == -1 && base.Owner.Target != null && !(base.Owner.Target is EntityPlayer))
		{
			Entity target = base.Owner.Target;
			oldEntityClass = target.entityClass;
			oldEntityID = target.entityId;
			oldPosition = target.position;
			oldRotation = target.rotation;
		}
		if (delay <= 0f)
		{
			World world = GameManager.Instance.World;
			GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence == null) ? base.Owner : base.Owner.OwnerSequence);
			Entity entity = EntityFactory.CreateEntity(oldEntityClass, oldPosition, oldRotation, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData);
			if (entity == null)
			{
				return ActionCompleteStates.Complete;
			}
			entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
			world.SpawnEntityInWorld(entity);
			world.RemoveEntity(oldEntityID, EnumRemoveEntityReason.Killed);
			base.Owner.Target = entity;
			EntityAlive entityAlive = entity as EntityAlive;
			if (gameEventActionSequence.Target is EntityAlive entityAlive2)
			{
				GameEventManager.Current.RegisterSpawnedEntity(entityAlive, entityAlive2, gameEventActionSequence.Requester, gameEventActionSequence);
				entityAlive.SetAttackTarget(entityAlive2, 12000);
			}
			if (base.Owner.Requester != null)
			{
				if (gameEventActionSequence.Requester is EntityPlayerLocal)
				{
					GameEventManager.Current.HandleGameEntitySpawned(gameEventActionSequence.Name, entityAlive.entityId, gameEventActionSequence.Tag);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameEventActionSequence.Name, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData, gameEventActionSequence.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner, entityAlive.entityId), _onlyClientsAttachedToAnEntity: false, gameEventActionSequence.Requester.entityId);
				}
			}
			if (entity != null && AddToGroups != null)
			{
				for (int i = 0; i < AddToGroups.Length; i++)
				{
					if (AddToGroups[i] != "")
					{
						base.Owner.AddEntityToGroup(AddToGroups[i], entity);
					}
				}
			}
			if (respawnSound != "")
			{
				Manager.BroadcastPlayByLocalPlayer(oldPosition, respawnSound);
			}
			return ActionCompleteStates.Complete;
		}
		delay -= Time.deltaTime;
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTargetGroup, ref targetGroup);
		properties.ParseString(PropRespawnSound, ref respawnSound);
		properties.ParseString(PropAddToGroup, ref addToGroup);
		properties.ParseFloat(PropDelay, ref delay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRespawnEntity
		{
			targetGroup = targetGroup,
			addToGroup = addToGroup,
			AddToGroups = AddToGroups,
			respawnSound = respawnSound,
			delay = delay
		};
	}
}
