using System.Collections.Generic;
using System.IO;

public struct ExplosionData
{
	public const string PropExplosion = "Explosion";

	public const string PropParticleIndex = "ParticleIndex";

	public const string PropDuration = "Duration";

	public const string PropRadiusBlocks = "RadiusBlocks";

	public const string PropRadiusEntities = "RadiusEntities";

	public const string PropBlockDamage = "BlockDamage";

	public const string PropEntityDamage = "EntityDamage";

	public const string PropBlockTags = "BlockTags";

	public const string PropBlastPower = "BlastPower";

	public const string PropBuff = "Buff";

	public const string PropIgnoreHeatMap = "IgnoreHeatMap";

	public const string PropDamageType = "DamageType";

	public const int cMaxBlastPower = 100;

	public int ParticleIndex = 0;

	public float Duration = 0f;

	public float BlockRadius = 0f;

	public int EntityRadius = 0;

	public int BlastPower = 100;

	public float EntityDamage = 0f;

	public float BlockDamage = 0f;

	public string BlockTags = string.Empty;

	public bool IgnoreHeatMap = false;

	public EnumDamageTypes DamageType = EnumDamageTypes.Heat;

	public DamageMultiplier damageMultiplier = null;

	public List<string> BuffActions = null;

	public ExplosionData(byte[] _explosionDataAsArr)
	{
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(new MemoryStream(_explosionDataAsArr));
		Read(pooledBinaryReader);
	}

	public ExplosionData(DynamicProperties _properties, MinEffectController _effects = null)
	{
		if (!_properties.Classes.ContainsKey("Explosion"))
		{
			return;
		}
		DynamicProperties dynamicProperties = _properties.Classes["Explosion"];
		ParticleIndex = 0;
		dynamicProperties.ParseInt("ParticleIndex", ref ParticleIndex);
		Duration = 0f;
		dynamicProperties.ParseFloat("Duration", ref Duration);
		BlockRadius = 1f;
		dynamicProperties.ParseFloat("RadiusBlocks", ref BlockRadius);
		if (dynamicProperties.Values.ContainsKey("BlockDamage"))
		{
			BlockDamage = StringParsers.ParseFloat(dynamicProperties.Values["BlockDamage"]);
		}
		else
		{
			BlockDamage = BlockRadius * BlockRadius;
		}
		BlockTags = string.Empty;
		dynamicProperties.ParseString("BlockTags", ref BlockTags);
		EntityRadius = 0;
		if (dynamicProperties.Values.ContainsKey("RadiusEntities"))
		{
			EntityRadius = (int)StringParsers.ParseFloat(dynamicProperties.Values["RadiusEntities"]);
		}
		if (dynamicProperties.Values.ContainsKey("EntityDamage"))
		{
			EntityDamage = StringParsers.ParseFloat(dynamicProperties.Values["EntityDamage"]);
		}
		else
		{
			EntityDamage = 20f * (float)EntityRadius;
		}
		BlastPower = 100;
		if (dynamicProperties.Values.ContainsKey("BlastPower"))
		{
			BlastPower = (int)StringParsers.ParseFloat(dynamicProperties.Values["BlastPower"]);
		}
		BuffActions = null;
		dynamicProperties.ParseFloat("RadiusBlocks", ref BlockRadius);
		if (dynamicProperties.Values.ContainsKey("Buff"))
		{
			string[] array = dynamicProperties.Values["Buff"].Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (BuffActions == null)
				{
					BuffActions = new List<string>();
				}
				BuffActions.Add(array[i]);
			}
		}
		damageMultiplier = new DamageMultiplier(dynamicProperties);
		IgnoreHeatMap = false;
		dynamicProperties.ParseBool("IgnoreHeatMap", ref IgnoreHeatMap);
		DamageType = EnumDamageTypes.Heat;
		dynamicProperties.ParseEnum("DamageType", ref DamageType);
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
