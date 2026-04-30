using System.Collections.Generic;
using System.IO;

namespace LockableWorkstations
{
	public class NetPackageLockableWorkstationState : NetPackage
	{
		private int _clrIdx;
		private Vector3i _blockPos = Vector3i.zero;
		private bool _isLocked;
		private string _ownerCombined = string.Empty;
		private string _passwordHash = string.Empty;
		private readonly List<string> _allowedUsersCombined = new List<string>();

		public NetPackageLockableWorkstationState Setup(int clrIdx, Vector3i blockPos, bool isLocked, string ownerCombined, string passwordHash, List<string> allowedUsersCombined)
		{
			_clrIdx = clrIdx;
			_blockPos = blockPos;
			_isLocked = isLocked;
			_ownerCombined = ownerCombined ?? string.Empty;
			_passwordHash = passwordHash ?? string.Empty;
			_allowedUsersCombined.Clear();
			if (allowedUsersCombined != null)
			{
				_allowedUsersCombined.AddRange(allowedUsersCombined);
			}

			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			_clrIdx = reader.ReadInt32();
			_blockPos = StreamUtils.ReadVector3i(reader);
			_isLocked = reader.ReadBoolean();
			_ownerCombined = reader.ReadString();
			_passwordHash = reader.ReadString();
			_allowedUsersCombined.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				_allowedUsersCombined.Add(reader.ReadString());
			}
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			((BinaryWriter)writer).Write(_clrIdx);
			StreamUtils.Write(writer, _blockPos);
			((BinaryWriter)writer).Write(_isLocked);
			((BinaryWriter)writer).Write(_ownerCombined ?? string.Empty);
			((BinaryWriter)writer).Write(_passwordHash ?? string.Empty);
			((BinaryWriter)writer).Write(_allowedUsersCombined.Count);
			for (int i = 0; i < _allowedUsersCombined.Count; i++)
			{
				((BinaryWriter)writer).Write(_allowedUsersCombined[i] ?? string.Empty);
			}
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			_ = callbacks;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return;
			}

			LockableWorkstationHelpers.ApplySyncedState(_clrIdx, _blockPos, _isLocked, _ownerCombined, _passwordHash, _allowedUsersCombined);
		}

		public override int GetLength()
		{
			int length = 4 + 12 + 1;
			length += (_ownerCombined?.Length ?? 0) * 2 + 4;
			length += (_passwordHash?.Length ?? 0) * 2 + 4;
			length += 4;
			for (int i = 0; i < _allowedUsersCombined.Count; i++)
			{
				length += (_allowedUsersCombined[i]?.Length ?? 0) * 2 + 4;
			}

			return length;
		}
	}
}
