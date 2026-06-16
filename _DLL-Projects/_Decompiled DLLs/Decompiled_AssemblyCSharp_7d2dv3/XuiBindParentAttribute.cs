using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public class XuiBindParentAttribute : Attribute
{
	public readonly bool Required;

	public XuiBindParentAttribute(bool _required = true)
	{
		Required = _required;
	}
}
