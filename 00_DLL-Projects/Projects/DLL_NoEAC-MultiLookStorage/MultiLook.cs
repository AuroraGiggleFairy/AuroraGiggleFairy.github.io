using System.Collections.Generic;
using System.Threading;
using Audio;

namespace MultiLookStorage
{
	public static class MultiLookSync
	{
		static int discardDepth;
		static int applyDepth;

		public static bool ForceDiscard => Volatile.Read(ref discardDepth) > 0;

		public static bool ForceApply => Volatile.Read(ref applyDepth) > 0;

		public static void PushDiscard()
		{
			Interlocked.Increment(ref discardDepth);
		}

		public static void PopDiscard()
		{
			Interlocked.Decrement(ref discardDepth);
		}

		public static void PushApply()
		{
			Interlocked.Increment(ref applyDepth);
		}

		public static void PopApply()
		{
			Interlocked.Decrement(ref applyDepth);
		}
	}

	public static class MultiLookRules
	{
		public static bool IsPlayerStorage(TEFeatureStorage loot)
		{
			return StorageAccess.IsPlayerOwned(loot);
		}

		public static bool Same(ItemStack left, ItemStack right)
		{
			if (left == null || left.IsEmpty())
			{
				return right == null || right.IsEmpty();
			}

			return right != null && left.Equals(right);
		}

		public static bool SlotLocked(TEFeatureStorage storage, int slot)
		{
			PackedBoolArray locks = StorageAccess.SlotLocks(storage);
			return locks != null && slot >= 0 && slot < locks.Length && locks[slot];
		}

		public static bool CanPlace(ItemStack stack)
		{
			if (stack == null || stack.IsEmpty())
			{
				return true;
			}

			ItemClass itemClass = StackAccess.Value(stack).ItemClassOrMissing;
			return itemClass == null || itemClass.CanPlaceInContainer();
		}

		public static int MaxCount(ItemStack stack)
		{
			if (stack == null || stack.IsEmpty() || StackAccess.Value(stack) == null)
			{
				return 1;
			}

			ItemClass itemClass = StackAccess.Value(stack).ItemClassOrMissing;
			if (itemClass == null || itemClass.MaxCount < 1)
			{
				return 1;
			}

			return itemClass.MaxCount;
		}

		public static void ResolveSwap(ItemStack slot, ItemStack cursor, out ItemStack newSlot, out ItemStack newCursor)
		{
			ItemStack slotCopy = slot == null || slot.IsEmpty() ? ItemStack.Empty.Clone() : slot.Clone();
			ItemStack cursorCopy = cursor == null || cursor.IsEmpty() ? ItemStack.Empty.Clone() : cursor.Clone();
			bool splitPlace = false;
			ItemClass cursorClass = cursorCopy.IsEmpty() ? null : StackAccess.Value(cursorCopy).ItemClassOrMissing;
			int max = cursorClass == null ? 0 : (cursorClass.MaxCount < 1 ? 1 : cursorClass.MaxCount);
			if (cursorClass != null && !cursorCopy.IsEmpty() && slotCopy.IsEmpty() && max < StackAccess.Count(cursorCopy))
			{
				splitPlace = true;
			}

			if (!splitPlace && (slotCopy.IsEmpty() || cursorCopy.IsEmpty()))
			{
				newSlot = cursorCopy;
				newCursor = slotCopy;
				return;
			}

			if (!splitPlace && (slotCopy.IsEmpty() || !StackAccess.Value(slotCopy).ItemClassOrMissing.CanStack() || cursorClass == null || !cursorClass.CanStack()))
			{
				newSlot = cursorCopy;
				newCursor = slotCopy;
				return;
			}

			if (!cursorCopy.IsEmpty() && !slotCopy.IsEmpty() && StackAccess.Value(cursorCopy).type == StackAccess.Value(slotCopy).type && !StackAccess.Value(cursorCopy).HasQuality && !StackAccess.Value(slotCopy).HasQuality)
			{
				if (StackAccess.Count(cursorCopy) + StackAccess.Count(slotCopy) > max)
				{
					int overflow = StackAccess.Count(cursorCopy) + StackAccess.Count(slotCopy) - max;
					newSlot = slotCopy;
					StackAccess.SetCount(newSlot, max);
					newCursor = cursorCopy;
					StackAccess.SetCount(newCursor, overflow);
					return;
				}

				newSlot = slotCopy;
				StackAccess.SetCount(newSlot, StackAccess.Count(newSlot) + (StackAccess.Count(cursorCopy)));
				newCursor = ItemStack.Empty.Clone();
				return;
			}

			if (splitPlace)
			{
				int remain = StackAccess.Count(cursorCopy) - max;
				newSlot = cursorCopy.Clone();
				StackAccess.SetCount(newSlot, max);
				newCursor = cursorCopy;
				StackAccess.SetCount(newCursor, remain);
				return;
			}

			newSlot = cursorCopy;
			newCursor = slotCopy;
		}

		public static bool ResolveHalf(ItemStack slot, ItemStack cursor, out ItemStack newSlot, out ItemStack newCursor)
		{
			newSlot = slot == null ? ItemStack.Empty.Clone() : slot.Clone();
			newCursor = cursor == null ? ItemStack.Empty.Clone() : cursor.Clone();
			if (cursor != null && !cursor.IsEmpty())
			{
				return false;
			}

			if (slot == null || slot.IsEmpty())
			{
				return false;
			}

			int half = StackAccess.Count(slot) / 2;
			if (half <= 0)
			{
				return false;
			}

			newCursor = slot.Clone();
			StackAccess.SetCount(newCursor, half);
			newSlot = slot.Clone();
			StackAccess.SetCount(newSlot, StackAccess.Count(newSlot) - (half));
			return true;
		}

		public static bool ResolveDropOne(ItemStack slot, ItemStack cursor, out ItemStack newSlot, out ItemStack newCursor)
		{
			newSlot = slot == null ? ItemStack.Empty.Clone() : slot.Clone();
			newCursor = cursor == null ? ItemStack.Empty.Clone() : cursor.Clone();
			if (cursor == null || cursor.IsEmpty())
			{
				return false;
			}

			int max = MaxCount(slot != null && !slot.IsEmpty() ? slot : cursor);
			if (slot == null || slot.IsEmpty())
			{
				newSlot = cursor.Clone();
				StackAccess.SetCount(newSlot, 1);
				newCursor = cursor.Clone();
				StackAccess.SetCount(newCursor, StackAccess.Count(newCursor) - (1));
			}
			else if (StackAccess.Value(cursor).type == StackAccess.Value(slot).type && !StackAccess.Value(cursor).HasQuality && !StackAccess.Value(slot).HasQuality && StackAccess.Count(slot) + 1 <= max)
			{
				newSlot = slot.Clone();
				StackAccess.SetCount(newSlot, StackAccess.Count(newSlot) + (1));
				newCursor = cursor.Clone();
				StackAccess.SetCount(newCursor, StackAccess.Count(newCursor) - (1));
			}
			else
			{
				return false;
			}

			if (StackAccess.Count(newCursor) <= 0)
			{
				newCursor = ItemStack.Empty.Clone();
			}

			return true;
		}
	}

	public static class MultiLookServer
	{
		static readonly Dictionary<Vector3i, HashSet<int>> OpenByPos = new Dictionary<Vector3i, HashSet<int>>();

		public static void MarkOpen(Vector3i pos, int playerId)
		{
			if (!OpenByPos.TryGetValue(pos, out HashSet<int> players))
			{
				players = new HashSet<int>();
				OpenByPos[pos] = players;
			}

			players.Add(playerId);
		}

		public static void MarkClosed(Vector3i pos, int playerId)
		{
			if (!OpenByPos.TryGetValue(pos, out HashSet<int> players))
			{
				return;
			}

			players.Remove(playerId);
			if (players.Count == 0)
			{
				OpenByPos.Remove(pos);
			}
		}

		public static void Handle(World world, int playerId, NetPackageMultiLookRequest request)
		{
			if (world == null || request == null)
			{
				return;
			}

			if (request.Op == MultiLookOp.PutBack)
			{
				PutBack(world, request.Pos, request.PutBack);
				return;
			}

			TEFeatureStorage storage = GetStorage(world, request.Pos);
			if (storage == null || !StorageAccess.IsPlayerOwned(storage) || !IsOpen(request.Pos, playerId))
			{
				Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
				return;
			}

			switch (request.Op)
			{
				case MultiLookOp.Swap:
					HandleCursor(world, playerId, storage, request, MultiLookOp.Swap);
					break;
				case MultiLookOp.Half:
					HandleCursor(world, playerId, storage, request, MultiLookOp.Half);
					break;
				case MultiLookOp.DropOne:
					HandleCursor(world, playerId, storage, request, MultiLookOp.DropOne);
					break;
				case MultiLookOp.Take:
					HandleTake(world, playerId, storage, request);
					break;
				case MultiLookOp.TakeAll:
					HandleTakeAll(world, playerId, storage, request);
					break;
				case MultiLookOp.Sort:
					HandleSort(world, playerId, storage, request);
					break;
				case MultiLookOp.Deposit:
					HandleDeposit(world, playerId, storage, request);
					break;
				default:
					Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
					break;
			}
		}

		static void HandleCursor(World world, int playerId, TEFeatureStorage storage, NetPackageMultiLookRequest request, MultiLookOp op)
		{
			if (!TryGetSlot(storage, request.Slot, out ItemStack current) || !MultiLookRules.Same(current, request.Expected))
			{
				Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
				return;
			}

			ItemStack offered = request.Offered ?? ItemStack.Empty;
			bool resolved;
			ItemStack newSlot;
			ItemStack newCursor;
			if (op == MultiLookOp.Half)
			{
				resolved = MultiLookRules.ResolveHalf(current, offered, out newSlot, out newCursor);
			}
			else if (op == MultiLookOp.DropOne)
			{
				resolved = MultiLookRules.ResolveDropOne(current, offered, out newSlot, out newCursor);
			}
			else
			{
				if (!MultiLookRules.CanPlace(offered))
				{
					Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
					return;
				}

				MultiLookRules.ResolveSwap(current, offered, out newSlot, out newCursor);
				resolved = true;
			}

			if (!resolved || !MultiLookRules.CanPlace(newSlot))
			{
				Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
				return;
			}

			storage.UpdateSlot(request.Slot, newSlot);
			storage.SetModified();
			Reply(world, playerId, request, true, newCursor, null, null);
		}

		static void HandleTake(World world, int playerId, TEFeatureStorage storage, NetPackageMultiLookRequest request)
		{
			if (!TryGetSlot(storage, request.Slot, out ItemStack current) || current.IsEmpty() || MultiLookRules.SlotLocked(storage, request.Slot) || !MultiLookRules.Same(current, request.Expected))
			{
				Reply(world, playerId, request, false, ItemStack.Empty.Clone(), null, null);
				return;
			}

			ItemStack grant = current.Clone();
			storage.UpdateSlot(request.Slot, ItemStack.Empty.Clone());
			storage.SetModified();
			Reply(world, playerId, request, true, ItemStack.Empty.Clone(), new List<ItemStack> { grant }, null);
		}

		static void HandleTakeAll(World world, int playerId, TEFeatureStorage storage, NetPackageMultiLookRequest request)
		{
			List<ItemStack> grants = new List<ItemStack>();
			ItemStack[] items = StorageAccess.Items(storage);
			if (items != null)
			{
				for (int i = 0; i < items.Length; i++)
				{
					if (items[i] == null || items[i].IsEmpty() || MultiLookRules.SlotLocked(storage, i))
					{
						continue;
					}

					grants.Add(items[i].Clone());
					storage.UpdateSlot(i, ItemStack.Empty.Clone());
				}
			}

			if (grants.Count > 0)
			{
				storage.SetModified();
			}

			Reply(world, playerId, request, true, ItemStack.Empty.Clone(), grants, null);
		}

		static void HandleSort(World world, int playerId, TEFeatureStorage storage, NetPackageMultiLookRequest request)
		{
			ItemStack[] sorted = StackSortUtil.CombineAndSortStacks(StorageAccess.Items(storage), 0, StorageAccess.SlotLocks(storage));
			if (sorted != null)
			{
				int count = sorted.Length < StorageAccess.Items(storage).Length ? sorted.Length : StorageAccess.Items(storage).Length;
				for (int i = 0; i < count; i++)
				{
					storage.UpdateSlot(i, sorted[i] ?? ItemStack.Empty.Clone());
				}
			}

			storage.SetModified();
			Reply(world, playerId, request, true, ItemStack.Empty.Clone(), null, null);
		}

		static void HandleDeposit(World world, int playerId, TEFeatureStorage storage, NetPackageMultiLookRequest request)
		{
			List<DepositTaken> taken = new List<DepositTaken>();
			for (int i = 0; i < request.Deposit.Count; i++)
			{
				DepositStack row = request.Deposit[i];
				ItemStack stack = row.Stack == null ? ItemStack.Empty.Clone() : row.Stack.Clone();
				if (stack.IsEmpty() || !MultiLookRules.CanPlace(stack))
				{
					continue;
				}

				int before = StackAccess.Count(stack);
				storage.TryStackItem(0, stack);
				if (!stack.IsEmpty())
				{
					PlaceInEmpty(storage, stack);
				}

				int moved = before - StackAccess.Count(stack);
				if (moved > 0)
				{
					taken.Add(new DepositTaken
					{
						Slot = row.Slot,
						Type = StackAccess.Value(row.Stack).type,
						Count = moved
					});
				}
			}

			if (taken.Count > 0)
			{
				storage.SetModified();
			}

			Reply(world, playerId, request, true, ItemStack.Empty.Clone(), null, taken);
		}

		static void PutBack(World world, Vector3i pos, List<ItemStack> stacks)
		{
			if (stacks == null || stacks.Count == 0)
			{
				return;
			}

			TEFeatureStorage storage = GetStorage(world, pos);
			List<ItemStack> dropped = new List<ItemStack>();
			if (storage != null && StorageAccess.IsPlayerOwned(storage))
			{
				for (int i = 0; i < stacks.Count; i++)
				{
					ItemStack stack = stacks[i] == null ? ItemStack.Empty.Clone() : stacks[i].Clone();
					if (stack.IsEmpty())
					{
						continue;
					}

					storage.TryStackItem(0, stack);
					if (!stack.IsEmpty())
					{
						PlaceInEmpty(storage, stack);
					}

					if (!stack.IsEmpty())
					{
						dropped.Add(stack);
					}
				}

				storage.SetModified();
			}
			else
			{
				dropped.AddRange(stacks);
			}

			for (int i = 0; i < dropped.Count; i++)
			{
				if (dropped[i] == null || dropped[i].IsEmpty() || GameManager.Instance == null)
				{
					continue;
				}

				GameManager.Instance.ItemDropServer(dropped[i], pos.ToVector3() + new UnityEngine.Vector3(0.5f, 1f, 0.5f), UnityEngine.Vector3.zero);
			}
		}

		static void PlaceInEmpty(TEFeatureStorage storage, ItemStack stack)
		{
			ItemStack[] items = StorageAccess.Items(storage);
			if (items == null)
			{
				return;
			}

			for (int i = 0; i < items.Length; i++)
			{
				if (stack.IsEmpty())
				{
					return;
				}

				if (MultiLookRules.SlotLocked(storage, i) || items[i] == null || !items[i].IsEmpty())
				{
					continue;
				}

				storage.UpdateSlot(i, stack.Clone());
				StackAccess.SetCount(stack, 0);
				return;
			}
		}

		static bool TryGetSlot(TEFeatureStorage storage, int slot, out ItemStack stack)
		{
			stack = ItemStack.Empty;
			ItemStack[] items = StorageAccess.Items(storage);
			if (items == null || slot < 0 || slot >= items.Length || items[slot] == null)
			{
				return false;
			}

			stack = items[slot];
			return true;
		}

		static bool IsOpen(Vector3i pos, int playerId)
		{
			return OpenByPos.TryGetValue(pos, out HashSet<int> players) && players.Contains(playerId);
		}

		public static TEFeatureStorage GetStorage(World world, Vector3i pos)
		{
			TileEntity tile = world.GetTileEntity(pos);
			if (tile == null)
			{
				return null;
			}

			return tile.GetSelfOrFeature<TEFeatureStorage>();
		}

		static void Reply(World world, int playerId, NetPackageMultiLookRequest request, bool accepted, ItemStack cursor, List<ItemStack> grants, List<DepositTaken> taken)
		{
			NetPackageMultiLookResult result = PackageEmit.Take<NetPackageMultiLookResult>().Setup(request.Op, accepted, request.RequestId, cursor);
			if (grants != null)
			{
				result.Grants.AddRange(grants);
			}

			if (taken != null)
			{
				result.Taken.AddRange(taken);
			}

			Entity entity = world.GetEntity(playerId);
			if (entity is EntityPlayerLocal)
			{
				MultiLookClient.Apply(result);
				return;
			}

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager != null)
			{
				manager.SendPackage(result, false, playerId);
			}
		}
	}

	public static class MultiLookClient
	{
		static bool busy;
		static int nextRequestId = 1;
		static readonly Dictionary<int, XUiC_ItemStackGrid> DepositGrids = new Dictionary<int, XUiC_ItemStackGrid>();

		public static void ClearBusy()
		{
			busy = false;
			DepositGrids.Clear();
		}

		public static bool TryPlayerStorage(XUiC_ItemStack stack, out TEFeatureStorage storage)
		{
			storage = null;
			if (stack == null || stack.StackLocation != XUiC_ItemStack.StackLocationTypes.LootContainer)
			{
				return false;
			}

			storage = StorageAccess.LootOf(stack.xui);
			return storage != null && StorageAccess.IsPlayerOwned(storage);
		}

		public static void SubmitSwap(XUiC_ItemStack stack, MultiLookOp op)
		{
			if (!TryPlayerStorage(stack, out TEFeatureStorage storage))
			{
				return;
			}

			ItemStack cursor = stack.xui.DragAndDropWindow != null ? stack.xui.DragAndDropWindow.CurrentStack : ItemStack.Empty;
			if (op != MultiLookOp.Half && !MultiLookRules.CanPlace(cursor))
			{
				Deny(stack);
				return;
			}

			if (!cursor.IsEmpty() && !cursor.CanMoveTo(stack.StackLocation))
			{
				Deny(stack);
				return;
			}

			ItemStack expected = SlotOrEmpty(storage, stack.SlotNumber);
			Submit(storage.ToWorldPos(), op, stack.SlotNumber, 0, expected, cursor, null, null);
		}

		public static void SubmitTake(XUiC_ItemStack stack)
		{
			if (!TryPlayerStorage(stack, out TEFeatureStorage storage))
			{
				return;
			}

			if (stack.StackLock || stack.ItemStack == null || stack.ItemStack.IsEmpty() || MultiLookRules.SlotLocked(storage, stack.SlotNumber))
			{
				return;
			}

			Submit(storage.ToWorldPos(), MultiLookOp.Take, stack.SlotNumber, 0, stack.ItemStack.Clone(), ItemStack.Empty.Clone(), null, null);
		}

		public static void SubmitTakeAll(XUi xui)
		{
			TEFeatureStorage storage = StorageAccess.LootOf(xui);
			if (storage == null || !StorageAccess.IsPlayerOwned(storage))
			{
				return;
			}

			Submit(storage.ToWorldPos(), MultiLookOp.TakeAll, 0, 0, ItemStack.Empty.Clone(), ItemStack.Empty.Clone(), null, null);
		}

		public static void SubmitSort(XUi xui)
		{
			TEFeatureStorage storage = StorageAccess.LootOf(xui);
			if (storage == null || !StorageAccess.IsPlayerOwned(storage))
			{
				return;
			}

			Submit(storage.ToWorldPos(), MultiLookOp.Sort, 0, 0, ItemStack.Empty.Clone(), ItemStack.Empty.Clone(), null, null);
		}

		public static void SubmitDeposit(XUiC_ItemStackGrid grid, TEFeatureStorage storage)
		{
			if (grid == null || storage == null || !StorageAccess.IsPlayerOwned(storage))
			{
				return;
			}

			XUiC_ItemStack[] controllers = grid.GetItemStackControllers();
			if (controllers == null)
			{
				return;
			}

			List<DepositStack> rows = new List<DepositStack>();
			for (int i = 0; i < controllers.Length; i++)
			{
				XUiC_ItemStack controller = controllers[i];
				if (controller == null || controller.StackLock || controller.ItemStack == null || controller.ItemStack.IsEmpty())
				{
					continue;
				}

				if (!MultiLookRules.CanPlace(controller.ItemStack))
				{
					continue;
				}

				rows.Add(new DepositStack
				{
					Slot = (short)controller.SlotNumber,
					Stack = controller.ItemStack.Clone()
				});
			}

			if (rows.Count == 0)
			{
				return;
			}

			int requestId = nextRequestId++;
			DepositGrids[requestId] = grid;
			Submit(storage.ToWorldPos(), MultiLookOp.Deposit, 0, requestId, ItemStack.Empty.Clone(), ItemStack.Empty.Clone(), rows, null);
		}

		public static void Apply(NetPackageMultiLookResult result)
		{
			busy = false;
			if (result == null)
			{
				return;
			}

			LocalPlayerUI ui = LocalPlayerUI.GetUIForPrimaryPlayer();
			if (ui == null || ui.xui == null)
			{
				return;
			}

			if (!result.Accepted)
			{
				Manager.PlayInsidePlayerHead("ui_denied");
				DepositGrids.Remove(result.RequestId);
				return;
			}

			if (result.Op == MultiLookOp.Swap || result.Op == MultiLookOp.Half || result.Op == MultiLookOp.DropOne)
			{
				if (ui.xui.DragAndDropWindow != null)
				{
					StorageAccess.SetDragStack(ui.xui.DragAndDropWindow, result.Cursor == null ? ItemStack.Empty.Clone() : result.Cursor.Clone());
				}

				PlayStackSound(result.Cursor);
				return;
			}

			if (result.Op == MultiLookOp.Take || result.Op == MultiLookOp.TakeAll)
			{
				List<ItemStack> leftovers = new List<ItemStack>();
				TEFeatureStorage storage = StorageAccess.LootOf(ui.xui);
				for (int i = 0; i < result.Grants.Count; i++)
				{
					ItemStack grant = result.Grants[i] == null ? ItemStack.Empty.Clone() : result.Grants[i].Clone();
					if (grant.IsEmpty())
					{
						continue;
					}

					PlayStackSound(grant);
					if (!ui.xui.PlayerInventory.AddItem(grant) || !grant.IsEmpty())
					{
						if (!grant.IsEmpty())
						{
							leftovers.Add(grant.Clone());
						}
					}
				}

				if (leftovers.Count > 0 && storage != null)
				{
					SubmitPutBack(storage.ToWorldPos(), leftovers);
				}

				return;
			}

			if (result.Op == MultiLookOp.Deposit)
			{
				ApplyDeposit(ui, result);
			}
		}

		static void ApplyDeposit(LocalPlayerUI ui, NetPackageMultiLookResult result)
		{
			DepositGrids.TryGetValue(result.RequestId, out XUiC_ItemStackGrid grid);
			DepositGrids.Remove(result.RequestId);
			XUiC_ItemStack[] controllers = grid != null ? grid.GetItemStackControllers() : null;
			for (int i = 0; i < result.Taken.Count; i++)
			{
				DepositTaken taken = result.Taken[i];
				if (taken.Count <= 0)
				{
					continue;
				}

				XUiC_ItemStack controller = FindController(controllers, taken.Slot);
				if (controller != null && controller.ItemStack != null && !controller.ItemStack.IsEmpty() && StackAccess.Value(controller.ItemStack).type == taken.Type)
				{
					ItemStack updated = controller.ItemStack.Clone();
					StackAccess.SetCount(updated, StackAccess.Count(updated) - (taken.Count));
					controller.ItemStack = StackAccess.Count(updated) > 0 ? updated : ItemStack.Empty.Clone();
					continue;
				}

				ui.xui.PlayerInventory.RemoveItem(new ItemStack(new ItemValue(taken.Type), taken.Count));
			}
		}

		static XUiC_ItemStack FindController(XUiC_ItemStack[] controllers, int slot)
		{
			if (controllers == null)
			{
				return null;
			}

			for (int i = 0; i < controllers.Length; i++)
			{
				if (controllers[i] != null && controllers[i].SlotNumber == slot)
				{
					return controllers[i];
				}
			}

			return null;
		}

		static void SubmitPutBack(Vector3i pos, List<ItemStack> leftovers)
		{
			NetPackageMultiLookRequest package = PackageEmit.Take<NetPackageMultiLookRequest>().Setup(MultiLookOp.PutBack, pos, 0, 0, ItemStack.Empty.Clone(), ItemStack.Empty.Clone());
			package.PutBack.AddRange(leftovers);
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager != null && manager.IsServer)
			{
				MultiLookServer.Handle(GameManager.Instance.World, GameManager.Instance.World.GetPrimaryPlayerId(), package);
				return;
			}

			manager?.SendToServer(package);
		}

		static void Submit(Vector3i pos, MultiLookOp op, int slot, int requestId, ItemStack expected, ItemStack offered, List<DepositStack> deposit, List<ItemStack> putBack)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || GameManager.Instance == null || GameManager.Instance.World == null)
			{
				return;
			}

			if (!manager.IsServer && busy)
			{
				if (op == MultiLookOp.Deposit)
				{
					DepositGrids.Remove(requestId);
				}

				return;
			}

			NetPackageMultiLookRequest package = PackageEmit.Take<NetPackageMultiLookRequest>().Setup(op, pos, slot, requestId, expected, offered);
			if (deposit != null)
			{
				package.Deposit.AddRange(deposit);
			}

			if (putBack != null)
			{
				package.PutBack.AddRange(putBack);
			}

			if (manager.IsServer)
			{
				MultiLookServer.Handle(GameManager.Instance.World, GameManager.Instance.World.GetPrimaryPlayerId(), package);
				return;
			}

			busy = true;
			manager.SendToServer(package);
		}

		static ItemStack SlotOrEmpty(TEFeatureStorage storage, int slot)
		{
			ItemStack[] items = StorageAccess.Items(storage);
			if (items == null || slot < 0 || slot >= items.Length || items[slot] == null)
			{
				return ItemStack.Empty.Clone();
			}

			return items[slot].Clone();
		}

		static void Deny(XUiC_ItemStack stack)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			if (stack?.xui?.playerUI?.entityPlayer != null)
			{
				GameManager.ShowTooltip(stack.xui.playerUI.entityPlayer, "Quest Items cannot be placed in containers.");
			}
		}

		static void PlayStackSound(ItemStack stack)
		{
			string sound = StackAccess.Value(stack)?.ItemClass?.SoundPlace;
			if (!string.IsNullOrEmpty(sound))
			{
				Manager.PlayInsidePlayerHead(sound);
			}
		}
	}
}
