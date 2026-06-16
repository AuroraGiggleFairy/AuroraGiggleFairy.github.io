using System.Collections.Generic;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.GameData;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class EntityClass : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("id");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyCommandIndex = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("commandId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyManualSpawnType = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("manualSpawnType");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] allClassesSerialized;

	public EntityClass(Web _parent)
	{
		JsonWriter jsonWriter = default(JsonWriter);
		jsonWriter.WriteBeginArray();
		int num = 0;
		int num2 = 0;
		foreach (var (value, entityClass2) in global::EntityClass.list.Dict)
		{
			if (num > 0)
			{
				jsonWriter.WriteValueSeparator();
			}
			num++;
			string entityClassName = entityClass2.entityClassName;
			global::EntityClass.UserSpawnType userSpawnType = entityClass2.userSpawnType;
			jsonWriter.WriteRaw(jsonKeyName);
			jsonWriter.WriteString(entityClassName);
			jsonWriter.WriteRaw(jsonKeyId);
			jsonWriter.WriteInt32(value);
			if (entityClass2.userSpawnType != global::EntityClass.UserSpawnType.None)
			{
				num2++;
				jsonWriter.WriteRaw(jsonKeyCommandIndex);
				jsonWriter.WriteInt32(num2);
			}
			jsonWriter.WriteRaw(jsonKeyManualSpawnType);
			jsonWriter.WriteString(userSpawnType.ToStringCached());
			jsonWriter.WriteEndObject();
		}
		jsonWriter.WriteEndArray();
		allClassesSerialized = jsonWriter.ToUtf8ByteArray();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(allClassesSerialized);
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
