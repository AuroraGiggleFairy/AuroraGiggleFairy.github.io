using System.Collections.Generic;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.LiveData;

namespace Webserver.WebAPI.APIs.WorldState;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class Hostile : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityEnemy> entities = new List<EntityEnemy>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyId = JsonWriter.GetEncodedPropertyNameWithBeginObject("id");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPosition = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("position");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteBeginArray();
		lock (entities)
		{
			Hostiles.Instance.Get(entities);
			for (int i = 0; i < entities.Count; i++)
			{
				if (i > 0)
				{
					_writer.WriteValueSeparator();
				}
				EntityAlive entityAlive = entities[i];
				Vector3i position = new Vector3i(entityAlive.GetPosition());
				_writer.WriteRaw(jsonKeyId);
				_writer.WriteInt32(entityAlive.entityId);
				_writer.WriteRaw(jsonKeyName);
				_writer.WriteString((!string.IsNullOrEmpty(entityAlive.EntityName)) ? entityAlive.EntityName : $"enemy class #{entityAlive.entityClass}");
				_writer.WriteRaw(jsonKeyPosition);
				JsonCommons.WriteVector3I(ref _writer, position);
				_writer.WriteEndObject();
			}
		}
		_writer.WriteEndArray();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}
}
