using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

namespace StorageLaptop
{
	[Preserve]
	public abstract class NetPackageStorageLaptop : NetPackage
	{
		private const byte ActionQuery = 1;
		private const byte ActionPull = 2;
		private const byte ActionResult = 3;
		private const byte ActionRelease = 4;
		private const byte ActionConsume = 5;

		private byte action;
		private int requestId;
		private byte status;
		private Vector3i blockPos;
		private bool takeAll;
		private bool locked;
		private int quality;
		private int amount;
		private string itemName = string.Empty;
		private List<StorageRow> rows = new List<StorageRow>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

		public NetPackageStorageLaptop SetupQuery(Vector3i pos, int request)
		{
			action = ActionQuery;
			blockPos = pos;
			requestId = request;
			rows = new List<StorageRow>();
			itemName = string.Empty;
			return this;
		}

		public NetPackageStorageLaptop SetupPull(Vector3i pos, int request, string name, int itemQuality, bool itemLocked, Vector3i chestPos, bool all, int pullAmount)
		{
			action = ActionPull;
			blockPos = pos;
			requestId = request;
			itemName = name ?? string.Empty;
			quality = itemQuality;
			amount = pullAmount;
			locked = itemLocked;
			takeAll = all;
			rows = new List<StorageRow> { new StorageRow { ChestPos = chestPos } };
			return this;
		}

		public NetPackageStorageLaptop SetupConsume(Vector3i pos, int request, string name, int itemQuality, bool itemLocked, Vector3i chestPos, int pullAmount)
		{
			action = ActionConsume;
			blockPos = pos;
			requestId = request;
			itemName = name ?? string.Empty;
			quality = itemQuality;
			amount = pullAmount;
			locked = itemLocked;
			takeAll = false;
			rows = new List<StorageRow> { new StorageRow { ChestPos = chestPos } };
			return this;
		}

		public NetPackageStorageLaptop SetupRelease(Vector3i pos)
		{
			action = ActionRelease;
			blockPos = pos;
			rows = new List<StorageRow>();
			itemName = string.Empty;
			return this;
		}

		public NetPackageStorageLaptop SetupResult(int request, byte resultStatus, List<StorageRow> resultRows)
		{
			action = ActionResult;
			requestId = request;
			status = resultStatus;
			rows = resultRows ?? new List<StorageRow>();
			itemName = string.Empty;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			action = _br.ReadByte();
			requestId = _br.ReadInt32();
			status = _br.ReadByte();
			blockPos = StreamUtils.ReadVector3i(_br);
			takeAll = _br.ReadBoolean();
			locked = _br.ReadBoolean();
			quality = _br.ReadInt32();
			amount = _br.ReadInt32();
			itemName = _br.ReadString();
			int count = _br.ReadInt32();
			rows = new List<StorageRow>(count);
			for (int i = 0; i < count; i++)
			{
				rows.Add(new StorageRow
				{
					ItemName = _br.ReadString(),
					Quality = _br.ReadInt32(),
					Count = _br.ReadInt32(),
					Locked = _br.ReadBoolean(),
					ChestPos = StreamUtils.ReadVector3i(_br),
					ChestBlock = _br.ReadString(),
					Distance = _br.ReadInt32(),
					HasDurability = _br.ReadBoolean(),
					DurabilityFill = _br.ReadSingle()
				});
			}
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			BinaryWriter writer = _bw;
			writer.Write(action);
			writer.Write(requestId);
			writer.Write(status);
			StreamUtils.Write(writer, blockPos);
			writer.Write(takeAll);
			writer.Write(locked);
			writer.Write(quality);
			writer.Write(amount);
			writer.Write(itemName ?? string.Empty);
			int count = rows != null ? rows.Count : 0;
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				StorageRow row = rows[i];
				writer.Write(row.ItemName ?? string.Empty);
				writer.Write(row.Quality);
				writer.Write(row.Count);
				writer.Write(row.Locked);
				StreamUtils.Write(writer, row.ChestPos);
				writer.Write(row.ChestBlock ?? string.Empty);
				writer.Write(row.Distance);
				writer.Write(row.HasDurability);
				writer.Write(row.DurabilityFill);
			}
		}

		public static void Submit(NetPackageStorageLaptop package)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (package == null || manager == null)
			{
				return;
			}

			if (manager.IsServer)
			{
				package.HandleServer(GameManager.Instance?.World);
				return;
			}

			manager.SendToServer(package);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager != null && manager.IsServer)
			{
				if (action == ActionQuery || action == ActionPull || action == ActionRelease || action == ActionConsume)
				{
					HandleServer(_world);
				}

				return;
			}

			if (action == ActionResult)
			{
				StorageLaptopWindow.Receive(requestId, status, rows);
			}
		}

		// 3.2 NetPackage.GetLength. Not an override: 3.3 removed that method. PackageEmit adds the override on 3.2.
		public int GetLength()
		{
			int length = 64 + (itemName != null ? itemName.Length * 2 : 0);
			if (rows != null)
			{
				foreach (StorageRow row in rows)
				{
					length += 48;
					length += row.ItemName != null ? row.ItemName.Length * 2 : 0;
					length += row.ChestBlock != null ? row.ChestBlock.Length * 2 : 0;
				}
			}

			return length;
		}

		private void HandleServer(World world)
		{
			int senderId = Sender != null ? Sender.entityId : -1;
			if (senderId < 0 && world != null)
			{
				senderId = world.GetPrimaryPlayerId();
			}

			EntityPlayer player = world != null ? world.GetEntity(senderId) as EntityPlayer : null;
			byte resultStatus = StorageService.StatusOk;
			Vector3i chestPos = rows != null && rows.Count > 0 ? rows[0].ChestPos : Vector3i.zero;
			if (player == null || world == null || !(world.GetBlock(blockPos).Block is BlockStorageLaptop))
			{
				SendResult(player, requestId, StorageService.StatusNoPower, new List<StorageRow>());
				return;
			}

			if (action == ActionRelease)
			{
				StorageService.Release(world, blockPos, player.entityId);
				return;
			}

			if (!BlockStorageLaptop.IsPoweredAt(world, blockPos))
			{
				SendResult(player, requestId, StorageService.StatusNoPower, new List<StorageRow>());
				return;
			}

			if (!StorageService.TryClaim(world, blockPos, player.entityId))
			{
				SendResult(player, requestId, StorageService.StatusInUse, new List<StorageRow>());
				return;
			}

			if (action == ActionPull)
			{
				resultStatus = StorageService.Pull(world, blockPos, player, itemName, quality, locked, chestPos, takeAll && !locked, amount);
			}
			else if (action == ActionConsume)
			{
				resultStatus = StorageService.Consume(world, blockPos, player, itemName, quality, locked, chestPos, amount);
			}

			SendResult(player, requestId, resultStatus, StorageService.Scan(world, blockPos, player));
		}

		private static void SendResult(EntityPlayer player, int request, byte resultStatus, List<StorageRow> resultRows)
		{
			if (player == null)
			{
				return;
			}

			if (player is EntityPlayerLocal)
			{
				StorageLaptopWindow.Receive(request, resultStatus, resultRows);
				return;
			}

			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
				PackageEmit.Take<NetPackageStorageLaptop>().SetupResult(request, resultStatus, resultRows),
				false,
				player.entityId);
		}
	}
}
