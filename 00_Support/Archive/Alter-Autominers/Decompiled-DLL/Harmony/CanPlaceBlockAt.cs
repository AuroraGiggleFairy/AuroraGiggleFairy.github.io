using System.Collections.Generic;
using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(Block))]
[HarmonyPatch("CanPlaceBlockAt")]
public class CanPlaceBlockAt
{
	public static bool Prefix(ref bool __result, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.Block.Properties.Contains("Workstation.Autominer"))
		{
			List<string> blockNames = new List<string>();
			foreach (string s in _blockValue.Block.Properties.Values.Dict.Keys)
			{
				if (s.Contains("MiningData") && !string.IsNullOrEmpty(s.Replace("MiningData.", "")))
				{
					blockNames.Add(s.Replace("MiningData.", ""));
				}
			}
			string blockName = _world.GetBlock(_clrIdx, _blockPos + ModUtil.getOffset(_blockValue)).Block.GetBlockName();
			if (!blockNames.ContainsCaseInsensitive(blockName))
			{
				__result = false;
				return false;
			}
		}
		return true;
	}
}
