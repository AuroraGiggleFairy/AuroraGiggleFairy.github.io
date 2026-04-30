using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPauseBuff : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> buffTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool checkAlreadyExists = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool pauseState = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffTags = "buff_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPauseState = "state";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCheckAlreadyExists = "check_already_exists";

	public override bool CanPerform(Entity target)
	{
		if (!checkAlreadyExists)
		{
			return true;
		}
		if (target is EntityAlive entityAlive && !entityAlive.Buffs.HasBuffByTag(buffTags))
		{
			return false;
		}
		return true;
	}

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityAlive entityAlive))
		{
			return;
		}
		for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
			if (buffValue.BuffClass.Tags.Test_AnySet(buffTags))
			{
				buffValue.Paused = pauseState;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnServerPerform(Entity target)
	{
		if (!(target is EntityAlive entityAlive))
		{
			return;
		}
		for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
			if (buffValue.BuffClass.Tags.Test_AnySet(buffTags))
			{
				buffValue.Paused = pauseState;
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string optionalValue = "";
		properties.ParseString(PropBuffTags, ref optionalValue);
		if (optionalValue != "")
		{
			buffTags = FastTags<TagGroup.Global>.Parse(optionalValue);
		}
		properties.ParseBool(PropPauseState, ref pauseState);
		properties.ParseBool(PropCheckAlreadyExists, ref checkAlreadyExists);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPauseBuff
		{
			buffTags = buffTags,
			pauseState = pauseState,
			targetGroup = targetGroup,
			checkAlreadyExists = checkAlreadyExists
		};
	}
}
