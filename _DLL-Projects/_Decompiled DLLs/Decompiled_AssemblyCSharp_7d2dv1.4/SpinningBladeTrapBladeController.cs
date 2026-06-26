using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SpinningBladeTrapBladeController : MonoBehaviour
{
	public SpinningBladeTrapController controller;

	public Transform[] Blades;

	public Transform BladeCenter;

	public bool IsOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string bladeImpactSound = "Electricity/BladeTrap/bladetrap_impact";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float entityDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float blockDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float selfDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float brokenPercentage;

	public TileEntityPoweredMeleeTrap OwnerTE;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffActions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Collider> CollidersThisFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, float> entityHitCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float entityHitTime = 0.05f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityHitList = new List<Entity>();

	public void Init(DynamicProperties _properties, Block _block)
	{
		entityDamage = 20f;
		if (_block.Damage > 0f)
		{
			entityDamage = _block.Damage;
		}
		selfDamage = 0.1f;
		_properties.ParseFloat("DamageReceived", ref selfDamage);
		_properties.ParseString("ImpactSound", ref bladeImpactSound);
		brokenPercentage = 0.25f;
		_properties.ParseFloat("BrokenPercentage", ref brokenPercentage);
		brokenPercentage = Mathf.Clamp01(brokenPercentage);
		blockDamage = 0f;
		if (_properties.Values.ContainsKey("Buff"))
		{
			buffActions = new List<string>();
			string[] collection = _properties.Values["Buff"].Replace(" ", "").Split(',');
			buffActions.AddRange(collection);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (CollidersThisFrame == null)
		{
			CollidersThisFrame = new List<Collider>();
		}
		if (!CollidersThisFrame.Contains(other))
		{
			CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (CollidersThisFrame == null)
		{
			CollidersThisFrame = new List<Collider>();
		}
		if (!CollidersThisFrame.Contains(other))
		{
			CollidersThisFrame.Remove(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (entityHitCount == null)
		{
			entityHitCount = new Dictionary<int, float>();
		}
		if (!IsOn)
		{
			entityHitCount.Clear();
		}
		else
		{
			if (controller.HealthRatio <= brokenPercentage)
			{
				return;
			}
			entityHitList.Clear();
			GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(base.transform.position + Origin.position, new Vector3(3f, 3f, 3f)), entityHitList);
			if (entityHitList.Count == 0)
			{
				return;
			}
			DamageMultiplier damageMultiplier = new DamageMultiplier();
			bool flag = false;
			Vector3 vector = BladeCenter.position + Origin.position + new Vector3(0f, 0.2f, 0f);
			for (int i = 0; i < Blades.Length; i++)
			{
				Vector3 direction = Blades[i].position + Origin.position - vector;
				Voxel.Raycast(ray: new Ray(vector, direction), _world: GameManager.Instance.World, distance: 1.24f, _layerMask: -538750981, _hitMask: 128, _sphereRadius: 0.1f);
				WorldRayHitInfo hitInfo = Voxel.voxelRayHitInfo.Clone();
				EntityAlive entityFromCollider = GetEntityFromCollider(Voxel.voxelRayHitInfo.hitCollider);
				if (!(entityFromCollider != null) || !entityFromCollider.IsAlive())
				{
					continue;
				}
				bool flag2;
				if (entityHitCount.ContainsKey(entityFromCollider.entityId))
				{
					entityHitCount[entityFromCollider.entityId] += Time.deltaTime;
					flag2 = entityHitCount[entityFromCollider.entityId] >= entityHitTime;
					if (flag2)
					{
						entityHitCount[entityFromCollider.entityId] = 0f;
					}
				}
				else
				{
					entityHitCount.Add(entityFromCollider.entityId, 0f);
					flag2 = true;
				}
				if (flag2)
				{
					flag = true;
					ItemActionAttack.Hit(hitInfo, (OwnerTE.OwnerEntityID == entityFromCollider.entityId) ? (-1) : OwnerTE.OwnerEntityID, EnumDamageTypes.Slashing, blockDamage, entityDamage, 1f, 1f, 0f, 0.05f, "metal", damageMultiplier, buffActions, new ItemActionAttack.AttackHitInfo(), 3, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -2, OwnerTE.blockValue.ToItemValue());
				}
			}
			if (flag)
			{
				controller.DamageSelf(selfDamage);
				Manager.BroadcastPlay(controller.BlockPosition.ToVector3(), bladeImpactSound);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive GetEntityFromCollider(Collider collider)
	{
		if (collider == null || collider.transform == null)
		{
			return null;
		}
		EntityAlive entityAlive = collider.transform.GetComponent<EntityAlive>();
		if (entityAlive == null)
		{
			entityAlive = collider.transform.GetComponentInParent<EntityAlive>();
		}
		if (entityAlive == null && collider.transform.parent != null)
		{
			entityAlive = collider.transform.parent.GetComponentInChildren<EntityAlive>();
		}
		if (entityAlive == null)
		{
			entityAlive = collider.transform.GetComponentInChildren<EntityAlive>();
		}
		return entityAlive;
	}
}
