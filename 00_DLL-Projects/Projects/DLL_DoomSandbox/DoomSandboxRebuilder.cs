using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SandboxOpt = global::SandboxOptions.SandboxOptions;
using SandboxOptFloat = global::SandboxOptions.SandboxOptionFloat;
using SandboxOptInt = global::SandboxOptions.SandboxOptionInt;
using SandboxOptBool = global::SandboxOptions.SandboxOptionBoolean;
using SandboxOptBase = global::SandboxOptions.BaseSandboxOption;
using SandboxOptManager = global::SandboxOptions.SandboxOptionManager;
using SandboxValueSet = global::SandboxOptions.SandboxOptionValueSet;
using SandboxValueSetFloat = global::SandboxOptions.SandboxOptionValueSetFloat;
using SandboxValueSetInt = global::SandboxOptions.SandboxOptionValueSetInt;
using SandboxValueSetBool = global::SandboxOptions.SandboxOptionValueSetBool;

namespace DoomSandbox
{
	public static class DoomSandboxRuntime
	{
		public const string LockedTabName = "Locked";
		/// <summary>Vanilla loc key → UI label "Difficulty".</summary>
		public const string DifficultyPresetGroup = "sandboxPresetGroupDifficulty";
		public const string BeGentlePresetName = "Be Gentle!";


		public static DoomSandboxConfig Config;
		public static readonly Dictionary<SandboxOpt, string> Descriptions = new Dictionary<SandboxOpt, string>();
		public static readonly HashSet<SandboxOpt> VisibleOptions = new HashSet<SandboxOpt>();
		public static readonly HashSet<SandboxOpt> LockedOptions = new HashSet<SandboxOpt>();
		public static readonly Dictionary<string, SandboxOpt> OptionByName =
			new Dictionary<string, SandboxOpt>(StringComparer.OrdinalIgnoreCase);

		public static float EnemyHealthMult = 1f;
		public static float PlayerDamageFlat = 0f;
		public static float ProjectileSpeedMult = 1f;
		public static float HitscanSpreadDegrees = 4.5f;
		public static float FlatGameStage = 0f;
		public static float FlatLootStage = 0f;
		public static float SkillGainLevelCap = 60f;
		public static bool FastMonsters;

		public static void RefreshFromManager()
		{
			var mgr = SandboxOptManager.Current;
			if (mgr == null || !mgr.IsInit)
				return;

			EnemyHealthMult = ReadNamed(mgr, "Enemy Health", SandboxOpt.EntityIncomingDamage, 1f);
			PlayerDamageFlat = ReadNamed(mgr, "Player Damage Taken", SandboxOpt.IncomingDamage, 0f);
			ProjectileSpeedMult = ReadNamed(mgr, "Enemy Projectile Speed", default(SandboxOpt), 1f);
			HitscanSpreadDegrees = ReadNamed(mgr, "Hitscanner Accuracy", default(SandboxOpt), 4.5f);
			FlatGameStage = ReadNamed(mgr, "Game stage modifier", SandboxOpt.GlobalGSModifier, 0f);
			FlatLootStage = ReadNamed(mgr, "Loot stage modifier", SandboxOpt.GlobalLSModifier, 0f);
			SkillGainLevelCap = ReadNamed(mgr, "Skill Gain Level Cap", default(SandboxOpt), 60f);
			FastMonsters = ReadNamedBool(mgr, "Fast Monsters", SandboxOpt.SillyBlackandWhite, false);
		}

		static float ReadNamed(SandboxOptManager mgr, string displayName, SandboxOpt fallbackOpt, float fallback)
		{
			SandboxOpt opt = fallbackOpt;
			if (OptionByName.TryGetValue(displayName, out var mapped))
				opt = mapped;
			return SafeFloat(mgr, opt, fallback);
		}

		static bool ReadNamedBool(SandboxOptManager mgr, string displayName, SandboxOpt fallbackOpt, bool fallback)
		{
			SandboxOpt opt = fallbackOpt;
			if (OptionByName.TryGetValue(displayName, out var mapped))
				opt = mapped;
			return SafeBool(mgr, opt, fallback);
		}

		static float SafeFloat(SandboxOptManager mgr, SandboxOpt opt, float fallback)
		{
			try
			{
				if (!mgr.SandboxOptionsDict.ContainsKey(opt))
					return fallback;
				return SandboxOptManager.GetFloat(opt);
			}
			catch
			{
				return fallback;
			}
		}

		static bool SafeBool(SandboxOptManager mgr, SandboxOpt opt, bool fallback)
		{
			try
			{
				if (!mgr.SandboxOptionsDict.ContainsKey(opt))
					return fallback;
				return SandboxOptManager.GetBool(opt);
			}
			catch
			{
				return fallback;
			}
		}
	}

	public static class DoomSandboxRebuilder
	{
		static readonly FieldInfo ValueSetNameField =
			AccessTools.Field(typeof(SandboxOptBase), "ValueSetName");

		static readonly PropertyInfo ValueOptionsProp =
			AccessTools.Property(typeof(SandboxOptBase), "ValueOptions");

		public static void Apply(SandboxOptManager mgr, DoomSandboxConfig cfg)
		{
			if (cfg == null || cfg.Options.Count == 0)
			{
				DoomLog.Error("No options in config; leaving vanilla sandbox UI.");
				return;
			}

			// Snapshot only to recover from a failed category rebuild (empty UI).
			// Never roll back to vanilla after Doom categories were successfully built.
			var backupCategories = new List<KeyValuePair<string, List<SandboxOptBase>>>();
			foreach (var kv in mgr.OptionsByCategory.dict)
				backupCategories.Add(new KeyValuePair<string, List<SandboxOptBase>>(kv.Key, new List<SandboxOptBase>(kv.Value)));

			bool categoriesCommitted = false;
			try
			{
				BuildCategories(mgr, cfg, out int lockedCount);
				categoriesCommitted = mgr.OptionsByCategory.Count > 0;
				if (!categoriesCommitted)
					throw new InvalidOperationException("Rebuild produced zero option categories.");

				try { EnsureDifficultyPresets(mgr, force: true); }
				catch (Exception ex) { DoomLog.Error("EnsureDifficultyPresets failed: " + ex); }

				try { ApplyCustomPresetIcons(mgr); }
				catch (Exception ex) { DoomLog.Error("ApplyCustomPresetIcons failed: " + ex); }

				try { DoomSandboxRuntime.RefreshFromManager(); }
				catch (Exception ex) { DoomLog.Error("RefreshFromManager failed: " + ex); }

				DoomLog.Info(
					$"Sandbox UI rebuilt: {DoomSandboxRuntime.VisibleOptions.Count - lockedCount} editable + " +
					$"{lockedCount} locked across {mgr.OptionsByCategory.Count} tabs.");
			}
			catch (Exception ex)
			{
				DoomLog.Error("Rebuild category pass failed: " + ex);
				if (!categoriesCommitted)
				{
					DoomLog.Error("Restoring prior sandbox categories (avoid empty UI).");
					mgr.OptionsByCategory.Clear();
					foreach (var kv in backupCategories)
					{
						if (!mgr.OptionsByCategory.dict.ContainsKey(kv.Key))
							mgr.OptionsByCategory.Add(kv.Key, kv.Value);
					}
				}
				throw;
			}
		}

		static void BuildCategories(SandboxOptManager mgr, DoomSandboxConfig cfg, out int lockedCount)
		{
			DoomSandboxRuntime.Config = cfg;
			DoomSandboxRuntime.Descriptions.Clear();
			DoomSandboxRuntime.VisibleOptions.Clear();
			DoomSandboxRuntime.LockedOptions.Clear();
			DoomSandboxRuntime.OptionByName.Clear();

			var vanillaByName = new Dictionary<string, SandboxOpt>(StringComparer.OrdinalIgnoreCase);
			foreach (var kv in mgr.SandboxOptionsDict)
				vanillaByName[kv.Value.OptionName] = kv.Key;

			mgr.OptionsByCategory.Clear();

			var lockedByTab = new Dictionary<string, List<SandboxOptBase>>(StringComparer.OrdinalIgnoreCase);
			int valueSetIndex = 0;
			lockedCount = 0;

			foreach (var def in cfg.Options)
			{
				try
				{
					if (!DoomSandboxNameMap.TryResolve(def.Name, vanillaByName, DoomSandboxRuntime.VisibleOptions, out var enumId))
					{
						DoomLog.Error("No enum slot for option: " + def.Name);
						continue;
					}

					if (!mgr.SandboxOptionsDict.TryGetValue(enumId, out SandboxOptBase option))
					{
						option = CreateMissingOption(mgr, enumId, def);
						if (option == null)
						{
							DoomLog.Error("Could not create option: " + def.Name);
							continue;
						}
					}
					else if (DoomSandboxValueMaps.MustRemainInt(enumId) && option is SandboxOptFloat)
					{
						// Undo prior buggy Float coercion so GetInt is not value*100.
						mgr.SandboxOptionsDict.Remove(enumId);
						option = new SandboxOptInt(enumId, def.Name, def.Tab ?? "World", "DamageValues", 0);
						mgr.SandboxOptionsDict.Add(enumId, option);
						DoomLog.Info($"Restored Int slot for '{def.Name}' ({enumId})");
					}
					else if (NeedsFloatSlot(def) && !(option is SandboxOptFloat) && !(option is SandboxOptInt))
					{
						// Only coerce Bool/non-int silly slots to float (projectile speed, skill cap, etc.).
						// NEVER replace Int options — GetInt on Float returns value*100 and breaks
						// Air Drops / Day Light Length / reset intervals / Feral Sense ladders.
						option = ReplaceWithFloatOption(mgr, enumId, def);
					}

					// Single-choice = Locked (respect worksheet Locked / Locked 2 when present).
					bool isLocked = def.Choices != null && def.Choices.Length == 1;
					string tab = def.Tab;
					if (isLocked)
					{
						if (string.IsNullOrEmpty(tab)
							|| !tab.StartsWith("Locked", StringComparison.OrdinalIgnoreCase))
							tab = DoomSandboxRuntime.LockedTabName;
					}

					option.OptionName = def.Name;
					option.CategoryName = tab;
					option.OverrideOptionName = null;
					option.OverrideDescriptionName = null;
					// Must stay enabled or the combo shows "----" instead of the value.
					option.IsEnabled = true;
					option.DisabledByText = isLocked ? "Locked for Doom Survival" : "";
					DoomSandboxRuntime.Descriptions[enumId] = def.Does ?? "";
					DoomSandboxRuntime.VisibleOptions.Add(enumId);
					DoomSandboxRuntime.OptionByName[def.Name] = enumId;
					if (isLocked)
						DoomSandboxRuntime.LockedOptions.Add(enumId);

					var setName = "DoomSet_" + (valueSetIndex++);
					var valueSet = BuildValueSet(option, def, out float defaultFloat, out int defaultInt, out bool defaultBool);
					if (valueSet == null)
					{
						DoomLog.Error("Could not build values for: " + def.Name);
						continue;
					}

					mgr.ValueSets[setName] = valueSet;
					valueSet.Init();
					ValueSetNameField?.SetValue(option, setName);
					ValueOptionsProp?.SetValue(option, null, null);

					// Defaults come from BuildValueSet (incl. semantic ladders). Do not
					// force index 0 for non-numeric locked labels — that broke smell/chicken.

					ApplyDefault(option, defaultFloat, defaultInt, defaultBool);

					if (isLocked)
					{
						if (!lockedByTab.TryGetValue(tab, out var list))
						{
							list = new List<SandboxOptBase>();
							lockedByTab[tab] = list;
						}
						list.Add(option);
						lockedCount++;
					}
					else
					{
						if (!mgr.OptionsByCategory.dict.ContainsKey(tab))
							mgr.OptionsByCategory.Add(tab, new List<SandboxOptBase>());
						mgr.OptionsByCategory.dict[tab].Add(option);
					}
				}
				catch (Exception ex)
				{
					DoomLog.Error($"Option '{def?.Name}' failed during rebuild: " + ex);
				}
			}

			foreach (var kv in lockedByTab)
			{
				string tabName = kv.Key;
				// Normalize "Locked 2" → category key the UI can page (space ok; matches worksheet).
				if (!mgr.OptionsByCategory.dict.ContainsKey(tabName))
					mgr.OptionsByCategory.Add(tabName, kv.Value);
				else
					mgr.OptionsByCategory.dict[tabName].AddRange(kv.Value);
			}

			foreach (var kv in mgr.SandboxOptionsDict)
			{
				if (!DoomSandboxRuntime.VisibleOptions.Contains(kv.Key))
					kv.Value.SetToDefault();
			}

			// Doom uses stamina as armour — never allow vanilla stamina regen.
			ForceFloatOption(mgr, SandboxOpt.StaminaRegen, 0f);

			// ggs / ItemActionEat: Jar Refund is locked to none — pin float 0 (vanilla default is 0.6).
			ForceFloatOption(mgr, SandboxOpt.JarRefund, 0f);
			// Locked Tech3 / Elites pins (worksheet Locked defaults).
			ForceIntOptionCurrent(mgr, SandboxOpt.MaxTechType, 4);
			ForceIntOptionCurrent(mgr, SandboxOpt.MaxEnemyTier, 5);

			try { AuditWorksheetMappings(mgr, cfg); }
			catch (Exception ex) { DoomLog.Error("AUDIT crashed: " + ex); }

			// Make Custom / Defaults use worksheet values (otherwise OnOpen reapplies stale vanilla Custom).
			try
			{
				if (SandboxOptManager.CustomPreset != null)
				{
					var save = AccessTools.Method(
						typeof(SandboxOptManager),
						"SaveCurrentToPreset",
						new[] { typeof(global::SandboxOptions.SandboxOptionPreset) });
					save?.Invoke(mgr, new object[] { SandboxOptManager.CustomPreset });
					DoomLog.Info("Synced worksheet defaults into Custom preset.");
				}
			}
			catch (Exception ex)
			{
				DoomLog.Error("SaveCurrentToPreset(Custom) failed: " + ex.Message);
			}
		}

		/// <summary>
		/// Difficulty-group presets from the blueprint compiled into this DLL.
		/// Falls back to a single Be Gentle! preset if that blueprint is missing/empty.
		/// </summary>
		static string _difficultyEnsureStamp;

		internal static void EnsureDifficultyPresets(SandboxOptManager mgr, bool force = false)
		{
			if (mgr?.SandboxPresets == null)
				return;

			string stamp = DoomSandboxBlueprint.Stamp + ":" + DoomSandboxRuntime.VisibleOptions.Count;

			bool hasDifficulty = false;
			foreach (var p in mgr.SandboxPresets)
			{
				if (p != null && string.Equals(p.Group, DoomSandboxRuntime.DifficultyPresetGroup, StringComparison.OrdinalIgnoreCase))
				{
					hasDifficulty = true;
					break;
				}
			}

			if (!force && hasDifficulty && stamp == _difficultyEnsureStamp)
				return;

			var defs = DoomSandboxDifficulties.Load();
			DoomSandboxDifficulties.ApplyToManager(mgr, defs);
			_difficultyEnsureStamp = stamp;
		}

		/// <summary>Compat alias used by older patch call sites.</summary>
		internal static void EnsureBeGentlePreset(SandboxOptManager mgr) =>
			EnsureDifficultyPresets(mgr);

		/// <summary>Replace vanilla user_custom art on Custom + User presets.</summary>
		internal static void ApplyCustomPresetIcons(SandboxOptManager mgr)
		{
			string icon = DoomSandboxMod.CustomPresetIconPath;
			if (SandboxOptManager.CustomPreset != null)
				SandboxOptManager.CustomPreset.Icon = icon;

			if (mgr?.SandboxPresets == null)
				return;

			foreach (var p in mgr.SandboxPresets)
			{
				if (p == null)
					continue;
				if (p.IsCustomPreset || p.IsUserPreset
					|| string.Equals(p.Group, SandboxOptManager.UserGroupName, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(p.Group, SandboxOptManager.CustomGroupName, StringComparison.OrdinalIgnoreCase))
				{
					p.Icon = icon;
				}
			}
		}

		static void ForceFloatOption(SandboxOptManager mgr, SandboxOpt enumId, float value)
		{
			if (!mgr.SandboxOptionsDict.TryGetValue(enumId, out var opt))
				return;
			var f = opt as SandboxOptFloat;
			if (f == null)
				return;
			f.DefaultValue = value;
			f.CurrentValue = value;
		}

		static void ForceIntOptionCurrent(SandboxOptManager mgr, SandboxOpt enumId, int value)
		{
			if (!mgr.SandboxOptionsDict.TryGetValue(enumId, out var opt))
				return;
			var i = opt as SandboxOptInt;
			if (i == null)
				return;
			i.DefaultValue = value;
			i.CurrentValue = value;
		}

		/// <summary>
		/// These enums must stay Int. Float.GetIntValue returns CurrentValue*100 and breaks
		/// SetupAirDropTimeRanges / CalcDuskDawnHours / smell / chicken / resets.
		/// </summary>
		static bool MustRemainIntOption(SandboxOpt enumId) =>
			DoomSandboxValueMaps.MustRemainInt(enumId);

		static void AuditWorksheetMappings(SandboxOptManager mgr, DoomSandboxConfig cfg)
		{
			if (cfg?.Options == null || mgr == null)
				return;

			int issues = 0;
			foreach (var def in cfg.Options)
			{
				if (!DoomSandboxRuntime.OptionByName.TryGetValue(def.Name, out var enumId))
				{
					DoomLog.Error($"AUDIT: worksheet '{def.Name}' did not bind to any SandboxOpt");
					issues++;
					continue;
				}
				if (!mgr.SandboxOptionsDict.TryGetValue(enumId, out var option) || option == null)
				{
					DoomLog.Error($"AUDIT: '{def.Name}' → {enumId} missing from manager");
					issues++;
					continue;
				}

				string wanted = string.IsNullOrEmpty(def.Default) ? (def.Choices?.Length > 0 ? def.Choices[0] : "") : def.Default;
				if (string.IsNullOrEmpty(wanted))
					continue;

				if (option is SandboxOptInt)
				{
					if (DoomSandboxValueMaps.TryMapInt(enumId, def.Name, wanted, out int expect))
					{
						int live = SandboxOptManager.GetInt(enumId);
						if (live != expect)
						{
							DoomLog.Error($"AUDIT MISMATCH '{def.Name}': choice '{wanted}' expects {expect}, live GetInt={live}");
							issues++;
						}
					}
					else if (def.Choices != null && def.Choices.Length == 1
						&& !DoomSandboxConfig.TryParsePercentOrNumber(wanted, out _)
						&& !IsYes(wanted) && !IsNo(wanted))
					{
						// Single non-numeric locked label with no semantic map — likely wrong (Tech3 bug class).
						DoomLog.Error($"AUDIT UNMAPPED locked label '{def.Name}' = '{wanted}' (enum {enumId}) — may be using index 0");
						issues++;
					}
				}
				else if (option is SandboxOptFloat)
				{
					if (DoomSandboxValueMaps.TryMapFloat(enumId, def.Name, wanted, out float expectF))
					{
						float live = SandboxOptManager.GetFloat(enumId);
						if (Math.Abs(live - expectF) > 0.0001f)
						{
							DoomLog.Error($"AUDIT MISMATCH '{def.Name}': choice '{wanted}' expects {expectF}, live GetFloat={live}");
							issues++;
						}
					}
				}
				else if (option is SandboxOptBool)
				{
					if (DoomSandboxValueMaps.TryMapBool(enumId, def.Name, wanted, out bool expectB))
					{
						bool live = SandboxOptManager.GetBool(enumId);
						if (live != expectB)
						{
							DoomLog.Error($"AUDIT MISMATCH '{def.Name}': choice '{wanted}' expects {expectB}, live GetBool={live}");
							issues++;
						}
					}
				}
			}

			if (issues == 0)
				DoomLog.Info($"AUDIT: {cfg.Options.Count} worksheet options OK (no semantic mismatches).");
			else
				DoomLog.Error($"AUDIT: {issues} issue(s) found across {cfg.Options.Count} worksheet options.");
		}

		static bool NeedsFloatSlot(DoomSandboxOptionDef def)
		{
			if (def?.Choices == null || def.Choices.Length == 0)
				return false;
			if (IsYesNo(def.Choices))
				return false;
			foreach (var c in def.Choices)
			{
				if (DoomSandboxConfig.TryParsePercentOrNumber(c, out _))
					return true;
				if (c.IndexOf("degree", StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			}
			// Numeric ladders like "10, 20, 30" already covered; skill cap too.
			return def.Name.IndexOf("Skill Gain Level Cap", StringComparison.OrdinalIgnoreCase) >= 0
				|| def.Name.IndexOf("Hitscanner", StringComparison.OrdinalIgnoreCase) >= 0
				|| def.Name.IndexOf("Projectile", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		static SandboxOptBase ReplaceWithFloatOption(SandboxOptManager mgr, SandboxOpt enumId, DoomSandboxOptionDef def)
		{
			// Do NOT call AddSandboxOption — it also appends OptionsByCategory and duplicates the enum in the UI dict.
			mgr.SandboxOptionsDict.Remove(enumId);
			var option = new SandboxOptFloat(enumId, def.Name, def.Tab ?? "Doom", "DamageValues", 1f, false, null);
			mgr.SandboxOptionsDict.Add(enumId, option);
			DoomLog.Info($"Replaced {enumId} with Float slot for '{def.Name}'");
			return option;
		}

		static SandboxOptBase CreateMissingOption(SandboxOptManager mgr, SandboxOpt enumId, DoomSandboxOptionDef def)
		{
			var option = new SandboxOptFloat(enumId, def.Name, def.Tab, "DamageValues", 1f, false, null);
			Traverse.Create(mgr).Method("AddSandboxOption", option).GetValue();
			return option;
		}

		static SandboxValueSet BuildValueSet(
			SandboxOptBase option,
			DoomSandboxOptionDef def,
			out float defaultFloat,
			out int defaultInt,
			out bool defaultBool)
		{
			defaultFloat = 1f;
			defaultInt = 0;
			defaultBool = false;

			var choices = def.Choices;
			if (choices == null || choices.Length == 0)
				return null;

			bool isBoolOption = option.OptionType == SandboxOptBase.OptionTypes.Bool;
			bool isIntOption = option.OptionType == SandboxOptBase.OptionTypes.Int;
			bool isFloatOption = option.OptionType == SandboxOptBase.OptionTypes.Float;

			if (IsYesNo(choices) || (choices.Length == 1 && (IsYes(choices[0]) || IsNo(choices[0]))))
			{
				defaultBool = choices.Length == 1 ? IsYes(choices[0]) : IsYes(def.Default);
				defaultInt = defaultBool ? 1 : 0;
				defaultFloat = defaultInt;

				// Only real Boolean options can use Bool value sets. Float/Int (e.g. Show Location Info)
				// must use numeric sets or the combo renders blank.
				if (isBoolOption)
				{
					if (choices.Length == 1)
					{
						return new SandboxValueSetBool
						{
							DisplayValues = new[] { defaultBool ? "xuiYes" : "xuiNo" },
							BoolValues = new[] { defaultBool }
						};
					}
					return new SandboxValueSetBool
					{
						DisplayValues = new[] { "xuiNo", "xuiYes" },
						BoolValues = new[] { false, true }
					};
				}

				var yesNoDisplay = choices.Length == 1
					? new[] { choices[0] }
					: (string[])choices.Clone();
				var yesNoVals = choices.Length == 1
					? new[] { defaultInt }
					: new[] { 0, 1 };
				if (choices.Length == 2)
				{
					yesNoVals[0] = IsYes(choices[0]) ? 1 : 0;
					yesNoVals[1] = IsYes(choices[1]) ? 1 : 0;
					defaultInt = IsYes(def.Default) ? 1 : 0;
					defaultFloat = defaultInt;
				}

				if (isIntOption)
				{
					return new SandboxValueSetInt
					{
						IntValues = yesNoVals,
						DisplayValues = yesNoDisplay
					};
				}

				var asFloat = new float[yesNoVals.Length];
				for (int i = 0; i < yesNoVals.Length; i++)
					asFloat[i] = yesNoVals[i];
				return new SandboxValueSetFloat
				{
					FloatValues = asFloat,
					DisplayValues = yesNoDisplay
				};
			}

			if (TryBuildPlayerDamageFlat(def, choices, out defaultFloat, out var flatSet))
				return flatSet;

			if (TryBuildSemanticBool(option, def, choices, out defaultBool, out var semanticBool))
			{
				defaultInt = defaultBool ? 1 : 0;
				defaultFloat = defaultInt;
				return semanticBool;
			}

			if (TryBuildSemanticFloatLadder(option, def, choices, out defaultFloat, out var semanticFloat))
			{
				defaultInt = (int)Math.Round(defaultFloat);
				return semanticFloat;
			}

			if (TryBuildSemanticIntLadder(option, def, choices, out defaultInt, out var semanticInt))
			{
				defaultFloat = defaultInt;
				return semanticInt;
			}

			if (AllParseAsFloat(choices, out var floats))
			{
				defaultFloat = ParseDefaultFloat(def.Default, floats, choices);

				// Int options (Crafting Max Tier, etc.) need Int value sets or defaults/red-state break.
				if (isIntOption)
				{
					var ints = new int[floats.Length];
					for (int i = 0; i < floats.Length; i++)
						ints[i] = (int)Math.Round(floats[i]);
					defaultInt = (int)Math.Round(defaultFloat);
					return new SandboxValueSetInt
					{
						IntValues = ints,
						DisplayValues = (string[])choices.Clone()
					};
				}

				return new SandboxValueSetFloat
				{
					FloatValues = floats,
					DisplayValues = (string[])choices.Clone()
				};
			}

			var indexInts = new int[choices.Length];
			for (int i = 0; i < choices.Length; i++)
				indexInts[i] = i;

			defaultInt = IndexOfChoice(choices, def.Default);
			if (defaultInt < 0)
				defaultInt = 0;
			defaultFloat = defaultInt;

			if (isFloatOption || (!isIntOption && !isBoolOption))
			{
				var asFloat = new float[choices.Length];
				for (int i = 0; i < choices.Length; i++)
					asFloat[i] = i;
				return new SandboxValueSetFloat
				{
					FloatValues = asFloat,
					DisplayValues = (string[])choices.Clone()
				};
			}

			return new SandboxValueSetInt
			{
				IntValues = indexInts,
				DisplayValues = (string[])choices.Clone()
			};
		}

		static bool TryBuildSemanticIntLadder(
			SandboxOptBase option,
			DoomSandboxOptionDef def,
			string[] choices,
			out int defaultInt,
			out SandboxValueSetInt valueSet)
		{
			defaultInt = 0;
			valueSet = null;

			var enumId = option.Option;
			if (!DoomSandboxValueMaps.NeedsSemanticInt(enumId, def.Name))
				return false;

			var ints = new int[choices.Length];
			bool any = false;
			for (int i = 0; i < choices.Length; i++)
			{
				if (DoomSandboxValueMaps.TryMapInt(enumId, def.Name, choices[i], out ints[i]))
					any = true;
				else
					ints[i] = i;
			}
			if (!any)
				return false;

			int defIdx = IndexOfChoice(choices, def.Default);
			if (defIdx < 0)
				defIdx = 0;
			if (!DoomSandboxValueMaps.TryMapInt(enumId, def.Name, def.Default, out defaultInt))
				defaultInt = ints[Math.Min(defIdx, ints.Length - 1)];

			valueSet = new SandboxValueSetInt
			{
				IntValues = ints,
				DisplayValues = (string[])choices.Clone()
			};
			return true;
		}

		static bool TryBuildSemanticFloatLadder(
			SandboxOptBase option,
			DoomSandboxOptionDef def,
			string[] choices,
			out float defaultFloat,
			out SandboxValueSetFloat valueSet)
		{
			defaultFloat = 0f;
			valueSet = null;

			var floats = new float[choices.Length];
			bool any = false;
			for (int i = 0; i < choices.Length; i++)
			{
				if (DoomSandboxValueMaps.TryMapFloat(option.Option, def.Name, choices[i], out floats[i]))
					any = true;
				else
					return false;
			}
			if (!any)
				return false;

			if (!DoomSandboxValueMaps.TryMapFloat(option.Option, def.Name, def.Default, out defaultFloat))
			{
				int defIdx = IndexOfChoice(choices, def.Default);
				defaultFloat = floats[Math.Max(0, defIdx)];
			}

			valueSet = new SandboxValueSetFloat
			{
				FloatValues = floats,
				DisplayValues = (string[])choices.Clone()
			};
			return true;
		}

		static bool TryBuildSemanticBool(
			SandboxOptBase option,
			DoomSandboxOptionDef def,
			string[] choices,
			out bool defaultBool,
			out SandboxValueSetBool valueSet)
		{
			defaultBool = false;
			valueSet = null;
			if (option.OptionType != SandboxOptBase.OptionTypes.Bool)
				return false;

			var bools = new bool[choices.Length];
			for (int i = 0; i < choices.Length; i++)
			{
				if (!DoomSandboxValueMaps.TryMapBool(option.Option, def.Name, choices[i], out bools[i]))
					return false;
			}

			if (!DoomSandboxValueMaps.TryMapBool(option.Option, def.Name, def.Default, out defaultBool))
			{
				int defIdx = IndexOfChoice(choices, def.Default);
				defaultBool = bools[Math.Max(0, defIdx)];
			}

			valueSet = new SandboxValueSetBool
			{
				BoolValues = bools,
				DisplayValues = (string[])choices.Clone()
			};
			return true;
		}

		static bool NeedsSemanticIntLadder(SandboxOpt enumId, string optionName) =>
			DoomSandboxValueMaps.NeedsSemanticInt(enumId, optionName);

		static bool TryMapSemanticInt(SandboxOpt enumId, string optionName, string choice, out int value) =>
			DoomSandboxValueMaps.TryMapInt(enumId, optionName, choice, out value);

		static void ApplyDefault(SandboxOptBase option, float defaultFloat, int defaultInt, bool defaultBool)
		{
			var f = option as SandboxOptFloat;
			if (f != null)
			{
				f.DefaultValue = defaultFloat;
				f.CurrentValue = defaultFloat;
				return;
			}

			var i = option as SandboxOptInt;
			if (i != null)
			{
				i.DefaultValue = defaultInt;
				i.CurrentValue = defaultInt;
				return;
			}

			var b = option as SandboxOptBool;
			if (b != null)
			{
				b.DefaultValue = defaultBool;
				b.CurrentValue = defaultBool;
			}
		}

		static bool TryBuildPlayerDamageFlat(
			DoomSandboxOptionDef def,
			string[] choices,
			out float defaultFloat,
			out SandboxValueSetFloat valueSet)
		{
			defaultFloat = 0f;
			valueSet = null;

			bool looksLike =
				def.Name.IndexOf("Player Damage Taken", StringComparison.OrdinalIgnoreCase) >= 0
				|| (choices.Length >= 2 && Array.Exists(choices, c =>
					c.IndexOf("health", StringComparison.OrdinalIgnoreCase) >= 0
					&& c.IndexOf("armour", StringComparison.OrdinalIgnoreCase) >= 0));
			if (!looksLike)
				return false;

			var floats = new float[choices.Length];
			for (int i = 0; i < choices.Length; i++)
				floats[i] = ParsePlayerDamageFlatChoice(choices[i], i, choices.Length);

			defaultFloat = ParsePlayerDamageFlatChoice(def.Default, IndexOfChoice(choices, def.Default), choices.Length);
			valueSet = new SandboxValueSetFloat
			{
				FloatValues = floats,
				DisplayValues = (string[])choices.Clone()
			};
			return true;
		}

		static float ParsePlayerDamageFlatChoice(string text, int indexFallback, int count)
		{
			if (string.IsNullOrEmpty(text)
				|| text.Equals("none", StringComparison.OrdinalIgnoreCase)
				|| text.Equals("No Modifier", StringComparison.OrdinalIgnoreCase)
				|| text.Equals("Default", StringComparison.OrdinalIgnoreCase)
				|| text.IndexOf("no modifier", StringComparison.OrdinalIgnoreCase) >= 0)
				return 0f;

			var m = System.Text.RegularExpressions.Regex.Match(text.Trim(), @"^[+\-]?\d+");
			if (m.Success && float.TryParse(m.Value, System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out var v))
				return v;

			if (count == 3)
			{
				if (indexFallback <= 0) return -1f;
				if (indexFallback == 1) return 0f;
				return 1f;
			}

			return indexFallback;
		}

		static bool IsYesNo(string[] choices)
		{
			if (choices.Length != 2)
				return false;
			return (IsYes(choices[0]) || IsNo(choices[0])) && (IsYes(choices[1]) || IsNo(choices[1]));
		}

		static bool IsYes(string s) =>
			s.Equals("Yes", StringComparison.OrdinalIgnoreCase) || s.Equals("true", StringComparison.OrdinalIgnoreCase);

		static bool IsNo(string s) =>
			s.Equals("No", StringComparison.OrdinalIgnoreCase) || s.Equals("false", StringComparison.OrdinalIgnoreCase);

		static bool AllParseAsFloat(string[] choices, out float[] values)
		{
			values = new float[choices.Length];
			for (int i = 0; i < choices.Length; i++)
			{
				if (!DoomSandboxConfig.TryParsePercentOrNumber(choices[i], out values[i]))
					return false;
			}
			return true;
		}

		static float ParseDefaultFloat(string defaultText, float[] values, string[] choices)
		{
			if (DoomSandboxConfig.TryParsePercentOrNumber(defaultText, out var v))
			{
				for (int i = 0; i < values.Length; i++)
				{
					if (Math.Abs(values[i] - v) < 0.0001f)
						return values[i];
				}
			}

			int idx = IndexOfChoice(choices, defaultText);
			if (idx >= 0 && idx < values.Length)
				return values[idx];

			return values.Length > 0 ? values[0] : 0f;
		}

		static int IndexOfChoice(string[] choices, string defaultText)
		{
			for (int i = 0; i < choices.Length; i++)
			{
				if (string.Equals(choices[i], defaultText, StringComparison.OrdinalIgnoreCase))
					return i;
			}
			return -1;
		}
	}
}
