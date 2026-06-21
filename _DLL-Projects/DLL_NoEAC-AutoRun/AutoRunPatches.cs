using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using InControl;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace AutoRun
{
    internal static class AutoRunBindingManager
    {
        private const string AutoRunActionName = "AutoRunActivate";
        private const string AutoRunActionNameKey = "inpActAutoRunName";
        private const string AutoRunActionDescKey = "inpActAutoRunDesc";
        private const string RunActionNameKey = "inpActPlayerRunName";
        private const string AutoRunBindingStoreKey = "autorun";

        private static readonly string AutoRunBindingStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DaysToDie", "autorun.bindingstore.txt");
        private static readonly FieldInfo ControllerRebindableActionsField = AccessTools.Field(typeof(PlayerActionsLocal), "ControllerRebindableActions");
        private static readonly FieldInfo PlayerActionSetActionsField = AccessTools.Field(typeof(PlayerActionSet), "actions");
        private static readonly FieldInfo ReadOnlyCollectionListField = AccessTools.Field(typeof(System.Collections.ObjectModel.ReadOnlyCollection<PlayerAction>), "list");
        private static readonly FieldInfo KeyComboIncludeSizeField = AccessTools.Field(typeof(KeyCombo), "includeSize");
        private static readonly FieldInfo KeyComboIncludeDataField = AccessTools.Field(typeof(KeyCombo), "includeData");
        private static readonly FieldInfo KeyComboExcludeSizeField = AccessTools.Field(typeof(KeyCombo), "excludeSize");
        private static readonly FieldInfo KeyComboExcludeDataField = AccessTools.Field(typeof(KeyCombo), "excludeData");
        private static readonly Dictionary<Type, PropertyInfo> BindingEntryActionPropertyCache = new Dictionary<Type, PropertyInfo>();

        private static MethodInfo _createPlayerActionMethod;
        private static bool _triedResolveCreatePlayerAction;

        public static bool IsActivationPressed(EntityPlayerLocal player)
        {
            PlayerAction action = GetActivationAction(player?.playerInput);
            if (action != null)
            {
                return action.IsPressed;
            }

            // Fallback for cases where injected actions are unavailable.
            return Input.GetKey(KeyCode.Z);
        }

        public static void EnsureInstalled(PlayerActionsLocal actions)
        {
            if (actions == null || actions.Run == null)
            {
                return;
            }

            PlayerAction action = GetActivationAction(actions);
            bool createdAction = false;
            if (action == null)
            {
                action = CreatePlayerAction(actions, AutoRunActionName);
                createdAction = action != null;
            }

            if (action == null)
            {
                return;
            }

            ApplyUserData(actions, action);

            if (createdAction && !HasKeyboardBinding(action))
            {
                action.AddDefaultBinding(Key.Z);
            }

            AddRebindableActionIfMissing(actions, action);
            MoveActionAfterRun(actions, action);

            if (createdAction || Patch_GameOptionsControls_Load.IsRunningLoadControls)
            {
                TryRestoreStoredBinding(action);
            }
        }

        public static PlayerActionsLocal TryGetLocalActionsFromPlatformInput()
        {
            try
            {
                IList<PlayerActionsBase> actionSets = PlatformManager.NativePlatform?.Input?.ActionSets;
                if (actionSets == null)
                {
                    return null;
                }

                for (int i = 0; i < actionSets.Count; i++)
                {
                    if (actionSets[i] is PlayerActionsLocal local)
                    {
                        return local;
                    }
                }
            }
            catch
            {
                // Ignore; caller can safely continue without injected binding.
            }

            return null;
        }

        public static void EnsureKeyboardEntryOrder(XUiC_OptionsControls options)
        {
            if (options?.TabSelector == null)
            {
                return;
            }

            for (int tabIndex = 1; tabIndex <= 12; tabIndex++)
            {
                XUiController tab;
                try
                {
                    tab = options.TabSelector.GetTab(tabIndex);
                }
                catch
                {
                    break;
                }

                if (tab == null)
                {
                    continue;
                }

                object[] entries = GetChildControllersUntyped(tab);
                if (entries == null || entries.Length == 0)
                {
                    continue;
                }

                int runIndex = FindBindingEntry(entries, RunActionNameKey);
                int autoRunIndex = FindBindingEntry(entries, AutoRunActionNameKey);
                if (runIndex < 0 || autoRunIndex < 0)
                {
                    continue;
                }

                int targetIndex = runIndex + 1;
                if (targetIndex >= entries.Length || targetIndex == autoRunIndex)
                {
                    return;
                }

                PlayerAction action = GetBindingEntryAction(entries[autoRunIndex]);
                if (action == null)
                {
                    return;
                }

                if (autoRunIndex > targetIndex)
                {
                    for (int i = autoRunIndex; i > targetIndex; i--)
                    {
                        SetBindingEntryAction(entries[i], GetBindingEntryAction(entries[i - 1]));
                    }
                }
                else
                {
                    for (int i = autoRunIndex; i < targetIndex; i++)
                    {
                        SetBindingEntryAction(entries[i], GetBindingEntryAction(entries[i + 1]));
                    }
                }

                SetBindingEntryAction(entries[targetIndex], action);
                return;
            }
        }

        private static void ApplyUserData(PlayerActionsLocal actions, PlayerAction action)
        {
            PlayerActionData.ActionUserData runData = actions.Run.UserData as PlayerActionData.ActionUserData;
            if (runData == null)
            {
                return;
            }

            action.UserData = new PlayerActionData.ActionUserData(
                AutoRunActionNameKey,
                AutoRunActionDescKey,
                runData.actionGroup,
                PlayerActionData.EAppliesToInputType.KbdMouseOnly,
                _allowRebind: true,
                _allowMultipleRebindings: false,
                _doNotDisplay: false,
                _defaultOnStartup: runData.defaultOnStartup);
        }

        private static bool HasKeyboardBinding(PlayerAction action)
        {
            if (action == null)
            {
                return false;
            }

            foreach (BindingSource binding in action.Bindings)
            {
                if (binding is KeyBindingSource)
                {
                    return true;
                }
            }

            return false;
        }

        private static PlayerAction GetActivationAction(PlayerActionsLocal actions)
        {
            if (actions == null)
            {
                return null;
            }

            try
            {
                return actions[AutoRunActionName];
            }
            catch
            {
                return null;
            }
        }

        private static string GetActionNameKey(PlayerAction action)
        {
            if (!(action?.UserData is PlayerActionData.ActionUserData userData))
            {
                return string.Empty;
            }

            return userData.actionNameKey ?? string.Empty;
        }

        public static string GetActionNameKeyForDebug(PlayerAction action)
        {
            return GetActionNameKey(action);
        }

        private static PlayerAction CreatePlayerAction(PlayerActionsLocal actions, string actionName)
        {
            MethodInfo createPlayerActionMethod = ResolveCreatePlayerActionMethod();
            if (createPlayerActionMethod == null)
            {
                return null;
            }

            try
            {
                return createPlayerActionMethod.Invoke(actions, new object[] { actionName }) as PlayerAction;
            }
            catch
            {
                return null;
            }
        }

        private static MethodInfo ResolveCreatePlayerActionMethod()
        {
            if (_triedResolveCreatePlayerAction)
            {
                return _createPlayerActionMethod;
            }

            _triedResolveCreatePlayerAction = true;
            _createPlayerActionMethod = AccessTools.Method(typeof(PlayerActionSet), "CreatePlayerAction", new[] { typeof(string) });
            return _createPlayerActionMethod;
        }

        private static void AddRebindableActionIfMissing(PlayerActionsLocal actions, PlayerAction action)
        {
            if (actions == null || action == null || ControllerRebindableActionsField == null)
            {
                return;
            }

            List<PlayerAction> rebindableActions = ControllerRebindableActionsField.GetValue(actions) as List<PlayerAction>;
            if (rebindableActions == null || rebindableActions.Contains(action))
            {
                return;
            }

            rebindableActions.Add(action);
        }

        private static void MoveActionAfterRun(PlayerActionsLocal actions, PlayerAction action)
        {
            if (actions == null || action == null || actions.Run == null)
            {
                return;
            }

            IList<PlayerAction> actionList = GetMutableActionList(actions);
            if (actionList == null)
            {
                return;
            }

            int runIndex = actionList.IndexOf(actions.Run);
            int actionIndex = actionList.IndexOf(action);
            if (runIndex < 0 || actionIndex < 0)
            {
                return;
            }

            int targetIndex = runIndex + 1;
            if (actionIndex == targetIndex)
            {
                return;
            }

            actionList.RemoveAt(actionIndex);
            if (actionIndex < targetIndex)
            {
                targetIndex--;
            }

            if (targetIndex < 0)
            {
                targetIndex = 0;
            }
            if (targetIndex > actionList.Count)
            {
                targetIndex = actionList.Count;
            }

            actionList.Insert(targetIndex, action);
        }

        private static IList<PlayerAction> GetMutableActionList(PlayerActionsLocal actions)
        {
            if (actions == null)
            {
                return null;
            }

            if (PlayerActionSetActionsField?.GetValue(actions) is IList<PlayerAction> reflectedList)
            {
                return reflectedList;
            }

            if (actions.Actions is IList<PlayerAction> list && !list.IsReadOnly)
            {
                return list;
            }

            if (actions.Actions is System.Collections.ObjectModel.ReadOnlyCollection<PlayerAction> readOnly
                && ReadOnlyCollectionListField?.GetValue(readOnly) is IList<PlayerAction> innerList)
            {
                return innerList;
            }

            return null;
        }

        private static int FindBindingEntry(object[] entries, string actionNameKey)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                PlayerAction action = GetBindingEntryAction(entries[i]);
                if (!(action?.UserData is PlayerActionData.ActionUserData userData))
                {
                    continue;
                }

                if (string.Equals(userData.actionNameKey, actionNameKey, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static object[] GetChildControllersUntyped(XUiController controller)
        {
            if (controller == null)
            {
                return null;
            }

            try
            {
                MethodInfo[] methods = controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (!method.IsGenericMethodDefinition || !string.Equals(method.Name, "GetChildControllers", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != 2 || parameters[0].ParameterType != typeof(string))
                    {
                        continue;
                    }

                    MethodInfo closed = method.MakeGenericMethod(typeof(XUiController));
                    object result = closed.Invoke(controller, new object[] { string.Empty, null });
                    if (result is Array array)
                    {
                        object[] values = new object[array.Length];
                        for (int idx = 0; idx < array.Length; idx++)
                        {
                            values[idx] = array.GetValue(idx);
                        }

                        return values;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static PlayerAction GetBindingEntryAction(object entry)
        {
            if (!TryGetBindingEntryActionProperty(entry, out PropertyInfo actionProperty))
            {
                return null;
            }

            return actionProperty?.GetValue(entry, null) as PlayerAction;
        }

        private static void SetBindingEntryAction(object entry, PlayerAction action)
        {
            if (!TryGetBindingEntryActionProperty(entry, out PropertyInfo actionProperty))
            {
                return;
            }

            if (!actionProperty.CanWrite)
            {
                return;
            }

            actionProperty.SetValue(entry, action, null);
        }

        private static bool TryGetBindingEntryActionProperty(object entry, out PropertyInfo actionProperty)
        {
            actionProperty = null;
            if (entry == null)
            {
                return false;
            }

            Type entryType = entry.GetType();
            lock (BindingEntryActionPropertyCache)
            {
                if (!BindingEntryActionPropertyCache.TryGetValue(entryType, out actionProperty))
                {
                    actionProperty = entryType.GetProperty("Action", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    BindingEntryActionPropertyCache[entryType] = actionProperty;
                }
            }

            return actionProperty != null;
        }

        public static void PersistBindingFromReceived(string actionNameKey, BindingSource binding)
        {
            if (binding == null)
            {
                return;
            }

            if (!string.Equals(actionNameKey, AutoRunActionNameKey, StringComparison.Ordinal))
            {
                return;
            }

            if (!(binding is KeyBindingSource keyBinding))
            {
                return;
            }

            try
            {
                KeyCombo combo = keyBinding.Control;
                if (!TryExtractComboState(combo, out int includeSize, out ulong includeData, out int excludeSize, out ulong excludeData))
                {
                    return;
                }

                Dictionary<string, string> map = LoadBindingMap();
                map[AutoRunBindingStoreKey] = $"{includeSize}:{includeData}:{excludeSize}:{excludeData}";
                SaveBindingMap(map);
                Console.WriteLine("AutoRun: Stored fallback binding.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoRun: PersistBindingFromReceived failed key={actionNameKey}: {ex}");
            }
        }

        public static void PersistCurrentFallback(PlayerActionsLocal actions)
        {
            PersistBindingFromAction(GetActivationAction(actions));
        }

        private static void PersistBindingFromAction(PlayerAction action)
        {
            if (action == null)
            {
                return;
            }

            KeyBindingSource keyBinding = null;
            foreach (BindingSource binding in action.Bindings)
            {
                if (binding is KeyBindingSource key)
                {
                    keyBinding = key;
                    break;
                }
            }

            if (keyBinding == null)
            {
                return;
            }

            KeyCombo combo = keyBinding.Control;
            if (!TryExtractComboState(combo, out int includeSize, out ulong includeData, out int excludeSize, out ulong excludeData))
            {
                return;
            }

            Dictionary<string, string> map = LoadBindingMap();
            map[AutoRunBindingStoreKey] = $"{includeSize}:{includeData}:{excludeSize}:{excludeData}";
            SaveBindingMap(map);
        }

        private static void TryRestoreStoredBinding(PlayerAction targetAction)
        {
            if (targetAction == null)
            {
                return;
            }

            try
            {
                if (!TryExtractActionKeyCombo(targetAction, out KeyCombo currentCombo, out _, out _))
                {
                    return;
                }

                Dictionary<string, string> map = LoadBindingMap();
                if (!map.TryGetValue(AutoRunBindingStoreKey, out string value) || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string[] parts = value.Split(':');
                if (parts.Length != 4)
                {
                    return;
                }

                if (!int.TryParse(parts[0], out int includeSize)
                    || !ulong.TryParse(parts[1], out ulong includeData)
                    || !int.TryParse(parts[2], out int excludeSize)
                    || !ulong.TryParse(parts[3], out ulong excludeData))
                {
                    return;
                }

                KeyCombo combo = new KeyCombo(new Key[0]);
                object boxed = combo;
                KeyComboIncludeSizeField?.SetValue(boxed, includeSize);
                KeyComboIncludeDataField?.SetValue(boxed, includeData);
                KeyComboExcludeSizeField?.SetValue(boxed, excludeSize);
                KeyComboExcludeDataField?.SetValue(boxed, excludeData);
                combo = (KeyCombo)boxed;

                if (AreCombosEqual(currentCombo, combo))
                {
                    return;
                }

                List<BindingSource> remove = new List<BindingSource>();
                foreach (BindingSource b in targetAction.Bindings)
                {
                    if (b is KeyBindingSource)
                    {
                        remove.Add(b);
                    }
                }

                for (int i = 0; i < remove.Count; i++)
                {
                    targetAction.RemoveBinding(remove[i]);
                }

                targetAction.AddBinding(new KeyBindingSource(combo));
                Console.WriteLine("AutoRun: Restored fallback binding.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoRun: TryRestoreStoredBinding failed: {ex}");
            }
        }

        private static bool TryExtractComboState(KeyCombo combo, out int includeSize, out ulong includeData, out int excludeSize, out ulong excludeData)
        {
            includeSize = 0;
            includeData = 0;
            excludeSize = 0;
            excludeData = 0;

            try
            {
                object boxed = combo;
                includeSize = (int)(KeyComboIncludeSizeField?.GetValue(boxed) ?? 0);
                includeData = (ulong)(KeyComboIncludeDataField?.GetValue(boxed) ?? 0UL);
                excludeSize = (int)(KeyComboExcludeSizeField?.GetValue(boxed) ?? 0);
                excludeData = (ulong)(KeyComboExcludeDataField?.GetValue(boxed) ?? 0UL);
                return includeSize > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryExtractActionKeyCombo(PlayerAction action, out KeyCombo combo, out int includeSize, out ulong includeData)
        {
            combo = default(KeyCombo);
            includeSize = 0;
            includeData = 0;

            if (action == null)
            {
                return false;
            }

            foreach (BindingSource binding in action.Bindings)
            {
                if (binding is KeyBindingSource keyBinding)
                {
                    combo = keyBinding.Control;
                    if (TryExtractComboState(combo, out includeSize, out includeData, out _, out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AreCombosEqual(KeyCombo a, KeyCombo b)
        {
            if (!TryExtractComboState(a, out int aIncludeSize, out ulong aIncludeData, out int aExcludeSize, out ulong aExcludeData))
            {
                return false;
            }

            if (!TryExtractComboState(b, out int bIncludeSize, out ulong bIncludeData, out int bExcludeSize, out ulong bExcludeData))
            {
                return false;
            }

            return aIncludeSize == bIncludeSize
                && aIncludeData == bIncludeData
                && aExcludeSize == bExcludeSize
                && aExcludeData == bExcludeData;
        }

        private static Dictionary<string, string> LoadBindingMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(AutoRunBindingStorePath))
            {
                return map;
            }

            foreach (string line in File.ReadAllLines(AutoRunBindingStorePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int idx = line.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                map[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
            }

            return map;
        }

        private static void SaveBindingMap(Dictionary<string, string> map)
        {
            string dir = Path.GetDirectoryName(AutoRunBindingStorePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> kvp in map)
            {
                lines.Add(kvp.Key + "=" + kvp.Value);
            }

            File.WriteAllLines(AutoRunBindingStorePath, lines.ToArray());
        }
    }

    internal static class ControllerAutoRunOptionStore
    {
        private const string ControllerAutoRunPrefName = "OptionsControlsControllerAutoRun";
        private const string ControllerAutoRunStoreKey = "controller_autorun";
        private const float UninitializedPressTime = -999f;

        private static readonly string ControllerAutoRunStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DaysToDie", "autorun.controllersettings.txt");
        private static readonly PropertyInfo OptionPrefNameProperty = AccessTools.Property(typeof(XUiC_OptionEntryAbs), "PrefName");
        private static readonly PropertyInfo OptionApplyDefaultsProperty = AccessTools.Property(typeof(XUiC_OptionEntryAbs), "ApplyDefaults");
        private static readonly PropertyInfo OptionApplyImmediatelyProperty = AccessTools.Property(typeof(XUiC_OptionEntryAbs), "ApplyImmediately");
        private static readonly PropertyInfo OptionCurrentValueProperty = AccessTools.Property(typeof(XUiC_OptionEntryGamePrefBool), "CurrentValue");
        private static readonly PropertyInfo OptionSettingValueProperty = AccessTools.Property(typeof(XUiC_OptionEntryGamePrefBool), "SettingValue");
        private static readonly MethodInfo OptionInvokeValueChangedMethod = AccessTools.Method(typeof(XUiC_OptionEntryAbs), "invokeValueChanged");

        private static bool _loaded;
        private static bool _enabled;

        public static bool IsEnabled()
        {
            EnsureLoaded();
            return _enabled;
        }

        public static bool IsControllerInputMode(EntityPlayerLocal player)
        {
            if (player == null || !IsEnabled())
            {
                return false;
            }

            PlayerInputManager.InputStyle currentStyle = PlatformManager.NativePlatform?.Input?.CurrentInputStyle ?? PlayerInputManager.InputStyle.Keyboard;
            return currentStyle != PlayerInputManager.InputStyle.Keyboard;
        }

        public static bool IsCustomOption(XUiC_OptionEntryAbs option)
        {
            string prefName = OptionPrefNameProperty?.GetValue(option, null) as string;
            return string.Equals(prefName, ControllerAutoRunPrefName, StringComparison.OrdinalIgnoreCase);
        }

        public static void InitializeOption(XUiC_OptionEntryGamePrefBool option)
        {
            if (!IsCustomOption(option))
            {
                return;
            }

            bool value = IsEnabled();
            SetCurrentValue(option, value);
            SetSettingValue(option, value);
        }

        public static void SaveOption(XUiC_OptionEntryGamePrefAbs option)
        {
            if (!(option is XUiC_OptionEntryGamePrefBool boolOption) || !IsCustomOption(boolOption))
            {
                return;
            }

            bool value = GetCurrentValue(boolOption);
            SetSettingValue(boolOption, value);
            SetEnabled(value);
        }

        public static void DiscardOption(XUiC_OptionEntryGamePrefBool option)
        {
            if (!IsCustomOption(option))
            {
                return;
            }

            SetCurrentValue(option, GetSettingValue(option));
        }

        public static void ResetOptionToDefault(XUiC_OptionEntryGamePrefBool option)
        {
            if (!IsCustomOption(option))
            {
                return;
            }

            if (!GetApplyDefaults(option))
            {
                return;
            }

            SetCurrentValue(option, value: false);
            if (GetApplyImmediately(option))
            {
                SetSettingValue(option, value: false);
                SetEnabled(value: false);
            }

            try
            {
                OptionInvokeValueChangedMethod?.Invoke(option, null);
            }
            catch
            {
            }
        }

        public static void ApplyImmediateSelection(XUiC_OptionEntryGamePrefBool option)
        {
            if (!IsCustomOption(option))
            {
                return;
            }

            SetEnabled(GetCurrentValue(option));
        }

        public static float DefaultLastPressTime => UninitializedPressTime;

        private static bool GetApplyDefaults(XUiC_OptionEntryAbs option)
        {
            try
            {
                return (bool)(OptionApplyDefaultsProperty?.GetValue(option, null) ?? true);
            }
            catch
            {
                return true;
            }
        }

        private static bool GetApplyImmediately(XUiC_OptionEntryAbs option)
        {
            try
            {
                return (bool)(OptionApplyImmediatelyProperty?.GetValue(option, null) ?? false);
            }
            catch
            {
                return false;
            }
        }

        private static bool GetCurrentValue(XUiC_OptionEntryGamePrefBool option)
        {
            try
            {
                return (bool)(OptionCurrentValueProperty?.GetValue(option, null) ?? false);
            }
            catch
            {
                return false;
            }
        }

        private static bool GetSettingValue(XUiC_OptionEntryGamePrefBool option)
        {
            try
            {
                return (bool)(OptionSettingValueProperty?.GetValue(option, null) ?? false);
            }
            catch
            {
                return false;
            }
        }

        private static void SetCurrentValue(XUiC_OptionEntryGamePrefBool option, bool value)
        {
            try
            {
                OptionCurrentValueProperty?.SetValue(option, value, null);
            }
            catch
            {
            }
        }

        private static void SetSettingValue(XUiC_OptionEntryGamePrefBool option, bool value)
        {
            try
            {
                OptionSettingValueProperty?.SetValue(option, value, null);
            }
            catch
            {
            }
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _enabled = LoadEnabledFromStore();
            _loaded = true;
        }

        private static void SetEnabled(bool value)
        {
            EnsureLoaded();
            _enabled = value;
            SaveEnabledToStore(value);
        }

        private static bool LoadEnabledFromStore()
        {
            try
            {
                if (!File.Exists(ControllerAutoRunStorePath))
                {
                    return false;
                }

                string[] lines = File.ReadAllLines(ControllerAutoRunStorePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, idx).Trim();
                    if (!string.Equals(key, ControllerAutoRunStoreKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string value = line.Substring(idx + 1).Trim();
                    if (string.Equals(value, "1", StringComparison.Ordinal))
                    {
                        return true;
                    }

                    if (string.Equals(value, "0", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    if (bool.TryParse(value, out bool parsed))
                    {
                        return parsed;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static void SaveEnabledToStore(bool enabled)
        {
            try
            {
                string dir = Path.GetDirectoryName(ControllerAutoRunStorePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllLines(ControllerAutoRunStorePath, new[]
                {
                    ControllerAutoRunStoreKey + "=" + (enabled ? "1" : "0")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoRun: Failed to save controller option: {ex}");
            }
        }
    }

    internal static class AutoRunStateStore
    {
        internal sealed class State
        {
            public bool VehicleEnabled;
            public bool VehicleSprintLocked;
            public bool VehicleWasForwardPressed;
            public bool VehicleWasTurboPressed;
            public bool VehicleWasActivationHeld;
            public float VehicleLastTurboPressTime;

            public bool OnFootEnabled;
            public bool OnFootSprintLocked;
            public bool OnFootWasForwardPressed;
            public bool OnFootWasSprintPressed;
            public bool OnFootWasActivationHeld;
            public float OnFootLastSprintPressTime;
        }

        private static readonly Dictionary<int, State> VehicleStates = new Dictionary<int, State>();
        private static readonly Dictionary<int, State> PlayerStates = new Dictionary<int, State>();

        private const string CVarVehicleEnabled = "agf_autorun_vehicle_enabled";
        private const string CVarOnFootEnabled = "agf_autorun_onfoot_enabled";
        private const string CVarAnyEnabled = "agf_autorun_active";
        private const float CVarEnabledThreshold = 0.5f;

        private static State CreateState()
        {
            return new State
            {
                VehicleLastTurboPressTime = ControllerAutoRunOptionStore.DefaultLastPressTime,
                OnFootLastSprintPressTime = ControllerAutoRunOptionStore.DefaultLastPressTime
            };
        }

        private static bool IsCVarEnabled(EntityPlayerLocal player, string cvarName)
        {
            return player != null && player.GetCVar(cvarName) > CVarEnabledThreshold;
        }

        private static void SyncCombinedIndicator(EntityPlayerLocal player)
        {
            if (player == null)
            {
                return;
            }

            bool enabled = IsCVarEnabled(player, CVarOnFootEnabled) || IsCVarEnabled(player, CVarVehicleEnabled);
            player.SetCVar(CVarAnyEnabled, enabled ? 1f : 0f);
        }

        public static bool IsAnyIndicatorActive(EntityPlayerLocal player)
        {
            if (player == null)
            {
                return false;
            }

            // Keep compatibility with either aggregated or split indicator tracking.
            if (IsCVarEnabled(player, CVarAnyEnabled))
            {
                return true;
            }

            return IsCVarEnabled(player, CVarOnFootEnabled) || IsCVarEnabled(player, CVarVehicleEnabled);
        }

        public static State GetOrCreateForVehicle(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return null;
            }

            if (!VehicleStates.TryGetValue(vehicle.entityId, out State state))
            {
                state = CreateState();
                VehicleStates[vehicle.entityId] = state;
            }

            return state;
        }

        public static State GetOrCreateForPlayer(EntityPlayerLocal player)
        {
            if (player == null)
            {
                return null;
            }

            if (!PlayerStates.TryGetValue(player.entityId, out State state))
            {
                state = CreateState();
                PlayerStates[player.entityId] = state;
            }

            return state;
        }

        public static void SetVehicleIndicator(EntityPlayerLocal player, bool enabled)
        {
            if (player == null)
            {
                return;
            }

            player.SetCVar(CVarVehicleEnabled, enabled ? 1f : 0f);
            SyncCombinedIndicator(player);
        }

        public static void SetOnFootIndicator(EntityPlayerLocal player, bool enabled)
        {
            if (player == null)
            {
                return;
            }

            player.SetCVar(CVarOnFootEnabled, enabled ? 1f : 0f);
            SyncCombinedIndicator(player);
        }

        public static void ResetVehicleRuntimeState(State state)
        {
            if (state == null)
            {
                return;
            }

            state.VehicleEnabled = false;
            state.VehicleSprintLocked = false;
            state.VehicleWasForwardPressed = false;
            state.VehicleWasTurboPressed = false;
            state.VehicleWasActivationHeld = false;
            state.VehicleLastTurboPressTime = ControllerAutoRunOptionStore.DefaultLastPressTime;
        }

        public static void ResetOnFootRuntimeState(State state)
        {
            if (state == null)
            {
                return;
            }

            state.OnFootEnabled = false;
            state.OnFootSprintLocked = false;
            state.OnFootWasForwardPressed = false;
            state.OnFootWasSprintPressed = false;
            state.OnFootWasActivationHeld = false;
            state.OnFootLastSprintPressTime = ControllerAutoRunOptionStore.DefaultLastPressTime;
        }

        public static void DisableAllVehicleStates()
        {
            foreach (KeyValuePair<int, State> kvp in VehicleStates)
            {
                State state = kvp.Value;
                if (state == null)
                {
                    continue;
                }

                ResetVehicleRuntimeState(state);
            }
        }
    }

    [HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.MoveByAttachedEntity))]
    internal static class Patch_EntityVehicle_MoveByAttachedEntity
    {
        private const float ForwardPressThreshold = 0.55f;
        private const float ReversePressThreshold = -0.15f;
        private const float DoublePressWindowSeconds = 0.4f;

        private static bool IsActivationKeyHeld(EntityPlayerLocal player)
        {
            return AutoRunBindingManager.IsActivationPressed(player);
        }

        private static bool ReadMenuPressed(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.Menu != null && actions.Menu.WasPressed)
            {
                return true;
            }

            if (actions?.VehicleActions?.Menu != null && actions.VehicleActions.Menu.WasPressed)
            {
                return true;
            }

            return Input.GetKeyDown(KeyCode.Escape);
        }

        private static void ReadDriverInputs(EntityVehicle vehicle, EntityPlayerLocal player, out bool forwardPressed, out bool reversePressed, out bool sprintPressed)
        {
            forwardPressed = false;
            reversePressed = false;
            sprintPressed = false;

            PlayerActionsVehicle vehicleActions = player?.playerInput?.VehicleActions;
            if (vehicleActions != null)
            {
                if (vehicleActions.MoveForward != null)
                {
                    forwardPressed = vehicleActions.MoveForward.IsPressed;
                }

                if (vehicleActions.MoveBack != null)
                {
                    reversePressed = vehicleActions.MoveBack.IsPressed;
                }

                if (vehicleActions.Turbo != null)
                {
                    sprintPressed = vehicleActions.Turbo.IsPressed;
                }
            }

            if (!forwardPressed && vehicle?.movementInput != null)
            {
                forwardPressed = vehicle.movementInput.moveForward >= ForwardPressThreshold;
            }

            if (!reversePressed && vehicle?.movementInput != null)
            {
                reversePressed = vehicle.movementInput.moveForward <= ReversePressThreshold;
            }

            if (!sprintPressed && vehicle?.movementInput != null)
            {
                sprintPressed = vehicle.movementInput.running;
            }
        }

        public static void Postfix(EntityVehicle __instance, EntityPlayerLocal _player)
        {
            if (__instance == null || _player == null || __instance.movementInput == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForVehicle(__instance);
            if (state == null)
            {
                return;
            }

            // Passengers should not affect driver auto-run state.
            if (_player != __instance.AttachedMainEntity)
            {
                return;
            }

            if (__instance.AttachedMainEntity == null)
            {
                AutoRunStateStore.ResetVehicleRuntimeState(state);
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            ReadDriverInputs(__instance, _player, out bool forwardPressed, out bool reversePressed, out bool turboPressed);
            bool menuPressed = ReadMenuPressed(_player);
            bool activationHeld = IsActivationKeyHeld(_player);
            bool controllerMode = ControllerAutoRunOptionStore.IsControllerInputMode(_player);
            bool activationEdge = activationHeld && !state.VehicleWasActivationHeld;
            bool forwardEdge = forwardPressed && !state.VehicleWasForwardPressed;
            bool turboEdge = turboPressed && !state.VehicleWasTurboPressed;

            if (menuPressed)
            {
                state.VehicleEnabled = false;
                state.VehicleSprintLocked = false;
                state.VehicleWasForwardPressed = forwardPressed;
                state.VehicleWasTurboPressed = turboPressed;
                state.VehicleWasActivationHeld = activationHeld;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            if (controllerMode)
            {
                if (turboEdge)
                {
                    float now = Time.unscaledTime;
                    if (!state.VehicleEnabled && now - state.VehicleLastTurboPressTime <= DoublePressWindowSeconds)
                    {
                        state.VehicleEnabled = true;
                        state.VehicleSprintLocked = true;
                        AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
                    }

                    state.VehicleLastTurboPressTime = now;
                }

                state.VehicleWasForwardPressed = forwardPressed;
                state.VehicleWasTurboPressed = turboPressed;
                state.VehicleWasActivationHeld = activationHeld;

                if (!state.VehicleEnabled)
                {
                    state.VehicleSprintLocked = false;
                    AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                    return;
                }

                if (reversePressed)
                {
                    state.VehicleEnabled = false;
                    state.VehicleSprintLocked = false;
                    AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                    return;
                }

                __instance.movementInput.moveForward = 1f;
                __instance.movementInput.running = true;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
                return;
            }

            if (activationEdge && !state.VehicleEnabled)
            {
                // Pressing activation key directly enables auto-run and sprint lock.
                state.VehicleEnabled = true;
                state.VehicleSprintLocked = true;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
            }

            if (forwardEdge)
            {
                if (activationHeld)
                {
                    if (!state.VehicleEnabled)
                    {
                        // Activation key + forward activates auto-run with sprint implied.
                        state.VehicleEnabled = true;
                        state.VehicleSprintLocked = true;
                        AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
                    }
                    // If already active, ignore repeated forward presses while activation key is held.
                }
                else if (state.VehicleEnabled)
                {
                    // Forward without activation key returns control to the player.
                    state.VehicleEnabled = false;
                    state.VehicleSprintLocked = false;
                    AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                }
            }

            bool turboReleased = !turboPressed && state.VehicleWasTurboPressed;
            if (state.VehicleEnabled && turboReleased)
            {
                // Releasing sprint toggles sprint lock while auto-run is active.
                state.VehicleSprintLocked = !state.VehicleSprintLocked;
            }

            state.VehicleWasForwardPressed = forwardPressed;
            state.VehicleWasTurboPressed = turboPressed;
            state.VehicleWasActivationHeld = activationHeld;

            if (!state.VehicleEnabled)
            {
                state.VehicleSprintLocked = false;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            if (reversePressed)
            {
                state.VehicleEnabled = false;
                state.VehicleSprintLocked = false;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            // Emulate held forward input while auto-run is active.
            __instance.movementInput.moveForward = 1f;

            // Sprint while key is held, or lock sprint with modifier+sprint.
            if (state.VehicleSprintLocked || turboPressed)
            {
                __instance.movementInput.running = true;
            }

            AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
        }
    }

    [HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.MoveByInput))]
    internal static class Patch_EntityPlayerLocal_MoveByInput
    {
        private const float DoublePressWindowSeconds = 0.4f;

        private static bool IsActivationKeyHeld(EntityPlayerLocal player)
        {
            return AutoRunBindingManager.IsActivationPressed(player);
        }

        private static bool ReadMenuPressed(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.Menu != null && actions.Menu.WasPressed)
            {
                return true;
            }

            return Input.GetKeyDown(KeyCode.Escape);
        }

        private static bool IsSelectionSetActionActive(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.SelectionSet == null || !actions.SelectionSet.IsPressed)
            {
                return false;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return false;
            }

            if (!gameManager.IsEditMode() && !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
            {
                return false;
            }

            if (gameManager.World?.ChunkCache == null || InputUtils.ControlKeyPressed)
            {
                return false;
            }

            return LocalPlayerUI.primaryUI?.windowManager == null || !LocalPlayerUI.primaryUI.windowManager.IsInputActive();
        }

        private static bool ReadForwardPressed(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.MoveForward != null)
            {
                return actions.MoveForward.IsPressed;
            }

            return player?.movementInput != null && player.movementInput.moveForward >= 0.5f;
        }

        private static bool ReadBackPressed(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.MoveBack != null)
            {
                return actions.MoveBack.IsPressed;
            }

            return player?.movementInput != null && player.movementInput.moveForward <= -0.5f;
        }

        private static bool ReadSprintPressed(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            if (actions?.Run != null)
            {
                return actions.Run.IsPressed;
            }

            return player?.movementInput != null && player.movementInput.running;
        }

        public static void Prefix(EntityPlayerLocal __instance)
        {
            if (__instance == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForPlayer(__instance);
            if (state == null)
            {
                return;
            }

            // On-foot auto-run is suspended while mounted; vehicle patch handles that mode.
            if (__instance.AttachedToEntity != null)
            {
                AutoRunStateStore.ResetOnFootRuntimeState(state);
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                AutoRunStateStore.SetVehicleIndicator(__instance, enabled: false);
                return;
            }

            // Ensure vehicle indicator is off when the local player is no longer driving.
            AutoRunStateStore.DisableAllVehicleStates();
            AutoRunStateStore.SetVehicleIndicator(__instance, enabled: false);

            if (IsSelectionSetActionActive(__instance))
            {
                state.OnFootEnabled = false;
                state.OnFootSprintLocked = false;
                state.OnFootWasForwardPressed = ReadForwardPressed(__instance);
                state.OnFootWasSprintPressed = ReadSprintPressed(__instance);
                state.OnFootWasActivationHeld = IsActivationKeyHeld(__instance);
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                return;
            }

            if (__instance.movementInput == null)
            {
                return;
            }

            bool forwardPressed = ReadForwardPressed(__instance);
            bool backPressed = ReadBackPressed(__instance);
            bool sprintPressed = ReadSprintPressed(__instance);
            bool activationHeld = IsActivationKeyHeld(__instance);
            bool menuPressed = ReadMenuPressed(__instance);
            bool controllerMode = ControllerAutoRunOptionStore.IsControllerInputMode(__instance);
            bool sprintEdge = sprintPressed && !state.OnFootWasSprintPressed;

            if (menuPressed)
            {
                state.OnFootEnabled = false;
                state.OnFootSprintLocked = false;
                state.OnFootWasForwardPressed = forwardPressed;
                state.OnFootWasSprintPressed = sprintPressed;
                state.OnFootWasActivationHeld = activationHeld;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                return;
            }

            if (controllerMode)
            {
                if (sprintEdge)
                {
                    float now = Time.unscaledTime;
                    if (!state.OnFootEnabled && now - state.OnFootLastSprintPressTime <= DoublePressWindowSeconds)
                    {
                        state.OnFootEnabled = true;
                        state.OnFootSprintLocked = true;
                        AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
                    }

                    state.OnFootLastSprintPressTime = now;
                }

                state.OnFootWasForwardPressed = forwardPressed;
                state.OnFootWasSprintPressed = sprintPressed;
                state.OnFootWasActivationHeld = activationHeld;

                if (!state.OnFootEnabled)
                {
                    state.OnFootSprintLocked = false;
                    AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                    return;
                }

                if (backPressed)
                {
                    state.OnFootEnabled = false;
                    state.OnFootSprintLocked = false;
                    AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                    return;
                }

                __instance.movementInput.moveForward = 1f;
                __instance.movementInput.running = true;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
                return;
            }

            bool activationEdge = activationHeld && !state.OnFootWasActivationHeld;
            bool forwardEdge = forwardPressed && !state.OnFootWasForwardPressed;

            if (activationEdge && !state.OnFootEnabled)
            {
                // Pressing activation key directly enables auto-run and sprint lock.
                state.OnFootEnabled = true;
                state.OnFootSprintLocked = true;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
            }

            if (forwardEdge)
            {
                if (activationHeld)
                {
                    if (!state.OnFootEnabled)
                    {
                        // Activation key + forward activates auto-run with sprint implied.
                        state.OnFootEnabled = true;
                        state.OnFootSprintLocked = true;
                        AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
                    }
                    // If already active, ignore repeated forward presses while activation key is held.
                }
                else if (state.OnFootEnabled)
                {
                    // Forward without activation key returns control to the player.
                    state.OnFootEnabled = false;
                    state.OnFootSprintLocked = false;
                    AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                }
            }

            bool sprintReleased = !sprintPressed && state.OnFootWasSprintPressed;
            if (state.OnFootEnabled && sprintReleased)
            {
                // Releasing sprint toggles sprint lock while auto-run is active.
                state.OnFootSprintLocked = !state.OnFootSprintLocked;
            }

            state.OnFootWasForwardPressed = forwardPressed;
            state.OnFootWasSprintPressed = sprintPressed;
            state.OnFootWasActivationHeld = activationHeld;

            if (!state.OnFootEnabled)
            {
                state.OnFootSprintLocked = false;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                return;
            }

            if (backPressed)
            {
                state.OnFootEnabled = false;
                state.OnFootSprintLocked = false;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                return;
            }

            __instance.movementInput.moveForward = 1f;
            // Sprint while key is held, or lock sprint with modifier+sprint.
            if (state.OnFootSprintLocked || sprintPressed)
            {
                __instance.movementInput.running = true;
            }

            AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefAbs), "parseGamePref")]
    internal static class Patch_XUiC_OptionEntryGamePrefAbs_parseGamePref
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_OptionEntryGamePrefAbs __instance)
        {
            return !ControllerAutoRunOptionStore.IsCustomOption(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_HUDStatBar), "GetBindingValueInternal")]
    internal static class Patch_XUiC_HUDStatBar_GetBindingValueInternal
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool Prefix(XUiC_HUDStatBar __instance, ref string _value, string _bindingName, ref bool __result)
        {
            if (!string.Equals(_bindingName, "autorunactive", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            EntityPlayerLocal player = __instance?.localPlayer ?? __instance?.xui?.playerUI?.entityPlayer;
            if (player == null)
            {
                player = LocalPlayerUI.primaryUI?.entityPlayer as EntityPlayerLocal;
            }

            if (player == null)
            {
                player = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            _value = AutoRunStateStore.IsAnyIndicatorActive(player) ? "true" : "false";
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiController), "GetBindingValueInternal")]
    internal static class Patch_XUiController_GetBindingValueInternal
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool Prefix(XUiController __instance, ref string _value, string _bindingName, ref bool __result)
        {
            if (!string.Equals(_bindingName, "autorunactive", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            EntityPlayerLocal player = null;

            if (__instance is XUiC_HUDStatBar statBar)
            {
                player = statBar.localPlayer ?? statBar.xui?.playerUI?.entityPlayer;
            }
            else
            {
                player = __instance?.xui?.playerUI?.entityPlayer;
            }

            if (player == null)
            {
                player = LocalPlayerUI.primaryUI?.entityPlayer as EntityPlayerLocal;
            }

            if (player == null)
            {
                player = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            _value = AutoRunStateStore.IsAnyIndicatorActive(player) ? "true" : "false";
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefBool), "initCurrentValue")]
    internal static class Patch_XUiC_OptionEntryGamePrefBool_initCurrentValue
    {
        [HarmonyPostfix]
        public static void Postfix(XUiC_OptionEntryGamePrefBool __instance)
        {
            ControllerAutoRunOptionStore.InitializeOption(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefAbs), "ApplySelection")]
    internal static class Patch_XUiC_OptionEntryGamePrefAbs_ApplySelection
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_OptionEntryGamePrefAbs __instance)
        {
            if (!ControllerAutoRunOptionStore.IsCustomOption(__instance))
            {
                return true;
            }

            ControllerAutoRunOptionStore.SaveOption(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefBool), "DiscardCurrentChange")]
    internal static class Patch_XUiC_OptionEntryGamePrefBool_DiscardCurrentChange
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_OptionEntryGamePrefBool __instance)
        {
            if (!ControllerAutoRunOptionStore.IsCustomOption(__instance))
            {
                return true;
            }

            ControllerAutoRunOptionStore.DiscardOption(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefBool), "ResetToDefault")]
    internal static class Patch_XUiC_OptionEntryGamePrefBool_ResetToDefault
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_OptionEntryGamePrefBool __instance)
        {
            if (!ControllerAutoRunOptionStore.IsCustomOption(__instance))
            {
                return true;
            }

            ControllerAutoRunOptionStore.ResetOptionToDefault(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionEntryGamePrefBool), "immediatelyApplyCurrentSelection")]
    internal static class Patch_XUiC_OptionEntryGamePrefBool_immediatelyApplyCurrentSelection
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_OptionEntryGamePrefBool __instance)
        {
            if (!ControllerAutoRunOptionStore.IsCustomOption(__instance))
            {
                return true;
            }

            ControllerAutoRunOptionStore.ApplyImmediateSelection(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerActionsLocal), "CreateActions")]
    internal static class Patch_PlayerActionsLocal_CreateActions
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(PlayerActionsLocal __instance)
        {
            AutoRunBindingManager.EnsureInstalled(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerActionsLocal), MethodType.Constructor)]
    internal static class Patch_PlayerActionsLocal_Ctor
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(PlayerActionsLocal __instance)
        {
            AutoRunBindingManager.EnsureInstalled(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionsControls), "createControlsEntries")]
    internal static class Patch_XUiC_OptionsControls_createControlsEntries
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(XUiC_OptionsControls __instance)
        {
            AutoRunBindingManager.EnsureInstalled(__instance?.xui?.playerUI?.playerInput);
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(XUiC_OptionsControls __instance)
        {
            AutoRunBindingManager.EnsureKeyboardEntryOrder(__instance);
        }
    }

    [HarmonyPatch(typeof(GameOptionsControls), "Save")]
    internal static class Patch_GameOptionsControls_Save
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            PlayerActionsLocal local = AutoRunBindingManager.TryGetLocalActionsFromPlatformInput();
            AutoRunBindingManager.EnsureInstalled(local);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PlayerActionsLocal local = AutoRunBindingManager.TryGetLocalActionsFromPlatformInput();
            AutoRunBindingManager.EnsureInstalled(local);
            AutoRunBindingManager.PersistCurrentFallback(local);
        }
    }

    [HarmonyPatch(typeof(GameOptionsControls), "Load")]
    internal static class Patch_GameOptionsControls_Load
    {
        internal static bool IsRunningLoadControls;

        [HarmonyPrefix]
        public static void Prefix()
        {
            IsRunningLoadControls = true;
            PlayerActionsLocal local = AutoRunBindingManager.TryGetLocalActionsFromPlatformInput();
            AutoRunBindingManager.EnsureInstalled(local);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                PlayerActionsLocal local = AutoRunBindingManager.TryGetLocalActionsFromPlatformInput();
                AutoRunBindingManager.EnsureInstalled(local);
            }
            finally
            {
                IsRunningLoadControls = false;
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_OptionsControlsNewBinding), "onBindingReceived")]
    internal static class Patch_XUiC_OptionsControlsNewBinding_onBindingReceived
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerAction _action, BindingSource _binding, bool __result)
        {
            if (!__result)
            {
                return;
            }

            string key = AutoRunBindingManager.GetActionNameKeyForDebug(_action);
            AutoRunBindingManager.PersistBindingFromReceived(key, _binding);
        }
    }
}
