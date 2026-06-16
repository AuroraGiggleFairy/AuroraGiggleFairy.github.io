using System;
using System.Collections.Generic;

namespace PrefabVolumes;

public abstract class PrefabVolumeListAbs
{
	public abstract PrefabVolumeAbs.EVolumeType VolumeType { get; }

	public abstract bool AnyUsedEntry { get; }

	public abstract int Count { get; }

	public abstract PrefabVolumeAbs Get(int _index);

	public abstract void CopyFrom(PrefabVolumeListAbs _other);

	public abstract void ReadFromProperties(DynamicProperties _properties);

	public abstract void WriteToProperties(DynamicProperties _properties);

	public abstract void SendAllVolumesToClient(ClientInfo _clientInfo, int _prefabInstanceId);

	public abstract void Move(Vector3i _moveDistance);

	public abstract void RotateY(bool _bLeft, Vector3i _prefabSize);

	public abstract bool CanCreateVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size);

	public abstract (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size);

	public abstract (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) CloneVolume(string _prefabInstanceName, Vector3i _bbPos, int _existingIndex, Vector3i _offset);

	public abstract void SetVolume(PrefabInstance _prefabInstance, int _index, PrefabVolumeAbs _volumeSettings);

	public abstract void CreateSelectionBoxes(PrefabInstance _prefabInstance);

	public abstract void ApplyVolumesToSelectionBoxes(PrefabInstance _prefabInstance);

	public abstract void RemoveSelectionBoxes(PrefabInstance _prefabInstance);

	public abstract void RemoveVolumes(PrefabInstance _prefabInstance);

	public abstract void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset);

	[PublicizedFrom(EAccessModifier.Protected)]
	public PrefabVolumeListAbs()
	{
	}
}
public abstract class PrefabVolumeListAbs<TVolumeList, TVolume> : PrefabVolumeListAbs where TVolumeList : PrefabVolumeListAbs<TVolumeList, TVolume> where TVolume : PrefabVolumeAbs<TVolume>, new()
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly Prefab Owner;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<TVolume> List = new List<TVolume>();

	public abstract SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public override bool AnyUsedEntry
	{
		get
		{
			if (List.Count > 0)
			{
				return List.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (TVolume _volume) => _volume.Used) >= 0;
			}
			return false;
		}
	}

	public override int Count => List.Count;

	public TVolume this[int _index] => List[_index];

	public override PrefabVolumeAbs Get(int _index)
	{
		return this[_index];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public PrefabVolumeListAbs(Prefab _owner)
	{
		Owner = _owner;
	}

	public override void CopyFrom(PrefabVolumeListAbs _other)
	{
		if (!(_other is TVolumeList val))
		{
			Log.Error("PrefabVolumeList.CopyFrom: Other list (" + _other.GetType().Name + ") is not the same type as the current instance (" + typeof(TVolumeList).Name + ")");
		}
		else
		{
			for (int i = 0; i < val.List.Count; i++)
			{
				List.Add(val.List[i].CloneGeneric());
			}
		}
	}

	public override void SendAllVolumesToClient(ClientInfo _clientInfo, int _prefabInstanceId)
	{
		for (int i = 0; i < List.Count; i++)
		{
			_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEditorUpdateVolume>().Setup(NetPackageEditorUpdateVolume.EChangeType.Added, _prefabInstanceId, i, List[i]));
		}
	}

	public override void Move(Vector3i _moveDistance)
	{
		foreach (TVolume item in List)
		{
			item.Move(_moveDistance);
		}
	}

	public override void RotateY(bool _bLeft, Vector3i _prefabSize)
	{
		foreach (TVolume item in List)
		{
			item.RotateY(_bLeft, _prefabSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public (TVolume volume, int index, string name) PrepareNewEntry(string _prefabInstanceName, Vector3i _startPos, Vector3i _size)
	{
		int num = -1;
		TVolume val = null;
		for (int i = 0; i < List.Count; i++)
		{
			if (!List[i].Used)
			{
				num = i;
				val = List[i];
				break;
			}
		}
		if (val == null)
		{
			val = new TVolume();
			num = List.Count;
			List.Add(val);
		}
		val.Reset();
		val.Use(_startPos, _size);
		return (volume: val, index: num, name: $"{_prefabInstanceName}_{num}");
	}

	public override bool CanCreateVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
	{
		return true;
	}

	public override (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
	{
		(TVolume volume, int index, string name) tuple = PrepareNewEntry(_prefabInstanceName, _startPos, _size);
		TVolume item = tuple.volume;
		int item2 = tuple.index;
		string item3 = tuple.name;
		SelectionBox selectionBox = AddSelectionBox(item, item3, _bbPos + _startPos);
		SelectionBoxManager.Instance.SetActive(selectionBox, _bActive: true);
		return (volumeIndex: item2, volume: item, box: selectionBox);
	}

	public override (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) CloneVolume(string _prefabInstanceName, Vector3i _bbPos, int _existingIndex, Vector3i _offset)
	{
		PrefabVolumeAbs prefabVolumeAbs = List[_existingIndex];
		var (item, prefabVolumeAbs2, selectionBox) = AddNewVolume(_prefabInstanceName, _bbPos, prefabVolumeAbs.startPos + _offset, prefabVolumeAbs.size);
		prefabVolumeAbs.CopyValues(prefabVolumeAbs2, _nonBasicOnly: true);
		selectionBox.UserData = prefabVolumeAbs2;
		return (volumeIndex: item, volume: prefabVolumeAbs2, box: selectionBox);
	}

	public override void SetVolume(PrefabInstance _prefabInstance, int _index, PrefabVolumeAbs _volumeSettings)
	{
		if (!(_volumeSettings is TVolume volumeSettings))
		{
			throw new ArgumentException("Can not add volume of type " + _volumeSettings.VolumeType.ToStringCached() + " to list of type " + VolumeType.ToStringCached(), "_volumeSettings");
		}
		SetVolume(_prefabInstance, _index, volumeSettings);
	}

	public virtual void SetVolume(PrefabInstance _prefabInstance, int _index, TVolume _volumeSettings)
	{
		while (_index >= List.Count)
		{
			List.Add(new TVolume());
		}
		bool used = List[_index].Used;
		List[_index] = _volumeSettings;
		string name = _prefabInstance.name + "_" + _index;
		if (_volumeSettings.Used)
		{
			if (!used)
			{
				SelectionBox box = AddSelectionBox(_volumeSettings, name, _prefabInstance.boundingBoxPosition + _volumeSettings.startPos);
				SelectionBoxManager.Instance.SetActive(box, _bActive: true);
			}
			else
			{
				SelectionCategory.TryGetBox(name, out var _box);
				_box.SetPositionAndSize(_prefabInstance.boundingBoxPosition + _volumeSettings.startPos, _volumeSettings.size);
				_box.UserData = _volumeSettings;
			}
		}
		else if (used)
		{
			SelectionCategory.RemoveBox(name);
		}
	}

	public virtual SelectionBox AddSelectionBox(TVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBox selectionBox = SelectionCategory.AddBox(_name, _pos, _volume.size);
		selectionBox.UserData = _volume;
		return selectionBox;
	}

	public override void CreateSelectionBoxes(PrefabInstance _prefabInstance)
	{
		for (int i = 0; i < List.Count; i++)
		{
			TVolume val = List[i];
			AddSelectionBox(val, _prefabInstance.name + "_" + i, _prefabInstance.boundingBoxPosition + val.startPos);
		}
	}

	public override void ApplyVolumesToSelectionBoxes(PrefabInstance _prefabInstance)
	{
		for (int i = 0; i < List.Count; i++)
		{
			TVolume val = List[i];
			if (val.Used)
			{
				SelectionBox box = SelectionCategory.GetBox(_prefabInstance.name + "_" + i);
				if (box != null)
				{
					box.SetPositionAndSize(_prefabInstance.boundingBoxPosition + val.startPos, val.size);
				}
			}
		}
	}

	public override void RemoveSelectionBoxes(PrefabInstance _prefabInstance)
	{
		for (int i = 0; i < List.Count; i++)
		{
			if (List[i].Used)
			{
				SelectionCategory.RemoveBox(_prefabInstance.name + "_" + i);
			}
		}
	}

	public override void RemoveVolumes(PrefabInstance _prefabInstance)
	{
		for (int i = 0; i < List.Count; i++)
		{
			TVolume val = List[i];
			val.MarkUnused();
			SetVolume(_prefabInstance, i, val);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
	{
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int CreateWorldVolume(int _prefabVolumeIndex, TVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
	{
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddWorldVolume(Chunk _chunk, int _volumeIndex)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyVolumesIntoWorldCommon(World _world, Chunk _chunk, Vector3i _offset, Vector3i _padding)
	{
		Vector3i vector3i = Vector3i.zero;
		Vector3i vector3i2 = Vector3i.zero;
		if (_chunk != null)
		{
			vector3i = _chunk.GetWorldPos();
			vector3i2 = vector3i + new Vector3i(16, 256, 16);
		}
		for (int i = 0; i < List.Count; i++)
		{
			TVolume val = List[i];
			if (!val.Used || (World.SandboxUseTraderArea != TraderAreaStates.Default && (val.VolumeType == PrefabVolumeAbs.EVolumeType.Teleport || val.VolumeType == PrefabVolumeAbs.EVolumeType.Wall)))
			{
				continue;
			}
			Vector3i startPos = val.startPos;
			Vector3i volumeMax = startPos + val.size;
			Vector3i vector3i3 = startPos + _offset;
			Vector3i vector3i4 = vector3i3 + val.size;
			Vector3i vector3i5 = vector3i3 - _padding;
			Vector3i vector3i6 = vector3i4 + _padding;
			if (_chunk != null)
			{
				if (vector3i5.x < vector3i2.x && vector3i6.x > vector3i.x && vector3i5.y < vector3i2.y && vector3i6.y > vector3i.y && vector3i5.z < vector3i2.z && vector3i6.z > vector3i.z)
				{
					int num = FindWorldVolume(_world, vector3i3, vector3i4);
					if (num < 0)
					{
						num = CreateWorldVolume(i, val, _offset, startPos, volumeMax, _world, vector3i3, vector3i4);
					}
					AddWorldVolume(_chunk, num);
				}
				continue;
			}
			int num2 = FindWorldVolume(_world, vector3i3, vector3i4);
			if (num2 < 0)
			{
				num2 = CreateWorldVolume(i, val, _offset, startPos, volumeMax, _world, vector3i3, vector3i4);
			}
			int num3 = World.toChunkXZ(vector3i5.x);
			int num4 = World.toChunkXZ(vector3i6.x - 1);
			int num5 = World.toChunkXZ(vector3i5.z);
			int num6 = World.toChunkXZ(vector3i6.z - 1);
			for (int j = num3; j <= num4; j++)
			{
				for (int k = num5; k <= num6; k++)
				{
					Chunk chunk = (Chunk)_world.GetChunkSync(j, k);
					if (chunk != null)
					{
						AddWorldVolume(chunk, num2);
					}
				}
			}
		}
	}

	public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
	}
}
