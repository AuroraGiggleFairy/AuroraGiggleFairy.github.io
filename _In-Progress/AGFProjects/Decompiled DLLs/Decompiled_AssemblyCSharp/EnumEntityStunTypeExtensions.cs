using UnityEngine.Scripting;

[Preserve]
public static class EnumEntityStunTypeExtensions
{
	public static bool CanMove(this EnumEntityStunType type)
	{
		return type == EnumEntityStunType.None;
	}
}
