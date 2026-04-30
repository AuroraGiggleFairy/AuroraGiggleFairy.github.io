using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class PlayerItemCount : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string item_name;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue item;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (item_name != null && item == null)
		{
			item = ItemClass.GetItem(item_name, _caseInsensitive: true);
		}
		if (item == null)
		{
			return false;
		}
		int itemCount = target.inventory.GetItemCount(item);
		itemCount += target.bag.GetItemCount(item);
		if (invert)
		{
			return !RequirementBase.compareValues(itemCount, operation, value);
		}
		return RequirementBase.compareValues(itemCount, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Item count {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "item_name")
		{
			item_name = _attribute.Value;
			return true;
		}
		return flag;
	}
}
