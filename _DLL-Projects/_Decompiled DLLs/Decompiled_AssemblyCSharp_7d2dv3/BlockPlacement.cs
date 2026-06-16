using UnityEngine;

public class BlockPlacement
{
	public enum EnumPlacement
	{
		Voxel,
		Free
	}

	public enum EnumRotationMode
	{
		ToFace,
		Simple,
		Advanced,
		Auto
	}

	public struct Result(EnumPlacement _placement, Vector3 _pos, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, PropTransform _propTransform)
	{
		public EnumPlacement placement = _placement;

		public Vector3 pos = _pos;

		public Vector3i blockPos = _blockPos;

		public BlockFace blockFace = _blockFace;

		public BlockValue blockValue = _blockValue;

		public PropTransform propTransform = _propTransform;

		public Result(EnumPlacement _placement, BlockValue _blockValue, PropTransform _propTransform, HitInfoDetails _hitInfo)
			: this(_placement, _hitInfo.pos, _hitInfo.blockPos, _hitInfo.blockFace, _blockValue, _propTransform)
		{
		}
	}

	public static BlockPlacement None = new BlockPlacement();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool supports45DegreeRotations(BlockValue _bv)
	{
		Block block = _bv.Block;
		if (block.isMultiBlock && (block.multiBlockPos.dim.x != 1 || block.multiBlockPos.dim.z != 1))
		{
			return false;
		}
		return (block.AllowedRotations & EBlockRotationClasses.Basic45) != 0;
	}

	public virtual Result OnPlaceBlock(EnumPlacement _placement, EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _bv, PropTransform _propTransform, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		Block block = _bv.Block;
		Result result = new Result(_placement, _bv, _propTransform, _hitInfo);
		bool flag = supports45DegreeRotations(_bv);
		if (_hitInfo.blockFace != BlockFace.Top && _hitInfo.voxelData.BlockValue.Block.CanBlocksReplaceOrGroundCover())
		{
			_hitInfo.blockFace = BlockFace.Top;
		}
		switch (_mode)
		{
		case EnumRotationMode.ToFace:
			if (!flag || _hitInfo.blockFace != BlockFace.Top || block.HandleFace != BlockFace.None)
			{
				int num = _localRot;
				Quaternion q = Quaternion.identity;
				if (block.HandleFace != BlockFace.None)
				{
					switch (block.HandleFace)
					{
					case BlockFace.Top:
						q = Quaternion.AngleAxis(180f, Vector3.right);
						break;
					case BlockFace.North:
						q = Quaternion.AngleAxis(90f, Vector3.right);
						break;
					case BlockFace.South:
						q = Quaternion.AngleAxis(-90f, Vector3.right);
						break;
					case BlockFace.East:
						q = Quaternion.AngleAxis(-90f, Vector3.forward);
						break;
					case BlockFace.West:
						q = Quaternion.AngleAxis(90f, Vector3.forward);
						break;
					}
				}
				result.blockValue.rotation = (byte)((uint)_hitInfo.blockFace << 2);
				result.blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree(result.blockValue.rotation, q, _bApplyRotFirst: true);
				switch (_hitInfo.blockFace)
				{
				case BlockFace.North:
					num += 2;
					break;
				case BlockFace.South:
					num += 2;
					break;
				case BlockFace.East:
					num++;
					break;
				case BlockFace.West:
					num += 3;
					break;
				}
				Vector3 axis = Vector3.up;
				switch (_hitInfo.blockFace)
				{
				case BlockFace.Top:
					axis = Vector3.up;
					break;
				case BlockFace.Bottom:
					axis = Vector3.down;
					break;
				case BlockFace.North:
					axis = Vector3.forward;
					break;
				case BlockFace.South:
					axis = Vector3.back;
					break;
				case BlockFace.East:
					axis = Vector3.right;
					break;
				case BlockFace.West:
					axis = Vector3.left;
					break;
				}
				for (int i = 0; i < num; i++)
				{
					result.blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree(result.blockValue.rotation, Quaternion.AngleAxis(90f, axis));
				}
			}
			else if ((_bv.rotation >= 0 && _bv.rotation <= 3) || (_bv.rotation >= 24 && _bv.rotation <= 27))
			{
				result.blockValue.rotation = _bv.rotation;
			}
			else
			{
				result.blockValue.rotation = 0;
			}
			break;
		case EnumRotationMode.Simple:
			if (!flag)
			{
				result.blockValue.rotation = (byte)(result.blockValue.rotation & 3);
			}
			else if ((_bv.rotation >= 0 && _bv.rotation <= 3) || (_bv.rotation >= 24 && _bv.rotation <= 27))
			{
				result.blockValue.rotation = _bv.rotation;
			}
			else
			{
				result.blockValue.rotation = 0;
			}
			break;
		}
		return result;
	}

	public virtual byte LimitRotation(EnumRotationMode _mode, ref int _localRot, HitInfoDetails _hitInfo, bool _bAdd, BlockValue _bv, byte _rotation)
	{
		bool flag = supports45DegreeRotations(_bv);
		Block block = _bv.Block;
		switch (_mode)
		{
		case EnumRotationMode.ToFace:
			if (!flag || (_rotation >= 4 && _rotation <= 23) || block.HandleFace != BlockFace.None)
			{
				Vector3 axis = Vector3.up;
				switch (_hitInfo.blockFace)
				{
				case BlockFace.Top:
					axis = Vector3.up;
					break;
				case BlockFace.Bottom:
					axis = Vector3.down;
					break;
				case BlockFace.North:
					axis = Vector3.forward;
					break;
				case BlockFace.South:
					axis = Vector3.back;
					break;
				case BlockFace.East:
					axis = Vector3.right;
					break;
				case BlockFace.West:
					axis = Vector3.left;
					break;
				}
				_localRot = (_localRot + (_bAdd ? 1 : (-1))) & 3;
				return (byte)BlockShapeNew.ConvertRotationFree(_bv.rotation, Quaternion.AngleAxis(90f, axis));
			}
			switch (_rotation)
			{
			case 0:
				if (_bAdd)
				{
					return 24;
				}
				return 27;
			case 1:
				if (_bAdd)
				{
					return 25;
				}
				return 24;
			case 2:
				if (_bAdd)
				{
					return 26;
				}
				return 25;
			case 3:
				if (_bAdd)
				{
					return 27;
				}
				return 26;
			case 24:
				if (_bAdd)
				{
					return 1;
				}
				return 0;
			case 25:
				if (_bAdd)
				{
					return 2;
				}
				return 1;
			case 26:
				if (_bAdd)
				{
					return 3;
				}
				return 2;
			case 27:
				if (_bAdd)
				{
					return 0;
				}
				return 3;
			default:
				return 0;
			}
		case EnumRotationMode.Simple:
			if (!flag)
			{
				return (byte)((_rotation + (_bAdd ? 1 : (-1))) & 3);
			}
			switch (_rotation)
			{
			case 0:
				if (_bAdd)
				{
					return 24;
				}
				return 27;
			case 1:
				if (_bAdd)
				{
					return 25;
				}
				return 24;
			case 2:
				if (_bAdd)
				{
					return 26;
				}
				return 25;
			case 3:
				if (_bAdd)
				{
					return 27;
				}
				return 26;
			case 24:
				if (_bAdd)
				{
					return 1;
				}
				return 0;
			case 25:
				if (_bAdd)
				{
					return 2;
				}
				return 1;
			case 26:
				if (_bAdd)
				{
					return 3;
				}
				return 2;
			case 27:
				if (_bAdd)
				{
					return 0;
				}
				return 3;
			default:
				return 0;
			}
		case EnumRotationMode.Advanced:
		{
			Block block2 = block;
			int num = _rotation;
			do
			{
				num += (_bAdd ? 1 : (-1));
				if (num > 27)
				{
					num = 0;
				}
				else if (num < 0)
				{
					num = 27;
				}
			}
			while (((num < 4) ? (block2.AllowedRotations & EBlockRotationClasses.Basic90) : ((num < 8) ? (block2.AllowedRotations & EBlockRotationClasses.Headfirst) : ((num >= 24) ? (block2.AllowedRotations & EBlockRotationClasses.Basic45) : (block2.AllowedRotations & EBlockRotationClasses.Sideways)))) == EBlockRotationClasses.None);
			return (byte)num;
		}
		default:
			return _rotation;
		}
	}
}
