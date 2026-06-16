using System.Collections.Generic;
using Audio;
using GameEvent.GameEventHelpers;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityHomerunGoal : Entity
{
	public enum Direction
	{
		YPositive,
		XPositive,
		XNegative,
		ZPositive,
		ZNegative,
		Max
	}

	public HomerunData Owner;

	public bool ReadyForDelete;

	public int ScoreAdded = 1;

	public float Size = 2f;

	public Vector3 StartPosition;

	public float TimeRemaining = 20f;

	public bool IsMoving = true;

	public Direction direction;

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEntityStatic()
	{
		return true;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		World world = GameManager.Instance.World;
		if (world == null)
		{
			ReadyForDelete = true;
			return;
		}
		if (Owner == null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				world.RemoveEntity(entityId, EnumRemoveEntityReason.Killed);
			}
			return;
		}
		TimeRemaining -= Time.deltaTime;
		if (TimeRemaining <= 0f)
		{
			ReadyForDelete = true;
			return;
		}
		if (IsMoving)
		{
			switch (direction)
			{
			case Direction.YPositive:
				SetPosition(StartPosition + Vector3.up * Mathf.PingPong(Time.time, 2f) * 2f);
				break;
			case Direction.XPositive:
				SetPosition(StartPosition + Vector3.right * Mathf.PingPong(Time.time, 2f) * 2f);
				break;
			case Direction.XNegative:
				SetPosition(StartPosition + Vector3.left * Mathf.PingPong(Time.time, 2f) * 2f);
				break;
			case Direction.ZPositive:
				SetPosition(StartPosition + Vector3.forward * Mathf.PingPong(Time.time, 2f) * 2f);
				break;
			case Direction.ZNegative:
				SetPosition(StartPosition + Vector3.back * Mathf.PingPong(Time.time, 2f) * 2f);
				break;
			}
		}
		if (Vector3.Distance(position, Owner.Player.position) > 50f)
		{
			ReadyForDelete = true;
			return;
		}
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Size));
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			if (entitiesInBounds[i] is EntityAlive entityAlive && entityAlive != null && entityAlive.IsAlive() && !(entityAlive is EntityPlayer) && entityAlive.emodel != null && entityAlive.emodel.transform != null && entityAlive.emodel.IsRagdollActive)
			{
				float lightBrightness = world.GetLightBrightness(entityAlive.GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("twitch_fireworks", entityAlive.position, lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityAlive.entityId, _forceCreation: false, _worldSpawn: true);
				Manager.BroadcastPlayByLocalPlayer(entityAlive.position, "twitch_baseball_balloon_pop");
				entityAlive.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
				world.RemoveEntity(entityAlive.entityId, EnumRemoveEntityReason.Killed);
				if (!ReadyForDelete)
				{
					Owner.Score += ScoreAdded;
					ReadyForDelete = true;
				}
				Owner.AddScoreDisplay(position);
				break;
			}
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass obj = EntityClass.list[entityClass];
		obj.Properties.ParseInt("ScoreAdded", ref ScoreAdded);
		obj.Properties.ParseFloat("Size", ref Size);
		obj.Properties.ParseBool("IsMoving", ref IsMoving);
	}
}
