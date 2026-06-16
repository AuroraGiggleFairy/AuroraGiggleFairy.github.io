using System;
using System.IO;

namespace Platform.XBL.Save.MasterFileTable.V05;

public sealed class ContainerData : IDisposable, IMigratable
{
	public const ushort VERSION = 5;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ushort Version
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 5;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte[] FutureData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = Array.Empty<byte>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Node RootNode
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Node
	{
		Attributes = NodeAttributes.Directory
	};

	public void Write(PooledBinaryWriter writer)
	{
		RootNode.Attributes = NodeAttributes.Directory;
		writer.Write(Version);
		int value = 0;
		long position = writer.BaseStream.Position;
		writer.Write(value);
		long position2 = writer.BaseStream.Position;
		writer.Write(FutureData);
		long position3 = writer.BaseStream.Position;
		value = (int)(position3 - position2);
		writer.BaseStream.Position = position;
		writer.Write(value);
		writer.BaseStream.Position = position3;
		RootNode.Write(writer);
	}

	public void Read(PooledBinaryReader reader)
	{
		Version = reader.ReadUInt16();
		int num = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		long position2 = reader.BaseStream.Position;
		FutureData = new byte[(int)(num - (position2 - position))];
		if (!reader.TryReadAllBytes(FutureData, out var totalBytesRead))
		{
			throw new IOException($"Expected {FutureData.Length} bytes to be read for future data but only got {totalBytesRead} bytes.");
		}
		Migrator.ReadMigrate(reader.BaseStream, RootNode, "Node", Migrator.s_nodeMigrators);
		RootNode.Attributes = NodeAttributes.Directory;
	}

	public void Dispose()
	{
		RootNode?.Dispose();
		RootNode = null;
	}
}
