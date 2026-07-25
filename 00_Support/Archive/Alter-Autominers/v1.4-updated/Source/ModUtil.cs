using System.Collections.Generic;
using UnityEngine;

namespace Autominers
{
	public static class ModUtil
	{
		public static Vector3i GetOffset(BlockValue blockValue)
		{
			Vector3i offset = Vector3i.zero;
			if (blockValue.Block.Properties.Values.ContainsKey("BlockCheckOffset"))
			{
				offset = Vector3i.Parse(blockValue.Block.Properties.Values["BlockCheckOffset"]);
				Vector3 rotated = Quaternion.Euler(0f, 90 * blockValue.rotation, 0f) * offset.ToVector3();
				offset = new Vector3i(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y), Mathf.RoundToInt(rotated.z));
			}
			return offset;
		}

		public static bool IsAutominerWorkstation(Block block)
		{
			return block != null && block.Properties.Contains("Workstation", "Autominer");
		}

		public static bool IsAutominerCraftingArea(string craftingArea)
		{
			return craftingArea == "oilPumpjack" || craftingArea == "miningMachine";
		}

		public static List<string> GetMiningSourceBlocks(Block block)
		{
			List<string> result = new List<string>();
			DynamicProperties miningData = block?.Properties.GetClass("MiningData");
			if (miningData?.Values == null)
			{
				return result;
			}

			foreach (string key in miningData.Values.Keys)
			{
				if (!string.IsNullOrEmpty(key))
				{
					result.Add(key);
				}
			}
			return result;
		}

		public static bool TryGetMiningOutputs(Block block, string sourceBlockName, out string outputsCsv)
		{
			outputsCsv = null;
			if (block == null || string.IsNullOrEmpty(sourceBlockName))
			{
				return false;
			}

			if (!block.Properties.Contains("MiningData", sourceBlockName))
			{
				return false;
			}

			outputsCsv = block.Properties.GetString("MiningData", sourceBlockName);
			return !string.IsNullOrEmpty(outputsCsv);
		}
	}
}
