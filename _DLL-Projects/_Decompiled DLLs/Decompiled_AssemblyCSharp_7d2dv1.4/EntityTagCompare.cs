using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class EntityTagCompare : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tagsToCompare;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (hasAllTags)
		{
			if (!invert)
			{
				return target.HasAllTags(tagsToCompare);
			}
			return !target.HasAllTags(tagsToCompare);
		}
		if (!invert)
		{
			return target.HasAnyTags(tagsToCompare);
		}
		return !target.HasAnyTags(tagsToCompare);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				tagsToCompare = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
