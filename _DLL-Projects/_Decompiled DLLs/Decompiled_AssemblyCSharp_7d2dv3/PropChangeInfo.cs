using System;
using System.IO;
using UnityEngine;

public class PropChangeInfo
{
	[Flags]
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Flags : byte
	{
		HasPropId = 1,
		HasPosition = 2,
		HasRotation = 4,
		HasScale = 8,
		HasBlockValue = 0x10
	}

	public class Builder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i ChunkPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int? PropId;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3? Position;

		[PublicizedFrom(EAccessModifier.Private)]
		public Quaternion? Rotation;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3? Scale;

		[PublicizedFrom(EAccessModifier.Private)]
		public BlockValue? BlockValue;

		public Builder(int chunkX, int chunkZ)
			: this(new Vector2i(chunkX, chunkZ))
		{
		}

		public Builder(Vector2i chunkPos)
		{
			ChunkPos = chunkPos;
		}

		public Builder(PropRef propRef)
		{
			ChunkPos = propRef.ChunkPos;
			PropId = propRef.PropId;
		}

		public PropChangeInfo Build()
		{
			return new PropChangeInfo(ChunkPos, PropId, Position, Rotation, Scale, BlockValue);
		}

		public Builder SetPropId(int propId)
		{
			PropId = propId;
			return this;
		}

		public Builder SetPosition(Vector3 position)
		{
			Position = position;
			return this;
		}

		public Builder SetRotation(Quaternion rotation)
		{
			Rotation = rotation;
			return this;
		}

		public Builder SetScale(Vector3 scale)
		{
			Scale = scale;
			return this;
		}

		public Builder SetBlockValue(BlockValue blockValue)
		{
			BlockValue = blockValue;
			return this;
		}
	}

	public static readonly PropChangeInfo Empty = new Builder(Vector2i.zero).Build();

	public readonly Vector2i ChunkPos;

	public readonly int? PropId;

	public readonly Vector3? Position;

	public readonly Quaternion? Rotation;

	public readonly Vector3? Scale;

	public readonly BlockValue? BlockValue;

	public PropChangeInfo(Vector2i chunkPos, int? propId, Vector3? position, Quaternion? rotation, Vector3? scale, BlockValue? blockValue)
	{
		ChunkPos = chunkPos;
		PropId = propId;
		Position = position;
		Rotation = rotation;
		Scale = scale;
		BlockValue = blockValue;
	}

	public static PropChangeInfo Read(BinaryReader br)
	{
		Builder builder = new Builder(br.ReadInt32(), br.ReadInt32());
		byte num = br.ReadByte();
		if ((num & 1) == 1)
		{
			builder.SetPropId(br.ReadInt32());
		}
		if ((num & 2) == 2)
		{
			builder.SetPosition(StreamUtils.ReadVector3(br));
		}
		if ((num & 4) == 4)
		{
			builder.SetRotation(StreamUtils.ReadQuaterion(br));
		}
		if ((num & 8) == 8)
		{
			builder.SetScale(StreamUtils.ReadVector3(br));
		}
		if ((num & 0x10) == 16)
		{
			builder.SetBlockValue(new BlockValue
			{
				rawData = br.ReadUInt32(),
				damage = br.ReadUInt16()
			});
		}
		return builder.Build();
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(ChunkPos.x);
		bw.Write(ChunkPos.y);
		Flags value = (Flags)((PropId.HasValue ? 1 : 0) | (Position.HasValue ? 2 : 0) | (Rotation.HasValue ? 4 : 0) | (Scale.HasValue ? 8 : 0) | (BlockValue.HasValue ? 16 : 0));
		bw.Write((byte)value);
		if (PropId.HasValue)
		{
			bw.Write(PropId.Value);
		}
		if (Position.HasValue)
		{
			StreamUtils.Write(bw, Position.Value);
		}
		if (Rotation.HasValue)
		{
			StreamUtils.Write(bw, Rotation.Value);
		}
		if (Scale.HasValue)
		{
			StreamUtils.Write(bw, Scale.Value);
		}
		if (BlockValue.HasValue)
		{
			bw.Write(BlockValue.Value.rawData);
			bw.Write((ushort)BlockValue.Value.damage);
		}
	}

	public override string ToString()
	{
		return $"ChunkId={ChunkPos}, PropId={PropId}, Position={Position}, Rotation={Rotation}, Scale={Scale}, BlockValue={BlockValue}";
	}
}
