using System.Collections.Generic;

public interface IFeatureTriggerCapability
{
	void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy);
}
