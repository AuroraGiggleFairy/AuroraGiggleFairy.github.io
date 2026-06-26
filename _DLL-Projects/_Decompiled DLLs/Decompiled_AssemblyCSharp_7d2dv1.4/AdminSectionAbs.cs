using System.Xml;

public abstract class AdminSectionAbs
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly AdminTools Parent;

	public readonly string SectionTypeName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AdminSectionAbs(AdminTools _parent, string _sectionTypeName)
	{
		Parent = _parent;
		SectionTypeName = _sectionTypeName;
	}

	public abstract void Clear();

	public virtual void Parse(XmlNode _parentNode)
	{
		foreach (XmlNode childNode in _parentNode.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Comment)
			{
				if (childNode.NodeType != XmlNodeType.Element)
				{
					Log.Warning("Unexpected XML node found in '" + SectionTypeName + "' section: " + childNode.OuterXml);
					continue;
				}
				XmlElement childElement = (XmlElement)childNode;
				ParseElement(childElement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ParseElement(XmlElement _childElement);

	public abstract void Save(XmlElement _root);
}
