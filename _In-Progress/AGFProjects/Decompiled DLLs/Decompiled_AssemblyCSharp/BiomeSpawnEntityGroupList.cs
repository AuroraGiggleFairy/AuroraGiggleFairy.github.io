using System.Collections.Generic;

public class BiomeSpawnEntityGroupList
{
	public List<BiomeSpawnEntityGroupData> list = new List<BiomeSpawnEntityGroupData>();

	public BiomeSpawnEntityGroupData Find(int _idHash)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].idHash == _idHash)
			{
				return list[i];
			}
		}
		return null;
	}
}
