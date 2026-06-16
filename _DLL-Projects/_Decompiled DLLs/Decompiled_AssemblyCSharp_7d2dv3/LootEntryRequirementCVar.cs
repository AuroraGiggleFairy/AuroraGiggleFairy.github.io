using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementCVar : BaseOperationLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvar;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	public override void Init(XElement e)
	{
		base.Init(e);
		e.ParseAttribute("cvar", ref cvar);
		e.ParseAttribute("value", ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			return player.Buffs.GetCustomVar(cvar);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return StringParsers.ParseFloat(valueText);
	}
}
