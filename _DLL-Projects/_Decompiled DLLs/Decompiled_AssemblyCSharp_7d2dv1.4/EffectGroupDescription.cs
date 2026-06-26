using System.Xml.Linq;

public class EffectGroupDescription
{
	public readonly int MinLevel;

	public readonly int MaxLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string DescriptionKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string CustomDescription;

	public readonly string LongDescriptionKey;

	public string Description
	{
		get
		{
			if (Localization.Exists(DescriptionKey))
			{
				return Localization.Get(DescriptionKey);
			}
			return CustomDescription;
		}
	}

	public string LongDescription => Localization.Get(LongDescriptionKey);

	public EffectGroupDescription(int _minLevel, int _maxLevel, string _desc_key, string _description, string _long_desc_key)
	{
		MinLevel = _minLevel;
		MaxLevel = _maxLevel;
		DescriptionKey = _desc_key;
		CustomDescription = _description;
		LongDescriptionKey = _long_desc_key;
	}

	public static EffectGroupDescription ParseDescription(XElement _element)
	{
		if (!_element.HasAttribute("level") || (!_element.HasAttribute("desc_key") && !_element.HasAttribute("desc_base")))
		{
			return null;
		}
		int num = 0;
		int minLevel;
		if (_element.GetAttribute("level").Contains(","))
		{
			string[] array = _element.GetAttribute("level").Split(',');
			if (array.Length < 1)
			{
				return null;
			}
			if (array.Length == 1)
			{
				minLevel = (num = StringParsers.ParseSInt32(array[0]));
			}
			else
			{
				minLevel = StringParsers.ParseSInt32(array[0]);
				num = StringParsers.ParseSInt32(array[1]);
			}
		}
		else
		{
			minLevel = (num = StringParsers.ParseSInt32(_element.GetAttribute("level")));
		}
		return new EffectGroupDescription(minLevel, num, _element.GetAttribute("desc_key"), _element.GetAttribute("desc_base"), _element.HasAttribute("long_desc_key") ? _element.GetAttribute("long_desc_key") : "");
	}
}
