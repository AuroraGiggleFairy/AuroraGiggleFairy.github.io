using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionExplodeTarget : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string blastPowerText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockDamageText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockRadiusText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string entityDamageText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string entityRadiusText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int particleIndex = 13;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ignoreHeatMap = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockTags = "";

	public static string PropBlastPower = "blast_power";

	public static string PropBlockDamage = "block_damage";

	public static string PropBlockRadius = "block_radius";

	public static string PropBlockTags = "block_tags";

	public static string PropEntityDamage = "entity_damage";

	public static string PropEntityRadius = "entity_radius";

	public static string PropParticleIndex = "particle_index";

	public static string PropIgnoreHeatMap = "ignore_heatmap";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		EntityAlive alive = base.Owner.Target as EntityAlive;
		if (entityAlive != null)
		{
			ExplosionData explosionData = new ExplosionData
			{
				BlastPower = GameEventManager.GetIntValue(alive, blastPowerText, 75),
				BlockDamage = GameEventManager.GetFloatValue(alive, blockDamageText, 1f),
				BlockRadius = GameEventManager.GetFloatValue(alive, blockRadiusText, 4f),
				BlockTags = blockTags,
				EntityDamage = GameEventManager.GetFloatValue(alive, entityDamageText, 5000f),
				EntityRadius = GameEventManager.GetIntValue(alive, entityRadiusText, 3),
				ParticleIndex = particleIndex,
				IgnoreHeatMap = ignoreHeatMap
			};
			GameManager.Instance.ExplosionServer(0, entityAlive.position, entityAlive.GetBlockPosition(), entityAlive.qrotation, explosionData, -1, 0.1f, _bRemoveBlockAtExplPosition: false);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropBlastPower, ref blastPowerText);
		properties.ParseString(PropBlockDamage, ref blockDamageText);
		properties.ParseString(PropBlockRadius, ref blockRadiusText);
		properties.ParseString(PropEntityDamage, ref entityDamageText);
		properties.ParseString(PropEntityRadius, ref entityRadiusText);
		properties.ParseString(PropBlockTags, ref blockTags);
		properties.ParseInt(PropParticleIndex, ref particleIndex);
		properties.ParseBool(PropIgnoreHeatMap, ref ignoreHeatMap);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionExplodeTarget
		{
			targetGroup = targetGroup,
			blastPowerText = blastPowerText,
			blockDamageText = blockDamageText,
			blockRadiusText = blockRadiusText,
			entityDamageText = entityDamageText,
			entityRadiusText = entityRadiusText,
			particleIndex = particleIndex,
			blockTags = blockTags
		};
	}
}
