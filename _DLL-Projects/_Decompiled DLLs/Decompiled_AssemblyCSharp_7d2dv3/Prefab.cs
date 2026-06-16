using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using PrefabVolumes;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Prefab : INeighborBlockCache, IChunkAccess
{
	public struct BlockStatistics
	{
		public int cntWindows;

		public int cntDoors;

		public int cntBlockEntities;

		public int cntBlockModels;

		public int cntSolid;

		public void Clear()
		{
			cntWindows = 0;
			cntDoors = 0;
			cntBlockEntities = 0;
			cntSolid = 0;
			cntBlockModels = 0;
		}

		public override string ToString()
		{
			return $"Blocks: {cntSolid} BEnts: {cntBlockEntities} BMods: {cntBlockModels} Wdws: {cntWindows}";
		}
	}

	public struct Data
	{
		public uint[] m_Blocks;

		public ushort[] m_Damage;

		public byte[] m_Density;

		public TextureFullArray[] m_Textures;

		public WaterValue[] m_Water;

		public void Init(int _count)
		{
			m_Blocks = new uint[_count];
			m_Damage = new ushort[_count];
			m_Density = new byte[_count];
			m_Textures = new TextureFullArray[_count];
			m_Water = new WaterValue[_count];
		}

		public void Expand(int _count)
		{
			int num = ((m_Blocks != null) ? m_Blocks.Length : 0);
			if (_count > num)
			{
				m_Blocks = new uint[_count];
				m_Damage = new ushort[_count];
				m_Density = new byte[_count];
				m_Textures = new TextureFullArray[_count];
				m_Water = new WaterValue[_count];
			}
			for (int i = 0; i < _count; i++)
			{
				m_Textures[i].Fill(0L);
				m_Water[i] = WaterValue.Empty;
			}
		}
	}

	public class Cells<T> where T : unmanaged
	{
		public class CellsAtX
		{
			public Cell[] a;
		}

		public class CellsAtZ
		{
			public CellsAtX[] a;
		}

		public struct Cell
		{
			public const int cSizeXZ = 4;

			public const int cSizeArray = 16;

			public const int cSizeXZMask = 3;

			public const int cSizeXZShift = 2;

			public static Cell empty;

			public T[] a = new T[16];

			public Cell(T _defaultValue)
			{
				for (int i = 0; i < 16; i++)
				{
					a[i] = _defaultValue;
				}
			}

			public Cell Clone()
			{
				Cell result = default(Cell);
				if (a != null)
				{
					result.a = new T[16];
					for (int i = 0; i < 16; i++)
					{
						result.a[i] = a[i];
					}
				}
				return result;
			}

			public override string ToString()
			{
				return $"{((a != null) ? a.Length : (-1))}";
			}

			public unsafe int Size()
			{
				return 16 * sizeof(T);
			}

			public unsafe int UsedCount(T _defaultValue)
			{
				int num = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf(ref _defaultValue);
				for (int i = 0; i < 16; i++)
				{
					byte* ptr2 = (byte*)UnsafeUtility.AddressOf(ref a[i]);
					for (int j = 0; j < sizeof(T); j++)
					{
						if (ptr[j] != ptr2[j])
						{
							num++;
							break;
						}
					}
				}
				return num;
			}

			public void Set(int _x, int _z, T _value)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				a[num] = _value;
			}

			public T Get(int _x, int _z)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				return a[num];
			}

			public unsafe void Load(PooledBinaryReader _br)
			{
				int num = (int)_br.BaseStream.Position;
				int num2 = _br.ReadUInt16();
				_br.Read(Cells<T>.cellBytes, 0, num2);
				Log.Warning("Cell Load at {0}, count{1}", num, num2);
				int num3 = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf(ref a[0]);
				int num4 = 0;
				while (num4 < num2)
				{
					int num5 = (sbyte)Cells<T>.cellBytes[num4++];
					if (num5 >= 0)
					{
						for (int i = 0; i < num5; i++)
						{
							byte b = Cells<T>.cellBytes[num4++];
							ptr[num3++] = b;
						}
						continue;
					}
					num5 = -num5;
					byte b2 = Cells<T>.cellBytes[num4++];
					for (int j = 0; j < num5; j++)
					{
						ptr[num3++] = b2;
					}
				}
			}

			public unsafe void Save(BinaryWriter _bw)
			{
				byte* ptr = (byte*)UnsafeUtility.AddressOf(ref a[0]);
				int num = 0;
				int num2 = a.Length * sizeof(T);
				int num3 = 0;
				while (num3 < num2)
				{
					int num4 = 1;
					byte b = ptr[num3];
					if (num3 + 1 < num2)
					{
						byte b2 = ptr[num3 + 1];
						if (b == b2)
						{
							int num5 = 2;
							for (int i = num3 + 2; i < num2 && ptr[i] == b; i++)
							{
								num5++;
								if (num5 >= 128)
								{
									break;
								}
							}
							if (num5 >= 3)
							{
								num4 = -num5;
								num3 += num5;
							}
							else
							{
								num4 = num5;
							}
						}
						if (num4 >= 0)
						{
							for (int j = num3 + num4; j < num2; j++)
							{
								b2 = ptr[j];
								if (j + 2 < num2 && b2 == ptr[j + 1] && b2 == ptr[j + 2])
								{
									break;
								}
								num4++;
								if (num4 >= 127)
								{
									break;
								}
							}
						}
					}
					Cells<T>.cellBytes[num++] = (byte)num4;
					if (num4 >= 0)
					{
						for (int k = 0; k < num4; k++)
						{
							byte b3 = ptr[num3++];
							Cells<T>.cellBytes[num++] = b3;
						}
					}
					else
					{
						Cells<T>.cellBytes[num++] = b;
					}
				}
				_bw.Write((ushort)num);
				_bw.Write(Cells<T>.cellBytes, 0, num);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public enum DataFormat
		{
			Empty,
			RLE
		}

		public T defaultValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public int sizeY;

		public CellsAtZ[] a;

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] cellBytes = new byte[256];

		public Cells(int _sizeY, T _defaultValue)
		{
			sizeY = _sizeY;
			defaultValue = _defaultValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Cells(Cells<T> _template)
		{
			sizeY = _template.sizeY;
			defaultValue = _template.defaultValue;
		}

		public Cell AllocCell(int _x, int _y, int _z)
		{
			if (a == null)
			{
				a = new CellsAtZ[sizeY];
			}
			CellsAtZ cellsAtZ = a[_y];
			if (cellsAtZ == null)
			{
				cellsAtZ = new CellsAtZ();
				a[_y] = cellsAtZ;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				Array.Resize(ref cellsAtZ.a, num + 1);
			}
			CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				cellsAtX = new CellsAtX();
				cellsAtZ.a[num] = cellsAtX;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				Array.Resize(ref cellsAtX.a, num2 + 1);
			}
			Cell cell = cellsAtX.a[num2];
			if (cell.a == null)
			{
				cell = new Cell(defaultValue);
				cellsAtX.a[num2] = cell;
			}
			return cell;
		}

		public Cell GetCell(int _x, int _y, int _z)
		{
			if (a == null)
			{
				return Cell.empty;
			}
			CellsAtZ cellsAtZ = a[_y];
			if (cellsAtZ == null)
			{
				return Cell.empty;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				return Cell.empty;
			}
			CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				return Cell.empty;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				return Cell.empty;
			}
			return cellsAtX.a[num2];
		}

		public T GetData(int _x, int _y, int _z)
		{
			Cell cell = GetCell(_x, _y, _z);
			if (cell.a == null)
			{
				return defaultValue;
			}
			return cell.Get(_x, _z);
		}

		public void SetData(int _x, int _y, int _z, T _data)
		{
			AllocCell(_x, _y, _z).Set(_x, _z, _data);
		}

		public Cells<T> Clone()
		{
			Cells<T> cells = new Cells<T>(sizeY, defaultValue);
			if (a == null)
			{
				return cells;
			}
			cells.a = new CellsAtZ[sizeY];
			for (int i = 0; i < sizeY; i++)
			{
				CellsAtZ cellsAtZ = a[i];
				if (cellsAtZ == null)
				{
					continue;
				}
				CellsAtZ cellsAtZ2 = new CellsAtZ();
				cellsAtZ2.a = new CellsAtX[cellsAtZ.a.Length];
				cells.a[i] = cellsAtZ2;
				for (int j = 0; j < cellsAtZ.a.Length; j++)
				{
					CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX == null)
					{
						continue;
					}
					CellsAtX cellsAtX2 = new CellsAtX();
					cellsAtX2.a = new Cell[cellsAtX.a.Length];
					cellsAtZ2.a[j] = cellsAtX2;
					for (int k = 0; k < cellsAtX.a.Length; k++)
					{
						Cell cell = cellsAtX.a[k];
						if (cell.a != null)
						{
							cellsAtX2.a[k] = cell.Clone();
						}
					}
				}
			}
			return cells;
		}

		public void Stats(out int _arrayCount, out int _arraySize, out int _cellsCount, out int _cellsSize, out int _usedCount)
		{
			_arrayCount = 0;
			_arraySize = 0;
			_cellsCount = 0;
			_cellsSize = 0;
			_usedCount = 0;
			int num = ((a != null) ? a.Length : 0);
			_arrayCount += num;
			_arraySize += num * 8 + 8;
			for (int i = 0; i < num; i++)
			{
				CellsAtZ cellsAtZ = a[i];
				if (cellsAtZ == null)
				{
					continue;
				}
				_arrayCount += cellsAtZ.a.Length;
				_arraySize += cellsAtZ.a.Length * 8 + 8;
				for (int j = 0; j < cellsAtZ.a.Length; j++)
				{
					CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX == null)
					{
						continue;
					}
					_arrayCount += cellsAtX.a.Length;
					_arraySize += cellsAtX.a.Length * 8 + 8;
					for (int k = 0; k < cellsAtX.a.Length; k++)
					{
						Cell cell = cellsAtX.a[k];
						if (cell.a != null)
						{
							_cellsCount += 16;
							_cellsSize += cell.Size();
							_usedCount += cell.UsedCount(defaultValue);
						}
					}
				}
			}
		}

		public void Load(PooledBinaryReader _br)
		{
			Array.Clear(a, 0, a.Length);
			if (_br.ReadByte() == 1 && _br.ReadUInt16() > 0)
			{
				LoadData(_br);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadData(PooledBinaryReader _br)
		{
			while (true)
			{
				_br.Read(cellBytes, 0, 3);
				int num = cellBytes[0];
				if (num != 255)
				{
					int x = cellBytes[1] << 2;
					int z = cellBytes[2] << 2;
					AllocCell(x, num, z).Load(_br);
					continue;
				}
				break;
			}
		}

		public void Save(BinaryWriter _bw)
		{
			ushort num = (ushort)((a != null) ? ((uint)a.Length) : 0u);
			if (num == 0)
			{
				_bw.Write((byte)0);
				return;
			}
			_bw.Write((byte)1);
			_bw.Write(num);
			for (int i = 0; i < num; i++)
			{
				CellsAtZ cellsAtZ = a[i];
				if (cellsAtZ == null)
				{
					continue;
				}
				for (int j = 0; j < cellsAtZ.a.Length; j++)
				{
					CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX == null)
					{
						continue;
					}
					for (int k = 0; k < cellsAtX.a.Length; k++)
					{
						Cell cell = cellsAtX.a[k];
						if (cell.a != null)
						{
							cellBytes[0] = (byte)i;
							cellBytes[1] = (byte)k;
							cellBytes[2] = (byte)j;
							_bw.Write(cellBytes, 0, 3);
							cell.Save(_bw);
						}
					}
				}
			}
			cellBytes[0] = byte.MaxValue;
			_bw.Write(cellBytes, 0, 3);
		}

		public T[] ToArray(Prefab prefab, Vector3i _size)
		{
			T[] array = new T[_size.x * _size.y * _size.z];
			int num = ((a != null) ? a.Length : 0);
			for (int i = 0; i < num; i++)
			{
				CellsAtZ cellsAtZ = a[i];
				if (cellsAtZ == null)
				{
					continue;
				}
				for (int j = 0; j < cellsAtZ.a.Length; j++)
				{
					CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX == null)
					{
						continue;
					}
					for (int k = 0; k < cellsAtX.a.Length; k++)
					{
						Cell cell = cellsAtX.a[k];
						if (cell.a == null)
						{
							continue;
						}
						int num2 = k << 2;
						int num3 = j << 2;
						int num4 = Utils.FastMin(_size.x - num2, 4);
						int num5 = Utils.FastMin(_size.z - num3, 4);
						for (int l = 0; l < num5; l++)
						{
							for (int m = 0; m < num4; m++)
							{
								T val = cell.Get(m, l);
								int num6 = prefab.CoordToOffset(0, num2 + m, i, num3 + l);
								array[num6] = val;
							}
						}
					}
				}
			}
			return array;
		}

		public unsafe void CompareTest(Vector3i size, PooledBinaryReader _br)
		{
			Cells<T> cells = new Cells<T>(this);
			cells.Load(_br);
			if (a.Length != cells.a.Length)
			{
				Log.Error("Cells size");
			}
			for (int i = 0; i < size.y; i++)
			{
				for (int j = 0; j < size.z; j++)
				{
					for (int k = 0; k < size.x; k++)
					{
						Cell cell = GetCell(k, i, j);
						Cell cell2 = cells.GetCell(k, i, j);
						if (cell.a == null)
						{
							if (cell2.a != null)
							{
								Log.Error("Cells one is null {0} {1} {2}", k, i, j);
							}
						}
						else
						{
							if (cell2.a == null)
							{
								continue;
							}
							for (int l = 0; l < 4; l++)
							{
								for (int m = 0; m < 4; m++)
								{
									T output = cell.Get(m, l);
									T output2 = cell2.Get(m, l);
									byte* ptr = (byte*)UnsafeUtility.AddressOf(ref output);
									byte* ptr2 = (byte*)UnsafeUtility.AddressOf(ref output2);
									for (int n = 0; n < sizeof(T); n++)
									{
										if (ptr[n] != ptr2[n])
										{
											Log.Error("Cells data {0} {1} {2}, {3} != {4}", k, i, j, ptr[n], ptr2[n]);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public class PrefabChunk : IChunk, IBlockAccess
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Prefab prefab;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int X { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int Y { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int Z { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Vector3i ChunkPos { get; set; }

		public PrefabChunk(Prefab _prefab, int _x, int _z)
		{
			prefab = _prefab;
			X = _x;
			Z = _z;
			Y = 0;
		}

		public bool GetAvailable()
		{
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkCoordinates(ref int _x, ref int _y, ref int _z)
		{
			_x = X * 16 + _x;
			_y = Y * 256 + _y;
			_z = Z * 16 + _z;
			if (_x >= 0 && _x < prefab.size.x && _y >= 0 && _y < prefab.size.y && _z >= 0 && _z < prefab.size.z)
			{
				return true;
			}
			return false;
		}

		public BlockValue GetBlock(int _x, int _y, int _z)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return BlockValue.Air;
			}
			return prefab.GetBlock(_x, _y, _z);
		}

		public BlockValue GetBlock(Vector3i pos)
		{
			return IBlockAccess.DefaultGetBlock(this, pos);
		}

		public BlockValue GetBlock(BlockValueRef bvRef)
		{
			return IBlockAccess.DefaultGetBlock(this, bvRef);
		}

		public BlockValue GetBlockNoDamage(int _x, int _y, int _z)
		{
			return GetBlock(_x, _y, _z);
		}

		public void GetBlockColumn(int _x, int _y, int _z, BlockValue[] _blocks)
		{
			int num = _blocks.Length;
			for (int i = 0; i < num; i++)
			{
				_blocks[i] = GetBlock(_x, _y + i, _z);
			}
		}

		public BlockValue GetBlock(int _bos, int _y)
		{
			return GetBlock(ChunkBlockLayerLegacy.OffsetX(_bos), _y, ChunkBlockLayerLegacy.OffsetX(_bos));
		}

		public bool IsAir(int _x, int _y, int _z)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return false;
			}
			if (prefab.GetBlock(_x, _y, _z).isair)
			{
				return !prefab.GetWater(_x, _y, _z).HasMass();
			}
			return false;
		}

		public WaterValue GetWater(int _x, int _y, int _z)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return WaterValue.Empty;
			}
			return prefab.GetWater(_x, _y, _z);
		}

		public bool IsWater(int _x, int _y, int _z)
		{
			return GetWater(_x, _y, _z).HasMass();
		}

		public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _blockFace, int _channel)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0;
			}
			return (int)((prefab.GetTexture(_x, _y, _z)[_channel] >> (int)_blockFace * 6) & 0x3F);
		}

		public long GetTextureFull(int _x, int _y, int _z, int _channel = 0)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0L;
			}
			return prefab.GetTexture(_x, _y, _z)[_channel];
		}

		public PropValue GetProp(int chunkX, int chunkZ, int propId)
		{
			return prefab.GetProp(chunkX, chunkZ, propId);
		}

		public PropValue GetProp(long chunkKey, int propId)
		{
			return prefab.GetProp(chunkKey, propId);
		}

		public PropValue GetProp(Vector2i chunkPos, int propId)
		{
			return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
		}

		public PropValue GetProp(PropRef propRef)
		{
			return IBlockAccess.DefaultGetProp(this, propRef);
		}

		public bool IsOnlyTerrain(int _y)
		{
			return false;
		}

		public bool IsOnlyTerrainLayer(int _idx)
		{
			return false;
		}

		public bool IsEmpty()
		{
			return false;
		}

		public bool IsEmpty(int _y)
		{
			return false;
		}

		public bool IsEmptyLayer(int _y)
		{
			return false;
		}

		public byte GetStability(int _x, int _y, int _z)
		{
			return 15;
		}

		public byte GetStability(int _offs, int _y)
		{
			return 15;
		}

		public void SetStability(int _offs, int _y, byte _v)
		{
		}

		public void SetStability(int _x, int _y, int _z, byte _v)
		{
		}

		public byte GetLight(int _x, int _y, int _z, Chunk.LIGHT_TYPE _type)
		{
			return 15;
		}

		public int GetLightValue(int _x, int _y, int _z, int _darknessV)
		{
			return 15;
		}

		public float GetLightBrightness(int _x, int _y, int _z, int _darknessV)
		{
			return 1f;
		}

		public Vector3i GetWorldPos()
		{
			return new Vector3i(X, Y, Z);
		}

		public void SetVertexOffset(int _x, int _y, int _z, Vector3 _vertexOffset)
		{
		}

		public bool GetVertexOffset(int _x, int _y, int _z, out Vector3 _vertexOffset)
		{
			_vertexOffset = Vector3.zero;
			return false;
		}

		public void SetVertexYOffset(int _x, int _y, int _z, float _addYPos)
		{
		}

		public byte GetHeight(int _blockOffset)
		{
			return (byte)prefab.size.y;
		}

		public byte GetHeight(int _x, int _z)
		{
			return (byte)prefab.size.y;
		}

		public sbyte GetDensity(int _xzOffs, int _y)
		{
			return GetDensity(ChunkBlockLayerLegacy.OffsetX(_xzOffs), _y, ChunkBlockLayerLegacy.OffsetX(_xzOffs));
		}

		public sbyte GetDensity(int _x, int _y, int _z)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return sbyte.MaxValue;
			}
			return prefab.GetDensity(_x, _y, _z);
		}

		public sbyte SetDensity(int _xzOffs, int _y, sbyte _density)
		{
			return 0;
		}

		public bool HasSameDensityValue(int _y)
		{
			return false;
		}

		public sbyte GetSameDensityValue(int _y)
		{
			return 0;
		}

		public BlockEntityData GetBlockEntity(Vector3i _blockPos)
		{
			return null;
		}

		public BlockEntityData GetBlockEntity(Transform _transform)
		{
			return null;
		}

		public void SetTopSoilBroken(int _x, int _z)
		{
		}

		public bool IsTopSoil(int _x, int _z)
		{
			return false;
		}

		public byte GetTerrainHeight(int _x, int _z)
		{
			for (int num = prefab.size.y - 1; num >= 0; num--)
			{
				if (GetBlock(_x, num, _z).Block.shape.IsTerrain())
				{
					return (byte)num;
				}
			}
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int CurrentSaveVersion = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumSupportedVersion = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_CopyAirBlocks = "CopyAirBlocks";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowTopSoilDecorations = "AllowTopSoilDecorations";

	public const string cProp_YOffset = "YOffset";

	public const string cProp_RotationToFaceNorth = "RotationToFaceNorth";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludeDistantPOIMesh = "ExcludeDistantPOIMesh";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludePOICulling = "ExcludePOICulling";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIYOffset = "DistantPOIYOffset";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIOverride = "DistantPOIOverride";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_EditorGroups = "EditorGroups";

	public const string cProp_IsTraderArea = "TraderArea";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TraderAreaProtect = "TraderAreaProtect";

	public const string cProp_SleeperVolumeStart = "SleeperVolumeStart";

	public const string cProp_SleeperVolumeSize = "SleeperVolumeSize";

	public const string cProp_SleeperVolumeGroup = "SleeperVolumeGroup";

	public const string cProp_SleeperVolumeGroupId = "SleeperVolumeGroupId";

	public const string cProp_SleeperIsPriorityVolume = "SleeperIsLootVolume";

	public const string cProp_SleeperIsQuestExclude = "SleeperIsQuestExclude";

	public const string cProp_SleeperVolumeFlags = "SleeperVolumeFlags";

	public const string cProp_SleeperVolumeTriggeredBy = "SleeperVolumeTriggeredBy";

	public const string cProp_SleeperVolumeScript = "SVS";

	public const string cProp_TeleportVolumeStart = "TeleportVolumeStart";

	public const string cProp_TeleportVolumeSize = "TeleportVolumeSize";

	public const string cProp_InfoVolumeStart = "InfoVolumeStart";

	public const string cProp_InfoVolumeSize = "InfoVolumeSize";

	public const string cProp_WallVolumeStart = "WallVolumeStart";

	public const string cProp_WallVolumeSize = "WallVolumeSize";

	public const string cProp_TriggerVolumeStart = "TriggerVolumeStart";

	public const string cProp_TriggerVolumeSize = "TriggerVolumeSize";

	public const string cProp_TriggerVolumeTriggers = "TriggerVolumeTriggers";

	public const string cProp_POIMarkerStart = "POIMarkerStart";

	public const string cProp_POIMarkerSize = "POIMarkerSize";

	public const string cProp_POIMarkerGroup = "POIMarkerGroup";

	public const string cProp_POIMarkerTags = "POIMarkerTags";

	public const string cProp_POIMarkerType = "POIMarkerType";

	public const string cProp_POIMarkerPartToSpawn = "POIMarkerPartToSpawn";

	public const string cProp_POIMarkerPartRotations = "POIMarkerPartRotations";

	public const string cProp_POIMarkerPartSpawnChance = "POIMarkerPartSpawnChance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_Zoning = "Zoning";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedBiomes = "AllowedBiomes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedTownships = "AllowedTownships";

	public const string cProp_Tags = "Tags";

	public const string cProp_ThemeTags = "ThemeTags";

	public const string cProp_ThemeRepeatDist = "ThemeRepeatDistance";

	public const string cProp_DuplicateRepeatDist = "DuplicateRepeatDistance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_IndexedBlockOffsets = "IndexedBlockOffsets";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_QuestTags = "QuestTags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ShowQuestClearCount = "ShowQuestClearCount";

	public const string cProp_DifficultyTier = "DifficultyTier";

	public const string cProp_PrefabSize = "PrefabSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string MISSING_BLOCK_NAME = "missingBlock";

	public Vector3i size;

	public PathAbstractions.AbstractedLocation location;

	public bool bCopyAirBlocks = true;

	public bool bExcludeDistantPOIMesh;

	public bool bExcludePOICulling;

	public float distantPOIYOffset;

	public string distantPOIOverride;

	public bool bAllowTopSoilDecorations;

	public bool bTraderArea;

	public Vector3i TraderAreaProtect;

	public readonly List<PrefabVolumeListAbs> AllVolumeLists;

	public readonly Dictionary<PrefabVolumeAbs.EVolumeType, PrefabVolumeListAbs> AllVolumeListsByType = new Dictionary<PrefabVolumeAbs.EVolumeType, PrefabVolumeListAbs>();

	public readonly PrefabSleeperVolumeList SleeperVolumeList;

	public readonly PrefabTeleportVolumeList TeleportVolumeList;

	public readonly PrefabInfoVolumeList InfoVolumeList;

	public readonly PrefabWallVolumeList WallVolumeList;

	public readonly PrefabTriggerVolumeList TriggerVolumeList;

	public readonly PrefabMarkerVolumeList MarkerVolumeList;

	public int yOffset;

	public int Transient_NumSleeperSpawns;

	public List<string> editorGroups = new List<string>();

	public int rotationToFaceNorth = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedZones = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedBiomes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedTownships = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> themeTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int themeRepeatDistance = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public int duplicateRepeatDistance = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> multiBlockParentIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> decoAllowedBlockIndices = new List<int>();

	public readonly Dictionary<string, List<Vector3i>> indexedBlockOffsets = new CaseInsensitiveStringDictionary<List<Vector3i>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockStatistics statistics;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldStats renderingCost;

	public int ShowQuestClearCount = 1;

	public byte DifficultyTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isCellsDataOwner = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Cells<uint> blockCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Cells<ushort> damageCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Cells<sbyte> densityCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Cells<TextureFullArray> textureCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Cells<WaterValue> waterCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Data sharedData = default(Data);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> entities = new List<EntityCreationData>();

	public DynamicProperties properties = new DynamicProperties();

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFillerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFiller2Type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockTypeMissingBlock = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, TileEntity> tileEntities = new Dictionary<Vector3i, TileEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector2i, Dictionary<int, PropValue>> propsByChunk = new Dictionary<Vector2i, Dictionary<int, PropValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PrefabInsideDataFile insidePos = new PrefabInsideDataFile();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, BlockTrigger> triggerData = new Dictionary<Vector3i, BlockTrigger>();

	public readonly List<byte> TriggerLayers = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] tempBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleBitStream simpleBitStreamReader = new SimpleBitStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, PrefabChunk> dictChunks;

	public string PrefabName => location.FileNameNoExtension ?? "";

	public string LocalizedName => Localization.Get(PrefabName);

	public string LocalizedEnglishName => Localization.Get(PrefabName, _caseInsensitive: false, "english");

	public float DensityScore
	{
		get
		{
			if (renderingCost != null)
			{
				return (float)renderingCost.TotalVertices / 100000f;
			}
			return 0f;
		}
	}

	public WorldStats RenderingCostStats
	{
		get
		{
			return renderingCost;
		}
		set
		{
			renderingCost = value;
		}
	}

	public FastTags<TagGroup.Poi> Tags
	{
		get
		{
			return tags;
		}
		set
		{
			tags = value;
		}
	}

	public FastTags<TagGroup.Poi> ThemeTags
	{
		get
		{
			return themeTags;
		}
		set
		{
			themeTags = value;
		}
	}

	public int ThemeRepeatDistance
	{
		get
		{
			return themeRepeatDistance;
		}
		set
		{
			themeRepeatDistance = value;
		}
	}

	public int DuplicateRepeatDistance
	{
		get
		{
			return duplicateRepeatDistance;
		}
		set
		{
			duplicateRepeatDistance = value;
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewPrefab(string _name, string _prefabsSubfolder = null)
	{
		Mod mod = null;
		PathAbstractions.EAbstractedLocationType locationType = PathAbstractions.EAbstractedLocationType.UserDataPath;
		string launchArgument = GameUtils.GetLaunchArgument("newprefabsmod");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			mod = ModManager.GetMod(launchArgument, _onlyLoaded: true);
			if (mod != null)
			{
				locationType = PathAbstractions.EAbstractedLocationType.Mods;
			}
			else
			{
				Log.Warning("Argument -newprefabsmod given but mod with name '" + launchArgument + "' not found, ignoring!");
			}
		}
		return PathAbstractions.PrefabsSearchPaths.BuildLocation(locationType, _prefabsSubfolder, _name, mod).Value;
	}

	public static bool CanSaveIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	public Prefab()
	{
		SleeperVolumeList = new PrefabSleeperVolumeList(this);
		TeleportVolumeList = new PrefabTeleportVolumeList(this);
		InfoVolumeList = new PrefabInfoVolumeList(this);
		WallVolumeList = new PrefabWallVolumeList(this);
		TriggerVolumeList = new PrefabTriggerVolumeList(this);
		MarkerVolumeList = new PrefabMarkerVolumeList(this);
		AllVolumeLists = new List<PrefabVolumeListAbs> { SleeperVolumeList, TeleportVolumeList, InfoVolumeList, WallVolumeList, TriggerVolumeList, MarkerVolumeList };
		foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
		{
			AllVolumeListsByType[allVolumeList.VolumeType] = allVolumeList;
		}
	}

	public Prefab(Prefab _other, bool _sharedData = false)
		: this()
	{
		size = _other.size;
		if (_sharedData)
		{
			isCellsDataOwner = false;
			blockCells = _other.blockCells;
			damageCells = _other.damageCells;
			densityCells = _other.densityCells;
			textureCells = _other.textureCells;
			waterCells = _other.waterCells;
		}
		else
		{
			blockCells = _other.blockCells.Clone();
			damageCells = _other.damageCells.Clone();
			densityCells = _other.densityCells.Clone();
			textureCells = _other.textureCells.Clone();
			waterCells = _other.waterCells.Clone();
		}
		if (_sharedData)
		{
			multiBlockParentIndices = _other.multiBlockParentIndices;
			decoAllowedBlockIndices = _other.decoAllowedBlockIndices;
		}
		else
		{
			multiBlockParentIndices = new List<int>(_other.multiBlockParentIndices);
			decoAllowedBlockIndices = new List<int>(_other.decoAllowedBlockIndices);
		}
		location = _other.location;
		bCopyAirBlocks = _other.bCopyAirBlocks;
		bExcludeDistantPOIMesh = _other.bExcludeDistantPOIMesh;
		bExcludePOICulling = _other.bExcludePOICulling;
		distantPOIYOffset = _other.distantPOIYOffset;
		distantPOIOverride = _other.distantPOIOverride;
		bAllowTopSoilDecorations = _other.bAllowTopSoilDecorations;
		bTraderArea = _other.bTraderArea;
		TraderAreaProtect = _other.TraderAreaProtect;
		for (int i = 0; i < AllVolumeLists.Count; i++)
		{
			AllVolumeLists[i].CopyFrom(_other.AllVolumeLists[i]);
		}
		yOffset = _other.yOffset;
		rotationToFaceNorth = _other.rotationToFaceNorth;
		allowedBiomes = new List<string>(_other.allowedBiomes);
		allowedTownships = new List<string>(_other.allowedTownships);
		allowedZones = new List<string>(_other.allowedZones);
		tags = new FastTags<TagGroup.Poi>(_other.tags);
		themeTags = new FastTags<TagGroup.Poi>(_other.themeTags);
		themeRepeatDistance = _other.themeRepeatDistance;
		duplicateRepeatDistance = _other.duplicateRepeatDistance;
		questTags = _other.questTags;
		DifficultyTier = _other.DifficultyTier;
		ShowQuestClearCount = _other.ShowQuestClearCount;
		localRotation = _other.localRotation;
		for (int j = 0; j < _other.entities.Count; j++)
		{
			EntityCreationData entityCreationData = _other.entities[j];
			entities.Add(entityCreationData.Clone());
		}
		foreach (KeyValuePair<Vector3i, TileEntity> tileEntity in _other.tileEntities)
		{
			tileEntities.Add(tileEntity.Key, tileEntity.Value);
		}
		insidePos = _other.insidePos.Clone();
		foreach (KeyValuePair<Vector3i, BlockTrigger> triggerDatum in _other.triggerData)
		{
			triggerData.Add(triggerDatum.Key, triggerDatum.Value);
		}
		for (int k = 0; k < _other.TriggerLayers.Count; k++)
		{
			TriggerLayers.Add(_other.TriggerLayers[k]);
		}
		renderingCost = _other.renderingCost;
	}

	public Prefab(Vector3i _size)
		: this()
	{
		size = _size;
		localRotation = 0;
		InitData();
	}

	public int EstimateOwnedBytes()
	{
		int num = 0;
		if (isCellsDataOwner)
		{
			num += IntPtr.Size;
			int _arrayCount;
			int _arraySize;
			int _cellsCount;
			int _cellsSize;
			int _usedCount;
			if (blockCells != null)
			{
				blockCells.Stats(out _arrayCount, out _arraySize, out _cellsCount, out _cellsSize, out _usedCount);
				num += _cellsSize + _arrayCount * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (damageCells != null)
			{
				damageCells.Stats(out _arrayCount, out _arraySize, out _cellsCount, out _cellsSize, out _usedCount);
				num += _cellsSize + _arrayCount * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (densityCells != null)
			{
				densityCells.Stats(out _arrayCount, out _arraySize, out _cellsCount, out _cellsSize, out _usedCount);
				num += _cellsSize + _arrayCount * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (textureCells != null)
			{
				textureCells.Stats(out _arrayCount, out _arraySize, out _cellsCount, out _cellsSize, out _usedCount);
				num += _cellsSize + _arrayCount * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (waterCells != null)
			{
				waterCells.Stats(out _arrayCount, out _arraySize, out _cellsCount, out _cellsSize, out _usedCount);
				num += _cellsSize + _arrayCount * IntPtr.Size;
			}
		}
		return num + MemoryTracker.GetSize(TriggerLayers);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitData()
	{
		if (!isCellsDataOwner)
		{
			Log.Error("InitData failed: Cannot set block data on non-owning Prefab instance.");
			return;
		}
		blockCells = new Cells<uint>(size.y, 0u);
		damageCells = new Cells<ushort>(size.y, 0);
		densityCells = new Cells<sbyte>(size.y, MarchingCubes.DensityAir);
		textureCells = new Cells<TextureFullArray>(size.y, TextureFullArray.Default);
		waterCells = new Cells<WaterValue>(size.y, WaterValue.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitTerrainFillers()
	{
		terrainFillerType = Block.GetBlockValue(Constants.cTerrainFillerBlockName).type;
		terrainFiller2Type = Block.GetBlockValue(Constants.cTerrainFiller2BlockName).type;
	}

	public Prefab Clone(bool _sharedData = false)
	{
		return new Prefab(this, _sharedData);
	}

	public int GetLocalRotation()
	{
		return localRotation;
	}

	public void SetLocalRotation(int _rot)
	{
		localRotation = _rot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CoordToOffset(int _localRotation, int _x, int _y, int _z)
	{
		return _localRotation switch
		{
			1 => _z + _y * size.z + (size.x - _x - 1) * size.z * size.y, 
			2 => size.x - _x - 1 + _y * size.x + (size.z - _z - 1) * size.x * size.y, 
			3 => size.z - _z - 1 + _y * size.z + _x * size.z * size.y, 
			_ => _x + _y * size.x + _z * size.x * size.y, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoord(int _offset, out int _x, out int _y, out int _z)
	{
		int num = size.x * size.y;
		_z = _offset / num;
		_offset %= num;
		_y = _offset / size.x;
		_x = _offset % size.x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoordRotated(int _offset, out int _x, out int _y, out int _z)
	{
		switch (localRotation)
		{
		default:
		{
			int num = size.x * size.y;
			_z = _offset / num;
			_offset %= num;
			_y = _offset / size.x;
			_x = _offset % size.x;
			break;
		}
		case 1:
			_x = -(_offset / (size.z * size.y) - size.x + 1);
			_offset %= size.z * size.y;
			_y = _offset / size.z;
			_z = _offset % size.z;
			break;
		case 2:
			_z = -(_offset / (size.x * size.y) - size.z + 1);
			_offset %= size.x * size.y;
			_y = _offset / size.x;
			_offset %= size.x;
			_x = -(_offset - size.x + 1);
			break;
		case 3:
			_x = _offset / (size.z * size.y);
			_offset %= size.z * size.y;
			_y = _offset / size.z;
			_offset %= size.z;
			_z = -(_offset - size.z + 1);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(ref int _x, ref int _z)
	{
		switch (localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = size.x - num - 1;
			break;
		}
		case 2:
			_x = size.x - _x - 1;
			_z = size.z - _z - 1;
			break;
		case 3:
		{
			int num = _x;
			_x = size.z - _z - 1;
			_z = num;
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(int _rot, ref int _x, ref int _z)
	{
		switch (_rot)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = size.x - num - 1;
			break;
		}
		case 2:
			_x = size.x - _x - 1;
			_z = size.z - _z - 1;
			break;
		case 3:
		{
			int num = _x;
			_x = size.z - _z - 1;
			_z = num;
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InverseRotateRelative(ref int _x, ref int _z)
	{
		switch (localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			break;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			break;
		case 3:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateRelative(ref int _x, ref int _z)
	{
		switch (localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			break;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			break;
		case 3:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			break;
		}
		}
	}

	public void SetBlock(int _x, int _y, int _z, BlockValue _bv)
	{
		if (_bv.isWater)
		{
			Log.Warning("Prefabs should no longer store water blocks. Please use SetWater instead");
			SetWater(_x, _y, _z, WaterValue.Full);
		}
		else if (!isCellsDataOwner)
		{
			Log.Error("SetBlock failed: Cannot set block data on non-owning Prefab instance.");
		}
		else if ((uint)_x < size.x && (uint)_y < size.y && (uint)_z < size.z)
		{
			RotateCoords(ref _x, ref _z);
			blockCells.SetData(_x, _y, _z, _bv.rawData);
			damageCells.SetData(_x, _y, _z, (ushort)_bv.damage);
		}
	}

	public float GetHeight(int _x, int _z, bool _terrainOnly = true)
	{
		for (int num = size.y; num >= 0; num--)
		{
			BlockValue block = GetBlock(_x, num, _z);
			if (!block.isair && !(!block.Block.shape.IsTerrain() && _terrainOnly))
			{
				float num2 = 1f - (float)(int)(byte)block.Block.Density / 255f;
				return (float)(num - 1) + num2;
			}
		}
		return 0f;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if ((uint)_x >= size.x || (uint)_y >= size.y || (uint)_z >= size.z)
		{
			return BlockValue.Air;
		}
		BlockValue _bv = BlockValue.Air;
		RotateCoords(ref _x, ref _z);
		Cells<uint>.Cell cell = blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			_bv.rawData = cell.Get(_x, _z);
			Cells<ushort>.Cell cell2 = damageCells.GetCell(_x, _y, _z);
			if (cell2.a != null)
			{
				_bv.damage = cell2.Get(_x, _z);
			}
			if (!isCellsDataOwner && localRotation != 0)
			{
				ApplyRotation(ref _bv);
			}
		}
		return _bv;
	}

	public BlockValue GetBlockNoDamage(int _localRotation, int _x, int _y, int _z)
	{
		BlockValue _bv = BlockValue.Air;
		RotateCoords(_localRotation, ref _x, ref _z);
		Cells<uint>.Cell cell = blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			_bv.rawData = cell.Get(_x, _z);
			if (!isCellsDataOwner && localRotation != 0)
			{
				ApplyRotation(ref _bv);
			}
		}
		return _bv;
	}

	public PropValue GetProp(long chunkKey, int propId)
	{
		return GetProp(WorldChunkCache.extractX(chunkKey), WorldChunkCache.extractZ(chunkKey), propId);
	}

	public PropValue GetProp(int chunkX, int chunkZ, int propId)
	{
		if (!propsByChunk.TryGetValue(new Vector2i(chunkX, chunkZ), out var value) || !value.TryGetValue(propId, out var value2))
		{
			return PropValue.AIR;
		}
		return value2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRotation(ref BlockValue _bv)
	{
		if (_bv.ischild)
		{
			int _x = _bv.parentx;
			int _z = _bv.parentz;
			if (_x != 0 || _z != 0)
			{
				InverseRotateRelative(ref _x, ref _z);
				_bv.parentx = _x;
				_bv.parentz = _z;
			}
		}
		else
		{
			_bv = _bv.Block.shape.RotateY(_bLeft: true, _bv, localRotation);
		}
	}

	public BlockValue GetBlockNoDamage(int _offs)
	{
		return BlockValue.Air;
	}

	public int GetBlockCount()
	{
		return size.x * size.y * size.z;
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		RotateCoords(ref _x, ref _z);
		return waterCells.GetData(_x, _y, _z);
	}

	public void SetWater(int _x, int _y, int _z, WaterValue _wv)
	{
		if ((uint)_x < size.x && (uint)_y < size.y && (uint)_z < size.z)
		{
			RotateCoords(ref _x, ref _z);
			waterCells.SetData(_x, _y, _z, _wv);
		}
	}

	public byte GetStab(int _relx, int _absy, int _relz)
	{
		return 0;
	}

	public void SetDensity(int _x, int _y, int _z, sbyte _density)
	{
		RotateCoords(ref _x, ref _z);
		densityCells.SetData(_x, _y, _z, _density);
	}

	public sbyte GetDensity(int _x, int _y, int _z)
	{
		RotateCoords(ref _x, ref _z);
		return densityCells.GetData(_x, _y, _z);
	}

	public sbyte GetDensity(int _localRotation, int _x, int _y, int _z)
	{
		RotateCoords(_localRotation, ref _x, ref _z);
		return densityCells.GetData(_x, _y, _z);
	}

	public void SetTexture(int _x, int _y, int _z, TextureFullArray _fullTexture)
	{
		RotateCoords(ref _x, ref _z);
		textureCells.SetData(_x, _y, _z, _fullTexture);
	}

	public TextureFullArray GetTexture(int _x, int _y, int _z)
	{
		RotateCoords(ref _x, ref _z);
		return textureCells.GetData(_x, _y, _z);
	}

	public bool IsInsidePrefab(int _x, int _y, int _z)
	{
		int x;
		int y;
		int z;
		switch (localRotation)
		{
		default:
			x = _x;
			y = _y;
			z = _z;
			break;
		case 1:
			x = _z;
			y = _y;
			z = size.x - _x - 1;
			break;
		case 2:
			x = size.x - _x - 1;
			y = _y;
			z = size.z - _z - 1;
			break;
		case 3:
			x = size.z - _z - 1;
			y = _y;
			z = _x;
			break;
		}
		return insidePos.Contains(x, y, z);
	}

	public void ToggleQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		if (GetQuestTag(_questTag))
		{
			questTags = questTags.Remove(_questTag);
		}
		else
		{
			questTags |= _questTag;
		}
	}

	public FastTags<TagGroup.Global> GetQuestTags()
	{
		return new FastTags<TagGroup.Global>(questTags);
	}

	public bool GetQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		return questTags.Test_AllSet(_questTag);
	}

	public bool HasAnyQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		return questTags.Test_AnySet(_questTag);
	}

	public bool HasQuestTag()
	{
		return !questTags.IsEmpty;
	}

	public TileEntity GetTileEntity(Vector3i _blockPos)
	{
		switch (localRotation)
		{
		case 1:
		{
			int x = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = size.x - x - 1;
			break;
		}
		case 2:
			_blockPos.x = size.x - _blockPos.x - 1;
			_blockPos.z = size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int x = _blockPos.x;
			_blockPos.x = size.z - _blockPos.z - 1;
			_blockPos.z = x;
			break;
		}
		}
		if (tileEntities.TryGetValue(_blockPos, out var value))
		{
			return value;
		}
		return null;
	}

	public BlockTrigger GetBlockTrigger(Vector3i _blockPos)
	{
		switch (localRotation)
		{
		case 1:
		{
			int x = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = size.x - x - 1;
			break;
		}
		case 2:
			_blockPos.x = size.x - _blockPos.x - 1;
			_blockPos.z = size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int x = _blockPos.x;
			_blockPos.x = size.z - _blockPos.z - 1;
			_blockPos.z = x;
			break;
		}
		}
		if (triggerData.TryGetValue(_blockPos, out var value))
		{
			return value;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadFromProperties()
	{
		bCopyAirBlocks = properties.GetBool("CopyAirBlocks");
		bExcludeDistantPOIMesh = properties.GetBool("ExcludeDistantPOIMesh");
		bExcludePOICulling = properties.GetBool("ExcludePOICulling");
		distantPOIYOffset = properties.GetFloat("DistantPOIYOffset");
		properties.ParseString("DistantPOIOverride", ref distantPOIOverride);
		bAllowTopSoilDecorations = properties.GetBool("AllowTopSoilDecorations");
		editorGroups.Clear();
		if (properties.Values.ContainsKey("EditorGroups"))
		{
			editorGroups.AddRange(properties.GetString("EditorGroups").Split(','));
			for (int i = 0; i < editorGroups.Count; i++)
			{
				editorGroups[i] = editorGroups[i].Trim();
			}
		}
		if (properties.Values.ContainsKey("DifficultyTier"))
		{
			DifficultyTier = (byte)properties.GetInt("DifficultyTier");
		}
		properties.ParseInt("ShowQuestClearCount", ref ShowQuestClearCount);
		bTraderArea = properties.GetBool("TraderArea");
		TraderAreaProtect = (properties.Values.ContainsKey("TraderAreaProtect") ? StringParsers.ParseVector3i(properties.Values["TraderAreaProtect"]) : Vector3i.zero);
		foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
		{
			allVolumeList.ReadFromProperties(properties);
		}
		yOffset = properties.GetInt("YOffset");
		if (size == Vector3i.zero && properties.Values.ContainsKey("PrefabSize"))
		{
			size = StringParsers.ParseVector3i(properties.Values["PrefabSize"]);
		}
		rotationToFaceNorth = properties.GetInt("RotationToFaceNorth");
		if (properties.Values.ContainsKey("Tags"))
		{
			tags = FastTags<TagGroup.Poi>.Parse(properties.Values["Tags"].Replace(" ", ""));
		}
		if (properties.Values.ContainsKey("ThemeTags"))
		{
			themeTags = FastTags<TagGroup.Poi>.Parse(properties.Values["ThemeTags"].Replace(" ", ""));
		}
		if (properties.Values.ContainsKey("ThemeRepeatDistance"))
		{
			themeRepeatDistance = StringParsers.ParseSInt32(properties.Values["ThemeRepeatDistance"]);
		}
		if (properties.Values.ContainsKey("DuplicateRepeatDistance"))
		{
			duplicateRepeatDistance = StringParsers.ParseSInt32(properties.Values["DuplicateRepeatDistance"]);
		}
		indexedBlockOffsets.Clear();
		if (properties.Classes.ContainsKey("IndexedBlockOffsets"))
		{
			foreach (KeyValuePair<string, DynamicProperties> @class in properties.Classes["IndexedBlockOffsets"].Classes)
			{
				if (@class.Value.Values.Count <= 0)
				{
					continue;
				}
				List<Vector3i> list = new List<Vector3i>();
				indexedBlockOffsets[@class.Key] = list;
				foreach (KeyValuePair<string, string> value in @class.Value.Values)
				{
					list.Add(StringParsers.ParseVector3i(@class.Value.Values[value.Key]));
				}
			}
		}
		if (properties.Values.ContainsKey("QuestTags"))
		{
			questTags = FastTags<TagGroup.Global>.Parse(properties.Values["QuestTags"]);
		}
		if (properties.Values.ContainsKey("AllowedTownships"))
		{
			allowedTownships.Clear();
			string[] array = properties.Values["AllowedTownships"].Replace(" ", "").Split(',');
			foreach (string text in array)
			{
				allowedTownships.Add(text.ToLower());
			}
		}
		if (properties.Values.ContainsKey("AllowedBiomes"))
		{
			allowedBiomes.Clear();
			string[] array = properties.Values["AllowedBiomes"].Replace(" ", "").Split(',');
			foreach (string text2 in array)
			{
				allowedBiomes.Add(text2.ToLower());
			}
		}
		if (properties.Values.ContainsKey("Zoning"))
		{
			allowedZones.Clear();
			string[] array2 = properties.Values["Zoning"].Split(',');
			for (int k = 0; k < array2.Length; k++)
			{
				AddAllowedZone(array2[k].Trim());
			}
		}
		else
		{
			allowedZones.Add("none");
		}
		if (properties.Classes.ContainsKey("Stats"))
		{
			renderingCost = WorldStats.FromProperties(properties.Classes["Stats"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeToProperties()
	{
		properties.Values["CopyAirBlocks"] = bCopyAirBlocks.ToString();
		properties.Values["ExcludeDistantPOIMesh"] = bExcludeDistantPOIMesh.ToString();
		properties.Values["ExcludePOICulling"] = bExcludePOICulling.ToString();
		properties.Values["DistantPOIYOffset"] = distantPOIYOffset.ToCultureInvariantString();
		if (distantPOIOverride != null)
		{
			properties.Values["DistantPOIOverride"] = distantPOIOverride;
		}
		properties.Values.Remove("EditorGroups");
		if (editorGroups.Count > 0)
		{
			string text = string.Empty;
			for (int i = 0; i < editorGroups.Count; i++)
			{
				text = text + editorGroups[i] + ((i < editorGroups.Count - 1) ? ", " : string.Empty);
			}
			properties.Values["EditorGroups"] = text;
		}
		properties.Values["AllowTopSoilDecorations"] = bAllowTopSoilDecorations.ToString();
		properties.Values["DifficultyTier"] = DifficultyTier.ToString();
		properties.Values["ShowQuestClearCount"] = ShowQuestClearCount.ToString();
		foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
		{
			allVolumeList.WriteToProperties(properties);
		}
		if (yOffset != 0)
		{
			properties.Values["YOffset"] = yOffset.ToString();
		}
		else
		{
			properties.Values.Remove("YOffset");
		}
		properties.Values["PrefabSize"] = size.ToString();
		properties.Values["RotationToFaceNorth"] = rotationToFaceNorth.ToString();
		string text2 = "";
		for (int j = 0; j < allowedTownships.Count; j++)
		{
			text2 = text2 + allowedTownships[j] + ((j < allowedTownships.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			properties.Values["AllowedTownships"] = text2;
		}
		else
		{
			properties.Values.Remove("AllowedTownships");
		}
		text2 = "";
		for (int k = 0; k < allowedBiomes.Count; k++)
		{
			text2 = text2 + allowedBiomes[k] + ((k < allowedBiomes.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			properties.Values["AllowedBiomes"] = text2;
		}
		else
		{
			properties.Values.Remove("AllowedBiomes");
		}
		if (tags.ToString() != "")
		{
			properties.Values["Tags"] = tags.ToString();
		}
		else
		{
			properties.Values.Remove("Tags");
		}
		if (themeTags.ToString() != "")
		{
			properties.Values["ThemeTags"] = themeTags.ToString();
		}
		else
		{
			properties.Values.Remove("ThemeTags");
		}
		if (themeRepeatDistance != 300)
		{
			properties.Values["ThemeRepeatDistance"] = themeRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			properties.Values.Remove("ThemeRepeatDistance");
		}
		if (duplicateRepeatDistance != 1000)
		{
			properties.Values["DuplicateRepeatDistance"] = duplicateRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			properties.Values.Remove("DuplicateRepeatDistance");
		}
		if (indexedBlockOffsets.Any([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, List<Vector3i>> _pair) => _pair.Value.Count > 0))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			properties.Classes["IndexedBlockOffsets"] = dynamicProperties;
			foreach (KeyValuePair<string, List<Vector3i>> indexedBlockOffset in indexedBlockOffsets)
			{
				if (indexedBlockOffset.Value.Count > 0)
				{
					DynamicProperties dynamicProperties2 = new DynamicProperties();
					dynamicProperties.Classes[indexedBlockOffset.Key] = dynamicProperties2;
					for (int num = 0; num < indexedBlockOffset.Value.Count; num++)
					{
						dynamicProperties2.Values[num.ToString()] = indexedBlockOffset.Value[num].ToString();
					}
				}
			}
		}
		else
		{
			properties.Classes.Remove("IndexedBlockOffsets");
		}
		if (!questTags.IsEmpty)
		{
			text2 = questTags.ToString();
		}
		if (text2.Length > 0)
		{
			properties.Values["QuestTags"] = text2;
		}
		else
		{
			properties.Values.Remove("QuestTags");
		}
		properties.Values.Remove("Zoning");
		if (allowedZones.Count > 0)
		{
			string text3 = string.Empty;
			for (int num2 = 0; num2 < allowedZones.Count; num2++)
			{
				text3 = text3 + allowedZones[num2] + ((num2 < allowedZones.Count - 1) ? ", " : string.Empty);
			}
			properties.Values["Zoning"] = text3;
		}
		if (renderingCost != null)
		{
			properties.Classes["Stats"] = renderingCost.ToProperties();
		}
	}

	public static bool PrefabExists(string _prefabFileName)
	{
		return PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFileName).Type != PathAbstractions.EAbstractedLocationType.None;
	}

	public bool Load(string _prefabName, bool _applyMapping = true, bool _fixChildBlocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		return Load(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName), _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData);
	}

	public bool Load(PathAbstractions.AbstractedLocation _location, bool _applyMapping = true, bool _fixChildBlocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				Log.Warning("Prefab loading failed. Prefab '{0}' does not exist!", _location.Name);
			}
			else
			{
				Log.Error("Prefab loading failed. Prefab '{0}' does not exist!", _location.Name);
			}
			return false;
		}
		location = _location;
		if (_skipLoadingBlockData)
		{
			if (!loadSizeDataOnly(_location, _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData))
			{
				return false;
			}
		}
		else if (!loadBlockData(_location, _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData))
		{
			return false;
		}
		loadWorldSignData(_location);
		return LoadXMLData(_location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadSizeDataOnly(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildBlocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		using Stream baseStream = SdFile.OpenRead(_location.FullPath);
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(baseStream);
		if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != 0)
		{
			return false;
		}
		pooledBinaryReader.ReadUInt32();
		size = default(Vector3i);
		size.x = pooledBinaryReader.ReadInt16();
		size.y = pooledBinaryReader.ReadInt16();
		size.z = pooledBinaryReader.ReadInt16();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadBlockData(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildBlocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		bool result = true;
		ArrayListMP<int> arrayListMP = null;
		if (_applyMapping)
		{
			arrayListMP = loadIdMapping(_location.Folder, _location.FileNameNoExtension, _allowMissingBlocks);
			if (arrayListMP == null)
			{
				return false;
			}
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(_location.FullPath);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != 0)
			{
				return false;
			}
			uint num = pooledBinaryReader.ReadUInt32();
			int[] blockIdMapping = arrayListMP?.Items;
			if (!readBlockData(pooledBinaryReader, num, blockIdMapping, _fixChildBlocks: true))
			{
				return false;
			}
			if (num > 12)
			{
				readTileEntities(pooledBinaryReader, blockIdMapping);
			}
			if (num > 15)
			{
				readTriggerData(pooledBinaryReader);
			}
			insidePos.Load(_location.FullPathNoExtension + ".ins", size);
		}
		catch (Exception e)
		{
			Log.Exception(e);
			result = false;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadWorldSignData(PathAbstractions.AbstractedLocation _location)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		if (PrefabEditModeManager.Instance != null && SdFile.Exists(_location.FullPathNoExtension + "_signs.xml"))
		{
			try
			{
				XmlFile xmlFile = new XmlFile(_location.Folder, _location.FileNameNoExtension + "_signs");
				SignLibrary signLibrary = new SignLibrary();
				signLibrary.ReadXml(xmlFile);
				SignDataManager.Instance.TryRegisterLibrary(_location.FileNameNoExtension, signLibrary, PrefabEditModeManager.Instance.IsActive());
				return true;
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}
		return false;
	}

	public bool LoadXMLData(PathAbstractions.AbstractedLocation _location)
	{
		location = _location;
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return true;
		}
		if (!properties.Load(_location.Folder, _location.Name))
		{
			return false;
		}
		ReadFromProperties();
		return true;
	}

	public bool Save(string _prefabName, bool _createMapping = true)
	{
		return Save(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName), _createMapping);
	}

	public bool Save(PathAbstractions.AbstractedLocation _location, bool _createMapping = true)
	{
		if (saveBlockData(_location, _createMapping) && SaveXMLData(_location))
		{
			if (SignDataManager.Instance.TryGetSignLibrary(_location.FileNameNoExtension, out var library))
			{
				string filePath = _location.FullPathNoExtension + "_signs.xml";
				library.WriteXml(filePath);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddAllChildBlocks()
	{
		if (!isCellsDataOwner || blockCells == null || blockCells.a == null)
		{
			return;
		}
		int num = blockCells.a.Length;
		for (int i = 0; i < num; i++)
		{
			Cells<uint>.CellsAtZ cellsAtZ = blockCells.a[i];
			if (cellsAtZ == null)
			{
				continue;
			}
			int num2 = cellsAtZ.a.Length;
			for (int j = 0; j < num2; j++)
			{
				Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
				if (cellsAtX == null)
				{
					continue;
				}
				int num3 = cellsAtX.a.Length;
				for (int k = 0; k < num3; k++)
				{
					Cells<uint>.Cell cell = cellsAtX.a[k];
					if (cell.a == null)
					{
						continue;
					}
					for (int l = 0; l < cell.a.Length; l++)
					{
						uint num4 = cell.a[l];
						if ((num4 & 0xFFFF) == 0)
						{
							continue;
						}
						BlockValue blockValue = new BlockValue(num4);
						if (blockValue.rawData == 0 || blockValue.ischild)
						{
							continue;
						}
						Block block = blockValue.Block;
						if (block == null || !block.isMultiBlock)
						{
							continue;
						}
						int num5 = (k << 2) + (l & 3);
						int num6 = i;
						int num7 = (j << 2) + (l >> 2);
						int rotation = blockValue.rotation;
						for (int num8 = block.multiBlockPos.Length - 1; num8 >= 0; num8--)
						{
							Vector3i vector3i = block.multiBlockPos.Get(num8, blockValue.type, rotation);
							if (!(vector3i == Vector3i.zero))
							{
								int _x = vector3i.x;
								int y = vector3i.y;
								int _z = vector3i.z;
								blockValue.ischild = true;
								blockValue.parentx = -_x;
								blockValue.parenty = -y;
								blockValue.parentz = -_z;
								RotateRelative(ref _x, ref _z);
								int num9 = num5 + _x;
								int num10 = num6 + y;
								int num11 = num7 + _z;
								if ((uint)num9 < size.x && (uint)num10 < size.y && (uint)num11 < size.z)
								{
									blockCells.SetData(num9, num10, num11, blockValue.rawData);
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAllChildAndOldBlocks()
	{
		for (int num = size.y - 1; num >= 0; num--)
		{
			for (int num2 = size.z - 1; num2 >= 0; num2--)
			{
				for (int num3 = size.x - 1; num3 >= 0; num3--)
				{
					BlockValue block = GetBlock(num3, num, num2);
					Block block2 = block.Block;
					if (block2 == null)
					{
						SetBlock(num3, num, num2, BlockValue.Air);
					}
					else if (block.ischild)
					{
						SetBlock(num3, num, num2, BlockValue.Air);
					}
					else if (block2 is BlockModelTree && (block.meta & 1) != 0)
					{
						SetBlock(num3, num, num2, BlockValue.Air);
					}
				}
			}
		}
	}

	public bool SaveXMLData(PathAbstractions.AbstractedLocation _location)
	{
		writeToProperties();
		return properties.Save("prefab", _location.Folder, _location.FileNameNoExtension);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveBlockData(PathAbstractions.AbstractedLocation _location, bool _createMapping)
	{
		RemoveAllChildAndOldBlocks();
		if (_createMapping)
		{
			NameIdMapping nameIdMapping = new NameIdMapping(_location.FullPathNoExtension + ".blocks.nim", Block.MAX_BLOCKS);
			for (int num = GetBlockCount() - 1; num >= 0; num--)
			{
				offsetToCoord(num, out var _x, out var _y, out var _z);
				Block block = GetBlock(_x, _y, _z).Block;
				nameIdMapping.AddMapping(block.blockID, block.GetBlockName());
			}
			nameIdMapping.WriteToFile();
		}
		try
		{
			using (Stream baseStream = SdFile.Open(_location.FullPath, FileMode.Create))
			{
				using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
				pooledBinaryWriter.SetBaseStream(baseStream);
				pooledBinaryWriter.Write('t');
				pooledBinaryWriter.Write('t');
				pooledBinaryWriter.Write('s');
				pooledBinaryWriter.Write((byte)0);
				pooledBinaryWriter.Write((uint)CurrentSaveVersion);
				writeBlockData(pooledBinaryWriter);
				writeTileEntities(pooledBinaryWriter);
				writeTriggerData(pooledBinaryWriter);
				if (IsCullThisPrefab())
				{
					insidePos.Save(_location.FullPathNoExtension + ".ins");
				}
				else
				{
					SdFile.Delete(_location.FullPathNoExtension + ".ins");
				}
			}
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool IsCullThisPrefab()
	{
		return !bExcludePOICulling;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeBlockData(BinaryWriter _bw)
	{
		_bw.Write((short)size.x);
		_bw.Write((short)size.y);
		_bw.Write((short)size.z);
		Data data = CellsToArrays();
		uint[] blocks = data.m_Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			_bw.Write(blocks[i]);
		}
		_bw.Write(data.m_Density);
		byte[] array = new byte[data.m_Damage.Length * 2];
		for (int j = 0; j < data.m_Damage.Length; j++)
		{
			array[j * 2] = (byte)(data.m_Damage[j] & 0xFF);
			array[j * 2 + 1] = (byte)((data.m_Damage[j] >> 8) & 0xFF);
		}
		_bw.Write(array);
		SimpleBitStream simpleBitStream = new SimpleBitStream();
		for (int k = 0; k < data.m_Textures.Length; k++)
		{
			bool b = !data.m_Textures[k].IsDefault;
			simpleBitStream.Add(b);
		}
		simpleBitStream.Write(_bw);
		for (int l = 0; l < data.m_Textures.Length; l++)
		{
			if (!data.m_Textures[l].IsDefault)
			{
				data.m_Textures[l].Write(_bw);
			}
		}
		SimpleBitStream simpleBitStream2 = new SimpleBitStream();
		for (int m = 0; m < data.m_Water.Length; m++)
		{
			simpleBitStream2.Add(data.m_Water[m].HasMass());
		}
		simpleBitStream2.Write(_bw);
		for (int n = 0; n < data.m_Water.Length; n++)
		{
			WaterValue waterValue = data.m_Water[n];
			if (waterValue.HasMass())
			{
				waterValue.Write(_bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTileEntities(BinaryWriter _bw)
	{
		_bw.Write((short)tileEntities.Count);
		foreach (KeyValuePair<Vector3i, TileEntity> tileEntity in tileEntities)
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				tileEntity.Value.write(pooledBinaryWriter, TileEntity.StreamModeWrite.Persistency);
			}
			_bw.Write((short)pooledExpandableMemoryStream.Length);
			_bw.Write((byte)tileEntity.Value.GetTileEntityType());
			pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTriggerData(BinaryWriter _bw)
	{
		_bw.Write((short)triggerData.Count);
		foreach (KeyValuePair<Vector3i, BlockTrigger> triggerDatum in triggerData)
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				triggerDatum.Value.Write(pooledBinaryWriter);
			}
			_bw.Write((short)pooledExpandableMemoryStream.Length);
			StreamUtils.Write(_bw, triggerDatum.Key);
			pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool readBlockData(PooledBinaryReader _br, uint _version, int[] _blockIdMapping, bool _fixChildBlocks)
	{
		statistics.Clear();
		multiBlockParentIndices.Clear();
		decoAllowedBlockIndices.Clear();
		localRotation = 0;
		size.x = _br.ReadInt16();
		size.y = _br.ReadInt16();
		size.z = _br.ReadInt16();
		int blockCount = GetBlockCount();
		InitData();
		sharedData.Expand(blockCount);
		Data _data = sharedData;
		if (_version >= 2 && _version < 7)
		{
			bCopyAirBlocks = _br.ReadBoolean();
		}
		if (_version >= 3 && _version < 7)
		{
			bAllowTopSoilDecorations = _br.ReadBoolean();
		}
		List<Vector3i> list = null;
		int num = blockTypeMissingBlock;
		if (_blockIdMapping != null && num >= 0)
		{
			list = new List<Vector3i>();
		}
		int num2 = blockCount * 4;
		if (tempBuf == null || tempBuf.Length < num2)
		{
			tempBuf = new byte[Utils.FastMax(200000, num2)];
		}
		int num3 = 0;
		_br.Read(tempBuf, 0, blockCount * 4);
		if (_version <= 4)
		{
			for (int i = 0; i < size.x; i++)
			{
				for (int j = 0; j < size.z; j++)
				{
					for (int k = 0; k < size.y; k++)
					{
						BlockValue bv = new BlockValue((uint)(tempBuf[num3] | (tempBuf[num3 + 1] << 8) | (tempBuf[num3 + 2] << 16) | (tempBuf[num3 + 3] << 24)));
						num3 += 4;
						if (_blockIdMapping != null)
						{
							int num4 = _blockIdMapping[bv.type];
							if (num4 < 0)
							{
								Log.Error("Loading prefab \"" + location.ToString() + "\" failed: Block " + bv.type + " used in prefab has no mapping.");
								return false;
							}
							bv.type = num4;
							if (num >= 0 && bv.type == blockTypeMissingBlock)
							{
								list.Add(new Vector3i(i, k, j));
							}
						}
						if (bv.isWater)
						{
							SetWater(i, k, j, WaterValue.Full);
							continue;
						}
						if (_fixChildBlocks)
						{
							if (bv.ischild)
							{
								continue;
							}
							Block block = bv.Block;
							if (block == null || ((bv.meta & 1) != 0 && block is BlockModelTree))
							{
								continue;
							}
						}
						SetBlock(i, k, j, bv);
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < blockCount; l++)
			{
				uint num5 = (uint)(tempBuf[num3] | (tempBuf[num3 + 1] << 8) | (tempBuf[num3 + 2] << 16) | (tempBuf[num3 + 3] << 24));
				num3 += 4;
				_data.m_Blocks[l] = 0u;
				if (num5 == 0)
				{
					continue;
				}
				if (_version < 18)
				{
					num5 = BlockValueV3.ConvertOldRawData(num5);
				}
				BlockValue blockValue = new BlockValue(num5);
				if (_blockIdMapping != null)
				{
					int type = blockValue.type;
					if (type != 0)
					{
						int num6 = _blockIdMapping[type];
						if (num6 < 0)
						{
							offsetToCoord(l, out var _x, out var _y, out var _z);
							Log.Error("Loading prefab \"" + location.ToString() + "\" failed: Block " + type + " used in prefab at " + _x + " / " + _y + " / " + _z + " has no mapping.");
							return false;
						}
						blockValue.type = num6;
						if (num >= 0 && num6 == blockTypeMissingBlock)
						{
							offsetToCoord(l, out var _x2, out var _y2, out var _z2);
							list.Add(new Vector3i(_x2, _y2, _z2));
						}
					}
				}
				if (_version < 17 && blockValue.isWater)
				{
					_data.m_Water[l] = WaterValue.Full;
					continue;
				}
				Block block2 = blockValue.Block;
				updateBlockStatistics(blockValue, block2);
				if (!_fixChildBlocks || (!blockValue.ischild && block2 != null && ((blockValue.meta & 1) == 0 || !(block2 is BlockModelTree))))
				{
					if (block2.isMultiBlock && !blockValue.ischild)
					{
						multiBlockParentIndices.Add(l);
					}
					if (DecoUtils.HasDecoAllowed(blockValue))
					{
						decoAllowedBlockIndices.Add(l);
					}
					_data.m_Blocks[l] = blockValue.rawData;
				}
			}
			_br.Read(_data.m_Density, 0, size.x * size.y * size.z);
		}
		if (_blockIdMapping != null && num >= 0)
		{
			foreach (Vector3i item in list)
			{
				SetDensity(item.x, item.y, item.z, MarchingCubes.DensityAir);
			}
		}
		if (_version > 8)
		{
			_br.Read(tempBuf, 0, blockCount * 2);
			for (int m = 0; m < blockCount; m++)
			{
				_data.m_Damage[m] = (ushort)(tempBuf[m * 2] | (tempBuf[m * 2 + 1] << 8));
			}
		}
		if (_version >= 10)
		{
			simpleBitStreamReader.Reset();
			simpleBitStreamReader.Read(_br);
			if (_version >= 19)
			{
				while ((num3 = simpleBitStreamReader.GetNextOffset()) >= 0)
				{
					_data.m_Textures[num3].Read(_br);
				}
			}
			else
			{
				while ((num3 = simpleBitStreamReader.GetNextOffset()) >= 0)
				{
					_data.m_Textures[num3][0] = _br.ReadInt64();
				}
			}
		}
		entities.Clear();
		if (_version >= 4 && _version < 12)
		{
			int num7 = _br.ReadInt16();
			for (int n = 0; n < num7; n++)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.read(_br, _bNetworkRead: false);
				entities.Add(entityCreationData);
			}
		}
		if (_version >= 17)
		{
			simpleBitStreamReader.Reset();
			simpleBitStreamReader.Read(_br);
			while ((num3 = simpleBitStreamReader.GetNextOffset()) >= 0)
			{
				_data.m_Water[num3] = WaterValue.FromStream(_br);
			}
		}
		CellsFromArrays(ref _data);
		if (_fixChildBlocks)
		{
			AddAllChildBlocks();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CellsFromArrays(ref Data _data)
	{
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < size.y; i++)
		{
			for (int num = size.z - 1; num >= 0; num--)
			{
				for (int num2 = size.x - 1; num2 >= 0; num2--)
				{
					int num3 = CoordToOffset(0, num2, i, num);
					air.rawData = _data.m_Blocks[num3];
					if (!air.isair)
					{
						blockCells.AllocCell(num2, i, num).Set(num2, num, air.rawData);
					}
					ushort num4 = _data.m_Damage[num3];
					if (num4 != 0)
					{
						damageCells.AllocCell(num2, i, num).Set(num2, num, num4);
					}
					sbyte b = (sbyte)_data.m_Density[num3];
					if (b != densityCells.defaultValue)
					{
						densityCells.AllocCell(num2, i, num).Set(num2, num, b);
					}
					TextureFullArray value = _data.m_Textures[num3];
					if (!value.IsDefault)
					{
						textureCells.AllocCell(num2, i, num).Set(num2, num, value);
					}
					WaterValue value2 = _data.m_Water[num3];
					if (value2.HasMass())
					{
						waterCells.AllocCell(num2, i, num).Set(num2, num, value2);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Data CellsToArrays()
	{
		Data result = default(Data);
		result.m_Blocks = blockCells.ToArray(this, size);
		result.m_Damage = damageCells.ToArray(this, size);
		result.m_Density = (byte[])(object)densityCells.ToArray(this, size);
		result.m_Textures = textureCells.ToArray(this, size);
		result.m_Water = waterCells.ToArray(this, size);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTileEntities(PooledBinaryReader _br, int[] _blockIdMapping)
	{
		tileEntities.Clear();
		int num = _br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int length = _br.ReadInt16();
			TileEntityType type = (TileEntityType)_br.ReadByte();
			try
			{
				TileEntity tileEntity = null;
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, length);
					pooledExpandableMemoryStream.Position = 0L;
					using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
					pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
					tileEntity = TileEntity.InstantiateFromRead(pooledBinaryReader, TileEntity.StreamModeRead.Persistency, type, null, _blockIdMapping, GetBlock);
				}
				Block block = GetBlock(tileEntity.localChunkPos.x, tileEntity.localChunkPos.y, tileEntity.localChunkPos.z).Block;
				if (block == null || block.IsTileEntitySavedInPrefab())
				{
					tileEntities.Add(tileEntity.localChunkPos, tileEntity);
				}
			}
			catch (Exception e)
			{
				Log.Error("Skipping loading of active block data for " + PrefabName + " because of the following exception:");
				Log.Exception(e);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTriggerData(PooledBinaryReader _br)
	{
		triggerData.Clear();
		TriggerLayers.Clear();
		int num = _br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int length = _br.ReadInt16();
			Vector3i localChunkPos = StreamUtils.ReadVector3i(_br);
			try
			{
				BlockTrigger blockTrigger = new BlockTrigger(null);
				blockTrigger.LocalChunkPos = localChunkPos;
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, length);
					pooledExpandableMemoryStream.Position = 0L;
					using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
					pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
					blockTrigger.Read(pooledBinaryReader);
				}
				if (!Block.BlocksLoaded || GetBlock(localChunkPos.x, localChunkPos.y, localChunkPos.z).Block.AllowBlockTriggers)
				{
					triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
					HandleAddingTriggerLayers(blockTrigger);
				}
			}
			catch (Exception e)
			{
				Log.Error("Skipping loading of active block data for " + PrefabName + " because of the following exception:");
				Log.Exception(e);
			}
		}
	}

	public void RotateY(bool _bLeft, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return;
		}
		if (Block.BlocksLoaded && isCellsDataOwner)
		{
			int num = blockCells.a.Length;
			for (int i = 0; i < num; i++)
			{
				Cells<uint>.CellsAtZ cellsAtZ = blockCells.a[i];
				if (cellsAtZ == null)
				{
					continue;
				}
				int num2 = cellsAtZ.a.Length;
				for (int j = 0; j < num2; j++)
				{
					Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX == null)
					{
						continue;
					}
					int num3 = cellsAtX.a.Length;
					for (int k = 0; k < num3; k++)
					{
						Cells<uint>.Cell cell = cellsAtX.a[k];
						if (cell.a == null)
						{
							continue;
						}
						for (int l = 0; l < cell.a.Length; l++)
						{
							uint num4 = cell.a[l];
							if ((num4 & 0xFFFF) == 0)
							{
								continue;
							}
							BlockValue blockValue = new BlockValue(num4);
							if (!blockValue.ischild)
							{
								Block block = blockValue.Block;
								if (block == null || ((blockValue.meta & 1) != 0 && block is BlockModelTree))
								{
									cell.a[l] = 0u;
									continue;
								}
								blockValue = block.shape.RotateY(_bLeft, blockValue, _rotCount);
								cell.a[l] = blockValue.rawData;
							}
						}
					}
				}
			}
		}
		for (int m = 0; m < _rotCount; m++)
		{
			localRotation += (_bLeft ? 1 : (-1));
			localRotation &= 3;
			for (int n = 0; n < entities.Count; n++)
			{
				EntityCreationData entityCreationData = entities[n];
				if (_bLeft)
				{
					entityCreationData.pos = new Vector3((float)size.z - entityCreationData.pos.z, entityCreationData.pos.y, entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y - 90f, entityCreationData.rot.z);
				}
				else
				{
					entityCreationData.pos = new Vector3(entityCreationData.pos.z, entityCreationData.pos.y, (float)size.x - entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y + 90f, entityCreationData.rot.z);
				}
			}
			MathUtils.Swap(ref TraderAreaProtect.x, ref TraderAreaProtect.z);
			foreach (KeyValuePair<string, List<Vector3i>> indexedBlockOffset in indexedBlockOffsets)
			{
				for (int num5 = 0; num5 < indexedBlockOffset.Value.Count; num5++)
				{
					Vector3i _center = indexedBlockOffset.Value[num5];
					RotatePointOnY(_bLeft, ref _center);
					indexedBlockOffset.Value[num5] = _center;
				}
			}
			foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
			{
				allVolumeList.RotateY(_bLeft, size);
			}
			MathUtils.Swap(ref size.x, ref size.z);
		}
		if (Block.BlocksLoaded)
		{
			AddAllChildBlocks();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RotatePointOnY(bool _bLeft, ref Vector3i _center)
	{
		Vector3 vector = ((!_bLeft) ? (Quaternion.AngleAxis(90f, Vector3.up) * _center.ToVector3()) : (Quaternion.AngleAxis(-90f, Vector3.up) * _center.ToVector3()));
		_center = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public void Replace(BlockValue _src, BlockValue _dst, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.y; k++)
				{
					BlockValue block = GetBlock(i, k, j);
					if (block.ischild || block.type != _src.type || (_bConsiderRotation && block.rotation != _src.rotation) || (_considerPaintId1 != -1 && !hasTexture(GetTexture(i, k, j), _considerPaintId1)) || (_considerPaintId2 != -1 && !hasTexture(GetTexture(i, k, j), _considerPaintId2)))
					{
						continue;
					}
					BlockValue bv = _dst;
					if (!_bConsiderRotation)
					{
						bv.rotation = block.rotation;
					}
					bv.meta = ((_dst.meta != 0) ? _dst.meta : block.meta);
					SetBlock(i, k, j, bv);
					bool flag = _src.Block.shape.IsTerrain();
					bool flag2 = _dst.Block?.shape.IsTerrain() ?? flag;
					if (flag != flag2)
					{
						sbyte b = GetDensity(i, k, j);
						if (flag2)
						{
							b = MarchingCubes.DensityTerrain;
						}
						else if (b != 0)
						{
							b = MarchingCubes.DensityAir;
						}
						SetDensity(i, k, j, b);
					}
				}
			}
		}
	}

	public void Replace(int _searchPaintId, int _replacePaintId, int _blockId = -1)
	{
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.y; k++)
				{
					BlockValue block = GetBlock(i, k, j);
					if (block.ischild || !hasTexture(GetTexture(i, k, j), _searchPaintId) || (_blockId != -1 && _blockId != block.type))
					{
						continue;
					}
					TextureFullArray texture = GetTexture(i, k, j);
					for (int l = 0; l < 1; l++)
					{
						for (int m = 0; m < 6; m++)
						{
							if (((texture[l] >> m * 8) & 0xFF) == _searchPaintId)
							{
								texture[l] &= ~(255L << m * 8);
								texture[l] |= (long)_replacePaintId << m * 8;
							}
						}
					}
					SetTexture(i, k, j, texture);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasTexture(TextureFullArray _fullTexture, int _textureIdx)
	{
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				if (((_fullTexture[i] >> j * 8) & 0xFF) == _textureIdx)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int Search(BlockValue _src, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		int num = 0;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.y; k++)
				{
					BlockValue block = GetBlock(i, k, j);
					if (!block.ischild && block.type == _src.type && (!_bConsiderRotation || block.rotation == _src.rotation) && (_considerPaintId1 == -1 || hasTexture(GetTexture(i, k, j), _considerPaintId1)) && (_considerPaintId2 == -1 || hasTexture(GetTexture(i, k, j), _considerPaintId2)))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public int Search(int _paintId, int _blockId = -1)
	{
		int num = 0;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.y; k++)
				{
					BlockValue block = GetBlock(i, k, j);
					if (!block.ischild && hasTexture(GetTexture(i, k, j), _paintId) && (_blockId == -1 || _blockId == block.type))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public void CopyIntoRPC(GameManager _gm, Vector3i _destinationPos, bool _pasteAirBlocks = false)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
		if (_pasteAirBlocks)
		{
			AddAllChildBlocks();
		}
		if (bCopyAirBlocks)
		{
			NetPackageWaterSet package2 = NetPackageManager.GetPackage<NetPackageWaterSet>();
			for (int i = 0; i < size.y; i++)
			{
				for (int j = 0; j < size.x; j++)
				{
					for (int k = 0; k < size.z; k++)
					{
						int num = j + _destinationPos.x;
						int num2 = i + _destinationPos.y;
						int num3 = k + _destinationPos.z;
						if (!_gm.World.GetBlock(num, num2, num3).isair)
						{
							list.Add(new BlockChangeInfo(new Vector3i(num, num2, num3), BlockValue.Air));
						}
						if (_gm.World.GetWater(num, num2, num3).HasMass())
						{
							package2.AddChange(num, num2, num3, WaterValue.Empty);
						}
					}
				}
			}
			_gm.SetWaterRPC(package2);
		}
		Dictionary<Vector3i, TileEntity> dictionary = new Dictionary<Vector3i, TileEntity>();
		Dictionary<Vector3i, BlockTrigger> dictionary2 = new Dictionary<Vector3i, BlockTrigger>();
		for (int l = 0; l < size.y; l++)
		{
			for (int m = 0; m < size.x; m++)
			{
				for (int n = 0; n < size.z; n++)
				{
					WaterValue water = GetWater(m, l, n);
					if (water.HasMass())
					{
						package.AddChange(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z, water);
					}
					BlockValue block = GetBlock(m, l, n);
					Block block2 = block.Block;
					if (block2 == null || !(!block.isair || _pasteAirBlocks) || (_pasteAirBlocks && block.ischild))
					{
						continue;
					}
					TextureFullArray texture = GetTexture(m, l, n);
					sbyte density = GetDensity(m, l, n);
					list.Add(new BlockChangeInfo(new Vector3i(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z), block, density, texture));
					Vector3i vector3i;
					if (block2.IsTileEntitySavedInPrefab())
					{
						vector3i = new Vector3i(m, l, n);
						TileEntity tileEntity;
						if ((tileEntity = GetTileEntity(vector3i)) != null)
						{
							dictionary.Add(vector3i, tileEntity);
						}
					}
					vector3i = new Vector3i(m, l, n);
					BlockTrigger blockTrigger;
					if ((blockTrigger = GetBlockTrigger(vector3i)) != null)
					{
						dictionary2.Add(vector3i, blockTrigger);
					}
				}
			}
		}
		_gm.SetBlocksRPC(list);
		_gm.SetWaterRPC(package);
		if (_pasteAirBlocks)
		{
			AddAllChildBlocks();
		}
		bool flag = PrefabName.StartsWith("part_");
		foreach (KeyValuePair<Vector3i, TileEntity> item in dictionary)
		{
			Vector3i vector3i2 = item.Key + _destinationPos;
			TileEntity tileEntity2 = _gm.World.GetTileEntity(vector3i2);
			if (tileEntity2 == null || flag)
			{
				Chunk chunk = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i2);
				Vector3i vector3i3 = World.toBlock(vector3i2);
				if (flag)
				{
					chunk.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i3);
				}
				tileEntity2 = item.Value.Clone();
				tileEntity2.SetChunk(chunk);
				tileEntity2.localChunkPos = vector3i3;
				chunk.AddTileEntity(tileEntity2);
			}
			Vector3i localChunkPos = tileEntity2.localChunkPos;
			tileEntity2.CopyFrom(item.Value);
			tileEntity2.localChunkPos = localChunkPos;
			tileEntity2.SetModified();
		}
		foreach (KeyValuePair<Vector3i, BlockTrigger> item2 in dictionary2)
		{
			Vector3i vector3i4 = item2.Key + _destinationPos;
			BlockTrigger blockTrigger2 = _gm.World.GetBlockTrigger(vector3i4);
			if (blockTrigger2 == null || flag)
			{
				Chunk chunk2 = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i4);
				Vector3i vector3i5 = World.toBlock(vector3i4);
				if (flag)
				{
					chunk2.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i5);
				}
				blockTrigger2 = item2.Value.Clone();
				blockTrigger2.Chunk = chunk2;
				blockTrigger2.LocalChunkPos = vector3i5;
				chunk2.AddBlockTrigger(blockTrigger2);
			}
			Vector3i localChunkPos2 = blockTrigger2.LocalChunkPos;
			blockTrigger2.CopyFrom(item2.Value);
			blockTrigger2.LocalChunkPos = localChunkPos2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
		foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
		{
			allVolumeList.CopyVolumesIntoWorld(_world, _chunk, _offset);
		}
	}

	public void MoveVolumes(Vector3i moveDistance)
	{
		foreach (PrefabVolumeListAbs allVolumeList in AllVolumeLists)
		{
			allVolumeList.Move(moveDistance);
		}
	}

	public short FindSleeperVolumeFreeGroupId()
	{
		int num = 0;
		for (int i = 0; i < SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeList[i];
			if (prefabSleeperVolume.groupId > num)
			{
				num = prefabSleeperVolume.groupId;
			}
		}
		return (short)(num + 1);
	}

	public void CopySleeperBlocksContainedInVolume(int _volumeIndex, Vector3i _offset, SleeperVolume _volume, Vector3i _volumeMins, Vector3i _volumeMaxs)
	{
		int num = Mathf.Max(_volumeMins.x, 0);
		int num2 = Mathf.Max(_volumeMins.y, 0);
		int num3 = Mathf.Max(_volumeMins.z, 0);
		int num4 = Mathf.Min(size.x, _volumeMaxs.x);
		int num5 = Mathf.Min(size.y, _volumeMaxs.y);
		int num6 = Mathf.Min(size.z, _volumeMaxs.z);
		for (int i = num; i < num4; i++)
		{
			int x = i + _offset.x;
			for (int j = num3; j < num6; j++)
			{
				int z = j + _offset.z;
				for (int k = num2; k < num5; k++)
				{
					if (k > 0 && GetBlockNoDamage(localRotation, i, k - 1, j).Block.IsSleeperBlock)
					{
						continue;
					}
					BlockValue block = GetBlock(i, k, j);
					Block block2 = block.Block;
					if (block2.IsSleeperBlock)
					{
						int y = k + _offset.y;
						Vector3i pos = new Vector3i(i, k, j);
						if (!IsPosInSleeperPriorityVolume(pos, _volumeIndex))
						{
							_volume.AddSpawnPoint(x, y, z, (BlockSleeper)block2, block);
						}
					}
				}
			}
		}
	}

	public PrefabSleeperVolume FindSleeperVolume(Vector3i _pos)
	{
		PrefabSleeperVolume result = null;
		for (int i = 0; i < SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeList[i];
			if (prefabSleeperVolume.Used && IsPosInSleeperVolume(prefabSleeperVolume, _pos))
			{
				result = prefabSleeperVolume;
				if (prefabSleeperVolume.isPriority)
				{
					break;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperPriorityVolume(Vector3i _pos, int _skipIndex)
	{
		for (int i = 0; i < SleeperVolumeList.Count; i++)
		{
			if (i != _skipIndex)
			{
				PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeList[i];
				if (prefabSleeperVolume.Used && prefabSleeperVolume.isPriority && IsPosInSleeperVolume(prefabSleeperVolume, _pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperVolume(PrefabSleeperVolume _volume, Vector3i _pos)
	{
		if (!_volume.Used)
		{
			return false;
		}
		Vector3i startPos = _volume.startPos;
		Vector3i vector3i = startPos + _volume.size;
		if (_pos.x >= startPos.x && _pos.x < vector3i.x && _pos.y >= startPos.y && _pos.y < vector3i.y && _pos.z >= startPos.z && _pos.z < vector3i.z)
		{
			return true;
		}
		return false;
	}

	public void CountSleeperSpawnsInVolume(World _world, Vector3i _offset, int _index)
	{
		Transient_NumSleeperSpawns = 0;
		PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeList[_index];
		Vector3i startPos = prefabSleeperVolume.startPos;
		Vector3i vector3i = prefabSleeperVolume.size;
		Vector3i vector3i2 = startPos + vector3i;
		Vector3i vector3i3 = startPos + _offset;
		Vector3i vector3i4 = vector3i2 + _offset;
		int x = vector3i3.x;
		int y = vector3i3.y;
		int z = vector3i3.z;
		int x2 = vector3i4.x;
		int y2 = vector3i4.y;
		int z2 = vector3i4.z;
		for (int i = z; i < z2; i++)
		{
			for (int j = y; j < y2; j++)
			{
				for (int k = x; k < x2; k++)
				{
					if (_world.GetBlock(k, j, i).Block.IsSleeperBlock && !_world.GetBlock(k, j - 1, i).Block.IsSleeperBlock)
					{
						Vector3i pos = new Vector3i(k - _offset.x, j - _offset.y, i - _offset.z);
						if (!IsPosInSleeperPriorityVolume(pos, _index))
						{
							Transient_NumSleeperSpawns++;
						}
					}
				}
			}
		}
	}

	public static void TransientSleeperBlockIncrement(Vector3i _point, int _c)
	{
		SelectionBox selection = SelectionBoxManager.Instance.Selection;
		if (!(selection == null) && selection.Category == SelectionBoxManager.Instance.CategorySleeperVolume && selection.WorldPosWithinBox(_point) && PrefabVolumeManager.GetPrefabIdAndVolumeId(selection.name, out var _volumeId, out var _prefabInstance))
		{
			Prefab prefab = _prefabInstance.prefab;
			if (_volumeId < prefab.SleeperVolumeList.Count && prefab.SleeperVolumeList[_volumeId].Used)
			{
				prefab.Transient_NumSleeperSpawns += _c;
			}
		}
	}

	public string CalcSleeperInfo()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumeList[i];
			int spawnCountMin = prefabSleeperVolume.spawnCountMin;
			if (spawnCountMin < 0)
			{
				flag = true;
			}
			else
			{
				num += spawnCountMin;
			}
			int spawnCountMax = prefabSleeperVolume.spawnCountMax;
			if (spawnCountMax < 0)
			{
				flag = true;
			}
			else
			{
				num2 += spawnCountMax;
			}
		}
		string text = $"{SleeperVolumeList.Count}, {num}-{num2}";
		if (flag)
		{
			text += "*";
		}
		return text;
	}

	public void CopyIntoLocal(ChunkCluster _cluster, Vector3i _destinationPos, bool _bOverwriteExistingBlocks, bool _bSetChunkToRegenerate, FastTags<TagGroup.Global> _questTags)
	{
		World world = _cluster.GetWorld();
		bool flag = world.IsEditor();
		if (!flag)
		{
			CopyVolumesIntoWorld(world, null, _destinationPos);
		}
		Chunk chunkSync = _cluster.GetChunkSync(World.toChunkXZ(_destinationPos.x), World.toChunkXZ(_destinationPos.z));
		int seed = world.Seed;
		GameRandom gameRandom = ((chunkSync != null) ? Utils.RandomFromSeedOnPos(chunkSync.X, chunkSync.Z, seed) : null);
		GameRandom gameRandom2 = GameRandomManager.Instance.CreateGameRandom((int)world.GetWorldTime());
		if (terrainFillerType == 0)
		{
			InitTerrainFillers();
		}
		for (int i = size.y + _destinationPos.y; i < 255; i++)
		{
			int y = World.toBlockY(i);
			bool flag2 = false;
			for (int j = 0; j < size.z; j++)
			{
				int v = j + _destinationPos.z;
				int num = World.toChunkXZ(v);
				int z = World.toBlockXZ(v);
				for (int k = 0; k < size.x; k++)
				{
					int v2 = k + _destinationPos.x;
					int num2 = World.toChunkXZ(v2);
					int x = World.toBlockXZ(v2);
					if (chunkSync == null || chunkSync.X != num2 || chunkSync.Z != num)
					{
						chunkSync = _cluster.GetChunkSync(num2, num);
						if (chunkSync == null)
						{
							continue;
						}
					}
					BlockValue block = chunkSync.GetBlock(x, y, z);
					if (!block.isair && !block.Block.shape.IsTerrain())
					{
						flag2 = true;
						if (!block.ischild)
						{
							chunkSync.SetBlock(world, x, y, z, BlockValue.Air, _notifyAddChange: true, _notifyRemove: true, _fromReset: false, _poiOwned: true);
						}
					}
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		if (_bOverwriteExistingBlocks)
		{
			DestroyAllMultiblocks(_cluster, _destinationPos);
		}
		for (int l = 0; l < size.z; l++)
		{
			int num3 = l + _destinationPos.z;
			int num4 = World.toChunkXZ(num3);
			int z2 = World.toBlockXZ(num3);
			for (int m = 0; m < size.x; m++)
			{
				int num5 = m + _destinationPos.x;
				int num6 = World.toChunkXZ(num5);
				int x2 = World.toBlockXZ(num5);
				if (chunkSync == null || chunkSync.X != num6 || chunkSync.Z != num4)
				{
					chunkSync = _cluster.GetChunkSync(num6, num4);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
					gameRandom = null;
					if (chunkSync == null)
					{
						UnityEngine.Debug.LogError($"Chunk ({num6}, {num4}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({num5},{num3}).");
						continue;
					}
				}
				if (gameRandom == null)
				{
					gameRandom = Utils.RandomFromSeedOnPos(num6, num4, seed);
				}
				int num7 = _destinationPos.y - 1;
				bool flag3 = false;
				for (int n = 0; n < size.y; n++)
				{
					WaterValue water = GetWater(m, n, l);
					BlockValue targetBV = GetBlock(m, n, l);
					if (!bCopyAirBlocks && targetBV.isair && !water.HasMass())
					{
						continue;
					}
					int num8 = World.toBlockY(n + _destinationPos.y);
					BlockValueRef bvRef = new BlockValueRef(x2, num8, z2);
					BlockValue block2 = chunkSync.GetBlock(x2, num8, z2);
					BlockValue blockValue = targetBV;
					sbyte b = GetDensity(m, n, l);
					bool flag4 = false;
					if (!targetBV.isair && !flag)
					{
						if (targetBV.Block.IsSleeperBlock)
						{
							flag4 = true;
							targetBV = BlockValue.Air;
						}
						if (targetBV.type == terrainFillerType)
						{
							BlockValue blockValue2 = block2;
							Block block3 = blockValue2.Block;
							if (blockValue2.isair || block3 == null || !block3.shape.IsTerrain())
							{
								int terrainHeight = chunkSync.GetTerrainHeight(x2, z2);
								blockValue2 = chunkSync.GetBlock(x2, terrainHeight, z2);
								block3 = blockValue2.Block;
								if (blockValue2.isair || block3 == null || !block3.shape.IsTerrain())
								{
									continue;
								}
							}
							targetBV = blockValue2;
							flag3 = true;
						}
						if (targetBV.type == terrainFiller2Type)
						{
							Block block4 = block2.Block;
							if (!block2.isair && block4 != null && block4.shape.IsTerrain())
							{
								targetBV = block2;
								b = 0;
							}
							else
							{
								targetBV = BlockValue.Air;
								b = MarchingCubes.DensityAir;
							}
						}
						if (targetBV.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
						{
							ProcessMultiBlock(ref targetBV, chunkSync, new Vector3i(m, n, l), new Vector3i(x2, num8, z2), _questTags, _bOverwriteExistingBlocks);
						}
						else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(targetBV))
						{
							byte meta = targetBV.meta;
							targetBV = BlockPlaceholderMap.Instance.Replace(bvRef, targetBV, GameManager.Instance.World.GetGameRandom(), chunkSync, _questTags, _bOverwriteExistingBlocks);
							targetBV.meta = meta;
						}
					}
					if (b == 0)
					{
						b = MarchingCubes.DensityAir;
						if (targetBV.Block.shape.IsTerrain())
						{
							b = MarchingCubes.DensityTerrain;
						}
					}
					if (block2.ischild || (!_bOverwriteExistingBlocks && !block2.isair && !block2.Block.shape.IsTerrain()))
					{
						chunkSync.SetDensity(x2, num8, z2, b);
						continue;
					}
					chunkSync.SetDecoAllowedSizeAt(x2, z2, EnumDecoAllowedSize.OnlySmall);
					if (!flag4)
					{
						TextureFullArray texture = GetTexture(m, n, l);
						chunkSync.GetSetTextureFullArray(x2, num8, z2, texture);
					}
					chunkSync.SetBlock(world, x2, num8, z2, targetBV, _notifyAddChange: true, _notifyRemove: true, !_questTags.IsEmpty, _poiOwned: true);
					chunkSync.SetWater(x2, num8, z2, water);
					Vector3i blockPos = new Vector3i(m, n, l);
					TileEntity tileEntity;
					if (blockValue.Block.IsTileEntitySavedInPrefab() && (tileEntity = GetTileEntity(blockPos)) != null)
					{
						Vector3i vector3i = new Vector3i(x2, num8, z2);
						TileEntity tileEntity2 = chunkSync.GetTileEntity(vector3i);
						if (tileEntity2 == null)
						{
							tileEntity2 = tileEntity.Clone();
							tileEntity2.localChunkPos = vector3i;
							tileEntity2.SetChunk(chunkSync);
							chunkSync.AddTileEntity(tileEntity2);
						}
						tileEntity2.CopyFrom(tileEntity);
						tileEntity2.localChunkPos = vector3i;
					}
					BlockTrigger blockTrigger = GetBlockTrigger(blockPos);
					if (blockTrigger != null)
					{
						BlockTrigger blockTrigger2 = chunkSync.GetBlockTrigger(new Vector3i(x2, num8, z2));
						if (blockTrigger2 == null)
						{
							blockTrigger2 = blockTrigger.Clone();
							blockTrigger2.LocalChunkPos = new Vector3i(x2, num8, z2);
							blockTrigger2.Chunk = chunkSync;
							chunkSync.AddBlockTrigger(blockTrigger2);
						}
						blockTrigger2.CopyFrom(blockTrigger);
						blockTrigger2.LocalChunkPos = new Vector3i(x2, num8, z2);
						targetBV.Block.OnTriggerAddedFromPrefab(blockTrigger2, blockTrigger2.LocalChunkPos, targetBV, FastTags<TagGroup.Global>.Parse(questTags.ToString()));
					}
					if (targetBV.Block.shape.IsTerrain())
					{
						num7 = num8;
					}
					chunkSync.SetDensity(x2, num8, z2, b);
				}
				if (num7 >= 0)
				{
					chunkSync.SetTerrainHeight(x2, z2, (byte)num7);
				}
				if (!flag3)
				{
					chunkSync.SetTopSoilBroken(x2, z2);
				}
				chunkSync.SetDecoAllowedSizeAt(x2, z2, EnumDecoAllowedSize.OnlySmall);
				if (_bSetChunkToRegenerate)
				{
					chunkSync.NeedsRegeneration = true;
				}
			}
		}
		ApplyDecoAllowed(_cluster, _destinationPos);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		GameRandomManager.Instance.FreeGameRandom(gameRandom2);
	}

	public void DestroyAllMultiblocks(ChunkCluster _cluster, Vector3i _destinationPos)
	{
		Chunk chunkSync = _cluster.GetChunkSync(World.toChunkXZ(_destinationPos.x), World.toChunkXZ(_destinationPos.z));
		for (int i = 0; i < size.z; i++)
		{
			int num = i + _destinationPos.z;
			int num2 = World.toChunkXZ(num);
			int z = World.toBlockXZ(num);
			for (int j = 0; j < size.x; j++)
			{
				int num3 = j + _destinationPos.x;
				int num4 = World.toChunkXZ(num3);
				int x = World.toBlockXZ(num3);
				if (chunkSync == null || chunkSync.X != num4 || chunkSync.Z != num2)
				{
					chunkSync = _cluster.GetChunkSync(num4, num2);
					if (chunkSync == null)
					{
						UnityEngine.Debug.LogError($"Chunk ({num4}, {num2}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({num3},{num}).");
						continue;
					}
				}
				for (int k = 0; k < size.y; k++)
				{
					int y = World.toBlockY(k + _destinationPos.y);
					BlockValue block = chunkSync.GetBlock(x, y, z);
					if (block.Block.isMultiBlock && !block.ischild)
					{
						chunkSync.SetBlock(_cluster.GetWorld(), x, y, z, BlockValue.Air);
					}
				}
			}
		}
		foreach (int multiBlockParentIndex in multiBlockParentIndices)
		{
			offsetToCoordRotated(multiBlockParentIndex, out var _x, out var _y, out var _z);
			Vector3i worldPos = new Vector3i(_x, _y, _z) + _destinationPos;
			MultiBlockManager.Instance.DeregisterTrackedBlockData(worldPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(ChunkCluster _cluster, Vector3i _prefabTargetPos)
	{
		int num = World.toChunkXZ(_prefabTargetPos.x);
		int num2 = World.toChunkXZ(_prefabTargetPos.z);
		int num3 = World.toChunkXZ(_prefabTargetPos.x + size.x - 1);
		int num4 = World.toChunkXZ(_prefabTargetPos.z + size.z - 1);
		for (int i = num2; i <= num4; i++)
		{
			for (int j = num; j <= num3; j++)
			{
				Chunk chunkSync = _cluster.GetChunkSync(j, i);
				if (chunkSync != null)
				{
					ApplyDecoAllowed(chunkSync, _prefabTargetPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessMultiBlock(ref BlockValue targetBV, Chunk chunk, Vector3i prefabRelPos, Vector3i chunkRelPos, FastTags<TagGroup.Global> questTags, bool overwriteExistingBlocks)
	{
		if (!targetBV.Block.isMultiBlock)
		{
			UnityEngine.Debug.LogError("[MultiBlockManager] BlockValue passed into ProcessMultiBlock is not a MultiBlock.");
			return;
		}
		Vector3i vector3i = chunk.GetWorldPos() + (chunkRelPos - prefabRelPos);
		Vector3i vector3i2 = prefabRelPos;
		if (targetBV.ischild)
		{
			vector3i2 += new Vector3i(targetBV.parentx, targetBV.parenty, targetBV.parentz);
		}
		Vector3i vector3i3 = vector3i + vector3i2;
		BlockValue blockValue;
		if (MultiBlockManager.Instance.TryGetPOIMultiBlock(vector3i3, out var poiMultiBlock))
		{
			blockValue = new BlockValue(poiMultiBlock.rawData);
		}
		else
		{
			BlockValue blockValue2 = ((!targetBV.ischild) ? targetBV : GetBlock(vector3i2.x, vector3i2.y, vector3i2.z));
			if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue2))
			{
				byte meta = blockValue2.meta;
				blockValue = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), chunk, chunkRelPos.x, chunkRelPos.y, chunkRelPos.z, questTags, overwriteExistingBlocks, allowRandomRotation: false);
				if (blockValue.isair)
				{
					blockValue = BlockValue.Air;
				}
				else
				{
					blockValue.meta = meta;
				}
			}
			else
			{
				blockValue = blockValue2;
			}
			MultiBlockManager.Instance.DeregisterTrackedBlockData(vector3i3);
			if (!MultiBlockManager.Instance.TryRegisterPOIMultiBlock(vector3i3, blockValue))
			{
				UnityEngine.Debug.LogError("[MultiBlockManager] Failed to register POI MultiBlock.");
			}
		}
		if (blockValue.type == targetBV.type)
		{
			return;
		}
		if (blockValue.isair)
		{
			targetBV = BlockValue.Air;
			return;
		}
		if (!blockValue.Block.isMultiBlock)
		{
			if (targetBV.ischild)
			{
				targetBV = BlockValue.Air;
				return;
			}
			targetBV.type = blockValue.type;
			targetBV.rotation = blockValue.rotation;
			return;
		}
		Vector3i dim = targetBV.Block.multiBlockPos.dim;
		Vector3i dim2 = blockValue.Block.multiBlockPos.dim;
		if (dim2 != dim)
		{
			if (dim2.x > dim.x || dim2.y > dim.y || dim2.z > dim.z)
			{
				UnityEngine.Debug.LogWarning("[MultiBlockManager] The replacement block \"" + blockValue.Block.GetBlockName() + "\" is larger than the original block \"" + targetBV.Block.GetBlockName() + "\" in dimensions. \n" + $"Replacement size: \"{dim2}\", Original size: \"{dim}\". " + $"Parent world position: {vector3i3}.\n" + "Child blocks of the replacement will not be placed outside the original block's dimensions. \nNote: We expect to see this warning when single-block helpers are used to place MultiBlocks at 45-degree rotations. Many of these instances will be resolved by converting to the new oversized block format in the near future. \nIn situations where 45-degree rotations aren't needed, helper blocks should be set to the maximum dimensions of any possible replacements. Affected prefabs may need to be re-saved to implement these changes.");
			}
			if (dim2.x < dim.x || dim2.y < dim.y || dim2.z < dim.z)
			{
				MultiBlockManager.GetMinMaxWorldPositions(vector3i3, blockValue, out var minPos, out var maxPos);
				Vector3i vector3i4 = vector3i + prefabRelPos;
				if (vector3i4.x < minPos.x || vector3i4.x > maxPos.x || vector3i4.y < minPos.y || vector3i4.y > maxPos.y || vector3i4.z < minPos.z || vector3i4.z > maxPos.z)
				{
					targetBV = BlockValue.Air;
					return;
				}
			}
		}
		targetBV.type = blockValue.type;
		if (!targetBV.ischild)
		{
			targetBV.rotation = blockValue.rotation;
		}
	}

	public void SnapTerrainToArea(ChunkCluster _cluster, Vector3i _destinationPos)
	{
		for (int i = -1; i < size.x + 1; i++)
		{
			for (int j = -1; j < size.z + 1; j++)
			{
				bool bUseHalfTerrainDensity = i == -1 || j == -1 || i == size.x || j == size.z;
				_cluster.SnapTerrainToPositionAtLocal(new Vector3i(_destinationPos.x + i, _destinationPos.y - 1, _destinationPos.z + j), _bLiftUpTerrainByOneIfNeeded: true, bUseHalfTerrainDensity);
			}
		}
	}

	public void CopyEntitiesIntoWorld(World _world, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		_entityIds?.Clear();
		for (int i = 0; i < entities.Count; i++)
		{
			EntityCreationData entityCreationData = entities[i];
			entityCreationData.id = -1;
			if (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity)
			{
				Entity entity = EntityFactory.CreateEntity(entityCreationData);
				entity.SetPosition(entity.position + _destinationPos.ToVector3());
				_world.SpawnEntityInWorld(entity);
				_entityIds?.Add(entity.entityId);
			}
		}
	}

	public void CopyEntitiesIntoChunkStub(Chunk _chunk, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			EntityCreationData entityCreationData = entities[i];
			if (EntityClass.list.ContainsKey(entityCreationData.entityClass) && (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity))
			{
				int v = Utils.Fastfloor(entityCreationData.pos.x) + _destinationPos.x;
				int v2 = Utils.Fastfloor(entityCreationData.pos.z) + _destinationPos.z;
				if (_chunk.X == World.toChunkXZ(v) && _chunk.Z == World.toChunkXZ(v2))
				{
					EntityCreationData entityCreationData2 = entityCreationData.Clone();
					entityCreationData2.pos += _destinationPos.ToVector3() + new Vector3(0f, 0.25f, 0f);
					entityCreationData2.id = EntityFactory.nextEntityID++;
					_chunk.AddEntityStub(entityCreationData2);
					_entityIds?.Add(entityCreationData2.id);
				}
			}
		}
	}

	public static Vector3i SizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	public Vector3i copyFromWorld(World _world, Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		size.x = Math.Abs(vector3i2.x - vector3i.x) + 1;
		size.y = Math.Abs(vector3i2.y - vector3i.y) + 1;
		size.z = Math.Abs(vector3i2.z - vector3i.z) + 1;
		localRotation = 0;
		InitData();
		tileEntities.Clear();
		int num = 0;
		int num2 = vector3i.y;
		while (num2 <= vector3i2.y)
		{
			int num3 = 0;
			int num4 = vector3i.x;
			while (num4 <= vector3i2.x)
			{
				int num5 = 0;
				int num6 = vector3i.z;
				while (num6 <= vector3i2.z)
				{
					BlockValue bv = _world.GetBlock(num4, num2, num6);
					if (bv.isWater)
					{
						SetWater(num4, num2, num6, WaterValue.Full);
						bv = BlockValue.Air;
					}
					SetDensity(num3, num, num5, _world.GetDensity(num4, num2, num6));
					if (!bv.ischild)
					{
						SetBlock(num3, num, num5, bv);
						SetWater(num3, num, num5, _world.GetWater(num4, num2, num6));
						SetTexture(num3, num, num5, _world.GetTextureFullArray(num4, num2, num6));
						if (bv.Block.IsTileEntitySavedInPrefab())
						{
							Vector3i vector3i3 = new Vector3i(num4, num2, num6);
							TileEntity tileEntity = _world.GetTileEntity(vector3i3);
							if (tileEntity != null)
							{
								TileEntity tileEntity2 = tileEntity.Clone();
								tileEntity2.localChunkPos = vector3i3 - vector3i;
								tileEntities.Add(tileEntity2.localChunkPos, tileEntity2);
							}
						}
					}
					num6++;
					num5++;
				}
				num4++;
				num3++;
			}
			num2++;
			num++;
		}
		return vector3i;
	}

	public Vector3i CopyFromWorldWithEntities(World _world, Vector3i _posStart, Vector3i _posEnd, ICollection<int> _entityIds)
	{
		copyFromWorld(_world, _posStart, _posEnd);
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		entities.Clear();
		int num = World.toChunkXZ(vector3i.x);
		int num2 = World.toChunkXZ(vector3i.z);
		int num3 = World.toChunkXZ(vector3i2.x);
		int num4 = World.toChunkXZ(vector3i2.z);
		Bounds bb = BoundsUtils.BoundsForMinMax(vector3i.x, vector3i.y, vector3i.z, vector3i2.x + 1, vector3i2.y + 1, vector3i2.z + 1);
		List<Entity> list = new List<Entity>();
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				((Chunk)_world.GetChunkSync(i, j))?.GetEntitiesInBounds(typeof(Entity), bb, list);
			}
		}
		indexedBlockOffsets.Clear();
		triggerData.Clear();
		for (int k = num; k <= num3; k++)
		{
			for (int l = num2; l <= num4; l++)
			{
				Chunk chunk = (Chunk)_world.GetChunkSync(k, l);
				if (chunk == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, List<Vector3i>> item2 in chunk.IndexedBlocks.Dict)
				{
					if (item2.Value == null || item2.Value.Count <= 0)
					{
						continue;
					}
					List<Vector3i> list2 = new List<Vector3i>();
					indexedBlockOffsets[item2.Key] = list2;
					foreach (Vector3i item3 in item2.Value)
					{
						Vector3i vector3i3 = chunk.ToWorldPos(item3);
						Vector3 point = vector3i3.ToVector3();
						Vector3i item = vector3i3 - vector3i;
						if (bb.Contains(point))
						{
							list2.Add(item);
						}
					}
				}
				List<BlockTrigger> list3 = chunk.GetBlockTriggers().list;
				for (int m = 0; m < list3.Count; m++)
				{
					BlockTrigger blockTrigger = list3[m].Clone();
					blockTrigger.LocalChunkPos = chunk.ToWorldPos(list3[m].LocalChunkPos) - _posStart;
					triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
				}
			}
		}
		_entityIds?.Clear();
		for (int n = 0; n < list.Count; n++)
		{
			Entity entity = list[n];
			if (!(entity is EntityPlayer))
			{
				EntityCreationData entityCreationData = new EntityCreationData(entity);
				entityCreationData.pos -= new Vector3(bb.min.x, bb.min.y, bb.min.z);
				entities.Add(entityCreationData);
				_entityIds?.Add(entity.entityId);
			}
		}
		return vector3i;
	}

	public BlockValue Get(int _absy)
	{
		int num = currX;
		int num2 = currZ;
		if (num >= 0 && num < size.x && _absy >= 0 && _absy < size.y && num2 >= 0 && num2 < size.z)
		{
			return GetBlock(num, _absy, num2);
		}
		return BlockValue.Air;
	}

	public BlockValue Get(int _relx, int _absy, int _relz)
	{
		int num = currX + _relx;
		int num2 = currZ + _relz;
		if (num >= 0 && num < size.x && _absy >= 0 && _absy < size.y && num2 >= 0 && num2 < size.z)
		{
			return GetBlock(num, _absy, num2);
		}
		return BlockValue.Air;
	}

	public IChunk GetChunk(int _x, int _z)
	{
		long key = WorldChunkCache.MakeChunkKey(_x, _z);
		if (!dictChunks.TryGetValue(key, out var value))
		{
			value = new PrefabChunk(this, _x, _z);
			dictChunks.Add(key, value);
		}
		return value;
	}

	public List<IChunk> GetChunks()
	{
		if (dictChunks.Count == 0)
		{
			int num = 0;
			int num2 = 0;
			while (num < size.x + 1)
			{
				int num3 = 0;
				int num4 = 0;
				while (num3 < size.z + 1)
				{
					GetChunk(num2, num4);
					num3 += 16;
					num4++;
				}
				num += 16;
				num2++;
			}
		}
		return ((IEnumerable<IChunk>)dictChunks.Values).ToList();
	}

	public IChunk GetNeighborChunk(int _x, int _z)
	{
		return GetChunk(_x, _z);
	}

	public bool IsWater(int _relx, int _absy, int _relz)
	{
		int num = currX + _relx;
		int num2 = currZ + _relz;
		if (num >= 0 && num < size.x && _absy >= 0 && _absy < size.y && num2 >= 0 && num2 < size.z)
		{
			return GetWater(num, _absy, num2).HasMass();
		}
		return false;
	}

	public bool IsAir(int _relx, int _absy, int _relz)
	{
		int num = currX + _relx;
		int num2 = currZ + _relz;
		if (num >= 0 && num < size.x && _absy >= 0 && _absy < size.y && num2 >= 0 && num2 < size.z)
		{
			if (GetBlock(num, _absy, num2).isair)
			{
				return !GetWater(num, _absy, num2).HasMass();
			}
			return false;
		}
		return false;
	}

	public void Init(int _bX, int _bZ)
	{
		currX = _bX;
		currZ = _bZ;
		dictChunks = new Dictionary<long, PrefabChunk>();
	}

	public void Clear()
	{
	}

	public void Cache()
	{
	}

	public void ToMesh(VoxelMesh[] _meshes)
	{
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), size + Vector3i.one, _meshes);
	}

	public Transform ToTransform()
	{
		MeshFilter[] array = new MeshFilter[MeshDescription.meshes.Length];
		MeshRenderer[] array2 = new MeshRenderer[MeshDescription.meshes.Length];
		MeshCollider[] array3 = new MeshCollider[MeshDescription.meshes.Length];
		GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = null;
		gameObject.name = "Prefab_" + PrefabName;
		GameObject gameObject2 = new GameObject("_BlockEntities");
		gameObject2.transform.parent = gameObject.transform;
		GameObject gameObject3 = new GameObject("Meshes");
		gameObject3.transform.parent = gameObject.transform;
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			array4[i] = new GameObject(MeshDescription.meshes[i].Name);
			array4[i].transform.parent = gameObject3.transform;
			VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, _bAllowLOD: false, out array[i], out array2[i], out array3[i]);
		}
		VoxelMesh[] array5 = new VoxelMesh[6];
		for (int j = 0; j < array5.Length; j++)
		{
			array5[j] = new VoxelMesh(j);
		}
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), size + Vector3i.one, array5);
		for (int k = 0; k < array5.Length; k++)
		{
			array5[k].CopyToMesh(array[k], array2[k], 0);
		}
		for (int l = 0; l < size.x; l++)
		{
			for (int m = 0; m < size.z; m++)
			{
				for (int n = 0; n < size.y; n++)
				{
					Vector3i vector3i = new Vector3i(l, n, m);
					BlockValue block = GetBlock(l, n, m);
					Block block2 = block.Block;
					if ((!block2.isMultiBlock || !block.ischild) && block2.shape is BlockShapeModelEntity blockShapeModelEntity)
					{
						Quaternion rotation = blockShapeModelEntity.GetRotation(block);
						Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
						rotatedOffset.x += 0.5f;
						rotatedOffset.z += 0.5f;
						rotatedOffset.y += 0f;
						Vector3 localPosition = vector3i.ToVector3() + rotatedOffset;
						GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
						if (!(objectForType == null))
						{
							objectForType.SetActive(value: true);
							Transform transform = objectForType.transform;
							transform.parent = gameObject2.transform;
							transform.localScale = Vector3.one;
							transform.localPosition = localPosition;
							transform.localRotation = rotation;
						}
					}
				}
			}
		}
		return gameObject.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BiomeDefinition> toBiomeArray(WorldBiomes _biomes, List<string> _biomeStrList)
	{
		List<BiomeDefinition> list = new List<BiomeDefinition>();
		for (int i = 0; i < _biomeStrList.Count; i++)
		{
			string name = _biomeStrList[i];
			BiomeDefinition biome;
			if ((biome = _biomes.GetBiome(name)) != null)
			{
				list.Add(biome);
			}
		}
		return list;
	}

	public string[] GetAllowedBiomes()
	{
		return allowedBiomes.ToArray();
	}

	public string[] GetAllowedZones()
	{
		return allowedZones.ToArray();
	}

	public bool IsAllowedZone(string _zone)
	{
		return allowedZones.ContainsCaseInsensitive(_zone);
	}

	public void AddAllowedZone(string _zone)
	{
		if (!IsAllowedZone(_zone))
		{
			allowedZones.Add(_zone);
		}
	}

	public void RemoveAllowedZone(string _zone)
	{
		int num = allowedZones.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (string _s) => _s.EqualsCaseInsensitive(_zone));
		if (num >= 0)
		{
			allowedZones.RemoveAt(num);
		}
	}

	public string[] GetAllowedTownships()
	{
		return allowedTownships.ToArray();
	}

	public void SetAllowedBiomes(string[] _b)
	{
		allowedBiomes = new List<string>(_b);
	}

	public List<BiomeDefinition> GetAllowedBiomes(WorldBiomes _biomes)
	{
		return toBiomeArray(_biomes, allowedBiomes);
	}

	public void CopyBlocksIntoChunkNoEntities(World _world, Chunk _chunk, Vector3i _prefabTargetPos, bool _bForceOverwriteBlocks, FastTags<TagGroup.Global> _questTags)
	{
		bool flag = IsCullThisPrefab() && GameStats.GetInt(EnumGameStats.OptionsPOICulling) > 1;
		bool flag2 = _world.IsEditor();
		if (terrainFillerType == 0)
		{
			InitTerrainFillers();
		}
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		Bounds aABB = _chunk.GetAABB();
		int num = 0;
		int num2 = 0;
		int num3 = _prefabTargetPos.x - (int)aABB.min.x;
		int num4;
		if (num3 >= 0)
		{
			num = num3;
			num4 = Utils.FastMin(16 - num3, size.x);
		}
		else
		{
			num2 = -num3;
			num4 = Utils.FastMin(size.x + num3, 16);
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = _prefabTargetPos.z - (int)aABB.min.z;
		int num8;
		if (num7 >= 0)
		{
			num5 = num7;
			num8 = Utils.FastMin(16 - num7, size.z);
		}
		else
		{
			num6 = -num7;
			num8 = Utils.FastMin(size.z + num7, 16);
		}
		for (int i = Utils.FastClamp(_prefabTargetPos.y + size.y, 0, 255); i < 256; i++)
		{
			bool flag3 = false;
			for (int j = 0; j < num8; j++)
			{
				int z = j + num5;
				for (int k = 0; k < num4; k++)
				{
					int x = k + num;
					BlockValue blockNoDamage = _chunk.GetBlockNoDamage(x, i, z);
					if (!blockNoDamage.isair && !blockNoDamage.Block.shape.IsTerrain())
					{
						flag3 = true;
						if (!blockNoDamage.ischild)
						{
							_chunk.SetBlock(_world, x, i, z, BlockValue.Air, _notifyAddChange: true, _notifyRemove: true, _fromReset: false, _poiOwned: true);
						}
					}
				}
			}
			if (!flag3)
			{
				break;
			}
		}
		for (int l = 0; l < num8; l++)
		{
			int num9 = l + num5;
			int z2 = l + num6;
			for (int m = 0; m < num4; m++)
			{
				int num10 = m + num;
				int x2 = m + num2;
				int num11 = _chunk.GetTerrainHeight(num10, num9);
				BlockValue block = _chunk.GetBlock(num10, num11, num9);
				Block block2 = block.Block;
				while ((block.isair || block2 == null || !block2.shape.IsTerrain()) && num11 > 0)
				{
					num11--;
					block = _chunk.GetBlock(num10, num11, num9);
					block2 = block.Block;
				}
				int num12 = _prefabTargetPos.y - 1;
				for (int n = 0; n < size.y; n++)
				{
					int num13 = n + _prefabTargetPos.y;
					if ((uint)num13 >= 255u)
					{
						continue;
					}
					BlockValue targetBV = GetBlock(x2, n, z2);
					bool flag4 = false;
					if (targetBV.Block.IsSleeperBlock)
					{
						flag4 = true;
						targetBV = BlockValue.Air;
					}
					if (targetBV.type == terrainFillerType && !flag2)
					{
						targetBV = block;
					}
					sbyte b = GetDensity(x2, n, z2);
					if (targetBV.type == terrainFiller2Type)
					{
						BlockValue block3 = _chunk.GetBlock(num10, num13, num9);
						Block block4 = block3.Block;
						if (!block3.isair && block4 != null && block4.shape.IsTerrain())
						{
							targetBV = block3;
							b = _chunk.GetDensity(num10, num13, num9);
						}
						else
						{
							targetBV = BlockValue.Air;
							b = MarchingCubes.DensityAir;
							if (num13 > 0 && _chunk.GetBlock(num10, num13 - 1, num9).Block.shape.IsTerrain())
							{
								sbyte density = _chunk.GetDensity(num10, num13 - 1, num9);
								b = (sbyte)(MarchingCubes.DensityAir + density);
							}
						}
					}
					if (!flag2)
					{
						if (targetBV.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
						{
							ProcessMultiBlock(ref targetBV, _chunk, new Vector3i(x2, n, z2), new Vector3i(num10, num13, num9), _questTags, _bForceOverwriteBlocks);
						}
						else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(targetBV))
						{
							targetBV = BlockPlaceholderMap.Instance.Replace(targetBV, gameRandom, _chunk, num10, num13, num9, _questTags, _bForceOverwriteBlocks);
						}
					}
					Block block5 = targetBV.Block;
					bool flag5 = block5.shape.IsTerrain();
					if (flag5)
					{
						num12 = num13;
					}
					if (b == 0)
					{
						b = MarchingCubes.DensityAir;
						if (flag5)
						{
							b = MarchingCubes.DensityTerrain;
						}
						else if (block5.shape.IsSolidCube && num13 <= num11)
						{
							b = 1;
						}
					}
					if (yOffset == 0)
					{
						sbyte density2 = _chunk.GetDensity(num10, num13, num9);
						if ((b >= 0 && density2 >= 0 && (density2 != MarchingCubes.DensityAir / 2 || (block5.IsTerrainDecoration && !bCopyAirBlocks))) || (b < 0 && density2 < 0 && density2 != MarchingCubes.DensityTerrain / 2))
						{
							b = density2;
						}
					}
					_chunk.SetDecoAllowedSizeAt(num10, num9, EnumDecoAllowedSize.OnlySmall);
					Vector3i blockPos = new Vector3i(x2, n, z2);
					if (flag && !block5.shape.IsTerrain() && IsInsidePrefab(blockPos.x, blockPos.y, blockPos.z))
					{
						_chunk.AddInsideDevicePosition(num10, num13, num9, targetBV);
					}
					WaterValue water = GetWater(x2, n, z2);
					if (!bCopyAirBlocks && targetBV.isair && n >= -yOffset && !water.HasMass())
					{
						continue;
					}
					BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(num10, num13, num9);
					if (!_bForceOverwriteBlocks && !blockNoDamage2.Block.shape.IsTerrain() && !blockNoDamage2.isair && (blockNoDamage2.ischild || blockNoDamage2.type == targetBV.type))
					{
						_chunk.SetDensity(num10, num13, num9, b);
						continue;
					}
					if (!flag4)
					{
						TextureFullArray texture = GetTexture(x2, n, z2);
						_chunk.GetSetTextureFullArray(num10, num13, num9, texture);
					}
					_chunk.SetBlock(_world, num10, num13, num9, targetBV, _notifyAddChange: true, _notifyRemove: true, !_questTags.IsEmpty, _poiOwned: true);
					_chunk.SetWater(num10, num13, num9, water);
					_chunk.SetDensity(num10, num13, num9, b);
					TileEntity tileEntity;
					if (block5.IsTileEntitySavedInPrefab() && (tileEntity = GetTileEntity(blockPos)) != null)
					{
						TileEntity tileEntity2 = _chunk.GetTileEntity(new Vector3i(num10, num13, num9));
						if (tileEntity2 == null)
						{
							tileEntity2 = tileEntity.Clone();
							tileEntity2.localChunkPos = new Vector3i(num10, num13, num9);
							tileEntity2.SetChunk(_chunk);
							_chunk.AddTileEntity(tileEntity2);
						}
						tileEntity2.CopyFrom(tileEntity);
						tileEntity2.localChunkPos = new Vector3i(num10, num13, num9);
					}
					BlockTrigger blockTrigger = GetBlockTrigger(blockPos);
					if (blockTrigger != null)
					{
						BlockTrigger blockTrigger2 = _chunk.GetBlockTrigger(new Vector3i(num10, num13, num9));
						if (blockTrigger2 == null)
						{
							blockTrigger2 = blockTrigger.Clone();
							blockTrigger2.LocalChunkPos = new Vector3i(num10, num13, num9);
							blockTrigger2.Chunk = _chunk;
							_chunk.AddBlockTrigger(blockTrigger2);
						}
						blockTrigger2.CopyFrom(blockTrigger);
						blockTrigger2.LocalChunkPos = new Vector3i(num10, num13, num9);
					}
				}
				if (num12 >= 0)
				{
					_chunk.SetTerrainHeight(num10, num9, (byte)num12);
				}
				_chunk.SetTopSoilBroken(num10, num9);
			}
		}
		CopyVolumesIntoWorld(_world, _chunk, _prefabTargetPos);
		ApplyDecoAllowed(_chunk, _prefabTargetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(Chunk _chunk, Vector3i _prefabTargetPos)
	{
		foreach (int decoAllowedBlockIndex in decoAllowedBlockIndices)
		{
			offsetToCoordRotated(decoAllowedBlockIndex, out var _x, out var _y, out var _z);
			BlockValue block = GetBlock(_x, _y, _z);
			DecoUtils.ApplyDecoAllowed(_chunk, _prefabTargetPos + new Vector3i(_x, _y, _z), block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockStatistics(BlockValue _bv, Block _b)
	{
		if (Block.BlocksLoaded && _b != null)
		{
			statistics.cntWindows += ((_b.BlockTag == BlockTags.Window) ? 1 : 0);
			statistics.cntDoors += ((_b.BlockTag == BlockTags.Door) ? 1 : 0);
			statistics.cntBlockEntities += ((_b.shape is BlockShapeModelEntity && !_bv.ischild && (!(_b is BlockModelTree) || _bv.meta == 0)) ? 1 : 0);
			statistics.cntSolid += ((!_bv.isair) ? 1 : 0);
		}
	}

	public BlockStatistics GetBlockStatistics()
	{
		return statistics;
	}

	public List<EntityCreationData> GetEntities()
	{
		return entities;
	}

	public void Mirror(EnumMirrorAlong _axis)
	{
		Data data = CellsToArrays();
		Data _data = default(Data);
		_data.Init(GetBlockCount());
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.y; k++)
				{
					int num = CoordToOffset(localRotation, i, k, j);
					WaterValue waterValue = data.m_Water[num];
					air.rawData = data.m_Blocks[num];
					if (!air.ischild && (!air.isair || waterValue.HasMass()))
					{
						Block block = air.Block;
						BlockShape shape = block.shape;
						int num2 = (byte)BlockShapeNew.MirrorStatic(_axis, air.rotation, shape.SymmetryType);
						Vector3i pos = new Vector3i(i, k, j);
						Vector3i vector3i = GameUtils.Mirror(_axis, pos, size);
						if (block.isMultiBlock)
						{
							Vector3 vector = new Vector3((block.multiBlockPos.dim.x % 2 == 0) ? (-0.5f) : 0f, (block.multiBlockPos.dim.y % 2 == 0) ? (-0.5f) : 0f, (block.multiBlockPos.dim.z % 2 == 0) ? (-0.5f) : 0f);
							Vector3 vector2 = BlockShapeNew.GetRotationStatic(air.rotation) * vector;
							Vector3 pos2 = GameUtils.Mirror(_axis, vector3i.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f), size) + vector2;
							Vector3 vector3 = GameUtils.Mirror(_axis, pos2, size);
							Vector3 vector4 = BlockShapeNew.GetRotationStatic(num2) * vector;
							vector3i = World.worldToBlockPos(vector3 - vector4);
						}
						int num3 = CoordToOffset(localRotation, vector3i.x, vector3i.y, vector3i.z);
						if (block.MirrorSibling != 0)
						{
							air.type = block.MirrorSibling;
						}
						air.rotation = (byte)num2;
						_data.m_Blocks[num3] = air.rawData;
						_data.m_Damage[num3] = data.m_Damage[num];
						_data.m_Density[num3] = data.m_Density[num];
						_data.m_Textures[num3] = mirrorTexture(_axis, shape, air.rotation, num2, data.m_Textures[num]);
						_data.m_Water[num3] = waterValue;
					}
				}
			}
		}
		CellsFromArrays(ref _data);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray mirrorTexture(EnumMirrorAlong _axis, BlockShape _shape, int _sourceRot, int _targetRot, TextureFullArray _tex)
	{
		TextureFullArray result = new TextureFullArray(0L);
		for (int i = 0; i < 6; i++)
		{
			BlockFace face = (BlockFace)i;
			_shape.MirrorFace(_axis, _sourceRot, _targetRot, face, out var _sourceFace, out var _targetFace);
			for (int j = 0; j < 1; j++)
			{
				long num = (_tex[j] >> 8 * (int)_sourceFace) & 0xFF;
				num <<= 8 * (int)_targetFace;
				result[j] |= num;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> loadIdMapping(string _directory, string _prefabFileName, bool _allowMissingBlocks)
	{
		if (!Block.BlocksLoaded)
		{
			Log.Error("Block data not loaded");
			return null;
		}
		string text = _directory + "/" + _prefabFileName + ".blocks.nim";
		if (!SdFile.Exists(text))
		{
			Log.Error("Loading prefab \"" + _prefabFileName + "\" failed: Block name to ID mapping file missing.");
			return null;
		}
		using NameIdMapping nameIdMapping = MemoryPools.poolNameIdMapping.AllocSync(_bReset: true);
		nameIdMapping.InitMapping(text, Block.MAX_BLOCKS);
		if (!nameIdMapping.LoadFromFile())
		{
			return null;
		}
		Block missingBlock = null;
		if (_allowMissingBlocks)
		{
			missingBlock = Block.GetBlockByName(MISSING_BLOCK_NAME);
			blockTypeMissingBlock = ((missingBlock != null) ? missingBlock.blockID : (-1));
		}
		return nameIdMapping.createIdTranslationTable([PublicizedFrom(EAccessModifier.Internal)] (string _blockName) => Block.GetBlockByName(_blockName)?.blockID ?? (-1), [PublicizedFrom(EAccessModifier.Internal)] (string _name, int _id) =>
		{
			if (!_allowMissingBlocks)
			{
				Log.Error($"Loading prefab \"{_prefabFileName}\" failed: Block \"{_name}\" ({_id}) used in prefab is unknown.");
				return -1;
			}
			if (missingBlock == null)
			{
				Log.Error($"Loading prefab \"{_prefabFileName}\" failed: Block \"{_name}\" ({_id}) used in prefab is unknown and the replacement block \"{MISSING_BLOCK_NAME}\" was not found.");
				return -1;
			}
			Log.Warning($"Loading prefab \"{_prefabFileName}\": Block \"{_name}\" ({_id}) used in prefab is unknown and getting replaced by \"{MISSING_BLOCK_NAME}\".");
			return missingBlock.blockID;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doRaycast(Ray _ray, out RaycastHit _hitInfo, Vector3i _min)
	{
		bool flag = Physics.Raycast(_ray, out _hitInfo, 255f, 1073807360);
		if (!flag)
		{
			return false;
		}
		Vector3 vector = _hitInfo.point + _ray.direction * 0.01f;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(vector.x), Utils.Fastfloor(vector.y), Utils.Fastfloor(vector.z));
		Block block = GetBlock(vector3i.x - _min.x, vector3i.y - _min.y, vector3i.z - _min.z).Block;
		if (block.bImposterDontBlock || block.bImposterExclude)
		{
			_ray.origin = _hitInfo.point + _ray.direction * 0.01f;
			flag = Physics.Raycast(_ray, out _hitInfo, 255f, 1073807360);
		}
		return flag;
	}

	public EnumInsideOutside[] UpdateInsideOutside(Vector3i _min, Vector3i _max)
	{
		EnumInsideOutside[] array = new EnumInsideOutside[GetBlockCount()];
		BlockValue air = BlockValue.Air;
		uint[] blocks = CellsToArrays().m_Blocks;
		RaycastHit _hitInfo;
		for (int i = _min.x; i <= _max.x; i++)
		{
			for (int j = _min.z; j <= _max.z; j++)
			{
				int num = _max.y;
				Ray ray = new Ray(Vector3.zero, Vector3.down);
				bool flag = false;
				float num2 = 0f;
				while (!flag && num2 <= 1f)
				{
					float num3 = 0f;
					while (!flag && num3 <= 1f)
					{
						ray.origin = new Vector3((float)i + num2, _max.y + 3, (float)j + num3);
						if (doRaycast(ray, out _hitInfo, _min))
						{
							num = Utils.FastMin(num, Utils.Fastfloor(_hitInfo.point.y + ray.direction.y * 0.1f));
						}
						else
						{
							num = _min.y;
							flag = true;
						}
						num3 += 0.25f;
					}
					num2 += 0.25f;
				}
				int num4 = i - _min.x + (num - _min.y) * size.x + (j - _min.z) * size.x * size.y;
				if (num4 >= 0 && num4 < array.Length)
				{
					while (num4 > 0)
					{
						air.rawData = blocks[num4];
						if (!air.isair)
						{
							break;
						}
						num4 -= size.x;
						num--;
					}
					if (num4 > 0)
					{
						air.rawData = blocks[num4];
						if (air.ischild)
						{
							int type = air.type;
							while (num4 > 0)
							{
								air.rawData = blocks[num4];
								if (air.type != type)
								{
									break;
								}
								num4 -= size.x;
								num--;
							}
						}
					}
				}
				for (int num5 = _max.y; num5 >= num; num5--)
				{
					num4 = i - _min.x + (num5 - _min.y) * size.x + (j - _min.z) * size.x * size.y;
					if (num4 >= 0 && num4 < array.Length)
					{
						array[num4] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int k = _min.z; k <= _max.z; k++)
		{
			for (int l = _min.y; l <= _max.y; l++)
			{
				int num6 = _min.x;
				Ray ray = new Ray(Vector3.zero, Vector3.right);
				bool flag2 = false;
				float num7 = 0f;
				while (!flag2 && num7 <= 1f)
				{
					float num8 = 0f;
					while (!flag2 && num8 <= 1f)
					{
						ray.origin = new Vector3(_min.x - 3, (float)l + num7, (float)k + num8);
						if (doRaycast(ray, out _hitInfo, _min))
						{
							num6 = Utils.FastMax(num6, Utils.Fastfloor(_hitInfo.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num6 = _max.x;
							flag2 = true;
						}
						num8 += 0.25f;
					}
					num7 += 0.25f;
				}
				int num9 = num6 - _min.x + (l - _min.y) * size.x + (k - _min.z) * size.x * size.y;
				if (num9 >= 0 && num9 < array.Length)
				{
					while (num9 < blocks.Length - 1)
					{
						air.rawData = blocks[num9];
						if (!air.isair)
						{
							break;
						}
						num9++;
						num6++;
					}
					if (num9 < array.Length)
					{
						air.rawData = blocks[num9];
						if (air.ischild)
						{
							int type2 = air.type;
							while (num9 > 0)
							{
								air.rawData = blocks[num9];
								if (air.type != type2)
								{
									break;
								}
								num9++;
								num6++;
							}
						}
					}
				}
				for (int m = _min.x; m <= num6; m++)
				{
					num9 = m - _min.x + (l - _min.y) * size.x + (k - _min.z) * size.x * size.y;
					if (num9 >= 0 && num9 < array.Length)
					{
						array[num9] = EnumInsideOutside.Outside;
					}
				}
				int num10 = _max.x;
				ray = new Ray(Vector3.zero, Vector3.left);
				flag2 = false;
				float num11 = 0f;
				while (!flag2 && num11 <= 1f)
				{
					float num12 = 0f;
					while (!flag2 && num12 <= 1f)
					{
						ray.origin = new Vector3(_max.x + 3, (float)l + num11, (float)k + num12);
						if (doRaycast(ray, out _hitInfo, _min))
						{
							num10 = Utils.FastMin(num10, Utils.Fastfloor(_hitInfo.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num10 = _min.x;
							flag2 = true;
						}
						num12 += 0.25f;
					}
					num11 += 0.25f;
				}
				num9 = num10 - _min.x + (l - _min.y) * size.x + (k - _min.z) * size.x * size.y;
				if (num9 >= 0 && num9 < array.Length)
				{
					while (num9 > 0)
					{
						air.rawData = blocks[num9];
						if (!air.isair)
						{
							break;
						}
						num9--;
						num10--;
					}
					if (num9 > 0)
					{
						air.rawData = blocks[num9];
						if (air.ischild)
						{
							int type3 = air.type;
							while (num9 > 0)
							{
								air.rawData = blocks[num9];
								if (air.type != type3)
								{
									break;
								}
								num9--;
								num10--;
							}
						}
					}
				}
				for (int num13 = _max.x; num13 >= num10; num13--)
				{
					num9 = num13 - _min.x + (l - _min.y) * size.x + (k - _min.z) * size.x * size.y;
					if (num9 >= 0 && num9 < array.Length)
					{
						array[num9] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int n = _min.x; n <= _max.x; n++)
		{
			for (int num14 = _min.y; num14 <= _max.y; num14++)
			{
				int num15 = _min.z;
				Ray ray = new Ray(Vector3.zero, Vector3.forward);
				bool flag3 = false;
				float num16 = 0f;
				while (!flag3 && num16 <= 1f)
				{
					float num17 = 0f;
					while (!flag3 && num17 <= 1f)
					{
						ray.origin = new Vector3((float)n + num16, (float)num14 + num17, _min.z - 3);
						if (doRaycast(ray, out _hitInfo, _min))
						{
							num15 = Utils.FastMax(num15, Utils.Fastfloor(_hitInfo.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num15 = _max.z;
							flag3 = true;
						}
						num17 += 0.25f;
					}
					num16 += 0.25f;
				}
				int num18 = n - _min.x + (num14 - _min.y) * size.x + (num15 - _min.z) * size.x * size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 < blocks.Length - 1)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 += size.x * size.y;
						num15++;
					}
					if (num18 < array.Length)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type4 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type4)
								{
									break;
								}
								num18 += size.x * size.y;
								num15++;
							}
						}
					}
				}
				UnityEngine.Debug.DrawLine(ray.origin, new Vector3(ray.origin.x, ray.origin.y, num15), Color.blue, 10f);
				for (int num19 = _min.z; num19 <= num15; num19++)
				{
					num18 = n - _min.x + (num14 - _min.y) * size.x + (num19 - _min.z) * size.x * size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
				int num20 = _max.z;
				ray = new Ray(Vector3.zero, Vector3.back);
				flag3 = false;
				float num21 = 0f;
				while (!flag3 && num21 <= 1f)
				{
					float num22 = 0f;
					while (!flag3 && num22 <= 1f)
					{
						ray.origin = new Vector3((float)n + num21, (float)num14 + num22, _max.z + 3);
						if (doRaycast(ray, out _hitInfo, _min))
						{
							num20 = Utils.FastMin(num20, Utils.Fastfloor(_hitInfo.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num20 = _min.z;
							flag3 = true;
						}
						num22 += 0.25f;
					}
					num21 += 0.25f;
				}
				num18 = n - _min.x + (num14 - _min.y) * size.x + (num20 - _min.z) * size.x * size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 -= size.x * size.y;
						num15--;
					}
					if (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type5 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type5)
								{
									break;
								}
								num18 -= size.x * size.y;
								num15--;
							}
						}
					}
				}
				for (int num23 = _max.z; num23 >= num20; num23--)
				{
					num18 = n - _min.x + (num14 - _min.y) * size.x + (num23 - _min.z) * size.x * size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		return array;
	}

	public void RecalcInsideDevices(EnumInsideOutside[] _eInsideOutside)
	{
		insidePos.Init(size);
		if (!IsCullThisPrefab())
		{
			return;
		}
		int blockCount = GetBlockCount();
		for (int i = 0; i < blockCount; i++)
		{
			offsetToCoord(i, out var _x, out var _y, out var _z);
			if (!GetBlock(_x, _y, _z).Block.shape.IsTerrain() && _eInsideOutside[i] == EnumInsideOutside.Inside)
			{
				insidePos.Add(i);
			}
		}
	}

	public Vector3i? GetFirstIndexedBlockOffsetOfType(string _indexName)
	{
		if (indexedBlockOffsets.TryGetValue(_indexName, out var value) && value.Count > 0)
		{
			return value[0];
		}
		return null;
	}

	public IChunk GetChunkSync(int _chunkX, int _chunkZ)
	{
		return GetChunk(_chunkX, _chunkZ);
	}

	public IChunk GetChunkSync(long _chunkKey)
	{
		return GetChunkSync(WorldChunkCache.extractX(_chunkKey), WorldChunkCache.extractZ(_chunkKey));
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkX, chunkY, chunkZ);
	}

	public IChunk GetChunkSync(Vector2i chunkPos)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkPos);
	}

	public IChunk GetChunkSync(PropRef propRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, propRef);
	}

	public IChunk GetChunkSync(BlockValueRef bvRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, bvRef);
	}

	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, y, z);
	}

	public IChunk GetChunkFromWorldPos(Vector3i blockPos)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos);
	}

	public bool GetChunkFromWorldPos(int x, int z, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z, ref chunk);
	}

	public bool GetChunkFromWorldPos(Vector3i blockPos, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos, ref chunk);
	}

	public IEnumerator ToTransform(bool _genBlockModels, bool _genTerrain, bool _genBlockShapes, bool _fillEmptySpace, Transform _parent, string _name, Vector3 _position, int _heightLimit = 0)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		GameObject _go = new GameObject();
		_go.name = _name;
		_go.transform.SetParent(_parent);
		int ySize = 8;
		if (_heightLimit == 0)
		{
			_heightLimit = size.y;
		}
		else if (_heightLimit < 0)
		{
			_heightLimit = -yOffset - _heightLimit;
		}
		_heightLimit = Mathf.Clamp(_heightLimit, 0, size.y);
		int y = 0;
		int y2 = 0;
		while (y < _heightLimit + 1)
		{
			int x = 0;
			int x2 = 0;
			while (x < size.x + 1)
			{
				int z = 0;
				int z2 = 0;
				while (z < size.z + 1 && !(_go == null))
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.SetParent(_go.transform, worldPositionStays: false);
					gameObject.name = $"Chunk[{x2},{z2}]";
					MeshFilter[] array = new MeshFilter[MeshDescription.meshes.Length];
					MeshRenderer[] array2 = new MeshRenderer[MeshDescription.meshes.Length];
					MeshCollider[] array3 = new MeshCollider[MeshDescription.meshes.Length];
					GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
					GameObject gameObject2 = new GameObject("_BlockEntities");
					GameObject gameObject3 = new GameObject("Meshes");
					gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
					gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: false);
					for (int i = 0; i < MeshDescription.meshes.Length; i++)
					{
						array4[i] = new GameObject(MeshDescription.meshes[i].Name);
						array4[i].transform.parent = gameObject3.transform;
						VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, _bAllowLOD: false, out array[i], out array2[i], out array3[i]);
					}
					VoxelMesh[] array5 = new VoxelMesh[6];
					for (int j = 0; j < array5.Length; j++)
					{
						if (j == 5)
						{
							array5[j] = new VoxelMeshTerrain(j)
							{
								IsPreviewVoxelMesh = true
							};
						}
						else
						{
							array5[j] = new VoxelMesh(j);
						}
					}
					MeshGeneratorPrefab meshGeneratorPrefab = new MeshGeneratorPrefab(this);
					Vector3i worldStartPos = new Vector3i(x, y, z);
					Vector3i worldEndPos = new Vector3i(x + 15, y + ySize, z + 16);
					if (_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshOffset(worldStartPos, worldEndPos, array5);
					}
					else if (!_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshNoTerrain(worldStartPos, worldEndPos, array5);
					}
					else if (_genTerrain && !_genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshTerrainOnly(worldStartPos, worldEndPos, array5);
					}
					for (int k = 0; k < array5.Length; k++)
					{
						array5[k].CopyToMesh(array[k], array2[k], 0);
					}
					if (_genBlockModels)
					{
						for (int l = y; l < y + ySize && l < size.y; l++)
						{
							for (int m = x; m < x + 16 && m < size.x; m++)
							{
								for (int n = z; n < z + 16 && n < size.z; n++)
								{
									Vector3i vector3i = new Vector3i(m, l, n);
									BlockValue block = GetBlock(m, l, n);
									if (block.ischild)
									{
										continue;
									}
									Block block2 = block.Block;
									if (block2.shape is BlockShapeModelEntity blockShapeModelEntity)
									{
										Quaternion rotation = blockShapeModelEntity.GetRotation(block);
										Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
										rotatedOffset.x += 0.5f;
										rotatedOffset.z += 0.5f;
										rotatedOffset.y += 0f;
										Vector3 localPosition = vector3i.ToVector3() + rotatedOffset;
										GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
										if (!(objectForType == null))
										{
											Transform transform = objectForType.transform;
											transform.parent = gameObject2.transform;
											transform.localScale = Vector3.one;
											transform.localPosition = localPosition;
											transform.localRotation = rotation;
										}
									}
								}
							}
						}
					}
					yield return null;
					z += 16;
					z2++;
				}
				x += 16;
				x2++;
			}
			y += ySize;
			y2++;
		}
		if (_go != null)
		{
			_go.transform.localPosition = new Vector3(_position.x * _go.transform.localScale.x, _position.y * _go.transform.localScale.y, _position.z * _go.transform.localScale.z);
		}
		Log.Out($"Prefab preview generation took {(float)ms.ElapsedMilliseconds / 1000f} seconds.");
	}

	public void HandleAddingTriggerLayers(BlockTrigger _trigger)
	{
		for (int i = 0; i < _trigger.TriggersIndices.Count; i++)
		{
			if (!TriggerLayers.Contains(_trigger.TriggersIndices[i]))
			{
				TriggerLayers.Add(_trigger.TriggersIndices[i]);
			}
		}
		for (int j = 0; j < _trigger.TriggeredByIndices.Count; j++)
		{
			if (!TriggerLayers.Contains(_trigger.TriggeredByIndices[j]))
			{
				TriggerLayers.Add(_trigger.TriggeredByIndices[j]);
			}
		}
	}

	public void HandleAddingTriggerLayers(PrefabTriggerVolume _trigger)
	{
		for (int i = 0; i < _trigger.TriggersIndices.Count; i++)
		{
			if (!TriggerLayers.Contains(_trigger.TriggersIndices[i]))
			{
				TriggerLayers.Add(_trigger.TriggersIndices[i]);
			}
		}
	}

	public void AddInitialTriggerLayers()
	{
		for (byte b = 1; b < 6; b++)
		{
			TriggerLayers.Add(b);
		}
	}

	public void AddNewTriggerLayer()
	{
		if (TriggerLayers.Count > 0)
		{
			TriggerLayers.Sort();
			int num = TriggerLayers[TriggerLayers.Count - 1] + 1;
			if (num < 255 && num > 0)
			{
				TriggerLayers.Add((byte)num);
			}
		}
		else
		{
			TriggerLayers.Add(1);
		}
	}

	[Conditional("DEBUG_PREFABLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogPrefab(string _format, params object[] _args)
	{
		Log.Warning($"{GameManager.frameCount} Prefab {_format}", _args);
	}
}
