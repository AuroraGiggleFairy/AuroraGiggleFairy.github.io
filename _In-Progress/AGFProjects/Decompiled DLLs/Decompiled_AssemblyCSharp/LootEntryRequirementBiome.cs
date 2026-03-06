using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementBiome : BaseLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] biomes;

	public override void Init(XElement e)
	{
		base.Init(e);
		string _result = "";
		if (e.ParseAttribute("biomes", ref _result))
		{
			biomes = _result.Split(',');
		}
		else
		{
			biomes = new string[0];
		}
	}

	public override bool CheckRequirement(EntityPlayer player)
	{
		return biomes.ContainsCaseInsensitive(player.biomeStandingOn.m_sBiomeName);
	}
}
