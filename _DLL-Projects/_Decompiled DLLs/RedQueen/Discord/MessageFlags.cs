using System;

namespace Discord;

[Flags]
internal enum MessageFlags
{
	None = 0,
	Crossposted = 1,
	IsCrosspost = 2,
	SuppressEmbeds = 4,
	SourceMessageDeleted = 8,
	Urgent = 0x10,
	HasThread = 0x20,
	Ephemeral = 0x40,
	Loading = 0x80
}
