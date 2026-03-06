using System.Collections.Generic;
using GUI_2;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class ItemActionAttack : ItemAction
{
	public enum EnumAttackType
	{
		Melee,
		Projectile,
		Missile
	}

	public struct Bonuses(float _tool, float _damage)
	{
		public float Tool = _tool;

		public float Damage = _damage;
	}

	public class AttackHitInfo
	{
		public Vector3i raycastHitPosition;

		public Vector3i hitPosition;

		public BlockValue blockBeingDamaged;

		public float damagePerHit;

		public float damage;

		public float hardnessScale;

		public float damageTotalOfTarget;

		public int damageGiven;

		public int damageMax;

		public Dictionary<EnumDropEvent, List<Block.SItemDropProb>> itemsToDrop;

		public bool bKilled;

		public bool bBlockHit;

		public bool bHarvestTool;

		public Entity entityHit;

		public string materialCategory;

		public bool isCriticalHit;

		public FastTags<TagGroup.Global> WeaponTypeTag;

		public byte ammoIndex;
	}

	public enum EnumAttackMode
	{
		RealNoHarvesting,
		RealAndHarvesting,
		RealNoHarvestingOrEffects,
		Simulate
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class RadialContextItem : XUiC_Radial.RadialContextAbs
	{
		public readonly ItemActionRanged RangedItemAction;

		public RadialContextItem(ItemActionRanged _rangedItemAction)
		{
			RangedItemAction = _rangedItemAction;
		}
	}

	public const string PropSurfaceCategory = "SurfaceCategory";

	public static float attackRangeMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundRepeat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundEnd;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundEmpty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundReload;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particlesMuzzleFire;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particlesMuzzleSmoke;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particlesMuzzleFireFpv;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particlesMuzzleSmokeFpv;

	public string[] MagazineItemNames;

	public int[] MagazineItemRayCount;

	public float[] MagazineItemSpread;

	public int BulletsPerMagazine;

	public bool AmmoIsPerMagazine;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float BulletUsePerShot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDamageTypes DamageType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float reloadingTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float damageEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float damageBlock;

	public new float BlockRange;

	public new int RaysPerShot = 1;

	public new float RaysSpread;

	public EnumAttackType Type;

	public new bool InfiniteAmmo;

	public bool ForceShowAmmo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DamageMultiplier damageMultiplier;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitmaskOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, Bonuses> ToolBonuses = new Dictionary<string, Bonuses>();

	public static FastTags<TagGroup.Global> ThrownTag = FastTags<TagGroup.Global>.Parse("thrown");

	public static FastTags<TagGroup.Global> RangedTag = FastTags<TagGroup.Global>.Parse("ranged");

	public static FastTags<TagGroup.Global> MeleeTag = FastTags<TagGroup.Global>.Parse("melee");

	public static FastTags<TagGroup.Global> PrimaryTag = FastTags<TagGroup.Global>.Parse("primary");

	public static FastTags<TagGroup.Global> SecondaryTag = FastTags<TagGroup.Global>.Parse("secondary");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;

	public const int cHitDefault = 1;

	public const int cHitToolBeltNotify = 1;

	public const int cHitElectricTrap = 2;

	public const int cHitHarvestParticles = 4;

	public const int cHitEffectOff = 8;

	public int Hitmask => hitmaskOverride;

	public ItemActionAttack()
	{
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? PrimaryTag : SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, damageEntity, _holdingEntity, null, tmpTag);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? PrimaryTag : SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, damageBlock, _holdingEntity, null, tmpTag);
		return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	public DamageMultiplier GetDamageMultiplier()
	{
		return damageMultiplier;
	}

	public Vector3 GetKickbackForce(Vector3 _shotDirection)
	{
		return Vector3.zero;
	}

	public virtual void ReloadGun(ItemActionData _actionData)
	{
	}

	public virtual bool CanReload(ItemActionData _actionData)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void showGunFire()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData actionData)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData actionData)
	{
		if (actionData.invData.holdingEntity as EntityPlayerLocal == null)
		{
			return false;
		}
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(actionData);
		if (executeActionTarget == null || !executeActionTarget.bHitValid || actionData.attackDetails.itemsToDrop == null || !(Time.time - actionData.lastUseTime <= 1.5f))
		{
			return false;
		}
		if (actionData.attackDetails.bBlockHit)
		{
			return actionData.attackDetails.raycastHitPosition == executeActionTarget.hit.blockPos;
		}
		if (!actionData.attackDetails.bBlockHit)
		{
			Entity hitRootEntity = GameUtils.GetHitRootEntity(executeActionTarget.tag, executeActionTarget.transform);
			if ((bool)hitRootEntity)
			{
				return hitRootEntity == actionData.attackDetails.entityHit;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getOverlayData(ItemActionData actionData, out float _perc, out string _text)
	{
		float num = actionData.attackDetails.damageTotalOfTarget;
		float num2 = actionData.attackDetails.damageMax;
		if (actionData.attackDetails.bBlockHit)
		{
			BlockValue block = actionData.invData.world.GetBlock(actionData.attackDetails.hitPosition);
			num = block.damage;
			num2 = block.Block.GetShownMaxDamage();
		}
		_perc = (num2 - num) / num2;
		_text = string.Format("{0}/{1}", Utils.FastMax(0f, num2 - num).ToCultureInvariantString("0"), num2.ToCultureInvariantString());
	}

	public override void OnHUD(ItemActionData _actionData, int _x, int _y)
	{
		if (!(_actionData is ItemActionAttackData itemActionAttackData) || !canShowOverlay(itemActionAttackData))
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = itemActionAttackData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!isShowOverlay(itemActionAttackData))
		{
			if (itemActionAttackData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
			{
				XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
				itemActionAttackData.uiOpenedByMe = false;
			}
			return;
		}
		if (!XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
		{
			itemActionAttackData.uiOpenedByMe = true;
		}
		getOverlayData(itemActionAttackData, out var _perc, out var _text);
		XUiC_FocusedBlockHealth.SetData(uIForPlayer, _text, _perc);
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
		if (uIForPlayer != null && _data.invData.holdingEntity is EntityPlayerLocal && _data is ItemActionAttackData && XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
		{
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
			((ItemActionAttackData)_data).uiOpenedByMe = false;
		}
	}

	public override RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.None;
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		foreach (KeyValuePair<string, string> item in _props.Values.Dict)
		{
			if (item.Key.StartsWith("ToolCategory."))
			{
				ToolBonuses[item.Key.Substring("ToolCategory.".Length)] = new Bonuses(StringParsers.ParseFloat(_props.Values[item.Key]), _props.Params1.ContainsKey(item.Key) ? StringParsers.ParseFloat(_props.Params1[item.Key]) : 2f);
			}
		}
		damageEntity = 0f;
		_props.ParseFloat("DamageEntity", ref damageEntity);
		damageBlock = 0f;
		_props.ParseFloat("DamageBlock", ref damageBlock);
		Range = 0f;
		_props.ParseFloat("Range", ref Range);
		BlockRange = Range;
		_props.ParseFloat("Block_range", ref BlockRange);
		SphereRadius = 0f;
		_props.ParseFloat("Sphere", ref SphereRadius);
		_props.ParseInt("Magazine_size", ref BulletsPerMagazine);
		if (_props.Values.ContainsKey("Magazine_items"))
		{
			string[] array = _props.Values["Magazine_items"].Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Trim();
			}
			MagazineItemNames = array;
		}
		if (_props.Values.ContainsKey("Magazine_item_ray_counts"))
		{
			MagazineItemRayCount = new int[MagazineItemNames.Length];
			string[] array2 = _props.Values["Magazine_item_ray_counts"].Split(',');
			for (int j = 0; j < array2.Length && j < MagazineItemRayCount.Length; j++)
			{
				if (j < MagazineItemRayCount.Length)
				{
					MagazineItemRayCount[j] = int.Parse(array2[j]);
				}
			}
		}
		if (_props.Values.ContainsKey("Magazine_item_ray_spreads"))
		{
			MagazineItemSpread = new float[MagazineItemNames.Length];
			string[] array3 = _props.Values["Magazine_item_ray_spreads"].Split(',');
			for (int k = 0; k < array3.Length && k < MagazineItemSpread.Length; k++)
			{
				MagazineItemSpread[k] = StringParsers.ParseFloat(array3[k]);
			}
		}
		if (_props.Values.ContainsKey("Single_magazine_usage"))
		{
			AmmoIsPerMagazine = StringParsers.ParseBool(_props.Values["Single_magazine_usage"]);
		}
		if (_props.Values.ContainsKey("Bullet_use_per_shot"))
		{
			BulletUsePerShot = StringParsers.ParseFloat(_props.Values["Bullet_use_per_shot"]);
		}
		else
		{
			BulletUsePerShot = 1f;
		}
		if (_props.Values.ContainsKey("Rays_per_shot"))
		{
			RaysPerShot = int.Parse(_props.Values["Rays_per_shot"]);
		}
		if (_props.Values.ContainsKey("Rays_spread"))
		{
			RaysSpread = StringParsers.ParseFloat(_props.Values["Rays_spread"]);
		}
		if (_props.Values.ContainsKey("Reload_time"))
		{
			reloadingTime = StringParsers.ParseFloat(_props.Values["Reload_time"]);
		}
		if (_props.Values.ContainsKey("Sound_repeat"))
		{
			soundRepeat = _props.Values["Sound_repeat"];
		}
		else
		{
			soundRepeat = "";
		}
		if (_props.Values.ContainsKey("Sound_end"))
		{
			soundEnd = _props.Values["Sound_end"];
		}
		else
		{
			soundEnd = "";
		}
		if (_props.Values.ContainsKey("Sound_empty"))
		{
			soundEmpty = _props.Values["Sound_empty"];
		}
		else
		{
			soundEmpty = "";
		}
		if (_props.Values.ContainsKey("Sound_reload"))
		{
			soundReload = _props.Values["Sound_reload"];
		}
		else
		{
			soundReload = "";
		}
		_props.ParseString("Particles_muzzle_fire", ref particlesMuzzleFire);
		_props.ParseString("Particles_muzzle_fire_fpv", ref particlesMuzzleFireFpv);
		if (_props.Values.ContainsKey("Particles_muzzle_smoke"))
		{
			particlesMuzzleSmoke = _props.Values["Particles_muzzle_smoke"];
		}
		if (_props.Values.ContainsKey("Particles_muzzle_smoke_fpv"))
		{
			particlesMuzzleSmokeFpv = _props.Values["Particles_muzzle_smoke_fpv"];
		}
		if (_props.Values.ContainsKey("Infinite_ammo"))
		{
			InfiniteAmmo = StringParsers.ParseBool(_props.Values["Infinite_ammo"]);
		}
		if (_props.Values.ContainsKey("Show_ammo_force"))
		{
			ForceShowAmmo = StringParsers.ParseBool(_props.Values["Show_ammo_force"]);
		}
		if (_props.Values.ContainsKey("Damage_type"))
		{
			DamageType = EnumUtils.Parse<EnumDamageTypes>(_props.Values["Damage_type"], _ignoreCase: true);
		}
		else
		{
			DamageType = EnumDamageTypes.None;
		}
		damageMultiplier = new DamageMultiplier(Properties, null);
		hitmaskOverride = Voxel.ToHitMask(_props.GetString("Hitmask_override"));
	}

	public static Entity GetEntityFromHit(WorldRayHitInfo hitInfo)
	{
		return GameUtils.GetHitRootEntity(hitInfo.tag, hitInfo.transform);
	}

	public static void Hit(WorldRayHitInfo hitInfo, int _attackerEntityId, EnumDamageTypes _damageType, float _blockDamage, float _entityDamage, float _staminaDamageMultiplier, float _weaponCondition, float _criticalHitChanceOLD, float _dismemberChance, string _attackingDeviceMadeOf, DamageMultiplier _damageMultiplier, List<string> _buffActions, AttackHitInfo _attackDetails, int _flags = 1, int _actionExp = 0, float _actionExpBonus = 0f, ItemActionAttack rangeCheckedAction = null, Dictionary<string, Bonuses> _toolBonuses = null, EnumAttackMode _attackMode = EnumAttackMode.RealNoHarvesting, Dictionary<string, string> _hitSoundOverrides = null, int ownedEntityId = -1, ItemValue damagingItemValue = null)
	{
		if (hitInfo == null || hitInfo.tag == null)
		{
			return;
		}
		World world = GameManager.Instance.World;
		bool flag = true;
		if (_attackMode == EnumAttackMode.RealNoHarvestingOrEffects)
		{
			flag = false;
			_attackMode = EnumAttackMode.RealNoHarvesting;
		}
		if (_attackDetails != null)
		{
			_attackDetails.itemsToDrop = null;
			_attackDetails.bBlockHit = false;
			_attackDetails.entityHit = null;
		}
		string text = null;
		string text2 = null;
		float lightValue = 1f;
		Color color = Color.white;
		bool flag2 = false;
		EntityAlive entityAlive = world.GetEntity(_attackerEntityId) as EntityAlive;
		bool flag3 = false;
		if (entityAlive != null)
		{
			if (damagingItemValue == null)
			{
				damagingItemValue = entityAlive.inventory.holdingItemItemValue;
			}
			flag3 = damagingItemValue.Equals(entityAlive.inventory.holdingItemItemValue);
		}
		bool flag4 = true;
		if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
		{
			if (ItemAction.ShowDebugDisplayHit)
			{
				Vector3 position = Camera.main.transform.position;
				DebugLines.Create(null, entityAlive.RootTransform, position + Origin.position, hitInfo.hit.pos, new Color(1f, 0.5f, 1f), new Color(1f, 0f, 1f), ItemAction.DebugDisplayHitSize * 2f, ItemAction.DebugDisplayHitSize, ItemAction.DebugDisplayHitTime);
			}
			ChunkCluster chunkCluster = world.ChunkClusters[hitInfo.hit.clrIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Vector3i vector3i = hitInfo.hit.blockPos;
			BlockValue blockValue = chunkCluster.GetBlock(vector3i);
			if (blockValue.isair && hitInfo.hit.blockValue.Block.IsDistantDecoration && hitInfo.hit.blockValue.damage >= hitInfo.hit.blockValue.Block.MaxDamage - 1)
			{
				blockValue = hitInfo.hit.blockValue;
				world.SetBlockRPC(vector3i, blockValue);
			}
			Block block = blockValue.Block;
			if (block == null)
			{
				return;
			}
			if (blockValue.ischild)
			{
				vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
				blockValue = chunkCluster.GetBlock(vector3i);
				block = blockValue.Block;
				if (block == null)
				{
					return;
				}
			}
			if (blockValue.isair)
			{
				return;
			}
			float num = world.GetLandProtectionHardnessModifier(hitInfo.hit.blockPos, entityAlive, world.GetGameManager().GetPersistentLocalPlayer());
			if (world.IsWithinTraderArea(hitInfo.hit.blockPos))
			{
				num = 0f;
			}
			if (world.InBoundsForPlayersPercent(hitInfo.hit.blockPos.ToVector3CenterXZ()) < 0.5f)
			{
				num = 0f;
			}
			if (!block.blockMaterial.CanDestroy)
			{
				num = 0f;
			}
			if (num != 1f)
			{
				if ((bool)entityAlive && _attackMode != EnumAttackMode.Simulate && entityAlive is EntityPlayer && !damagingItemValue.ItemClass.ignoreKeystoneSound && !damagingItemValue.ToBlockValue().Block.IgnoreKeystoneOverlay)
				{
					entityAlive.PlayOneShot("keystone_impact_overlay");
				}
				if (num < 1f)
				{
					flag2 = true;
				}
			}
			if (vector3i != _attackDetails.hitPosition || num != _attackDetails.hardnessScale || blockValue.type != _attackDetails.blockBeingDamaged.type || (flag3 && damagingItemValue.SelectedAmmoTypeIndex != _attackDetails.ammoIndex))
			{
				float num2 = Mathf.Max(block.GetHardness(), 0.1f) * num;
				float num3 = _blockDamage * (_damageMultiplier?.Get(block.blockMaterial.DamageCategory) ?? 1f);
				if ((bool)entityAlive)
				{
					num3 *= entityAlive.GetBlockDamageScale();
				}
				if (_toolBonuses != null && _toolBonuses.Count > 0)
				{
					num3 *= calculateHarvestToolDamageBonus(_toolBonuses, block.itemsToDrop);
					_attackDetails.bHarvestTool = true;
				}
				_attackDetails.damagePerHit = ((!flag2) ? (num3 / num2) : 0f);
				_attackDetails.damage = 0f;
				_attackDetails.hardnessScale = num;
				_attackDetails.hitPosition = vector3i;
				_attackDetails.blockBeingDamaged = blockValue;
				if (flag3)
				{
					_attackDetails.ammoIndex = damagingItemValue.SelectedAmmoTypeIndex;
				}
			}
			_attackDetails.raycastHitPosition = hitInfo.hit.blockPos;
			Block block2 = hitInfo.fmcHit.blockValue.Block;
			lightValue = world.GetLightBrightness(hitInfo.fmcHit.blockPos);
			color = block2.GetColorForSide(hitInfo.fmcHit.blockValue, hitInfo.fmcHit.blockFace);
			text = block2.GetParticleForSide(hitInfo.fmcHit.blockValue, hitInfo.fmcHit.blockFace);
			MaterialBlock materialForSide = block2.GetMaterialForSide(hitInfo.fmcHit.blockValue, hitInfo.fmcHit.blockFace);
			text2 = materialForSide.SurfaceCategory;
			float num4 = _attackDetails.damagePerHit * _staminaDamageMultiplier;
			if ((bool)entityAlive)
			{
				string str = materialForSide.DamageCategory ?? string.Empty;
				num4 = (int)EffectManager.GetValue(PassiveEffects.DamageModifier, damagingItemValue, num4, entityAlive, null, FastTags<TagGroup.Global>.Parse(str) | _attackDetails.WeaponTypeTag | hitInfo.fmcHit.blockValue.Block.Tags);
			}
			num4 = DegradationModifier(num4, _weaponCondition);
			num4 = ((!flag2 && !materialForSide.CheckDamageIgnore(damagingItemValue.ItemClass.ItemTags, entityAlive)) ? Utils.FastMax(1f, num4) : 0f);
			_attackDetails.damage += num4;
			_attackDetails.bKilled = false;
			_attackDetails.damageTotalOfTarget = (float)blockValue.damage + _attackDetails.damage;
			if (_attackDetails.damage > 0f)
			{
				Vector3 _hitFaceCenter;
				Vector3 _hitFaceNormal;
				BlockFace blockFaceFromHitInfo = GameUtils.GetBlockFaceFromHitInfo(vector3i, blockValue, hitInfo.hitCollider, hitInfo.hitTriangleIdx, out _hitFaceCenter, out _hitFaceNormal);
				int blockFaceTexture = chunkCluster.GetBlockFaceTexture(vector3i, blockFaceFromHitInfo, 0);
				int damage = blockValue.damage;
				bool flag5 = damage >= block.MaxDamage;
				int entityIdThatDamaged = ((ownedEntityId != -1 && ownedEntityId != -2) ? ownedEntityId : _attackerEntityId);
				int num5 = ((_attackMode != EnumAttackMode.Simulate) ? block.DamageBlock(world, 0, vector3i, blockValue, (int)_attackDetails.damage, entityIdThatDamaged, _attackDetails, _attackDetails.bHarvestTool) : 0);
				if (num5 == 0)
				{
					_attackDetails.damage = 0f;
				}
				else
				{
					_attackDetails.damage -= num5 - damage;
				}
				if (_attackMode != EnumAttackMode.Simulate && flag && entityAlive is EntityPlayerLocal && blockFaceTexture > 0 && block.MeshIndex == 0 && (float)num5 >= (float)block.MaxDamage * 1f)
				{
					ParticleEffect particleEffect = new ParticleEffect("paint_block", hitInfo.fmcHit.pos - Origin.position, Utils.BlockFaceToRotation(hitInfo.fmcHit.blockFace), lightValue, color, null, null);
					particleEffect.opqueTextureId = BlockTextureData.list[blockFaceTexture].TextureID;
					GameManager.Instance.SpawnParticleEffectClient(particleEffect, _attackerEntityId);
				}
				_attackDetails.damageGiven = ((!flag5) ? (num5 - damage) : 0);
				_attackDetails.damageMax = block.MaxDamage;
				_attackDetails.bKilled = !flag5 && num5 >= block.MaxDamage;
				_attackDetails.itemsToDrop = block.itemsToDrop;
				_attackDetails.bBlockHit = true;
				_attackDetails.materialCategory = block.blockMaterial.SurfaceCategory;
				if (entityAlive != null && _attackMode != EnumAttackMode.Simulate)
				{
					entityAlive.MinEventContext.ItemValue = damagingItemValue;
					entityAlive.MinEventContext.BlockValue = blockValue;
					entityAlive.MinEventContext.Tags = block.Tags;
					if (_attackDetails.bKilled)
					{
						entityAlive.FireEvent(MinEventTypes.onSelfDestroyedBlock, flag3);
						entityAlive.NotifyDestroyedBlock(_attackDetails);
					}
					else
					{
						entityAlive.FireEvent(MinEventTypes.onSelfDamagedBlock, flag3);
					}
				}
			}
		}
		else if (hitInfo.tag.StartsWith("E_"))
		{
			bool flag6 = ownedEntityId == -2;
			string bodyPartName;
			Entity entity = FindHitEntityNoTagCheck(hitInfo, out bodyPartName);
			if (entity == null || (!flag6 && entity.entityId == _attackerEntityId) || !entity.CanDamageEntity(_attackerEntityId))
			{
				return;
			}
			EntityAlive entityAlive2 = entity as EntityAlive;
			Vector2 uvHit = Vector2.zero;
			MeshCollider meshCollider = Voxel.phyxRaycastHit.collider as MeshCollider;
			if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.HasVertexAttribute(VertexAttribute.TexCoord0))
			{
				uvHit = Voxel.phyxRaycastHit.textureCoord;
			}
			DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, _damageType, _attackerEntityId, hitInfo.ray.direction, hitInfo.transform.name, hitInfo.hit.pos, uvHit);
			damageSourceEntity.AttackingItem = damagingItemValue;
			damageSourceEntity.DismemberChance = _dismemberChance;
			damageSourceEntity.CreatorEntityId = ownedEntityId;
			bool isCriticalHit = _attackDetails.isCriticalHit;
			int num6 = (int)_entityDamage;
			if (entityAlive != null && entityAlive2 != null)
			{
				FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
				if (entityAlive2.Health > 0)
				{
					fastTags = FastTags<TagGroup.Global>.Parse(damageSourceEntity.GetEntityDamageEquipmentSlotGroup(entityAlive2).ToStringCached());
				}
				num6 = (int)EffectManager.GetValue(PassiveEffects.DamageModifier, damagingItemValue, num6, entityAlive, null, fastTags | _attackDetails.WeaponTypeTag | entityAlive2.EntityClass.Tags);
				num6 = (int)EffectManager.GetValue(PassiveEffects.InternalDamageModifier, damagingItemValue, num6, entityAlive2, null, fastTags | damagingItemValue.ItemClass.ItemTags);
			}
			if (!entityAlive2 || entityAlive2.Health > 0)
			{
				num6 = Utils.FastMax(1, difficultyModifier(num6, world.GetEntity(_attackerEntityId), entity));
			}
			else if (_toolBonuses != null)
			{
				num6 = (int)((float)num6 * calculateHarvestToolDamageBonus(_toolBonuses, EntityClass.list[entity.entityClass].itemsToDrop));
			}
			bool flag7 = entity.IsDead();
			int num7 = ((entityAlive2 != null) ? entityAlive2.DeathHealth : 0);
			if (_attackMode != EnumAttackMode.Simulate)
			{
				if (entityAlive != null)
				{
					MinEventParams minEventContext = entityAlive.MinEventContext;
					minEventContext.Other = entityAlive2;
					minEventContext.ItemValue = damagingItemValue;
					minEventContext.StartPosition = hitInfo.ray.origin;
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (entityAlive as EntityPlayer == null || !entityAlive.isEntityRemote) && entity.isEntityRemote && rangeCheckedAction != null)
				{
					EntityPlayer entityPlayer = entity as EntityPlayer;
					if (entityPlayer != null)
					{
						flag4 = false;
						Ray lookRay = entityAlive.GetLookRay();
						lookRay.origin -= lookRay.direction * 0.15f;
						float maxRange = Utils.FastMax(rangeCheckedAction.Range, rangeCheckedAction.BlockRange) * attackRangeMultiplier;
						string buffActionsContext = null;
						List<string> list = _buffActions;
						if (list != null)
						{
							if ((bool)entityAlive2)
							{
								buffActionsContext = ((bodyPartName != null) ? GameUtils.GetChildTransformPath(entity.transform, hitInfo.transform) : null);
							}
							else
							{
								list = null;
							}
						}
						if (entityAlive != null)
						{
							entityAlive.FireEvent(MinEventTypes.onSelfAttackedOther, flag3);
							if (entityAlive2 != null && entityAlive2.RecordedDamage.Strength > 0)
							{
								entityAlive.FireEvent(MinEventTypes.onSelfDamagedOther, flag3);
							}
						}
						if (!flag7 && entity.IsDead() && entityAlive != null)
						{
							entityAlive.FireEvent(MinEventTypes.onSelfKilledOther, flag3);
						}
						text2 = ((!entityAlive2 || !entityAlive2.CanUseHeavyArmorSound() || entityAlive2.RecordedDamage.ArmorDamage <= entityAlive2.RecordedDamage.Strength) ? EntityClass.list[entity.entityClass].Properties.Values["SurfaceCategory"] : "metal");
						text = text2;
						lightValue = entity.GetLightBrightness();
						string name = $"impact_{_attackingDeviceMadeOf}_on_{text}";
						string soundName = ((text2 != null) ? $"{_attackingDeviceMadeOf}hit{text2}" : null);
						if (_hitSoundOverrides != null && _hitSoundOverrides.ContainsKey(text2))
						{
							soundName = _hitSoundOverrides[text2];
						}
						ParticleEffect particleEffect2 = new ParticleEffect(name, hitInfo.fmcHit.pos, Utils.BlockFaceToRotation(hitInfo.fmcHit.blockFace), lightValue, color, soundName, null);
						entityPlayer.ServerNetSendRangeCheckedDamage(lookRay.origin, maxRange, damageSourceEntity, num6, isCriticalHit, list, buffActionsContext, particleEffect2);
					}
				}
				if (flag4)
				{
					int num8 = entity.DamageEntity(damageSourceEntity, num6, isCriticalHit);
					if (num8 != -1 && (bool)entityAlive)
					{
						MinEventParams minEventContext2 = entityAlive.MinEventContext;
						minEventContext2.Other = entityAlive2;
						minEventContext2.ItemValue = damagingItemValue;
						minEventContext2.StartPosition = hitInfo.ray.origin;
						if (ownedEntityId != -1)
						{
							damagingItemValue.FireEvent(MinEventTypes.onSelfAttackedOther, entityAlive.MinEventContext);
						}
						entityAlive.FireEvent(MinEventTypes.onSelfAttackedOther, flag3);
						if ((bool)entityAlive2 && entityAlive2.RecordedDamage.Strength > 0)
						{
							entityAlive.FireEvent(MinEventTypes.onSelfDamagedOther, flag3);
						}
					}
					if (!flag7 && entity.IsDead() && (bool)entityAlive)
					{
						entityAlive.FireEvent(MinEventTypes.onSelfKilledOther, flag3);
					}
					if (num8 != -1 && (bool)entityAlive2 && _buffActions != null && _buffActions.Count > 0)
					{
						for (int i = 0; i < _buffActions.Count; i++)
						{
							BuffClass buff = BuffManager.GetBuff(_buffActions[i]);
							if (buff != null)
							{
								float originalValue = 1f;
								originalValue = EffectManager.GetValue(PassiveEffects.BuffProcChance, null, originalValue, entityAlive, null, FastTags<TagGroup.Global>.Parse(buff.Name));
								if (entityAlive2.rand.RandomFloat <= originalValue)
								{
									entityAlive2.Buffs.AddBuff(_buffActions[i], entityAlive.entityId);
								}
							}
						}
					}
				}
			}
			text2 = ((!entityAlive2 || !entityAlive2.CanUseHeavyArmorSound() || entityAlive2.RecordedDamage.ArmorDamage <= entityAlive2.RecordedDamage.Strength) ? EntityClass.list[entity.entityClass].Properties.Values["SurfaceCategory"] : "metal");
			text = text2;
			lightValue = entity.GetLightBrightness();
			EntityPlayer entityPlayer2 = entityAlive as EntityPlayer;
			if ((bool)entityPlayer2)
			{
				if (flag7 && entity.IsDead() && (bool)entityAlive2 && entityAlive2.DeathHealth + num6 > -1 * EntityClass.list[entity.entityClass].DeadBodyHitPoints)
				{
					_attackDetails.damageTotalOfTarget = -1 * entityAlive2.DeathHealth;
					_attackDetails.damageGiven = num7 + Mathf.Min(EntityClass.list[entity.entityClass].DeadBodyHitPoints, Mathf.Abs(entityAlive2.DeathHealth));
					_attackDetails.damageMax = EntityClass.list[entity.entityClass].DeadBodyHitPoints;
					_attackDetails.bKilled = -1 * entityAlive2.DeathHealth >= EntityClass.list[entity.entityClass].DeadBodyHitPoints;
					_attackDetails.itemsToDrop = EntityClass.list[entity.entityClass].itemsToDrop;
					_attackDetails.entityHit = entity;
					_attackDetails.materialCategory = text2;
				}
				if (!flag7 && (entityAlive2.IsDead() || entityAlive2.Health <= 0) && EntityClass.list.ContainsKey(entity.entityClass))
				{
					if ((_flags & 2) > 0)
					{
						float value = EffectManager.GetValue(PassiveEffects.ElectricalTrapXP, entityPlayer2.inventory.holdingItemItemValue, 0f, entityPlayer2);
						if (value > 0f)
						{
							entityPlayer2.AddKillXP(entityAlive2, value);
						}
					}
					else
					{
						entityPlayer2.AddKillXP(entityAlive2);
					}
				}
			}
			if (entity is EntityDrone)
			{
				_attackDetails.entityHit = entity;
			}
		}
		if ((_flags & 8) > 0)
		{
			flag = false;
		}
		if (flag4 && _attackMode != EnumAttackMode.Simulate && flag && text != null && ((_attackDetails.bBlockHit && !_attackDetails.bKilled) || !_attackDetails.bBlockHit))
		{
			string text3 = $"impact_{_attackingDeviceMadeOf}_on_{text}";
			if (_attackMode == EnumAttackMode.RealAndHarvesting && (_flags & 4) > 0 && ParticleEffect.IsAvailable(text3 + "_harvest"))
			{
				text3 += "_harvest";
			}
			string soundName2 = ((text2 != null) ? $"{_attackingDeviceMadeOf}hit{text2}" : null);
			if (_hitSoundOverrides != null && _hitSoundOverrides.ContainsKey(text2))
			{
				soundName2 = _hitSoundOverrides[text2];
			}
			world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(text3, hitInfo.fmcHit.pos, Utils.BlockFaceToRotation(hitInfo.fmcHit.blockFace), lightValue, color, soundName2, null), _attackerEntityId, _forceCreation: false, _worldSpawn: true);
		}
		if ((_flags & 1) > 0 && entityAlive != null && entityAlive.inventory != null)
		{
			entityAlive.inventory.CallOnToolbeltChangedInternal();
		}
	}

	public static BlockValue GetBlockHit(World _world, WorldRayHitInfo hitInfo)
	{
		if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
		{
			BlockValue air = BlockValue.Air;
			Vector3i blockPos = hitInfo.hit.blockPos;
			ChunkCluster chunkCluster = _world.ChunkClusters[hitInfo.hit.clrIdx];
			if (chunkCluster == null)
			{
				return BlockValue.Air;
			}
			air = chunkCluster.GetBlock(blockPos);
			if (air.isair && hitInfo.hit.blockValue.Block.IsDistantDecoration && hitInfo.hit.blockValue.damage >= hitInfo.hit.blockValue.Block.MaxDamage - 1)
			{
				air = hitInfo.hit.blockValue;
			}
			if (air.Block == null)
			{
				return BlockValue.Air;
			}
			if (air.ischild)
			{
				blockPos = air.Block.multiBlockPos.GetParentPos(blockPos, air);
				air = chunkCluster.GetBlock(blockPos);
				if (air.Block == null)
				{
					return BlockValue.Air;
				}
			}
			if (air.Equals(BlockValue.Air))
			{
				return BlockValue.Air;
			}
			return air;
		}
		return BlockValue.Air;
	}

	public static Entity FindHitEntity(WorldRayHitInfo hitInfo)
	{
		if (!hitInfo.tag.StartsWith("E_"))
		{
			return null;
		}
		string bodyPartName;
		return FindHitEntityNoTagCheck(hitInfo, out bodyPartName);
	}

	public static Entity FindHitEntityNoTagCheck(WorldRayHitInfo hitInfo, out string bodyPartName)
	{
		Transform transform = hitInfo.transform;
		bodyPartName = null;
		if (hitInfo.tag.StartsWith("E_BP_"))
		{
			bodyPartName = hitInfo.tag.Substring("E_BP_".Length).ToLower();
			transform = RootTransformRefEntity.FindEntityUpwards(hitInfo.transform);
		}
		if (transform == null)
		{
			return null;
		}
		Entity entity = transform.GetComponent<Entity>();
		if (entity == null && hitInfo.tag.StartsWith("E_Vehicle"))
		{
			entity = GameUtils.GetHitRootEntity(hitInfo.tag, hitInfo.transform);
		}
		return entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float calculateHarvestToolDamageBonus(Dictionary<string, Bonuses> _toolBonuses, Dictionary<EnumDropEvent, List<Block.SItemDropProb>> _harvestItems)
	{
		if (!_harvestItems.ContainsKey(EnumDropEvent.Harvest))
		{
			return 1f;
		}
		List<Block.SItemDropProb> list = _harvestItems[EnumDropEvent.Harvest];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].toolCategory != null && _toolBonuses.ContainsKey(list[i].toolCategory))
			{
				return _toolBonuses[list[i].toolCategory].Damage;
			}
		}
		return 1f;
	}

	public static float DegradationModifier(float _strength, float _condition)
	{
		return Mathf.Lerp(_strength * 0.5f, _strength, (_condition < 0.5f) ? (_condition + 0.5f) : 1f);
	}

	public static float StaminaModifier(float _stamina)
	{
		return _stamina;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int difficultyModifier(int _strength, Entity _attacker, Entity _target)
	{
		if (_attacker == null || _target == null)
		{
			return _strength;
		}
		if (_attacker.IsClientControlled() && _target.IsClientControlled())
		{
			return _strength;
		}
		if (!_attacker.IsClientControlled() && !_target.IsClientControlled())
		{
			return _strength;
		}
		int num = GameStats.GetInt(EnumGameStats.GameDifficulty);
		if (_attacker.IsClientControlled())
		{
			switch (num)
			{
			case 0:
				_strength = Mathf.RoundToInt((float)_strength * 2f);
				break;
			case 1:
				_strength = Mathf.RoundToInt((float)_strength * 1.5f);
				break;
			case 3:
				_strength = Mathf.RoundToInt((float)_strength * 0.83f);
				break;
			case 4:
				_strength = Mathf.RoundToInt((float)_strength * 0.66f);
				break;
			case 5:
				_strength = Mathf.RoundToInt((float)_strength * 0.5f);
				break;
			}
		}
		else
		{
			switch (num)
			{
			case 0:
				_strength = Mathf.RoundToInt((float)_strength * 0.5f);
				break;
			case 1:
				_strength = Mathf.RoundToInt((float)_strength * 0.75f);
				break;
			case 3:
				_strength = Mathf.RoundToInt((float)_strength * 1.5f);
				break;
			case 4:
				_strength = Mathf.RoundToInt((float)_strength * 2f);
				break;
			case 5:
				_strength = Mathf.RoundToInt((float)_strength * 2.5f);
				break;
			}
		}
		return _strength;
	}

	public virtual bool ShowAmmoInUI()
	{
		return BulletsPerMagazine > 0;
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		float num = 1f;
		switch (GameStats.GetInt(EnumGameStats.GameDifficulty))
		{
		case 0:
			num = 2f;
			break;
		case 1:
			num = 1.5f;
			break;
		case 2:
			num = 1f;
			break;
		case 3:
			num = 0.83f;
			break;
		case 4:
			num = 0.66f;
			break;
		case 5:
			num = 0.5f;
			break;
		}
		float num2 = GetDamageEntity(_itemValue, _xui.playerUI.entityPlayer, _actionIndex) * num;
		float num3 = GetDamageBlock(_itemValue, BlockValue.Air, _xui.playerUI.entityPlayer, _actionIndex) * num;
		if (num2 > 0f)
		{
			_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblEntDmg"), num2.ToCultureInvariantString("0")));
		}
		if (num3 > 0f)
		{
			_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblBlkDmg"), num3.ToCultureInvariantString("0")));
		}
		ItemAction.BuffActionStrings(this, _infoList);
	}

	public override bool HasRadial()
	{
		if (MagazineItemNames != null)
		{
			return MagazineItemNames.Length != 0;
		}
		return false;
	}

	public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
		_xuiRadialWindow.ResetRadialEntries();
		string[] magazineItemNames = _epl.inventory.GetHoldingGun().MagazineItemNames;
		int preSelectedCommandIndex = -1;
		for (int i = 0; i < magazineItemNames.Length; i++)
		{
			ItemClass itemClass = ItemClass.GetItemClass(magazineItemNames[i]);
			if (itemClass != null && (!_epl.isHeadUnderwater || itemClass.UsableUnderwater))
			{
				int itemCount = _xuiRadialWindow.xui.PlayerInventory.GetItemCount(itemClass.Id);
				bool flag = _epl.inventory.holdingItemItemValue.SelectedAmmoTypeIndex == i;
				_xuiRadialWindow.CreateRadialEntry(i, itemClass.GetIconName(), (itemCount > 0) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale", itemCount.ToString(), itemClass.GetLocalizedItemName(), flag);
				if (flag)
				{
					preSelectedCommandIndex = i;
				}
			}
		}
		_xuiRadialWindow.SetCommonData(UIUtils.GetButtonIconForAction(_epl.playerInput.Reload), handleRadialCommand, new RadialContextItem((ItemActionRanged)_epl.inventory.GetHoldingGun()), preSelectedCommandIndex, _hasSpecialActionPriorToRadialVisibility: false, radialValidTest);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool radialValidTest(XUiC_Radial _sender, XUiC_Radial.RadialContextAbs _context)
	{
		if (!(_context is RadialContextItem radialContextItem))
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		return radialContextItem.RangedItemAction == entityPlayer.inventory.GetHoldingGun();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		if (_context is RadialContextItem radialContextItem)
		{
			EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
			if (radialContextItem.RangedItemAction == entityPlayer.inventory.GetHoldingGun())
			{
				radialContextItem.RangedItemAction.SwapSelectedAmmo(entityPlayer, _commandIndex);
			}
		}
	}
}
