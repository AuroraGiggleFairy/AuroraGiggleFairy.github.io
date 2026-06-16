using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.WorldState;

[Preserve]
public class Bloodmoon : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGameTime = JsonWriter.GetEncodedPropertyNameWithBeginObject("gameTime");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyBloodmoonActive = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("bloodmoonActive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyNextBloodmoon = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("nextBloodmoon");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyNextBloodmoonEnd = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("nextBloodmoonEnd");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		ulong worldTime = GameManager.Instance.World.worldTime;
		(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(worldTime);
		int item = tuple.Days;
		int item2 = tuple.Hours;
		int item3 = tuple.Minutes;
		int num = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay"));
		(int, int) duskDawnTimes = GameUtils.CalcDuskDawnHours(GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("DayLightLength")));
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonKeyGameTime);
		JsonCommons.WriteGameTimeObject(ref _writer, item, item2, item3);
		_writer.WriteRaw(jsonKeyBloodmoonActive);
		_writer.WriteBoolean(GameUtils.IsBloodMoonTime(worldTime, duskDawnTimes, num));
		_writer.WriteRaw(jsonKeyNextBloodmoon);
		JsonCommons.WriteGameTimeObject(ref _writer, num, duskDawnTimes.Item1, 0);
		_writer.WriteRaw(jsonKeyNextBloodmoonEnd);
		JsonCommons.WriteGameTimeObject(ref _writer, num + 1, duskDawnTimes.Item2, 0);
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}
}
