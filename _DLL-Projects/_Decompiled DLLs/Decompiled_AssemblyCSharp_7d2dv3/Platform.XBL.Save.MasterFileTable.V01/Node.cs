using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save.MasterFileTable.V01;

public sealed class Node : IDisposable
{
	public readonly List<ulong> BlobIds;

	public readonly SortedDictionary<string, Node> Children;

	public Node Parent;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Node()
	{
		Name = string.Empty;
		BlobIds = new List<ulong>();
		Children = new SortedDictionary<string, Node>();
		Parent = null;
	}

	public override string ToString()
	{
		return string.Format("{0}[{1}=\"{2}\", {3}=[{4}], #{5}={6}]", "Node", "Name", Name, "BlobIds", string.Join(", ", BlobIds.Select(SaveContainer.IdToString)), "Children", Children.Count);
	}

	public void Write(PooledBinaryWriter writer)
	{
		writer.Write(Name);
		ushort value = (ushort)BlobIds.Count;
		writer.Write(value);
		foreach (ulong blobId in BlobIds)
		{
			writer.Write(blobId);
		}
		ushort value2 = (ushort)Children.Count;
		writer.Write(value2);
		foreach (Node value3 in Children.Values)
		{
			value3.Write(writer);
		}
	}

	public void Read(PooledBinaryReader reader)
	{
		Name = reader.ReadString();
		ushort num = reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			ulong item = reader.ReadUInt64();
			BlobIds.Add(item);
		}
		ushort num2 = reader.ReadUInt16();
		for (int j = 0; j < num2; j++)
		{
			Node node = new Node();
			node.Read(reader);
			Children.Add(node.Name, node);
			node.Parent = this;
		}
	}

	public void Dispose()
	{
		foreach (Node value in Children.Values)
		{
			value.Dispose();
		}
		Children.Clear();
		Parent = null;
		BlobIds.Clear();
		Name = string.Empty;
	}

	public bool IsDirectory()
	{
		return BlobIds.Count == 0;
	}

	public bool IsFile()
	{
		return Children.Count == 0;
	}

	public Node GetChildNode(string name)
	{
		if (!Children.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public Node GetOrCreateChildNode(string name)
	{
		if (!Children.TryGetValue(name, out var value))
		{
			value = new Node
			{
				Name = name
			};
			value.Parent = this;
			Children.Add(name, value);
		}
		return value;
	}
}
