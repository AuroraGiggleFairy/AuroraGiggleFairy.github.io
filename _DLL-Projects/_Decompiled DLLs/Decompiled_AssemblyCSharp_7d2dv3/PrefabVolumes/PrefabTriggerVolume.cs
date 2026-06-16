using System.Collections.Generic;
using UnityEngine.Scripting;

namespace PrefabVolumes;

[Preserve]
public class PrefabTriggerVolume : PrefabVolumeAbs<PrefabTriggerVolume>
{
	public readonly List<byte> TriggersIndices = new List<byte>();

	public override EVolumeType VolumeType => EVolumeType.Trigger;

	public override int SerializedSize => 26 + TriggersIndices.Count;

	public override void Reset()
	{
		base.Reset();
		TriggersIndices.Clear();
	}

	public override void Read(PooledBinaryReader _br)
	{
		base.Read(_br);
		TriggersIndices.Clear();
		int num = _br.ReadByte();
		for (int i = 0; i < num; i++)
		{
			TriggersIndices.Add(_br.ReadByte());
		}
	}

	public override void Write(PooledBinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write((byte)TriggersIndices.Count);
		for (int i = 0; i < TriggersIndices.Count; i++)
		{
			_bw.Write(TriggersIndices[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(PrefabVolumeAbs<PrefabTriggerVolume> _target, bool _nonBasicOnly = false)
	{
		if (!_nonBasicOnly)
		{
			base.CopyValues(_target);
		}
		((PrefabTriggerVolume)_target).TriggersIndices.AddRange(TriggersIndices);
	}

	public void SetTriggersFlag(byte _index)
	{
		if (!TriggersIndices.Contains(_index))
		{
			TriggersIndices.Add(_index);
		}
	}

	public void RemoveTriggersFlag(byte _index)
	{
		TriggersIndices.Remove(_index);
	}

	public void RemoveAllTriggersFlags()
	{
		TriggersIndices.Clear();
	}

	public bool HasTriggers(byte _index)
	{
		return TriggersIndices.Contains(_index);
	}

	public void ToggleTriggersFlag(byte layer)
	{
		int num = TriggersIndices.IndexOf(layer);
		if (num >= 0)
		{
			TriggersIndices.RemoveAt(num);
		}
		else
		{
			TriggersIndices.Add(layer);
		}
	}

	public bool HasAnyTriggers()
	{
		return TriggersIndices.Count > 0;
	}
}
