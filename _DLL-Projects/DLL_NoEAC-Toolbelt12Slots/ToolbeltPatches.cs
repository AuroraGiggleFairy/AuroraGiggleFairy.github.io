using HarmonyLib;
using UnityEngine;

namespace Toolbelt12Slots
{
	public static class ToolbeltConstants
	{
		public const int PlayerSlots = 12;

		public static readonly KeyCode[] HotbarPhysicalKeys =
		{
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0,
			KeyCode.Minus,
			KeyCode.Equals
		};
	}

	public static class ToolbeltRuntimeState
	{
		public static int LastPhysicalHotkeySlot = -1;
		public static int LastPhysicalHotkeyFrame = -1;
		public static bool WasMountedLastFrame;

		public static void ClearRecentHotkeyState()
		{
			LastPhysicalHotkeySlot = -1;
			LastPhysicalHotkeyFrame = -1;
		}

		public static int NormalizeToPrimaryRow(int slotIndex)
		{
			if (slotIndex < 0)
			{
				return slotIndex;
			}

			if (slotIndex < ToolbeltConstants.PlayerSlots)
			{
				return slotIndex;
			}

			return slotIndex % ToolbeltConstants.PlayerSlots;
		}
	}

	internal static class ToolbeltFrameCache
	{
		private static int _snapshotFrame = -1;
		private static readonly bool[] _currentHotbarKeyHeld = new bool[ToolbeltConstants.PlayerSlots];
		private static readonly bool[] _previousHotbarKeyHeld = new bool[ToolbeltConstants.PlayerSlots];
		private static int _hotbarKeyDownSlot = -1;
		private static bool _shiftPressed;
		private static Inventory _primaryToolbelt;

		public static bool ShiftPressed
		{
			get
			{
				EnsureSnapshot();
				return _shiftPressed;
			}
		}

		public static Inventory GetPrimaryToolbelt()
		{
			EnsureSnapshot();
			return _primaryToolbelt;
		}

		public static bool TryGetHotbarKeyDown(out int slotIndex)
		{
			EnsureSnapshot();
			slotIndex = _hotbarKeyDownSlot;
			return slotIndex >= 0;
		}

		public static bool TryGetHeldHotbarKey(out int slotIndex)
		{
			EnsureSnapshot();
			for (int i = 0; i < _currentHotbarKeyHeld.Length; i++)
			{
				if (_currentHotbarKeyHeld[i])
				{
					slotIndex = i;
					return true;
				}
			}

			slotIndex = -1;
			return false;
		}

		private static void EnsureSnapshot()
		{
			int frame = Time.frameCount;
			if (_snapshotFrame == frame)
			{
				return;
			}

			_snapshotFrame = frame;
			_shiftPressed = InputUtils.ShiftKeyPressed;
			_hotbarKeyDownSlot = -1;

			for (int i = 0; i < ToolbeltConstants.HotbarPhysicalKeys.Length; i++)
			{
				bool held = Input.GetKey(ToolbeltConstants.HotbarPhysicalKeys[i]);
				_currentHotbarKeyHeld[i] = held;
				if (_hotbarKeyDownSlot < 0 && held && !_previousHotbarKeyHeld[i])
				{
					_hotbarKeyDownSlot = i;
				}
			}

			for (int i = 0; i < _previousHotbarKeyHeld.Length; i++)
			{
				_previousHotbarKeyHeld[i] = _currentHotbarKeyHeld[i];
			}

			GameManager gameManager = GameManager.Instance;
			EntityPlayerLocal player = gameManager?.World?.GetPrimaryPlayer();
			_primaryToolbelt = player?.inventory;
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
		public static bool Prefix(Inventory __instance, ref int __result)
		{
			// Disable vanilla global shift offset; we apply offset ourselves only when row 2 is active.
			__result = __instance.PUBLIC_SLOTS;
			return false;
		}
	}

	[HarmonyPatch(typeof(PlayerActionsLocal), "get_InventorySlotWasPressed")]
	public class Patch_PlayerActionsLocal_InventorySlotWasPressed
	{
		[HarmonyPriority(Priority.Last)]
		public static void Postfix(ref int __result)
		{
			int desiredSlot = __result;
			Inventory toolbelt = ToolbeltFrameCache.GetPrimaryToolbelt();
			bool detectedPhysicalHotkey = ToolbeltFrameCache.TryGetHotbarKeyDown(out int physicalHotkeySlot);

			// If a physical hotkey press is detected, trust it over incoming values from other patches.
			if (detectedPhysicalHotkey)
			{
				desiredSlot = physicalHotkeySlot;
				ToolbeltRuntimeState.LastPhysicalHotkeySlot = physicalHotkeySlot;
				ToolbeltRuntimeState.LastPhysicalHotkeyFrame = Time.frameCount;
			}

			if (desiredSlot < 0)
			{
				return;
			}

			desiredSlot = ToolbeltRuntimeState.NormalizeToPrimaryRow(desiredSlot);

			__result = desiredSlot;

			int vanillaShiftOffset = 0;
			bool vanillaOffsetActive = ToolbeltFrameCache.ShiftPressed && TryGetVanillaShiftOffset(toolbelt, out vanillaShiftOffset);
			if (vanillaOffsetActive)
			{
				// PlayerMoveController will add this offset later; pre-subtract so final slot stays intentional.
				__result -= vanillaShiftOffset;
			}
		}

		private static bool TryGetVanillaShiftOffset(Inventory toolbelt, out int vanillaShiftOffset)
		{
			vanillaShiftOffset = 0;
			if (toolbelt == null)
			{
				return false;
			}

			int publicSlots = toolbelt.PUBLIC_SLOTS;
			int shiftOffset = toolbelt.SHIFT_KEY_SLOT_OFFSET;
			if (publicSlots <= shiftOffset)
			{
				return false;
			}

			vanillaShiftOffset = shiftOffset;
			return true;
		}

	}

	[HarmonyPatch(typeof(Inventory), "SetFocusedItemIdx")]
	public class Patch_Inventory_SetFocusedItemIdx
	{
		private static bool _isReapplyingFocusedSlot;

		[HarmonyPriority(Priority.Last)]
		public static void Prefix(Inventory __instance, ref int _idx)
		{
			if (_isReapplyingFocusedSlot)
			{
				return;
			}

			if (!ToolbeltFrameCache.ShiftPressed)
			{
				_idx = ToolbeltRuntimeState.NormalizeToPrimaryRow(_idx);
				ToolbeltRuntimeState.ClearRecentHotkeyState();
				return;
			}

			if (__instance == null)
			{
				return;
			}

			int recentHotkeySlot = ToolbeltRuntimeState.LastPhysicalHotkeySlot;
			if (recentHotkeySlot < 0 || recentHotkeySlot >= ToolbeltConstants.PlayerSlots)
			{
				return;
			}

			if (Time.frameCount - ToolbeltRuntimeState.LastPhysicalHotkeyFrame > 2)
			{
				return;
			}

			bool secondRowActive = __instance.PUBLIC_SLOTS > ToolbeltConstants.PlayerSlots && __instance.GetFocusedItemIdx() >= ToolbeltConstants.PlayerSlots;
			if (secondRowActive)
			{
				return;
			}

			// In normal 12-slot play mode, force the intended recent hotkey slot under Shift.
			if (__instance.PUBLIC_SLOTS == ToolbeltConstants.PlayerSlots)
			{
				_idx = recentHotkeySlot;
				return;
			}

			int shiftOffset = __instance.SHIFT_KEY_SLOT_OFFSET;
			if (_idx == recentHotkeySlot + shiftOffset || _idx == recentHotkeySlot + 10 || _idx == recentHotkeySlot + ToolbeltConstants.PlayerSlots)
			{
				_idx = recentHotkeySlot;
			}
		}

		[HarmonyPriority(Priority.Last)]
		public static void Postfix(Inventory __instance)
		{
			if (_isReapplyingFocusedSlot)
			{
				return;
			}

			if (__instance == null || !ToolbeltFrameCache.ShiftPressed)
			{
				return;
			}

			int recentHotkeySlot = ToolbeltRuntimeState.LastPhysicalHotkeySlot;
			if (recentHotkeySlot < 0 || recentHotkeySlot >= ToolbeltConstants.PlayerSlots)
			{
				return;
			}

			if (Time.frameCount - ToolbeltRuntimeState.LastPhysicalHotkeyFrame > 2)
			{
				return;
			}

			bool secondRowActive = __instance.PUBLIC_SLOTS > ToolbeltConstants.PlayerSlots && __instance.GetFocusedItemIdx() >= ToolbeltConstants.PlayerSlots;
			if (secondRowActive)
			{
				return;
			}

			if (__instance.GetFocusedItemIdx() == recentHotkeySlot)
			{
				return;
			}

			try
			{
				_isReapplyingFocusedSlot = true;
				__instance.SetFocusedItemIdx(recentHotkeySlot);
			}
			finally
			{
				_isReapplyingFocusedSlot = false;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMoveController), "Update")]
	public class Patch_PlayerMoveController_Update
	{
		private static bool _isApplyingFromUpdate;

		[HarmonyPriority(Priority.Last)]
		public static void Postfix()
		{
			if (_isApplyingFromUpdate)
			{
				return;
			}

			GameManager gameManager = GameManager.Instance;
			EntityPlayerLocal player = gameManager?.World?.GetPrimaryPlayer();
			bool isMounted = player != null && player.AttachedToEntity != null;
			bool justDismounted = ToolbeltRuntimeState.WasMountedLastFrame && !isMounted;
			ToolbeltRuntimeState.WasMountedLastFrame = isMounted;

			Inventory toolbelt = ToolbeltFrameCache.GetPrimaryToolbelt();
			if (toolbelt == null)
			{
				return;
			}

			if (isMounted || justDismounted)
			{
				ToolbeltRuntimeState.ClearRecentHotkeyState();
				int focusedIdx = toolbelt.GetFocusedItemIdx();
				int normalizedIdx = ToolbeltRuntimeState.NormalizeToPrimaryRow(focusedIdx);
				if (focusedIdx == normalizedIdx)
				{
					return;
				}

				try
				{
					_isApplyingFromUpdate = true;
					toolbelt.SetFocusedItemIdx(normalizedIdx);
				}
				finally
				{
					_isApplyingFromUpdate = false;
				}

				return;
			}

			if (!ToolbeltFrameCache.ShiftPressed)
			{
				ToolbeltRuntimeState.ClearRecentHotkeyState();
				int focusedIdx = toolbelt.GetFocusedItemIdx();
				int normalizedIdx = ToolbeltRuntimeState.NormalizeToPrimaryRow(focusedIdx);
				if (focusedIdx == normalizedIdx)
				{
					return;
				}

				try
				{
					_isApplyingFromUpdate = true;
					toolbelt.SetFocusedItemIdx(normalizedIdx);
				}
				finally
				{
					_isApplyingFromUpdate = false;
				}

				return;
			}

			int desiredBaseSlot = ResolveDesiredShiftSlot();
			if (desiredBaseSlot < 0 || desiredBaseSlot >= ToolbeltConstants.PlayerSlots)
			{
				return;
			}

			int desiredSlot = desiredBaseSlot;
			desiredSlot = ToolbeltRuntimeState.NormalizeToPrimaryRow(desiredSlot);

			if (desiredSlot < 0 || desiredSlot >= toolbelt.PUBLIC_SLOTS)
			{
				return;
			}

			if (toolbelt.GetFocusedItemIdx() == desiredSlot)
			{
				return;
			}

			try
			{
				_isApplyingFromUpdate = true;
				toolbelt.SetFocusedItemIdx(desiredSlot);
			}
			finally
			{
				_isApplyingFromUpdate = false;
			}
		}

		private static int ResolveDesiredShiftSlot()
		{
			if (ToolbeltFrameCache.TryGetHeldHotbarKey(out int heldSlot))
			{
				ToolbeltRuntimeState.LastPhysicalHotkeySlot = heldSlot;
				ToolbeltRuntimeState.LastPhysicalHotkeyFrame = Time.frameCount;
				return heldSlot;
			}

			int recentHotkeySlot = ToolbeltRuntimeState.LastPhysicalHotkeySlot;
			if (recentHotkeySlot < 0 || recentHotkeySlot >= ToolbeltConstants.PlayerSlots)
			{
				return -1;
			}

			if (Time.frameCount - ToolbeltRuntimeState.LastPhysicalHotkeyFrame > 2)
			{
				return -1;
			}

			return recentHotkeySlot;
		}
	}
}
