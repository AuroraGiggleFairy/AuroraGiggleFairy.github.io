using UnityEngine.Scripting;

[Preserve]
public class ItemActionData
{
	public ItemInventoryData invData;

	public float lastUseTime;

	public int indexInEntityOfAction;

	public bool bWaitForRelease;

	public ItemActionAttack.AttackHitInfo attackDetails;

	public FastTags<TagGroup.Global> ActionTags;

	public bool HasExecuted;

	public bool uiOpenedByMe;

	public MinEventParams EventParms;

	public WorldRayHitInfo hitInfo;

	public ItemActionData(ItemInventoryData _inventoryData, int _indexInEntityOfAction)
	{
		invData = _inventoryData;
		indexInEntityOfAction = _indexInEntityOfAction;
		ActionTags = FastTags<TagGroup.Global>.Parse(_indexInEntityOfAction switch
		{
			1 => "secondary", 
			0 => "primary", 
			_ => "action2", 
		});
		EventParms = new MinEventParams();
		hitInfo = new WorldRayHitInfo();
	}

	public WorldRayHitInfo GetUpdatedHitInfo()
	{
		hitInfo.CopyFrom(Voxel.voxelRayHitInfo);
		return hitInfo;
	}
}
