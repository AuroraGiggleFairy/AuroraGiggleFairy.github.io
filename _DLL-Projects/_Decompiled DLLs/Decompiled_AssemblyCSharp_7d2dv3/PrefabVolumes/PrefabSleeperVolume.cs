using System.Collections.Generic;

namespace PrefabVolumes;

public class PrefabSleeperVolume : PrefabVolumeAbs<PrefabSleeperVolume>
{
	public string groupName;

	public bool isPriority;

	public bool isQuestExclude;

	public short spawnCountMin;

	public short spawnCountMax;

	public short groupId;

	public int flags;

	public string minScript;

	public readonly List<byte> triggeredByIndices = new List<byte>();

	public override EVolumeType VolumeType => EVolumeType.Sleeper;

	public override int SerializedSize => 25 + (1 + groupName?.Length).GetValueOrDefault() + 1 + 1 + 2 + 2 + 2 + 4 + (1 + minScript?.Length).GetValueOrDefault();

	public override void Reset()
	{
		base.Reset();
		groupName = "GroupGenericZombie";
		isPriority = false;
		isQuestExclude = false;
		spawnCountMin = 5;
		spawnCountMax = 6;
		groupId = 0;
		flags = 0;
		minScript = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(PrefabVolumeAbs<PrefabSleeperVolume> _target, bool _nonBasicOnly = false)
	{
		if (!_nonBasicOnly)
		{
			base.CopyValues(_target);
		}
		PrefabSleeperVolume obj = (PrefabSleeperVolume)_target;
		obj.groupId = groupId;
		obj.groupName = groupName;
		obj.isPriority = isPriority;
		obj.isQuestExclude = isQuestExclude;
		obj.spawnCountMin = spawnCountMin;
		obj.spawnCountMax = spawnCountMax;
		obj.triggeredByIndices.AddRange(triggeredByIndices);
		obj.flags = flags;
		obj.minScript = minScript;
	}

	public override void Read(PooledBinaryReader _br)
	{
		base.Read(_br);
		groupName = _br.ReadString();
		isPriority = _br.ReadBoolean();
		isQuestExclude = _br.ReadBoolean();
		spawnCountMin = _br.ReadInt16();
		spawnCountMax = _br.ReadInt16();
		groupId = _br.ReadInt16();
		flags = _br.ReadInt32();
		minScript = _br.ReadString();
	}

	public override void Write(PooledBinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write(groupName);
		_bw.Write(isPriority);
		_bw.Write(isQuestExclude);
		_bw.Write(spawnCountMin);
		_bw.Write(spawnCountMax);
		_bw.Write(groupId);
		_bw.Write(flags);
		_bw.Write(minScript ?? "");
	}

	public void SetTrigger(SleeperVolume.ETriggerType _type)
	{
		flags = (flags & -8) | (int)_type;
	}

	public void SetTriggeredByFlag(byte _index)
	{
		if (!triggeredByIndices.Contains(_index))
		{
			triggeredByIndices.Add(_index);
		}
	}

	public void ClearTriggeredBy()
	{
		triggeredByIndices.Clear();
	}

	public void RemoveTriggeredByFlag(byte _index)
	{
		triggeredByIndices.Remove(_index);
	}

	public bool HasTriggeredBy(byte _index)
	{
		return triggeredByIndices.Contains(_index);
	}

	public void ToggleTriggeredByFlag(byte layer)
	{
		int num = triggeredByIndices.IndexOf(layer);
		if (num >= 0)
		{
			triggeredByIndices.RemoveAt(num);
		}
		else
		{
			triggeredByIndices.Add(layer);
		}
	}

	public bool HasAnyTriggeredBy()
	{
		return triggeredByIndices.Count > 0;
	}
}
