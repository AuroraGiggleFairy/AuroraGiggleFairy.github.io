using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchAddPoints : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum RecipientTypes
	{
		Requester,
		All,
		Random
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string amountText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string viewer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int amount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TwitchAction.PointTypes pointType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool requesterOnly = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string awardText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public RecipientTypes recipientType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAmount = "amount";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPointType = "point_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRecipientType = "recipient_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRequesterOnly = "requester_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAwardText = "award_text";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayerLocal entityPlayerLocal) || !(base.Owner.Requester == entityPlayerLocal))
		{
			return;
		}
		amount = GameEventManager.GetIntValue(entityPlayerLocal, amountText);
		if (recipientType == RecipientTypes.All)
		{
			switch (pointType)
			{
			case TwitchAction.PointTypes.PP:
				TwitchManager.Current.ViewerData.AddPointsAll(amount, 0);
				break;
			case TwitchAction.PointTypes.SP:
				TwitchManager.Current.ViewerData.AddPointsAll(0, amount);
				break;
			case TwitchAction.PointTypes.Bits:
				Debug.LogWarning("TwitchAddPoints: Cannot add Bit Credit to all.");
				break;
			}
			TwitchManager.Current.SendChannelMessage(Localization.Get(awardText));
			return;
		}
		viewer = ((recipientType == RecipientTypes.Requester) ? base.Owner.ExtraData : TwitchManager.Current.ViewerData.GetRandomActiveViewer());
		if (!(viewer == ""))
		{
			switch (pointType)
			{
			case TwitchAction.PointTypes.PP:
				TwitchManager.Current.ViewerData.AddPoints(viewer, amount, isSpecial: false, displayNewTotal: false);
				break;
			case TwitchAction.PointTypes.SP:
				TwitchManager.Current.ViewerData.AddPoints(viewer, amount, isSpecial: true, displayNewTotal: false);
				break;
			case TwitchAction.PointTypes.Bits:
				TwitchManager.Current.ViewerData.AddCredit(viewer, amount, displayNewTotal: false);
				break;
			}
			if (awardText != "")
			{
				TwitchManager.Current.SendChannelMessage(GetTextWithElements(Localization.Get(awardText)));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string ParseTextElement(string element)
	{
		if (!(element == "amount"))
		{
			if (element == "viewer")
			{
				return viewer;
			}
			return base.ParseTextElement(element);
		}
		return amount.ToString();
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropAmount, ref amountText);
		properties.ParseEnum(PropPointType, ref pointType);
		properties.ParseEnum(PropRecipientType, ref recipientType);
		properties.ParseBool(PropRequesterOnly, ref requesterOnly);
		properties.ParseString(PropAwardText, ref awardText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchAddPoints
		{
			amountText = amountText,
			pointType = pointType,
			recipientType = recipientType,
			requesterOnly = requesterOnly,
			awardText = awardText
		};
	}
}
