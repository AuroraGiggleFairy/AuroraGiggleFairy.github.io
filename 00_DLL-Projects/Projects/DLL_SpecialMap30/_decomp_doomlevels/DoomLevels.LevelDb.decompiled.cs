using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;

namespace DoomLevels;

public static class LevelDb
{
	private static readonly Dictionary<string, Level> Levels = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase);

	public static int Shell { get; private set; } = 3;

	public static int ChunkSize { get; private set; } = 16;

	public static int Lift { get; private set; } = 140;

	public static bool Loaded { get; private set; }

	public static Dictionary<string, Level>.ValueCollection All => Levels.Values;

	public static Vector3i MaxSize
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			Vector3i zero = Vector3i.zero;
			foreach (Level value in Levels.Values)
			{
				zero.x = Mathf.Max(zero.x, value.Size.x);
				zero.y = Mathf.Max(zero.y, value.Size.y);
				zero.z = Mathf.Max(zero.z, value.Size.z);
			}
			return zero;
		}
	}

	public static Level Get(string map)
	{
		if (!Levels.TryGetValue(map, out var value))
		{
			return null;
		}
		return value;
	}

	public static void Load()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (Loaded)
		{
			return;
		}
		Loaded = true;
		string text = Path.Combine(LevelsApi.ModPath, "Config/doomlevels.xml");
		if (!File.Exists(text))
		{
			Log.Error("[DoomLevels] doomlevels.xml not found at " + text);
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(text);
		XmlElement documentElement = xmlDocument.DocumentElement;
		if (documentElement == null)
		{
			return;
		}
		Shell = ReadInt(documentElement, "shell", Shell);
		ChunkSize = ReadInt(documentElement, "chunk", ChunkSize);
		Lift = ReadInt(documentElement, "lift", Lift);
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode is XmlElement xmlElement && !(xmlElement.Name != "level"))
			{
				Level level = new Level
				{
					Name = xmlElement.GetAttribute("name"),
					Prefab = xmlElement.GetAttribute("prefab"),
					Size = ReadVector(xmlElement.GetAttribute("size")),
					Start = ReadVector(xmlElement.GetAttribute("start")),
					StartAngle = ReadInt(xmlElement, "startangle", 0),
					StartOffX = ReadOffset(xmlElement, 0),
					StartOffZ = ReadOffset(xmlElement, 1),
					Sky = ReadColor(xmlElement.GetAttribute("sky")),
					Kills = ReadInt(xmlElement, "kills", 0),
					Items = ReadInt(xmlElement, "items", 0),
					Secrets = ReadInt(xmlElement, "secrets", 0),
					Par = ReadInt(xmlElement, "par", 0),
					Tally = xmlElement.GetAttribute("tally"),
					Game = xmlElement.GetAttribute("game")
				};
				if (!string.IsNullOrEmpty(level.Name))
				{
					level.Sectors = Sectors.Load(level.Name);
					level.Route = Waypoints.Load(level.Name);
					Levels[level.Name] = level;
				}
			}
		}
		int num = 0;
		int num2 = 0;
		foreach (Level value in Levels.Values)
		{
			if (value.Sectors != null)
			{
				num++;
			}
			if (value.Route != null && value.Route.Count > 0)
			{
				num2++;
			}
		}
		Log.Out($"[DoomLevels] loaded {Levels.Count} levels, shell {Shell}, lift {Lift}, " + $"sector data for {num}, routes for {num2}");
	}

	public static Level NextAfter(string map)
	{
		if (string.IsNullOrEmpty(map) || !Levels.TryGetValue(map, out var value))
		{
			return null;
		}
		Level level = null;
		foreach (Level value2 in Levels.Values)
		{
			if (!(value2.Game != value.Game) && string.CompareOrdinal(value2.Name, map) > 0 && (level == null || string.CompareOrdinal(value2.Name, level.Name) < 0))
			{
				level = value2;
			}
		}
		return level;
	}

	private static float ReadOffset(XmlElement element, int index)
	{
		string attribute = element.GetAttribute("startoff");
		if (string.IsNullOrEmpty(attribute))
		{
			return 0.5f;
		}
		string[] array = attribute.Split(',', StringSplitOptions.None);
		if (index >= array.Length || !float.TryParse(array[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return 0.5f;
		}
		return result;
	}

	private static int ReadInt(XmlElement element, string name, int fallback)
	{
		if (!int.TryParse(element.GetAttribute(name), out var result))
		{
			return fallback;
		}
		return result;
	}

	private static Color ReadColor(string raw)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(raw) || raw.Length != 6)
		{
			return Color.grey;
		}
		if (!int.TryParse(raw, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
		{
			return Color.grey;
		}
		return new Color((float)((result >> 16) & 0xFF) / 255f, (float)((result >> 8) & 0xFF) / 255f, (float)(result & 0xFF) / 255f);
	}

	private static Vector3i ReadVector(string raw)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		string[] array = (raw ?? "").Split(',', StringSplitOptions.None);
		if (array.Length != 3)
		{
			return Vector3i.zero;
		}
		int.TryParse(array[0], out var result);
		int.TryParse(array[1], out var result2);
		int.TryParse(array[2], out var result3);
		return new Vector3i(result, result2, result3);
	}
}
