using UnityEngine.Scripting;

[Preserve]
public class EntityBandit : EntityHuman
{
	public override void PostInit()
	{
		ItemValue bareHandItemValue = inventory.GetBareHandItemValue();
		bareHandItemValue.Quality = (ushort)rand.RandomRange(1, 3);
		bareHandItemValue.UseTimes = (float)bareHandItemValue.MaxUseTimes * 0.7f - 1f;
		inventory.SetItem(0, bareHandItemValue, 1);
	}

	public override bool UseHoldingItem(int _actionIndex, bool _isReleased)
	{
		if (!_isReleased && inventory.holdingItemData.actionData[0] is ItemActionAttackData itemActionAttackData)
		{
			ItemValue itemValue = itemActionAttackData.invData.itemValue;
			itemValue.UseTimes = (float)itemValue.MaxUseTimes * 0.8f - 1f;
			if (itemActionAttackData is ItemActionRanged.ItemActionDataRanged)
			{
				itemValue.Meta = 2;
			}
		}
		return base.UseHoldingItem(_actionIndex, _isReleased);
	}
}
