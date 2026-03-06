using System;

namespace Discord.Commands;

[Flags]
internal enum ContextType
{
	Guild = 1,
	DM = 2,
	Group = 4
}
