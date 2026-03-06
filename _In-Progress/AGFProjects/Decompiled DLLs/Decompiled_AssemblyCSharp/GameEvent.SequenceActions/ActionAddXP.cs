using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddXP : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string xpAmountText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xpAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropXPAmount = "xp_amount";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			xpAmount = GameEventManager.GetIntValue(entityPlayer, xpAmountText);
			if (xpAmount < 0)
			{
				xpAmount = 0;
			}
			entityPlayer.Progression.AddLevelExp(xpAmount);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropXPAmount, ref xpAmountText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddXP
		{
			xpAmountText = xpAmountText
		};
	}
}
