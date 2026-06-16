using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class TriggerHasTags : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> currentItemTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag = false;
		flag = (hasAllTags ? _params.Tags.Test_AllSet(currentItemTags) : _params.Tags.Test_AnySet(currentItemTags));
		if (!invert)
		{
			return flag;
		}
		return !flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				currentItemTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				hasAllTags = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
