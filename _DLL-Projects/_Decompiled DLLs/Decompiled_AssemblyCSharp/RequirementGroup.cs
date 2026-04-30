using System.Collections.Generic;

public sealed class RequirementGroup
{
	public enum Op
	{
		And,
		Or
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Op op;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IRequirement> reqs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<RequirementGroup> groups;

	public RequirementGroup(Op _op, List<IRequirement> _reqs, List<RequirementGroup> _groups)
	{
		op = _op;
		reqs = _reqs;
		groups = _groups;
	}

	public bool IsValid(MinEventParams _params)
	{
		return op switch
		{
			Op.And => EvalAnd(_params), 
			Op.Or => EvalOr(_params), 
			_ => EvalAnd(_params), 
		};
	}

	public void GetInfoStrings(ref List<string> list)
	{
		if (reqs != null)
		{
			for (int i = 0; i < reqs.Count; i++)
			{
				string description = reqs[i].GetDescription();
				if (string.IsNullOrEmpty(description))
				{
					reqs[i].GetInfoStrings(ref list);
				}
				else
				{
					list.Add(description);
				}
			}
		}
		if (groups != null)
		{
			for (int j = 0; j < groups.Count; j++)
			{
				groups[j].GetInfoStrings(ref list);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EvalAnd(MinEventParams _params)
	{
		if ((reqs == null || reqs.Count == 0) && (groups == null || groups.Count == 0))
		{
			return true;
		}
		if (reqs != null)
		{
			for (int i = 0; i < reqs.Count; i++)
			{
				if (!reqs[i].IsValid(_params))
				{
					return false;
				}
			}
		}
		if (groups != null)
		{
			for (int j = 0; j < groups.Count; j++)
			{
				if (!groups[j].IsValid(_params))
				{
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EvalOr(MinEventParams _params)
	{
		bool flag = false;
		if (reqs != null)
		{
			flag |= reqs.Count > 0;
			for (int i = 0; i < reqs.Count; i++)
			{
				if (reqs[i].IsValid(_params))
				{
					return true;
				}
			}
		}
		if (groups != null)
		{
			flag |= groups.Count > 0;
			for (int j = 0; j < groups.Count; j++)
			{
				if (groups[j].IsValid(_params))
				{
					return true;
				}
			}
		}
		return !flag;
	}
}
