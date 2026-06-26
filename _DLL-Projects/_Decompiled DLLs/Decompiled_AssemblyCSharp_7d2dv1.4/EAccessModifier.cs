using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public enum EAccessModifier
{
	Unknown,
	Public,
	Private,
	Protected,
	Internal,
	ProtectedInternal,
	PrivateProtected
}
