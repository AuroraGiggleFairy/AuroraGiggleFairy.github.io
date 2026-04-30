using System.IO;

public class CraftCompleteData
{
	public int CrafterEntityID;

	public ItemStack CraftedItemStack;

	public string RecipeName = "";

	public string ItemScrapped = "";

	public ushort RecipeUsedCount;

	public int CraftExpGain;

	public CraftCompleteData()
	{
	}

	public CraftCompleteData(int crafterEntityID, ItemStack craftedItemStack, string recipeName, string itemScrapped, int craftExpGain, ushort recipeUsedCount)
	{
		CrafterEntityID = crafterEntityID;
		CraftedItemStack = craftedItemStack;
		RecipeName = recipeName;
		ItemScrapped = itemScrapped;
		RecipeUsedCount = recipeUsedCount;
		CraftExpGain = craftExpGain;
	}

	public void Write(BinaryWriter _bw, int version)
	{
		_bw.Write(CrafterEntityID);
		CraftedItemStack.Write(_bw);
		_bw.Write(RecipeName);
		_bw.Write(CraftExpGain);
		_bw.Write(RecipeUsedCount);
		_bw.Write(ItemScrapped);
	}

	public void Read(BinaryReader _br, int version)
	{
		CrafterEntityID = _br.ReadInt32();
		CraftedItemStack = new ItemStack().Read(_br);
		RecipeName = _br.ReadString();
		CraftExpGain = _br.ReadInt32();
		if (version >= 48)
		{
			RecipeUsedCount = _br.ReadUInt16();
		}
		else
		{
			RecipeUsedCount = (ushort)CraftedItemStack.count;
		}
		if (version >= 49)
		{
			ItemScrapped = _br.ReadString();
		}
	}
}
