using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver;
using Webserver.WebAPI;

namespace MapRendering.Api;

[Preserve]
public class Map : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyEnabled = JsonWriter.GetEncodedPropertyNameWithBeginObject("enabled");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyMapBlockSize = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("mapBlockSize");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyMaxZoom = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("maxZoom");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyMapSize = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("mapSize");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		if (requestPath == "config")
		{
			writeConfig(ref _writer);
			AbsRestApi.SendEnvelopedResult(_context, ref _writer);
		}
		else
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotImplemented, null, "INVALID_ID");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeConfig(ref JsonWriter writer)
	{
		writer.WriteRaw(jsonKeyEnabled);
		writer.WriteBoolean(MapRenderer.Enabled);
		writer.WriteRaw(jsonKeyMapBlockSize);
		writer.WriteInt32(Constants.MapBlockSize);
		writer.WriteRaw(jsonKeyMaxZoom);
		writer.WriteInt32(Constants.Zoomlevels - 1);
		GameManager.Instance.World.GetWorldExtent(out var _minSize, out var _maxSize);
		Vector3i position = _maxSize - _minSize;
		writer.WriteRaw(jsonKeyMapSize);
		JsonCommons.WriteVector3I(ref writer, position);
		writer.WriteEndObject();
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, 2000, -2147483648, -2147483648, -2147483648 };
	}
}
