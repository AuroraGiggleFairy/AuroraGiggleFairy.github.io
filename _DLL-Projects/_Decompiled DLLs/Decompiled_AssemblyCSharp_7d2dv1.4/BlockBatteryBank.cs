using UnityEngine.Scripting;

[Preserve]
public class BlockBatteryBank : BlockPowerSource
{
	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (slotItem == null)
		{
			slotItem = ItemClass.GetItemClass(SlotItemName);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.BatteryBank,
			SlotItem = slotItem
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "battery";
	}
}
