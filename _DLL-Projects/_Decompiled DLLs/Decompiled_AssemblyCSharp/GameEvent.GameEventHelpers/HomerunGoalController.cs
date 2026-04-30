using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace GameEvent.GameEventHelpers;

public class HomerunGoalController : MonoBehaviour
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

	public Vector3 position;

	public bool ReadyForDelete;

	public int ScoreAdded = 1;

	public float Size = 2f;

	public Vector3 StartPosition;

	public Direction direction;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.World == null)
		{
			ReadyForDelete = true;
			return;
		}
		switch (direction)
		{
		case Direction.YPositive:
			position = StartPosition + Vector3.up * Mathf.PingPong(Time.time, 2f) * 2f;
			base.transform.position = position - Origin.position;
			break;
		case Direction.XPositive:
			position = StartPosition + Vector3.right * Mathf.PingPong(Time.time, 2f) * 2f;
			base.transform.position = position - Origin.position;
			break;
		case Direction.XNegative:
			position = StartPosition + Vector3.left * Mathf.PingPong(Time.time, 2f) * 2f;
			base.transform.position = position - Origin.position;
			break;
		case Direction.ZPositive:
			position = StartPosition + Vector3.forward * Mathf.PingPong(Time.time, 2f) * 2f;
			base.transform.position = position - Origin.position;
			break;
		case Direction.ZNegative:
			position = StartPosition + Vector3.back * Mathf.PingPong(Time.time, 2f) * 2f;
			base.transform.position = position - Origin.position;
			break;
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
				World world = GameManager.Instance.World;
				float lightBrightness = world.GetLightBrightness(entityAlive.GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("twitch_fireworks", entityAlive.position, lightBrightness, Color.white, null, null, _OLDCreateColliders: false), entityAlive.entityId, _forceCreation: false, _worldSpawn: true);
				Manager.BroadcastPlayByLocalPlayer(entityAlive.position, "twitch_celebrate");
				entityAlive.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
				GameManager.Instance.World.RemoveEntity(entityAlive.entityId, EnumRemoveEntityReason.Killed);
				if (!ReadyForDelete)
				{
					Owner.Score += ScoreAdded;
					ReadyForDelete = true;
				}
			}
		}
	}
}
