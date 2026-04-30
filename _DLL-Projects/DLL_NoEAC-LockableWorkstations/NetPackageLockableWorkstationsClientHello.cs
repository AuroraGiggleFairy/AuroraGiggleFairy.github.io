using System.IO;

namespace LockableWorkstations
{
	public class NetPackageLockableWorkstationsClientHello : NetPackage
	{
		private int _entityId = -1;
		private string _userCombined = string.Empty;

		public NetPackageLockableWorkstationsClientHello Setup(int entityId, string userCombined)
		{
			_entityId = entityId;
			_userCombined = userCombined ?? string.Empty;
			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			_entityId = reader.ReadInt32();
			_userCombined = reader.ReadString();
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			((BinaryWriter)writer).Write(_entityId);
			((BinaryWriter)writer).Write(_userCombined ?? string.Empty);
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			_ = callbacks;
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || !manager.IsServer)
			{
				return;
			}

			LockableWorkstationHybridRouting.MarkClientCapability(_entityId, _userCombined);
			World serverWorld = world ?? GameManager.Instance?.World;
			LockableWorkstationHelpers.SyncAllStatesToPlayer(serverWorld, _entityId);
		}

		public override int GetLength()
		{
			return 8 + (_userCombined?.Length ?? 0) * 2;
		}
	}
}
