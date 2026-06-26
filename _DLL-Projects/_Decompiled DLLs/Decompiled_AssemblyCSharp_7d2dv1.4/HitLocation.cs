using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class HitLocation : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string bodyPartNames = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumBodyPartHit bodyParts;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return (bodyParts & _params.DamageResponse.HitBodyPart) != 0;
		}
		return (bodyParts & _params.DamageResponse.HitBodyPart) == 0;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0} hit location: ", invert ? "NOT " : "", bodyPartNames));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "body_parts")
		{
			bodyPartNames = _attribute.Value;
			string[] array = bodyPartNames.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				bodyParts |= EnumUtils.Parse<EnumBodyPartHit>(array[i], _ignoreCase: true);
			}
			return true;
		}
		return flag;
	}
}
