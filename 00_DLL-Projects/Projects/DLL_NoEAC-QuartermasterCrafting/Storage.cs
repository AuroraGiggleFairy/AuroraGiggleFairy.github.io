using System.Collections.Generic;
using UnityEngine;

namespace QuartermasterCrafting
{
	internal struct Undo
	{
		public Vector3i Pos;
		public int Slot;
		public int Count;
		public int Type;
	}

	internal static class Storage
	{
		private const int YMin = 0;
		private const int YMax = 253;
		private const int StationReach = 8;

		public static bool IsServer()
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			return manager != null && manager.IsServer;
		}

		public static int CountInv(EntityPlayer player, ItemValue item)
		{
			if (player == null || item == null)
			{
				return 0;
			}

			int count = 0;
			if (player.bag != null)
			{
				count += player.bag.GetItemCount(item);
			}

			if (player.inventory != null)
			{
				count += player.inventory.GetItemCount(item);
			}

			return count;
		}

		public static int CountGrid(TileEntityWorkstation station, ItemValue item)
		{
			if (station?.Input == null || item == null)
			{
				return 0;
			}

			int count = 0;
			ItemStack[] slots = station.Input;
			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i] != null && !slots[i].IsEmpty() && StackAccess.TypeId(StackAccess.Value(slots[i])) == StackAccess.TypeId(item))
				{
					count += StackAccess.Count(slots[i]);
				}
			}

			return count;
		}

		public static bool OriginAllowed(EntityPlayer player, Vector3i origin, bool station)
		{
			if (player == null)
			{
				return false;
			}

			Vector3i playerPos = new Vector3i(player.position);
			if (!station)
			{
				return true;
			}

			return Chebyshev(playerPos, origin) <= StationReach;
		}

		public static Vector3i PlayerOrigin(EntityPlayer player)
		{
			return new Vector3i(player.position);
		}

		public static void Fill(EntityPlayer player, Vector3i origin, bool station, Recipe recipe, int craftCount, List<Line> lines, bool countOpen)
		{
			List<Chest> closed = Fresh(player, origin, false);
			FillFrom(player, origin, station, recipe, craftCount, lines, closed);
			if (countOpen && !RecipeLines.Covers(lines))
			{
				NoteOpen(player, origin, lines);
			}
		}

		internal static List<Chest> Fresh(EntityPlayer player, Vector3i origin, bool openOnly)
		{
			World world = GameManager.Instance?.World;
			if (world == null || player == null)
			{
				return new List<Chest>();
			}

			return FindChests(world, origin, player, openOnly, false);
		}

		internal static void FillFrom(EntityPlayer player, Vector3i origin, bool station, Recipe recipe, int craftCount, List<Line> lines, List<Chest> closed)
		{
			lines.Clear();
			if (player == null || recipe?.ingredients == null)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			TileEntityWorkstation bench = station ? world?.GetTileEntity(origin) as TileEntityWorkstation : null;
			TallyScratch.Clear();
			Tally(closed, TallyScratch, false);
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				ItemValue item = StackAccess.Value(ingredient);
				int type = StackAccess.TypeId(item);
				int per = RecipeLines.PerCraft(recipe, ingredient, player);
				TallyScratch.TryGetValue(type, out int nearby);
				lines.Add(new Line
				{
					Type = type,
					Need = per * craftCount,
					Inv = CountInv(player, item),
					Grid = CountGrid(bench, item),
					Nearby = nearby,
					Open = 0
				});
			}
		}

		public static void NoteOpen(EntityPlayer player, Vector3i origin, List<Line> lines)
		{
			List<Chest> open = Fresh(player, origin, true);
			TallyScratch.Clear();
			Tally(open, TallyScratch, false);
			for (int i = 0; i < lines.Count; i++)
			{
				Line line = lines[i];
				TallyScratch.TryGetValue(line.Type, out line.Open);
				lines[i] = line;
			}
		}

		public static bool TryTake(EntityPlayer player, Vector3i origin, List<Line> lines, List<Undo> undos)
		{
			return TakeFrom(Fresh(player, origin, false), lines, undos);
		}

		internal static bool TakeFrom(List<Chest> chests, List<Line> lines, List<Undo> undos)
		{
			undos.Clear();
			World world = GameManager.Instance?.World;
			if (chests == null || world == null)
			{
				return false;
			}
			for (int i = 0; i < lines.Count; i++)
			{
				int deficit = lines[i].Need - lines[i].Inv - lines[i].Grid;
				if (deficit <= 0)
				{
					continue;
				}

				ItemValue sample = new ItemValue(lines[i].Type);
				if (StackAccess.TypeId(sample) == 0)
				{
					Refund(world, undos);
					undos.Clear();
					return false;
				}

				int got = 0;
				for (int c = 0; c < chests.Count && got < deficit; c++)
				{
					got += TakeChest(chests[c], sample, deficit - got, undos);
				}

				if (got < deficit)
				{
					Refund(world, undos);
					undos.Clear();
					return false;
				}
			}

			return true;
		}

		public static void Refund(World world, List<Undo> undos)
		{
			if (world == null || undos == null)
			{
				return;
			}

			for (int i = undos.Count - 1; i >= 0; i--)
			{
				PutBack(world, undos[i]);
			}
		}

		public static byte ShortReason(List<Line> lines)
		{
			bool anyShort = false;
			bool openCovers = true;
			for (int i = 0; i < lines.Count; i++)
			{
				Line line = lines[i];
				int deficit = line.Need - line.Inv - line.Grid;
				if (deficit <= line.Nearby)
				{
					continue;
				}

				anyShort = true;
				if (deficit > line.Nearby + line.Open)
				{
					openCovers = false;
				}
			}

			if (!anyShort)
			{
				return PayCode.Paid;
			}

			return openCovers ? PayCode.Open : PayCode.Missing;
		}

		private static int TakeChest(Chest chest, ItemValue want, int amount, List<Undo> undos)
		{
			TEFeatureStorage storage = chest.Storage;
			ItemStack[] items = StorageAccess.Items(storage);
			if (items == null || amount <= 0)
			{
				return 0;
			}

			int got = 0;
			for (int i = 0; i < items.Length && got < amount; i++)
			{
				ItemStack stack = items[i];
				if (!Counts(storage, stack, i) || StackAccess.TypeId(StackAccess.Value(stack)) != StackAccess.TypeId(want))
				{
					continue;
				}

				int take = StackAccess.Count(stack) < amount - got ? StackAccess.Count(stack) : amount - got;
				undos.Add(new Undo
				{
					Pos = chest.Pos,
					Slot = i,
					Count = take,
					Type = StackAccess.TypeId(want)
				});
				StackAccess.SetCount(stack, StackAccess.Count(stack) - (take));
				if (StackAccess.Count(stack) <= 0)
				{
					storage.UpdateSlot(i, ItemStack.Empty.Clone());
				}
				else
				{
					storage.UpdateSlot(i, stack);
				}

				got += take;
			}

			if (got > 0)
			{
				MarkModified(chest.Tile, storage);
			}

			return got;
		}

		private static void PutBack(World world, Undo undo)
		{
			int left = undo.Count;
			TileEntity tile = world.GetTileEntity(undo.Pos);
			if (TryContainer(tile, out TEFeatureStorage storage) && StorageAccess.Items(storage) != null)
			{
				left = AddToSlots(storage, undo.Type, left, undo.Slot);
				if (left < undo.Count)
				{
					MarkModified(tile, storage);
				}
			}

			if (left <= 0 || GameManager.Instance == null)
			{
				return;
			}

			GameManager.Instance.ItemDropServer(
				new ItemStack(new ItemValue(undo.Type), left),
				undo.Pos.ToVector3() + new Vector3(0.5f, 1f, 0.5f),
				Vector3.zero);
		}

		private static int AddToSlots(TEFeatureStorage storage, int type, int count, int preferred)
		{
			ItemStack[] items = StorageAccess.Items(storage);
			int stackMax = StackMax(type);
			count = AddAt(storage, items, preferred, type, count, stackMax);
			for (int i = 0; i < items.Length && count > 0; i++)
			{
				count = AddAt(storage, items, i, type, count, stackMax);
			}

			return count;
		}

		private static int AddAt(TEFeatureStorage storage, ItemStack[] items, int index, int type, int count, int stackMax)
		{
			if (index < 0 || index >= items.Length || count <= 0 || IsSlotLocked(storage, index))
			{
				return count;
			}

			ItemStack stack = items[index];
			if (stack == null || stack.IsEmpty())
			{
				int place = count < stackMax ? count : stackMax;
				storage.UpdateSlot(index, new ItemStack(new ItemValue(type), place));
				return count - place;
			}

			if (StackAccess.TypeId(StackAccess.Value(stack)) != type || (StackAccess.Value(stack).HasModSlots && StackAccess.Value(stack).HasMods()))
			{
				return count;
			}

			int room = stackMax - StackAccess.Count(stack);
			if (room <= 0)
			{
				return count;
			}

			int add = count < room ? count : room;
			StackAccess.SetCount(stack, StackAccess.Count(stack) + (add));
			storage.UpdateSlot(index, stack);
			return count - add;
		}

		private static int StackMax(int type)
		{
			ItemClass itemClass = new ItemValue(type).ItemClass;
			int max = itemClass?.Stacknumber != null ? itemClass.Stacknumber.Value : 1;
			return max < 1 ? 1 : max;
		}

		public static void CollectClosed(EntityPlayer player, Vector3i origin, Dictionary<int, int> counts)
		{
			counts.Clear();
			World world = GameManager.Instance?.World;
			if (world == null || player == null)
			{
				return;
			}

			List<Chest> chests = Remember(world, player, origin);
			bool dirty = false;
			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				TileEntity live = world.GetTileEntity(chest.Pos);
				if (live == null || live != chest.Tile)
				{
					dirty = true;
					continue;
				}

				if (IsInUse(chest.Tile) || StorageAccess.Items(chest.Storage) == null)
				{
					continue;
				}

				ItemStack[] items = StorageAccess.Items(chest.Storage);
				for (int slot = 0; slot < items.Length; slot++)
				{
					ItemStack stack = items[slot];
					if (!Counts(chest.Storage, stack, slot))
					{
						continue;
					}

					int type = StackAccess.TypeId(StackAccess.Value(stack));
					counts.TryGetValue(type, out int have);
					counts[type] = have + StackAccess.Count(stack);
				}
			}

			if (dirty && Memories.TryGetValue(player.entityId, out Memory memory))
			{
				memory.Dirty = true;
			}
		}

		private static int Sum(List<Chest> chests, ItemValue item)
		{
			if (item == null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < chests.Count; i++)
			{
				ItemStack[] items = StorageAccess.Items(chests[i].Storage);
				if (items == null)
				{
					continue;
				}

				for (int s = 0; s < items.Length; s++)
				{
					ItemStack stack = items[s];
					if (Counts(chests[i].Storage, stack, s) && StackAccess.TypeId(StackAccess.Value(stack)) == StackAccess.TypeId(item))
					{
						count += StackAccess.Count(stack);
					}
				}
			}

			return count;
		}

		private static bool Counts(TEFeatureStorage storage, ItemStack stack, int index)
		{
			return stack != null
				&& !stack.IsEmpty()
				&& StackAccess.Value(stack) != null
				&& !(StackAccess.Value(stack).HasModSlots && StackAccess.Value(stack).HasMods())
				&& !IsSlotLocked(storage, index);
		}

		private static readonly Dictionary<int, int> TallyScratch = new Dictionary<int, int>();
		private static readonly Dictionary<int, Memory> Memories = new Dictionary<int, Memory>();
		private const float MemorySeconds = 8f;

		private sealed class Memory
		{
			public string Key = "";
			public float Until;
			public bool Dirty;
			public readonly List<Chest> Chests = new List<Chest>();
		}

		private static void Tally(List<Chest> chests, Dictionary<int, int> counts, bool skipOpen)
		{
			if (chests == null)
			{
				return;
			}

			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				if (StorageAccess.Items(chest.Storage) == null)
				{
					continue;
				}

				if (skipOpen && IsInUse(chest.Tile))
				{
					continue;
				}

				ItemStack[] items = StorageAccess.Items(chest.Storage);
				for (int slot = 0; slot < items.Length; slot++)
				{
					ItemStack stack = items[slot];
					if (!Counts(chest.Storage, stack, slot))
					{
						continue;
					}

					int type = StackAccess.TypeId(StackAccess.Value(stack));
					counts.TryGetValue(type, out int have);
					counts[type] = have + StackAccess.Count(stack);
				}
			}
		}

		private static List<Chest> Remember(World world, EntityPlayer player, Vector3i origin)
		{
			int id = player.entityId;
			if (!Memories.TryGetValue(id, out Memory memory))
			{
				memory = new Memory();
				Memories[id] = memory;
			}

			string key = AreaKey(origin);
			if (!memory.Dirty && memory.Key == key && Time.unscaledTime <= memory.Until)
			{
				return memory.Chests;
			}

			memory.Chests.Clear();
			memory.Chests.AddRange(FindChests(world, origin, player, false, true));
			memory.Key = key;
			memory.Until = Time.unscaledTime + MemorySeconds;
			memory.Dirty = false;
			return memory.Chests;
		}

		private static string AreaKey(Vector3i origin)
		{
			var claims = new List<Vector3i>();
			int radius = LandClaimRadius();
			CollectClaims(origin, radius, claims);
			if (claims.Count == 0)
			{
				return "o:" + (origin.x >> 3) + ":" + (origin.z >> 3);
			}

			claims.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.z.CompareTo(b.z));
			string key = "c";
			for (int i = 0; i < claims.Count; i++)
			{
				key += ":" + claims[i].x + "," + claims[i].z;
			}

			return key;
		}

		private static List<Chest> FindChests(World world, Vector3i origin, EntityPlayer player, bool openOnly, bool anyState)
		{
			var results = new List<Chest>();
			if (!TryBounds(origin, out Vector3i min, out Vector3i max, out List<Vector3i> claims))
			{
				return results;
			}

			int radius = LandClaimRadius();
			PlatformUserIdentifierAbs playerId = PlayerId(player);
			int minCx = min.x >> 4;
			int maxCx = max.x >> 4;
			int minCz = min.z >> 4;
			int maxCz = max.z >> 4;
			for (int cx = minCx; cx <= maxCx; cx++)
			{
				for (int cz = minCz; cz <= maxCz; cz++)
				{
					if (!(world.GetChunkSync(cx, cz) is Chunk chunk))
					{
						continue;
					}

					DictionaryList<Vector3i, TileEntity> tes = chunk.GetTileEntities();
					if (tes?.dict == null)
					{
						continue;
					}

					foreach (KeyValuePair<Vector3i, TileEntity> pair in tes.dict)
					{
						TileEntity tile = pair.Value;
						if (tile == null)
						{
							continue;
						}

						Vector3i pos = tile.ToWorldPos();
						if (!InBounds(pos, min, max))
						{
							continue;
						}

						if (claims.Count > 0 && !InAnyClaim(pos, claims, radius))
						{
							continue;
						}

						if (!TryContainer(tile, out TEFeatureStorage storage) || !StorageAccess.IsPlayerOwned(storage))
						{
							continue;
						}

						if (!CanOpen(tile, playerId))
						{
							continue;
						}

						if (!anyState && IsInUse(tile) != openOnly)
						{
							continue;
						}

						results.Add(new Chest
						{
							Tile = tile,
							Storage = storage,
							Pos = pos,
							Distance = Chebyshev(origin, pos)
						});
					}
				}
			}

			if (!anyState)
			{
				results.Sort((a, b) => a.Distance.CompareTo(b.Distance));
			}

			return results;
		}

		private static bool TryBounds(Vector3i origin, out Vector3i min, out Vector3i max, out List<Vector3i> claims)
		{
			min = max = default;
			claims = new List<Vector3i>();
			World world = GameManager.Instance?.World;
			if (world == null || !world.GetWorldExtent(out Vector3i worldMin, out Vector3i worldMax))
			{
				return false;
			}

			int radius = LandClaimRadius();
			CollectClaims(origin, radius, claims);
			if (claims.Count == 0)
			{
				min.x = origin.x - radius;
				max.x = origin.x + radius;
				min.z = origin.z - radius;
				max.z = origin.z + radius;
			}
			else
			{
				min.x = int.MaxValue;
				max.x = int.MinValue;
				min.z = int.MaxValue;
				max.z = int.MinValue;
				for (int i = 0; i < claims.Count; i++)
				{
					Vector3i claim = claims[i];
					min.x = Mathf.Min(min.x, claim.x - radius);
					max.x = Mathf.Max(max.x, claim.x + radius);
					min.z = Mathf.Min(min.z, claim.z - radius);
					max.z = Mathf.Max(max.z, claim.z + radius);
				}
			}

			min.y = YMin;
			max.y = YMax;
			min.x = Mathf.Max(min.x, worldMin.x);
			max.x = Mathf.Min(max.x, worldMax.x);
			min.y = Mathf.Max(min.y, worldMin.y, YMin);
			max.y = Mathf.Min(max.y, worldMax.y, YMax);
			min.z = Mathf.Max(min.z, worldMin.z);
			max.z = Mathf.Min(max.z, worldMax.z);
			return min.x <= max.x && min.y <= max.y && min.z <= max.z;
		}

		private static int LandClaimRadius()
		{
			int size = GameStats.GetInt(EnumGameStats.LandClaimSize);
			if (size < 1)
			{
				size = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
			}

			if (size < 1)
			{
				size = 1;
			}

			int radius = (size % 2 == 1 ? size - 1 : size) / 2;
			return radius < 1 ? 1 : radius;
		}

		private static void CollectClaims(Vector3i source, int radius, List<Vector3i> claims)
		{
			PersistentPlayerList players = GameManager.Instance?.persistentPlayers;
			World world = GameManager.Instance?.World;
			if (players?.Players == null || world == null)
			{
				return;
			}

			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> pair in players.Players)
			{
				if (pair.Value == null || !world.IsLandProtectionValidForPlayer(pair.Value))
				{
					continue;
				}

				var blocks = pair.Value.GetLandProtectionBlocks();
				if (blocks == null)
				{
					continue;
				}

				foreach (Vector3i pos in blocks)
				{
					if (InClaim(source, pos, radius))
					{
						claims.Add(pos);
					}
				}
			}
		}

		private static bool InAnyClaim(Vector3i pos, List<Vector3i> claims, int radius)
		{
			for (int i = 0; i < claims.Count; i++)
			{
				if (InClaim(pos, claims[i], radius))
				{
					return true;
				}
			}

			return false;
		}

		private static bool InClaim(Vector3i pos, Vector3i claim, int radius)
		{
			return pos.x >= claim.x - radius && pos.x <= claim.x + radius
				&& pos.z >= claim.z - radius && pos.z <= claim.z + radius;
		}

		private static bool InBounds(Vector3i pos, Vector3i min, Vector3i max)
		{
			return pos.x >= min.x && pos.x <= max.x
				&& pos.y >= min.y && pos.y <= max.y
				&& pos.z >= min.z && pos.z <= max.z;
		}

		private static int Chebyshev(Vector3i a, Vector3i b)
		{
			int dx = a.x > b.x ? a.x - b.x : b.x - a.x;
			int dy = a.y > b.y ? a.y - b.y : b.y - a.y;
			int dz = a.z > b.z ? a.z - b.z : b.z - a.z;
			int m = dx > dy ? dx : dy;
			return m > dz ? m : dz;
		}

		private static PlatformUserIdentifierAbs PlayerId(EntityPlayer player)
		{
			return GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId)?.PrimaryId;
		}

		private static bool CanOpen(TileEntity tile, PlatformUserIdentifierAbs playerId)
		{
			if (!TryLock(tile, out ILockable lockable) || !lockable.IsLocked())
			{
				return true;
			}

			if (playerId == null)
			{
				return false;
			}

			if (lockable.IsUserAllowed(playerId) || lockable.IsOwner(playerId))
			{
				return true;
			}

			PlatformUserIdentifierAbs owner = lockable.GetOwner();
			return owner != null && owner.Equals(playerId);
		}

		private static bool IsInUse(TileEntity tile)
		{
			if (tile == null || LockManager.Instance == null)
			{
				return false;
			}

			if (LockManager.Instance.IsLockedServer(tile))
			{
				return true;
			}

			return TryContainer(tile, out TEFeatureStorage storage)
				&& storage is ILockTarget target
				&& LockManager.Instance.IsLockedServer(target);
		}

		private static bool TryContainer(TileEntity tile, out TEFeatureStorage storage)
		{
			storage = null;
			if (tile == null)
			{
				return false;
			}

			if (tile is TileEntityComposite composite)
			{
				storage = composite.GetFeature<TEFeatureStorage>();
				return storage != null;
			}

			if (typeof(TEFeatureStorage).IsInstanceOfType(tile))
			{
				storage = (TEFeatureStorage)(object)tile;
				return true;
			}

			return false;
		}

		private static bool TryLock(TileEntity tile, out ILockable lockable)
		{
			lockable = null;
			if (tile == null)
			{
				return false;
			}

			if (tile is TileEntityComposite composite)
			{
				lockable = composite.GetFeature<TEFeatureLockable>();
				return lockable != null;
			}

			lockable = tile as ILockable;
			return lockable != null;
		}

		private static bool IsSlotLocked(TEFeatureStorage storage, int index)
		{
			return StorageAccess.SlotLocked(storage, index);
		}

		private static void MarkModified(TileEntity tile, TEFeatureStorage storage)
		{
			storage?.SetModified();
			if (tile is TileEntityComposite composite)
			{
				composite.SetModified();
			}
		}

		internal struct Chest
		{
			public TileEntity Tile;
			public TEFeatureStorage Storage;
			public Vector3i Pos;
			public int Distance;
		}
	}

	internal static class PayCode
	{
		public const byte Paid = 1;
		public const byte Open = 2;
		public const byte Missing = 3;
		public const byte NoNeed = 4;
		public const byte Bad = 5;
	}
}
