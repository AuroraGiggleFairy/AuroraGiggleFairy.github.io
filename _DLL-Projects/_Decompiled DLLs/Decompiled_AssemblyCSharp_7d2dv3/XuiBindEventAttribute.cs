using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
[MeansImplicitUse]
public class XuiBindEventAttribute : Attribute
{
	public readonly string ComponentFieldName;

	public readonly string TargetEvent;

	public XuiBindEventAttribute(string _targetEvent, string _componentFieldName = null)
	{
		ComponentFieldName = _componentFieldName;
		TargetEvent = _targetEvent;
	}
}
