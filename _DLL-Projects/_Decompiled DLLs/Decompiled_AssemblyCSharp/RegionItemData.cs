using UnityEngine.Scripting;

[Preserve]
public class RegionItemData
{
	public int X;

	public int Z;

	public int UpdateTime;

	public RegionItemData(int x, int z, int updateTime)
	{
		X = x;
		Z = z;
		UpdateTime = updateTime;
	}

	public void Update(int x, int z, int updateTime)
	{
		X = x;
		Z = z;
		UpdateTime = updateTime;
	}

	public void Update(DynamicMeshItem item)
	{
		X = item.WorldPosition.x;
		Z = item.WorldPosition.z;
		UpdateTime = item.UpdateTime;
	}
}
