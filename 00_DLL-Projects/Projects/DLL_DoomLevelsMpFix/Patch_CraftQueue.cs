using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Plain mods stack, and 3.3 still counts one slot as one mod.
	/// Quality parts such as a battery stay one craft unless the queue remembers each part.
	/// </summary>
	internal static class CraftQueue
	{
		internal static bool RealQuality(ItemValue value)
		{
			ItemClass item = value?.ItemClass;
			if (item == null || item is ItemClassModifier)
			{
				return false;
			}

			return value.HasQuality;
		}

		internal static bool HasRealQuality(Recipe recipe)
		{
			if (recipe?.ingredients == null)
			{
				return false;
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				if (RealQuality(recipe.ingredients[i]?.itemValue))
				{
					return true;
				}
			}

			return false;
		}

		internal static bool HasModifier(Recipe recipe)
		{
			if (recipe?.ingredients == null)
			{
				return false;
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				if (recipe.ingredients[i]?.itemValue?.ItemClass is ItemClassModifier)
				{
					return true;
				}
			}

			return false;
		}

		internal static int OneNeed(Recipe recipe, ItemStack ingredient, EntityAlive player, int tier)
		{
			if (ingredient?.itemValue == null)
			{
				return 0;
			}

			int need = ingredient.count;
			if (!recipe.UseIngredientModifier)
			{
				return need;
			}

			need = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, ingredient.count, player, recipe, FastTags<TagGroup.Global>.Parse(ingredient.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier: tier);
			if (RealQuality(ingredient.itemValue) && need <= 0)
			{
				return 1;
			}

			if (need <= 0)
			{
				return 0;
			}

			float scale = XUiM_Recipes.GetCraftingInputModifier(recipe);
			need = (int)(need * scale);
			if (XUiM_Recipes.CraftingInputModifier > 0f)
			{
				need = Math.Max(1, need);
			}

			return need;
		}

		internal static int TierOf(Recipe recipe, EntityPlayer player)
		{
			if (recipe.craftingTier != -1)
			{
				return recipe.craftingTier;
			}

			return recipe.GetCraftingTier(player);
		}

		internal static int CraftsPossible(XUi xui, Recipe recipe, int tier)
		{
			EntityPlayer player = xui.playerUI.entityPlayer;
			if (XUiM_Recipes.GetCraftingInputModifier(recipe) == 0f)
			{
				return 10000;
			}

			int max = int.MaxValue;
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				int one = OneNeed(recipe, ingredient, player, tier);
				if (one <= 0)
				{
					continue;
				}

				int have = RealQuality(ingredient.itemValue)
					? EligibleCount(player, ingredient.itemValue.type)
					: xui.PlayerInventory.GetItemCount(ingredient.itemValue);
				max = Math.Min(max, have / one);
			}

			if (max == int.MaxValue)
			{
				return 1;
			}

			return max;
		}

		internal static bool Covered(XUi xui, Recipe recipe, int crafts, int tier)
		{
			if (crafts <= 0)
			{
				return false;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			if (XUiM_Recipes.GetCraftingInputModifier(recipe) == 0f)
			{
				return true;
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				int one = OneNeed(recipe, ingredient, player, tier);
				if (one <= 0)
				{
					continue;
				}

				int have = RealQuality(ingredient.itemValue)
					? EligibleCount(player, ingredient.itemValue.type)
					: xui.PlayerInventory.GetItemCount(ingredient.itemValue);
				if (have < one * crafts)
				{
					return false;
				}
			}

			return true;
		}

		internal static List<ItemStack> Take(EntityPlayer player, int type, int count)
		{
			List<ItemStack> taken = new List<ItemStack>();
			if (count <= 0)
			{
				return taken;
			}

			List<SlotRef> slots = EligibleSlots(player, type);
			slots.Sort(CompareWorstFirst);
			for (int i = 0; i < slots.Count && count > 0; i++)
			{
				SlotRef slot = slots[i];
				ItemStack live = slot.Grid[slot.Index];
				if (live == null || live.IsEmpty() || live.itemValue.type != type)
				{
					continue;
				}

				int pull = Math.Min(live.count, count);
				ItemValue kept = live.itemValue.Clone();
				live.count -= pull;
				if (live.count <= 0)
				{
					live.Clear();
				}

				slot.Grid.OnSlotChanged(slot.Index);
				taken.Add(new ItemStack(kept, pull));
				count -= pull;
			}

			return taken;
		}

		internal static void Spend(Recipe recipe, int craftsBefore, int craftsDone)
		{
			if (recipe?.ingredients == null || craftsBefore <= 0 || craftsDone <= 0)
			{
				return;
			}

			List<int> types = new List<int>();
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemValue value = recipe.ingredients[i]?.itemValue;
				if (!RealQuality(value) || types.Contains(value.type))
				{
					continue;
				}

				types.Add(value.type);
			}

			for (int t = 0; t < types.Count; t++)
			{
				int have = CountOf(recipe, types[t]);
				int per = have / craftsBefore;
				if (per <= 0)
				{
					continue;
				}

				RemoveFront(recipe.ingredients, types[t], per * craftsDone);
			}
		}

		internal static void GiveBack(XUi xui, Recipe recipe)
		{
			if (xui == null || recipe?.ingredients == null)
			{
				return;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			for (int i = recipe.ingredients.Count - 1; i >= 0; i--)
			{
				ItemStack stack = recipe.ingredients[i];
				if (!RealQuality(stack?.itemValue) || stack.count <= 0)
				{
					continue;
				}

				recipe.ingredients.RemoveAt(i);
				ItemStack refund = new ItemStack(stack.itemValue.Clone(), stack.count);
				if (!xui.PlayerInventory.AddItem(refund))
				{
					GameManager.Instance.ItemDropServer(refund, player.GetPosition(), UnityEngine.Vector3.zero, player.entityId, 120f);
				}
			}
		}

		private static int CountOf(Recipe recipe, int type)
		{
			int count = 0;
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack stack = recipe.ingredients[i];
				if (RealQuality(stack?.itemValue) && stack.itemValue.type == type)
				{
					count += stack.count;
				}
			}

			return count;
		}

		private static void RemoveFront(List<ItemStack> ingredients, int type, int count)
		{
			for (int i = 0; i < ingredients.Count && count > 0;)
			{
				ItemStack stack = ingredients[i];
				if (!RealQuality(stack?.itemValue) || stack.itemValue.type != type)
				{
					i++;
					continue;
				}

				int pull = Math.Min(stack.count, count);
				stack.count -= pull;
				count -= pull;
				if (stack.count <= 0)
				{
					ingredients.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		private static int EligibleCount(EntityPlayer player, int type)
		{
			int count = 0;
			List<SlotRef> slots = EligibleSlots(player, type);
			for (int i = 0; i < slots.Count; i++)
			{
				count += slots[i].Grid[slots[i].Index].count;
			}

			return count;
		}

		private static List<SlotRef> EligibleSlots(EntityPlayer player, int type)
		{
			List<SlotRef> slots = new List<SlotRef>();
			if (player == null)
			{
				return slots;
			}

			AddGrid(slots, player.bag?.ItemGrid, player, type, false);
			AddGrid(slots, player.inventory?.ItemGrid, player, type, true);
			return slots;
		}

		private static void AddGrid(List<SlotRef> slots, ItemStackGrid grid, EntityPlayer player, int type, bool toolbelt)
		{
			if (grid == null)
			{
				return;
			}

			int held = toolbelt ? player.inventory.holdingItemIdx : -1;
			for (int i = 0; i < grid.Length; i++)
			{
				if (toolbelt && i == held)
				{
					continue;
				}

				ItemStack stack = grid[i];
				if (stack == null || stack.IsEmpty() || stack.itemValue.type != type)
				{
					continue;
				}

				if (stack.itemValue.HasMods() || IsWorn(player, stack.itemValue))
				{
					continue;
				}

				slots.Add(new SlotRef { Grid = grid, Index = i });
			}
		}

		private static bool IsWorn(EntityPlayer player, ItemValue value)
		{
			Equipment worn = player.equipment;
			if (worn == null || value == null)
			{
				return false;
			}

			MethodInfo getter = AccessTools.Method(worn.GetType(), "GetSlotItem");
			if (getter == null)
			{
				return false;
			}

			for (int i = 0; i < 12; i++)
			{
				object slot;
				try
				{
					slot = getter.Invoke(worn, new object[] { i });
				}
				catch (Exception)
				{
					break;
				}

				if (ReferenceEquals(slot, value))
				{
					return true;
				}
			}

			return false;
		}

		private static int CompareWorstFirst(SlotRef a, SlotRef b)
		{
			ItemValue left = a.Grid[a.Index].itemValue;
			ItemValue right = b.Grid[b.Index].itemValue;
			int quality = left.Quality.CompareTo(right.Quality);
			if (quality != 0)
			{
				return quality;
			}

			return Wear(right).CompareTo(Wear(left));
		}

		private static float Wear(ItemValue value)
		{
			int max = value.MaxUseTimes;
			if (max <= 0)
			{
				return 0f;
			}

			return value.UseTimes / max;
		}

		private struct SlotRef
		{
			public ItemStackGrid Grid;
			public int Index;
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeCraftCount), "calcMaxCraftable")]
	internal static class Patch_CraftMax
	{
		private static readonly FieldInfo RecipeField = AccessTools.Field(typeof(XUiC_RecipeCraftCount), "recipe");

		private static void Postfix(XUiC_RecipeCraftCount __instance, ref int __result)
		{
			Recipe recipe = RecipeField?.GetValue(__instance) as Recipe;
			if (recipe == null || !CraftQueue.HasRealQuality(recipe) || __instance.xui == null)
			{
				return;
			}

			int possible = CraftQueue.CraftsPossible(__instance.xui, recipe, CraftQueue.TierOf(recipe, __instance.xui.playerUI.entityPlayer));
			__result = Math.Max(1, Math.Min(possible, 10000));
		}
	}

	[HarmonyPatch(typeof(ItemActionEntryCraft), "hasItems")]
	internal static class Patch_CraftHasItems
	{
		private static readonly FieldInfo CounterField = AccessTools.Field(typeof(ItemActionEntryCraft), "craftCountControl");
		private static readonly FieldInfo TierField = AccessTools.Field(typeof(ItemActionEntryCraft), "craftingTier");
		private static readonly PropertyInfo CountProperty = AccessTools.Property(typeof(XUiC_Counter), "Count");

		private static void Postfix(ItemActionEntryCraft __instance, XUi _xui, Recipe _recipe, ref bool __result)
		{
			if (_recipe == null || _xui == null)
			{
				return;
			}

			int crafts = 1;
			XUiC_RecipeCraftCount counter = CounterField?.GetValue(__instance) as XUiC_RecipeCraftCount;
			if (counter != null && CountProperty != null)
			{
				crafts = Convert.ToInt32(CountProperty.GetValue(counter, null));
			}

			int tier = TierField != null ? Convert.ToInt32(TierField.GetValue(__instance)) : CraftQueue.TierOf(_recipe, _xui.playerUI.entityPlayer);
			if (CraftQueue.HasRealQuality(_recipe))
			{
				__result = CraftQueue.Covered(_xui, _recipe, crafts, tier);
				return;
			}

			if (!__result && CraftQueue.HasModifier(_recipe))
			{
				__result = CraftQueue.Covered(_xui, _recipe, crafts, tier);
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionEntryCraft), "OnActivated")]
	internal static class Patch_CraftActivate
	{
		private static readonly FieldInfo CounterField = AccessTools.Field(typeof(ItemActionEntryCraft), "craftCountControl");
		private static readonly FieldInfo TierField = AccessTools.Field(typeof(ItemActionEntryCraft), "craftingTier");
		private static readonly PropertyInfo CountProperty = AccessTools.Property(typeof(XUiC_Counter), "Count");

		private static bool Prefix(ItemActionEntryCraft __instance)
		{
			XUiC_RecipeEntry entry = __instance.ItemController as XUiC_RecipeEntry;
			Recipe recipe = entry?.Recipe;
			XUi xui = __instance.ItemController?.xui;
			if (recipe == null || xui == null || !CraftQueue.HasRealQuality(recipe))
			{
				return true;
			}

			XUiC_CraftingWindowGroup window = OpenWindow(xui);
			if (window == null)
			{
				return false;
			}

			if (!XUiM_Recipes.GetRecipeIsUnlocked(xui, recipe) || !window.CraftingRequirementsValid(recipe))
			{
				Dirty(window);
				return false;
			}

			XUiC_RecipeCraftCount counter = CounterField?.GetValue(__instance) as XUiC_RecipeCraftCount;
			int crafts = 1;
			if (counter != null && CountProperty != null)
			{
				crafts = Convert.ToInt32(CountProperty.GetValue(counter, null));
			}

			int tier = TierField != null ? Convert.ToInt32(TierField.GetValue(__instance)) : CraftQueue.TierOf(recipe, xui.playerUI.entityPlayer);
			if (!CraftQueue.Covered(xui, recipe, crafts, tier))
			{
				return false;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			Recipe queued = new Recipe
			{
				itemValueType = recipe.itemValueType,
				count = XUiM_Recipes.GetRecipeCraftOutputCount(xui, recipe),
				craftingArea = recipe.craftingArea,
				craftExpGain = recipe.craftExpGain,
				craftingTime = XUiM_Recipes.GetRecipeCraftTime(xui, recipe),
				craftingToolType = recipe.craftingToolType,
				craftingTier = tier,
				tags = recipe.tags
			};

			List<ItemStack> plain = new List<ItemStack>();
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				int one = CraftQueue.OneNeed(recipe, ingredient, player, tier);
				if (one <= 0 || CraftQueue.RealQuality(ingredient.itemValue))
				{
					continue;
				}

				plain.Add(new ItemStack(ingredient.itemValue, one));
			}

			queued.AddIngredients(plain);
			if (!window.AddItemToQueue(queued, crafts))
			{
				GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiCraftQueueFull"));
				Audio.Manager.PlayInsidePlayerHead("ui_denied");
				Dirty(window);
				return false;
			}

			if (plain.Count > 0)
			{
				xui.PlayerInventory.RemoveItems(plain, crafts, null);
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				if (!CraftQueue.RealQuality(ingredient?.itemValue))
				{
					continue;
				}

				int one = CraftQueue.OneNeed(recipe, ingredient, player, tier);
				List<ItemStack> taken = CraftQueue.Take(player, ingredient.itemValue.type, one * crafts);
				for (int n = 0; n < taken.Count; n++)
				{
					queued.ingredients.Add(taken[n]);
				}
			}

			xui.PlayerInventory.dispatchBackpackItemsChanged();
			xui.PlayerInventory.dispatchToolbeltItemsChanged();
			if (recipe == xui.Recipes.TrackedRecipe)
			{
				xui.Recipes.TrackedRecipe = null;
				xui.Recipes.ResetToPreviousTracked(player as EntityPlayerLocal);
			}

			Dirty(window);
			return false;
		}

		private static XUiC_CraftingWindowGroup OpenWindow(XUi xui)
		{
			List<XUiC_CraftingWindowGroup> windows = xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].WindowGroup != null && windows[i].WindowGroup.isShowing)
				{
					return windows[i];
				}
			}

			return null;
		}

		private static void Dirty(XUiC_CraftingWindowGroup window)
		{
			window?.WindowGroup?.Controller?.SetAllChildrenDirty();
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeStack), "HandleOnPress")]
	internal static class Patch_CraftCancel
	{
		private static readonly FieldInfo RepairField = AccessTools.Field(typeof(XUiC_RecipeStack), "amountToRepair");
		private static readonly FieldInfo RecipeField = AccessTools.Field(typeof(XUiC_RecipeStack), "recipe");

		private static void Prefix(XUiC_RecipeStack __instance)
		{
			if (RepairField != null && Convert.ToInt32(RepairField.GetValue(__instance)) > 0)
			{
				return;
			}

			Recipe recipe = RecipeField?.GetValue(__instance) as Recipe;
			CraftQueue.GiveBack(__instance.xui, recipe);
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeStack), "Update")]
	internal static class Patch_CraftStackTick
	{
		private static readonly FieldInfo RecipeField = AccessTools.Field(typeof(XUiC_RecipeStack), "recipe");
		private static readonly FieldInfo CountField = AccessTools.Field(typeof(XUiC_RecipeStack), "recipeCount");
		private static readonly Dictionary<XUiC_RecipeStack, int> Seen = new Dictionary<XUiC_RecipeStack, int>();

		private static void Postfix(XUiC_RecipeStack __instance)
		{
			Recipe recipe = RecipeField?.GetValue(__instance) as Recipe;
			int count = CountField != null ? Convert.ToInt32(CountField.GetValue(__instance)) : 0;
			if (!Seen.TryGetValue(__instance, out int previous))
			{
				if (recipe != null)
				{
					Seen[__instance] = count;
				}

				return;
			}

			if (recipe != null && previous > count)
			{
				CraftQueue.Spend(recipe, previous, previous - count);
			}

			if (recipe == null)
			{
				Seen.Remove(__instance);
			}
			else
			{
				Seen[__instance] = count;
			}
		}
	}

	[HarmonyPatch(typeof(TileEntityWorkstation), "HandleRecipeQueue")]
	internal static class Patch_CraftStationTick
	{
		private static readonly FieldInfo QueueField = AccessTools.Field(typeof(TileEntityWorkstation), "queue");
		private static readonly List<Snap> Before = new List<Snap>();

		private static void Prefix(TileEntityWorkstation __instance)
		{
			Before.Clear();
			Array queue = QueueField?.GetValue(__instance) as Array;
			if (queue == null)
			{
				return;
			}

			for (int i = 0; i < queue.Length; i++)
			{
				object item = queue.GetValue(i);
				if (item == null)
				{
					continue;
				}

				Recipe recipe = ReadRecipe(item);
				int multiplier = ReadMultiplier(item);
				if (recipe != null && multiplier > 0)
				{
					Before.Add(new Snap { Recipe = recipe, Multiplier = multiplier });
				}
			}
		}

		private static void Postfix(TileEntityWorkstation __instance)
		{
			Array queue = QueueField?.GetValue(__instance) as Array;
			for (int i = 0; i < Before.Count; i++)
			{
				Snap snap = Before[i];
				int left = MultiplierOf(queue, snap.Recipe);
				int done = snap.Multiplier - Math.Max(0, left);
				if (done > 0)
				{
					CraftQueue.Spend(snap.Recipe, snap.Multiplier, done);
				}
			}

			Before.Clear();
		}

		private static int MultiplierOf(Array queue, Recipe recipe)
		{
			if (queue == null)
			{
				return -1;
			}

			for (int i = 0; i < queue.Length; i++)
			{
				object item = queue.GetValue(i);
				if (item != null && ReferenceEquals(ReadRecipe(item), recipe))
				{
					return ReadMultiplier(item);
				}
			}

			return -1;
		}

		private static Recipe ReadRecipe(object item)
		{
			PropertyInfo property = item.GetType().GetProperty("Recipe");
			if (property != null)
			{
				return property.GetValue(item, null) as Recipe;
			}

			FieldInfo field = item.GetType().GetField("Recipe");
			return field?.GetValue(item) as Recipe;
		}

		private static int ReadMultiplier(object item)
		{
			PropertyInfo property = item.GetType().GetProperty("Multiplier");
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(item, null));
			}

			FieldInfo field = item.GetType().GetField("Multiplier");
			return field != null ? Convert.ToInt32(field.GetValue(item)) : 0;
		}

		private struct Snap
		{
			public Recipe Recipe;
			public int Multiplier;
		}
	}
}
