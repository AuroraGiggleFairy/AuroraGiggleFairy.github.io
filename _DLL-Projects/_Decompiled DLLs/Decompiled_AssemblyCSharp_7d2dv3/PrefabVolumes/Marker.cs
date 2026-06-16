using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PrefabVolumes;

public class Marker : PrefabVolumeAbs<Marker>
{
	public enum MarkerTypes : byte
	{
		None,
		POISpawn,
		RoadExit,
		PartSpawn
	}

	public enum MarkerSize : byte
	{
		One,
		ExtraSmall,
		Small,
		Medium,
		Large,
		Custom
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MarkerTypes markerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int groupId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color color;

	[PublicizedFrom(EAccessModifier.Private)]
	public string partToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public float partChanceToSpawn = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool partDirty = true;

	public static readonly List<Vector3i> MarkerSizes = new List<Vector3i>
	{
		Vector3i.one,
		new Vector3i(25, 0, 25),
		new Vector3i(42, 0, 42),
		new Vector3i(60, 0, 60),
		new Vector3i(100, 0, 100)
	};

	public bool PartDirty => partDirty;

	public string GroupName
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return groupName;
		}
		set
		{
			if (groupName != value)
			{
				color = default(Color);
				groupId = -1;
				groupName = value;
			}
		}
	}

	public Color GroupColor
	{
		get
		{
			if (color == default(Color))
			{
				GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(GroupId);
				color = new Color32((byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)((MarkerType == MarkerTypes.PartSpawn) ? 32u : 128u));
			}
			return color;
		}
	}

	public int GroupId
	{
		get
		{
			if (groupId == -1)
			{
				groupId = GroupName.GetHashCode();
			}
			return groupId;
		}
	}

	public override Vector3i startPos
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return startPosInternal;
		}
		set
		{
			startPosInternal = value;
		}
	}

	public override Vector3i size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return sizeInternal;
		}
		set
		{
			if (sizeInternal != value)
			{
				sizeInternal = value;
				partDirty = true;
			}
		}
	}

	public override EVolumeType VolumeType => EVolumeType.Marker;

	public override int SerializedSize => 26 + (1 + GroupName?.Length).GetValueOrDefault() + 100 + (1 + PartToSpawn?.Length).GetValueOrDefault() + 1 + 4;

	public MarkerTypes MarkerType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return markerType;
		}
		set
		{
			if (markerType != value)
			{
				markerType = value;
				partDirty = true;
			}
		}
	}

	public FastTags<TagGroup.Poi> Tags
	{
		get
		{
			return tags;
		}
		set
		{
			tags = value;
		}
	}

	public string PartToSpawn
	{
		get
		{
			return partToSpawn;
		}
		set
		{
			if (partToSpawn != value)
			{
				partToSpawn = value;
				partDirty = true;
			}
		}
	}

	public byte Rotations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return rotations;
		}
		set
		{
			if (rotations != value)
			{
				rotations = value;
				partDirty = true;
			}
		}
	}

	public float PartChanceToSpawn
	{
		get
		{
			return partChanceToSpawn;
		}
		set
		{
			if (!Mathf.Approximately((int)rotations, value))
			{
				partChanceToSpawn = value;
				partDirty = true;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		markerType = MarkerTypes.None;
		tags = FastTags<TagGroup.Poi>.none;
		groupId = -1;
		groupName = "new";
		color = default(Color);
		partToSpawn = null;
		rotations = 0;
		partChanceToSpawn = 0f;
		partDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(PrefabVolumeAbs<Marker> _target, bool _nonBasicOnly = false)
	{
		if (!_nonBasicOnly)
		{
			base.CopyValues(_target);
		}
		Marker obj = (Marker)_target;
		obj.MarkerType = MarkerType;
		obj.GroupName = GroupName;
		obj.Tags = Tags;
		obj.PartToSpawn = PartToSpawn;
		obj.Rotations = Rotations;
		obj.PartChanceToSpawn = PartChanceToSpawn;
	}

	public override void Read(PooledBinaryReader _br)
	{
		base.Read(_br);
		MarkerType = (MarkerTypes)_br.ReadByte();
		GroupName = StreamUtils.ReadString(_br);
		Tags = FastTags<TagGroup.Poi>.Parse(_br.ReadString());
		PartToSpawn = StreamUtils.ReadString(_br);
		Rotations = _br.ReadByte();
		PartChanceToSpawn = _br.ReadSingle();
	}

	public override void Write(PooledBinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write((byte)MarkerType);
		StreamUtils.Write(_bw, GroupName);
		_bw.Write(Tags.ToString());
		StreamUtils.Write(_bw, PartToSpawn);
		_bw.Write(Rotations);
		_bw.Write(PartChanceToSpawn);
	}
}
