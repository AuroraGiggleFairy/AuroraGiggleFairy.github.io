using System.Collections.Generic;

namespace Discord;

internal class MessageProperties
{
	public Optional<string> Content { get; set; }

	public Optional<Embed> Embed { get; set; }

	public Optional<Embed[]> Embeds { get; set; }

	public Optional<MessageComponent> Components { get; set; }

	public Optional<MessageFlags?> Flags { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<IEnumerable<FileAttachment>> Attachments { get; set; }
}
