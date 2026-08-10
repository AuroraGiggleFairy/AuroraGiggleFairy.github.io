using System;
using System.Collections.Generic;
using HarmonyLib;
using SandboxOptBase = global::SandboxOptions.BaseSandboxOption;
using SandboxOptManager = global::SandboxOptions.SandboxOptionManager;
using SandboxOptPreset = global::SandboxOptions.SandboxOptionPreset;

namespace DoomSandbox
{
	internal static class DoomLog
	{
		public static void Info(string msg)
		{
			var line = "[DoomSandbox] " + msg;
			Console.WriteLine(line);
			try { UnityEngine.Debug.Log(line); } catch { /* ignore */ }
		}

		public static void Error(string msg)
		{
			var line = "[DoomSandbox] ERROR: " + msg;
			Console.WriteLine(line);
			try { UnityEngine.Debug.LogError(line); } catch { /* ignore */ }
		}
	}

	internal static class DoomSandboxRebuildGate
	{
		static int _lastApplyFrame = -1;
		static string _lastConfigStamp;

		public static void Rebuild(SandboxOptManager mgr, string reason)
		{
			if (mgr == null)
			{
				DoomLog.Error("Rebuild skipped (" + reason + "): manager null");
				return;
			}

			string stamp = ConfigStamp(DoomSandboxMod.ConfigPath);
			bool hasDoomTab = mgr.OptionsByCategory?.dict != null
				&& mgr.OptionsByCategory.dict.ContainsKey("Doom");

			// Skip only when worksheet unchanged AND Doom categories are already live.
			if (hasDoomTab
				&& DoomSandboxRuntime.VisibleOptions.Count > 0
				&& !string.IsNullOrEmpty(_lastConfigStamp)
				&& stamp == _lastConfigStamp)
			{
				return;
			}

			// Avoid hammering if multiple hooks fire in the same frame.
			int frame = 0;
			try { frame = UnityEngine.Time.frameCount; } catch { /* ignore */ }
			if (frame != 0 && frame == _lastApplyFrame && hasDoomTab && DoomSandboxRuntime.VisibleOptions.Count > 0)
			{
				return;
			}

			try
			{
				DoomLog.Info($"Rebuild start ({reason}). Config=" + DoomSandboxMod.ConfigPath);
				var cfg = DoomSandboxConfig.Load(DoomSandboxMod.ConfigPath);
				DoomSandboxRebuilder.Apply(mgr, cfg);
				_lastApplyFrame = frame;
				_lastConfigStamp = stamp;
			}
			catch (Exception ex)
			{
				// Apply() restores prior categories only if Doom categories never committed.
				DoomLog.Error("Rebuild failed (" + reason + "): " + ex);
			}
		}

		static string ConfigStamp(string path)
		{
			try
			{
				if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
					return "missing";
				var fi = new System.IO.FileInfo(path);
				return $"{fi.Length}:{fi.LastWriteTimeUtc.Ticks}";
			}
			catch
			{
				return "err";
			}
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), "SetupOptions")]
	public static class Patch_SandboxOptionManager_SetupOptions
	{
		public static void Postfix(SandboxOptManager __instance)
		{
			DoomSandboxRebuildGate.Rebuild(__instance, "SetupOptions.Postfix");
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.Init))]
	public static class Patch_SandboxOptionManager_Init
	{
		public static void Postfix(SandboxOptManager __instance)
		{
			DoomSandboxRebuildGate.Rebuild(__instance, "Init.Postfix");
		}
	}

	/// <summary>
	/// UI builds tabs from OptionsByCategory here. Rebuild immediately before that
	/// so worksheet changes are visible even if manager Init ran early/unpatched.
	/// </summary>
	[HarmonyPatch(typeof(XUiC_SandboxOptions), "setupOptions")]
	public static class Patch_XUiC_SandboxOptions_setupOptions
	{
		public static void Prefix()
		{
			var mgr = SandboxOptManager.Current;
			DoomSandboxRebuildGate.Rebuild(mgr, "XUiC_SandboxOptions.setupOptions.Prefix");
		}
	}

	/// <summary>Review tab: hide default/unchanged rows (vanilla paints them white).</summary>
	[HarmonyPatch(typeof(SandboxOptManager))]
	public static class Patch_GetChangedPresetOptions
	{
		static System.Reflection.MethodBase TargetMethod()
		{
			return AccessTools.Method(
				typeof(SandboxOptManager),
				nameof(SandboxOptManager.GetChangedPresetOptions),
				new[]
				{
					typeof(global::SandboxOptions.SandboxOptionPreset),
					typeof(List<(string name, string value, bool isDefault)>).MakeByRefType()
				});
		}

		public static void Postfix(ref List<(string name, string value, bool isDefault)> valuesList)
		{
			if (valuesList == null || valuesList.Count == 0)
				return;
			valuesList.RemoveAll(t => t.isDefault);
		}
	}

	[HarmonyPatch(typeof(XUiC_TabSelectorTab), "set_TabKey")]
	public static class Patch_TabSelectorTab_set_TabKey
	{
		public static void Postfix(XUiC_TabSelectorTab __instance, string __0)
		{
			// Friendly fallback when localization key is missing (shows raw KEY otherwise).
			if (string.IsNullOrEmpty(__0))
				return;
			if (__0.Equals("sandboxOptionCategoryDoom", StringComparison.OrdinalIgnoreCase))
				__instance.TabHeaderText = "Doom";
			else if (__0.Equals("sandboxOptionCategoryLocked", StringComparison.OrdinalIgnoreCase))
				__instance.TabHeaderText = "Locked";
			else if (__0.Equals("sandboxOptionCategoryLocked 2", StringComparison.OrdinalIgnoreCase)
				|| __0.Equals("sandboxOptionCategoryLocked2", StringComparison.OrdinalIgnoreCase)
				|| __0.StartsWith("sandboxOptionCategoryLocked", StringComparison.OrdinalIgnoreCase))
				__instance.TabHeaderText = __0.IndexOf("2", StringComparison.OrdinalIgnoreCase) >= 0 ? "Locked 2" : "Locked";
			else if (__0.Equals("sandboxOptionCategoryReview", StringComparison.OrdinalIgnoreCase))
				__instance.TabHeaderText = "Review";
		}
	}

	/// <summary>Block edits to forced single-choice rules if they ever appear in UI.</summary>
	[HarmonyPatch(typeof(XUiC_SandBoxOptionEntry), "ControlCombo_OnValueChanged")]
	public static class Patch_SandBoxOptionEntry_OnValueChanged
	{
		public static bool Prefix(XUiC_SandBoxOptionEntry __instance)
		{
			var opt = __instance.Option;
			if (opt != null && DoomSandboxRuntime.LockedOptions.Contains(opt.Option))
			{
				__instance.SetComboBoxIndex(opt.GetDefaultIndex());
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Preset apply indexes UI entries by enum. Options that didn't fit in a tab's
	/// XUi slots are missing from that dictionary and throw KeyNotFoundException.
	/// </summary>
	[HarmonyPatch(typeof(XUiC_SandboxOptions), "updateOptionsToPreset")]
	public static class Patch_XUiC_SandboxOptions_updateOptionsToPreset
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is KeyNotFoundException kne)
			{
				DoomLog.Error("updateOptionsToPreset missing UI entry (overflow?): " + kne.Message);
				return null; // swallow — option values still live on SandboxOptionManager
			}
			return __exception;
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), "LoadInternalPresets")]
	public static class Patch_SandboxOptionManager_LoadInternalPresets
	{
		public static bool Prefix()
		{
			DoomLog.Info("LoadInternalPresets Prefix — blocking vanilla Official presets");
			return false;
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.LoadPresets))]
	public static class Patch_SandboxOptionManager_LoadPresets
	{
		public static void Postfix(SandboxOptManager __instance)
		{
			try
			{
				int before = __instance.SandboxPresets.Count;
				__instance.SandboxPresets.RemoveAll(IsVanillaOfficialPreset);
				DoomSandboxRebuilder.EnsureBeGentlePreset(__instance);
				DoomSandboxRebuilder.ApplyCustomPresetIcons(__instance);
				DoomLog.Info($"LoadPresets Postfix — presets {before} -> {__instance.SandboxPresets.Count}");
			}
			catch (Exception ex)
			{
				DoomLog.Error("LoadPresets strip failed: " + ex);
			}
		}

		static bool IsVanillaOfficialPreset(SandboxOptPreset p)
		{
			if (p == null)
				return true;
			if (p.IsCustomPreset)
				return false;
			if (p.IsModded)
				return false;
			if (string.Equals(p.Group, SandboxOptManager.ModdedGroupName, StringComparison.OrdinalIgnoreCase))
				return false;
			if (string.Equals(p.Group, SandboxOptManager.UserGroupName, StringComparison.OrdinalIgnoreCase))
				return false;
			if (string.Equals(p.Group, SandboxOptManager.CustomGroupName, StringComparison.OrdinalIgnoreCase))
				return false;
			if (string.Equals(p.Group, DoomSandboxRuntime.DifficultyPresetGroup, StringComparison.OrdinalIgnoreCase))
				return false;
			return true;
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.GetAllPresetGroups))]
	public static class Patch_SandboxOptionManager_GetAllPresetGroups
	{
		public static void Postfix(SandboxOptManager __instance, ref List<string> __result)
		{
			try
			{
				DoomSandboxRebuilder.EnsureBeGentlePreset(__instance);

				__instance.SandboxPresets.RemoveAll(p =>
					p == null
					|| (!p.IsCustomPreset && !p.IsModded
						&& !string.Equals(p.Group, SandboxOptManager.ModdedGroupName, StringComparison.OrdinalIgnoreCase)
						&& !string.Equals(p.Group, SandboxOptManager.UserGroupName, StringComparison.OrdinalIgnoreCase)
						&& !string.Equals(p.Group, SandboxOptManager.CustomGroupName, StringComparison.OrdinalIgnoreCase)
						&& !string.Equals(p.Group, DoomSandboxRuntime.DifficultyPresetGroup, StringComparison.OrdinalIgnoreCase)));

				if (__result == null)
					__result = new List<string>();

				// Prefer Difficulty first in the group spinner.
				__result.RemoveAll(g =>
					!string.Equals(g, SandboxOptManager.CustomGroupName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(g, SandboxOptManager.UserGroupName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(g, SandboxOptManager.ModdedGroupName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(g, DoomSandboxRuntime.DifficultyPresetGroup, StringComparison.OrdinalIgnoreCase));

				if (!__result.Contains(DoomSandboxRuntime.DifficultyPresetGroup)
					&& __instance.GetPresetsForGroup(DoomSandboxRuntime.DifficultyPresetGroup))
					__result.Insert(0, DoomSandboxRuntime.DifficultyPresetGroup);
			}
			catch (Exception ex)
			{
				DoomLog.Error("GetAllPresetGroups strip failed: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.RemoveOverrides))]
	public static class Patch_SandboxOptionManager_RemoveOverrides
	{
		public static void Postfix(SandboxOptManager __instance)
		{
			// CreateOverrides strips IsModded then reloads XML; re-seed after that pass via LoadPresetFromXml too.
			DoomSandboxRebuilder.EnsureBeGentlePreset(__instance);
		}
	}

	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.LoadPresetFromXml))]
	public static class Patch_SandboxOptionManager_LoadPresetFromXml
	{
		public static void Postfix(SandboxOptManager __instance)
		{
			DoomSandboxRebuilder.EnsureBeGentlePreset(__instance);
		}
	}

	/// <summary>Ensure Difficulty / Be Gentle! exist before the group combo is filled.</summary>
	[HarmonyPatch(typeof(XUiC_SandboxPresetSelector), "updatePresetGroups")]
	public static class Patch_SandboxPresetSelector_updatePresetGroups
	{
		public static void Prefix()
		{
			var mgr = SandboxOptManager.Current;
			if (mgr != null)
			{
				DoomSandboxRebuilder.EnsureBeGentlePreset(mgr);
				DoomSandboxRebuilder.ApplyCustomPresetIcons(mgr);
			}
		}
	}

	[HarmonyPatch(typeof(SandboxOptBase), "get_OptionNameText")]
	public static class Patch_BaseSandboxOption_OptionNameText
	{
		public static void Postfix(SandboxOptBase __instance, ref string __result)
		{
			// Prefer worksheet display names for options we own.
			if (DoomSandboxRuntime.VisibleOptions.Contains(__instance.Option)
				&& !string.IsNullOrEmpty(__instance.OptionName))
			{
				__result = __instance.OptionName;
				return;
			}

			if (string.IsNullOrEmpty(__result) && !string.IsNullOrEmpty(__instance.OptionName))
				__result = __instance.OptionName;
		}
	}

	[HarmonyPatch(typeof(SandboxOptBase), "get_DescriptionText")]
	public static class Patch_BaseSandboxOption_DescriptionText
	{
		public static void Postfix(SandboxOptBase __instance, ref string __result)
		{
			if (DoomSandboxRuntime.Descriptions.TryGetValue(__instance.Option, out var does)
				&& !string.IsNullOrEmpty(does))
			{
				__result = does;
			}
		}
	}
}
