using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionExplode : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int blastPower = 75;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int blockDamage = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int blockRadius = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityDamage = 5000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityRadius = 3;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockTags = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDamageTypes damageType = EnumDamageTypes.Heat;

	public override void Execute(MinEventParams _params)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		new Vector3(0f, 0.5f, 0f);
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (entityAlive != null)
			{
				ExplosionData explosionData = new ExplosionData
				{
					BlastPower = blastPower,
					BlockDamage = blockDamage,
					BlockRadius = blockRadius,
					BlockTags = blockTags,
					EntityDamage = entityDamage,
					EntityRadius = entityRadius,
					DamageType = damageType,
					ParticleIndex = 13
				};
				GameManager.Instance.ExplosionServer(0, entityAlive.getHeadPosition(), entityAlive.GetBlockPosition(), entityAlive.qrotation, explosionData, entityAlive.entityId, 0.1f, _bRemoveBlockAtExplPosition: false);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "blast_power":
				blastPower = StringParsers.ParseSInt32(_attribute.Value);
				return true;
			case "block_damage":
				blockDamage = StringParsers.ParseSInt32(_attribute.Value);
				return true;
			case "block_radius":
				blockRadius = StringParsers.ParseSInt32(_attribute.Value);
				break;
			case "block_tags":
				blockTags = _attribute.Value;
				break;
			case "entity_damage":
				entityDamage = StringParsers.ParseSInt32(_attribute.Value);
				break;
			case "entity_radius":
				entityRadius = StringParsers.ParseSInt32(_attribute.Value);
				break;
			case "damage_type":
				damageType = (EnumDamageTypes)Enum.Parse(typeof(EnumDamageTypes), _attribute.Value);
				break;
			}
		}
		return flag;
	}
}
