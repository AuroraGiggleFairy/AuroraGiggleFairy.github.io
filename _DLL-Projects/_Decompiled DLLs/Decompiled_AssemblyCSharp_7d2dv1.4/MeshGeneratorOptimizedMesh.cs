using System;
using UnityEngine;

public class MeshGeneratorOptimizedMesh
{
	public delegate bool DelegateBlocksHaveSameFaced(Block _b1, Block _b2);

	[PublicizedFrom(EAccessModifier.Private)]
	public INeighborBlockCache nBlocks;

	public MeshGeneratorOptimizedMesh(INeighborBlockCache _nBlocks)
	{
		nBlocks = _nBlocks;
	}

	public void GenerateCollisionMesh(Vector3i _startPos, Vector3i _endPos, VoxelMesh _voxelMesh)
	{
		Vector3i vector3i = _endPos - _startPos;
		int num = Math.Max(Math.Max(vector3i.x, vector3i.y), vector3i.z);
		bool[,] array = new bool[num, num];
		bool[,] array2 = new bool[num, num];
		int x = _startPos.x;
		int y = _startPos.y;
		int z = _startPos.z;
		for (int num2 = y + vector3i.y - 1; num2 >= y; num2--)
		{
			if (num2 != 0 && num2 != 255)
			{
				int num3 = 0;
				int num4 = 0;
				for (int i = x; i < x + vector3i.x; i++)
				{
					for (int j = z; j < z + vector3i.z; j++)
					{
						nBlocks.Init(i, j);
						Block block = nBlocks.Get(0, num2, 0).Block;
						Block block2 = nBlocks.Get(0, num2 + 1, 0).Block;
						bool flag = block.IsCollideMovement && !block2.IsCollideMovement;
						if (flag)
						{
							num3++;
						}
						array[i - x, j - z] = flag;
						block2 = nBlocks.Get(0, num2 - 1, 0).Block;
						flag = block.IsCollideMovement && !block2.IsCollideMovement;
						if (flag)
						{
							num4++;
						}
						array2[i - x, j - z] = flag;
					}
				}
				if (num3 > 0)
				{
					createFaces(array, num3, BlockFace.Top, num2 - y, _voxelMesh);
				}
				if (num4 > 0)
				{
					createFaces(array2, num4, BlockFace.Bottom, num2 - y, _voxelMesh);
				}
			}
		}
		for (int k = z; k < z + vector3i.z; k++)
		{
			int num5 = 0;
			int num6 = 0;
			for (int l = x; l < x + vector3i.x; l++)
			{
				nBlocks.Init(l, k);
				for (int m = y; m < y + vector3i.y; m++)
				{
					Block block3 = nBlocks.Get(0, m, 0).Block;
					Block block4 = nBlocks.Get(0, m, 1).Block;
					bool flag2 = block3.IsCollideMovement && !block4.IsCollideMovement;
					if (flag2)
					{
						num5++;
					}
					array[l - x, m - y] = flag2;
					block4 = nBlocks.Get(0, m, -1).Block;
					flag2 = block3.IsCollideMovement && !block4.IsCollideMovement;
					if (flag2)
					{
						num6++;
					}
					array2[l - x, m - y] = flag2;
				}
			}
			if (num5 > 0)
			{
				createFaces(array, num5, BlockFace.North, k - z, _voxelMesh);
			}
			if (num6 > 0)
			{
				createFaces(array2, num6, BlockFace.South, k - z, _voxelMesh);
			}
		}
		for (int n = x; n < x + vector3i.x; n++)
		{
			int num7 = 0;
			int num8 = 0;
			for (int num9 = y; num9 < y + vector3i.y; num9++)
			{
				for (int num10 = z; num10 < z + vector3i.z; num10++)
				{
					nBlocks.Init(n, num10);
					Block block5 = nBlocks.Get(0, num9, 0).Block;
					Block block6 = nBlocks.Get(1, num9, 0).Block;
					bool flag3 = block5.IsCollideMovement && !block6.IsCollideMovement;
					if (flag3)
					{
						num7++;
					}
					array[num9 - y, num10 - z] = flag3;
					block6 = nBlocks.Get(-1, num9, 0).Block;
					flag3 = block5.IsCollideMovement && !block6.IsCollideMovement;
					if (flag3)
					{
						num8++;
					}
					array2[num9 - y, num10 - z] = flag3;
				}
			}
			if (num7 > 0)
			{
				createFaces(array, num7, BlockFace.East, n - x, _voxelMesh);
			}
			if (num8 > 0)
			{
				createFaces(array2, num8, BlockFace.West, n - x, _voxelMesh);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createFaces(bool[,] _array, int _count, BlockFace _direction, int _y, VoxelMesh _voxelMesh)
	{
		int length = _array.GetLength(0);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (_array[i, j])
				{
					num++;
					int num2 = j;
					for (int k = j + 1; k < length; k++)
					{
						num2 = k;
						if (!_array[i, k])
						{
							num2 = k - 1;
							break;
						}
					}
					int num3 = num2 - j + 1;
					int num4 = i;
					for (int l = i + 1; l < length; l++)
					{
						num4 = l;
						int num5 = j;
						while (num5 < j + num3)
						{
							if (_array[l, num5])
							{
								num5++;
								continue;
							}
							goto IL_007c;
						}
						continue;
						IL_007c:
						num4 = l - 1;
						break;
					}
					int num6 = num4 - i + 1;
					for (int m = i; m < i + num6; m++)
					{
						for (int n = j; n < j + num3; n++)
						{
							_array[m, n] = false;
						}
					}
					switch (_direction)
					{
					case BlockFace.Top:
						_voxelMesh.AddRectXZFacingUp(i, _y + 1, j, num6, num3);
						break;
					case BlockFace.Bottom:
						_voxelMesh.AddRectXZFacingDown(i, _y, j, num6, num3);
						break;
					case BlockFace.East:
						_voxelMesh.AddRectYZFacingEast(_y + 1, i, j, num6, num3);
						break;
					case BlockFace.West:
						_voxelMesh.AddRectYZFacingWest(_y, i, j, num6, num3);
						break;
					case BlockFace.North:
						_voxelMesh.AddRectXYFacingNorth(i, j, _y + 1, num6, num3);
						break;
					case BlockFace.South:
						_voxelMesh.AddRectXYFacingSouth(i, j, _y, num6, num3);
						break;
					}
				}
				if (num == _count)
				{
					return;
				}
			}
		}
	}

	public void GenerateColorCubeMesh(Vector3i _startPos, Vector3i _endPos, VoxelMesh _voxelMesh)
	{
		Vector3i vector3i = _endPos - _startPos;
		int num = Math.Max(Math.Max(vector3i.x, vector3i.y), vector3i.z);
		BlockValue[,] array = new BlockValue[num, num];
		BlockValue[,] array2 = new BlockValue[num, num];
		int x = _startPos.x;
		int y = _startPos.y;
		int z = _startPos.z;
		for (int num2 = y + vector3i.y - 1; num2 >= y; num2--)
		{
			if (num2 != 0 && num2 != 255)
			{
				int num3 = 0;
				int num4 = 0;
				for (int i = x; i < x + vector3i.x; i++)
				{
					for (int j = z; j < z + vector3i.z; j++)
					{
						nBlocks.Init(i, j);
						BlockValue blockValue = nBlocks.Get(0, num2, 0);
						BlockValue blockValue2 = nBlocks.Get(0, num2 + 1, 0);
						bool flag = blockValue.Block.IsCollideMovement && !blockValue2.Block.IsCollideMovement;
						if (flag)
						{
							num3++;
						}
						array[i - x, j - z] = (flag ? blockValue : BlockValue.Air);
						blockValue2 = nBlocks.Get(0, num2 - 1, 0);
						flag = blockValue.Block.IsCollideMovement && !blockValue2.Block.IsCollideMovement;
						if (flag)
						{
							num4++;
						}
						array2[i - x, j - z] = (flag ? blockValue : BlockValue.Air);
					}
				}
				if (num3 > 0)
				{
					createFaces2(array, num3, BlockFace.Top, num2 - y, _voxelMesh);
				}
				if (num4 > 0)
				{
					createFaces2(array2, num4, BlockFace.Bottom, num2 - y, _voxelMesh);
				}
			}
		}
		for (int k = z; k < z + vector3i.z; k++)
		{
			int num5 = 0;
			int num6 = 0;
			for (int l = x; l < x + vector3i.x; l++)
			{
				nBlocks.Init(l, k);
				for (int m = y; m < y + vector3i.y; m++)
				{
					BlockValue blockValue3 = nBlocks.Get(0, m, 0);
					BlockValue blockValue4 = nBlocks.Get(0, m, 1);
					bool flag2 = blockValue3.Block.IsCollideMovement && !blockValue4.Block.IsCollideMovement;
					if (flag2)
					{
						num5++;
					}
					array[l - x, m - y] = (flag2 ? blockValue3 : BlockValue.Air);
					blockValue4 = nBlocks.Get(0, m, -1);
					flag2 = blockValue3.Block.IsCollideMovement && !blockValue4.Block.IsCollideMovement;
					if (flag2)
					{
						num6++;
					}
					array2[l - x, m - y] = (flag2 ? blockValue3 : BlockValue.Air);
				}
			}
			if (num5 > 0)
			{
				createFaces2(array, num5, BlockFace.North, k - z, _voxelMesh);
			}
			if (num6 > 0)
			{
				createFaces2(array2, num6, BlockFace.South, k - z, _voxelMesh);
			}
		}
		for (int n = x; n < x + vector3i.x; n++)
		{
			int num7 = 0;
			int num8 = 0;
			for (int num9 = y; num9 < y + vector3i.y; num9++)
			{
				for (int num10 = z; num10 < z + vector3i.z; num10++)
				{
					nBlocks.Init(n, num10);
					BlockValue blockValue5 = nBlocks.Get(0, num9, 0);
					BlockValue blockValue6 = nBlocks.Get(1, num9, 0);
					bool flag3 = blockValue5.Block.IsCollideMovement && !blockValue6.Block.IsCollideMovement;
					if (flag3)
					{
						num7++;
					}
					array[num9 - y, num10 - z] = (flag3 ? blockValue5 : BlockValue.Air);
					blockValue6 = nBlocks.Get(-1, num9, 0);
					flag3 = blockValue5.Block.IsCollideMovement && !blockValue6.Block.IsCollideMovement;
					if (flag3)
					{
						num8++;
					}
					array2[num9 - y, num10 - z] = (flag3 ? blockValue5 : BlockValue.Air);
				}
			}
			if (num7 > 0)
			{
				createFaces2(array, num7, BlockFace.East, n - x, _voxelMesh);
			}
			if (num8 > 0)
			{
				createFaces2(array2, num8, BlockFace.West, n - x, _voxelMesh);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createFaces2(BlockValue[,] _array, int _count, BlockFace _face, int _y, VoxelMesh _voxelMesh)
	{
		int length = _array.GetLength(0);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (!_array[i, j].Equals(BlockValue.Air))
				{
					num++;
					int num2 = j;
					for (int k = j + 1; k < length; k++)
					{
						num2 = k;
						if (!_array[i, k].Equals(_array[i, j]))
						{
							num2 = k - 1;
							break;
						}
					}
					int num3 = num2 - j + 1;
					int num4 = i;
					for (int l = i + 1; l < length; l++)
					{
						num4 = l;
						int num5 = j;
						while (num5 < j + num3)
						{
							if (_array[l, num5].Equals(_array[i, j]))
							{
								num5++;
								continue;
							}
							goto IL_00a0;
						}
						continue;
						IL_00a0:
						num4 = l - 1;
						break;
					}
					int num6 = num4 - i + 1;
					for (int m = i; m < i + num6; m++)
					{
						for (int n = j; n < j + num3; n++)
						{
							_array[m, n] = BlockValue.Air;
						}
					}
					Color white = Color.white;
					switch (_face)
					{
					case BlockFace.Top:
						_voxelMesh.AddRectXZFacingUp(i, _y + 1, j, num6, num3, white);
						break;
					case BlockFace.Bottom:
						_voxelMesh.AddRectXZFacingDown(i, _y, j, num6, num3, white);
						break;
					case BlockFace.East:
						_voxelMesh.AddRectYZFacingEast(_y + 1, i, j, num6, num3, white);
						break;
					case BlockFace.West:
						_voxelMesh.AddRectYZFacingWest(_y, i, j, num6, num3, white);
						break;
					case BlockFace.North:
						_voxelMesh.AddRectXYFacingNorth(i, j, _y + 1, num6, num3, white);
						break;
					case BlockFace.South:
						_voxelMesh.AddRectXYFacingSouth(i, j, _y, num6, num3, white);
						break;
					}
				}
				if (num == _count)
				{
					return;
				}
			}
		}
	}
}
