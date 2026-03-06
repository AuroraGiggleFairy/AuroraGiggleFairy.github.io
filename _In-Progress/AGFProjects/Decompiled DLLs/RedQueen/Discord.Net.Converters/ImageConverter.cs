using System;
using System.IO;
using Discord.API;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class ImageConverter : JsonConverter
{
	public static readonly ImageConverter Instance = new ImageConverter();

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Discord.API.Image image = (Discord.API.Image)value;
		if (image.Stream != null)
		{
			byte[] array;
			int length;
			if (image.Stream.CanSeek)
			{
				array = new byte[image.Stream.Length - image.Stream.Position];
				length = image.Stream.Read(array, 0, array.Length);
			}
			else
			{
				using MemoryStream memoryStream = new MemoryStream();
				image.Stream.CopyTo(memoryStream);
				array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, array.Length);
				length = (int)memoryStream.Length;
			}
			string text = Convert.ToBase64String(array, 0, length);
			writer.WriteValue("data:image/jpeg;base64," + text);
		}
		else if (image.Hash != null)
		{
			writer.WriteValue(image.Hash);
		}
	}
}
