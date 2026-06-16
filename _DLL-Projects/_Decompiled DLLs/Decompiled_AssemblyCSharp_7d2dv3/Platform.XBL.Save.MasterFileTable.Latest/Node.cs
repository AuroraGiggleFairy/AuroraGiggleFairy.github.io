using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.XBL.Save.MasterFileTable.Latest;

public sealed class Node : IDisposable, IMigratable
{
	public const ushort VERSION = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DateTime DefaultLastWriteTime = new DateTime(0L, DateTimeKind.Utc);

	public DateTime LastWriteTimeUtc;

	public NodeAttributes Attributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlobRef[] m_blobRefs;

	public DateTime CreationTimeUtc;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringSpanDictionary<Node> m_children;

	public readonly object m_blobLock;

	public readonly object m_lock;

	public Node Parent;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ushort Version
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 6;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IReadOnlyList<BlobRef> BlobRefs => m_blobRefs;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte[] FutureData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = Array.Empty<byte>();

	public IReadOnlyDictionary<string, Node> Children => m_children;

	public Node()
	{
		Name = string.Empty;
		CreationTimeUtc = DateTime.UtcNow;
		LastWriteTimeUtc = DefaultLastWriteTime;
		Attributes = NodeAttributes.None;
		m_blobRefs = Array.Empty<BlobRef>();
		m_blobLock = new object();
		m_children = new StringSpanDictionary<Node>(new SortedDictionary<string, Node>());
		m_lock = new object();
		Parent = null;
	}

	public void SetBlobRefs(BlobRef[] blobRefs)
	{
		lock (m_lock)
		{
			m_blobRefs = blobRefs;
		}
	}

	public override string ToString()
	{
		lock (m_lock)
		{
			return string.Format("{0}[{1}=\"{2}\", {3}=\"{4}\", {5}=\"{6}\", {7}=\"{8}\", {9}=[{10}], #{11}={12}]", "Node", "Name", Name, "CreationTimeUtc", CreationTimeUtc, "LastWriteTimeUtc", LastWriteTimeUtc, "Attributes", Attributes, "BlobRefs", string.Join(", ", BlobRefs), "Children", Children.Count);
		}
	}

	public void Write(PooledBinaryWriter writer)
	{
		lock (m_lock)
		{
			writer.Write(Version);
			int value = 0;
			long position = writer.BaseStream.Position;
			writer.Write(value);
			long position2 = writer.BaseStream.Position;
			writer.Write(Name);
			writer.Write(LastWriteTimeUtc.Ticks);
			writer.Write(Convert.ToUInt32(Attributes));
			BlobRef[] blobRefs = m_blobRefs;
			ushort value2 = (ushort)blobRefs.Length;
			writer.Write(value2);
			BlobRef[] array = blobRefs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(writer);
			}
			writer.Write(CreationTimeUtc.Ticks);
			writer.Write(FutureData);
			long position3 = writer.BaseStream.Position;
			value = (int)(position3 - position2);
			writer.BaseStream.Position = position;
			writer.Write(value);
			writer.BaseStream.Position = position3;
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
			Version = reader.ReadUInt16();
			int num = reader.ReadInt32();
			long position = reader.BaseStream.Position;
			Name = reader.ReadString();
			LastWriteTimeUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
			Attributes = (NodeAttributes)reader.ReadUInt32();
			ushort num2 = reader.ReadUInt16();
			BlobRef[] array = new BlobRef[num2];
			for (int i = 0; i < num2; i++)
			{
				BlobRef blobRef = new BlobRef();
				Migrator.ReadMigrate(reader.BaseStream, blobRef, "BlobRef", Migrator.s_blobRefMigrators);
				array[i] = blobRef;
			}
			m_blobRefs = array;
			CreationTimeUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
			long position2 = reader.BaseStream.Position;
			FutureData = new byte[(int)(num - (position2 - position))];
			if (!reader.TryReadAllBytes(FutureData, out var totalBytesRead))
			{
				throw new IOException($"Expected {FutureData.Length} bytes to be read for future data but only got {totalBytesRead} bytes.");
			}
			ushort num3 = reader.ReadUInt16();
			for (int j = 0; j < num3; j++)
			{
				Node node = new Node();
				Migrator.ReadMigrate(reader.BaseStream, node, "Node", Migrator.s_nodeMigrators);
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
			m_blobRefs = Array.Empty<BlobRef>();
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
