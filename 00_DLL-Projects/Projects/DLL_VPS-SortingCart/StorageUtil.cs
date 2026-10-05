namespace SortingCart
{
	internal static class StorageUtil
	{
		private static readonly FastTags<TagGroup.Global> CartTag = FastTags<TagGroup.Global>.Parse("sortingcart");

		public static bool IsServer()
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			return manager != null && manager.IsServer;
		}

		public static bool IsSortingCart(Block block)
		{
			return block != null && block.Tags.Test_AnySet(CartTag);
		}

		public static bool TryAsContainer(TileEntity entity, out StorageBox storage)
		{
			storage = null;
			if (entity == null)
			{
				return false;
			}

			if (entity is TileEntityComposite composite)
			{
				storage = StorageBox.FromFeature(composite.GetFeature<TEFeatureStorage>());
				if (storage != null)
				{
					return true;
				}
			}

			if (GameVersion.UseV33)
			{
				return false;
			}

			storage = StorageBox.TryLegacy(entity);
			return storage != null;
		}

		public static bool TryAsLock(TileEntity entity, out ILockable lockable)
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

		public static bool IsBagSlotLocked(Bag bag, int index)
		{
			PackedBoolArray locks = BagAccess.LockedSlots(bag);
			if (locks == null)
			{
				return false;
			}

			return index >= 0 && index < locks.Length && locks[index];
		}

		public static bool IsInUse(TileEntity tileEntity)
		{
			if (tileEntity == null || LockManager.Instance == null)
			{
				return false;
			}

			if (LockManager.Instance.IsLockedServer(tileEntity))
			{
				return true;
			}

			return TryAsContainer(tileEntity, out StorageBox storage)
				&& storage.AsLockTarget() is ILockTarget lockTarget
				&& LockManager.Instance.IsLockedServer(lockTarget);
		}

		public static PlatformUserIdentifierAbs GetPlayerId(EntityPlayer player)
		{
			if (player == null)
			{
				return null;
			}

			return GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId)?.PrimaryId;
		}

		public static bool CanPlayerOpenLock(ILockable lockable, PlatformUserIdentifierAbs playerId)
		{
			if (lockable == null || !lockable.IsLocked())
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

		public static bool BoxCanAccessTarget(TileEntity source, TileEntity target)
		{
			if (!TryAsLock(target, out ILockable targetLock) || !targetLock.IsLocked())
			{
				return true;
			}

			if (!targetLock.HasPassword())
			{
				return false;
			}

			if (!TryAsLock(source, out ILockable sourceLock) || !sourceLock.IsLocked())
			{
				return false;
			}

			string sourceHash = sourceLock.GetPasswordHash();
			string targetHash = targetLock.GetPasswordHash();
			return !string.IsNullOrEmpty(sourceHash) && sourceHash.Equals(targetHash);
		}

		public static void MarkModified(TileEntity tileEntity, StorageBox storage)
		{
			storage?.SetModified();
			if (tileEntity is TileEntityComposite composite)
			{
				composite.SetModified();
			}
		}

		public static bool CanRunSortCommands(EntityPlayer player)
		{
			return player is EntityPlayerLocal && player.bag != null;
		}

		public static Bag GetPlayerBag(EntityPlayer player)
		{
			return CanRunSortCommands(player) ? player.bag : null;
		}
	}
}
