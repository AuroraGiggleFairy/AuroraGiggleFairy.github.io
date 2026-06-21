using HarmonyLib;
using InControl;
using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Toolbelt12Slots
{
	public static class ToolbeltConstants
	{
		public const int PlayerSlots = 12;
		public const int Slot11Index = 10;
		public const int Slot12Index = 11;
		public const string Slot11ActionName = "InventorySlot11";
		public const string Slot12ActionName = "InventorySlot12";
		public const string Slot11ActionFallbackName = "Inventory11";
		public const string Slot12ActionFallbackName = "Inventory12";
	}

	internal static class ToolbeltExtendedActions
	{
		private static readonly PlayerActionData.ActionGroup ToolbeltCycleGroup = new PlayerActionData.ActionGroup("inpGrpToolbeltName", null, 11, PlayerActionData.TabToolbelt);

		private sealed class InstalledActions
		{
			public PlayerAction Slot11;
			public PlayerAction Slot12;
		}

		private static readonly ConditionalWeakTable<PlayerActionsLocal, InstalledActions> InstalledBySet = new ConditionalWeakTable<PlayerActionsLocal, InstalledActions>();
		private static MethodInfo _createPlayerActionMethod;
		private static bool _triedResolveCreatePlayerAction;
		private static readonly string SlotBindingStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DaysToDie", "toolbelt12slots.bindingstore.txt");
		private static readonly FieldInfo KeyComboIncludeSizeField = AccessTools.Field(typeof(KeyCombo), "includeSize");
		private static readonly FieldInfo KeyComboIncludeDataField = AccessTools.Field(typeof(KeyCombo), "includeData");
		private static readonly FieldInfo KeyComboExcludeSizeField = AccessTools.Field(typeof(KeyCombo), "excludeSize");
		private static readonly FieldInfo KeyComboExcludeDataField = AccessTools.Field(typeof(KeyCombo), "excludeData");

		public static void EnsureInstalled(PlayerActionsLocal actions)
		{
			if (actions == null)
			{
				return;
			}

			if (actions.InventorySlot1 == null)
			{
				return;
			}

			InstalledActions installed = InstalledBySet.GetOrCreateValue(actions);
			EnsureCycleActionsSortAfterSlots(actions);

			PlayerActionData.ActionUserData slot1UserData = actions.InventorySlot1.UserData as PlayerActionData.ActionUserData;
			if (slot1UserData == null)
			{
				return;
			}

			bool createdSlot11 = false;
			if (installed.Slot11 == null)
			{
				installed.Slot11 = CreateToolbeltSlotAction(actions, ToolbeltConstants.Slot11ActionName, ToolbeltConstants.Slot11ActionFallbackName);
				createdSlot11 = installed.Slot11 != null;
			}

			if (installed.Slot11 != null)
			{
				PlayerActionData.ActionUserData slot11UserData = CreateToolbeltUserData(slot1UserData, "inpActInventorySlot11Name", "inpActInventorySlot11Desc");
				if (slot11UserData != null)
				{
					installed.Slot11.UserData = slot11UserData;
				}
				if (createdSlot11)
				{
					installed.Slot11.AddDefaultBinding(Key.Minus);
				}
				AddInventoryActionIfMissing(actions.InventoryActions, installed.Slot11, actions.InventorySlot10);
				AddRebindableActionIfMissing(actions, installed.Slot11);
				if (createdSlot11 || Patch_GameOptionsControls_Load.IsRunningLoadControls)
				{
					TryRestoreStoredSlotBinding(installed.Slot11, slot11: true);
				}
				LogInstalledAction("Slot11", installed.Slot11);
			}

			bool createdSlot12 = false;
			if (installed.Slot12 == null)
			{
				installed.Slot12 = CreateToolbeltSlotAction(actions, ToolbeltConstants.Slot12ActionName, ToolbeltConstants.Slot12ActionFallbackName);
				createdSlot12 = installed.Slot12 != null;
			}

			if (installed.Slot12 != null)
			{
				PlayerActionData.ActionUserData slot12UserData = CreateToolbeltUserData(slot1UserData, "inpActInventorySlot12Name", "inpActInventorySlot12Desc");
				if (slot12UserData != null)
				{
					installed.Slot12.UserData = slot12UserData;
				}
				if (createdSlot12)
				{
					installed.Slot12.AddDefaultBinding(Key.Equals);
				}
				AddInventoryActionIfMissing(actions.InventoryActions, installed.Slot12, installed.Slot11 ?? actions.InventorySlot10);
				AddRebindableActionIfMissing(actions, installed.Slot12);
				if (createdSlot12 || Patch_GameOptionsControls_Load.IsRunningLoadControls)
				{
					TryRestoreStoredSlotBinding(installed.Slot12, slot11: false);
				}
				LogInstalledAction("Slot12", installed.Slot12);
			}

			EnforceInventorySlotOrder(actions, installed);
		}

		private static PlayerActionData.ActionUserData CreateToolbeltUserData(PlayerActionData.ActionUserData template, string nameKey, string descKey)
		{
			if (template == null)
			{
				return null;
			}

			try
			{
				return new PlayerActionData.ActionUserData(
					nameKey,
					descKey,
					PlayerActionData.GroupToolbelt,
					template.appliesToInputType,
					template.allowRebind,
					template.allowMultipleBindings,
					template.doNotDisplay,
					template.defaultOnStartup);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Toolbelt12Slots: CreateToolbeltUserData failed: {ex}");
				return null;
			}
		}

		private static void EnsureCycleActionsSortAfterSlots(PlayerActionsLocal actions)
		{
			if (actions == null)
			{
				return;
			}

			MoveActionToCycleGroup(actions.InventorySlotLeft);
			MoveActionToCycleGroup(actions.InventorySlotRight);
		}

		private static void MoveActionToCycleGroup(PlayerAction action)
		{
			PlayerActionData.ActionUserData data = action?.UserData as PlayerActionData.ActionUserData;
			if (data == null)
			{
				return;
			}

			if (data.actionGroup == ToolbeltCycleGroup)
			{
				return;
			}

			action.UserData = new PlayerActionData.ActionUserData(
				data.actionNameKey,
				data.actionDescKey,
				ToolbeltCycleGroup,
				data.appliesToInputType,
				data.allowRebind,
				data.allowMultipleBindings,
				data.doNotDisplay,
				data.defaultOnStartup);
		}

		private static PlayerAction CreatePlayerAction(PlayerActionsLocal actions, string name)
		{
			MethodInfo createPlayerActionMethod = ResolveCreatePlayerActionMethod();
			if (createPlayerActionMethod == null)
			{
				return null;
			}

			try
			{
				return createPlayerActionMethod.Invoke(actions, new object[] { name }) as PlayerAction;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log($"Toolbelt12Slots: CreatePlayerAction invoke failed for {name}: {ex}");
				return null;
			}
		}

		private static PlayerAction CreateToolbeltSlotAction(PlayerActionsLocal actions, string primaryName, string fallbackName)
		{
			PlayerAction action = CreatePlayerAction(actions, primaryName);
			if (action != null)
			{
				return action;
			}

			if (!string.IsNullOrEmpty(fallbackName))
			{
				action = CreatePlayerAction(actions, fallbackName);
				if (action != null)
				{
					return action;
				}
			}

			UnityEngine.Debug.Log($"Toolbelt12Slots: Failed to create action names '{primaryName}' and '{fallbackName}'.");

			return action;
		}

		private static MethodInfo ResolveCreatePlayerActionMethod()
		{
			if (_triedResolveCreatePlayerAction)
			{
				return _createPlayerActionMethod;
			}

			_triedResolveCreatePlayerAction = true;
			try
			{
				_createPlayerActionMethod = AccessTools.Method(typeof(PlayerActionSet), "CreatePlayerAction", new[] { typeof(string) });
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log($"Toolbelt12Slots: Resolve CreatePlayerAction failed: {ex}");
				_createPlayerActionMethod = null;
			}

			if (_createPlayerActionMethod == null)
			{
				UnityEngine.Debug.Log("Toolbelt12Slots: CreatePlayerAction method not found.");
			}

			return _createPlayerActionMethod;
		}

		private static void LogInstalledAction(string tag, PlayerAction action)
		{
			PlayerActionData.ActionUserData userData = action?.UserData as PlayerActionData.ActionUserData;
			if (userData == null)
			{
				Console.WriteLine($"Toolbelt12Slots: {tag} installed with null userData.");
				return;
			}

			string actionNameKey = userData.actionNameKey ?? "<null>";
			string actionDescKey = userData.actionDescKey ?? "<null>";
			Console.WriteLine($"Toolbelt12Slots: {tag} installed nameKey={actionNameKey} descKey={actionDescKey}.");
		}

		private static void AddInventoryActionIfMissing(List<PlayerAction> inventoryActions, PlayerAction action, PlayerAction insertAfter)
		{
			if (inventoryActions == null || action == null)
			{
				return;
			}

			if (inventoryActions.Contains(action))
			{
				return;
			}

			int insertIndex = -1;
			if (insertAfter != null)
			{
				insertIndex = inventoryActions.IndexOf(insertAfter);
			}

			if (insertIndex >= 0 && insertIndex < inventoryActions.Count)
			{
				inventoryActions.Insert(insertIndex + 1, action);
				return;
			}

			inventoryActions.Add(action);
		}

		private static void AddRebindableActionIfMissing(PlayerActionsLocal actions, PlayerAction action)
		{
			if (actions == null || action == null)
			{
				return;
			}

			FieldInfo rebindableField = AccessTools.Field(typeof(PlayerActionsLocal), "ControllerRebindableActions");
			if (rebindableField == null)
			{
				return;
			}

			List<PlayerAction> rebindableActions = rebindableField.GetValue(actions) as List<PlayerAction>;
			if (rebindableActions == null)
			{
				return;
			}

			if (rebindableActions.Contains(action))
			{
				return;
			}

			rebindableActions.Add(action);
		}

		private static void EnforceInventorySlotOrder(PlayerActionsLocal actions, InstalledActions installed)
		{
			if (actions?.InventoryActions == null || installed == null)
			{
				return;
			}

			if (installed.Slot11 == null || installed.Slot12 == null)
			{
				return;
			}

			List<PlayerAction> inventoryActions = actions.InventoryActions;
			inventoryActions.Remove(installed.Slot11);
			inventoryActions.Remove(installed.Slot12);

			int slot10Index = inventoryActions.IndexOf(actions.InventorySlot10);
			if (slot10Index < 0)
			{
				inventoryActions.Add(installed.Slot11);
				inventoryActions.Add(installed.Slot12);
				return;
			}

			inventoryActions.Insert(slot10Index + 1, installed.Slot11);
			inventoryActions.Insert(slot10Index + 2, installed.Slot12);
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
			catch (Exception ex)
			{
				UnityEngine.Debug.Log($"Toolbelt12Slots: TryGetLocalActionsFromPlatformInput failed: {ex}");
			}

			return null;
		}

		private static PlayerAction ResolveRuntimeSlotAction(PlayerActionsLocal runtimeActions, bool slot11)
		{
			string primaryName = slot11 ? ToolbeltConstants.Slot11ActionName : ToolbeltConstants.Slot12ActionName;
			string fallbackName = slot11 ? ToolbeltConstants.Slot11ActionFallbackName : ToolbeltConstants.Slot12ActionFallbackName;

			PlayerAction action = runtimeActions[primaryName];
			if (action != null)
			{
				return action;
			}

			return runtimeActions[fallbackName];
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

		public static void PersistSlotBindingFromReceived(string actionNameKey, BindingSource binding)
		{
			if (binding == null)
			{
				return;
			}

			bool isSlot11 = string.Equals(actionNameKey, "inpActInventorySlot11Name", StringComparison.Ordinal);
			bool isSlot12 = string.Equals(actionNameKey, "inpActInventorySlot12Name", StringComparison.Ordinal);
			if (!isSlot11 && !isSlot12)
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

				Dictionary<string, string> map = LoadSlotBindingMap();
				string slotKey = isSlot11 ? "slot11" : "slot12";
				map[slotKey] = $"{includeSize}:{includeData}:{excludeSize}:{excludeData}";
				SaveSlotBindingMap(map);
				Console.WriteLine($"Toolbelt12Slots: Stored fallback binding for {slotKey}.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Toolbelt12Slots: PersistSlotBindingFromReceived failed key={actionNameKey}: {ex}");
			}
		}

		public static void PersistCurrentSlotFallbacks(PlayerActionsLocal actions)
		{
			if (actions == null)
			{
				return;
			}

			PlayerAction slot11 = ResolveRuntimeSlotAction(actions, slot11: true);
			PlayerAction slot12 = ResolveRuntimeSlotAction(actions, slot11: false);
			PersistSlotBindingFromAction(slot11, slot11: true);
			PersistSlotBindingFromAction(slot12, slot11: false);
		}

		private static void PersistSlotBindingFromAction(PlayerAction action, bool slot11)
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

			Dictionary<string, string> map = LoadSlotBindingMap();
			string slotKey = slot11 ? "slot11" : "slot12";
			map[slotKey] = $"{includeSize}:{includeData}:{excludeSize}:{excludeData}";
			SaveSlotBindingMap(map);
		}

		private static void TryRestoreStoredSlotBinding(PlayerAction targetAction, bool slot11)
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

				Dictionary<string, string> map = LoadSlotBindingMap();
				if (!map.TryGetValue(slot11 ? "slot11" : "slot12", out string value) || string.IsNullOrWhiteSpace(value))
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
				Console.WriteLine($"Toolbelt12Slots: Restored fallback binding for {(slot11 ? "slot11" : "slot12")}." );
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Toolbelt12Slots: TryRestoreStoredSlotBinding failed slot11={slot11}: {ex}");
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

		private static Dictionary<string, string> LoadSlotBindingMap()
		{
			Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (!File.Exists(SlotBindingStorePath))
			{
				return map;
			}

			foreach (string line in File.ReadAllLines(SlotBindingStorePath))
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

		private static void SaveSlotBindingMap(Dictionary<string, string> map)
		{
			string dir = Path.GetDirectoryName(SlotBindingStorePath);
			if (!string.IsNullOrEmpty(dir))
			{
				Directory.CreateDirectory(dir);
			}

			File.WriteAllLines(SlotBindingStorePath, map.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());
		}

	}

	[HarmonyPatch(typeof(Inventory), "get_PUBLIC_SLOTS_PLAYMODE")]
	public class Patch_Inventory_PUBLIC_SLOTS_PLAYMODE
	{
		public static bool Prefix(ref int __result)
		{
			__result = ToolbeltConstants.PlayerSlots;
			return false;
		}
	}

	[HarmonyPatch(typeof(Inventory), "get_SHIFT_KEY_SLOT_OFFSET")]
	public class Patch_Inventory_SHIFT_KEY_SLOT_OFFSET
	{
		public static bool Prefix(ref int __result)
		{
			__result = ToolbeltConstants.PlayerSlots;
			return false;
		}
	}

	// No patch on InventorySlotWasPressed: let vanilla evaluate slot keypresses.

	[HarmonyPatch(typeof(PlayerActionsLocal), "CreateActions")]
	public class Patch_PlayerActionsLocal_CreateActions
	{
		[HarmonyPriority(Priority.Last)]
		public static void Postfix(PlayerActionsLocal __instance)
		{
			ToolbeltExtendedActions.EnsureInstalled(__instance);
		}
	}

	[HarmonyPatch(typeof(PlayerActionsLocal), MethodType.Constructor)]
	public class Patch_PlayerActionsLocal_Ctor
	{
		[HarmonyPriority(Priority.Last)]
		public static void Postfix(PlayerActionsLocal __instance)
		{
			ToolbeltExtendedActions.EnsureInstalled(__instance);
		}
	}

	[HarmonyPatch(typeof(XUiC_OptionsControls), "createControlsEntries")]
	public class Patch_XUiC_OptionsControls_createControlsEntries
	{
		[HarmonyPriority(Priority.First)]
		public static void Prefix(XUiC_OptionsControls __instance)
		{
			PlayerActionsLocal actions = __instance?.xui?.playerUI?.playerInput;
			ToolbeltExtendedActions.EnsureInstalled(actions);
		}
	}

	[HarmonyPatch(typeof(XUiC_OptionsController), "createControlsEntries")]
	public class Patch_XUiC_OptionsController_createControlsEntries
	{
		[HarmonyPriority(Priority.First)]
		public static void Prefix(PlayerActionsBase ___actionSetInGame)
		{
			ToolbeltExtendedActions.EnsureInstalled(___actionSetInGame as PlayerActionsLocal);
		}
	}


	[HarmonyPatch(typeof(GameOptionsControls), "Save")]
	public class Patch_GameOptionsControls_Save
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			PlayerActionsLocal local = ToolbeltExtendedActions.TryGetLocalActionsFromPlatformInput();
			ToolbeltExtendedActions.EnsureInstalled(local);
		}

		[HarmonyPostfix]
		public static void Postfix()
		{
			PlayerActionsLocal local = ToolbeltExtendedActions.TryGetLocalActionsFromPlatformInput();
			ToolbeltExtendedActions.EnsureInstalled(local);
			ToolbeltExtendedActions.PersistCurrentSlotFallbacks(local);
		}
	}

	[HarmonyPatch(typeof(GameOptionsControls), "Load")]
	public class Patch_GameOptionsControls_Load
	{
		internal static bool IsRunningLoadControls;

		[HarmonyPrefix]
		public static void Prefix()
		{
			IsRunningLoadControls = true;
			PlayerActionsLocal local = ToolbeltExtendedActions.TryGetLocalActionsFromPlatformInput();
			ToolbeltExtendedActions.EnsureInstalled(local);
		}

		[HarmonyPostfix]
		public static void Postfix()
		{
			try
			{
				PlayerActionsLocal local = ToolbeltExtendedActions.TryGetLocalActionsFromPlatformInput();
				ToolbeltExtendedActions.EnsureInstalled(local);
			}
			finally
			{
				IsRunningLoadControls = false;
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_OptionsControlsNewBinding), "onBindingReceived")]
	public class Patch_XUiC_OptionsControlsNewBinding_onBindingReceived
	{
		[HarmonyPostfix]
		public static void Postfix(XUiC_OptionsControlsNewBinding __instance, PlayerAction _action, BindingSource _binding, bool ___forController, bool __result)
		{
			string key = ToolbeltExtendedActions.GetActionNameKeyForDebug(_action);

			if (!__result)
			{
				return;
			}

			ToolbeltExtendedActions.PersistSlotBindingFromReceived(key, _binding);
		}
	}

}
