using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class GameStatInt : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumGameStats GameStat = EnumGameStats.AnimalCount;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		int num = GameStats.GetInt(GameStat);
		if (invert)
		{
			return !RequirementBase.compareValues(num, operation, value);
		}
		return RequirementBase.compareValues(num, operation, value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "gamestat")
		{
			GameStat = Enum.Parse<EnumGameStats>(_attribute.Value);
			return true;
		}
		return flag;
	}
}
