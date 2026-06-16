using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class PerksUnlocked : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string skill_name;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (skill_name == null)
		{
			return false;
		}
		ProgressionValue progressionValue = target.Progression.GetProgressionValue(skill_name);
		int num = 0;
		for (int i = 0; i < progressionValue.ProgressionClass.Children.Count; i++)
		{
			num += target.Progression.GetProgressionValue(progressionValue.ProgressionClass.Children[i].Name).Level;
		}
		if (invert)
		{
			return !RequirementBase.compareValues(num, operation, value);
		}
		return RequirementBase.compareValues(num, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("perks unlocked count {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "skill_name")
		{
			skill_name = _attribute.Value;
			return true;
		}
		return flag;
	}
}
