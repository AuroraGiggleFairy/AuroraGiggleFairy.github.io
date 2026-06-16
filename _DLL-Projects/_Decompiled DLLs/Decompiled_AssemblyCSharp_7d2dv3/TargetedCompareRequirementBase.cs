using System.Xml.Linq;

public class TargetedCompareRequirementBase : RequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TargetTypes
	{
		self,
		other,
		instigator
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TargetTypes targetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive target;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		target = null;
		if (targetType == TargetTypes.other)
		{
			if (_params.Other != null)
			{
				target = _params.Other;
			}
		}
		else if (targetType == TargetTypes.instigator)
		{
			if (_params.Instigator != null)
			{
				target = _params.Instigator;
			}
		}
		else if (_params.Self != null)
		{
			target = _params.Self;
		}
		return target != null;
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "target")
		{
			targetType = EnumUtils.Parse<TargetTypes>(_attribute.Value, _ignoreCase: true);
			return true;
		}
		return flag;
	}
}
