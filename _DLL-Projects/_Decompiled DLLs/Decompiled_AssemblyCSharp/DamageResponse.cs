public struct DamageResponse
{
	public DamageSource Source;

	public int Strength;

	public int ModStrength;

	public int MovementState;

	public Utils.EnumHitDirection HitDirection;

	public EnumBodyPartHit HitBodyPart;

	public bool PainHit;

	public bool Fatal;

	public bool Critical;

	public bool Dismember;

	public bool CrippleLegs;

	public bool TurnIntoCrawler;

	public float Random;

	public float ImpulseScale;

	public EnumEntityStunType Stun;

	public float StunDuration;

	public EquipmentSlots ArmorSlot;

	public EquipmentSlotGroups ArmorSlotGroup;

	public int ArmorDamage;

	public static DamageResponse New(bool _fatal)
	{
		return new DamageResponse
		{
			HitBodyPart = EnumBodyPartHit.Torso,
			Random = GameManager.Instance.World.GetGameRandom().RandomFloat,
			Fatal = _fatal,
			PainHit = !_fatal,
			ImpulseScale = 1f,
			ArmorSlot = EquipmentSlots.Count,
			ArmorDamage = 0
		};
	}

	public static DamageResponse New(DamageSource _source, bool _fatal)
	{
		return new DamageResponse
		{
			HitBodyPart = EnumBodyPartHit.Torso,
			Random = GameManager.Instance.World.GetGameRandom().RandomFloat,
			Fatal = _fatal,
			PainHit = !_fatal,
			Source = _source,
			ImpulseScale = 1f,
			ArmorSlot = EquipmentSlots.Count,
			ArmorDamage = 0
		};
	}
}
