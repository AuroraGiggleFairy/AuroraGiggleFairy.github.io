using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRangeCheckDamageEntity : NetPackage
{
	public int entityId;

	public int attackerEntityId;

	public EnumDamageSource damageStr;

	public EnumDamageTypes damageTyp;

	public int strength;

	public Vector3 origin;

	public float maxRangeSq;

	public float dirX;

	public float dirY;

	public float dirZ;

	public string hitTransformName;

	public Vector3 hitTransformPosition;

	public float uvHitx;

	public float uvHity;

	public float damageMultiplier;

	public bool bIgnoreConsecutiveDamages;

	public bool bCritical;

	public bool bIsDamageTransfer;

	public byte bonusDamageType;

	public List<string> buffActions;

	public string buffActionContext;

	public string particleName;

	public Vector3 particlePos;

	public Vector3 particleRot;

	public float particleLight;

	public Color particleColor;

	public string particleSound;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageRangeCheckDamageEntity Setup(int _targetEntityId, Vector3 _origin, float _maxRange, DamageSourceEntity _damageSource, int _strength, bool _isCritical, List<string> _buffActions, string _buffActionContext, ParticleEffect particleEffect)
	{
		entityId = _targetEntityId;
		origin = _origin;
		maxRangeSq = _maxRange * _maxRange;
		strength = _strength;
		damageStr = _damageSource.GetSource();
		damageTyp = _damageSource.GetDamageType();
		bCritical = _isCritical;
		attackerEntityId = _damageSource.getEntityId();
		dirX = _damageSource.getDirection().x;
		dirY = _damageSource.getDirection().y;
		dirZ = _damageSource.getDirection().z;
		hitTransformName = ((_damageSource.getHitTransformName() != null) ? _damageSource.getHitTransformName() : string.Empty);
		hitTransformPosition = _damageSource.getHitTransformPosition();
		uvHitx = _damageSource.getUVHit().x;
		uvHity = _damageSource.getUVHit().y;
		damageMultiplier = _damageSource.DamageMultiplier;
		bonusDamageType = (byte)_damageSource.BonusDamageType;
		bIgnoreConsecutiveDamages = _damageSource.IsIgnoreConsecutiveDamages();
		buffActions = _buffActions;
		buffActionContext = _buffActionContext;
		particleName = particleEffect.debugName;
		particlePos = particleEffect.pos;
		particleRot = particleEffect.rot.eulerAngles;
		particleLight = particleEffect.lightValue;
		particleColor = particleEffect.color;
		particleSound = particleEffect.soundName;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		damageStr = (EnumDamageSource)_reader.ReadByte();
		damageTyp = (EnumDamageTypes)_reader.ReadByte();
		origin = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		maxRangeSq = _reader.ReadSingle();
		strength = _reader.ReadInt16();
		bCritical = _reader.ReadBoolean();
		attackerEntityId = _reader.ReadInt32();
		dirX = _reader.ReadSingle();
		dirY = _reader.ReadSingle();
		dirZ = _reader.ReadSingle();
		hitTransformName = _reader.ReadString();
		hitTransformPosition = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		uvHitx = _reader.ReadSingle();
		uvHity = _reader.ReadSingle();
		damageMultiplier = _reader.ReadSingle();
		bIgnoreConsecutiveDamages = _reader.ReadBoolean();
		bIsDamageTransfer = _reader.ReadBoolean();
		bonusDamageType = _reader.ReadByte();
		particleName = _reader.ReadString();
		particlePos = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		particleRot = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		particleLight = _reader.ReadSingle();
		particleColor = new Color(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		particleSound = _reader.ReadString();
		int num = _reader.ReadByte();
		if (num > 0)
		{
			buffActions = new List<string>();
			for (int i = 0; i < num; i++)
			{
				buffActions.Add(_reader.ReadString());
			}
		}
		else
		{
			buffActions = null;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((byte)damageStr);
		_writer.Write((byte)damageTyp);
		_writer.Write(origin.x);
		_writer.Write(origin.y);
		_writer.Write(origin.z);
		_writer.Write(maxRangeSq);
		_writer.Write((short)strength);
		_writer.Write(bCritical);
		_writer.Write(attackerEntityId);
		_writer.Write(dirX);
		_writer.Write(dirY);
		_writer.Write(dirZ);
		_writer.Write(hitTransformName);
		_writer.Write(hitTransformPosition.x);
		_writer.Write(hitTransformPosition.y);
		_writer.Write(hitTransformPosition.z);
		_writer.Write(uvHitx);
		_writer.Write(uvHity);
		_writer.Write(damageMultiplier);
		_writer.Write(bIgnoreConsecutiveDamages);
		_writer.Write(bIsDamageTransfer);
		_writer.Write(bonusDamageType);
		_writer.Write(particleName);
		_writer.Write(particlePos.x);
		_writer.Write(particlePos.y);
		_writer.Write(particlePos.z);
		_writer.Write(particleRot.x);
		_writer.Write(particleRot.y);
		_writer.Write(particleRot.z);
		_writer.Write(particleLight);
		_writer.Write(particleColor.r);
		_writer.Write(particleColor.g);
		_writer.Write(particleColor.b);
		_writer.Write(particleColor.a);
		_writer.Write(particleSound);
		if (buffActions != null && buffActions.Count > 0)
		{
			_writer.Write((byte)buffActions.Count);
			for (int i = 0; i < buffActions.Count; i++)
			{
				_writer.Write(buffActions[i]);
			}
		}
		else
		{
			_writer.Write((byte)0);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(entityId);
		if (!(entity != null))
		{
			return;
		}
		Entity entity2 = _world.GetEntity(attackerEntityId);
		bool flag = false;
		if (entity2 == null)
		{
			flag = true;
		}
		else
		{
			Vector3 vector = entity.GetPosition() - entity2.GetPosition();
			float num = Vector3.Dot((entity2.transform.position - entity.transform.position).normalized, entity2.transform.forward);
			flag = vector.sqrMagnitude <= maxRangeSq && num < 0f;
		}
		if (flag)
		{
			DamageSource damageSource = new DamageSourceEntity(damageStr, damageTyp, attackerEntityId, new Vector3(dirX, dirY, dirZ), hitTransformName, hitTransformPosition, new Vector2(uvHitx, uvHity));
			damageSource.SetIgnoreConsecutiveDamages(bIgnoreConsecutiveDamages);
			damageSource.DamageMultiplier = damageMultiplier;
			damageSource.BonusDamageType = (EnumDamageBonusType)bonusDamageType;
			entity.DamageEntity(damageSource, strength, bCritical);
			if (buffActions != null)
			{
				ItemAction.ExecuteBuffActions(buffActions, attackerEntityId, entity as EntityAlive, bCritical, damageSource.GetEntityDamageBodyPart(entity), buffActionContext);
			}
			string.IsNullOrEmpty(particleName);
			_world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(particleName, particlePos, Quaternion.Euler(particleRot), particleLight, particleColor, particleSound, null), _world.GetPrimaryPlayerId());
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
