using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class HasParticle : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string particleName = "";

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return _params.Self.HasParticle(particleName);
		}
		return !_params.Self.HasParticle(particleName);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "particle")
		{
			particleName = _attribute.Value;
			return true;
		}
		return flag;
	}
}
