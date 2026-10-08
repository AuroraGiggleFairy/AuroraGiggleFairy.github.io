using System;
using System.Collections.Generic;
using System.IO;

namespace AGF.Compat.OutbackRoadies
{
	internal static class CompatLocLoader
	{
		internal static void ApplyAfterOurModLoc(bool loadingInGame)
		{
			string modPath = ModAPI.ModPath;
			if (string.IsNullOrEmpty(modPath))
			{
				return;
			}

			string folder = Path.Combine(modPath, "Config", "HelpfulRenames");
			if (!Directory.Exists(folder))
			{
				return;
			}

			TryLoadPatchFolder(folder, loadingInGame);
		}

		static void TryLoadPatchFolder(string folder, bool loadingInGame)
		{
			string locFile = Path.Combine(folder, "Localization.csv");
			string modsFile = Path.Combine(folder, "mods.txt");
			string folderName = Path.GetFileName(folder);
			if (!File.Exists(locFile) || !File.Exists(modsFile))
			{
				return;
			}

			List<string> required = ReadRequiredMods(modsFile);
			if (required.Count == 0)
			{
				Console.WriteLine("[AGF-OutbackRoadies] Skipping " + folderName + ": mods.txt has no mod names.");
				return;
			}

			foreach (string name in required)
			{
				if (!ModManager.ModLoaded(name))
				{
					Console.WriteLine("[AGF-OutbackRoadies] Skipping " + folderName + ": missing " + name);
					return;
				}
			}

			Localization.LoadPatchDictionaries("NoEACCompat:" + folderName, folder, loadingInGame);
			Console.WriteLine("[AGF-OutbackRoadies] Loaded " + folderName + " (" + string.Join(" + ", required.ToArray()) + ")");
		}

		static List<string> ReadRequiredMods(string modsFile)
		{
			var names = new List<string>();
			foreach (string raw in File.ReadAllLines(modsFile))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
				{
					continue;
				}
				names.Add(line);
			}
			return names;
		}
	}
}
