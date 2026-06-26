using UnityEngine.Scripting;

[Preserve]
public class BlockCampfire : BlockWorkstation
{
	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useCampfire");
	}
}
