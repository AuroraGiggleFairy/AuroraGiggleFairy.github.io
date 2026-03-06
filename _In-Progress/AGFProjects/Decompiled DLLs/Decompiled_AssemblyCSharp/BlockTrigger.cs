using System.Collections.Generic;
using UnityEngine;

public class BlockTrigger
{
	public enum TriggeredStates
	{
		NotTriggered,
		NeedsTriggered,
		HasTriggered
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const ushort version = 5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ushort currentVersion;

	public Vector3i LocalChunkPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	public PrefabTriggerData TriggerDataOwner;

	public List<byte> TriggersIndices = new List<byte>();

	public List<byte> TriggeredByIndices = new List<byte>();

	public List<byte> TriggeredValues = new List<byte>();

	public bool ExcludeIcon;

	public bool UseOrForMultipleTriggers;

	public bool Unlock;

	public TriggeredStates NeedsTriggered;

	public Chunk Chunk
	{
		get
		{
			return chunk;
		}
		set
		{
			chunk = value;
			long num = 0L;
			if (chunk != null)
			{
				num = chunk.Key;
			}
			chunkKey = num;
		}
	}

	public BlockValue BlockValue => Chunk.GetBlock(LocalChunkPos);

	public BlockTrigger(Chunk chunkNew)
	{
		Chunk = chunkNew;
	}

	public void Refresh(FastTags<TagGroup.Global> questTag)
	{
		chunk = (Chunk)GameManager.Instance.World.GetChunkSync(chunkKey);
		if (chunk == null)
		{
			Log.Error($"BlockTrigger.Refresh: Chunk null. ChunkKey={chunkKey}, LocalChunkPos={LocalChunkPos}, PrefabInstance={TriggerDataOwner?.PrefabInstance?.name}. From: {StackTraceUtility.ExtractStackTrace()}");
		}
		else
		{
			BlockValue blockValue = BlockValue;
			blockValue.Block.OnTriggerRefresh(this, blockValue, questTag);
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		currentVersion = _br.ReadUInt16();
		if (currentVersion >= 2)
		{
			NeedsTriggered = (TriggeredStates)_br.ReadByte();
		}
		int num = _br.ReadByte();
		TriggersIndices.Clear();
		for (int i = 0; i < num; i++)
		{
			TriggersIndices.Add(_br.ReadByte());
		}
		num = _br.ReadByte();
		TriggeredByIndices.Clear();
		for (int j = 0; j < num; j++)
		{
			TriggeredByIndices.Add(_br.ReadByte());
		}
		num = _br.ReadByte();
		TriggeredValues.Clear();
		for (int k = 0; k < num; k++)
		{
			TriggeredValues.Add(_br.ReadByte());
		}
		if (currentVersion >= 3)
		{
			ExcludeIcon = _br.ReadBoolean();
		}
		if (currentVersion >= 4)
		{
			UseOrForMultipleTriggers = _br.ReadBoolean();
		}
		if (currentVersion >= 5)
		{
			Unlock = _br.ReadBoolean();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write((ushort)5);
		_bw.Write((byte)NeedsTriggered);
		_bw.Write((byte)TriggersIndices.Count);
		for (int i = 0; i < TriggersIndices.Count; i++)
		{
			_bw.Write(TriggersIndices[i]);
		}
		_bw.Write((byte)TriggeredByIndices.Count);
		for (int j = 0; j < TriggeredByIndices.Count; j++)
		{
			_bw.Write(TriggeredByIndices[j]);
		}
		_bw.Write((byte)TriggeredValues.Count);
		for (int k = 0; k < TriggeredValues.Count; k++)
		{
			_bw.Write(TriggeredValues[k]);
		}
		_bw.Write(ExcludeIcon);
		_bw.Write(UseOrForMultipleTriggers);
		_bw.Write(Unlock);
	}

	public BlockTrigger Clone()
	{
		BlockTrigger blockTrigger = new BlockTrigger(Chunk);
		blockTrigger.LocalChunkPos = LocalChunkPos;
		blockTrigger.TriggersIndices.Clear();
		blockTrigger.TriggeredByIndices.Clear();
		blockTrigger.TriggeredValues.Clear();
		for (int i = 0; i < TriggersIndices.Count; i++)
		{
			blockTrigger.TriggersIndices.Add(TriggersIndices[i]);
		}
		for (int j = 0; j < TriggeredByIndices.Count; j++)
		{
			blockTrigger.TriggeredByIndices.Add(TriggeredByIndices[j]);
		}
		for (int k = 0; k < TriggeredValues.Count; k++)
		{
			blockTrigger.TriggeredValues.Add(TriggeredValues[k]);
		}
		blockTrigger.ExcludeIcon = ExcludeIcon;
		blockTrigger.UseOrForMultipleTriggers = UseOrForMultipleTriggers;
		blockTrigger.Unlock = Unlock;
		return blockTrigger;
	}

	public void CopyFrom(BlockTrigger _other)
	{
		LocalChunkPos = _other.LocalChunkPos;
		TriggersIndices.Clear();
		TriggeredByIndices.Clear();
		TriggeredValues.Clear();
		for (int i = 0; i < _other.TriggersIndices.Count; i++)
		{
			TriggersIndices.Add(_other.TriggersIndices[i]);
		}
		for (int j = 0; j < _other.TriggeredByIndices.Count; j++)
		{
			TriggeredByIndices.Add(_other.TriggeredByIndices[j]);
		}
		for (int k = 0; k < _other.TriggeredValues.Count; k++)
		{
			TriggeredValues.Add(_other.TriggeredValues[k]);
		}
		_other.ExcludeIcon = ExcludeIcon;
		_other.UseOrForMultipleTriggers = UseOrForMultipleTriggers;
		_other.Unlock = Unlock;
	}

	public void SetTriggersFlag(byte index)
	{
		if (!TriggersIndices.Contains(index))
		{
			TriggersIndices.Add(index);
		}
	}

	public void RemoveTriggersFlag(byte index)
	{
		TriggersIndices.Remove(index);
	}

	public void RemoveAllTriggersFlags()
	{
		TriggersIndices.Clear();
	}

	public bool HasTriggers(byte index)
	{
		return TriggersIndices.Contains(index);
	}

	public bool HasAnyTriggers()
	{
		return TriggersIndices.Count > 0;
	}

	public void SetTriggeredByFlag(byte index)
	{
		if (!TriggeredByIndices.Contains(index))
		{
			TriggeredByIndices.Add(index);
		}
	}

	public void RemoveTriggeredByFlag(byte index)
	{
		TriggeredByIndices.Remove(index);
	}

	public bool HasTriggeredBy(byte index)
	{
		return TriggeredByIndices.Contains(index);
	}

	public bool HasAnyTriggeredBy()
	{
		return TriggeredByIndices.Count > 0;
	}

	public void SetTriggeredValueFlag(byte index)
	{
		if (TriggeredValues.Contains(index))
		{
			TriggeredValues.Remove(index);
		}
		else
		{
			TriggeredValues.Add(index);
		}
	}

	public bool CheckIsTriggered()
	{
		if (UseOrForMultipleTriggers)
		{
			foreach (byte triggeredByIndex in TriggeredByIndices)
			{
				if (!TriggeredValues.Contains(triggeredByIndex))
				{
					return true;
				}
			}
			return false;
		}
		foreach (byte triggeredByIndex2 in TriggeredByIndices)
		{
			if (!TriggeredValues.Contains(triggeredByIndex2))
			{
				return false;
			}
		}
		return true;
	}

	public string TriggerDisplay()
	{
		if (TriggeredByIndices.Count != 0 && TriggersIndices.Count == 0)
		{
			return string.Format("[0000FF]{0}[-]", string.Join(",", TriggeredByIndices));
		}
		if (TriggersIndices.Count != 0 && TriggeredByIndices.Count == 0)
		{
			return string.Format("[FF0000]{0}[-][0000FF]{1}[-]", string.Join(",", TriggersIndices), string.Join(",", TriggeredByIndices));
		}
		return string.Format("[FF0000]{0}[-] | [0000FF]{1}[-]", string.Join(",", TriggersIndices), string.Join(",", TriggeredByIndices));
	}

	public Vector3i ToWorldPos()
	{
		if (Chunk != null)
		{
			return new Vector3i(Chunk.X * 16, Chunk.Y * 256, Chunk.Z * 16) + LocalChunkPos;
		}
		return Vector3i.zero;
	}

	public void TriggerUpdated(List<BlockChangeInfo> _blockChanges)
	{
		BlockValue block = Chunk.GetBlock(LocalChunkPos);
		if (_blockChanges != null)
		{
			block.Block.OnTriggerChanged(this, Chunk, ToWorldPos(), block, _blockChanges);
		}
		else
		{
			block.Block.OnTriggerChanged(this, Chunk, ToWorldPos(), block);
		}
	}

	public void OnTriggered(EntityPlayer _player, World _world, int index, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy = null)
	{
		SetTriggeredValueFlag((byte)index);
		if (CheckIsTriggered())
		{
			BlockValue block = Chunk.GetBlock(LocalChunkPos);
			block.Block.OnTriggered(_player, _world, Chunk.ClrIdx, ToWorldPos(), block, _blockChanges, _triggeredBy);
			TriggeredValues.Clear();
		}
	}
}
