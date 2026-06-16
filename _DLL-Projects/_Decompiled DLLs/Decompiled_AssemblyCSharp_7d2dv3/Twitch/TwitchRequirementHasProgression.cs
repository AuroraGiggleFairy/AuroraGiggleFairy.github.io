using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class TwitchRequirementHasProgression : BaseTwitchOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string SkillName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int Level = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSkillName = "skill_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLevel = "level";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity entity)
	{
		if (entity is EntityPlayer entityPlayer)
		{
			return entityPlayer.Progression.GetProgressionValue(SkillName).CalculatedLevel(entityPlayer);
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity entity)
	{
		return Level;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckPerk(EntityPlayer player, string buffName)
	{
		if (player.Progression.GetProgressionValue(buffName).CalculatedLevel(player) >= Level)
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropSkillName, ref SkillName);
		properties.ParseInt(PropLevel, ref Level);
	}
}
