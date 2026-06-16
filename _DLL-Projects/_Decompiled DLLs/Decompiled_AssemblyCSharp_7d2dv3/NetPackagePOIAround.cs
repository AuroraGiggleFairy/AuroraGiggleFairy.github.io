using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePOIAround : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public override int Channel => 1;

	public override bool Compress => true;

	public NetPackagePOIAround Setup(Dictionary<int, PrefabInstance> _prefabsAroundFar, Dictionary<int, PrefabInstance> _prefabsAroundNear)
	{
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		pooledBinaryWriter.Write((ushort)_prefabsAroundFar.Count);
		foreach (KeyValuePair<int, PrefabInstance> item in _prefabsAroundFar)
		{
			pooledBinaryWriter.Write(item.Value.id);
			pooledBinaryWriter.Write(item.Value.rotation);
			pooledBinaryWriter.Write((item.Value.prefab.distantPOIOverride == null) ? item.Value.prefab.PrefabName : item.Value.prefab.distantPOIOverride);
			StreamUtils.Write(pooledBinaryWriter, item.Value.boundingBoxPosition);
			StreamUtils.Write(pooledBinaryWriter, item.Value.boundingBoxSize);
			pooledBinaryWriter.Write(item.Value.prefab.distantPOIYOffset);
		}
		pooledBinaryWriter.Write((ushort)_prefabsAroundNear.Count);
		foreach (KeyValuePair<int, PrefabInstance> item2 in _prefabsAroundNear)
		{
			pooledBinaryWriter.Write(item2.Value.id);
			pooledBinaryWriter.Write(item2.Value.rotation);
			pooledBinaryWriter.Write((item2.Value.prefab.distantPOIOverride == null) ? item2.Value.prefab.PrefabName : item2.Value.prefab.distantPOIOverride);
			StreamUtils.Write(pooledBinaryWriter, item2.Value.boundingBoxPosition);
			StreamUtils.Write(pooledBinaryWriter, item2.Value.boundingBoxSize);
			pooledBinaryWriter.Write(item2.Value.prefab.distantPOIYOffset);
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackagePOIAround()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _reader)
	{
		int length = _reader.ReadInt32();
		StreamUtils.StreamCopy(_reader.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)ms.Length);
		ms.WriteTo(_writer.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		Dictionary<int, PrefabInstance> dictionary = new Dictionary<int, PrefabInstance>();
		Dictionary<int, PrefabInstance> dictionary2 = new Dictionary<int, PrefabInstance>();
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			lock (ms)
			{
				pooledBinaryReader.SetBaseStream(ms);
				ms.Position = 0L;
				int num = pooledBinaryReader.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					int num2 = pooledBinaryReader.ReadInt32();
					byte rotation = pooledBinaryReader.ReadByte();
					string name = pooledBinaryReader.ReadString();
					PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(name);
					Vector3i position = StreamUtils.ReadVector3i(pooledBinaryReader);
					Vector3i boundingBoxSize = StreamUtils.ReadVector3i(pooledBinaryReader);
					PrefabInstance prefabInstance = new PrefabInstance(num2, location, position, rotation, null, 1);
					prefabInstance.boundingBoxSize = boundingBoxSize;
					prefabInstance.yOffsetOfPrefab = pooledBinaryReader.ReadSingle();
					dictionary.Add(num2, prefabInstance);
				}
				num = pooledBinaryReader.ReadUInt16();
				for (int j = 0; j < num; j++)
				{
					int num3 = pooledBinaryReader.ReadInt32();
					byte rotation2 = pooledBinaryReader.ReadByte();
					string name2 = pooledBinaryReader.ReadString();
					PathAbstractions.AbstractedLocation location2 = PathAbstractions.PrefabsSearchPaths.GetLocation(name2);
					Vector3i position2 = StreamUtils.ReadVector3i(pooledBinaryReader);
					Vector3i boundingBoxSize2 = StreamUtils.ReadVector3i(pooledBinaryReader);
					PrefabInstance prefabInstance2 = new PrefabInstance(num3, location2, position2, rotation2, null, 1);
					prefabInstance2.boundingBoxSize = boundingBoxSize2;
					prefabInstance2.yOffsetOfPrefab = pooledBinaryReader.ReadSingle();
					dictionary2.Add(num3, prefabInstance2);
				}
			}
		}
		GameManager.Instance.prefabLODManager.UpdatePrefabsAround(dictionary, dictionary2);
	}

	public override int GetLength()
	{
		return (int)ms.Length;
	}
}
