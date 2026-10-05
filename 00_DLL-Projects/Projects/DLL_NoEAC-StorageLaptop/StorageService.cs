using System;
using System.Collections.Generic;
using UnityEngine;

namespace StorageLaptop
{
	public sealed class StorageRow
	{
		public string ItemName;
		public int Quality;
		public int Count;
		public bool Locked;
		public bool HasDurability;
		public float DurabilityFill;
		public Vector3i ChestPos;
		public string ChestBlock;
		public int Distance;
	}

	public static class StorageService
	{
		public const byte StatusOk = 0;
		public const byte StatusBagFull = 1;
		public const byte StatusNoPower = 2;
		public const byte StatusInUse = 3;

		private static readonly Dictionary<Vector3i, int> occupants = new Dictionary<Vector3i, int>();

		private const int YMin = 0;
		private const int YMax = 253;
		private static readonly FastTags<TagGroup.Global> CartTag = FastTags<TagGroup.Global>.Parse("sortingcart");

		private struct SlotRef
		{
			public TileEntity Tile;
			public TEFeatureStorage Storage;
			public int Slot;
			public Vector3i Pos;
			public int Distance;
			public bool Locked;
			public ItemStack Stack;
		}

		public static bool IsServer()
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			return manager != null && manager.IsServer;
		}

		public static bool TryClaim(World world, Vector3i pos, int entityId)
		{
			if (entityId < 0)
			{
				return false;
			}

			if (occupants.TryGetValue(pos, out int current) && current != entityId && IsOnline(world, current))
			{
				return false;
			}

			occupants[pos] = entityId;
			SetBusy(world, pos, true);
			return true;
		}

		public static bool IsOccupant(World world, Vector3i pos, int entityId)
		{
			if (!occupants.TryGetValue(pos, out int current))
			{
				return false;
			}

			if (!IsOnline(world, current))
			{
				occupants.Remove(pos);
				SetBusy(world, pos, false);
				return false;
			}

			return current == entityId;
		}

		public static void Release(World world, Vector3i pos, int entityId)
		{
			if (!occupants.TryGetValue(pos, out int current) || current != entityId)
			{
				return;
			}

			occupants.Remove(pos);
			SetBusy(world, pos, false);
		}

		public static void ReleasePlayer(int entityId)
		{
			if (entityId < 0)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			List<Vector3i> drop = new List<Vector3i>();
			foreach (KeyValuePair<Vector3i, int> pair in occupants)
			{
				if (pair.Value == entityId)
				{
					drop.Add(pair.Key);
				}
			}

			for (int i = 0; i < drop.Count; i++)
			{
				occupants.Remove(drop[i]);
				SetBusy(world, drop[i], false);
			}
		}

		private static bool IsOnline(World world, int entityId)
		{
			return world != null && entityId >= 0 && world.GetEntity(entityId) != null;
		}

		private static void SetBusy(World world, Vector3i pos, bool busy)
		{
			if (world?.GetTileEntity(pos) is TileEntity tile)
			{
				tile.SetUserAccessing(busy);
			}
		}

		public static List<StorageRow> Scan(World world, Vector3i origin, EntityPlayer player)
		{
			var rows = new List<StorageRow>();
			if (world == null || player == null || !TryGetBounds(origin, out Vector3i min, out Vector3i max, out List<Vector3i> claims))
			{
				return rows;
			}

			List<SlotRef> slots = Collect(world, origin, min, max, claims, player);
			Console.WriteLine("[StorageLaptop] Scan slots=" + slots.Count);
			var groups = new Dictionary<string, StorageRow>();
			foreach (SlotRef slot in slots)
			{
				ItemStack stack = slot.Stack;
				if (StackAccess.Value(stack)?.ItemClass == null)
				{
					continue;
				}

				string name = StackAccess.Value(stack).ItemClass.GetItemName();
				int quality = StackAccess.Value(stack).Quality;
				ItemClass itemClass = StackAccess.Value(stack).ItemClass;
				bool showBar = itemClass != null && itemClass.ShowQualityBar;
				float durability = 1f;
				if (showBar && StackAccess.Value(stack).MaxUseTimesUI > 0)
				{
					durability = Mathf.Clamp01((StackAccess.Value(stack).MaxUseTimes - StackAccess.Value(stack).UseTimes) / StackAccess.Value(stack).MaxUseTimesUI);
				}
				string key = slot.Locked
					? name + "|" + quality + "|L|" + slot.Pos.x + "," + slot.Pos.y + "," + slot.Pos.z
					: name + "|" + quality + "|U";
				if (!groups.TryGetValue(key, out StorageRow row))
				{
					row = new StorageRow
					{
						ItemName = name,
						Quality = quality,
						Locked = slot.Locked,
						HasDurability = showBar,
						DurabilityFill = durability,
						ChestPos = slot.Locked ? slot.Pos : Vector3i.zero,
						ChestBlock = slot.Locked ? slot.Tile.blockValue.Block.GetBlockName() : string.Empty,
						Distance = slot.Distance
					};
					groups[key] = row;
					rows.Add(row);
				}

				if (showBar)
				{
					if (!row.HasDurability || durability < row.DurabilityFill)
					{
						row.DurabilityFill = durability;
					}

					row.HasDurability = true;
				}

				row.Count += StackAccess.Count(stack);
				if (slot.Distance < row.Distance)
				{
					row.Distance = slot.Distance;
				}
			}

			return rows;
		}

		public static byte Pull(World world, Vector3i origin, EntityPlayer player, string itemName, int quality, bool locked, Vector3i chestPos, bool takeAll, int amount)
		{
			if (world == null || player?.bag == null || string.IsNullOrEmpty(itemName))
			{
				return StatusOk;
			}

			if (!BlockStorageLaptop.IsPoweredAt(world, origin))
			{
				return StatusNoPower;
			}

			if (!TryGetBounds(origin, out Vector3i min, out Vector3i max, out List<Vector3i> claims))
			{
				return StatusOk;
			}

			int want = takeAll && !locked ? int.MaxValue : Math.Max(1, amount);
			List<SlotRef> slots = Collect(world, origin, min, max, claims, player);
			slots.Sort((a, b) => a.Distance.CompareTo(b.Distance));
			int moved = 0;
			bool hadMatch = false;
			foreach (SlotRef slot in slots)
			{
				if (want <= 0)
				{
					break;
				}

				if (slot.Locked != locked)
				{
					continue;
				}

				if (locked && slot.Pos != chestPos)
				{
					continue;
				}

				ItemStack stack = slot.Stack;
				if (StackAccess.Value(stack)?.ItemClass == null || StackAccess.Count(stack) <= 0)
				{
					continue;
				}

				if (!string.Equals(StackAccess.Value(stack).ItemClass.GetItemName(), itemName, StringComparison.Ordinal) || StackAccess.Value(stack).Quality != quality)
				{
					continue;
				}

				hadMatch = true;
				int take = Math.Min(want, StackAccess.Count(stack));
				int gave = GiveToBag(player, StackAccess.Value(stack), take);
				if (gave <= 0)
				{
					break;
				}

				StackAccess.SetCount(stack, StackAccess.Count(stack) - (gave));
				if (StackAccess.Count(stack) <= 0)
				{
					StorageAccess.Items(slot.Storage)[slot.Slot] = ItemStack.Empty.Clone();
				}

				slot.Storage.UpdateSlot(slot.Slot, StorageAccess.Items(slot.Storage)[slot.Slot]);
				Mark(slot.Tile, slot.Storage);
				moved += gave;
				want -= gave;
			}

			if (moved <= 0 && hadMatch)
			{
				return StatusBagFull;
			}

			return StatusOk;
		}

		public static byte Consume(World world, Vector3i origin, EntityPlayer player, string itemName, int quality, bool locked, Vector3i chestPos, int amount)
		{
			if (world == null || player == null || string.IsNullOrEmpty(itemName))
			{
				return StatusOk;
			}

			if (!BlockStorageLaptop.IsPoweredAt(world, origin))
			{
				return StatusNoPower;
			}

			if (!TryGetBounds(origin, out Vector3i min, out Vector3i max, out List<Vector3i> claims))
			{
				return StatusOk;
			}

			int want = Math.Max(1, amount);
			List<SlotRef> slots = Collect(world, origin, min, max, claims, player);
			slots.Sort((a, b) => a.Distance.CompareTo(b.Distance));
			foreach (SlotRef slot in slots)
			{
				if (want <= 0)
				{
					break;
				}

				if (slot.Locked != locked || (locked && slot.Pos != chestPos))
				{
					continue;
				}

				ItemStack stack = slot.Stack;
				if (StackAccess.Value(stack)?.ItemClass == null || StackAccess.Count(stack) <= 0)
				{
					continue;
				}

				if (!string.Equals(StackAccess.Value(stack).ItemClass.GetItemName(), itemName, StringComparison.Ordinal) || StackAccess.Value(stack).Quality != quality)
				{
					continue;
				}

				int take = Math.Min(want, StackAccess.Count(stack));
				StackAccess.SetCount(stack, StackAccess.Count(stack) - take);
				if (StackAccess.Count(stack) <= 0)
				{
					StorageAccess.Items(slot.Storage)[slot.Slot] = ItemStack.Empty.Clone();
				}

				slot.Storage.UpdateSlot(slot.Slot, StorageAccess.Items(slot.Storage)[slot.Slot]);
				Mark(slot.Tile, slot.Storage);
				want -= take;
			}

			return StatusOk;
		}

		private static int GiveToBag(EntityPlayer player, ItemValue sourceValue, int count)
		{
			if (count <= 0 || sourceValue == null)
			{
				return 0;
			}

			ItemStack[] slots = StorageAccess.BagSlots(player.bag);
			if (slots == null)
			{
				return 0;
			}

			ItemValue iv = sourceValue.Clone();
			int max = iv.ItemClass != null ? iv.ItemClass.Stacknumber.Value : 1;
			if (max < 1)
			{
				max = 1;
			}

			int moved = 0;
			int want = count;
			for (int i = 0; i < slots.Length && want > 0; i++)
			{
				if (IsBagLocked(player.bag, i) || slots[i] == null || slots[i].IsEmpty())
				{
					continue;
				}

				if (StackAccess.Value(slots[i]).type != iv.type || StackAccess.Value(slots[i]).Quality != iv.Quality)
				{
					continue;
				}

				int space = max - StackAccess.Count(slots[i]);
				if (space <= 0)
				{
					continue;
				}

				int add = Math.Min(space, want);
				StackAccess.SetCount(slots[i], StackAccess.Count(slots[i]) + (add));
				want -= add;
				moved += add;
			}

			for (int i = 0; i < slots.Length && want > 0; i++)
			{
				if (IsBagLocked(player.bag, i) || (slots[i] != null && !slots[i].IsEmpty()))
				{
					continue;
				}

				int add = Math.Min(max, want);
				slots[i] = new ItemStack(iv.Clone(), add);
				want -= add;
				moved += add;
			}

			if (moved > 0)
			{
				StorageAccess.SetBagSlots(player.bag, slots);
			}

			return moved;
		}

		private static List<SlotRef> Collect(World world, Vector3i origin, Vector3i min, Vector3i max, List<Vector3i> claims, EntityPlayer player)
		{
			var results = new List<SlotRef>();
			int radius = GetLandClaimRadius();
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

					foreach (KeyValuePair<Vector3i, TileEntity> kvp in tes.dict)
					{
						TileEntity te = kvp.Value;
						if (te == null)
						{
							continue;
						}

						Vector3i pos = te.ToWorldPos();
						if (!InBounds(pos, min, max))
						{
							continue;
						}

						if (claims != null && claims.Count > 0 && !InAnyClaim(pos, claims, radius))
						{
							continue;
						}

						if (!TryAsContainer(te, out TEFeatureStorage storage) || StorageAccess.Items(storage) == null || !IsPlayerContainer(storage))
						{
							continue;
						}

						string blockName = te.blockValue.Block != null ? te.blockValue.Block.GetBlockName() : string.Empty;
						if (!string.IsNullOrEmpty(blockName) && blockName.IndexOf("backpack", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							continue;
						}

						if (te.blockValue.Block != null && te.blockValue.Block.Tags.Test_AnySet(CartTag))
						{
							continue;
						}

						if (IsInUse(te) || !CanOpen(te, player))
						{
							continue;
						}

						int distance = Chebyshev(origin, pos);
						for (int i = 0; i < StorageAccess.Items(storage).Length; i++)
						{
							ItemStack stack = StorageAccess.Items(storage)[i];
							if (stack == null || stack.IsEmpty() || ItemStack.Empty.Equals(stack))
							{
								continue;
							}

							results.Add(new SlotRef
							{
								Tile = te,
								Storage = storage,
								Slot = i,
								Pos = pos,
								Distance = distance,
								Locked = IsSlotLocked(storage, i),
								Stack = stack
							});
						}
					}
				}
			}

			return results;
		}

		private static bool TryGetBounds(Vector3i origin, out Vector3i min, out Vector3i max, out List<Vector3i> containingClaims)
		{
			min = max = default;
			containingClaims = new List<Vector3i>();
			World world = GameManager.Instance?.World;
			if (world == null || !world.GetWorldExtent(out Vector3i worldMin, out Vector3i worldMax))
			{
				return false;
			}

			int radius = GetLandClaimRadius();
			CollectContainingClaims(origin, radius, containingClaims);
			if (containingClaims.Count == 0)
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
				foreach (Vector3i lcb in containingClaims)
				{
					min.x = Mathf.Min(min.x, lcb.x - radius);
					max.x = Mathf.Max(max.x, lcb.x + radius);
					min.z = Mathf.Min(min.z, lcb.z - radius);
					max.z = Mathf.Max(max.z, lcb.z + radius);
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

		private static void Mark(TileEntity tileEntity, TEFeatureStorage storage)
		{
			storage?.SetModified();
			if (tileEntity is TileEntityComposite composite)
			{
				composite.SetModified();
			}
		}

		private static bool IsSlotLocked(TEFeatureStorage storage, int index)
		{
			return StorageAccess.SlotLocked(storage, index);
		}

		private static bool IsBagLocked(Bag bag, int index)
		{
			return StorageAccess.BagSlotLocked(bag, index);
		}

		private static bool IsPlayerContainer(TEFeatureStorage storage)
		{
			if (storage == null)
			{
				return false;
			}

			if (StorageAccess.IsPlayerOwned(storage))
			{
				return true;
			}

			TEFeatureStorage feature = storage as TEFeatureStorage;
			string lootList = feature != null ? feature.lootListName : null;
			return !string.IsNullOrEmpty(lootList) && lootList.StartsWith("player", StringComparison.OrdinalIgnoreCase);
		}

		private static bool TryAsContainer(TileEntity entity, out TEFeatureStorage storage)
		{
			storage = null;
			if (entity == null)
			{
				return false;
			}

			if (entity is TileEntityComposite composite)
			{
				storage = composite.GetFeature<TEFeatureStorage>();
				return storage != null;
			}

			if (typeof(TEFeatureStorage).IsInstanceOfType(entity))
			{
				storage = (TEFeatureStorage)(object)entity;
				return true;
			}

			return false;
		}

		private static bool CanOpen(TileEntity entity, EntityPlayer player)
		{
			if (!TryAsLock(entity, out ILockable lockable) || lockable == null || !lockable.IsLocked())
			{
				return true;
			}

			ResolvePlayerIds(player, out PlatformUserIdentifierAbs primary, out PlatformUserIdentifierAbs internalId);
			return IsAllowed(lockable, primary) || IsAllowed(lockable, internalId);
		}

		private static bool IsAllowed(ILockable lockable, PlatformUserIdentifierAbs playerId)
		{
			return playerId != null && (lockable.IsUserAllowed(playerId) || lockable.IsOwner(playerId));
		}

		private static void ResolvePlayerIds(EntityPlayer player, out PlatformUserIdentifierAbs primary, out PlatformUserIdentifierAbs internalId)
		{
			primary = null;
			internalId = null;
			if (player == null)
			{
				return;
			}

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager != null && manager.IsServer && manager.Clients != null)
			{
				ClientInfo info = manager.Clients.ForEntityId(player.entityId);
				if (info != null)
				{
					internalId = info.InternalId;
				}
			}

			primary = GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId)?.PrimaryId;
		}

		private static bool TryAsLock(TileEntity entity, out ILockable lockable)
		{
			lockable = null;
			if (entity == null)
			{
				return false;
			}

			if (entity is TileEntityComposite composite)
			{
				lockable = composite.GetFeature<TEFeatureLockable>();
				return lockable != null;
			}

			lockable = entity as ILockable;
			return lockable != null;
		}

		private static bool IsInUse(TileEntity tileEntity)
		{
			if (tileEntity == null || LockManager.Instance == null)
			{
				return false;
			}

			if (LockManager.Instance.IsLockedServer(tileEntity))
			{
				return true;
			}

			return TryAsContainer(tileEntity, out TEFeatureStorage storage)
				&& storage is ILockTarget lockTarget
				&& LockManager.Instance.IsLockedServer(lockTarget);
		}

		private static int GetLandClaimRadius()
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

		private static void CollectContainingClaims(Vector3i sourcePos, int radius, List<Vector3i> claims)
		{
			PersistentPlayerList players = GameManager.Instance?.persistentPlayers;
			World world = GameManager.Instance?.World;
			if (players?.Players == null || world == null)
			{
				return;
			}

			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> kvp in players.Players)
			{
				if (kvp.Value == null || !world.IsLandProtectionValidForPlayer(kvp.Value))
				{
					continue;
				}

				var blocks = kvp.Value.GetLandProtectionBlocks();
				if (blocks == null)
				{
					continue;
				}

				foreach (Vector3i pos in blocks)
				{
					if (InClaim(sourcePos, pos, radius))
					{
						claims.Add(pos);
					}
				}
			}
		}

		private static bool InAnyClaim(Vector3i pos, List<Vector3i> claims, int radius)
		{
			foreach (Vector3i lcb in claims)
			{
				if (InClaim(pos, lcb, radius))
				{
					return true;
				}
			}

			return false;
		}

		private static bool InClaim(Vector3i pos, Vector3i lcb, int radius)
		{
			return pos.x >= lcb.x - radius && pos.x <= lcb.x + radius && pos.z >= lcb.z - radius && pos.z <= lcb.z + radius;
		}

		private static bool InBounds(Vector3i pos, Vector3i min, Vector3i max)
		{
			return pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y && pos.z >= min.z && pos.z <= max.z;
		}

		private static int Chebyshev(Vector3i a, Vector3i b)
		{
			int dx = a.x > b.x ? a.x - b.x : b.x - a.x;
			int dy = a.y > b.y ? a.y - b.y : b.y - a.y;
			int dz = a.z > b.z ? a.z - b.z : b.z - a.z;
			int m = dx > dy ? dx : dy;
			return m > dz ? m : dz;
		}
	}
}
