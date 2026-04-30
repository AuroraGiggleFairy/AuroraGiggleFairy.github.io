using System;

namespace Platform;

[Flags]
public enum EUserPerms
{
	Multiplayer = 1,
	Communication = 2,
	Crossplay = 4,
	HostMultiplayer = 8,
	All = 0xF
}
