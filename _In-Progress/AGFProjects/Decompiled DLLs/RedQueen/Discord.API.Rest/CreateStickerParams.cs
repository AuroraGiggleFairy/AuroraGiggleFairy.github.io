using System.Collections.Generic;
using System.IO;
using Discord.Net.Rest;

namespace Discord.API.Rest;

internal class CreateStickerParams
{
	public Stream File { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string Tags { get; set; }

	public string FileName { get; set; }

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			["name"] = Name ?? "",
			["description"] = Description,
			["tags"] = Tags
		};
		string contentType;
		if (File is FileStream fileStream)
		{
			string text = Path.GetExtension(fileStream.Name).TrimStart('.');
			contentType = ((text == "json") ? "application/json" : ("image/" + text));
		}
		else if (FileName != null)
		{
			string text2 = Path.GetExtension(FileName).TrimStart('.');
			contentType = ((text2 == "json") ? "application/json" : ("image/" + text2));
		}
		else
		{
			contentType = "image/png";
		}
		dictionary["file"] = new MultipartFile(File, FileName ?? "image", contentType);
		return dictionary;
	}
}
