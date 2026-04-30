using System.Diagnostics;
using Discord.API;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Attachment : IAttachment
{
	public ulong Id { get; }

	public string Filename { get; }

	public string Url { get; }

	public string ProxyUrl { get; }

	public int Size { get; }

	public int? Height { get; }

	public int? Width { get; }

	public bool Ephemeral { get; }

	public string Description { get; }

	public string ContentType { get; }

	private string DebuggerDisplay => $"{Filename} ({Size} bytes)";

	internal Attachment(ulong id, string filename, string url, string proxyUrl, int size, int? height, int? width, bool? ephemeral, string description, string contentType)
	{
		Id = id;
		Filename = filename;
		Url = url;
		ProxyUrl = proxyUrl;
		Size = size;
		Height = height;
		Width = width;
		Ephemeral = ephemeral ?? false;
		Description = description;
		ContentType = contentType;
	}

	internal static Attachment Create(Discord.API.Attachment model)
	{
		return new Attachment(model.Id, model.Filename, model.Url, model.ProxyUrl, model.Size, model.Height.IsSpecified ? new int?(model.Height.Value) : ((int?)null), model.Width.IsSpecified ? new int?(model.Width.Value) : ((int?)null), model.Ephemeral.ToNullable(), model.Description.GetValueOrDefault(), model.ContentType.GetValueOrDefault());
	}

	public override string ToString()
	{
		return Filename;
	}
}
