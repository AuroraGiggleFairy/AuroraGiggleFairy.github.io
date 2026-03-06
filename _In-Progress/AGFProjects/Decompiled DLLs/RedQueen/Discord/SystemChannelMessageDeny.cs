using System;

namespace Discord;

[Flags]
internal enum SystemChannelMessageDeny
{
	None = 0,
	WelcomeMessage = 1,
	GuildBoost = 2,
	GuildSetupTip = 4,
	WelcomeMessageReply = 8
}
