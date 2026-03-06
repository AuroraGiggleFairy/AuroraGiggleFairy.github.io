namespace Discord;

internal interface IAttachment
{
	ulong Id { get; }

	string Filename { get; }

	string Url { get; }

	string ProxyUrl { get; }

	int Size { get; }

	int? Height { get; }

	int? Width { get; }

	bool Ephemeral { get; }

	string Description { get; }

	string ContentType { get; }
}
