using System;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.ServerState;

[Preserve]
public class GamePrefs : KeyValueListAbs
{
	public GamePrefs()
		: base("GamePrefs")
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void iterateList(ref JsonWriter _writer, ref bool _first)
	{
		foreach (EnumGamePrefs item in EnumUtils.Values<EnumGamePrefs>())
		{
			string text = item.ToStringCached();
			if (!text.Contains("Password", StringComparison.Ordinal))
			{
				global::GamePrefs.EnumType? prefType = global::GamePrefs.GetPrefType(item);
				object obj = global::GamePrefs.GetDefault(item);
				switch (prefType)
				{
				case global::GamePrefs.EnumType.Int:
				{
					int? num2 = obj as int?;
					addItem(ref _writer, ref _first, text, global::GamePrefs.GetInt(item), num2);
					break;
				}
				case global::GamePrefs.EnumType.Float:
				{
					float? num = obj as float?;
					addItem(ref _writer, ref _first, text, global::GamePrefs.GetFloat(item), num);
					break;
				}
				case global::GamePrefs.EnumType.String:
				{
					string text2 = obj as string;
					addItem(ref _writer, ref _first, text, global::GamePrefs.GetString(item), text2);
					break;
				}
				case global::GamePrefs.EnumType.Bool:
				{
					bool? flag = obj as bool?;
					addItem(ref _writer, ref _first, text, global::GamePrefs.GetBool(item), flag);
					break;
				}
				}
			}
		}
	}
}
