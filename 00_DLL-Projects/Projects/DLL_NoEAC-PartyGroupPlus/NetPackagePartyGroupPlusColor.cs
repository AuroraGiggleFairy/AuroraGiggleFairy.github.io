using UnityEngine.Scripting;

namespace PartyGroupPlus
{
	[Preserve]
	public class NetPackagePartyGroupPlusColor : NetPackage
	{
		public enum Actions : byte
		{
			Request = 0,
			Query = 1,
			Sync = 2,
			SyncAll = 3,
			Release = 4
		}

		Actions action;
		int entityId;
		int colorIndex;
		int[] ids;
		int[] colors;

		public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

		public NetPackagePartyGroupPlusColor SetupRequest(int _colorIndex)
		{
			action = Actions.Request;
			colorIndex = _colorIndex;
			return this;
		}

		public NetPackagePartyGroupPlusColor SetupQuery()
		{
			action = Actions.Query;
			return this;
		}

		public NetPackagePartyGroupPlusColor SetupSync(int _entityId, int _colorIndex)
		{
			action = Actions.Sync;
			entityId = _entityId;
			colorIndex = _colorIndex;
			return this;
		}

		public NetPackagePartyGroupPlusColor SetupSyncAll(int[] _ids, int[] _colors)
		{
			action = Actions.SyncAll;
			ids = _ids ?? new int[0];
			colors = _colors ?? new int[0];
			return this;
		}

		public NetPackagePartyGroupPlusColor SetupRelease(int _entityId)
		{
			action = Actions.Release;
			entityId = _entityId;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			action = (Actions)_br.ReadByte();
			entityId = _br.ReadInt32();
			colorIndex = _br.ReadInt32();
			int count = _br.ReadInt32();
			ids = new int[count];
			colors = new int[count];
			for (int i = 0; i < count; i++)
			{
				ids[i] = _br.ReadInt32();
				colors[i] = _br.ReadInt32();
			}
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			System.IO.BinaryWriter writer = _bw;
			writer.Write((byte)action);
			writer.Write(entityId);
			writer.Write(colorIndex);
			int count = ids != null ? ids.Length : 0;
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				writer.Write(ids[i]);
				writer.Write(colors != null && i < colors.Length ? colors[i] : 0);
			}
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				int senderId = Sender != null ? Sender.entityId : -1;
				if (senderId < 0 && GameManager.Instance?.World != null)
				{
					senderId = GameManager.Instance.World.GetPrimaryPlayerId();
				}

				if (action == Actions.Request)
				{
					PartyGroupPlusColorStore.ServerTryClaim(senderId, colorIndex);
				}
				else if (action == Actions.Query)
				{
					if (senderId >= 0)
					{
						PartyGroupPlusColorStore.ServerSendColorSnapshot(senderId);
						EntityPlayer player = _world != null
							? _world.GetEntity(senderId) as EntityPlayer
							: null;
						PartyGroupPlusForceParty.SendMemberListTo(player);
					}
				}

				return;
			}

			if (action == Actions.Sync)
			{
				PartyGroupPlusColorStore.ClientSet(entityId, colorIndex);
			}
			else if (action == Actions.SyncAll)
			{
				PartyGroupPlusColorStore.ClientApplySyncAll(ids, colors);
			}
			else if (action == Actions.Release)
			{
				PartyGroupPlusColorStore.ClientClear(entityId);
			}
		}

		public override int GetLength()
		{
			return 13 + (ids != null ? ids.Length * 8 : 0);
		}
	}
}
