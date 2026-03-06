using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementInBiome : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] biomeList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBiome = "biomes";

	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			bool flag = biomeList.ContainsCaseInsensitive(entityAlive.biomeStandingOn.m_sBiomeName);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropBiome, ref biomes);
		if (!string.IsNullOrEmpty(biomes))
		{
			biomeList = biomes.Split(',');
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementInBiome
		{
			Invert = Invert,
			biomeList = biomeList
		};
	}
}
