using System.Runtime.CompilerServices;

namespace Platform;

public static class EUserPermsExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasMultiplayer(this EUserPerms _perms)
	{
		return _perms.HasFlag(EUserPerms.Multiplayer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasCommunication(this EUserPerms _perms)
	{
		return _perms.HasFlag(EUserPerms.Communication);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasCrossplay(this EUserPerms _perms)
	{
		return _perms.HasFlag(EUserPerms.Crossplay);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasHostMultiplayer(this EUserPerms _perms)
	{
		return _perms.HasFlag(EUserPerms.HostMultiplayer);
	}
}
