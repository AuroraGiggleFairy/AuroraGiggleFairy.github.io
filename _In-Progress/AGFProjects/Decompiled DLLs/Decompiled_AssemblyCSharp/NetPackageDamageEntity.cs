using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDamageEntity : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackerEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageSource damageSrc;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageTypes damageTyp;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort strength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hitDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	public short hitBodyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int movementState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 dirV;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hitTransformName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hitTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 uvHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public float random;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIgnoreConsecutiveDamages;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPainHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFatal;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCritical;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIsDamageTransfer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDismember;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCrippleLegs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bTurnIntoCrawler;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte bonusDamageType;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte StunType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float StunDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots ArmorSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlotGroups ArmorSlotGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ArmorDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue attackingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFromBuff;

	public NetPackageDamageEntity Setup(int _targetEntityId, DamageResponse _dmResponse)
	{
		entityId = _targetEntityId;
		DamageSource source = _dmResponse.Source;
		damageSrc = source.GetSource();
		damageTyp = source.GetDamageType();
		attackingItem = source.AttackingItem;
		int num = _dmResponse.Strength;
		if (num > 65535)
		{
			num = 65535;
		}
		strength = (ushort)num;
		hitDirection = (int)_dmResponse.HitDirection;
		hitBodyPart = (short)_dmResponse.HitBodyPart;
		movementState = _dmResponse.MovementState;
		bPainHit = _dmResponse.PainHit;
		bFatal = _dmResponse.Fatal;
		bCritical = _dmResponse.Critical;
		attackerEntityId = source.getEntityId();
		dirV = source.getDirection();
		blockPos = source.BlockPosition;
		hitTransformName = source.getHitTransformName() ?? string.Empty;
		hitTransformPosition = source.getHitTransformPosition();
		uvHit = source.getUVHit();
		damageMultiplier = source.DamageMultiplier;
		bonusDamageType = (byte)source.BonusDamageType;
		random = _dmResponse.Random;
		bIgnoreConsecutiveDamages = source.IsIgnoreConsecutiveDamages();
		bDismember = _dmResponse.Dismember;
		bCrippleLegs = _dmResponse.CrippleLegs;
		bTurnIntoCrawler = _dmResponse.TurnIntoCrawler;
		StunType = (byte)_dmResponse.Stun;
		StunDuration = _dmResponse.StunDuration;
		ArmorSlot = _dmResponse.ArmorSlot;
		ArmorSlotGroup = _dmResponse.ArmorSlotGroup;
		ArmorDamage = _dmResponse.ArmorDamage;
		bFromBuff = source.BuffClass != null;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		damageSrc = (EnumDamageSource)_reader.ReadByte();
		damageTyp = (EnumDamageTypes)_reader.ReadByte();
		strength = _reader.ReadUInt16();
		hitDirection = _reader.ReadByte();
		hitBodyPart = _reader.ReadInt16();
		movementState = _reader.ReadByte();
		bPainHit = _reader.ReadBoolean();
		bFatal = _reader.ReadBoolean();
		bCritical = _reader.ReadBoolean();
		attackerEntityId = _reader.ReadInt32();
		dirV.x = _reader.ReadSingle();
		dirV.y = _reader.ReadSingle();
		dirV.z = _reader.ReadSingle();
		blockPos = StreamUtils.ReadVector3i(_reader);
		hitTransformName = _reader.ReadString();
		hitTransformPosition.x = _reader.ReadSingle();
		hitTransformPosition.y = _reader.ReadSingle();
		hitTransformPosition.z = _reader.ReadSingle();
		uvHit.x = _reader.ReadSingle();
		uvHit.y = _reader.ReadSingle();
		damageMultiplier = _reader.ReadSingle();
		random = _reader.ReadSingle();
		bIgnoreConsecutiveDamages = _reader.ReadBoolean();
		bIsDamageTransfer = _reader.ReadBoolean();
		bDismember = _reader.ReadBoolean();
		bCrippleLegs = _reader.ReadBoolean();
		bTurnIntoCrawler = _reader.ReadBoolean();
		bonusDamageType = _reader.ReadByte();
		StunType = _reader.ReadByte();
		StunDuration = _reader.ReadSingle();
		bFromBuff = _reader.ReadBoolean();
		ArmorSlot = (EquipmentSlots)_reader.ReadByte();
		ArmorSlotGroup = (EquipmentSlotGroups)_reader.ReadByte();
		ArmorDamage = _reader.ReadInt16();
		if (_reader.ReadBoolean())
		{
			attackingItem = new ItemValue();
			attackingItem.Read(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((byte)damageSrc);
		_writer.Write((byte)damageTyp);
		_writer.Write(strength);
		_writer.Write((byte)hitDirection);
		_writer.Write(hitBodyPart);
		_writer.Write((byte)movementState);
		_writer.Write(bPainHit);
		_writer.Write(bFatal);
		_writer.Write(bCritical);
		_writer.Write(attackerEntityId);
		_writer.Write(dirV.x);
		_writer.Write(dirV.y);
		_writer.Write(dirV.z);
		StreamUtils.Write(_writer, blockPos);
		_writer.Write(hitTransformName);
		_writer.Write(hitTransformPosition.x);
		_writer.Write(hitTransformPosition.y);
		_writer.Write(hitTransformPosition.z);
		_writer.Write(uvHit.x);
		_writer.Write(uvHit.y);
		_writer.Write(damageMultiplier);
		_writer.Write(random);
		_writer.Write(bIgnoreConsecutiveDamages);
		_writer.Write(bIsDamageTransfer);
		_writer.Write(bDismember);
		_writer.Write(bCrippleLegs);
		_writer.Write(bTurnIntoCrawler);
		_writer.Write(bonusDamageType);
		_writer.Write(StunType);
		_writer.Write(StunDuration);
		_writer.Write(bFromBuff);
		_writer.Write((byte)ArmorSlot);
		_writer.Write((byte)ArmorSlotGroup);
		_writer.Write((ushort)ArmorDamage);
		_writer.Write(attackingItem != null);
		if (attackingItem != null)
		{
			attackingItem.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || (_world.GetPrimaryPlayer() != null && _world.GetPrimaryPlayer().entityId == entityId && (damageTyp == EnumDamageTypes.Falling || (damageSrc == EnumDamageSource.External && (damageTyp == EnumDamageTypes.Piercing || damageTyp == EnumDamageTypes.BarbedWire) && attackerEntityId == -1))))
		{
			return;
		}
		Entity entity = _world.GetEntity(entityId);
		if (entity != null)
		{
			DamageSource damageSource = new DamageSourceEntity(damageSrc, damageTyp, attackerEntityId, dirV, hitTransformName, hitTransformPosition, uvHit);
			damageSource.SetIgnoreConsecutiveDamages(bIgnoreConsecutiveDamages);
			damageSource.DamageMultiplier = damageMultiplier;
			damageSource.BonusDamageType = (EnumDamageBonusType)bonusDamageType;
			damageSource.AttackingItem = attackingItem;
			damageSource.BlockPosition = blockPos;
			DamageResponse dmResponse = new DamageResponse
			{
				Strength = strength,
				ModStrength = 0,
				MovementState = movementState,
				HitDirection = (Utils.EnumHitDirection)hitDirection,
				HitBodyPart = (EnumBodyPartHit)hitBodyPart,
				PainHit = bPainHit,
				Fatal = bFatal,
				Critical = bCritical,
				Random = random,
				Source = damageSource,
				CrippleLegs = bCrippleLegs,
				Dismember = bDismember,
				TurnIntoCrawler = bTurnIntoCrawler,
				Stun = (EnumEntityStunType)StunType,
				StunDuration = StunDuration,
				ArmorSlot = ArmorSlot,
				ArmorSlotGroup = ArmorSlotGroup,
				ArmorDamage = ArmorDamage
			};
			if (bFromBuff)
			{
				dmResponse.Source.BuffClass = new BuffClass();
			}
			entity.FireAttackedEvents(dmResponse);
			entity.ProcessDamageResponse(dmResponse);
		}
	}

	public override int GetLength()
	{
		return 50;
	}
}
