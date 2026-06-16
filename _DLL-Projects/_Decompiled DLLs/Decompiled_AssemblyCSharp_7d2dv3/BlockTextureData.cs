public class BlockTextureData
{
	public static BlockTextureData[] list;

	public int ID;

	public ushort TextureID;

	public string Name;

	public string LocalizedName;

	public string Group;

	public ushort PaintCost;

	public bool Hidden;

	public byte SortIndex = byte.MaxValue;

	public string LockedByPerk = "";

	public ushort RequiredLevel;

	public static void InitStatic()
	{
		list = new BlockTextureData[256];
	}

	public void Init()
	{
		list[ID] = this;
	}

	public static void Cleanup()
	{
		list = null;
	}

	public static BlockTextureData GetDataByTextureID(int textureID)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] != null && list[i].TextureID == textureID)
			{
				return list[i];
			}
		}
		return null;
	}

	public bool GetLocked(EntityPlayerLocal player)
	{
		if (LockedByPerk != "")
		{
			ProgressionValue progressionValue = player.Progression.GetProgressionValue(LockedByPerk);
			if (progressionValue != null && progressionValue.CalculatedLevel(player) >= RequiredLevel)
			{
				return true;
			}
		}
		return false;
	}
}
