namespace Discord;

internal class GameAsset
{
	internal ulong? ApplicationId { get; set; }

	public string Text { get; internal set; }

	public string ImageId { get; internal set; }

	internal GameAsset()
	{
	}

	public string GetImageUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		if (!ApplicationId.HasValue)
		{
			return null;
		}
		return CDN.GetRichAssetUrl(ApplicationId.Value, ImageId, size, format);
	}
}
