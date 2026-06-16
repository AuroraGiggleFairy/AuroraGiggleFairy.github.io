using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.GameData;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class Item : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyLocalizedName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("localizedName");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyIsBlock = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isBlock");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] allItemsSerialized;

	public Item(Web _parent)
	{
		JsonWriter jsonWriter = default(JsonWriter);
		jsonWriter.WriteBeginArray();
		int num = 0;
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			ItemClass itemClass = ItemClass.list[i];
			if (itemClass != null)
			{
				if (num > 0)
				{
					jsonWriter.WriteValueSeparator();
				}
				num++;
				string name = itemClass.Name;
				string localizedItemName = itemClass.GetLocalizedItemName();
				bool value = itemClass.IsBlock();
				jsonWriter.WriteRaw(jsonKeyName);
				jsonWriter.WriteString(name);
				jsonWriter.WriteRaw(jsonKeyLocalizedName);
				jsonWriter.WriteString(localizedItemName);
				jsonWriter.WriteRaw(jsonKeyIsBlock);
				jsonWriter.WriteBoolean(value);
				jsonWriter.WriteEndObject();
			}
		}
		jsonWriter.WriteEndArray();
		allItemsSerialized = jsonWriter.ToUtf8ByteArray();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(allItemsSerialized);
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
