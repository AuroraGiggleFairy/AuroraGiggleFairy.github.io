using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementHasBuff : BaseRequirement
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

	public override bool CanPerform(Entity target)
	{
		if (target is EntityAlive player)
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
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckBuff(EntityAlive player, string buffName)
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
		if (properties.Values.ContainsKey(PropBuffName))
		{
			BuffName = properties.Values[PropBuffName];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementHasBuff
		{
			BuffName = BuffName,
			BuffList = BuffList,
			Invert = Invert
		};
	}
}
