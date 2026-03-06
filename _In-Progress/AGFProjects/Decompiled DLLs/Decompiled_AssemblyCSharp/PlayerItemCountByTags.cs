using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class PlayerItemCountByTags : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> itemTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		int itemCount = target.inventory.GetItemCount(itemTags);
		itemCount += target.bag.GetItemCount(itemTags);
		if (invert)
		{
			return !RequirementBase.compareValues(itemCount, operation, value);
		}
		return RequirementBase.compareValues(itemCount, operation, value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "item_tags")
		{
			itemTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
			return true;
		}
		return flag;
	}
}
