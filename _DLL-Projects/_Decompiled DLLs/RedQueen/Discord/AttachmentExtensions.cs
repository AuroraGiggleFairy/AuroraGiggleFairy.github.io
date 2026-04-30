namespace Discord;

internal static class AttachmentExtensions
{
	public const string SpoilerPrefix = "SPOILER_";

	public static bool IsSpoiler(this IAttachment attachment)
	{
		return attachment.Filename.StartsWith("SPOILER_");
	}
}
