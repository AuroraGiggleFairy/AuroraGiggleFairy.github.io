using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MultiLookStorage
{
	public enum MultiLookOp : byte
	{
		Swap = 1,
		Half = 2,
		DropOne = 3,
		Take = 4,
		TakeAll = 5,
		Sort = 6,
		Deposit = 7,
		PutBack = 8
	}

	public struct DepositStack
	{
		public short Slot;
		public ItemStack Stack;
	}

	public struct DepositTaken
	{
		public short Slot;
		public int Type;
		public int Count;
	}

	[UnityEngine.Scripting.Preserve]
	public abstract class NetPackageMultiLookRequest : NetPackage
	{
		public MultiLookOp Op;
		public Vector3i Pos;
		public int Slot;
		public int RequestId;
		public ItemStack Expected = ItemStack.Empty;
		public ItemStack Offered = ItemStack.Empty;
		public List<DepositStack> Deposit = new List<DepositStack>();
		public List<ItemStack> PutBack = new List<ItemStack>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public NetPackageMultiLookRequest Setup(MultiLookOp op, Vector3i pos, int slot, int requestId, ItemStack expected, ItemStack offered)
		{
			Op = op;
			Pos = pos;
			Slot = slot;
			RequestId = requestId;
			Expected = expected ?? ItemStack.Empty;
			Offered = offered ?? ItemStack.Empty;
			Deposit.Clear();
			PutBack.Clear();
			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			Op = (MultiLookOp)reader.ReadByte();
			Pos = StreamUtils.ReadVector3i(reader);
			Slot = reader.ReadInt32();
			RequestId = reader.ReadInt32();
			Expected = ReadStack(reader);
			Offered = ReadStack(reader);
			Deposit.Clear();
			int depositCount = reader.ReadInt16();
			for (int i = 0; i < depositCount; i++)
			{
				Deposit.Add(new DepositStack
				{
					Slot = reader.ReadInt16(),
					Stack = ReadStack(reader)
				});
			}

			PutBack.Clear();
			int putCount = reader.ReadInt16();
			for (int i = 0; i < putCount; i++)
			{
				PutBack.Add(ReadStack(reader));
			}
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			BinaryWriter raw = writer;
			raw.Write((byte)Op);
			StreamUtils.Write(writer, Pos);
			raw.Write(Slot);
			raw.Write(RequestId);
			WriteStack(raw, Expected);
			WriteStack(raw, Offered);
			raw.Write((short)Deposit.Count);
			for (int i = 0; i < Deposit.Count; i++)
			{
				raw.Write(Deposit[i].Slot);
				WriteStack(raw, Deposit[i].Stack);
			}

			raw.Write((short)PutBack.Count);
			for (int i = 0; i < PutBack.Count; i++)
			{
				WriteStack(raw, PutBack[i]);
			}
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			_ = callbacks;
			if (world == null || Sender == null)
			{
				return;
			}

			MultiLookServer.Handle(world, Sender.entityId, this);
		}

		// 3.2 NetPackage.GetLength. Not an override: 3.3 removed that method. PackageEmit adds the override on 3.2.
		public int GetLength()
		{
			return 64 + Deposit.Count * 32 + PutBack.Count * 32;
		}

		public static ItemStack ReadStack(PooledBinaryReader reader)
		{
			ItemStack stack = ItemStack.Empty.Clone();
			StackAccess.Read(stack, reader);
			return stack;
		}

		public static void WriteStack(BinaryWriter writer, ItemStack stack)
		{
			StackAccess.Write((stack ?? ItemStack.Empty).Clone(), writer);
		}
	}

	[UnityEngine.Scripting.Preserve]
	public abstract class NetPackageMultiLookResult : NetPackage
	{
		public MultiLookOp Op;
		public bool Accepted;
		public int RequestId;
		public ItemStack Cursor = ItemStack.Empty;
		public List<ItemStack> Grants = new List<ItemStack>();
		public List<DepositTaken> Taken = new List<DepositTaken>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageMultiLookResult Setup(MultiLookOp op, bool accepted, int requestId, ItemStack cursor)
		{
			Op = op;
			Accepted = accepted;
			RequestId = requestId;
			Cursor = cursor ?? ItemStack.Empty;
			Grants.Clear();
			Taken.Clear();
			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			Op = (MultiLookOp)reader.ReadByte();
			Accepted = reader.ReadBoolean();
			RequestId = reader.ReadInt32();
			Cursor = NetPackageMultiLookRequest.ReadStack(reader);
			Grants.Clear();
			int grantCount = reader.ReadInt16();
			for (int i = 0; i < grantCount; i++)
			{
				Grants.Add(NetPackageMultiLookRequest.ReadStack(reader));
			}

			Taken.Clear();
			int takenCount = reader.ReadInt16();
			for (int i = 0; i < takenCount; i++)
			{
				Taken.Add(new DepositTaken
				{
					Slot = reader.ReadInt16(),
					Type = reader.ReadInt32(),
					Count = reader.ReadInt32()
				});
			}
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			BinaryWriter raw = writer;
			raw.Write((byte)Op);
			raw.Write(Accepted);
			raw.Write(RequestId);
			NetPackageMultiLookRequest.WriteStack(raw, Cursor);
			raw.Write((short)Grants.Count);
			for (int i = 0; i < Grants.Count; i++)
			{
				NetPackageMultiLookRequest.WriteStack(raw, Grants[i]);
			}

			raw.Write((short)Taken.Count);
			for (int i = 0; i < Taken.Count; i++)
			{
				raw.Write(Taken[i].Slot);
				raw.Write(Taken[i].Type);
				raw.Write(Taken[i].Count);
			}
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			_ = world;
			_ = callbacks;
			MultiLookClient.Apply(this);
		}

		// 3.2 NetPackage.GetLength. Not an override: 3.3 removed that method. PackageEmit adds the override on 3.2.
		public int GetLength()
		{
			return 64 + Grants.Count * 32 + Taken.Count * 12;
		}
	}
}
