using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementProgression : BaseOperationLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string progressionName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;

	public override void Init(XElement e)
	{
		base.Init(e);
		e.ParseAttribute("name", ref progressionName);
		e.ParseAttribute("value", ref valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			ProgressionValue progressionValue = player.Progression.GetProgressionValue(progressionName);
			if (progressionValue != null)
			{
				return progressionValue.GetCalculatedLevel(player);
			}
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return StringParsers.ParseFloat(valueText);
	}
}
