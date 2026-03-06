using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDynamicClientArrive : NetPackage
{
	public List<RegionItemData> Items = new List<RegionItemData>();

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public override int Channel => 0;

	public override bool Compress => true;

	public void BuildData()
	{
		foreach (DynamicMeshItem value in DynamicMeshManager.Instance.ItemsDictionary.Values)
		{
			Items.Add(FromPool(value));
		}
		Log.Out("Client package items: " + Items.Count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionItemData FromPool(DynamicMeshItem i)
	{
		return new RegionItemData(i.WorldPosition.x, i.WorldPosition.z, i.UpdateTime);
	}

	public override void read(PooledBinaryReader reader)
	{
		int num = reader.ReadInt32();
		Items = new List<RegionItemData>(num);
		for (int i = 0; i < num; i++)
		{
			int x = reader.ReadInt32();
			int z = reader.ReadInt32();
			int updateTime = reader.ReadInt32();
			Items.Add(new RegionItemData(x, z, updateTime));
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(Items.Count);
		for (int i = 0; i < Items.Count; i++)
		{
			writer.Write(Items[i].X);
			writer.Write(Items[i].Z);
			writer.Write(Items[i].UpdateTime);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (DynamicMeshManager.CONTENT_ENABLED)
		{
			DynamicMeshServer.ClientMessageRecieved(this);
		}
	}

	public override int GetLength()
	{
		return 4 + 12 * Items.Count;
	}
}
