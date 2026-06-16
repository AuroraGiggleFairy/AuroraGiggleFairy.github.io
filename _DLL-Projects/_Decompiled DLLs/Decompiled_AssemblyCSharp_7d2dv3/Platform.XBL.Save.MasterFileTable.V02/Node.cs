using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save.MasterFileTable.V02;

public sealed class Node : IDisposable
{
	public static readonly DateTime DefaultLastWriteTime = new DateTime(0L, DateTimeKind.Utc);

	public DateTime LastWriteTimeUtc;

	public readonly List<ulong> BlobIds;

	public readonly SortedDictionary<string, Node> Children;

	public readonly object m_lock;

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
		LastWriteTimeUtc = DefaultLastWriteTime;
		BlobIds = new List<ulong>();
		Children = new SortedDictionary<string, Node>();
		m_lock = new object();
		Parent = null;
	}

	public override string ToString()
	{
		lock (m_lock)
		{
			return string.Format("{0}[{1}=\"{2}\", {3}=[{4}], #{5}={6}]", "Node", "Name", Name, "BlobIds", string.Join(", ", BlobIds.Select(SaveContainer.IdToString)), "Children", Children.Count);
		}
	}

	public void Write(PooledBinaryWriter writer)
	{
		lock (m_lock)
		{
			writer.Write(Name);
			writer.Write(LastWriteTimeUtc.Ticks);
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
	}

	public void Read(PooledBinaryReader reader)
	{
		lock (m_lock)
		{
			Name = reader.ReadString();
			LastWriteTimeUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
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
	}

	public void Dispose()
	{
		lock (m_lock)
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
	}

	public bool IsDirectory()
	{
		lock (m_lock)
		{
			return BlobIds.Count == 0;
		}
	}

	public bool IsFile()
	{
		lock (m_lock)
		{
			return Children.Count == 0;
		}
	}

	public Node GetChildNode(string name)
	{
		lock (m_lock)
		{
			if (!Children.TryGetValue(name, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public Node GetOrCreateChildNode(string name)
	{
		lock (m_lock)
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

	public IEnumerable<Node> Enumerate(bool includeSelf, bool recursive)
	{
		List<Node> list = new List<Node>();
		if (!recursive)
		{
			if (includeSelf)
			{
				list.Add(this);
			}
			lock (m_lock)
			{
				list.AddRange(Children.Values);
			}
		}
		else
		{
			Stack<Node> stack = new Stack<Node>();
			if (includeSelf)
			{
				stack.Push(this);
			}
			else
			{
				AddNodeChildrenToStack(stack, this);
			}
			while (stack.Count > 0)
			{
				Node node = stack.Pop();
				list.Add(node);
				AddNodeChildrenToStack(stack, node);
			}
		}
		return list;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void AddNodeChildrenToStack(Stack<Node> stack2, Node currentNode)
		{
			lock (currentNode.m_lock)
			{
				foreach (Node item in currentNode.Children.Values.Reverse())
				{
					stack2.Push(item);
				}
			}
		}
	}
}
