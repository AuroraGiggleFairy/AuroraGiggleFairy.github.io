using System.Collections.Generic;
using UnityEngine;

namespace QuartermasterCrafting
{
	internal struct Line
	{
		public int Type;
		public int Need;
		public int Inv;
		public int Grid;
		public int Nearby;
		public int Open;
	}

	internal static class RecipeLines
	{
		public static bool LeaveVanilla(Recipe recipe)
		{
			if (recipe == null || recipe.materialBasedRecipe || recipe.ingredients == null)
			{
				return true;
			}

			if (XUiM_Recipes.GetCraftingInputModifier(recipe) == 0f)
			{
				return true;
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemValue item = StackAccess.Value(recipe.ingredients[i]);
				if (item != null && item.HasQuality)
				{
					return true;
				}
			}

			return false;
		}

		private static readonly Dictionary<int, int> NeedCache = new Dictionary<int, int>();
		private static readonly Dictionary<string, FastTags<TagGroup.Global>> TagCache = new Dictionary<string, FastTags<TagGroup.Global>>();
		private static int needFrame = -1;
		private static int tierFrame = -1;
		private static int tierRecipe;
		private static int tierArea;
		private static int tierValue;

		public static int PerCraft(Recipe recipe, ItemStack ingredient, EntityPlayer player)
		{
			if (recipe == null || ingredient == null || player == null)
			{
				return 0;
			}

			return CachedNeed(recipe, ingredient, player, Tier(recipe, player));
		}

		public static int NeedAtTier(Recipe recipe, ItemStack ingredient, EntityAlive actor, int tier)
		{
			if (recipe == null || ingredient == null)
			{
				return 0;
			}

			return CachedNeed(recipe, ingredient, actor, tier);
		}

		private static int Tier(Recipe recipe, EntityPlayer player)
		{
			int frame = Time.frameCount;
			int area = recipe.craftingArea != null ? recipe.craftingArea.GetHashCode() : 0;
			if (tierFrame == frame && tierRecipe == recipe.itemValueType && tierArea == area)
			{
				return tierValue;
			}

			tierFrame = frame;
			tierRecipe = recipe.itemValueType;
			tierArea = area;
			tierValue = recipe.GetCraftingTier(player);
			return tierValue;
		}

		private static int CachedNeed(Recipe recipe, ItemStack ingredient, EntityAlive actor, int tier)
		{
			int frame = Time.frameCount;
			if (needFrame != frame)
			{
				NeedCache.Clear();
				needFrame = frame;
			}

			int type = StackAccess.Value(ingredient) != null ? StackAccess.Value(ingredient).type : 0;
			unchecked
			{
				int key = recipe.itemValueType;
				key = (key * 31) ^ type;
				key = (key * 31) ^ StackAccess.Count(ingredient);
				key = (key * 31) ^ tier;
				key = (key * 31) ^ (recipe.craftingArea != null ? recipe.craftingArea.GetHashCode() : 0);
				if (NeedCache.TryGetValue(key, out int cached))
				{
					return cached;
				}

				int count = ComputeNeed(recipe, ingredient, actor, tier);
				NeedCache[key] = count;
				return count;
			}
		}

		private static int ComputeNeed(Recipe recipe, ItemStack ingredient, EntityAlive actor, int tier)
		{
			if (XUiM_Recipes.GetCraftingInputModifier(recipe) == 0f)
			{
				return 0;
			}

			int count = StackAccess.Count(ingredient);
			if (!recipe.UseIngredientModifier || StackAccess.Value(ingredient)?.ItemClass == null)
			{
				return count;
			}

			string name = StackAccess.Value(ingredient).ItemClass.GetItemName();
			if (name == null)
			{
				name = "";
			}

			if (!TagCache.TryGetValue(name, out FastTags<TagGroup.Global> tags))
			{
				tags = FastTags<TagGroup.Global>.Parse(name);
				TagCache[name] = tags;
			}

			count = (int)EffectManager.GetValue(
				PassiveEffects.CraftingIngredientCount,
				null,
				StackAccess.Count(ingredient),
				actor,
				recipe,
				tags,
				true,
				true,
				true,
				true,
				true,
				tier);
			if (count <= 0)
			{
				return count;
			}

			count = (int)(count * XUiM_Recipes.GetCraftingInputModifier(recipe));
			if (XUiM_Recipes.CraftingInputModifier > 0f)
			{
				count = Utils.FastMax(1, count);
			}

			return count;
		}

		private static readonly Dictionary<int, bool> CoverCache = new Dictionary<int, bool>();

		public static bool CoveredFromPool(IList<ItemStack> items, Recipe recipe, EntityPlayer player, Dictionary<int, int> nearby)
		{
			if (recipe?.ingredients == null || player == null || nearby == null || !TouchesPool(recipe, nearby))
			{
				return false;
			}

			int tier = recipe.craftingTier == -1 ? recipe.GetCraftingTier(player) : recipe.craftingTier;
			int key = CoverKey(recipe, items, nearby, tier);
			if (CoverCache.TryGetValue(key, out bool cached))
			{
				return cached;
			}

			bool covered = TierCovered(items, recipe, player, nearby, tier);
			if (CoverCache.Count > 2048)
			{
				CoverCache.Clear();
			}

			CoverCache[key] = covered;
			return covered;
		}

		private static bool TouchesPool(Recipe recipe, Dictionary<int, int> nearby)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemValue item = StackAccess.Value(recipe.ingredients[i]);
				if (item != null && nearby.TryGetValue(item.type, out int extra) && extra > 0)
				{
					return true;
				}
			}

			return false;
		}

		private static int CoverKey(Recipe recipe, IList<ItemStack> items, Dictionary<int, int> nearby, int tier)
		{
			unchecked
			{
				int key = recipe.itemValueType;
				key = (key * 31) ^ (recipe.craftingArea != null ? recipe.craftingArea.GetHashCode() : 0);
				key = (key * 31) ^ tier;
				for (int i = 0; i < recipe.ingredients.Count; i++)
				{
					ItemValue item = StackAccess.Value(recipe.ingredients[i]);
					int type = item != null ? item.type : 0;
					nearby.TryGetValue(type, out int extra);
					key = (key * 31) ^ type;
					key = (key * 31) ^ CountOf(items, type);
					key = (key * 31) ^ extra;
				}

				return key;
			}
		}

		public static int MaxCrafts(XUi xui, Recipe recipe, Dictionary<int, int> nearby)
		{
			if (xui?.playerUI?.entityPlayer == null || recipe?.ingredients == null || nearby == null)
			{
				return -1;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			CraftBridge.Resolve(xui, out Vector3i origin, out bool station);
			World world = GameManager.Instance?.World;
			TileEntityWorkstation bench = station ? world?.GetTileEntity(origin) as TileEntityWorkstation : null;
			int tier = recipe.craftingTier == -1 ? recipe.GetCraftingTier(player) : recipe.craftingTier;
			int best = int.MaxValue;
			bool required = false;
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				if (StackAccess.Value(ingredient) == null || StackAccess.Value(ingredient).type == 0)
				{
					continue;
				}

				int need = NeedAtTier(recipe, ingredient, player, tier);
				if (need < 1)
				{
					continue;
				}

				required = true;
				int have = Storage.CountInv(player, StackAccess.Value(ingredient)) + Storage.CountGrid(bench, StackAccess.Value(ingredient));
				if (nearby.TryGetValue(StackAccess.Value(ingredient).type, out int extra))
				{
					have += extra;
				}

				int crafts = have / need;
				if (crafts < best)
				{
					best = crafts;
				}
			}

			if (!required)
			{
				return 10000;
			}

			if (best == int.MaxValue)
			{
				return 0;
			}

			return Mathf.Clamp(best, 0, 10000);
		}

		private static bool TierCovered(IList<ItemStack> items, Recipe recipe, EntityAlive actor, Dictionary<int, int> nearby, int tier)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				if (StackAccess.Value(ingredient) == null || StackAccess.Value(ingredient).type == 0)
				{
					continue;
				}

				int need = NeedAtTier(recipe, ingredient, actor, tier);
				if (need < 1)
				{
					continue;
				}

				int have = CountOf(items, StackAccess.Value(ingredient).type);
				if (nearby.TryGetValue(StackAccess.Value(ingredient).type, out int extra))
				{
					have += extra;
				}

				if (have < need)
				{
					return false;
				}
			}

			return true;
		}

		private static int CountOf(IList<ItemStack> items, int type)
		{
			if (items == null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < items.Count; i++)
			{
				ItemStack stack = items[i];
				if (StackAccess.Value(stack) == null || StackAccess.Value(stack).type != type)
				{
					continue;
				}

				if (StackAccess.Value(stack).HasModSlots && StackAccess.Value(stack).HasMods())
				{
					continue;
				}

				count += StackAccess.Count(stack);
			}

			return count;
		}

		public static bool Covers(List<Line> lines)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				Line line = lines[i];
				if (line.Inv + line.Grid + line.Nearby < line.Need)
				{
					return false;
				}
			}

			return true;
		}

		public static bool VanillaPays(List<Line> lines, bool station)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				Line line = lines[i];
				int local = station ? line.Grid : line.Inv;
				if (local < line.Need)
				{
					return false;
				}
			}

			return true;
		}
	}
}
