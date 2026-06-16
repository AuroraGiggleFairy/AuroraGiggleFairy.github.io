using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetTwitchCooldown : MinEventActionTargetedBase
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer.TwitchActionsStates state
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			EntityPlayer entityPlayer = targets[i] as EntityPlayer;
			if (entityPlayer != null && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
			{
				entityPlayer.TwitchActionsEnabled = state;
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "state")
		{
			state = (EntityPlayer.TwitchActionsStates)StringParsers.ParseSInt32(_attribute.Value);
			if (state == EntityPlayer.TwitchActionsStates.Disabled)
			{
				state = EntityPlayer.TwitchActionsStates.TempDisabled;
			}
		}
		return num;
	}
}
