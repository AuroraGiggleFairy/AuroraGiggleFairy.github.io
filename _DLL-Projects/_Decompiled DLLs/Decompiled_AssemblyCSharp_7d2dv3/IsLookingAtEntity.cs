using System.Xml.Linq;

public class IsLookingAtEntity : IsLookingAtBlock
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new FastTags<TagGroup.Global> tagsToCompare;

	[PublicizedFrom(EAccessModifier.Private)]
	public new bool hasAllTags;

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				tagsToCompare = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				hasAllTags = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
