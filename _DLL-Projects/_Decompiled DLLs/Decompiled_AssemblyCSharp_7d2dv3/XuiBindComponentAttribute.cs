using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public class XuiBindComponentAttribute : Attribute
{
	public readonly string XmlElementName;

	public readonly bool Required;

	public XuiBindComponentAttribute(bool _required = true)
	{
		Required = _required;
	}

	public XuiBindComponentAttribute(string _xmlElementName, bool _required = true)
	{
		XmlElementName = _xmlElementName;
		Required = _required;
	}
}
