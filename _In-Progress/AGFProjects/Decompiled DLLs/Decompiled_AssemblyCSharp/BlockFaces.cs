using UnityEngine;

public static class BlockFaces
{
	public static BlockFace CharToFace(char c)
	{
		switch (c)
		{
		case 'T':
		case 't':
			return BlockFace.Top;
		case 'B':
		case 'b':
			return BlockFace.Bottom;
		case 'N':
		case 'n':
			return BlockFace.North;
		case 'W':
		case 'w':
			return BlockFace.West;
		case 'S':
		case 's':
			return BlockFace.South;
		case 'E':
		case 'e':
			return BlockFace.East;
		default:
			return BlockFace.None;
		}
	}

	public static BlockFace RotateFace(BlockFace face, int rotation)
	{
		Vector3 vector = Vector3.zero;
		switch (face)
		{
		case BlockFace.Top:
			vector = Vector3.up;
			break;
		case BlockFace.Bottom:
			vector = Vector3.down;
			break;
		case BlockFace.North:
			vector = Vector3.forward;
			break;
		case BlockFace.West:
			vector = Vector3.left;
			break;
		case BlockFace.South:
			vector = Vector3.back;
			break;
		case BlockFace.East:
			vector = Vector3.right;
			break;
		}
		vector = BlockShapeNew.GetRotationStatic(rotation) * vector;
		if (vector.y > 0.9f)
		{
			return BlockFace.Top;
		}
		if (vector.y < -0.9f)
		{
			return BlockFace.Bottom;
		}
		if (vector.z > 0.9f)
		{
			return BlockFace.North;
		}
		if (vector.x < -0.9f)
		{
			return BlockFace.West;
		}
		if (vector.z < -0.9f)
		{
			return BlockFace.South;
		}
		if (vector.x > 0.9f)
		{
			return BlockFace.East;
		}
		return face;
	}
}
