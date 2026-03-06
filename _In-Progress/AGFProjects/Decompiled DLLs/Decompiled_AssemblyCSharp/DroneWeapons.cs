using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DroneWeapons
{
	[Preserve]
	public class Weapon
	{
		public Transform WeaponJoint;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive entity;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DynamicProperties properties;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int belongsPlayerId;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float actionTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float cooldown = 1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float range = 10f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive target;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onFireComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public float cooldownTimer;

		public float TimeRemaning => cooldownTimer;

		public float TimeLength => actionTime + cooldown;

		public float Range => range;

		public Weapon(EntityAlive _entity)
		{
			entity = _entity;
			properties = _entity.EntityClass.Properties;
			belongsPlayerId = entity.belongsPlayerId;
		}

		public virtual void Init()
		{
		}

		public virtual void Update()
		{
			if (cooldownTimer > 0f)
			{
				cooldownTimer -= 0.05f;
				if (cooldownTimer <= 0f || ((bool)target && target.IsDead()))
				{
					InvokeFireComplete();
				}
			}
		}

		public virtual bool canFire()
		{
			return cooldownTimer <= 0f;
		}

		public virtual void Fire(EntityAlive _target)
		{
			target = _target;
			cooldownTimer = actionTime + cooldown;
		}

		public virtual bool hasActionCompleted()
		{
			return cooldownTimer < cooldown;
		}

		public void RegisterOnFireComplete(Action _onFireComplete)
		{
			onFireComplete = _onFireComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnFireComplete()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void InvokeFireComplete()
		{
			OnFireComplete();
			onFireComplete?.Invoke();
			onFireComplete = null;
			target = null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void TargetApplyBuff(string _buff)
		{
			target.Buffs.AddBuff(_buff, (belongsPlayerId != -1) ? belongsPlayerId : entity.entityId);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void SpawnParticleEffect(ParticleEffect _pe, int _entityId)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (!GameManager.IsDedicatedServer)
				{
					GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId);
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Transform SpawnDroneParticleEffect(ParticleEffect _pe, int _entityId, NetPackageDroneParticleEffect.cActionType actionType)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType));
				if (!GameManager.IsDedicatedServer)
				{
					return GameManager.Instance.SpawnParticleEffectClientForceCreation(_pe, _entityId, _worldSpawn: false);
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType));
			}
			return null;
		}
	}

	[Preserve]
	public class HealBeamWeapon(EntityAlive _entity) : Weapon(_entity)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public enum HealItemType
		{
			None,
			AloeCream,
			Bandage,
			MedicalBandage,
			FirstAidKit
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealBeam = "drone_heal_beam";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealPlayer = "drone_heal_player";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cIdxHealing = 0;

		public float HealDamageThreshold = 35f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cAbrasionInjury = "buffInjuryAbrasion";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealingBuff = "buffHealHealth";

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cModifiedHealthCutoff = 0.67f;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] supportedItems = new string[5] { "none", "medicalAloeCream", "medicalBandage", "medicalFirstAidBandage", "medicalFirstAidKit" };

		public override void Init()
		{
			WeaponJoint = entity.transform.FindInChilds("WristLeft");
			if (properties.Values.ContainsKey("HealCooldown"))
			{
				float.TryParse(properties.Values["HealCooldown"], out cooldown);
			}
			if (properties.Values.ContainsKey("HealActionTime"))
			{
				float.TryParse(properties.Values["HealActionTime"], out actionTime);
			}
			if (properties.Values.ContainsKey("HealDamageThreshold"))
			{
				float.TryParse(properties.Values["HealDamageThreshold"], out HealDamageThreshold);
			}
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return;
			}
			HealItemType healItemType = findNeededHealType(_target);
			if (healItemType == HealItemType.None)
			{
				InvokeFireComplete();
				Log.Out("HealBeamWeapon: failed to determine heal type");
				return;
			}
			ItemStack healingItemStack = getHealingItemStack(healItemType);
			if (healingItemStack == null)
			{
				InvokeFireComplete();
				return;
			}
			entity.inventory.SetItem(0, healingItemStack);
			entity.inventory.SetHoldingItemIdx(0);
			entity.inventory.ForceHoldingItemUpdate();
			ItemAction itemAction = entity.inventory.holdingItem.Actions[1];
			ItemActionData itemActionData = entity.inventory.holdingItemData.actionData[1];
			ItemActionUseOther.FeedInventoryData feedInventoryData = itemActionData as ItemActionUseOther.FeedInventoryData;
			EntityDrone obj = entity as EntityDrone;
			EntityAlive attackTarget = obj.GetAttackTarget();
			if ((bool)attackTarget)
			{
				_ = attackTarget.AttachedToEntity as EntityVehicle != null;
			}
			else
				_ = 0;
			if (feedInventoryData != null)
			{
				feedInventoryData.TargetEntity = attackTarget;
				itemActionData = feedInventoryData;
			}
			if (itemAction != null && itemAction.CanExecute(itemActionData))
			{
				itemAction.ExecuteAction(itemActionData, _bReleased: false);
				itemAction.ExecuteAction(itemActionData, _bReleased: true);
			}
			EntityAlive owner = obj.Owner;
			if ((bool)owner)
			{
				owner.Buffs.AddBuff("buffJunkDroneHealCooldownEffect");
			}
			ParticleEffect pe = new ParticleEffect("drone_heal_beam", Vector3.zero, Quaternion.LookRotation(_target.getHeadPosition() - entity.position), 1f, Color.clear, null, entity.transform);
			Transform transform = SpawnDroneParticleEffect(pe, entity.entityId, NetPackageDroneParticleEffect.cActionType.Heal);
			if ((bool)transform && !GameManager.IsDedicatedServer)
			{
				transform.GetComponent<DroneBeamParticle>().SetDisplayTime(actionTime);
			}
			ParticleEffect pe2 = new ParticleEffect("drone_heal_player", Vector3.zero, Quaternion.identity, 1f, Color.clear, null, _target.transform);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(pe2, _target.entityId));
				if (!GameManager.IsDedicatedServer)
				{
					GameManager.Instance.SpawnParticleEffectClient(pe2, _target.entityId);
				}
			}
		}

		public override bool canFire()
		{
			if (base.canFire())
			{
				return hasHealingItem();
			}
			return false;
		}

		public bool hasHealingItem()
		{
			return hasSupportedHealingItem();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasSupportedHealingItem()
		{
			for (int i = 0; i < supportedItems.Length; i++)
			{
				if (hasItem(supportedItems[i]))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasMedicalTreatment()
		{
			bool num = hasItem(HealItemType.MedicalBandage);
			bool flag = hasItem(HealItemType.FirstAidKit);
			return num || flag;
		}

		public bool targetCanBeHealed(EntityAlive _target)
		{
			if (_target.IsAlive() && !_target.Buffs.HasBuff("buffHealHealth") && (float)_target.Health < _target.Stats.Health.ModifiedMax)
			{
				return true;
			}
			return false;
		}

		public bool isTargetInNeedOfMedical(EntityAlive _target)
		{
			float num = _target.GetMaxHealth();
			float modifiedMax = _target.Stats.Health.ModifiedMax;
			if (num != modifiedMax || !((float)_target.Health < num - HealDamageThreshold))
			{
				return (float)_target.Health < modifiedMax * 0.67f;
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkNeedsHealing(EntityAlive _target)
		{
			if (targetCanBeHealed(_target) && isTargetInNeedOfMedical(_target))
			{
				return true;
			}
			return false;
		}

		public bool targetNeedsHealing(EntityAlive _target)
		{
			return findNeededHealType(_target) != HealItemType.None;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasBleedingTreatment()
		{
			bool num = hasItem(HealItemType.Bandage);
			bool flag = hasItem(HealItemType.MedicalBandage);
			bool flag2 = hasItem(HealItemType.FirstAidKit);
			return num || flag || flag2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isTargetBleeding(EntityAlive _target)
		{
			return _target.Buffs.ActiveBuffs.Find([PublicizedFrom(EAccessModifier.Internal)] (BuffValue b) => b.BuffName.ContainsCaseInsensitive("buffInjuryBleeding")) != null;
		}

		public bool targetNeedsTreatment(EntityAlive _target)
		{
			bool flag = isTargetBleeding(_target);
			if (hasBleedingTreatment() && flag)
			{
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HealItemType findNeededHealType(EntityAlive _entity)
		{
			bool flag = hasItem(HealItemType.Bandage);
			bool flag2 = hasItem(HealItemType.MedicalBandage);
			bool flag3 = hasItem(HealItemType.FirstAidKit);
			if (isTargetInNeedOfMedical(_entity) || targetCanBeHealed(_entity))
			{
				if (flag2)
				{
					return HealItemType.MedicalBandage;
				}
				if (flag3)
				{
					return HealItemType.FirstAidKit;
				}
				if (isTargetBleeding(_entity))
				{
					return HealItemType.Bandage;
				}
			}
			else if (isTargetBleeding(_entity))
			{
				if (flag)
				{
					return HealItemType.Bandage;
				}
				if (flag2)
				{
					return HealItemType.MedicalBandage;
				}
				if (flag3)
				{
					return HealItemType.FirstAidKit;
				}
			}
			return HealItemType.None;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemStack getHealingItemStack(HealItemType healType)
		{
			ItemStack[] array = entity.bag.GetSlots();
			if (entity.lootContainer != null)
			{
				array = entity.lootContainer.GetItems();
			}
			for (int i = 0; i < array.Length; i++)
			{
				ItemStack itemStack = array[i];
				if (itemStack != null && itemStack.itemValue != null && itemStack.itemValue.ItemClass != null && itemStack.itemValue.ItemClass.HasAnyTags(healingItemTags) && itemStack.count > 0 && isItem(itemStack.itemValue, healType))
				{
					ItemValue itemValue = itemStack.itemValue.Clone();
					itemStack.count--;
					if (itemStack.count == 0)
					{
						itemStack = ItemStack.Empty.Clone();
					}
					array[i] = itemStack;
					entity.bag.SetSlots(array);
					entity.bag.OnUpdate();
					entity.lootContainer.UpdateSlot(i, itemStack);
					return new ItemStack(itemValue, 1);
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasItem(string itemGroupOrName)
		{
			ItemStack[] array = entity.bag.GetSlots();
			if (entity.lootContainer != null)
			{
				array = entity.lootContainer.GetItems();
			}
			foreach (ItemStack itemStack in array)
			{
				if (itemStack != null && itemStack.itemValue != null && itemStack.itemValue.ItemClass != null && itemStack.itemValue.ItemClass.HasAnyTags(healingItemTags) && itemStack.itemValue.ItemClass.Name.Equals(itemGroupOrName))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasItem(HealItemType itemType)
		{
			return hasItem(supportedItems[(int)itemType]);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isItem(ItemValue iv, string itemGroupOrName)
		{
			if (iv.ItemClass.Name.Equals(itemGroupOrName))
			{
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isItem(ItemValue iv, HealItemType itemType)
		{
			return isItem(iv, supportedItems[(int)itemType]);
		}
	}

	[Preserve]
	public class StunBeamWeapon : Weapon
	{
		public StunBeamWeapon(EntityAlive _entity)
			: base(_entity)
		{
		}

		public override void Init()
		{
			WeaponJoint = entity.transform.FindInChilds("WristRight");
			if (properties.Values.ContainsKey("StunCooldown"))
			{
				float.TryParse(properties.Values["StunCooldown"], out cooldown);
			}
			if (properties.Values.ContainsKey("StunActionTime"))
			{
				float.TryParse(properties.Values["StunActionTime"], out actionTime);
			}
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			TargetApplyBuff("buffShocked");
			Manager.Play(entity, "drone_attackeffect");
			ParticleEffect pe = new ParticleEffect("nozzleflashuzi", WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", WeaponJoint);
			SpawnParticleEffect(pe, -1);
			float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(entity.position)) / 2f;
			ParticleEffect pe2 = new ParticleEffect("nozzlesmokeuzi", WeaponJoint.position + Origin.position, lightValue, new Color(1f, 1f, 1f, 0.3f), null, WeaponJoint, _OLDCreateColliders: false);
			SpawnParticleEffect(pe2, -1);
		}
	}

	[Preserve]
	public class MachineGunWeapon(EntityAlive _entity) : Weapon(_entity)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public float RayCount = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadHorizontal;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadVertical;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCountMax = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float burstFireRateMax = 0.1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float burstFireRate = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public int blockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DamageMultiplier damageMultiplier;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> buffActions;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> tmpTag;

		public override void Init()
		{
			damageMultiplier = new DamageMultiplier(properties, null);
			WeaponJoint = entity.transform.FindInChilds("WristRight");
			if (properties.Values.ContainsKey("MaxDistance"))
			{
				range = StringParsers.ParseFloat(properties.Values["MaxDistance"]);
			}
			spreadHorizontal = new Vector2(-1f, 1f);
			spreadVertical = new Vector2(-1f, 1f);
			if (properties.Values.ContainsKey("RaySpread"))
			{
				float num = StringParsers.ParseFloat(properties.Values["RaySpread"]);
				num *= 0.5f;
				spreadHorizontal = new Vector2(0f - num, num);
				spreadVertical = new Vector2(0f - num, num);
			}
			if (properties.Values.ContainsKey("RayCount"))
			{
				RayCount = int.Parse(properties.Values["RayCount"]);
			}
			if (properties.Values.ContainsKey("BurstRoundCount"))
			{
				burstRoundCountMax = int.Parse(properties.Values["BurstRoundCount"]);
			}
			if (properties.Values.ContainsKey("BurstFireRate"))
			{
				burstFireRate = Mathf.Max(StringParsers.ParseFloat(properties.Values["BurstFireRate"]), 0.1f);
			}
			actionTime = burstFireRate * (float)burstRoundCountMax;
			if (properties.Values.ContainsKey("CooldownTime"))
			{
				cooldown = StringParsers.ParseFloat(properties.Values["CooldownTime"]);
			}
			if (properties.Values.ContainsKey("EntityDamage"))
			{
				entityDamage = int.Parse(properties.Values["EntityDamage"]);
			}
			buffActions = new List<string>();
			if (properties.Values.ContainsKey("Buff"))
			{
				string[] collection = properties.Values["Buff"].Replace(" ", "").Split(',');
				buffActions.AddRange(collection);
			}
		}

		public override void Update()
		{
			base.Update();
			if (target != null && !target.IsDead() && burstRoundCount < burstRoundCountMax && base.TimeRemaning > 0f && base.TimeRemaning < base.TimeLength - burstFireRate * (float)burstRoundCount)
			{
				_fireWeapon();
			}
		}

		public override void Fire(EntityAlive _target)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				base.Fire(_target);
				_fireWeapon();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnFireComplete()
		{
			base.OnFireComplete();
			burstRoundCount = 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void _fireWeapon()
		{
			EntityDrone entityDrone = entity as EntityDrone;
			Vector3 position = WeaponJoint.transform.position;
			Vector3 vector = target.getChestPosition() - Origin.position;
			EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive;
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			_ = entityDrone.OriginalItemValue.ItemClass.ItemTags;
			int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, entityDrone.OriginalItemValue, RayCount, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
			float value = EffectManager.GetValue(PassiveEffects.MaxRange, entityDrone.OriginalItemValue, range, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
			for (int i = 0; i < num; i++)
			{
				Vector3 normalized = (vector - position).normalized;
				normalized = Quaternion.Euler(gameRandom.RandomRange(spreadHorizontal.x, spreadHorizontal.y), gameRandom.RandomRange(spreadVertical.x, spreadVertical.y), 0f) * normalized;
				Ray ray = new Ray(position + Origin.position, normalized);
				int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, entityDrone.OriginalItemValue, 0f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false));
				num2++;
				int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, entityDrone.OriginalItemValue, 1f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false));
				EntityAlive entityAlive2 = null;
				for (int j = 0; j < num2; j++)
				{
					if (!Voxel.Raycast(GameManager.Instance.World, ray, value, -538750997, 8, 0f))
					{
						continue;
					}
					WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
					if (worldRayHitInfo.tag.StartsWith("E_"))
					{
						string bodyPartName;
						EntityAlive entityAlive3 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out bodyPartName) as EntityAlive;
						if (entityAlive2 == entityAlive3)
						{
							ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
							j--;
							continue;
						}
						entityAlive2 = entityAlive3;
					}
					else
					{
						j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num3);
					}
					ItemActionAttack.Hit(worldRayHitInfo, entityDrone.belongsPlayerId, EnumDamageTypes.Piercing, GetDamageBlock(entityDrone.OriginalItemValue, BlockValue.Air, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), GetDamageEntity(entityDrone.OriginalItemValue, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), 1f, entityDrone.OriginalItemValue.PercentUsesLeft, 0f, 0f, "bullet", damageMultiplier, buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, entityDrone.entityId, entityDrone.OriginalItemValue);
				}
			}
			ParticleEffect pe = new ParticleEffect("nozzleflashuzi", WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", WeaponJoint);
			SpawnParticleEffect(pe, -1);
			float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(entity.position)) / 2f;
			ParticleEffect pe2 = new ParticleEffect("nozzlesmokeuzi", WeaponJoint.position + Origin.position, lightValue, new Color(1f, 1f, 1f, 0.3f), null, WeaponJoint, _OLDCreateColliders: false);
			SpawnParticleEffect(pe2, -1);
			burstRoundCount++;
			if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, entityDrone.OriginalItemValue) > 0)
			{
				entityDrone.AmmoCount--;
			}
			entityDrone.OriginalItemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, entityDrone.OriginalItemValue, 1f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, entityDamage, _holdingEntity, null, _itemValue.ItemClass.ItemTags, calcEquipment: true, calcHoldingItem: false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			tmpTag = _itemValue.ItemClass.ItemTags;
			tmpTag |= _blockValue.Block.Tags;
			float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, blockDamage, _holdingEntity, null, tmpTag, calcEquipment: true, calcHoldingItem: false);
			return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
		}
	}

	[Preserve]
	public class NetPackageDroneParticleEffect : NetPackage
	{
		public enum cActionType
		{
			Attack,
			Heal
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ParticleEffect pe;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityThatCausedIt;

		[PublicizedFrom(EAccessModifier.Private)]
		public cActionType actionType;

		public NetPackageDroneParticleEffect Setup(ParticleEffect _pe, int _entityThatCausedIt, cActionType _actionType)
		{
			pe = _pe;
			entityThatCausedIt = _entityThatCausedIt;
			actionType = _actionType;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			pe = new ParticleEffect();
			pe.Read(_br);
			entityThatCausedIt = _br.ReadInt32();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			pe.Write(_bw);
			_bw.Write(entityThatCausedIt);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			if (!_world.IsRemote())
			{
				_world.GetGameManager().SpawnParticleEffectServer(pe, entityThatCausedIt);
				return;
			}
			Transform transform = _world.GetGameManager().SpawnParticleEffectClientForceCreation(pe, entityThatCausedIt, worldSpawn: false);
			if (!(transform != null))
			{
				return;
			}
			EntityDrone entityDrone = _world.GetEntity(entityThatCausedIt) as EntityDrone;
			if (entityDrone != null)
			{
				DroneBeamParticle component = transform.GetComponent<DroneBeamParticle>();
				if (actionType == cActionType.Attack)
				{
					transform.parent = entityDrone.stunWeapon.WeaponJoint;
					component.SetDisplayTime(entityDrone.AttackActionTime);
				}
				else if (actionType == cActionType.Heal)
				{
					transform.parent = entityDrone.healWeapon.WeaponJoint;
					component.SetDisplayTime(entityDrone.HealActionTime);
				}
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}

		public override int GetLength()
		{
			return 20;
		}
	}

	public const string cSHOCK_BUFF_NAME = "buffShocked";

	public const string cBuffHealCooldown = "buffJunkDroneHealCooldownEffect";

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> healingItemTags = FastTags<TagGroup.Global>.Parse("medical");

	public const string cHealWeaponJoint = "WristLeft";
}
