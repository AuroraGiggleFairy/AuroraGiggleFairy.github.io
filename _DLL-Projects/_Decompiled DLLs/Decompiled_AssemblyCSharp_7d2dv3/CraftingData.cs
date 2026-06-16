using System.IO;

public class CraftingData
{
	public enum BreakdownType
	{
		None,
		Part,
		Recipe
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 100;

	public ItemStack[] items;

	public ItemStack[] outputItems;

	public bool isCrafting;

	public ulong lastWorldTick;

	public int totalLeftToCraft;

	public float currentRecipeTimer;

	public Recipe currentRecipeToCraft;

	public Recipe lastRecipeToCraft;

	public ItemValue repairedItem;

	public BreakdownType breakDownType;

	public bool isItemPlacedByUser;

	public ulong savedWorldTick;

	public RecipeQueueItem[] RecipeQueueItems;

	public CraftingData()
	{
		items = new ItemStack[0];
		outputItems = new ItemStack[0];
		breakDownType = BreakdownType.None;
		RecipeQueueItems = new RecipeQueueItem[0];
	}

	public void Write(BinaryWriter _bw)
	{
		int num = RecipeQueueItems.Length;
		_bw.Write((byte)num);
		for (int i = 0; i < num; i++)
		{
			if (RecipeQueueItems[i] == null)
			{
				RecipeQueueItems[i] = new RecipeQueueItem();
			}
			RecipeQueueItems[i].Write(_bw, 0u);
		}
	}

	public void Read(BinaryReader _br, uint _version = 100u)
	{
		int num = _br.ReadByte();
		RecipeQueueItems = new RecipeQueueItem[num];
		for (int i = 0; i < num; i++)
		{
			RecipeQueueItems[i] = new RecipeQueueItem();
			RecipeQueueItems[i].Read(_br, _version);
		}
	}
}
