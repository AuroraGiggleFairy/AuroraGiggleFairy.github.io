using System;
using UnityEngine;

public class AGFLevelerAction : ItemActionDynamicMelee
{
    static AGFLevelerAction()
    {
        // Log.Out("AGFLevelerAction static constructor called");
    }
    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        // Call base to ensure animation and default logic
        base.ExecuteAction(_actionData, _bReleased);
        // Debug log for testing action trigger
        UnityEngine.Debug.Log("AGFLevelerAction.ExecuteAction called");
        EntityAlive player = _actionData.invData.holdingEntity as EntityAlive;
        if (player == null) return;
        World world = GameManager.Instance.World;

        RaycastHit hit;
        Vector3 eyePos = player.getHeadPosition();
        Vector3 forward = player.GetLookRay().direction;
        if (Physics.Raycast(eyePos, forward, out hit, 6f))
        {
            Vector3i pos = World.worldToBlockPos(hit.point - forward * 0.01f);
            BlockValue blockValue = world.GetBlock(pos);
            Block block = blockValue.Block;
            if (block.shape.IsTerrain())
            {
                sbyte maxDensity = MarchingCubes.DensityTerrain;
                world.SetBlockRPC(0, pos, blockValue, maxDensity);
            }
        }
    }
}
