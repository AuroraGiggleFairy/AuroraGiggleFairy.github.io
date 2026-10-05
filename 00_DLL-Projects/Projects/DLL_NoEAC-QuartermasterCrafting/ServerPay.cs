using System.Collections.Generic;

namespace QuartermasterCrafting
{
	internal sealed class PendingTake
	{
		public int Token;
		public List<Undo> Undos;
	}

	internal static class ServerPay
	{
		private static readonly object Gate = new object();
		private static readonly Dictionary<int, PendingTake> Pending = new Dictionary<int, PendingTake>();
		private static int nextToken = 1;

		public static Recipe Find(string name, string area)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			List<Recipe> all = CraftingManager.GetAllRecipes();
			if (all == null)
			{
				return null;
			}

			Recipe fallback = null;
			string wanted = area ?? "";
			for (int i = 0; i < all.Count; i++)
			{
				Recipe recipe = all[i];
				if (recipe == null || recipe.GetName() != name)
				{
					continue;
				}

				if (fallback == null)
				{
					fallback = recipe;
				}

				string craftArea = recipe.craftingArea ?? "";
				if (craftArea == wanted)
				{
					return recipe;
				}
			}

			return fallback;
		}

		public static void Query(EntityPlayer player, string name, string area, int count, Vector3i origin, bool station, List<Line> lines)
		{
			lines.Clear();
			Recipe recipe = Find(name, area);
			if (player == null || recipe == null || RecipeLines.LeaveVanilla(recipe))
			{
				return;
			}

			if (!Storage.OriginAllowed(player, origin, station))
			{
				return;
			}

			if (!station)
			{
				origin = Storage.PlayerOrigin(player);
			}

			Storage.Fill(player, origin, station, recipe, Clamp(count), lines, false);
		}

		public static void Pool(EntityPlayer player, Vector3i origin, bool station, List<Line> lines)
		{
			lines.Clear();
			if (player == null || !Storage.OriginAllowed(player, origin, station))
			{
				return;
			}

			if (!station)
			{
				origin = Storage.PlayerOrigin(player);
			}

			var counts = new Dictionary<int, int>();
			Storage.CollectClosed(player, origin, counts);
			foreach (KeyValuePair<int, int> pair in counts)
			{
				lines.Add(new Line
				{
					Type = pair.Key,
					Nearby = pair.Value
				});
			}
		}

		public static byte Pay(EntityPlayer player, string name, string area, int count, Vector3i origin, bool station, List<Line> lines, out int token)
		{
			token = 0;
			lines.Clear();
			if (player == null)
			{
				return PayCode.Bad;
			}

			lock (Gate)
			{
				if (Pending.ContainsKey(player.entityId))
				{
					return PayCode.Bad;
				}

				Recipe recipe = Find(name, area);
				if (recipe == null || RecipeLines.LeaveVanilla(recipe))
				{
					return PayCode.Bad;
				}

				if (!Storage.OriginAllowed(player, origin, station))
				{
					return PayCode.Bad;
				}

				if (!station)
				{
					origin = Storage.PlayerOrigin(player);
				}

				count = Clamp(count);
				List<Storage.Chest> closed = Storage.Fresh(player, origin, false);
				Storage.FillFrom(player, origin, station, recipe, count, lines, closed);
				if (lines.Count == 0)
				{
					return PayCode.Bad;
				}

				if (!RecipeLines.Covers(lines))
				{
					Storage.NoteOpen(player, origin, lines);
					return Storage.ShortReason(lines);
				}

				if (!NeedsBoxes(lines))
				{
					return PayCode.NoNeed;
				}

				var undos = new List<Undo>();
				if (!Storage.TakeFrom(closed, lines, undos))
				{
					Storage.NoteOpen(player, origin, lines);
					return Storage.ShortReason(lines);
				}

				token = nextToken++;
				if (token == 0)
				{
					token = nextToken++;
				}

				Pending[player.entityId] = new PendingTake
				{
					Token = token,
					Undos = undos
				};
				return PayCode.Paid;
			}
		}

		public static void Ack(int entityId, int token)
		{
			lock (Gate)
			{
				PendingTake pending;
				if (Pending.TryGetValue(entityId, out pending) && pending.Token == token)
				{
					Pending.Remove(entityId);
				}
			}
		}

		public static void Refund(int entityId, int token)
		{
			PendingTake pending = null;
			lock (Gate)
			{
				if (Pending.TryGetValue(entityId, out pending) && pending.Token == token)
				{
					Pending.Remove(entityId);
				}
				else
				{
					pending = null;
				}
			}

			if (pending != null)
			{
				Storage.Refund(GameManager.Instance?.World, pending.Undos);
			}
		}

		private static bool NeedsBoxes(List<Line> lines)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].Need > lines[i].Inv + lines[i].Grid)
				{
					return true;
				}
			}

			return false;
		}

		private static int Clamp(int count)
		{
			if (count < 1)
			{
				return 1;
			}

			return count > 10000 ? 10000 : count;
		}
	}
}
