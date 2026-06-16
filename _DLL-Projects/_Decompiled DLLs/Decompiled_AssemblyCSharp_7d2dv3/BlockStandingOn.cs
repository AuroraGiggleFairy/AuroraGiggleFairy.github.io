using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BlockStandingOn : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> blockTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag = false;
		flag = ((!hasAllTags) ? target.blockValueStandingOn.Block.HasAnyFastTags(blockTags) : target.blockValueStandingOn.Block.HasAllFastTags(blockTags));
		if (!invert)
		{
			return flag;
		}
		return !flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0}Male", invert ? "NOT " : ""));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				blockTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
