using System;
using System.Collections.Generic;
using System.IO;
using Platform.XBL.Save.MasterFileTable.Latest;
using Platform.XBL.Save.MasterFileTable.V01;
using Platform.XBL.Save.MasterFileTable.V02;
using Platform.XBL.Save.MasterFileTable.V03;
using Platform.XBL.Save.MasterFileTable.V04;
using Platform.XBL.Save.MasterFileTable.V05;

namespace Platform.XBL.Save.MasterFileTable;

public static class Migrator
{
	public static readonly Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> s_containerDataMigrators = new Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>>
	{
		{ 1, MigrateFromV1 },
		{ 2, MigrateFromV2 },
		{ 3, MigrateFromV3 },
		{ 4, MigrateFromV4 },
		{ 5, MigrateFromV5ContainerData }
	};

	public static readonly Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> s_nodeMigrators = new Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> { { 5, MigrateFromV5Node } };

	public static readonly Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> s_blobRefMigrators = new Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> { { 5, MigrateFromV5BlobRef } };

	public static void ReadMigrate(Stream inputStream, IMigratable newMigratable, string identifier, Dictionary<ushort, Action<PooledBinaryReader, PooledBinaryWriter>> migrators)
	{
		ushort version = newMigratable.Version;
		MemoryStream memoryStream = null;
		MemoryStream memoryStream2 = null;
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
		pooledBinaryReader.SetBaseStream(inputStream);
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
		while (true)
		{
			long position = pooledBinaryReader.BaseStream.Position;
			ushort num = pooledBinaryReader.ReadUInt16();
			pooledBinaryReader.BaseStream.Position = position;
			if (num > version)
			{
				Log.Out(string.Format("[{0}] Newer {1} is being loaded. V{2} > V{3}", "Migrator", identifier, num, version));
			}
			if (num >= version)
			{
				break;
			}
			if (memoryStream2 == null)
			{
				memoryStream2 = new MemoryStream();
			}
			pooledBinaryWriter.SetBaseStream(memoryStream2);
			pooledBinaryWriter.BaseStream.Position = 0L;
			migrators[num](pooledBinaryReader, pooledBinaryWriter);
			MemoryStream memoryStream3 = memoryStream2;
			MemoryStream memoryStream4 = memoryStream;
			memoryStream = memoryStream3;
			memoryStream2 = memoryStream4;
			pooledBinaryReader.SetBaseStream(memoryStream);
			pooledBinaryReader.BaseStream.Position = 0L;
		}
		newMigratable.Read(pooledBinaryReader);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNode(Platform.XBL.Save.MasterFileTable.V01.Node oldNode, Platform.XBL.Save.MasterFileTable.V02.Node newNode)
	{
		newNode.LastWriteTimeUtc = Platform.XBL.Save.MasterFileTable.V02.Node.DefaultLastWriteTime;
		foreach (ulong blobId in oldNode.BlobIds)
		{
			newNode.BlobIds.Add(blobId);
		}
		foreach (Platform.XBL.Save.MasterFileTable.V01.Node value in oldNode.Children.Values)
		{
			Platform.XBL.Save.MasterFileTable.V02.Node orCreateChildNode = newNode.GetOrCreateChildNode(value.Name);
			MigrateNode(value, orCreateChildNode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateContainerData(Platform.XBL.Save.MasterFileTable.V01.ContainerData oldContainerData, Platform.XBL.Save.MasterFileTable.V02.ContainerData newContainerData)
	{
		MigrateNode(oldContainerData.RootNode, newContainerData.RootNode);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void MigrateFromV1(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V01.ContainerData containerData = new Platform.XBL.Save.MasterFileTable.V01.ContainerData();
		using Platform.XBL.Save.MasterFileTable.V02.ContainerData containerData2 = new Platform.XBL.Save.MasterFileTable.V02.ContainerData();
		containerData.Read(reader);
		MigrateContainerData(containerData, containerData2);
		containerData2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNode(Platform.XBL.Save.MasterFileTable.V02.Node oldNode, Platform.XBL.Save.MasterFileTable.V03.Node newNode)
	{
		newNode.Attributes = (oldNode.IsDirectory() ? Platform.XBL.Save.MasterFileTable.V03.NodeAttributes.Directory : Platform.XBL.Save.MasterFileTable.V03.NodeAttributes.None);
		newNode.SetBlobIds(oldNode.BlobIds.ToArray());
		foreach (Platform.XBL.Save.MasterFileTable.V02.Node value in oldNode.Children.Values)
		{
			Platform.XBL.Save.MasterFileTable.V03.Node orCreateChildNode = newNode.GetOrCreateChildNode(value.Name, createDirectory: false);
			MigrateNode(value, orCreateChildNode);
		}
		newNode.LastWriteTimeUtc = oldNode.LastWriteTimeUtc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateContainerData(Platform.XBL.Save.MasterFileTable.V02.ContainerData oldContainerData, Platform.XBL.Save.MasterFileTable.V03.ContainerData newContainerData)
	{
		MigrateNode(oldContainerData.RootNode, newContainerData.RootNode);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void MigrateFromV2(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V02.ContainerData containerData = new Platform.XBL.Save.MasterFileTable.V02.ContainerData();
		using Platform.XBL.Save.MasterFileTable.V03.ContainerData containerData2 = new Platform.XBL.Save.MasterFileTable.V03.ContainerData();
		containerData.Read(reader);
		MigrateContainerData(containerData, containerData2);
		containerData2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNodeAttributes(Platform.XBL.Save.MasterFileTable.V03.NodeAttributes oldAttributes, out Platform.XBL.Save.MasterFileTable.V04.NodeAttributes newAttributes)
	{
		newAttributes = Platform.XBL.Save.MasterFileTable.V04.NodeAttributes.None;
		if (oldAttributes.HasFlag(Platform.XBL.Save.MasterFileTable.V03.NodeAttributes.Directory))
		{
			newAttributes |= Platform.XBL.Save.MasterFileTable.V04.NodeAttributes.Directory;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateBlobRef(ulong oldBlobRef, out Platform.XBL.Save.MasterFileTable.V04.BlobRef newBlobRef)
	{
		newBlobRef = new Platform.XBL.Save.MasterFileTable.V04.BlobRef(oldBlobRef, 0u);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNode(Platform.XBL.Save.MasterFileTable.V03.Node oldNode, Platform.XBL.Save.MasterFileTable.V04.Node newNode)
	{
		MigrateNodeAttributes(oldNode.Attributes, out newNode.Attributes);
		IReadOnlyList<ulong> blobIds = oldNode.BlobIds;
		Platform.XBL.Save.MasterFileTable.V04.BlobRef[] array = new Platform.XBL.Save.MasterFileTable.V04.BlobRef[blobIds.Count];
		for (int i = 0; i < blobIds.Count; i++)
		{
			MigrateBlobRef(blobIds[i], out array[i]);
		}
		newNode.SetBlobRefs(array);
		foreach (Platform.XBL.Save.MasterFileTable.V03.Node value in oldNode.Children.Values)
		{
			Platform.XBL.Save.MasterFileTable.V04.Node orCreateChildNode = newNode.GetOrCreateChildNode(value.Name, value.IsDirectory());
			MigrateNode(value, orCreateChildNode);
		}
		newNode.LastWriteTimeUtc = oldNode.LastWriteTimeUtc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateContainerData(Platform.XBL.Save.MasterFileTable.V03.ContainerData oldContainerData, Platform.XBL.Save.MasterFileTable.V04.ContainerData newContainerData)
	{
		MigrateNode(oldContainerData.RootNode, newContainerData.RootNode);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void MigrateFromV3(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V03.ContainerData containerData = new Platform.XBL.Save.MasterFileTable.V03.ContainerData();
		using Platform.XBL.Save.MasterFileTable.V04.ContainerData containerData2 = new Platform.XBL.Save.MasterFileTable.V04.ContainerData();
		containerData.Read(reader);
		MigrateContainerData(containerData, containerData2);
		containerData2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNodeAttributes(Platform.XBL.Save.MasterFileTable.V04.NodeAttributes oldAttributes, out Platform.XBL.Save.MasterFileTable.V05.NodeAttributes newAttributes)
	{
		newAttributes = Platform.XBL.Save.MasterFileTable.V05.NodeAttributes.None;
		if (oldAttributes.HasFlag(Platform.XBL.Save.MasterFileTable.V04.NodeAttributes.Directory))
		{
			newAttributes |= Platform.XBL.Save.MasterFileTable.V05.NodeAttributes.Directory;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateBlobRef(Platform.XBL.Save.MasterFileTable.V04.BlobRef oldBlobRef, out Platform.XBL.Save.MasterFileTable.V05.BlobRef newBlobRef)
	{
		newBlobRef = new Platform.XBL.Save.MasterFileTable.V05.BlobRef
		{
			Id = oldBlobRef.Id,
			Length = oldBlobRef.Length,
			Hash = oldBlobRef.Hash
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNode(Platform.XBL.Save.MasterFileTable.V04.Node oldNode, Platform.XBL.Save.MasterFileTable.V05.Node newNode)
	{
		MigrateNodeAttributes(oldNode.Attributes, out newNode.Attributes);
		IReadOnlyList<Platform.XBL.Save.MasterFileTable.V04.BlobRef> blobRefs = oldNode.BlobRefs;
		Platform.XBL.Save.MasterFileTable.V05.BlobRef[] array = new Platform.XBL.Save.MasterFileTable.V05.BlobRef[blobRefs.Count];
		for (int i = 0; i < blobRefs.Count; i++)
		{
			MigrateBlobRef(blobRefs[i], out array[i]);
		}
		newNode.SetBlobRefs(array);
		foreach (Platform.XBL.Save.MasterFileTable.V04.Node value in oldNode.Children.Values)
		{
			Platform.XBL.Save.MasterFileTable.V05.Node orCreateChildNode = newNode.GetOrCreateChildNode(value.Name, value.IsDirectory());
			MigrateNode(value, orCreateChildNode);
		}
		newNode.LastWriteTimeUtc = oldNode.LastWriteTimeUtc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateContainerData(Platform.XBL.Save.MasterFileTable.V04.ContainerData oldContainerData, Platform.XBL.Save.MasterFileTable.V05.ContainerData newContainerData)
	{
		MigrateNode(oldContainerData.RootNode, newContainerData.RootNode);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void MigrateFromV4(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V04.ContainerData containerData = new Platform.XBL.Save.MasterFileTable.V04.ContainerData();
		using Platform.XBL.Save.MasterFileTable.V05.ContainerData containerData2 = new Platform.XBL.Save.MasterFileTable.V05.ContainerData();
		containerData.Read(reader);
		MigrateContainerData(containerData, containerData2);
		containerData2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNodeAttributes(Platform.XBL.Save.MasterFileTable.V05.NodeAttributes oldAttributes, out Platform.XBL.Save.MasterFileTable.Latest.NodeAttributes newAttributes)
	{
		newAttributes = Platform.XBL.Save.MasterFileTable.Latest.NodeAttributes.None;
		if (oldAttributes.HasFlag(Platform.XBL.Save.MasterFileTable.V05.NodeAttributes.Directory))
		{
			newAttributes |= Platform.XBL.Save.MasterFileTable.Latest.NodeAttributes.Directory;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateBlobRef(Platform.XBL.Save.MasterFileTable.V05.BlobRef oldBlobRef, Platform.XBL.Save.MasterFileTable.Latest.BlobRef newBlobRef)
	{
		newBlobRef.Id = oldBlobRef.Id;
		newBlobRef.Length = oldBlobRef.Length;
		newBlobRef.Hash = oldBlobRef.Hash;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateNode(Platform.XBL.Save.MasterFileTable.V05.Node oldNode, Platform.XBL.Save.MasterFileTable.Latest.Node newNode)
	{
		MigrateNodeAttributes(oldNode.Attributes, out newNode.Attributes);
		IReadOnlyList<Platform.XBL.Save.MasterFileTable.V05.BlobRef> blobRefs = oldNode.BlobRefs;
		Platform.XBL.Save.MasterFileTable.Latest.BlobRef[] array = new Platform.XBL.Save.MasterFileTable.Latest.BlobRef[blobRefs.Count];
		for (int i = 0; i < blobRefs.Count; i++)
		{
			array[i] = new Platform.XBL.Save.MasterFileTable.Latest.BlobRef();
			MigrateBlobRef(blobRefs[i], array[i]);
		}
		newNode.SetBlobRefs(array);
		foreach (Platform.XBL.Save.MasterFileTable.V05.Node value in oldNode.Children.Values)
		{
			Platform.XBL.Save.MasterFileTable.Latest.Node orCreateChildNode = newNode.GetOrCreateChildNode(value.Name, value.IsDirectory());
			MigrateNode(value, orCreateChildNode);
		}
		newNode.CreationTimeUtc = oldNode.LastWriteTimeUtc;
		newNode.LastWriteTimeUtc = oldNode.LastWriteTimeUtc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateContainerData(Platform.XBL.Save.MasterFileTable.V05.ContainerData oldContainerData, Platform.XBL.Save.MasterFileTable.Latest.ContainerData newContainerData)
	{
		MigrateNode(oldContainerData.RootNode, newContainerData.RootNode);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void MigrateFromV5ContainerData(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V05.ContainerData containerData = new Platform.XBL.Save.MasterFileTable.V05.ContainerData();
		using Platform.XBL.Save.MasterFileTable.Latest.ContainerData containerData2 = new Platform.XBL.Save.MasterFileTable.Latest.ContainerData();
		containerData.Read(reader);
		MigrateContainerData(containerData, containerData2);
		containerData2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateFromV5Node(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		using Platform.XBL.Save.MasterFileTable.V05.Node node = new Platform.XBL.Save.MasterFileTable.V05.Node();
		using Platform.XBL.Save.MasterFileTable.Latest.Node node2 = new Platform.XBL.Save.MasterFileTable.Latest.Node();
		node.Read(reader);
		MigrateNode(node, node2);
		node2.Write(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MigrateFromV5BlobRef(PooledBinaryReader reader, PooledBinaryWriter writer)
	{
		Platform.XBL.Save.MasterFileTable.V05.BlobRef blobRef = new Platform.XBL.Save.MasterFileTable.V05.BlobRef();
		Platform.XBL.Save.MasterFileTable.Latest.BlobRef blobRef2 = new Platform.XBL.Save.MasterFileTable.Latest.BlobRef();
		blobRef.Read(reader);
		MigrateBlobRef(blobRef, blobRef2);
		blobRef2.Write(writer);
	}
}
