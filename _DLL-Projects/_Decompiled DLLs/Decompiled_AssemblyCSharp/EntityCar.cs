using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCar : EntityAlive
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bBoundingBoxNeedsUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropDamagedModel = "Model-Damage-";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int curModelIdx;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int modelCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPrimed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int explosionTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UpdateLightOnAllMaterials updateLightOnAllMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public new EntityActivationCommand[] cmds = new EntityActivationCommand[1]
	{
		new EntityActivationCommand("Search", "search", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		stepHeight = 0.2f;
		updateLightOnAllMaterials = base.transform.GetComponent<UpdateLightOnAllMaterials>();
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		EntityClass entityClass = EntityClass.list[base.entityClass];
		curModelIdx = 0;
		modelCount = 1;
		bool flag = true;
		int num = 1;
		while (flag)
		{
			flag = entityClass.Properties.Values.ContainsKey(PropDamagedModel + num);
			if (flag)
			{
				string text = entityClass.Properties.Values[PropDamagedModel + num];
				if (DataLoader.IsInResources(text))
				{
					text = "Entities/" + text;
				}
				GameObject obj = DataLoader.LoadAsset<GameObject>(text);
				if (obj == null)
				{
					throw new Exception("Missing car model '" + text + "'");
				}
				GameObject obj2 = UnityEngine.Object.Instantiate(obj);
				obj2.transform.parent = emodel.GetModelTransformParent();
				obj2.transform.localEulerAngles = Vector3.zero;
				obj2.transform.localPosition = Vector3.zero;
				obj2.transform.gameObject.SetActive(value: false);
				modelCount++;
			}
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (bBoundingBoxNeedsUpdate)
		{
			bBoundingBoxNeedsUpdate = false;
			BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
			Vector3 vector = base.transform.localRotation * component.size / 2f;
			vector.x = Mathf.Abs(vector.x);
			vector.y = Mathf.Abs(vector.y);
			vector.z = Mathf.Abs(vector.z);
			scaledExtent = new Vector3(vector.x * base.transform.localScale.x, vector.y * base.transform.localScale.y, vector.z * base.transform.localScale.z);
			SetPosition(position);
		}
	}

	public override void OnUpdateLive()
	{
		updateDamageModel();
		if (!isEntityRemote)
		{
			entityCollision(motion);
			motion.y -= 0.08f;
			motion.y *= 0.98f;
			motion.x *= 0.75f;
			motion.z *= 0.75f;
			if (base.transform.position.y + Origin.position.y < 0f)
			{
				SetDead();
			}
			if (bPrimed && --explosionTimer <= 0)
			{
				SetDead();
				bPrimed = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pushOutOfBlocks(float _x, float _y, float _z)
	{
		int num = Utils.Fastfloor(_x);
		int num2 = Utils.Fastfloor(_y);
		int num3 = Utils.Fastfloor(_z);
		float num4 = _x - (float)num;
		float num5 = _y - (float)num2;
		float num6 = _z - (float)num3;
		Block block = world.GetBlock(num, num2, num3).Block;
		if (block.blockID > 0 && block.IsCollideMovement)
		{
			bool num7 = !world.GetBlock(num - 1, num2, num3).Block.shape.IsSolidCube;
			bool flag = !world.GetBlock(num + 1, num2, num3).Block.shape.IsSolidCube;
			bool flag2 = !world.GetBlock(num, num2 - 1, num3).Block.shape.IsSolidCube;
			bool flag3 = !world.GetBlock(num, num2 + 1, num3).Block.shape.IsSolidCube;
			bool flag4 = !world.GetBlock(num, num2, num3 - 1).Block.shape.IsSolidCube;
			bool flag5 = !world.GetBlock(num, num2, num3 + 1).Block.shape.IsSolidCube;
			byte b = byte.MaxValue;
			double num8 = 9999.0;
			if (num7 && (double)num4 < num8)
			{
				num8 = num4;
				b = 0;
			}
			if (flag && 1.0 - (double)num4 < num8)
			{
				num8 = 1f - num4;
				b = 1;
			}
			if (flag2 && (double)num5 < num8)
			{
				num8 = num5;
				b = 2;
			}
			if (flag3 && (double)(1f - num5) < num8)
			{
				num8 = 1f - num5;
				b = 3;
			}
			if (flag4 && (double)num6 < num8)
			{
				num8 = num6;
				b = 4;
			}
			if (flag5 && (double)(1f - num6) < num8)
			{
				b = 5;
			}
			float num9 = rand.RandomFloat * 0.2f + 0.1f;
			if (b == 0)
			{
				motion.x = 0f - num9;
			}
			if (b == 1)
			{
				motion.x = num9;
			}
			if (b == 2)
			{
				motion.y = 0f - num9;
			}
			if (b == 3)
			{
				motion.y = num9;
			}
			if (b == 4)
			{
				motion.z = 0f - num9;
			}
			if (b == 5)
			{
				motion.z = num9;
			}
			return true;
		}
		return false;
	}

	public override void SetRotation(Vector3 _rot)
	{
		if (isEntityRemote)
		{
			base.SetRotation(_rot);
		}
		else
		{
			rotation.y = _rot.y % 360f;
			if (rotation.y < 0f)
			{
				rotation.y += 360f;
			}
			rotation.y = (int)((rotation.y + 45f) / 90f) * 90;
		}
		bBoundingBoxNeedsUpdate = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDamageModel()
	{
		if (!IsDead())
		{
			float v = (float)Health / (float)GetMaxHealth();
			int num = (int)((1f - Utils.FastMax(v, 0f)) * (float)(modelCount - 1));
			if (num != curModelIdx)
			{
				emodel.GetModelTransformParent().GetChild(num).gameObject.SetActive(value: true);
				emodel.GetModelTransformParent().GetChild(curModelIdx).gameObject.SetActive(value: false);
				curModelIdx = num;
				updateLightOnAllMaterials.Reset();
			}
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impuleScale)
	{
		if (_strength < 10)
		{
			return 0;
		}
		_strength = base.DamageEntity(_damageSource, _strength, _criticalHit, impuleScale);
		float num = (float)Health / (float)GetMaxHealth();
		if (!bPrimed && num <= 0.4f)
		{
			bPrimed = true;
			world.GetGameManager().SpawnParticleEffectClient(new ParticleEffect("smoke", GetPosition() + Vector3.up * 0.2f, GetLightBrightness(), Color.white, "Ambient_Loops/a_fire_med_lp", base.transform, _OLDCreateColliders: false), entityId);
			explosionTimer = 100;
		}
		return _strength;
	}

	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		if (!isEntityRemote)
		{
			if (EntityClass.list[entityClass].explosionData.ParticleIndex > 0)
			{
				GameManager.Instance.ExplosionServer(0, GetPosition(), World.worldToBlockPos(GetPosition()), base.transform.rotation, EntityClass.list[entityClass].explosionData, entityId, 0f, _bRemoveBlockAtExplPosition: false);
			}
			SetDeathTime(int.MaxValue);
		}
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		return cmds;
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (_indexInBlockActivationCommands == 0)
		{
			GameManager.Instance.TELockServer(0, _tePos, entityId, _entityFocusing.entityId);
		}
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
}
