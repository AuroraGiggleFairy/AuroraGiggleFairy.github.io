using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class TwitchVoteRequirementHasBuff : BaseTwitchVoteRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string BuffName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] BuffList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffName = "buff_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		BuffList = BuffName.Split(',');
	}

	public override bool CanPerform(EntityPlayer player)
	{
		for (int i = 0; i < BuffList.Length; i++)
		{
			if (!CheckBuff(player, BuffList[i]))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckBuff(EntityPlayer player, string buffName)
	{
		if (player.Buffs.HasBuff(buffName))
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropBuffName, ref BuffName);
	}
}
