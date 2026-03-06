namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
internal sealed class _003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
