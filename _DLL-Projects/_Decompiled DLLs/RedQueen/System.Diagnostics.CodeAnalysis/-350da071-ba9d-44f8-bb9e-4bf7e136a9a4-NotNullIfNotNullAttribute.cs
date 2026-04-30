using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal sealed class _003C350da071_002Dba9d_002D44f8_002Dbb9e_002D4bf7e136a9a4_003ENotNullIfNotNullAttribute : Attribute
{
	public string ParameterName { get; }

	public _003C350da071_002Dba9d_002D44f8_002Dbb9e_002D4bf7e136a9a4_003ENotNullIfNotNullAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}
}
