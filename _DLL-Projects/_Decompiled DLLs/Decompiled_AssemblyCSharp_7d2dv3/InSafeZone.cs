using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class InSafeZone : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target is EntityPlayer entityPlayer)
		{
			if (!invert)
			{
				return entityPlayer.TwitchSafe;
			}
			return !entityPlayer.TwitchSafe;
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} In Safe Zone", invert ? "NOT " : ""));
	}
}
