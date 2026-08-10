using HarmonyLib;

namespace FuelAutoShutOff
{
	/// <summary>
	/// While a fuel workstation UI is open, mirror extinguish into the fuel On/Off control.
	/// TE UpdateTick remains authoritative on server/host.
	/// </summary>
	[HarmonyPatch(typeof(XUiC_WorkstationWindowGroup), nameof(XUiC_WorkstationWindowGroup.Update))]
	public static class Patch_WorkstationWindowGroup_Update
	{
		public static void Prefix(
			XUiC_CraftingQueue ___craftingQueue,
			XUiC_WorkstationInputGrid ___inputWindow,
			out bool __state)
		{
			__state = HasUiProductiveWork(___craftingQueue, ___inputWindow);
		}

		public static void Postfix(
			bool __state,
			XUiC_CraftingQueue ___craftingQueue,
			XUiC_WorkstationInputGrid ___inputWindow,
			XUiC_WorkstationFuelGrid ___fuelWindow)
		{
			if (___fuelWindow == null)
			{
				return;
			}

			try
			{
				TileEntityWorkstation te = ___fuelWindow.WorkstationData?.TileEntity;
				if (te == null)
				{
					return;
				}

				// TE UpdateTick may extinguish first (common for "can't continue smelting").
				// Crafting-complete often TurnOffs while still burning; sync the button if TE
				// is already off but the fuel grid still thinks it is on.
				if (!te.IsBurning)
				{
					SyncFuelUiToOff(___fuelWindow);
					return;
				}

				bool[] modules = AccessTools.Field(typeof(TileEntityWorkstation), "isModuleUsed")
					?.GetValue(te) as bool[];
				RecipeQueueItem[] queue = te.Queue;
				ItemStack[] input = te.Input;
				float[] meltTimes = BuildMeltSnapshot(te);

				if (!FuelAutoShutOffLogic.ShouldExtinguish(
					te,
					queue,
					input,
					modules,
					meltTimes,
					__state))
				{
					return;
				}

				___fuelWindow.TurnOff();
			}
			catch
			{
				// UI-only assist; ignore.
			}
		}

		private static void SyncFuelUiToOff(XUiC_WorkstationFuelGrid fuelWindow)
		{
			var isOnField = AccessTools.Field(typeof(XUiC_WorkstationFuelGrid), "isOn");
			if (isOnField == null)
			{
				return;
			}

			object value = isOnField.GetValue(fuelWindow);
			if (value is bool isOn && isOn)
			{
				fuelWindow.TurnOff();
			}
		}

		private static bool HasUiProductiveWork(XUiC_CraftingQueue craftingQueue, XUiC_WorkstationInputGrid inputWindow)
		{
			if (craftingQueue != null)
			{
				XUiC_RecipeStack[] recipes = craftingQueue.GetRecipesToCraft();
				if (recipes != null)
				{
					for (int i = 0; i < recipes.Length; i++)
					{
						XUiC_RecipeStack item = recipes[i];
						if (item != null && item.IsCrafting)
						{
							return true;
						}
					}
				}
			}

			if (inputWindow == null)
			{
				return false;
			}

			TileEntityWorkstation te = inputWindow.WorkstationData?.TileEntity;
			if (te == null)
			{
				return false;
			}

			bool[] modules = AccessTools.Field(typeof(TileEntityWorkstation), "isModuleUsed")
				?.GetValue(te) as bool[];
			return FuelAutoShutOffLogic.HasProductiveWork(
				te,
				te.Queue,
				te.Input,
				modules,
				BuildMeltSnapshot(te));
		}

		private static float[] BuildMeltSnapshot(TileEntityWorkstation te)
		{
			if (te?.Input == null)
			{
				return null;
			}

			float[] times = new float[te.Input.Length];
			for (int i = 0; i < times.Length; i++)
			{
				times[i] = te.GetTimerForSlot(i);
			}

			return times;
		}
	}
}
