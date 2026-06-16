using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save.MasterFileTable.V03;

public sealed class Node : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DateTime DefaultLastWriteTime = new DateTime(0L, DateTimeKind.Utc);

	public DateTime LastWriteTimeUtc;

	public NodeAttributes Attributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong[] m_blobIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringSpanDictionary<Node> m_children;

	public readonly object m_blobLock;

	public readonly object m_lock;

	public Node Parent;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IReadOnlyList<ulong> BlobIds => m_blobIds;

	public IReadOnlyDictionary<string, Node> Children => m_children;

	public Node()
	{
		Name = string.Empty;
		LastWriteTimeUtc = DefaultLastWriteTime;
		Attributes = NodeAttributes.None;
		m_blobIds = Array.Empty<ulong>();
		m_blobLock = new object();
		m_children = new StringSpanDictionary<Node>(new SortedDictionary<string, Node>());
		m_lock = new object();
		Parent = null;
	}

	public void SetBlobIds(ulong[] blobIds)
	{
		lock (m_lock)
		{
			m_blobIds = blobIds;
		}
	}

	public override string ToString()
	{
		lock (m_lock)
		{
			return string.Format("{0}[{1}=\"{2}\", {3}=\"{4}\", {5}=\"{6}\", {7}=[{8}], #{9}={10}]", "Node", "Name", Name, "LastWriteTimeUtc", LastWriteTimeUtc, "Attributes", Attributes, "BlobIds", string.Join(", ", BlobIds.Select(SaveContainer.IdToString)), "Children", Children.Count);
		}
	}

	public void Write(PooledBinaryWriter writer)
	{
		lock (m_lock)
		{
			writer.Write(Name);
			writer.Write(LastWriteTimeUtc.Ticks);
			writer.Write(Convert.ToUInt32(Attributes));
			ulong[] blobIds = m_blobIds;
			ushort value = (ushort)blobIds.Length;
			writer.Write(value);
			ulong[] array = blobIds;
			foreach (ulong value2 in array)
			{
				writer.Write(value2);
			}
			ushort value3 = (ushort)Children.Count;
			writer.Write(value3);
			foreach (Node value4 in Children.Values)
			{
				value4.Write(writer);
			}
		}
	}

	public void Read(PooledBinaryReader reader)
	{
		lock (m_lock)
		{
			Name = reader.ReadString();
			LastWriteTimeUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
			Attributes = (NodeAttributes)reader.ReadUInt32();
			ushort num = reader.ReadUInt16();
			ulong[] array = new ulong[num];
			for (int i = 0; i < num; i++)
			{
				ulong num2 = reader.ReadUInt64();
				array[i] = num2;
			}
			m_blobIds = array;
			ushort num3 = reader.ReadUInt16();
			for (int j = 0; j < num3; j++)
			{
				Node node = new Node();
				node.Read(reader);
				m_children.Add(node.Name, node);
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
			m_children.Clear();
			Parent = null;
			m_blobIds = Array.Empty<ulong>();
			Name = string.Empty;
		}
	}

	public bool IsDirectory()
	{
		return Attributes.HasFlag(NodeAttributes.Directory);
	}

	public bool IsFile()
	{
		return !IsDirectory();
	}

	public Node GetChildNode(StringSpan name)
	{
		lock (m_lock)
		{
			if (!m_children.TryGetValue(name, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public Node GetOrCreateChildNode(StringSpan name, bool createDirectory)
	{
		bool wasCreated;
		return GetOrCreateChildNode(name, createDirectory, out wasCreated);
	}

	public Node GetOrCreateChildNode(StringSpan name, bool createDirectory, out bool wasCreated)
	{
		lock (m_lock)
		{
			if (m_children.TryGetValue(name, out var value))
			{
				wasCreated = false;
				return value;
			}
			string text = name.ToString();
			value = new Node
			{
				Name = text
			};
			value.Parent = this;
			m_children.Add(text, value);
			LastWriteTimeUtc = (value.LastWriteTimeUtc = DateTime.UtcNow);
			Attributes |= NodeAttributes.Directory;
			if (createDirectory)
			{
				value.Attributes |= NodeAttributes.Directory;
			}
			wasCreated = true;
			return value;
		}
	}

	public Node DeleteChildNode(StringSpan name)
	{
		lock (m_lock)
		{
			if (!m_children.TryGetValue(name, out var value) || !m_children.Remove(name))
			{
				return null;
			}
			LastWriteTimeUtc = DateTime.UtcNow;
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

	public void MoveChild(Node child)
	{
		MoveChild(child, child.Name);
	}

	public void MoveChild(Node child, StringSpan newName)
	{
		lock (m_lock)
		{
			lock (child.m_lock)
			{
				Node parent = child.Parent;
				if (parent == null || parent == this)
				{
					MoveChildInternal(parent, this, child, newName);
					return;
				}
				lock (parent.m_lock)
				{
					MoveChildInternal(parent, this, child, newName);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void MoveChildInternal(Node oldParent, Node newParent, Node node, StringSpan stringSpan)
		{
			if (oldParent != newParent || !(node.Name == stringSpan))
			{
				DateTime utcNow = DateTime.UtcNow;
				if (oldParent != null)
				{
					oldParent.m_children.Remove(node.Name);
					oldParent.LastWriteTimeUtc = utcNow;
				}
				if (node.Name != stringSpan)
				{
					node.Name = stringSpan.ToString();
				}
				node.Parent = newParent;
				newParent.m_children.Remove(node.Name, out var value);
				if (value != null && value != node)
				{
					value.Dispose();
				}
				newParent.m_children.Add(node.Name, node);
				newParent.LastWriteTimeUtc = utcNow;
			}
		}
	}
}
