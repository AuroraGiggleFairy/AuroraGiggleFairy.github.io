using System;

namespace SortingCart
{
	internal static class AppearanceSwap
	{
		private const string FullSuffix = "Full";

		public static void Sync(World world, Vector3i boxPos, bool boxEmpty)
		{
			if (world == null)
			{
				return;
			}

			BlockValue current = world.GetBlock(boxPos);
			Block block = current.Block;
			if (block == null || !StorageUtil.IsSortingCart(block))
			{
				return;
			}

			string name = block.GetBlockName();
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			bool isFull = name.EndsWith(FullSuffix, StringComparison.Ordinal);
			if (boxEmpty == !isFull)
			{
				return;
			}

			string targetName = boxEmpty
				? name.Substring(0, name.Length - FullSuffix.Length)
				: name + FullSuffix;
			if (string.IsNullOrEmpty(targetName))
			{
				return;
			}

			BlockValue next = Block.GetBlockValue(targetName);
			if (next.isair || next.type == current.type || !StorageUtil.IsSortingCart(next.Block))
			{
				return;
			}

			next.rotation = current.rotation;
			next.meta = current.meta;
			next.damage = current.damage;
			world.SetBlockRPC(new BlockValueRef(boxPos), next);
		}
	}
}
