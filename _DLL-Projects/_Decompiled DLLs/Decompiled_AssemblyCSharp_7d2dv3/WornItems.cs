using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class WornItems : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> equipmentTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		int num = 0;
		int slotCount = target.equipment.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			ItemValue slotItem = target.equipment.GetSlotItem(i);
			if (slotItem != null && slotItem.ItemClass.HasAnyTags(equipmentTags))
			{
				num++;
			}
		}
		return invert != RequirementBase.compareValues(num, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("WornItems: {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "tags")
		{
			equipmentTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
			return true;
		}
		return flag;
	}
}
