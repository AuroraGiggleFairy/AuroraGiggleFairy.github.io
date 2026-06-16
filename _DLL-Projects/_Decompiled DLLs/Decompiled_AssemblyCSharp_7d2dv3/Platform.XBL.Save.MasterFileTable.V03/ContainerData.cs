using System;

namespace Platform.XBL.Save.MasterFileTable.V03;

public sealed class ContainerData : IDisposable, IMigratable
{
	public const ushort VERSION = 3;

	public ushort Version => 3;

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
		writer.Write((ushort)3);
		RootNode.Write(writer);
	}

	public void Read(PooledBinaryReader reader)
	{
		reader.ReadUInt16();
		RootNode.Read(reader);
		RootNode.Attributes = NodeAttributes.Directory;
	}

	public void Dispose()
	{
		RootNode?.Dispose();
		RootNode = null;
	}
}
