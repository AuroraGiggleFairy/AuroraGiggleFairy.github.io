using UnityEngine;

public class DamageSourceEntity : DamageSource
{
	public Vector2 uvHit;

	public string hitTransformName;

	public Vector3 hitTransformPosition;

	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId)
		: base(_damageSource, _damageType)
	{
		ownerEntityId = _damageSourceEntityId;
	}

	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId, Vector3 _direction)
		: base(_damageSource, _damageType, _direction)
	{
		ownerEntityId = _damageSourceEntityId;
	}

	public DamageSourceEntity(EnumDamageSource _damageSource, EnumDamageTypes _damageType, int _damageSourceEntityId, Vector3 _direction, string _hitTransformName, Vector3 _hitTransformPosition, Vector2 _uvHit)
		: this(_damageSource, _damageType, _damageSourceEntityId, _direction)
	{
		hitTransformName = _hitTransformName;
		hitTransformPosition = _hitTransformPosition;
		uvHit = _uvHit;
	}

	public override Vector3 getHitTransformPosition()
	{
		return hitTransformPosition;
	}

	public override string getHitTransformName()
	{
		return hitTransformName;
	}

	public override Vector2 getUVHit()
	{
		return uvHit;
	}
}
