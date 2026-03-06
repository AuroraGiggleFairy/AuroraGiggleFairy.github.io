using UnityEngine;

public class DamageSource
{
	public static readonly DamageSource eat = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Slashing);

	public static readonly DamageSource fallingBlock = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing);

	public static readonly DamageSource radiation = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Radiation);

	public static readonly DamageSource fall = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Falling);

	public static readonly DamageSource starve = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Starvation);

	public static readonly DamageSource dehydrate = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Dehydration);

	public static readonly DamageSource radiationSickness = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Radiation);

	public static readonly DamageSource disease = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Disease);

	public static readonly DamageSource suffocating = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suffocation);

	public BuffClass BuffClass;

	public ItemValue AttackingItem;

	public EnumDamageSource damageSource;

	public readonly EnumDamageTypes damageType;

	public EnumBodyPartHit bodyParts;

	public float DismemberChance;

	public EnumDamageBonusType BonusDamageType;

	public FastTags<TagGroup.Global> DamageTypeTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIgnoreConsecutiveDamages;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int ownerEntityId = -1;

	public int CreatorEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 direction;

	public Vector3i BlockPosition;

	public ItemClass ItemClass
	{
		get
		{
			if (AttackingItem != null)
			{
				return AttackingItem.ItemClass;
			}
			return null;
		}
	}

	public bool CanStun
	{
		get
		{
			if (damageType != EnumDamageTypes.Bashing && damageType != EnumDamageTypes.Heat && damageType != EnumDamageTypes.Piercing && damageType != EnumDamageTypes.Crushing)
			{
				return damageType == EnumDamageTypes.Falling;
			}
			return true;
		}
	}

	public float DamageMultiplier
	{
		get
		{
			return damageMultiplier;
		}
		set
		{
			damageMultiplier = value;
		}
	}

	public DamageSource(EnumDamageSource _dsn, EnumDamageTypes _damageType)
	{
		damageSource = _dsn;
		damageType = _damageType;
		DamageTypeTag = FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached());
	}

	public DamageSource(EnumDamageSource _dsn, EnumDamageTypes _damageType, Vector3 _direction)
	{
		damageSource = _dsn;
		damageType = _damageType;
		direction = _direction;
		DamageTypeTag = FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached());
	}

	public bool AffectedByArmor()
	{
		return damageSource == EnumDamageSource.External;
	}

	public EquipmentSlots GetEntityDamageEquipmentSlot(Entity entity)
	{
		if ((bool)entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if ((bool)hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					return EquipmentSlots.Head;
				}
				if ("E_BP_Body".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_LLeg".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_RLeg".Equals(tag))
				{
					return EquipmentSlots.Chest;
				}
				if ("E_BP_LArm".Equals(tag))
				{
					return EquipmentSlots.Hands;
				}
				if ("E_BP_RArm".Equals(tag))
				{
					return EquipmentSlots.Hands;
				}
			}
		}
		return EquipmentSlots.Count;
	}

	public EquipmentSlotGroups GetEntityDamageEquipmentSlotGroup(Entity entity)
	{
		if ((bool)entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if ((bool)hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					return EquipmentSlotGroups.Head;
				}
				if ("E_BP_Body".Equals(tag))
				{
					return EquipmentSlotGroups.UpperBody;
				}
				if ("E_BP_LLeg".Equals(tag))
				{
					return EquipmentSlotGroups.LowerBody;
				}
				if ("E_BP_RLeg".Equals(tag))
				{
					return EquipmentSlotGroups.LowerBody;
				}
				if ("E_BP_LArm".Equals(tag))
				{
					return EquipmentSlotGroups.UpperBody;
				}
				"E_BP_RArm".Equals(tag);
				return EquipmentSlotGroups.UpperBody;
			}
		}
		return EquipmentSlotGroups.UpperBody;
	}

	public EnumBodyPartHit GetEntityDamageBodyPart(Entity entity)
	{
		if (bodyParts != EnumBodyPartHit.None)
		{
			return bodyParts;
		}
		if ((bool)entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if ((bool)hitTransform)
			{
				return TagToBodyPart(hitTransform.tag);
			}
		}
		return EnumBodyPartHit.None;
	}

	public static EnumBodyPartHit TagToBodyPart(string _name)
	{
		return _name switch
		{
			"E_BP_Head" => EnumBodyPartHit.Head, 
			"E_BP_Body" => EnumBodyPartHit.Torso, 
			"E_BP_LLeg" => EnumBodyPartHit.LeftUpperLeg, 
			"E_BP_LLowerLeg" => EnumBodyPartHit.LeftLowerLeg, 
			"E_BP_RLeg" => EnumBodyPartHit.RightUpperLeg, 
			"E_BP_RLowerLeg" => EnumBodyPartHit.RightLowerLeg, 
			"E_BP_LArm" => EnumBodyPartHit.LeftUpperArm, 
			"E_BP_LLowerArm" => EnumBodyPartHit.LeftLowerArm, 
			"E_BP_RArm" => EnumBodyPartHit.RightUpperArm, 
			"E_BP_RLowerArm" => EnumBodyPartHit.RightLowerArm, 
			"E_BP_Special" => EnumBodyPartHit.Special, 
			_ => EnumBodyPartHit.None, 
		};
	}

	public void GetEntityDamageBodyPartAndEquipmentSlot(Entity entity, out EnumBodyPartHit bodyPartHit, out EquipmentSlots damageSlot)
	{
		damageSlot = EquipmentSlots.Count;
		bodyPartHit = EnumBodyPartHit.None;
		if ((bool)entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if ((bool)hitTransform)
			{
				string tag = hitTransform.tag;
				if ("E_BP_Head".Equals(tag))
				{
					damageSlot = EquipmentSlots.Head;
					bodyPartHit = EnumBodyPartHit.Head;
				}
				else if ("E_BP_Body".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.Torso;
				}
				else if ("E_BP_LLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.LeftUpperLeg;
				}
				else if ("E_BP_LLowerLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Feet;
					bodyPartHit = EnumBodyPartHit.LeftLowerLeg;
				}
				else if ("E_BP_RLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Chest;
					bodyPartHit = EnumBodyPartHit.RightUpperLeg;
				}
				else if ("E_BP_RLowerLeg".Equals(tag))
				{
					damageSlot = EquipmentSlots.Feet;
					bodyPartHit = EnumBodyPartHit.RightLowerLeg;
				}
				else if ("E_BP_LArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.LeftUpperArm;
				}
				else if ("E_BP_LLowerArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.LeftLowerArm;
				}
				else if ("E_BP_RArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.RightUpperArm;
				}
				else if ("E_BP_RLowerArm".Equals(tag))
				{
					damageSlot = EquipmentSlots.Hands;
					bodyPartHit = EnumBodyPartHit.RightLowerArm;
				}
			}
		}
		else if (damageType == EnumDamageTypes.Falling)
		{
			bodyPartHit = EnumBodyPartHit.RightLowerLeg;
			damageSlot = EquipmentSlots.Feet;
		}
		else
		{
			bodyPartHit = EnumBodyPartHit.Torso;
			damageSlot = EquipmentSlots.Chest;
		}
	}

	public EquipmentSlots GetDamagedEquipmentSlot(Entity entity)
	{
		if ((bool)entity.emodel)
		{
			Transform hitTransform = entity.emodel.GetHitTransform(this);
			if ((bool)hitTransform)
			{
				return hitTransform.tag switch
				{
					"E_BP_Head" => EquipmentSlots.Head, 
					"E_BP_Body" => EquipmentSlots.Chest, 
					"E_BP_LLeg" => EquipmentSlots.Chest, 
					"E_BP_LLowerLeg" => EquipmentSlots.Feet, 
					"E_BP_RLeg" => EquipmentSlots.Chest, 
					"E_BP_RLowerLeg" => EquipmentSlots.Feet, 
					"E_BP_LArm" => EquipmentSlots.Chest, 
					"E_BP_LLowerArm" => EquipmentSlots.Hands, 
					"E_BP_RArm" => EquipmentSlots.Chest, 
					"E_BP_RLowerArm" => EquipmentSlots.Hands, 
					_ => EquipmentSlots.Chest, 
				};
			}
		}
		else if (damageType == EnumDamageTypes.Falling)
		{
			return EquipmentSlots.Feet;
		}
		return EquipmentSlots.Chest;
	}

	public virtual Vector3 getDirection()
	{
		return direction;
	}

	public virtual int getEntityId()
	{
		return ownerEntityId;
	}

	public virtual string getHitTransformName()
	{
		return null;
	}

	public virtual Vector3 getHitTransformPosition()
	{
		return Vector3.zero;
	}

	public virtual Vector2 getUVHit()
	{
		return Vector2.zero;
	}

	public virtual EnumDamageSource GetSource()
	{
		return damageSource;
	}

	public virtual EnumDamageTypes GetDamageType()
	{
		return damageType;
	}

	public void SetIgnoreConsecutiveDamages(bool _b)
	{
		bIgnoreConsecutiveDamages = _b;
	}

	public virtual bool IsIgnoreConsecutiveDamages()
	{
		return bIgnoreConsecutiveDamages;
	}
}
