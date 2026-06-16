using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.ServerState;

[Preserve]
public class GameStats : KeyValueListAbs
{
	public GameStats()
		: base("GameStats")
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void iterateList(ref JsonWriter _writer, ref bool _first)
	{
		foreach (EnumGameStats item in EnumUtils.Values<EnumGameStats>())
		{
			string key = item.ToStringCached();
			switch (global::GameStats.GetStatType(item))
			{
			case global::GameStats.EnumType.Int:
				addItem(ref _writer, ref _first, key, global::GameStats.GetInt(item));
				break;
			case global::GameStats.EnumType.Float:
				addItem(ref _writer, ref _first, key, global::GameStats.GetFloat(item));
				break;
			case global::GameStats.EnumType.String:
				addItem(ref _writer, ref _first, key, global::GameStats.GetString(item));
				break;
			case global::GameStats.EnumType.Bool:
				addItem(ref _writer, ref _first, key, global::GameStats.GetBool(item));
				break;
			}
		}
	}
}
