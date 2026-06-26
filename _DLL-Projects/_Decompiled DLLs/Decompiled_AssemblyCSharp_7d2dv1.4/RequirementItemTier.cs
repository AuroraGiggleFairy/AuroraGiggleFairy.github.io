using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class RequirementItemTier : RequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvarName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int cvarNameHash;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue == null)
		{
			return false;
		}
		if (!invert)
		{
			return RequirementBase.compareValues((int)_params.ItemValue.Quality, operation, value);
		}
		return !RequirementBase.compareValues((int)_params.ItemValue.Quality, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Item tier {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
