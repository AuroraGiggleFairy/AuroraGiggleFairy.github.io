using System;

namespace Discord;

[Flags]
internal enum ActivityProperties
{
	None = 0,
	Instance = 1,
	Join = 2,
	Spectate = 4,
	JoinRequest = 8,
	Sync = 0x10,
	Play = 0x20,
	PartyPrivacyFriends = 0x40,
	PartyPrivacyVoiceChannel = 0x80,
	Embedded = 0x80
}
