using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
[MeansImplicitUse]
public class XuiXmlAttributeAttribute : Attribute
{
	public readonly string AttributeName;

	public readonly bool Override;

	public XuiXmlAttributeAttribute(string _attributeName, bool _override = false)
	{
		AttributeName = _attributeName;
		Override = _override;
	}
}
