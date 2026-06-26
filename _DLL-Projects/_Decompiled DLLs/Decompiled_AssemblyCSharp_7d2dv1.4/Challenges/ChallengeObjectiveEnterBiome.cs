using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveEnterBiome : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string biome;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.EnterBiome;

	public override string DescriptionText => Localization.Get("challengeObjectiveEnter") + " " + Localization.Get("biome_" + biome) + ":";

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.BiomeEnter += Current_BiomeEnter;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.BiomeEnter -= Current_BiomeEnter;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BiomeEnter(BiomeDefinition biomeDef)
	{
		if (biomeDef != null && biomeDef.m_sBiomeName == biome)
		{
			base.Current++;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("biome"))
		{
			biome = e.GetAttribute("biome");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveEnterBiome
		{
			biome = biome
		};
	}
}
