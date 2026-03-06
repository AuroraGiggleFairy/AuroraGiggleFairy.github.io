using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectricWireController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDipValue = 0.25f;

	public TileEntityPoweredMeleeTrap TileEntityParent;

	public TileEntityPoweredMeleeTrap TileEntityChild;

	public IWireNode WireNode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float healthRatio = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffActions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageReceived = "Damage_received";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float damageReceived;

	public Vector3i BlockPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float totalDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float particleDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float brokenPercentage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float breakingPercentage;

	public int OwnerEntityID = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Collider> CollidersThisFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthRatio = 1f;

	public float HealthRatio
	{
		get
		{
			return healthRatio;
		}
		set
		{
			lastHealthRatio = healthRatio;
			healthRatio = value;
			if (lastHealthRatio != -1f)
			{
				if (lastHealthRatio > brokenPercentage && healthRatio <= brokenPercentage)
				{
					setWireDip(dip: true);
				}
				else if (lastHealthRatio <= brokenPercentage && healthRatio > brokenPercentage)
				{
					setWireDip(dip: false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setWireDip(bool dip)
	{
		float wireDip = WireNode.GetWireDip();
		if (dip)
		{
			if (Mathf.Approximately(wireDip, 0f))
			{
				WireNode.SetWireDip(0.25f);
				WireNode.BuildMesh();
			}
		}
		else if (!Mathf.Approximately(wireDip, 0f))
		{
			WireNode.SetWireDip(0f);
			WireNode.BuildMesh();
		}
	}

	public void Init(DynamicProperties _properties)
	{
		if (_properties.Values.ContainsKey("Buff"))
		{
			if (buffActions == null)
			{
				buffActions = new List<string>();
			}
			string[] array = _properties.Values["Buff"].Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				buffActions.Add(array[i]);
			}
		}
		if (_properties.Values.ContainsKey("BreakingPercentage"))
		{
			breakingPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BreakingPercentage"]));
		}
		else
		{
			breakingPercentage = 0.5f;
		}
		if (_properties.Values.ContainsKey("BrokenPercentage"))
		{
			brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BrokenPercentage"]));
		}
		else
		{
			brokenPercentage = 0.25f;
		}
		if (_properties.Values.ContainsKey("DamageReceived"))
		{
			StringParsers.TryParseFloat(_properties.Values["DamageReceived"], out damageReceived);
		}
		else
		{
			damageReceived = 0.1f;
		}
		healthRatio = -1f;
		BlockPosition = TileEntityChild.ToWorldPos();
		startPoint = WireNode.GetStartPosition() + WireNode.GetStartPositionOffset();
		endPoint = WireNode.GetEndPosition() + WireNode.GetEndPositionOffset();
		BlockValue block = GameManager.Instance.World.GetBlock(BlockPosition);
		float num = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		if (num <= brokenPercentage)
		{
			setWireDip(dip: true);
		}
		else if (num > brokenPercentage)
		{
			setWireDip(dip: false);
		}
	}

	public void DamageSelf(float damage)
	{
		totalDamage += damage;
		if (!(totalDamage < 1f))
		{
			damage = (int)totalDamage;
			totalDamage = 0f;
			if (chunk == null)
			{
				chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(BlockPosition);
			}
			BlockValue block = GameManager.Instance.World.GetBlock(BlockPosition);
			HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
			_ = HealthRatio;
			_ = ((float)block.damage + damage) / (float)block.Block.MaxDamage;
			block.damage = Mathf.Clamp(block.damage + (int)damage, 0, block.Block.MaxDamage);
			GameManager.Instance.World.SetBlockRPC(chunk.ClrIdx, BlockPosition, block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		BlockValue block = GameManager.Instance.World.GetBlock(BlockPosition);
		HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		bool flag = HealthRatio < brokenPercentage;
		HandleParticlesForBroken(flag);
		setWireDip(flag);
		if (TileEntityParent == null || !TileEntityParent.IsPowered)
		{
			if (CollidersThisFrame != null && CollidersThisFrame.Count > 0)
			{
				CollidersThisFrame.Clear();
			}
		}
		else if (CollidersThisFrame != null && CollidersThisFrame.Count != 0)
		{
			for (int i = 0; i < CollidersThisFrame.Count; i++)
			{
				touched(CollidersThisFrame[i]);
			}
			CollidersThisFrame.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (TileEntityParent != null && TileEntityParent.IsPowered)
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		if (TileEntityParent != null && TileEntityParent.IsPowered)
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (TileEntityParent != null && TileEntityParent.IsPowered)
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void touched(Collider collider)
	{
		if (TileEntityParent == null || TileEntityChild == null || WireNode == null || collider == null || !TileEntityParent.IsPowered || !TileEntityChild.IsPowered || !(collider.transform != null))
		{
			return;
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
		if (!(entityAlive != null) || !entityAlive.IsAlive())
		{
			return;
		}
		bool flag = false;
		if (HealthRatio < brokenPercentage)
		{
			HandleParticlesForBroken(isBroken: true);
			return;
		}
		if (!entityAlive.Electrocuted && entityAlive.Buffs.GetCustomVar("ShockImmunity") == 0f && buffActions != null)
		{
			for (int i = 0; i < buffActions.Count; i++)
			{
				if (entityAlive.emodel != null && entityAlive.emodel.transform != null)
				{
					_ = entityAlive.emodel.transform;
					if (entityAlive.emodel.GetHitTransform(BodyPrimaryHit.Torso) != null)
					{
						entityAlive.Buffs.SetCustomVar("ETrapHit", 1f);
						entityAlive.Buffs.AddBuff(buffActions[i], TileEntityParent.OwnerEntityID, _netSync: true, _fromElectrical: true);
						entityAlive.Electrocuted = true;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			DamageSelf(damageReceived);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetGameObjectPath(Entity e, Transform transform)
	{
		string text = transform.name;
		while (transform.parent != null && transform.parent.name != e.transform.name)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParticlesForBroken(bool isBroken)
	{
		if (isBroken && TileEntityParent.IsPowered)
		{
			if (particleDelay > 0f)
			{
				particleDelay -= Time.deltaTime;
			}
			if (isBroken && particleDelay <= 0f)
			{
				Vector3 pos = WireNode.GetEndPosition() + WireNode.GetEndPositionOffset();
				float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(BlockPosition.ToVector3())) / 2f;
				ParticleEffect pe = new ParticleEffect("electric_fence_sparks", pos, lightValue, new Color(1f, 1f, 1f, 0.3f), "electric_fence_impact", null, _OLDCreateColliders: false);
				GameManager.Instance.SpawnParticleEffectServer(pe, -1, _forceCreation: true, _worldSpawn: true);
				particleDelay = 1f + UnityEngine.Random.value * 4f;
			}
		}
	}
}
