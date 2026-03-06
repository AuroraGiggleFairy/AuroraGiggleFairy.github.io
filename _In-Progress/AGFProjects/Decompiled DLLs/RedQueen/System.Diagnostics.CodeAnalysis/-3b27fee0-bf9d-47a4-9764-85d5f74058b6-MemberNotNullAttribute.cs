using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal sealed class _003C3b27fee0_002Dbf9d_002D47a4_002D9764_002D85d5f74058b6_003EMemberNotNullAttribute : Attribute
{
	public string[] Members { get; }

	public _003C3b27fee0_002Dbf9d_002D47a4_002D9764_002D85d5f74058b6_003EMemberNotNullAttribute(string member)
	{
		Members = new string[1] { member };
	}

	public _003C3b27fee0_002Dbf9d_002D47a4_002D9764_002D85d5f74058b6_003EMemberNotNullAttribute(params string[] members)
	{
		Members = members;
	}
}
