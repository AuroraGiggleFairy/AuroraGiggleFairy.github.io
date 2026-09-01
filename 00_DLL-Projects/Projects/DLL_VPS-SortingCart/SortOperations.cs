using System.Collections.Generic;
using UnityEngine;

namespace SortingCart
{
	internal static class SortOperations
	{
		public static int SortFromBox(Vector3i boxPos, int playerId = -1)
		{
			World world = GameManager.Instance.World;
			TileEntity sourceTe = world.GetTileEntity(boxPos);
			if (sourceTe == null
				|| !StorageUtil.IsSortingCart(sourceTe.blockValue.Block)
				|| !StorageUtil.TryAsContainer(sourceTe, out ITileEntityLootable source)
				|| StorageUtil.IsInUse(sourceTe))
			{
				return 0;
			}

			if (!NearbyScanner.TryGetBounds(boxPos, out Vector3i min, out Vector3i max, out List<Vector3i> claims))
			{
				return 0;
			}

			List<NearbyScanner.ChestTarget> chests = NearbyScanner.FindPlayerChests(world, boxPos, min, max, claims);
			int moved = 0;
			foreach (NearbyScanner.ChestTarget chest in chests)
			{
				if (!StorageUtil.BoxCanAccessTarget(sourceTe, chest.TileEntity))
				{
					continue;
				}

				moved += ItemMover.MoveLikeItems(source, chest.Storage, true);
				StorageUtil.MarkModified(chest.TileEntity, chest.Storage);
			}

			if (moved > 0)
			{
				StorageUtil.MarkModified(sourceTe, source);
			}

			bool boxEmpty = source.IsEmpty();
			NotifyCloseSort(world, boxPos, playerId, moved, boxEmpty);
			AppearanceSwap.Sync(world, boxPos, boxEmpty);
			return moved;
		}

		private static void NotifyCloseSort(World world, Vector3i boxPos, int playerId, int moved, bool boxEmpty)
		{
			if (world == null)
			{
				return;
			}

			string key = moved <= 0
				? "agfSortingCartNothingSorted"
				: boxEmpty
					? "agfSortingCartAllSorted"
					: "agfSortingCartSomeSorted";
			string text = Localization.Get(key);
			if (string.IsNullOrEmpty(text) || text == key)
			{
				text = moved <= 0 ? "Nothing sorted" : boxEmpty ? "All sorted" : "Some sorted";
			}

			if (world.GetEntity(playerId) is EntityPlayer player)
			{
				GameManager.ShowTooltipMP(player, text);
			}

			if (moved > 0)
			{
				GameManager.Instance.PlaySoundAtPositionServer(
					boxPos.ToVector3() + Vector3.one * 0.5f,
					"vehicle_storage_close",
					AudioRolloffMode.Logarithmic,
					5);
			}
		}
	}
}
