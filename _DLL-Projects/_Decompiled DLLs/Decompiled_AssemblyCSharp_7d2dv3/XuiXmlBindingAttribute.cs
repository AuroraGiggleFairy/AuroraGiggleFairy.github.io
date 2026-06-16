using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
[MeansImplicitUse]
public class XuiXmlBindingAttribute : Attribute
{
	public readonly string BindingName;

	public XuiXmlBindingAttribute(string _bindingName)
	{
		BindingName = _bindingName;
	}
}
