using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class PublicizedFromAttribute : Attribute
{
	public readonly EAccessModifier OriginalAccessModifier;

	public PublicizedFromAttribute(EAccessModifier _originalAccessModifier)
	{
		OriginalAccessModifier = _originalAccessModifier;
	}
}
