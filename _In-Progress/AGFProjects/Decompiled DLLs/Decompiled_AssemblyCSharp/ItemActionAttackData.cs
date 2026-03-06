using UnityEngine.Scripting;

[Preserve]
public class ItemActionAttackData : ItemActionData
{
	public delegate WorldRayHitInfo HitDelegate(out float damageScale);

	public HitDelegate hitDelegate;

	public ItemActionAttackData(ItemInventoryData _inventoryData, int _indexInEntityOfAction)
		: base(_inventoryData, _indexInEntityOfAction)
	{
		attackDetails = new ItemActionAttack.AttackHitInfo();
	}
}
