using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CompareItemMetaFloat : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string metaKey;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		ItemValue itemValue = _params.ItemValue;
		if (itemValue == null || string.IsNullOrEmpty(metaKey))
		{
			return false;
		}
		if (!itemValue.TryGetMetadata(metaKey, out float valueA))
		{
			return false;
		}
		if (invert)
		{
			return !RequirementBase.compareValues(valueA, operation, value);
		}
		return RequirementBase.compareValues(valueA, operation, value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "key")
		{
			metaKey = _attribute.Value;
			return true;
		}
		return flag;
	}
}
