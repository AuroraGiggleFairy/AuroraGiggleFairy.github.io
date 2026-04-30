using System.Collections.Generic;
using System.IO;

public struct ExplosionData
{
	public const int cMaxBlastPower = 100;

	public int ParticleIndex;

	public float Duration;

	public float BlockRadius;

	public int EntityRadius;

	public int BlastPower;

	public float EntityDamage;

	public float BlockDamage;

	public string BlockTags;

	public bool IgnoreHeatMap;

	public EnumDamageTypes DamageType;

	public DamageMultiplier damageMultiplier;

	public List<string> BuffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PREFIX = "Explosion.";

	public ExplosionData(byte[] _explosionDataAsArr)
	{
		ParticleIndex = 0;
		Duration = 0f;
		BlockRadius = 0f;
		EntityRadius = 0;
		EntityDamage = 0f;
		BlockDamage = 0f;
		BlockTags = string.Empty;
		BlastPower = 100;
		damageMultiplier = null;
		BuffActions = null;
		IgnoreHeatMap = false;
		DamageType = EnumDamageTypes.Heat;
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(new MemoryStream(_explosionDataAsArr));
		Read(pooledBinaryReader);
	}

	public ExplosionData(DynamicProperties _properties, MinEffectController _effects = null)
	{
		ParticleIndex = 0;
		_properties.ParseInt("Explosion.ParticleIndex", ref ParticleIndex);
		Duration = 0f;
		_properties.ParseFloat("Explosion.Duration", ref Duration);
		BlockRadius = 1f;
		_properties.ParseFloat("Explosion.RadiusBlocks", ref BlockRadius);
		if (_properties.Values.ContainsKey("Explosion.BlockDamage"))
		{
			BlockDamage = StringParsers.ParseFloat(_properties.Values["Explosion.BlockDamage"]);
		}
		else
		{
			BlockDamage = BlockRadius * BlockRadius;
		}
		BlockTags = string.Empty;
		_properties.ParseString("Explosion.BlockTags", ref BlockTags);
		EntityRadius = 0;
		if (_properties.Values.ContainsKey("Explosion.RadiusEntities"))
		{
			EntityRadius = (int)StringParsers.ParseFloat(_properties.Values["Explosion.RadiusEntities"]);
		}
		if (_properties.Values.ContainsKey("Explosion.EntityDamage"))
		{
			EntityDamage = StringParsers.ParseFloat(_properties.Values["Explosion.EntityDamage"]);
		}
		else
		{
			EntityDamage = 20f * (float)EntityRadius;
		}
		BlastPower = 100;
		if (_properties.Values.ContainsKey("Explosion.BlastPower"))
		{
			BlastPower = (int)StringParsers.ParseFloat(_properties.Values["Explosion.BlastPower"]);
		}
		BuffActions = null;
		_properties.ParseFloat("Explosion.RadiusBlocks", ref BlockRadius);
		if (_properties.Values.ContainsKey("Explosion.Buff"))
		{
			string[] array = _properties.Values["Explosion.Buff"].Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (BuffActions == null)
				{
					BuffActions = new List<string>();
				}
				BuffActions.Add(array[i]);
			}
		}
		damageMultiplier = new DamageMultiplier(_properties, "Explosion.");
		IgnoreHeatMap = false;
		_properties.ParseBool("Explosion.IgnoreHeatMap", ref IgnoreHeatMap);
		DamageType = EnumDamageTypes.Heat;
		_properties.ParseEnum("Explosion.DamageType", ref DamageType);
		if (_effects != null)
		{
			MinEffectGroup minEffectGroup = new MinEffectGroup
			{
				OwnerTiered = false
			};
			if (!_effects.PassivesIndex.Contains(PassiveEffects.ExplosionBlockDamage))
			{
				PassiveEffect pe = PassiveEffect.CreateEmptyPassiveEffect(PassiveEffects.ExplosionBlockDamage);
				MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, pe);
			}
			if (!_effects.PassivesIndex.Contains(PassiveEffects.ExplosionEntityDamage))
			{
				PassiveEffect pe2 = PassiveEffect.CreateEmptyPassiveEffect(PassiveEffects.ExplosionEntityDamage);
				MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, pe2);
			}
			if (minEffectGroup.PassiveEffects.Count > 0)
			{
				_effects.AddEffectGroup(minEffectGroup);
			}
		}
	}

	public byte[] ToByteArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(memoryStream);
			Write(pooledBinaryWriter);
		}
		return memoryStream.ToArray();
	}

	public void Read(BinaryReader _br)
	{
		ParticleIndex = _br.ReadInt16();
		Duration = (float)_br.ReadInt16() * 0.1f;
		BlockRadius = (float)_br.ReadInt16() * 0.05f;
		EntityRadius = _br.ReadInt16();
		BlastPower = _br.ReadInt16();
		BlockDamage = _br.ReadSingle();
		EntityDamage = _br.ReadSingle();
		BlockTags = _br.ReadString();
		IgnoreHeatMap = _br.ReadBoolean();
		DamageType = (EnumDamageTypes)_br.ReadInt16();
		damageMultiplier = new DamageMultiplier();
		damageMultiplier.Read(_br);
		int num = _br.ReadByte();
		if (num > 0)
		{
			BuffActions = new List<string>();
			for (int i = 0; i < num; i++)
			{
				BuffActions.Add(_br.ReadString());
			}
		}
		else
		{
			BuffActions = null;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)ParticleIndex);
		_bw.Write((short)(Duration * 10f));
		_bw.Write((short)(BlockRadius * 20f));
		_bw.Write((short)EntityRadius);
		_bw.Write((short)BlastPower);
		_bw.Write(BlockDamage);
		_bw.Write(EntityDamage);
		_bw.Write(BlockTags);
		_bw.Write(IgnoreHeatMap);
		_bw.Write((short)DamageType);
		damageMultiplier.Write(_bw);
		if (BuffActions != null)
		{
			_bw.Write((byte)BuffActions.Count);
			for (int i = 0; i < BuffActions.Count; i++)
			{
				_bw.Write(BuffActions[i]);
			}
		}
		else
		{
			_bw.Write((byte)0);
		}
	}
}
