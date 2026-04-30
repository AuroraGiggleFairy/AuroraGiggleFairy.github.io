using System.Collections.Generic;

namespace Discord;

internal class StickerProperties
{
	public Optional<string> Name { get; set; }

	public Optional<string> Description { get; set; }

	public Optional<IEnumerable<string>> Tags { get; set; }
}
