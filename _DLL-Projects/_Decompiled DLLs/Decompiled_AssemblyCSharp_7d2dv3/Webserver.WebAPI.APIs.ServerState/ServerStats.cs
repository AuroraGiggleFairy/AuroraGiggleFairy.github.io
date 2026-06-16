using UnityEngine.Scripting;
using Utf8Json;
using Webserver.LiveData;

namespace Webserver.WebAPI.APIs.ServerState;

[Preserve]
public class ServerStats : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGameTime = JsonWriter.GetEncodedPropertyNameWithBeginObject("gameTime");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPlayers = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("players");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyHostiles = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("hostiles");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyAnimals = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("animals");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonKeyGameTime);
		var (days, hours, minutes) = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
		JsonCommons.WriteGameTimeObject(ref _writer, days, hours, minutes);
		_writer.WriteRaw(jsonKeyPlayers);
		_writer.WriteInt32(GameManager.Instance.World.Players.Count);
		_writer.WriteRaw(jsonKeyHostiles);
		_writer.WriteInt32(Hostiles.Instance.GetCount());
		_writer.WriteRaw(jsonKeyAnimals);
		_writer.WriteInt32(Animals.Instance.GetCount());
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
