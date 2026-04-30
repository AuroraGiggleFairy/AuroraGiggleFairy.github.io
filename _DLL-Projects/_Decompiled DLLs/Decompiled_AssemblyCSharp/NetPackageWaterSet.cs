using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaterSet : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct WaterSetInfo
	{
		public Vector3i worldPos;

		public WaterValue waterData;

		public void Read(BinaryReader _br)
		{
			worldPos = StreamUtils.ReadVector3i(_br);
			waterData.Read(_br);
		}

		public void Write(BinaryWriter _bw)
		{
			StreamUtils.Write(_bw, worldPos);
			waterData.Write(_bw);
		}

		public static int GetLength()
		{
			return 12 + WaterValue.SerializedLength();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WaterSetInfo> changes = new List<WaterSetInfo>();

	public bool HasChanges => changes.Count > 0;

	public void SetSenderId(int _entityId)
	{
		senderEntityId = _entityId;
	}

	public void Reset()
	{
		senderEntityId = -1;
		changes.Clear();
	}

	public void AddChange(int _worldX, int _worldY, int _worldZ, WaterValue _data)
	{
		AddChange(new Vector3i(_worldX, _worldY, _worldZ), _data);
	}

	public void AddChange(Vector3i _worldPos, WaterValue _data)
	{
		WaterSetInfo item = new WaterSetInfo
		{
			worldPos = _worldPos,
			waterData = _data
		};
		changes.Add(item);
	}

	public override void read(PooledBinaryReader _br)
	{
		senderEntityId = _br.ReadInt32();
		changes.Clear();
		int num = _br.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			WaterSetInfo item = default(WaterSetInfo);
			item.Read(_br);
			changes.Add(item);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(senderEntityId);
		int count = changes.Count;
		_bw.Write((ushort)count);
		for (int i = 0; i < changes.Count; i++)
		{
			changes[i].Write(_bw);
		}
	}

	public void ApplyChanges(ChunkCluster _cc)
	{
		_cc.ChunkPosNeedsRegeneration_DelayedStart();
		for (int i = 0; i < changes.Count; i++)
		{
			WaterSetInfo waterSetInfo = changes[i];
			_cc.SetWater(waterSetInfo.worldPos, waterSetInfo.waterData);
			GameManager.Instance.World.HandleWaterLevelChanged(waterSetInfo.worldPos, waterSetInfo.waterData.GetMassPercent());
		}
		_cc.ChunkPosNeedsRegeneration_DelayedStop();
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(this, _onlyClientsAttachedToAnEntity: false, -1, senderEntityId);
		}
		if (_world != null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[0];
			if (chunkCluster != null)
			{
				ApplyChanges(chunkCluster);
			}
		}
	}

	public override int GetLength()
	{
		return 2 + changes.Count * WaterSetInfo.GetLength();
	}
}
