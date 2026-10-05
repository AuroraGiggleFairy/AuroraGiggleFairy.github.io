using System.IO;
using UnityEngine;

namespace LockableWorkstations
{
	public class NetPackageLockableWorkstationCommand : NetPackage
	{
		private Vector3i _blockPos = Vector3i.zero;
		private int _entityId = -1;
		private string _command = string.Empty;
		private string _payload = string.Empty;

		public NetPackageLockableWorkstationCommand Setup(Vector3i blockPos, int entityId, string command, string payload)
		{
			_blockPos = blockPos;
			_entityId = entityId;
			_command = command ?? string.Empty;
			_payload = payload ?? string.Empty;
			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			_blockPos = StreamUtils.ReadVector3i(reader);
			_entityId = reader.ReadInt32();
			_command = reader.ReadString();
			_payload = reader.ReadString();
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			StreamUtils.Write(writer, _blockPos);
			((BinaryWriter)writer).Write(_entityId);
			((BinaryWriter)writer).Write(_command ?? string.Empty);
			((BinaryWriter)writer).Write(_payload ?? string.Empty);
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			_ = callbacks;
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || !manager.IsServer)
			{
				return;
			}

			if (string.Equals(_command, "lock", System.StringComparison.Ordinal) || string.Equals(_command, "unlock", System.StringComparison.Ordinal))
			{
				if (!LockableWorkstationHelpers.TryGetAdapter(world, 0, _blockPos, out _, out TileEntityLockAdapter lockAdapter))
				{
					return;
				}

				PlatformUserIdentifierAbs lockUser = LockableWorkstationHelpers.ResolveUserIdentifier(world, _entityId);
				bool isAdmin = LockableWorkstationHelpers.IsAdminEntityId(_entityId);
				bool canChangeLock = lockAdapter.IsOwner(lockUser) || lockAdapter.GetOwner() == null || isAdmin;
				if (!canChangeLock)
				{
					return;
				}

				if (lockAdapter.GetOwner() == null && lockUser != null)
				{
					lockAdapter.SetOwner(lockUser);
				}

				lockAdapter.SetLocked(string.Equals(_command, "lock", System.StringComparison.Ordinal));
				return;
			}

			if (!LockableWorkstationHelpers.TryGetAdapter(world, 0, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			PlatformUserIdentifierAbs user = LockableWorkstationHelpers.ResolveUserIdentifier(world, _entityId);
			if (string.Equals(_command, "lw-setcode", System.StringComparison.Ordinal))
			{
				adapter.SetPasswordHash(_payload, user);
			}
			else if (string.Equals(_command, "lw-allow", System.StringComparison.Ordinal))
			{
				if (string.Equals(adapter.GetPasswordHash(), _payload, System.StringComparison.Ordinal))
				{
					adapter.AddAllowedUserServer(user);
				}
			}
		}

		public override int GetLength()
		{
			return 16 + (_command?.Length ?? 0) * 2 + (_payload?.Length ?? 0) * 2;
		}
	}
}
