using System.Collections.Generic;
using System.Xml.Linq;

public abstract class StatCompareAbs : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water,
		Armor
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatSample
	{
		Current,
		StartOfFrame
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatTypes stat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatSample sample;

	public EntityStats Stats
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return sample switch
			{
				StatSample.Current => target.Stats, 
				StatSample.StartOfFrame => target.StartOfFrameStats, 
				_ => target.Stats, 
			};
		}
	}

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		return Compare(_params);
	}

	public abstract bool Compare(MinEventParams _params);

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}' {0} {2} {3}", invert ? "NOT" : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "stat")
			{
				stat = EnumUtils.Parse<StatTypes>(_attribute.Value, _ignoreCase: true);
				return true;
			}
			if (localName == "sample")
			{
				sample = EnumUtils.Parse<StatSample>(_attribute.Value, _ignoreCase: true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatCompareAbs()
	{
	}
}
