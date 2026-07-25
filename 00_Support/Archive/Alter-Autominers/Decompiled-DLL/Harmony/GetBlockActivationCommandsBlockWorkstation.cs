using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(BlockWorkstation))]
[HarmonyPatch("GetBlockActivationCommands")]
public class GetBlockActivationCommandsBlockWorkstation
{
	public static bool Prefix(ref BlockActivationCommand[] __result, BlockActivationCommand[] ___cmds, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.Block.Tags.Test_AnySet(FastTags<TagGroup.Global>.Parse("miningMachine")))
		{
			bool flag = _blockValue.Block.Properties.GetBool("CanPickup");
			TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_clrIdx, _blockPos);
			bool flag2 = false;
			if (tileEntityWorkstation != null)
			{
				flag2 = tileEntityWorkstation.IsPlayerPlaced;
			}
			___cmds[1].enabled = flag && flag2;
			__result = ___cmds;
			return false;
		}
		return true;
	}
}
