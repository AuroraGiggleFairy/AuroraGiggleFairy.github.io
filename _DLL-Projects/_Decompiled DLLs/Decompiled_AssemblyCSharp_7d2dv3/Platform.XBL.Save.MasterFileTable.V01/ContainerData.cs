using System;

namespace Platform.XBL.Save.MasterFileTable.V01;

public sealed class ContainerData : IDisposable, IMigratable
{
	public const ushort VERSION = 1;

	public ushort Version => 1;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Node RootNode
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Node();

	public void Write(PooledBinaryWriter writer)
	{
		writer.Write((ushort)1);
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
