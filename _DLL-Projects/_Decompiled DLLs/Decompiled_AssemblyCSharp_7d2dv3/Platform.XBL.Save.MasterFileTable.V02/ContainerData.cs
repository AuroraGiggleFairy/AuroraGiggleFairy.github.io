using System;

namespace Platform.XBL.Save.MasterFileTable.V02;

public sealed class ContainerData : IDisposable, IMigratable
{
	public const ushort VERSION = 2;

	public ushort Version => 2;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Node RootNode
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Node();

	public void Write(PooledBinaryWriter writer)
	{
		writer.Write((ushort)2);
		RootNode.Write(writer);
	}

	public void Read(PooledBinaryReader reader)
	{
		reader.ReadUInt16();
		RootNode.Read(reader);
	}

	public void Dispose()
	{
		RootNode?.Dispose();
		RootNode = null;
	}
}
