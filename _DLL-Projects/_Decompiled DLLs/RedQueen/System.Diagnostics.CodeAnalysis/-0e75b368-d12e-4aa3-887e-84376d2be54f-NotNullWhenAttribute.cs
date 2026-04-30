namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class _003C0e75b368_002Dd12e_002D4aa3_002D887e_002D84376d2be54f_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003C0e75b368_002Dd12e_002D4aa3_002D887e_002D84376d2be54f_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
