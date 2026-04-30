using UnityEngine.Scripting;

[Preserve]
public class BlockGenerator : BlockPowerSource
{
	public static FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse("gasoline");

	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (slotItem == null)
		{
			slotItem = ItemClass.GetItemClass(SlotItemName);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.Generator,
			SlotItem = slotItem
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "electric_generator";
	}
}
