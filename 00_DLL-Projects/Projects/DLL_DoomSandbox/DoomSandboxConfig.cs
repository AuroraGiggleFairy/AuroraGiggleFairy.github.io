using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace DoomSandbox
{
	public sealed class DoomSandboxOptionDef
	{
		public string Tab;
		public string Name;
		public string Default;
		public string[] Choices;
		public string Does;
	}

	public sealed class DoomSandboxConfig
	{
		public readonly List<string> TabOrder = new List<string>();
		public readonly List<DoomSandboxOptionDef> Options = new List<DoomSandboxOptionDef>();

		public static DoomSandboxConfig Load(string path)
		{
			var cfg = new DoomSandboxConfig();
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				DoomLog.Error("Config not found: " + path);
				return cfg;
			}

			string currentTab = null;
			foreach (var raw in File.ReadAllLines(path))
			{
				var line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("="))
					continue;

				if (line.StartsWith("TAB:", StringComparison.OrdinalIgnoreCase))
				{
					currentTab = line.Substring(4).Trim();
					if (currentTab.Length > 0 && !cfg.TabOrder.Contains(currentTab))
						cfg.TabOrder.Add(currentTab);
					continue;
				}

				if (currentTab == null)
					continue;

				var parts = SplitPipe(line);
				if (parts.Count < 4)
					continue;

				cfg.Options.Add(new DoomSandboxOptionDef
				{
					Tab = currentTab,
					Name = parts[0].Trim(),
					Default = parts[1].Trim(),
					Choices = SplitChoices(parts[2]),
					Does = parts[3].Trim()
				});
			}

			DoomLog.Info($"Loaded {cfg.Options.Count} options across {cfg.TabOrder.Count} tabs.");
			return cfg;
		}

		static List<string> SplitPipe(string line)
		{
			var parts = new List<string>();
			int start = 0;
			for (int i = 0; i < line.Length; i++)
			{
				if (line[i] == '|')
				{
					parts.Add(line.Substring(start, i - start));
					start = i + 1;
					if (parts.Count == 3)
					{
						parts.Add(line.Substring(start));
						return parts;
					}
				}
			}
			if (start <= line.Length)
				parts.Add(line.Substring(start));
			return parts;
		}

		static string[] SplitChoices(string choices)
		{
			var list = new List<string>();
			foreach (var piece in choices.Split(','))
			{
				var t = piece.Trim();
				if (t.Length > 0)
					list.Add(t);
			}
			return list.ToArray();
		}

		public static bool TryParsePercentOrNumber(string text, out float value)
		{
			value = 0f;
			if (string.IsNullOrEmpty(text))
				return false;

			var t = text.Trim();
			if (t.Equals("none", StringComparison.OrdinalIgnoreCase)
				|| t.Equals("disabled", StringComparison.OrdinalIgnoreCase)
				|| t.Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				value = 0f;
				return true;
			}

			// "Never" / "Never/10000" → vanilla TraderResetInterval sentinel (-1)
			if (t.Equals("never", StringComparison.OrdinalIgnoreCase)
				|| Regex.IsMatch(t, @"^never\s*/\s*\d+(\.\d+)?$", RegexOptions.IgnoreCase))
			{
				value = -1f;
				return true;
			}

			// "Default (100%)", "High (150%)", "Disabled (0%)", "Very Low (25%)"
			var m = Regex.Match(t, @"\(([+\-]?\d+(\.\d+)?)\s*%\)", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				value = float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) / 100f;
				return true;
			}

			m = Regex.Match(t, @"\(([+\-]?\d+(\.\d+)?)\)");
			if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				return true;

			if (Regex.IsMatch(t, @"^[+\-]?\d+(\.\d+)?\s*%$"))
			{
				value = float.Parse(t.TrimEnd('%').Trim(), CultureInfo.InvariantCulture) / 100f;
				return true;
			}

			m = Regex.Match(t, @"^[+\-]?(\d+(\.\d+)?)\s*(degrees?|deg)$", RegexOptions.IgnoreCase);
			if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				return true;

			m = Regex.Match(t, @"^([+\-]?)\s*(\d+)\s*(GS|LS)$", RegexOptions.IgnoreCase);
			if (m.Success && float.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				if (m.Groups[1].Value == "-")
					value = -value;
				return true;
			}

			// Bare number (including "4.5" without unit)
			return float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}
	}
}
