using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class ProgressionLevel : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string progressionName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int progressionId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue pv;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target.Progression != null)
		{
			pv = target.Progression.GetProgressionValue(progressionId);
			if (pv != null)
			{
				if (invert)
				{
					return !RequirementBase.compareValues(pv.GetCalculatedLevel(target), operation, value);
				}
				return RequirementBase.compareValues(pv.GetCalculatedLevel(target), operation, value);
			}
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("'{1}' level {0} {2} {3}", invert ? "NOT" : "", progressionName, operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "progression_name")
		{
			progressionName = _attribute.Value;
			progressionId = Progression.CalcId(progressionName);
			return true;
		}
		return flag;
	}
}
