using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse]
public class XuiBindingNcalcFunctionAttribute : Attribute
{
	public readonly string FunctionName;

	public readonly int ExpectedArgumentCount;

	public readonly object ErrorResult;

	public XuiBindingNcalcFunctionAttribute()
	{
	}

	public XuiBindingNcalcFunctionAttribute(object _errorResultValue, int _expectedArgumentCount, string _functionName = null)
	{
		FunctionName = _functionName;
		ExpectedArgumentCount = _expectedArgumentCount;
		ErrorResult = _errorResultValue;
	}
}
