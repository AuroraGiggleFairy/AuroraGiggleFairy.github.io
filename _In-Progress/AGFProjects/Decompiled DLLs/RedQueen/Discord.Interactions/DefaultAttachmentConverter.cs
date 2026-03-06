namespace Discord.Interactions;

internal class DefaultAttachmentConverter<T> : DefaultEntityTypeConverter<T> where T : class, IAttachment
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.Attachment;
	}
}
