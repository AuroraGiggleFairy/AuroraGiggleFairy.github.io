using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddBuff : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string removesBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] removesBuffs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string altVisionBuffName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool checkAlreadyExists = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string sequenceLink = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float duration = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffName = "buff_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemovesBuff = "removes_buff";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAltVisionBuffName = "alt_vision_buff_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCheckAlreadyExists = "check_already_exists";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSequenceLink = "sequence_link";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDuration = "duration";

	public override bool CanPerform(Entity target)
	{
		if (!checkAlreadyExists)
		{
			return true;
		}
		if (target is EntityAlive entityAlive)
		{
			if (entityAlive.Buffs.HasBuff(buffName) || (altVisionBuffName != "" && entityAlive.Buffs.HasBuff(altVisionBuffName)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		if (removesBuffs == null)
		{
			removesBuffs = removesBuff.Split(',');
		}
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			bool flag = false;
			for (int i = 0; i < removesBuffs.Length; i++)
			{
				if (entityAlive.Buffs.HasBuff(removesBuffs[i]))
				{
					entityAlive.Buffs.RemoveBuff(removesBuffs[i]);
					flag = true;
				}
			}
			if (!flag)
			{
				if (altVisionBuffName != "" && entityAlive is EntityPlayer && (entityAlive as EntityPlayer).TwitchVisionDisabled)
				{
					entityAlive.Buffs.AddBuff(altVisionBuffName);
					return ActionCompleteStates.Complete;
				}
				entityAlive.Buffs.AddBuff(buffName, -1, _netSync: true, _fromElectrical: false, duration);
				if (sequenceLink != "" && entityAlive.Buffs.GetBuff(buffName) != null)
				{
					GameEventManager.Current.RegisterLink(entityAlive as EntityPlayer, base.Owner, sequenceLink);
				}
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropBuffName, ref buffName);
		properties.ParseString(PropRemovesBuff, ref removesBuff);
		properties.ParseString(PropAltVisionBuffName, ref altVisionBuffName);
		properties.ParseBool(PropCheckAlreadyExists, ref checkAlreadyExists);
		properties.ParseString(PropSequenceLink, ref sequenceLink);
		Properties.ParseFloat(PropDuration, ref duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddBuff
		{
			buffName = buffName,
			removesBuff = removesBuff,
			targetGroup = targetGroup,
			altVisionBuffName = altVisionBuffName,
			checkAlreadyExists = checkAlreadyExists,
			sequenceLink = sequenceLink,
			duration = duration
		};
	}
}
