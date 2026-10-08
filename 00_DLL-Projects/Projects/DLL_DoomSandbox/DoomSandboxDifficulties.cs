using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using SandboxOpt = global::SandboxOptions.SandboxOptions;
using SandboxOptBase = global::SandboxOptions.BaseSandboxOption;
using SandboxOptManager = global::SandboxOptions.SandboxOptionManager;
using SandboxOptPreset = global::SandboxOptions.SandboxOptionPreset;
using SandboxValueSet = global::SandboxOptions.SandboxOptionValueSet;

namespace DoomSandbox
{
	public sealed class DoomDifficultyDef
	{
		public string Name;
		public short Rating = 1;
		public bool IsDefault;
		public string Description = "";
		public string Icon = "";
		public readonly Dictionary<string, string> Values =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}

	public static class DoomSandboxDifficulties
	{
		public static List<DoomDifficultyDef> Load()
		{
			var list = new List<DoomDifficultyDef>();
			string[] lines;
			try
			{
				lines = DoomSandboxBlueprint.ReadDifficultiesLines();
			}
			catch (Exception ex)
			{
				DoomLog.Error("Compiled difficulties blueprint missing: " + ex.Message);
				return list;
			}

			DoomDifficultyDef current = null;
			bool inPresetSection = false;
			foreach (var raw in lines)
			{
				var line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#"))
					continue;

				// Real presets only after a long ==== divider (not the short title underline).
				if (line.StartsWith("=") && line.Replace("=", "").Length == 0 && line.Length >= 60)
				{
					inPresetSection = true;
					continue;
				}

				if (line.StartsWith("=") || line.StartsWith("---", StringComparison.Ordinal))
					continue;

				if (!inPresetSection)
					continue;

				if (line.StartsWith("DIFFICULTY:", StringComparison.OrdinalIgnoreCase))
				{
					string difficultyName = line.Substring("DIFFICULTY:".Length).Trim();
					// Skip docs examples like: DIFFICULTY: <display name> ...
					if (difficultyName.Length == 0 || IsDocPlaceholder(difficultyName))
					{
						current = null;
						continue;
					}

					// Dedupe if the same name appears twice in the file.
					current = list.Find(d => d.Name.Equals(difficultyName, StringComparison.OrdinalIgnoreCase));
					if (current == null)
					{
						current = new DoomDifficultyDef { Name = difficultyName };
						list.Add(current);
					}
					else
					{
						current.Values.Clear();
					}
					continue;
				}

				if (current == null)
					continue;

				if (line.StartsWith("Rating:", StringComparison.OrdinalIgnoreCase))
				{
					string ratingText = line.Substring(7).Trim();
					if (!IsDocPlaceholder(ratingText)
						&& short.TryParse(ratingText, NumberStyles.Integer,
							CultureInfo.InvariantCulture, out var rating))
						current.Rating = rating;
					continue;
				}

				if (line.StartsWith("Default:", StringComparison.OrdinalIgnoreCase))
				{
					var v = line.Substring(8).Trim();
					if (!IsDocPlaceholder(v))
					{
						current.IsDefault = v.Equals("true", StringComparison.OrdinalIgnoreCase)
							|| v.Equals("yes", StringComparison.OrdinalIgnoreCase)
							|| v.Equals("1", StringComparison.OrdinalIgnoreCase);
					}
					continue;
				}

				if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
				{
					string desc = line.Substring("Description:".Length).Trim();
					if (!IsDocPlaceholder(desc))
						current.Description = DoomSandboxBlueprint.AsDisplayText(desc);
					continue;
				}

				if (line.StartsWith("Icon:", StringComparison.OrdinalIgnoreCase))
				{
					string icon = line.Substring(5).Trim();
					if (!IsDocPlaceholder(icon))
						current.Icon = NormalizeIconPath(icon);
					continue;
				}

				int pipe = line.IndexOf('|');
				if (pipe <= 0)
					continue;

				string name = line.Substring(0, pipe).Trim();
				string value = line.Substring(pipe + 1).Trim();
				if (name.Length == 0)
					continue;

				// Blank value = worksheet default at apply time.
				current.Values[name] = value;
			}

			string stamp = DoomSandboxBlueprint.Stamp;
			if (stamp != _lastLoadedStamp || list.Count != _lastLoadedCount)
				DoomLog.Info($"Loaded {list.Count} difficulty presets from compiled blueprint.");
			_lastLoadedCount = list.Count;
			_lastLoadedStamp = stamp;
			return list;
		}

		static string _lastLoadedStamp;
		static int _lastLoadedCount = -1;
		static string _lastAppliedStamp;
		static int _lastAppliedCount = -1;

		static bool IsDocPlaceholder(string text)
		{
			if (string.IsNullOrEmpty(text))
				return true;
			// Header field docs use <display name>, <int>, <text>, <path>, true|false, etc.
			if (text.IndexOf('<') >= 0 || text.IndexOf('>') >= 0)
				return true;
			if (text.IndexOf('|') >= 0 && text.IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0)
				return true;
			return false;
		}

		public static void ApplyToManager(SandboxOptManager mgr, List<DoomDifficultyDef> difficulties)
		{
			if (mgr?.SandboxPresets == null)
				return;

			if (difficulties == null || difficulties.Count == 0)
			{
				EnsureFallbackBeGentle(mgr);
				return;
			}

			foreach (var p in mgr.SandboxPresets)
			{
				if (p != null)
					p.IsDefault = false;
			}

			// Replace ALL prior Difficulty-group presets (modded + accidental user saves in that group).
			// Official Doom ladder is the blueprint compiled into this DLL.
			mgr.SandboxPresets.RemoveAll(p =>
				p != null
				&& !p.IsCustomPreset
				&& string.Equals(p.Group, DoomSandboxRuntime.DifficultyPresetGroup, StringComparison.OrdinalIgnoreCase));

			bool anyDefault = false;
			var built = new List<SandboxOptPreset>();
			foreach (var def in difficulties)
			{
				if (string.IsNullOrEmpty(def.Name))
					continue;

				var preset = new SandboxOptPreset
				{
					Name = def.Name,
					Description = string.IsNullOrEmpty(def.Description)
						? def.Name
						: def.Description,
					Group = DoomSandboxRuntime.DifficultyPresetGroup,
					IsModded = true,
					IsDefault = def.IsDefault,
					IsUserPreset = false,
					IsCustomPreset = false,
					DifficultyRating = ClampRating(def.Rating),
					Icon = string.IsNullOrEmpty(def.Icon)
						? DoomSandboxMod.CustomPresetIconPath
						: def.Icon
				};

				FillPresetValues(mgr, preset, def);
				built.Add(preset);
				if (preset.IsDefault)
					anyDefault = true;
			}

			built.Sort((a, b) => a.DifficultyRating.CompareTo(b.DifficultyRating));
			if (!anyDefault && built.Count > 0)
				built[0].IsDefault = true;

			int insertAt = Math.Max(0, mgr.SandboxPresets.Count - 1);
			for (int i = 0; i < built.Count; i++)
				mgr.SandboxPresets.Insert(insertAt + i, built[i]);

			string applyStamp = $"{built.Count}:{(built.Find(p => p.IsDefault)?.Name ?? "?")}";
			if (applyStamp != _lastAppliedStamp || built.Count != _lastAppliedCount)
				DoomLog.Info($"Applied {built.Count} Difficulty presets (default={(built.Find(p => p.IsDefault)?.Name ?? "?")}).");
			_lastAppliedStamp = applyStamp;
			_lastAppliedCount = built.Count;
		}

		static void FillPresetValues(SandboxOptManager mgr, SandboxOptPreset preset, DoomDifficultyDef def)
		{
			preset.PresetValues.Clear();
			var cfg = DoomSandboxRuntime.Config;
			if (cfg == null)
				return;

			foreach (var optDef in cfg.Options)
			{
				if (!DoomSandboxRuntime.OptionByName.TryGetValue(optDef.Name, out SandboxOpt enumId))
					continue;
				if (!mgr.SandboxOptionsDict.TryGetValue(enumId, out SandboxOptBase option) || option == null)
					continue;

				string wanted = null;
				if (def.Values.TryGetValue(optDef.Name, out var fromDiff) && !string.IsNullOrWhiteSpace(fromDiff))
					wanted = fromDiff.Trim();
				else
					wanted = optDef.Default;

				int index = MatchChoiceIndex(option, optDef, wanted);
				if (index < 0)
					index = option.GetDefaultIndex();

				// Review tab lists every PresetValues entry; store overrides only
				// (updateOptionsToPreset resets to default first, then applies these).
				if (index == option.GetDefaultIndex())
					continue;

				preset.PresetValues[enumId] = index;
			}
		}

		static int MatchChoiceIndex(SandboxOptBase option, DoomSandboxOptionDef optDef, string wanted)
		{
			if (string.IsNullOrEmpty(wanted))
				return option.GetDefaultIndex();

			var choices = optDef.Choices;
			if (choices != null)
			{
				int idx = IndexOfChoiceLoose(choices, wanted);
				if (idx >= 0)
					return idx;
			}

			// Scan live display values (same order as value-set indices).
			try
			{
				var vs = option.GetValueSet();
				if (vs != null)
				{
					int count = vs.GetValueCount();
					for (int i = 0; i < count; i++)
					{
						string display = SandboxValueSetCompat.GetDisplayAtIndex(vs, i);
						if (ChoiceEquals(display, wanted))
							return i;
					}
				}
			}
			catch
			{
				/* ignore */
			}

			return -1;
		}

		static int IndexOfChoiceLoose(string[] choices, string wanted)
		{
			for (int i = 0; i < choices.Length; i++)
			{
				if (ChoiceEquals(choices[i], wanted))
					return i;
			}

			// "Default" → "Default (100%)", "Disabled" → "Disabled (0%)"
			for (int i = 0; i < choices.Length; i++)
			{
				if (choices[i].StartsWith(wanted + " (", StringComparison.OrdinalIgnoreCase)
					|| choices[i].StartsWith(wanted + "(", StringComparison.OrdinalIgnoreCase))
					return i;
			}

			if (DoomSandboxConfig.TryParsePercentOrNumber(wanted, out float wantedNum))
			{
				for (int i = 0; i < choices.Length; i++)
				{
					if (DoomSandboxConfig.TryParsePercentOrNumber(choices[i], out float choiceNum)
						&& Math.Abs(choiceNum - wantedNum) < 0.0001f)
						return i;
				}
			}

			return -1;
		}

		static bool ChoiceEquals(string a, string b)
		{
			if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
				return false;
			return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// V 3.2: GetDisplayAtIndex(int, string languageName).
		/// Bound on the abstract base so Invoke uses virtual dispatch on Float/Int/Bool sets.
		/// </summary>
		static class SandboxValueSetCompat
		{
			static MethodInfo _getDisplay;
			static bool _resolved;

			public static string GetDisplayAtIndex(object valueSet, int index)
			{
				if (valueSet == null)
					return null;

				Resolve();
				if (_getDisplay == null)
					return null;

				try
				{
					return _getDisplay.Invoke(valueSet, new object[] { index, null }) as string;
				}
				catch
				{
					return null;
				}
			}

			static void Resolve()
			{
				if (_resolved)
					return;

				_resolved = true;
				_getDisplay = AccessTools.Method(typeof(SandboxValueSet), "GetDisplayAtIndex", new[] { typeof(int), typeof(string) });
			}
		}

		static short ClampRating(short rating)
		{
			if (rating < 1) return 1;
			if (rating > 10) return 10;
			return rating;
		}

		public static string NormalizeIconPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return "";

			path = path.Trim();
			// Worksheet style: #@modfolder:UIAtlases/doomguy_4.png
			path = Regex.Replace(path, @"^#?@modfolder\s*:\s*", "", RegexOptions.IgnoreCase);
			if (path.StartsWith("@modfolder(", StringComparison.OrdinalIgnoreCase))
				return path;

			path = path.Replace('\\', '/').TrimStart('/');
			return $"@modfolder(DoomSandbox):{path}";
		}

		static void EnsureFallbackBeGentle(SandboxOptManager mgr)
		{
			const string name = DoomSandboxRuntime.BeGentlePresetName;
			const string group = DoomSandboxRuntime.DifficultyPresetGroup;

			foreach (var p in mgr.SandboxPresets)
			{
				if (p == null) continue;
				bool isOurs = p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
					|| p.Name.Equals("Be Gentle", StringComparison.OrdinalIgnoreCase);
				if (!isOurs)
					p.IsDefault = false;
			}

			var existing = mgr.SandboxPresets.Find(p =>
				p != null
				&& (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
					|| p.Name.Equals("Be Gentle", StringComparison.OrdinalIgnoreCase)));
			if (existing != null)
			{
				existing.Name = name;
				existing.IsDefault = true;
				existing.IsModded = true;
				existing.IsUserPreset = false;
				existing.IsCustomPreset = false;
				existing.Group = group;
				if (string.IsNullOrEmpty(existing.Description))
					existing.Description = "Doom Survival recommended defaults";
				if (string.IsNullOrEmpty(existing.Icon))
					existing.Icon = DoomSandboxMod.CustomPresetIconPath;
				existing.PresetValues?.Clear();
				return;
			}

			mgr.SandboxPresets.Insert(Math.Max(0, mgr.SandboxPresets.Count - 1), new SandboxOptPreset
			{
				Name = name,
				Description = "Doom Survival recommended defaults",
				Group = group,
				IsModded = true,
				IsDefault = true,
				IsUserPreset = false,
				IsCustomPreset = false,
				Icon = DoomSandboxMod.CustomPresetIconPath,
				DifficultyRating = 1
			});
		}
	}
}
