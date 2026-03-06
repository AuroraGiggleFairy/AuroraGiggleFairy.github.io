using System.Collections.Generic;

namespace Discord;

internal class EmoteProperties
{
	public Optional<string> Name { get; set; }

	public Optional<IEnumerable<IRole>> Roles { get; set; }
}
