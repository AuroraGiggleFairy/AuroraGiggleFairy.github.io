using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Scripting;

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

	public class PrefabSleeperVolume
	{
		public bool used;

		public Vector3i startPos;

		public Vector3i size;

		public string groupName;

		public bool isPriority;

		public bool isQuestExclude;

		public short spawnCountMin;

		public short spawnCountMax;

		public short groupId;

		public int flags;

		public string minScript;

		public List<byte> triggeredByIndices = new List<byte>();

		public PrefabSleeperVolume()
		{
		}

		public PrefabSleeperVolume(PrefabSleeperVolume _other)
		{
			used = _other.used;
			startPos = _other.startPos;
			size = _other.size;
			groupId = _other.groupId;
			groupName = _other.groupName;
			isPriority = _other.isPriority;
			isQuestExclude = _other.isQuestExclude;
			spawnCountMin = _other.spawnCountMin;
			spawnCountMax = _other.spawnCountMax;
			triggeredByIndices = _other.triggeredByIndices;
			flags = _other.flags;
			minScript = _other.minScript;
		}

		public void Use(Vector3i _startPos, Vector3i _size, short _groupId, string _groupName, bool _isPriority, bool _isQuestExclude, int _spawnMin, int _spawnMax, int _flags)
		{
			used = true;
			startPos = _startPos;
			size = _size;
			groupId = _groupId;
			groupName = _groupName;
			isPriority = _isPriority;
			isQuestExclude = _isQuestExclude;
			spawnCountMin = (short)_spawnMin;
			spawnCountMax = (short)_spawnMax;
			flags = _flags;
		}

		public void SetTrigger(SleeperVolume.ETriggerType type)
		{
			flags = (flags & -8) | (int)type;
		}

		public void SetTriggeredByFlag(byte index)
		{
			if (!triggeredByIndices.Contains(index))
			{
				triggeredByIndices.Add(index);
			}
		}

		public void ClearTriggeredBy()
		{
			triggeredByIndices.Clear();
		}

		public void RemoveTriggeredByFlag(byte index)
		{
			triggeredByIndices.Remove(index);
		}

		public bool HasTriggeredBy(byte index)
		{
			return triggeredByIndices.Contains(index);
		}

		public bool HasAnyTriggeredBy()
		{
			return triggeredByIndices.Count > 0;
		}
	}

	public class PrefabTeleportVolume
	{
		public Vector3i startPos;

		public Vector3i size;

		public bool used;

		public PrefabTeleportVolume()
		{
		}

		public PrefabTeleportVolume(PrefabTeleportVolume _other)
		{
			startPos = _other.startPos;
			size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			used = true;
			startPos = _startPos;
			size = _size;
		}
	}

	public class PrefabInfoVolume
	{
		public Vector3i startPos;

		public Vector3i size;

		public bool used;

		public PrefabInfoVolume()
		{
		}

		public PrefabInfoVolume(PrefabInfoVolume _other)
		{
			startPos = _other.startPos;
			size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			used = true;
			startPos = _startPos;
			size = _size;
		}
	}

	public class PrefabWallVolume
	{
		public Vector3i startPos;

		public Vector3i size;

		public PrefabWallVolume()
		{
		}

		public PrefabWallVolume(PrefabWallVolume _other)
		{
			startPos = _other.startPos;
			size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			startPos = _startPos;
			size = _size;
		}
	}

	[Preserve]
	public class PrefabTriggerVolume
	{
		public Vector3i startPos;

		public Vector3i size;

		public PrefabTriggerData TriggerDataOwner;

		public List<byte> TriggersIndices = new List<byte>();

		public bool used;

		public PrefabTriggerVolume()
		{
		}

		public PrefabTriggerVolume(PrefabTriggerVolume _other)
		{
			startPos = _other.startPos;
			size = _other.size;
			TriggersIndices = _other.TriggersIndices;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			startPos = _startPos;
			size = _size;
			used = true;
		}

		public void SetTriggersFlag(byte index)
		{
			if (!TriggersIndices.Contains(index))
			{
				TriggersIndices.Add(index);
			}
		}

		public void RemoveTriggersFlag(byte index)
		{
			TriggersIndices.Remove(index);
		}

		public void RemoveAllTriggersFlags()
		{
			TriggersIndices.Clear();
		}

		public bool HasTriggers(byte index)
		{
			return TriggersIndices.Contains(index);
		}

		public bool HasAnyTriggers()
		{
			return TriggersIndices.Count > 0;
		}
	}

	public class Marker
	{
		public enum MarkerTypes : byte
		{
			None,
			POISpawn,
			RoadExit,
			PartSpawn
		}

		public enum MarkerSize : byte
		{
			One,
			ExtraSmall,
			Small,
			Medium,
			Large,
			Custom
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i start;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i size;

		[PublicizedFrom(EAccessModifier.Private)]
		public MarkerTypes markerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Poi> tags;

		[PublicizedFrom(EAccessModifier.Private)]
		public string partToSpawn;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte rotations;

		[PublicizedFrom(EAccessModifier.Private)]
		public float partChanceToSpawn = 1f;

		public bool PartDirty = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public int groupId = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public string groupName;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color color;

		public static List<Vector3i> MarkerSizes = new List<Vector3i>
		{
			Vector3i.one,
			new Vector3i(25, 0, 25),
			new Vector3i(42, 0, 42),
			new Vector3i(60, 0, 60),
			new Vector3i(100, 0, 100)
		};

		public string GroupName
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return groupName;
			}
			set
			{
				if (groupName != value)
				{
					color = default(Color);
					groupId = -1;
					groupName = value;
				}
			}
		}

		public Color GroupColor
		{
			get
			{
				if (color == default(Color))
				{
					GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(GroupId);
					color = new Color32((byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)((MarkerType == MarkerTypes.PartSpawn) ? 32u : 128u));
				}
				return color;
			}
		}

		public int GroupId
		{
			get
			{
				if (groupId == -1)
				{
					groupId = GroupName.GetHashCode();
				}
				return groupId;
			}
		}

		public Vector3i Start
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return start;
			}
			set
			{
				if (start != value)
				{
					start = value;
				}
			}
		}

		public Vector3i Size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return size;
			}
			set
			{
				if (size != value)
				{
					size = value;
					PartDirty = true;
				}
			}
		}

		public MarkerTypes MarkerType
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return markerType;
			}
			set
			{
				if (markerType != value)
				{
					markerType = value;
					PartDirty = true;
				}
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (name != value)
				{
					name = value;
				}
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
				if (!tags.Equals(value))
				{
					tags = value;
				}
			}
		}

		public string PartToSpawn
		{
			get
			{
				return partToSpawn;
			}
			set
			{
				if (partToSpawn != value)
				{
					partToSpawn = value;
					PartDirty = true;
				}
			}
		}

		public byte Rotations
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return rotations;
			}
			set
			{
				if (rotations != value)
				{
					rotations = value;
					PartDirty = true;
				}
			}
		}

		public float PartChanceToSpawn
		{
			get
			{
				return partChanceToSpawn;
			}
			set
			{
				if ((float)(int)rotations != value)
				{
					partChanceToSpawn = value;
					PartDirty = true;
				}
			}
		}

		public Marker()
		{
		}

		public Marker(Vector3i _start, Vector3i _size, MarkerTypes _type, string _group, FastTags<TagGroup.Poi> _tags)
		{
			Start = _start;
			Size = _size;
			MarkerType = _type;
			GroupName = _group;
			Tags = _tags;
		}

		public Marker(Marker _other)
		{
			Start = _other.Start;
			Size = _other.Size;
			MarkerType = _other.MarkerType;
			GroupName = _other.GroupName;
			Tags = _other.Tags;
			Name = _other.Name;
			PartToSpawn = _other.PartToSpawn;
			Rotations = _other.Rotations;
			PartChanceToSpawn = _other.PartChanceToSpawn;
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

	public class PrefabChunk : IChunk
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

		public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _blockFace, int channel)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0;
			}
			return (int)((prefab.GetTexture(_x, _y, _z)[channel] >> (int)_blockFace * 6) & 0x3F);
		}

		public long GetTextureFull(int _x, int _y, int _z, int channel = 0)
		{
			if (!checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0L;
			}
			return prefab.GetTexture(_x, _y, _z)[channel];
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

		public byte GetLight(int x, int y, int z, Chunk.LIGHT_TYPE type)
		{
			return 15;
		}

		public int GetLightValue(int x, int y, int z, int _darknessV)
		{
			return 15;
		}

		public float GetLightBrightness(int x, int y, int z, int _darknessV)
		{
			return 1f;
		}

		public Vector3i GetWorldPos()
		{
			return new Vector3i(X, Y, Z);
		}

		public void SetVertexOffset(int x, int y, int z, Vector3 _vertexOffset)
		{
		}

		public bool GetVertexOffset(int _x, int _y, int _z, out Vector3 _vertexOffset)
		{
			_vertexOffset = Vector3.zero;
			return false;
		}

		public void SetVertexYOffset(int x, int y, int z, float _addYPos)
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

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_IsTraderArea = "TraderArea";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TraderAreaProtect = "TraderAreaProtect";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeStart = "SleeperVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeSize = "SleeperVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeGroup = "SleeperVolumeGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeGroupId = "SleeperVolumeGroupId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperIsPriorityVolume = "SleeperIsLootVolume";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperIsQuestExclude = "SleeperIsQuestExclude";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeFlags = "SleeperVolumeFlags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeTriggeredBy = "SleeperVolumeTriggeredBy";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeScript = "SVS";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TeleportVolumeStart = "TeleportVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TeleportVolumeSize = "TeleportVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_InfoVolumeStart = "InfoVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_InfoVolumeSize = "InfoVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_WallVolumeStart = "WallVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_WallVolumeSize = "WallVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TriggerVolumeStart = "TriggerVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TriggerVolumeSize = "TriggerVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
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
	public const string cProp_StaticSpawnerClass = "StaticSpawner.Class";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_StaticSpawnerSize = "StaticSpawner.Size";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_StaticSpawnerTrigger = "StaticSpawner.Trigger";

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

	public List<PrefabSleeperVolume> SleeperVolumes = new List<PrefabSleeperVolume>();

	public List<PrefabTeleportVolume> TeleportVolumes = new List<PrefabTeleportVolume>();

	public List<PrefabInfoVolume> InfoVolumes = new List<PrefabInfoVolume>();

	public List<PrefabWallVolume> WallVolumes = new List<PrefabWallVolume>();

	public List<PrefabTriggerVolume> TriggerVolumes = new List<PrefabTriggerVolume>();

	public int yOffset;

	public int Transient_NumSleeperSpawns;

	public List<Marker> POIMarkers = new List<Marker>();

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

	public string StaticSpawnerClass;

	public Vector3i StaticSpawnerSize;

	public int StaticSpawnerTrigger;

	public bool StaticSpawnerCreated;

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
	public PrefabInsideDataFile insidePos = new PrefabInsideDataFile();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, BlockTrigger> triggerData = new Dictionary<Vector3i, BlockTrigger>();

	public List<byte> TriggerLayers = new List<byte>();

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

	public string LocalizedEnglishName => Localization.Get(PrefabName, "english");

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

	public bool bSleeperVolumes
	{
		get
		{
			if (SleeperVolumes != null)
			{
				return SleeperVolumes.Count > 0;
			}
			return false;
		}
	}

	public bool bInfoVolumes
	{
		get
		{
			if (InfoVolumes != null)
			{
				return InfoVolumes.Count > 0;
			}
			return false;
		}
	}

	public bool bWallVolumes
	{
		get
		{
			if (WallVolumes != null)
			{
				return WallVolumes.Count > 0;
			}
			return false;
		}
	}

	public bool bTriggerVolumes
	{
		get
		{
			if (TriggerVolumes != null)
			{
				return TriggerVolumes.Count > 0;
			}
			return false;
		}
	}

	public bool bPOIMarkers
	{
		get
		{
			if (POIMarkers != null)
			{
				return POIMarkers.Count > 0;
			}
			return false;
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
	}

	public Prefab(Prefab _other, bool sharedData = false)
	{
		size = _other.size;
		if (sharedData)
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
		if (sharedData)
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
		SleeperVolumes = (_other.bSleeperVolumes ? _other.SleeperVolumes.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _input) => new PrefabSleeperVolume(_input)) : new List<PrefabSleeperVolume>());
		TeleportVolumes = (_other.bTraderArea ? _other.TeleportVolumes.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabTeleportVolume _input) => new PrefabTeleportVolume(_input)) : new List<PrefabTeleportVolume>());
		InfoVolumes = (_other.bInfoVolumes ? _other.InfoVolumes.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabInfoVolume _input) => new PrefabInfoVolume(_input)) : new List<PrefabInfoVolume>());
		WallVolumes = (_other.bWallVolumes ? _other.WallVolumes.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabWallVolume _input) => new PrefabWallVolume(_input)) : new List<PrefabWallVolume>());
		TriggerVolumes = (_other.bTriggerVolumes ? _other.TriggerVolumes.ConvertAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabTriggerVolume _input) => new PrefabTriggerVolume(_input)) : new List<PrefabTriggerVolume>());
		yOffset = _other.yOffset;
		rotationToFaceNorth = _other.rotationToFaceNorth;
		allowedBiomes = new List<string>(_other.allowedBiomes);
		allowedTownships = new List<string>(_other.allowedTownships);
		allowedZones = new List<string>(_other.allowedZones);
		tags = new FastTags<TagGroup.Poi>(_other.tags);
		themeTags = new FastTags<TagGroup.Poi>(_other.themeTags);
		themeRepeatDistance = _other.themeRepeatDistance;
		duplicateRepeatDistance = _other.duplicateRepeatDistance;
		StaticSpawnerClass = _other.StaticSpawnerClass;
		StaticSpawnerSize = _other.StaticSpawnerSize;
		StaticSpawnerTrigger = _other.StaticSpawnerTrigger;
		questTags = _other.questTags;
		DifficultyTier = _other.DifficultyTier;
		ShowQuestClearCount = _other.ShowQuestClearCount;
		localRotation = _other.localRotation;
		for (int num = 0; num < _other.entities.Count; num++)
		{
			EntityCreationData entityCreationData = _other.entities[num];
			entities.Add(entityCreationData.Clone());
		}
		foreach (KeyValuePair<Vector3i, TileEntity> tileEntity in _other.tileEntities)
		{
			tileEntities.Add(tileEntity.Key, tileEntity.Value);
		}
		POIMarkers = new List<Marker>();
		for (int num2 = 0; num2 < _other.POIMarkers.Count; num2++)
		{
			POIMarkers.Add(new Marker(_other.POIMarkers[num2]));
		}
		insidePos = _other.insidePos.Clone();
		foreach (KeyValuePair<Vector3i, BlockTrigger> triggerDatum in _other.triggerData)
		{
			triggerData.Add(triggerDatum.Key, triggerDatum.Value);
		}
		for (int num3 = 0; num3 < _other.TriggerLayers.Count; num3++)
		{
			TriggerLayers.Add(_other.TriggerLayers[num3]);
		}
		renderingCost = _other.renderingCost;
	}

	public Prefab(Vector3i _size)
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

	public Prefab Clone(bool sharedData = false)
	{
		return new Prefab(this, sharedData);
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
		BlockValue bv = BlockValue.Air;
		RotateCoords(ref _x, ref _z);
		Cells<uint>.Cell cell = blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			bv.rawData = cell.Get(_x, _z);
			Cells<ushort>.Cell cell2 = damageCells.GetCell(_x, _y, _z);
			if (cell2.a != null)
			{
				bv.damage = cell2.Get(_x, _z);
			}
			if (!isCellsDataOwner && localRotation != 0)
			{
				ApplyRotation(ref bv);
			}
		}
		return bv;
	}

	public BlockValue GetBlockNoDamage(int _localRotation, int _x, int _y, int _z)
	{
		BlockValue bv = BlockValue.Air;
		RotateCoords(_localRotation, ref _x, ref _z);
		Cells<uint>.Cell cell = blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			bv.rawData = cell.Get(_x, _z);
			if (!isCellsDataOwner && localRotation != 0)
			{
				ApplyRotation(ref bv);
			}
		}
		return bv;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRotation(ref BlockValue bv)
	{
		if (bv.ischild)
		{
			int _x = bv.parentx;
			int _z = bv.parentz;
			if (_x != 0 || _z != 0)
			{
				InverseRotateRelative(ref _x, ref _z);
				bv.parentx = _x;
				bv.parentz = _z;
			}
		}
		else
		{
			bv = bv.Block.shape.RotateY(_bLeft: true, bv, localRotation);
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

	public byte GetStab(int relx, int absy, int relz)
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

	public void SetTexture(int _x, int _y, int _z, TextureFullArray _fulltexture)
	{
		RotateCoords(ref _x, ref _z);
		textureCells.SetData(_x, _y, _z, _fulltexture);
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

	public void ToggleQuestTag(FastTags<TagGroup.Global> questTag)
	{
		if (GetQuestTag(questTag))
		{
			questTags = questTags.Remove(questTag);
		}
		else
		{
			questTags |= questTag;
		}
	}

	public FastTags<TagGroup.Global> GetQuestTags()
	{
		return new FastTags<TagGroup.Global>(questTags);
	}

	public bool GetQuestTag(FastTags<TagGroup.Global> questTag)
	{
		return questTags.Test_AllSet(questTag);
	}

	public bool HasAnyQuestTag(FastTags<TagGroup.Global> questTag)
	{
		return questTags.Test_AnySet(questTag);
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
			editorGroups.AddRange(properties.GetStringValue("EditorGroups").Split(','));
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
		SleeperVolumes = new List<PrefabSleeperVolume>();
		DictionarySave<string, string> values = properties.Values;
		if (values.ContainsKey("SleeperVolumeSize") && values.ContainsKey("SleeperVolumeStart"))
		{
			List<Vector3i> list = StringParsers.ParseList(values["SleeperVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list2 = StringParsers.ParseList(values["SleeperVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<string> list3 = null;
			if (values.TryGetValue("SleeperVolumeGroupId", out var _value))
			{
				list3 = new List<string>(_value.Split(','));
			}
			List<string> list4 = null;
			if (values.TryGetValue("SleeperVolumeGroup", out _value))
			{
				list4 = new List<string>(_value.Split(','));
			}
			List<bool> list5 = (values.ContainsKey("SleeperIsLootVolume") ? StringParsers.ParseList(values["SleeperIsLootVolume"], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end)) : new List<bool>());
			List<bool> list6 = (values.ContainsKey("SleeperIsQuestExclude") ? StringParsers.ParseList(values["SleeperIsQuestExclude"], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end)) : new List<bool>());
			List<int> list7 = null;
			if (values.TryGetValue("SleeperVolumeFlags", out _value))
			{
				list7 = StringParsers.ParseList(_value, ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseSInt32(_s, _start, _end, NumberStyles.HexNumber));
			}
			List<string> list8 = null;
			if (values.TryGetValue("SleeperVolumeTriggeredBy", out _value))
			{
				list8 = StringParsers.ParseList(_value, '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
			}
			for (int num = 0; num < list2.Count; num++)
			{
				Vector3i startPos = list2[num];
				Vector3i vector3i = ((num < list.Count) ? list[num] : Vector3i.one);
				short groupId = 0;
				string text = "???";
				short spawnMin = 5;
				short spawnMax = 5;
				if (list3 != null)
				{
					groupId = StringParsers.ParseSInt16(list3[num]);
				}
				if (list4 != null)
				{
					if (list4.Count == list2.Count)
					{
						text = list4[num];
					}
					else if (list4.Count == list2.Count * 3)
					{
						int num2 = num * 3;
						text = list4[num2];
						spawnMin = StringParsers.ParseSInt16(list4[num2 + 1]);
						spawnMax = StringParsers.ParseSInt16(list4[num2 + 2]);
					}
					text = GameStageGroup.CleanName(text);
				}
				bool isPriority = num < list5.Count && list5[num];
				bool isQuestExclude = num < list6.Count && list6[num];
				int flags = 0;
				if (list7 != null && num < list7.Count)
				{
					flags = list7[num];
				}
				PrefabSleeperVolume prefabSleeperVolume = new PrefabSleeperVolume();
				prefabSleeperVolume.Use(startPos, vector3i, groupId, text, isPriority, isQuestExclude, spawnMin, spawnMax, flags);
				string text2 = properties.GetString("SVS" + num);
				if (text2.Length > 0)
				{
					prefabSleeperVolume.minScript = text2;
				}
				if (list8 != null && list8[num].Trim() != "")
				{
					prefabSleeperVolume.triggeredByIndices = StringParsers.ParseList(list8[num], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end));
				}
				SleeperVolumes.Add(prefabSleeperVolume);
			}
		}
		TeleportVolumes = new List<PrefabTeleportVolume>();
		if (values.ContainsKey("TeleportVolumeSize") && values.ContainsKey("TeleportVolumeStart"))
		{
			List<Vector3i> list9 = StringParsers.ParseList(values["TeleportVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list10 = StringParsers.ParseList(values["TeleportVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num3 = 0; num3 < list10.Count; num3++)
			{
				Vector3i startPos2 = list10[num3];
				Vector3i vector3i2 = ((num3 < list9.Count) ? list9[num3] : Vector3i.one);
				PrefabTeleportVolume prefabTeleportVolume = new PrefabTeleportVolume();
				prefabTeleportVolume.Use(startPos2, vector3i2);
				TeleportVolumes.Add(prefabTeleportVolume);
			}
		}
		InfoVolumes = new List<PrefabInfoVolume>();
		if (values.ContainsKey("InfoVolumeSize") && values.ContainsKey("InfoVolumeStart"))
		{
			List<Vector3i> list11 = StringParsers.ParseList(values["InfoVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list12 = StringParsers.ParseList(values["InfoVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num4 = 0; num4 < list12.Count; num4++)
			{
				Vector3i startPos3 = list12[num4];
				Vector3i vector3i3 = ((num4 < list11.Count) ? list11[num4] : Vector3i.one);
				PrefabInfoVolume prefabInfoVolume = new PrefabInfoVolume();
				prefabInfoVolume.Use(startPos3, vector3i3);
				InfoVolumes.Add(prefabInfoVolume);
			}
		}
		WallVolumes = new List<PrefabWallVolume>();
		if (values.ContainsKey("WallVolumeSize") && values.ContainsKey("WallVolumeStart"))
		{
			List<Vector3i> list13 = StringParsers.ParseList(values["WallVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list14 = StringParsers.ParseList(values["WallVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num5 = 0; num5 < list14.Count; num5++)
			{
				Vector3i startPos4 = list14[num5];
				Vector3i vector3i4 = ((num5 < list13.Count) ? list13[num5] : Vector3i.one);
				PrefabWallVolume prefabWallVolume = new PrefabWallVolume();
				prefabWallVolume.Use(startPos4, vector3i4);
				WallVolumes.Add(prefabWallVolume);
			}
		}
		TriggerVolumes = new List<PrefabTriggerVolume>();
		if (values.ContainsKey("TriggerVolumeSize") && values.ContainsKey("TriggerVolumeStart"))
		{
			List<Vector3i> list15 = StringParsers.ParseList(values["TriggerVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list16 = StringParsers.ParseList(values["TriggerVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<string> list17 = StringParsers.ParseList(values["TriggerVolumeTriggers"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
			for (int num6 = 0; num6 < list16.Count; num6++)
			{
				Vector3i startPos5 = list16[num6];
				Vector3i vector3i5 = ((num6 < list15.Count) ? list15[num6] : Vector3i.one);
				PrefabTriggerVolume prefabTriggerVolume = new PrefabTriggerVolume();
				prefabTriggerVolume.Use(startPos5, vector3i5);
				if (list17[num6].Trim() != "")
				{
					prefabTriggerVolume.TriggersIndices = StringParsers.ParseList(list17[num6], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end));
				}
				TriggerVolumes.Add(prefabTriggerVolume);
				HandleAddingTriggerLayers(prefabTriggerVolume);
			}
		}
		if (values.ContainsKey("POIMarkerSize") && values.ContainsKey("POIMarkerStart"))
		{
			POIMarkers.Clear();
			List<Vector3i> list18 = StringParsers.ParseList(values["POIMarkerSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list19 = StringParsers.ParseList(values["POIMarkerStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Marker.MarkerTypes> list20 = new List<Marker.MarkerTypes>();
			if (values.ContainsKey("POIMarkerType"))
			{
				string[] array = values["POIMarkerType"].Split(',');
				for (int num7 = 0; num7 < array.Length; num7++)
				{
					if (Enum.TryParse<Marker.MarkerTypes>(array[num7], ignoreCase: true, out var result))
					{
						list20.Add(result);
					}
				}
			}
			List<FastTags<TagGroup.Poi>> list21 = new List<FastTags<TagGroup.Poi>>();
			if (values.ContainsKey("POIMarkerTags"))
			{
				string[] array = values["POIMarkerTags"].Split('#');
				for (int num8 = 0; num8 < array.Length; num8++)
				{
					if (array[num8].Length > 0)
					{
						list21.Add(FastTags<TagGroup.Poi>.Parse(array[num8]));
					}
					else
					{
						list21.Add(FastTags<TagGroup.Poi>.none);
					}
				}
			}
			List<string> list22 = new List<string>();
			if (values.ContainsKey("POIMarkerGroup"))
			{
				list22.AddRange(values["POIMarkerGroup"].Split(','));
			}
			List<string> list23 = new List<string>();
			if (values.ContainsKey("POIMarkerPartToSpawn"))
			{
				list23.AddRange(values["POIMarkerPartToSpawn"].Split(','));
			}
			List<int> list24 = new List<int>();
			if (values.ContainsKey("POIMarkerPartRotations"))
			{
				string[] array = values["POIMarkerPartRotations"].Split(',');
				string[] array2 = array;
				for (int num9 = 0; num9 < array2.Length; num9++)
				{
					if (StringParsers.TryParseSInt32(array2[num9], out var _result))
					{
						list24.Add(_result);
					}
					else
					{
						list24.Add(0);
					}
				}
			}
			List<float> list25 = new List<float>();
			if (values.ContainsKey("POIMarkerPartSpawnChance"))
			{
				string[] array = values["POIMarkerPartSpawnChance"].Split(',');
				string[] array2 = array;
				for (int num9 = 0; num9 < array2.Length; num9++)
				{
					if (StringParsers.TryParseFloat(array2[num9], out var _result2))
					{
						list25.Add(_result2);
					}
					else
					{
						list25.Add(0f);
					}
				}
			}
			for (int num10 = 0; num10 < list19.Count; num10++)
			{
				Marker marker = new Marker();
				marker.Start = list19[num10];
				if (num10 < list18.Count)
				{
					marker.Size = list18[num10];
				}
				if (num10 < list20.Count)
				{
					marker.MarkerType = list20[num10];
				}
				if (num10 < list22.Count)
				{
					marker.GroupName = list22[num10];
				}
				if (num10 < list21.Count)
				{
					marker.Tags = list21[num10];
				}
				if (num10 < list23.Count)
				{
					marker.PartToSpawn = list23[num10];
				}
				if (num10 < list24.Count)
				{
					marker.Rotations = (byte)list24[num10];
				}
				if (num10 < list25.Count)
				{
					marker.PartChanceToSpawn = list25[num10];
				}
				POIMarkers.Add(marker);
			}
		}
		yOffset = properties.GetInt("YOffset");
		if (size == Vector3i.zero && values.ContainsKey("PrefabSize"))
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
			foreach (KeyValuePair<string, DynamicProperties> item in properties.Classes["IndexedBlockOffsets"].Classes.Dict)
			{
				if (item.Value.Values.Dict.Count <= 0)
				{
					continue;
				}
				List<Vector3i> list26 = new List<Vector3i>();
				indexedBlockOffsets[item.Key] = list26;
				foreach (KeyValuePair<string, string> item2 in item.Value.Values.Dict)
				{
					list26.Add(StringParsers.ParseVector3i(item.Value.Values[item2.Key]));
				}
			}
		}
		if (properties.Values.ContainsKey("QuestTags"))
		{
			questTags = FastTags<TagGroup.Global>.Parse(properties.Values["QuestTags"]);
		}
		properties.ParseString("StaticSpawner.Class", ref StaticSpawnerClass);
		if (properties.Values.ContainsKey("StaticSpawner.Size"))
		{
			string[] array3 = properties.Values["StaticSpawner.Size"].Replace(" ", "").Split(',');
			int x = int.Parse(array3[0]);
			int y = int.Parse(array3[1]);
			int z = int.Parse(array3[2]);
			StaticSpawnerSize = new Vector3i(x, y, z);
		}
		properties.ParseInt("StaticSpawner.Trigger", ref StaticSpawnerTrigger);
		if (properties.Values.ContainsKey("AllowedTownships"))
		{
			allowedTownships.Clear();
			string[] array2 = properties.Values["AllowedTownships"].Replace(" ", "").Split(',');
			foreach (string text3 in array2)
			{
				allowedTownships.Add(text3.ToLower());
			}
		}
		if (properties.Values.ContainsKey("AllowedBiomes"))
		{
			allowedBiomes.Clear();
			string[] array2 = properties.Values["AllowedBiomes"].Replace(" ", "").Split(',');
			foreach (string text4 in array2)
			{
				allowedBiomes.Add(text4.ToLower());
			}
		}
		if (properties.Values.ContainsKey("Zoning"))
		{
			allowedZones.Clear();
			string[] array4 = properties.Values["Zoning"].Split(',');
			for (int num11 = 0; num11 < array4.Length; num11++)
			{
				AddAllowedZone(array4[num11].Trim());
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
		properties.Values["TraderArea"] = bTraderArea.ToString();
		if (bTraderArea)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (PrefabTeleportVolume teleportVolume in TeleportVolumes)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('#');
					stringBuilder2.Append('#');
				}
				stringBuilder.Append(teleportVolume.size.ToString());
				stringBuilder2.Append(teleportVolume.startPos.ToString());
			}
			properties.Values["TeleportVolumeSize"] = stringBuilder.ToString();
			properties.Values["TeleportVolumeStart"] = stringBuilder2.ToString();
		}
		else
		{
			properties.Values.Remove("TeleportVolumeSize");
			properties.Values.Remove("TeleportVolumeStart");
		}
		foreach (KeyValuePair<string, string> item in properties.Values.Dict)
		{
			if (item.Key.StartsWith("SVS"))
			{
				properties.Values.MarkToRemove(item.Key);
			}
		}
		properties.Values.RemoveAllMarked([PublicizedFrom(EAccessModifier.Private)] (string _key) =>
		{
			properties.Values.Remove(_key);
		});
		bool flag = true;
		if (bSleeperVolumes)
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder6 = new StringBuilder();
			StringBuilder stringBuilder7 = new StringBuilder();
			StringBuilder stringBuilder8 = new StringBuilder();
			StringBuilder stringBuilder9 = new StringBuilder();
			StringBuilder stringBuilder10 = new StringBuilder();
			foreach (PrefabSleeperVolume sleeperVolume in SleeperVolumes)
			{
				if (!sleeperVolume.used)
				{
					continue;
				}
				if (stringBuilder3.Length > 0)
				{
					stringBuilder3.Append('#');
					stringBuilder4.Append('#');
					stringBuilder5.Append(',');
					stringBuilder6.Append(',');
					stringBuilder7.Append(',');
					stringBuilder8.Append(',');
					stringBuilder9.Append(',');
					stringBuilder10.Append('#');
				}
				stringBuilder3.Append(sleeperVolume.size.ToString());
				stringBuilder4.Append(sleeperVolume.startPos.ToString());
				stringBuilder5.Append(sleeperVolume.groupId);
				stringBuilder6.Append(sleeperVolume.groupName);
				stringBuilder6.Append(',');
				stringBuilder6.Append(sleeperVolume.spawnCountMin.ToString());
				stringBuilder6.Append(',');
				stringBuilder6.Append(sleeperVolume.spawnCountMax.ToString());
				stringBuilder7.Append(sleeperVolume.isPriority.ToString());
				stringBuilder8.Append(sleeperVolume.isQuestExclude.ToString());
				stringBuilder9.Append(sleeperVolume.flags.ToString("x"));
				for (int num = 0; num < sleeperVolume.triggeredByIndices.Count; num++)
				{
					if (num > 0)
					{
						stringBuilder10.Append(',');
					}
					stringBuilder10.Append(sleeperVolume.triggeredByIndices[num].ToString());
				}
				if (sleeperVolume.triggeredByIndices.Count == 0)
				{
					stringBuilder10.Append(" ");
				}
			}
			if (stringBuilder3.Length > 0)
			{
				flag = false;
				properties.Values["SleeperVolumeSize"] = stringBuilder3.ToString();
				properties.Values["SleeperVolumeStart"] = stringBuilder4.ToString();
				properties.Values["SleeperVolumeGroupId"] = stringBuilder5.ToString();
				properties.Values["SleeperVolumeGroup"] = stringBuilder6.ToString();
				properties.Values["SleeperIsLootVolume"] = stringBuilder7.ToString();
				properties.Values["SleeperIsQuestExclude"] = stringBuilder8.ToString();
				properties.Values["SleeperVolumeFlags"] = stringBuilder9.ToString();
				properties.Values["SleeperVolumeTriggeredBy"] = stringBuilder10.ToString();
				int num2 = 0;
				for (int num3 = 0; num3 < SleeperVolumes.Count; num3++)
				{
					PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[num3];
					if (prefabSleeperVolume.used)
					{
						if (prefabSleeperVolume.minScript != null)
						{
							properties.Values["SVS" + num2] = prefabSleeperVolume.minScript;
						}
						num2++;
					}
				}
			}
		}
		if (flag)
		{
			properties.Values.Remove("SleeperVolumeSize");
			properties.Values.Remove("SleeperVolumeStart");
			properties.Values.Remove("SleeperVolumeGroupId");
			properties.Values.Remove("SleeperVolumeGroup");
			properties.Values.Remove("SleeperIsLootVolume");
			properties.Values.Remove("SleeperIsQuestExclude");
			properties.Values.Remove("SleeperVolumeFlags");
			properties.Values.Remove("SleeperVolumeTriggeredBy");
		}
		if (bInfoVolumes)
		{
			StringBuilder stringBuilder11 = new StringBuilder();
			StringBuilder stringBuilder12 = new StringBuilder();
			foreach (PrefabInfoVolume infoVolume in InfoVolumes)
			{
				if (stringBuilder11.Length > 0)
				{
					stringBuilder11.Append('#');
					stringBuilder12.Append('#');
				}
				stringBuilder11.Append(infoVolume.size.ToString());
				stringBuilder12.Append(infoVolume.startPos.ToString());
			}
			properties.Values["InfoVolumeSize"] = stringBuilder11.ToString();
			properties.Values["InfoVolumeStart"] = stringBuilder12.ToString();
		}
		else
		{
			properties.Values.Remove("InfoVolumeSize");
			properties.Values.Remove("InfoVolumeStart");
		}
		if (bWallVolumes)
		{
			StringBuilder stringBuilder13 = new StringBuilder();
			StringBuilder stringBuilder14 = new StringBuilder();
			foreach (PrefabWallVolume wallVolume in WallVolumes)
			{
				if (stringBuilder13.Length > 0)
				{
					stringBuilder13.Append('#');
					stringBuilder14.Append('#');
				}
				stringBuilder13.Append(wallVolume.size.ToString());
				stringBuilder14.Append(wallVolume.startPos.ToString());
			}
			properties.Values["WallVolumeSize"] = stringBuilder13.ToString();
			properties.Values["WallVolumeStart"] = stringBuilder14.ToString();
		}
		else
		{
			properties.Values.Remove("WallVolumeSize");
			properties.Values.Remove("WallVolumeStart");
		}
		if (bTriggerVolumes)
		{
			StringBuilder stringBuilder15 = new StringBuilder();
			StringBuilder stringBuilder16 = new StringBuilder();
			StringBuilder stringBuilder17 = new StringBuilder();
			foreach (PrefabTriggerVolume triggerVolume in TriggerVolumes)
			{
				if (stringBuilder15.Length > 0)
				{
					stringBuilder15.Append('#');
					stringBuilder16.Append('#');
					stringBuilder17.Append('#');
				}
				for (int num4 = 0; num4 < triggerVolume.TriggersIndices.Count; num4++)
				{
					if (num4 > 0)
					{
						stringBuilder17.Append(',');
					}
					stringBuilder17.Append(triggerVolume.TriggersIndices[num4].ToString());
				}
				if (triggerVolume.TriggersIndices.Count == 0)
				{
					stringBuilder17.Append(" ");
				}
				stringBuilder15.Append(triggerVolume.size.ToString());
				stringBuilder16.Append(triggerVolume.startPos.ToString());
			}
			properties.Values["TriggerVolumeSize"] = stringBuilder15.ToString();
			properties.Values["TriggerVolumeStart"] = stringBuilder16.ToString();
			properties.Values["TriggerVolumeTriggers"] = stringBuilder17.ToString();
		}
		else
		{
			properties.Values.Remove("TriggerVolumeSize");
			properties.Values.Remove("TriggerVolumeStart");
			properties.Values.Remove("TriggerVolumeTriggers");
		}
		if (bPOIMarkers)
		{
			StringBuilder stringBuilder18 = new StringBuilder();
			StringBuilder stringBuilder19 = new StringBuilder();
			StringBuilder stringBuilder20 = new StringBuilder();
			StringBuilder stringBuilder21 = new StringBuilder();
			StringBuilder stringBuilder22 = new StringBuilder();
			StringBuilder stringBuilder23 = new StringBuilder();
			StringBuilder stringBuilder24 = new StringBuilder();
			StringBuilder stringBuilder25 = new StringBuilder();
			foreach (Marker pOIMarker in POIMarkers)
			{
				if (stringBuilder19.Length > 0)
				{
					stringBuilder18.Append('#');
					stringBuilder19.Append('#');
					stringBuilder20.Append(',');
					stringBuilder21.Append('#');
					stringBuilder22.Append(',');
					stringBuilder23.Append(',');
					stringBuilder24.Append(',');
					stringBuilder25.Append(',');
				}
				stringBuilder18.Append(pOIMarker.Size.ToString());
				stringBuilder19.Append(pOIMarker.Start.ToString());
				stringBuilder20.Append(pOIMarker.GroupName);
				stringBuilder21.Append(pOIMarker.Tags.ToString());
				stringBuilder22.Append(pOIMarker.MarkerType.ToString());
				stringBuilder23.Append(pOIMarker.PartToSpawn);
				stringBuilder24.Append(pOIMarker.Rotations.ToString());
				stringBuilder25.Append(pOIMarker.PartChanceToSpawn.ToString());
			}
			properties.Values["POIMarkerSize"] = stringBuilder18.ToString();
			properties.Values["POIMarkerStart"] = stringBuilder19.ToString();
			properties.Values["POIMarkerGroup"] = stringBuilder20.ToString();
			properties.Values["POIMarkerTags"] = stringBuilder21.ToString();
			properties.Values["POIMarkerType"] = stringBuilder22.ToString();
			properties.Values["POIMarkerPartToSpawn"] = stringBuilder23.ToString();
			properties.Values["POIMarkerPartRotations"] = stringBuilder24.ToString();
			properties.Values["POIMarkerPartSpawnChance"] = stringBuilder25.ToString();
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
		if (StaticSpawnerClass != null)
		{
			properties.Values["StaticSpawner.Class"] = StaticSpawnerClass;
		}
		else
		{
			properties.Values.Remove("StaticSpawner.Class");
		}
		if (StaticSpawnerSize != Vector3i.zero)
		{
			properties.Values["StaticSpawner.Size"] = StaticSpawnerSize.x + "," + StaticSpawnerSize.y + "," + StaticSpawnerSize.z;
		}
		else
		{
			properties.Values.Remove("StaticSpawner.Size");
		}
		if (StaticSpawnerTrigger > 0)
		{
			properties.Values["StaticSpawner.Trigger"] = StaticSpawnerTrigger.ToString();
		}
		else
		{
			properties.Values.Remove("StaticSpawner.Trigger");
		}
		string text2 = "";
		for (int num5 = 0; num5 < allowedTownships.Count; num5++)
		{
			text2 = text2 + allowedTownships[num5] + ((num5 < allowedTownships.Count - 1) ? "," : "");
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
		for (int num6 = 0; num6 < allowedBiomes.Count; num6++)
		{
			text2 = text2 + allowedBiomes[num6] + ((num6 < allowedBiomes.Count - 1) ? "," : "");
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
					for (int num7 = 0; num7 < indexedBlockOffset.Value.Count; num7++)
					{
						dynamicProperties2.Values[num7.ToString()] = indexedBlockOffset.Value[num7].ToString();
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
			for (int num8 = 0; num8 < allowedZones.Count; num8++)
			{
				text3 = text3 + allowedZones[num8] + ((num8 < allowedZones.Count - 1) ? ", " : string.Empty);
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

	public bool Load(string _prefabName, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		return Load(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName), _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData);
	}

	public bool Load(PathAbstractions.AbstractedLocation _location, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
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
		if (_skipLoadingBlockData && !loadSizeDataOnly(_location, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData))
		{
			return false;
		}
		if (!loadBlockData(_location, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData))
		{
			return false;
		}
		return LoadXMLData(_location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadSizeDataOnly(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildblocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		using (Stream baseStream = SdFile.OpenRead(_location.FullPath))
		{
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
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadBlockData(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildblocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
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
			if (!readBlockData(pooledBinaryReader, num, arrayListMP?.Items, _fixChildblocks: true))
			{
				return false;
			}
			if (num > 12)
			{
				readTileEntities(pooledBinaryReader);
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

	public bool LoadXMLData(PathAbstractions.AbstractedLocation _location)
	{
		location = _location;
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return true;
		}
		if (!properties.Load(_location.Folder, _location.Name, _addClassesToMain: false))
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
		if (saveBlockData(_location, _createMapping))
		{
			return SaveXMLData(_location);
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
	public bool readBlockData(PooledBinaryReader _br, uint _version, int[] _blockIdMapping, bool _fixChildblocks)
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
						if (_fixChildblocks)
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
				if (!_fixChildblocks || (!blockValue.ischild && block2 != null && ((blockValue.meta & 1) == 0 || !(block2 is BlockModelTree))))
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
		if (_fixChildblocks)
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
	public void readTileEntities(PooledBinaryReader _br)
	{
		tileEntities.Clear();
		int num = _br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int length = _br.ReadInt16();
			TileEntityType type = (TileEntityType)_br.ReadByte();
			try
			{
				TileEntity tileEntity = TileEntity.Instantiate(type, null);
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, length);
					pooledExpandableMemoryStream.Position = 0L;
					using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
					pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
					tileEntity.read(pooledBinaryReader, TileEntity.StreamModeRead.Persistency);
				}
				Block block = GetBlock(tileEntity.localChunkPos.x, tileEntity.localChunkPos.y, tileEntity.localChunkPos.z).Block;
				if (block == null || block.IsTileEntitySavedInPrefab())
				{
					tileEntities.Add(tileEntity.localChunkPos, tileEntity);
				}
			}
			catch (Exception e)
			{
				Log.Error($"Skipping loading of active block data for {PrefabName} because of the following exception:");
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
				Log.Error($"Skipping loading of active block data for {PrefabName} because of the following exception:");
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
			if (bTraderArea)
			{
				for (int num6 = 0; num6 < TeleportVolumes.Count; num6++)
				{
					Vector3i vector3i = TeleportVolumes[num6].size;
					Vector3i startPos = TeleportVolumes[num6].startPos;
					Vector3i vector3i2 = startPos + vector3i;
					if (_bLeft)
					{
						startPos = new Vector3i(size.z - startPos.z, startPos.y, startPos.x);
						vector3i2 = new Vector3i(size.z - vector3i2.z, vector3i2.y, vector3i2.x);
					}
					else
					{
						startPos = new Vector3i(startPos.z, startPos.y, size.x - startPos.x);
						vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, size.x - vector3i2.x);
					}
					if (startPos.x > vector3i2.x)
					{
						MathUtils.Swap(ref startPos.x, ref vector3i2.x);
					}
					if (startPos.z > vector3i2.z)
					{
						MathUtils.Swap(ref startPos.z, ref vector3i2.z);
					}
					TeleportVolumes[num6].startPos = startPos;
					MathUtils.Swap(ref vector3i.x, ref vector3i.z);
					TeleportVolumes[num6].size = vector3i;
				}
			}
			if (bSleeperVolumes)
			{
				for (int num7 = 0; num7 < SleeperVolumes.Count; num7++)
				{
					Vector3i vector3i3 = SleeperVolumes[num7].size;
					Vector3i startPos2 = SleeperVolumes[num7].startPos;
					Vector3i vector3i4 = startPos2 + vector3i3;
					if (_bLeft)
					{
						startPos2 = new Vector3i(size.z - startPos2.z, startPos2.y, startPos2.x);
						vector3i4 = new Vector3i(size.z - vector3i4.z, vector3i4.y, vector3i4.x);
					}
					else
					{
						startPos2 = new Vector3i(startPos2.z, startPos2.y, size.x - startPos2.x);
						vector3i4 = new Vector3i(vector3i4.z, vector3i4.y, size.x - vector3i4.x);
					}
					if (startPos2.x > vector3i4.x)
					{
						MathUtils.Swap(ref startPos2.x, ref vector3i4.x);
					}
					if (startPos2.z > vector3i4.z)
					{
						MathUtils.Swap(ref startPos2.z, ref vector3i4.z);
					}
					SleeperVolumes[num7].startPos = startPos2;
					MathUtils.Swap(ref vector3i3.x, ref vector3i3.z);
					SleeperVolumes[num7].size = vector3i3;
				}
			}
			if (bInfoVolumes)
			{
				for (int num8 = 0; num8 < InfoVolumes.Count; num8++)
				{
					Vector3i vector3i5 = InfoVolumes[num8].size;
					Vector3i startPos3 = InfoVolumes[num8].startPos;
					Vector3i vector3i6 = startPos3 + vector3i5;
					if (_bLeft)
					{
						startPos3 = new Vector3i(size.z - startPos3.z, startPos3.y, startPos3.x);
						vector3i6 = new Vector3i(size.z - vector3i6.z, vector3i6.y, vector3i6.x);
					}
					else
					{
						startPos3 = new Vector3i(startPos3.z, startPos3.y, size.x - startPos3.x);
						vector3i6 = new Vector3i(vector3i6.z, vector3i6.y, size.x - vector3i6.x);
					}
					if (startPos3.x > vector3i6.x)
					{
						MathUtils.Swap(ref startPos3.x, ref vector3i6.x);
					}
					if (startPos3.z > vector3i6.z)
					{
						MathUtils.Swap(ref startPos3.z, ref vector3i6.z);
					}
					InfoVolumes[num8].startPos = startPos3;
					MathUtils.Swap(ref vector3i5.x, ref vector3i5.z);
					InfoVolumes[num8].size = vector3i5;
				}
			}
			if (bWallVolumes)
			{
				for (int num9 = 0; num9 < WallVolumes.Count; num9++)
				{
					Vector3i vector3i7 = WallVolumes[num9].size;
					Vector3i startPos4 = WallVolumes[num9].startPos;
					Vector3i vector3i8 = startPos4 + vector3i7;
					if (_bLeft)
					{
						startPos4 = new Vector3i(size.z - startPos4.z, startPos4.y, startPos4.x);
						vector3i8 = new Vector3i(size.z - vector3i8.z, vector3i8.y, vector3i8.x);
					}
					else
					{
						startPos4 = new Vector3i(startPos4.z, startPos4.y, size.x - startPos4.x);
						vector3i8 = new Vector3i(vector3i8.z, vector3i8.y, size.x - vector3i8.x);
					}
					if (startPos4.x > vector3i8.x)
					{
						MathUtils.Swap(ref startPos4.x, ref vector3i8.x);
					}
					if (startPos4.z > vector3i8.z)
					{
						MathUtils.Swap(ref startPos4.z, ref vector3i8.z);
					}
					WallVolumes[num9].startPos = startPos4;
					MathUtils.Swap(ref vector3i7.x, ref vector3i7.z);
					WallVolumes[num9].size = vector3i7;
				}
			}
			if (bTriggerVolumes)
			{
				for (int num10 = 0; num10 < TriggerVolumes.Count; num10++)
				{
					Vector3i vector3i9 = TriggerVolumes[num10].size;
					Vector3i startPos5 = TriggerVolumes[num10].startPos;
					Vector3i vector3i10 = startPos5 + vector3i9;
					if (_bLeft)
					{
						startPos5 = new Vector3i(size.z - startPos5.z, startPos5.y, startPos5.x);
						vector3i10 = new Vector3i(size.z - vector3i10.z, vector3i10.y, vector3i10.x);
					}
					else
					{
						startPos5 = new Vector3i(startPos5.z, startPos5.y, size.x - startPos5.x);
						vector3i10 = new Vector3i(vector3i10.z, vector3i10.y, size.x - vector3i10.x);
					}
					if (startPos5.x > vector3i10.x)
					{
						MathUtils.Swap(ref startPos5.x, ref vector3i10.x);
					}
					if (startPos5.z > vector3i10.z)
					{
						MathUtils.Swap(ref startPos5.z, ref vector3i10.z);
					}
					TriggerVolumes[num10].startPos = startPos5;
					MathUtils.Swap(ref vector3i9.x, ref vector3i9.z);
					TriggerVolumes[num10].size = vector3i9;
				}
			}
			for (int num11 = 0; num11 < POIMarkers.Count; num11++)
			{
				Vector3i vector3i11 = POIMarkers[num11].Size;
				Vector3i start = POIMarkers[num11].Start;
				Vector3i vector3i12 = start + vector3i11;
				if (_bLeft)
				{
					start = new Vector3i(size.z - start.z, start.y, start.x);
					vector3i12 = new Vector3i(size.z - vector3i12.z, vector3i12.y, vector3i12.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, size.x - start.x);
					vector3i12 = new Vector3i(vector3i12.z, vector3i12.y, size.x - vector3i12.x);
				}
				if (start.x > vector3i12.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i12.x);
				}
				if (start.z > vector3i12.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i12.z);
				}
				POIMarkers[num11].Start = start;
				MathUtils.Swap(ref vector3i11.x, ref vector3i11.z);
				POIMarkers[num11].Size = vector3i11;
			}
			MathUtils.Swap(ref size.x, ref size.z);
		}
		if (Block.BlocksLoaded)
		{
			AddAllChildBlocks();
		}
	}

	public void RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i vector3i = size;
		for (int i = 0; i < _rotCount; i++)
		{
			for (int j = 0; j < POIMarkers.Count; j++)
			{
				Vector3i vector3i2 = POIMarkers[j].Size;
				Vector3i start = POIMarkers[j].Start;
				Vector3i vector3i3 = start + vector3i2;
				if (_bLeft)
				{
					start = new Vector3i(vector3i.z - start.z, start.y, start.x);
					vector3i3 = new Vector3i(vector3i.z - vector3i3.z, vector3i3.y, vector3i3.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, vector3i.x - start.x);
					vector3i3 = new Vector3i(vector3i3.z, vector3i3.y, vector3i.x - vector3i3.x);
				}
				if (start.x > vector3i3.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i3.x);
				}
				if (start.z > vector3i3.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i3.z);
				}
				POIMarkers[j].Start = start;
				MathUtils.Swap(ref vector3i2.x, ref vector3i2.z);
				POIMarkers[j].Size = vector3i2;
			}
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
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
	public bool hasTexture(TextureFullArray fulltexture, int textureIdx)
	{
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				if (((fulltexture[i] >> j * 8) & 0xFF) == textureIdx)
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
							list.Add(new BlockChangeInfo(0, new Vector3i(num, num2, num3), BlockValue.Air));
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
					list.Add(new BlockChangeInfo(0, new Vector3i(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z), block, density, texture));
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
			BlockTrigger blockTrigger2 = _gm.World.GetBlockTrigger(0, vector3i4);
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

	public void CountSleeperSpawnsInVolume(World _world, Vector3i _offset, int index)
	{
		Transient_NumSleeperSpawns = 0;
		PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[index];
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
					if (!_world.GetBlock(k, j - 1, i).Block.IsSleeperBlock && _world.GetBlock(k, j, i).Block.IsSleeperBlock)
					{
						Vector3i pos = new Vector3i(k - _offset.x, j - _offset.y, i - _offset.z);
						if (!IsPosInSleeperPriorityVolume(pos, index))
						{
							Transient_NumSleeperSpawns++;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySleeperBlocksContainedInVolume(int volumeIndex, Vector3i _offset, SleeperVolume _volume, Vector3i _volumeMins, Vector3i _volumeMaxs)
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
						if (!IsPosInSleeperPriorityVolume(pos, volumeIndex))
						{
							_volume.AddSpawnPoint(x, y, z, (BlockSleeper)block2, block);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySleeperVolumes(World _world, Chunk _chunk, Vector3i _offset)
	{
		Vector3i vector3i = Vector3i.zero;
		Vector3i vector3i2 = Vector3i.zero;
		if (_chunk != null)
		{
			vector3i = _chunk.GetWorldPos();
			vector3i2 = vector3i + new Vector3i(16, 256, 16);
		}
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[i];
			if (!prefabSleeperVolume.used)
			{
				continue;
			}
			Vector3i startPos = prefabSleeperVolume.startPos;
			Vector3i volumeMaxs = startPos + prefabSleeperVolume.size;
			Vector3i vector3i3 = startPos + _offset;
			Vector3i vector3i4 = vector3i3 + prefabSleeperVolume.size;
			Vector3i vector3i5 = vector3i3 - SleeperVolume.chunkPadding;
			Vector3i vector3i6 = vector3i4 + SleeperVolume.chunkPadding;
			if (_chunk != null)
			{
				if (vector3i5.x < vector3i2.x && vector3i6.x > vector3i.x && vector3i5.y < vector3i2.y && vector3i6.y > vector3i.y && vector3i5.z < vector3i2.z && vector3i6.z > vector3i.z)
				{
					int num = _world.FindSleeperVolume(vector3i3, vector3i4);
					if (num < 0)
					{
						SleeperVolume volume = SleeperVolume.Create(prefabSleeperVolume, vector3i3, vector3i4);
						num = _world.AddSleeperVolume(volume);
						CopySleeperBlocksContainedInVolume(i, _offset, volume, startPos, volumeMaxs);
					}
					_chunk.AddSleeperVolumeId(num);
				}
				continue;
			}
			int num2 = _world.FindSleeperVolume(vector3i3, vector3i4);
			if (num2 < 0)
			{
				SleeperVolume volume2 = SleeperVolume.Create(prefabSleeperVolume, vector3i3, vector3i4);
				num2 = _world.AddSleeperVolume(volume2);
				CopySleeperBlocksContainedInVolume(i, _offset, volume2, startPos, volumeMaxs);
			}
			int num3 = World.toChunkXZ(vector3i5.x);
			int num4 = World.toChunkXZ(vector3i6.x - 1);
			int num5 = World.toChunkXZ(vector3i5.z);
			int num6 = World.toChunkXZ(vector3i6.z - 1);
			for (int j = num3; j <= num4; j++)
			{
				for (int k = num5; k <= num6; k++)
				{
					((Chunk)_world.GetChunkSync(j, k))?.AddSleeperVolumeId(num2);
				}
			}
		}
		for (int l = 0; l < TriggerVolumes.Count; l++)
		{
			PrefabTriggerVolume prefabTriggerVolume = TriggerVolumes[l];
			Vector3i startPos2 = prefabTriggerVolume.startPos;
			_ = startPos2 + prefabTriggerVolume.size;
			Vector3i vector3i7 = startPos2 + _offset;
			Vector3i vector3i8 = vector3i7 + prefabTriggerVolume.size;
			Vector3i vector3i9 = vector3i7 - SleeperVolume.chunkPadding;
			Vector3i vector3i10 = vector3i8 + SleeperVolume.chunkPadding;
			if (_chunk != null)
			{
				if (vector3i9.x < vector3i2.x && vector3i10.x > vector3i.x && vector3i9.y < vector3i2.y && vector3i10.y > vector3i.y && vector3i9.z < vector3i2.z && vector3i10.z > vector3i.z)
				{
					int num7 = _world.FindTriggerVolume(vector3i7, vector3i8);
					if (num7 < 0)
					{
						TriggerVolume volume3 = TriggerVolume.Create(prefabTriggerVolume, vector3i7, vector3i8);
						num7 = _world.AddTriggerVolume(volume3);
					}
					_chunk.AddTriggerVolumeId(num7);
				}
				continue;
			}
			int num8 = _world.FindTriggerVolume(vector3i7, vector3i8);
			if (num8 < 0)
			{
				TriggerVolume volume4 = TriggerVolume.Create(prefabTriggerVolume, vector3i7, vector3i8);
				num8 = _world.AddTriggerVolume(volume4);
			}
			int num9 = World.toChunkXZ(vector3i9.x);
			int num10 = World.toChunkXZ(vector3i10.x - 1);
			int num11 = World.toChunkXZ(vector3i9.z);
			int num12 = World.toChunkXZ(vector3i10.z - 1);
			for (int m = num9; m <= num10; m++)
			{
				for (int n = num11; n <= num12; n++)
				{
					((Chunk)_world.GetChunkSync(m, n))?.AddTriggerVolumeId(num8);
				}
			}
		}
		for (int num13 = 0; num13 < WallVolumes.Count; num13++)
		{
			PrefabWallVolume prefabWallVolume = WallVolumes[num13];
			Vector3i startPos3 = prefabWallVolume.startPos;
			_ = startPos3 + prefabWallVolume.size;
			Vector3i vector3i11 = startPos3 + _offset;
			Vector3i vector3i12 = vector3i11 + prefabWallVolume.size;
			Vector3i vector3i13 = vector3i11;
			Vector3i vector3i14 = vector3i12;
			if (_chunk != null)
			{
				if (vector3i13.x < vector3i2.x && vector3i14.x > vector3i.x && vector3i13.y < vector3i2.y && vector3i14.y > vector3i.y && vector3i13.z < vector3i2.z && vector3i14.z > vector3i.z)
				{
					int num14 = _world.FindWallVolume(vector3i11, vector3i12);
					if (num14 < 0)
					{
						WallVolume volume5 = WallVolume.Create(prefabWallVolume, vector3i11, vector3i12);
						num14 = _world.AddWallVolume(volume5);
					}
					_chunk.AddWallVolumeId(num14);
				}
				continue;
			}
			int num15 = _world.FindWallVolume(vector3i11, vector3i12);
			if (num15 < 0)
			{
				WallVolume volume6 = WallVolume.Create(prefabWallVolume, vector3i11, vector3i12);
				num15 = _world.AddWallVolume(volume6);
			}
			int num16 = World.toChunkXZ(vector3i13.x);
			int num17 = World.toChunkXZ(vector3i14.x - 1);
			int num18 = World.toChunkXZ(vector3i13.z);
			int num19 = World.toChunkXZ(vector3i14.z - 1);
			for (int num20 = num16; num20 <= num17; num20++)
			{
				for (int num21 = num18; num21 <= num19; num21++)
				{
					((Chunk)_world.GetChunkSync(num20, num21))?.AddWallVolumeId(num15);
				}
			}
		}
	}

	public PrefabSleeperVolume FindSleeperVolume(Vector3i _pos)
	{
		PrefabSleeperVolume result = null;
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[i];
			if (prefabSleeperVolume.used && IsPosInSleeperVolume(prefabSleeperVolume, _pos))
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
	public bool IsPosInSleeperPriorityVolume(Vector3i _pos, int skipIndex)
	{
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			if (i != skipIndex)
			{
				PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[i];
				if (prefabSleeperVolume.used && prefabSleeperVolume.isPriority && IsPosInSleeperVolume(prefabSleeperVolume, _pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperVolume(PrefabSleeperVolume volume, Vector3i _pos)
	{
		if (volume.used)
		{
			Vector3i startPos = volume.startPos;
			Vector3i vector3i = startPos + volume.size;
			if (_pos.x >= startPos.x && _pos.x < vector3i.x && _pos.y >= startPos.y && _pos.y < vector3i.y && _pos.z >= startPos.z && _pos.z < vector3i.z)
			{
				return true;
			}
		}
		return false;
	}

	public void MoveVolumes(Vector3i moveDistance)
	{
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			SleeperVolumes[i].startPos += moveDistance;
		}
		for (int j = 0; j < TeleportVolumes.Count; j++)
		{
			TeleportVolumes[j].startPos += moveDistance;
		}
		for (int k = 0; k < TriggerVolumes.Count; k++)
		{
			TriggerVolumes[k].startPos += moveDistance;
		}
		for (int l = 0; l < InfoVolumes.Count; l++)
		{
			InfoVolumes[l].startPos += moveDistance;
		}
		for (int m = 0; m < WallVolumes.Count; m++)
		{
			WallVolumes[m].startPos += moveDistance;
		}
	}

	public static void TransientSleeperBlockIncrement(Vector3i point, int c)
	{
		if (XUiC_WoPropsSleeperVolume.selectedVolumeIndex >= 0)
		{
			PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
			Prefab prefab = selectedPrefabInstance.prefab;
			if (XUiC_WoPropsSleeperVolume.selectedVolumeIndex < prefab.SleeperVolumes.Count && prefab.IsPosInSleeperVolume(prefab.SleeperVolumes[XUiC_WoPropsSleeperVolume.selectedVolumeIndex], point - selectedPrefabInstance.boundingBoxPosition))
			{
				prefab.Transient_NumSleeperSpawns += c;
			}
		}
	}

	public string CalcSleeperInfo()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[i];
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
		string text = $"{SleeperVolumes.Count}, {num}-{num2}";
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
			CopySleeperVolumes(world, null, _destinationPos);
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
						if (chunkSync == null)
						{
							UnityEngine.Debug.LogError($"Chunk ({num6}, {num4}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({num5},{num3}).");
							continue;
						}
					}
					for (int n = 0; n < size.y; n++)
					{
						int y2 = World.toBlockY(n + _destinationPos.y);
						BlockValue block2 = chunkSync.GetBlock(x2, y2, z2);
						if (block2.Block.isMultiBlock && !block2.ischild)
						{
							chunkSync.SetBlock(world, x2, y2, z2, BlockValue.Air);
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
		for (int num7 = 0; num7 < size.z; num7++)
		{
			int num8 = num7 + _destinationPos.z;
			int num9 = World.toChunkXZ(num8);
			int num10 = World.toBlockXZ(num8);
			for (int num11 = 0; num11 < size.x; num11++)
			{
				int num12 = num11 + _destinationPos.x;
				int num13 = World.toChunkXZ(num12);
				int num14 = World.toBlockXZ(num12);
				if (chunkSync == null || chunkSync.X != num13 || chunkSync.Z != num9)
				{
					chunkSync = _cluster.GetChunkSync(num13, num9);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
					gameRandom = null;
					if (chunkSync == null)
					{
						UnityEngine.Debug.LogError($"Chunk ({num13}, {num9}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({num12},{num8}).");
						continue;
					}
				}
				if (gameRandom == null)
				{
					gameRandom = Utils.RandomFromSeedOnPos(num13, num9, seed);
				}
				int num15 = -1;
				bool flag3 = false;
				for (int num16 = 0; num16 < size.y; num16++)
				{
					WaterValue water = GetWater(num11, num16, num7);
					BlockValue targetBV = GetBlock(num11, num16, num7);
					if (!bCopyAirBlocks && targetBV.isair && !water.HasMass())
					{
						continue;
					}
					int num17 = World.toBlockY(num16 + _destinationPos.y);
					BlockValue block3 = chunkSync.GetBlock(num14, num17, num10);
					BlockValue blockValue = targetBV;
					sbyte b = GetDensity(num11, num16, num7);
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
							BlockValue blockValue2 = block3;
							Block block4 = blockValue2.Block;
							if (blockValue2.isair || block4 == null || !block4.shape.IsTerrain())
							{
								int terrainHeight = chunkSync.GetTerrainHeight(num14, num10);
								blockValue2 = chunkSync.GetBlock(num14, terrainHeight, num10);
								block4 = blockValue2.Block;
								if (blockValue2.isair || block4 == null || !block4.shape.IsTerrain())
								{
									continue;
								}
							}
							targetBV = blockValue2;
							flag3 = true;
						}
						if (targetBV.type == terrainFiller2Type)
						{
							Block block5 = block3.Block;
							if (!block3.isair && block5 != null && block5.shape.IsTerrain())
							{
								targetBV = block3;
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
							ProcessMultiBlock(ref targetBV, chunkSync, new Vector3i(num11, num16, num7), new Vector3i(num14, num17, num10), _questTags, _bOverwriteExistingBlocks);
						}
						else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(targetBV))
						{
							byte meta = targetBV.meta;
							targetBV = BlockPlaceholderMap.Instance.Replace(targetBV, GameManager.Instance.World.GetGameRandom(), chunkSync, num14, num17, num10, _questTags, _bOverwriteExistingBlocks);
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
					if (block3.ischild || (!_bOverwriteExistingBlocks && !block3.isair && !block3.Block.shape.IsTerrain()))
					{
						chunkSync.SetDensity(num14, num17, num10, b);
						continue;
					}
					chunkSync.SetDecoAllowedSizeAt(num14, num10, EnumDecoAllowedSize.NoBigOnlySmall);
					if (!flag4)
					{
						TextureFullArray texture = GetTexture(num11, num16, num7);
						chunkSync.GetSetTextureFullArray(num14, num17, num10, texture);
					}
					chunkSync.SetBlock(world, num14, num17, num10, targetBV, _notifyAddChange: true, _notifyRemove: true, !_questTags.IsEmpty, _poiOwned: true);
					chunkSync.SetWater(num14, num17, num10, water);
					Vector3i blockPos = new Vector3i(num11, num16, num7);
					TileEntity tileEntity;
					if (blockValue.Block.IsTileEntitySavedInPrefab() && (tileEntity = GetTileEntity(blockPos)) != null)
					{
						Vector3i vector3i = new Vector3i(num14, num17, num10);
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
						BlockTrigger blockTrigger2 = chunkSync.GetBlockTrigger(new Vector3i(num14, num17, num10));
						if (blockTrigger2 == null)
						{
							blockTrigger2 = blockTrigger.Clone();
							blockTrigger2.LocalChunkPos = new Vector3i(num14, num17, num10);
							blockTrigger2.Chunk = chunkSync;
							chunkSync.AddBlockTrigger(blockTrigger2);
						}
						blockTrigger2.CopyFrom(blockTrigger);
						blockTrigger2.LocalChunkPos = new Vector3i(num14, num17, num10);
						targetBV.Block.OnTriggerAddedFromPrefab(blockTrigger2, blockTrigger2.LocalChunkPos, targetBV, FastTags<TagGroup.Global>.Parse(questTags.ToString()));
					}
					if (targetBV.Block.shape.IsTerrain())
					{
						num15 = num17;
					}
					chunkSync.SetDensity(num14, num17, num10, b);
				}
				if (num15 >= 0)
				{
					chunkSync.SetTerrainHeight(num14, num10, (byte)num15);
				}
				if (!flag3)
				{
					chunkSync.SetTopSoilBroken(num14, num10);
				}
				chunkSync.SetDecoAllowedSizeAt(num14, num10, EnumDecoAllowedSize.NoBigOnlySmall);
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
			if (!EntityClass.list.ContainsKey(entityCreationData.entityClass) || (!_bSpawnEnemies && EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity))
			{
				continue;
			}
			int v = Utils.Fastfloor(entityCreationData.pos.x) + _destinationPos.x;
			int v2 = Utils.Fastfloor(entityCreationData.pos.z) + _destinationPos.z;
			if (_chunk.X == World.toChunkXZ(v) && _chunk.Z == World.toChunkXZ(v2))
			{
				EntityCreationData entityCreationData2 = entityCreationData.Clone();
				entityCreationData2.pos += _destinationPos.ToVector3() + new Vector3(0f, 0.25f, 0f);
				entityCreationData2.id = EntityFactory.nextEntityID++;
				if (entityCreationData2.lootContainer != null)
				{
					entityCreationData2.lootContainer.entityId = entityCreationData2.id;
				}
				_chunk.AddEntityStub(entityCreationData2);
				_entityIds?.Add(entityCreationData2.id);
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
					SetDensity(num3, num, num5, _world.GetDensity(0, num4, num2, num6));
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

	public BlockValue Get(int relx, int absy, int relz)
	{
		int num = currX + relx;
		int num2 = currZ + relz;
		if (num >= 0 && num < size.x && absy >= 0 && absy < size.y && num2 >= 0 && num2 < size.z)
		{
			return GetBlock(num, absy, num2);
		}
		return BlockValue.Air;
	}

	public IChunk GetChunk(int x, int z)
	{
		long key = WorldChunkCache.MakeChunkKey(x, z);
		if (!dictChunks.TryGetValue(key, out var value))
		{
			value = new PrefabChunk(this, x, z);
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

	public IChunk GetNeighborChunk(int x, int z)
	{
		return GetChunk(x, z);
	}

	public bool IsWater(int relx, int absy, int relz)
	{
		int num = currX + relx;
		int num2 = currZ + relz;
		if (num >= 0 && num < size.x && absy >= 0 && absy < size.y && num2 >= 0 && num2 < size.z)
		{
			return GetWater(num, absy, num2).HasMass();
		}
		return false;
	}

	public bool IsAir(int relx, int absy, int relz)
	{
		int num = currX + relx;
		int num2 = currZ + relz;
		if (num >= 0 && num < size.x && absy >= 0 && absy < size.y && num2 >= 0 && num2 < size.z)
		{
			if (GetBlock(num, absy, num2).isair)
			{
				return !GetWater(num, absy, num2).HasMass();
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

	public void ToOptimizedColorCubeMesh(VoxelMesh _mesh)
	{
		new MeshGeneratorOptimizedMesh(this).GenerateColorCubeMesh(Vector3i.zero, size, _mesh);
	}

	public Transform ToTransform()
	{
		MeshFilter[][] array = new MeshFilter[MeshDescription.meshes.Length][];
		MeshRenderer[][] array2 = new MeshRenderer[MeshDescription.meshes.Length][];
		MeshCollider[][] array3 = new MeshCollider[MeshDescription.meshes.Length][];
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

	public void CopyBlocksIntoChunkNoEntities(World _world, Chunk _chunk, Vector3i _prefabTargetPos, bool _bForceOverwriteBlocks)
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
		int num3 = 16;
		int num4 = _prefabTargetPos.x - (int)aABB.min.x;
		if (num4 >= 0)
		{
			num = num4;
			num3 = Utils.FastMin(16 - num4, size.x);
		}
		else
		{
			num2 = -1 * num4;
			num3 = Utils.FastMin(size.x + num4, 16);
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = 16;
		int num8 = _prefabTargetPos.z - (int)aABB.min.z;
		if (num8 >= 0)
		{
			num5 = num8;
			num7 = Utils.FastMin(16 - num8, size.z);
		}
		else
		{
			num6 = -1 * num8;
			num7 = Utils.FastMin(size.z + num8, 16);
		}
		for (int i = 0; i < num7; i++)
		{
			int num9 = i + num5;
			for (int j = 0; j < num3; j++)
			{
				int num10 = j + num;
				int terrainHeight = _chunk.GetTerrainHeight(num10, num9);
				BlockValue block = _chunk.GetBlock(num10, terrainHeight, num9);
				BlockValue blockValue = block;
				int num11 = terrainHeight;
				bool flag3 = false;
				for (int k = 0; k < size.y; k++)
				{
					BlockValue targetBV = GetBlock(j + num2, k, i + num6);
					WaterValue water = GetWater(j + num2, k, i + num6);
					Block block2 = targetBV.Block;
					bool flag4 = false;
					if (block2.IsSleeperBlock)
					{
						flag4 = true;
						targetBV = BlockValue.Air;
					}
					int num12 = k + _prefabTargetPos.y;
					if (num12 < 0 || num12 >= 255)
					{
						continue;
					}
					_ = bAllowTopSoilDecorations;
					if (targetBV.type == terrainFillerType)
					{
						if (!flag2)
						{
							BlockValue blockValue2 = _chunk.GetBlock(num10, num12, num9);
							block2 = blockValue2.Block;
							if (blockValue2.isair || block2 == null || !block2.shape.IsTerrain())
							{
								blockValue2 = ((num12 >= terrainHeight) ? block : blockValue);
								block2 = blockValue2.Block;
								if (blockValue2.isair || block2 == null || !block2.shape.IsTerrain())
								{
									continue;
								}
							}
							targetBV = blockValue2;
						}
						if (block2.multiBlockPos != null && block2.multiBlockPos.dim.x != 1 && block2.multiBlockPos.dim.y != 1)
						{
							_ = block2.multiBlockPos.dim.z;
							_ = 1;
						}
					}
					sbyte b = GetDensity(j + num2, k, i + num6);
					if (targetBV.type == terrainFiller2Type)
					{
						BlockValue block3 = _chunk.GetBlock(num10, num12, num9);
						Block block4 = block3.Block;
						if (!block3.isair && block4 != null && block4.shape.IsTerrain())
						{
							targetBV = block3;
							b = _chunk.GetDensity(num10, num12, num9);
						}
						else
						{
							targetBV = BlockValue.Air;
							b = MarchingCubes.DensityAir;
							if (num12 > 0 && _chunk.GetBlock(num10, num12 - 1, num9).Block.shape.IsTerrain())
							{
								sbyte density = _chunk.GetDensity(num10, num12 - 1, num9);
								b = (sbyte)(MarchingCubes.DensityAir + density);
							}
						}
						block2 = targetBV.Block;
					}
					if (!flag2)
					{
						if (targetBV.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
						{
							ProcessMultiBlock(ref targetBV, _chunk, new Vector3i(j + num2, k, i + num6), new Vector3i(num10, num12, num9), FastTags<TagGroup.Global>.none, _bForceOverwriteBlocks);
							block2 = targetBV.Block;
						}
						else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(targetBV))
						{
							targetBV = BlockPlaceholderMap.Instance.Replace(targetBV, gameRandom, _chunk, num10, num12, num9, FastTags<TagGroup.Global>.none, _bForceOverwriteBlocks);
							block2 = targetBV.Block;
						}
					}
					bool flag5 = block2.shape.IsTerrain();
					if (flag5)
					{
						blockValue = targetBV;
						if (num12 > num11)
						{
							num11 = num12;
							flag3 = true;
						}
					}
					else if (num12 <= num11)
					{
						num11 = num12 - 1;
						flag3 = true;
					}
					if (b == 0)
					{
						b = (sbyte)(flag5 ? MarchingCubes.DensityTerrain : ((block2.shape.IsSolidCube && num12 <= num11) ? 1 : MarchingCubes.DensityAir));
					}
					if (yOffset == 0)
					{
						sbyte density2 = _chunk.GetDensity(num10, num12, num9);
						if ((b >= 0 && density2 >= 0 && (density2 != MarchingCubes.DensityAir / 2 || (block2.IsTerrainDecoration && !bCopyAirBlocks))) || (b < 0 && density2 < 0 && density2 != MarchingCubes.DensityTerrain / 2))
						{
							b = density2;
						}
					}
					_chunk.SetDecoAllowedSizeAt(num10, num9, EnumDecoAllowedSize.NoBigOnlySmall);
					Vector3i blockPos = new Vector3i(j + num2, k, i + num6);
					if (flag && !block2.shape.IsTerrain() && IsInsidePrefab(blockPos.x, blockPos.y, blockPos.z))
					{
						_chunk.AddInsideDevicePosition(num10, num12, num9, targetBV);
					}
					if (!bCopyAirBlocks && targetBV.isair && k >= -yOffset && !water.HasMass())
					{
						continue;
					}
					BlockValue block5 = _chunk.GetBlock(num10, num12, num9);
					if (!_bForceOverwriteBlocks && !block5.Block.shape.IsTerrain() && !block5.isair && (block5.ischild || block5.type == targetBV.type))
					{
						_chunk.SetDensity(num10, num12, num9, b);
						continue;
					}
					if (!flag4)
					{
						TextureFullArray texture = GetTexture(j + num2, k, i + num6);
						_chunk.GetSetTextureFullArray(num10, num12, num9, texture);
					}
					_chunk.SetBlock(_world, num10, num12, num9, targetBV, _notifyAddChange: true, _notifyRemove: true, _fromReset: false, _poiOwned: true);
					_chunk.SetWater(num10, num12, num9, water);
					_chunk.SetDensity(num10, num12, num9, b);
					TileEntity tileEntity;
					if (targetBV.Block.IsTileEntitySavedInPrefab() && (tileEntity = GetTileEntity(blockPos)) != null)
					{
						TileEntity tileEntity2 = _chunk.GetTileEntity(new Vector3i(num10, num12, num9));
						if (tileEntity2 == null)
						{
							tileEntity2 = tileEntity.Clone();
							tileEntity2.localChunkPos = new Vector3i(num10, num12, num9);
							tileEntity2.SetChunk(_chunk);
							_chunk.AddTileEntity(tileEntity2);
						}
						tileEntity2.CopyFrom(tileEntity);
						tileEntity2.localChunkPos = new Vector3i(num10, num12, num9);
					}
					BlockTrigger blockTrigger;
					if ((blockTrigger = GetBlockTrigger(blockPos)) != null)
					{
						BlockTrigger blockTrigger2 = _chunk.GetBlockTrigger(new Vector3i(num10, num12, num9));
						if (blockTrigger2 == null)
						{
							blockTrigger2 = blockTrigger.Clone();
							blockTrigger2.LocalChunkPos = new Vector3i(num10, num12, num9);
							blockTrigger2.Chunk = _chunk;
							_chunk.AddBlockTrigger(blockTrigger2);
						}
						blockTrigger2.CopyFrom(blockTrigger);
						blockTrigger2.LocalChunkPos = new Vector3i(num10, num12, num9);
					}
				}
				if (flag3 && (num11 > terrainHeight || _prefabTargetPos.y + size.y >= terrainHeight))
				{
					_chunk.SetTerrainHeight(num10, num9, (byte)num11);
				}
				_chunk.SetTopSoilBroken(num10, num9);
			}
		}
		CopySleeperVolumes(_world, _chunk, _prefabTargetPos);
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
	public void updateBlockStatistics(BlockValue bv, Block b)
	{
		if (Block.BlocksLoaded && b != null)
		{
			statistics.cntWindows += ((b.BlockTag == BlockTags.Window) ? 1 : 0);
			statistics.cntDoors += ((b.BlockTag == BlockTags.Door) ? 1 : 0);
			statistics.cntBlockEntities += ((b.shape is BlockShapeModelEntity && !bv.ischild && (!(b is BlockModelTree) || bv.meta == 0)) ? 1 : 0);
			statistics.cntBlockModels += ((b.shape is BlockShapeExt3dModel && !bv.ischild) ? 1 : 0);
			statistics.cntSolid += ((!bv.isair) ? 1 : 0);
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

	public void CloneSleeperVolume(string name, Vector3i boundingBoxPosition, int idx)
	{
		PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[idx];
		AddSleeperVolume(name, boundingBoxPosition, prefabSleeperVolume.startPos + new Vector3i(0, prefabSleeperVolume.size.y + 1, 0), prefabSleeperVolume.size, prefabSleeperVolume.groupId, prefabSleeperVolume.groupName, prefabSleeperVolume.spawnCountMin, prefabSleeperVolume.spawnCountMax);
	}

	public int AddSleeperVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size, short groupId, string _groupName, int _spawnMin, int _spawnMax)
	{
		int result = -1;
		PrefabSleeperVolume prefabSleeperVolume = null;
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			if (!SleeperVolumes[i].used)
			{
				result = i;
				prefabSleeperVolume = SleeperVolumes[i];
				break;
			}
		}
		if (prefabSleeperVolume == null)
		{
			prefabSleeperVolume = new PrefabSleeperVolume();
			result = SleeperVolumes.Count;
			SleeperVolumes.Add(prefabSleeperVolume);
		}
		prefabSleeperVolume.Use(startPos, size, groupId, _groupName, _isPriority: false, _isQuestExclude: false, _spawnMin, _spawnMax, 0);
		string name = _prefabInstanceName + "_" + result;
		AddSleeperVolumeSelectionBox(prefabSleeperVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("SleeperVolume", name, _bActive: true);
		return result;
	}

	public void SetSleeperVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, PrefabSleeperVolume _volumeSettings)
	{
		while (_index >= SleeperVolumes.Count)
		{
			SleeperVolumes.Add(new PrefabSleeperVolume());
		}
		bool used = SleeperVolumes[_index].used;
		SleeperVolumes[_index] = _volumeSettings;
		string name = _prefabInstanceName + "_" + _index;
		if (_volumeSettings.used)
		{
			if (!used)
			{
				AddSleeperVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
				SelectionBoxManager.Instance.SetActive("SleeperVolume", name, _bActive: true);
			}
			else
			{
				SelectionBoxManager.Instance.GetCategory("SleeperVolume").GetBox(name).SetPositionAndSize(_prefabInstanceBoundingBox + _volumeSettings.startPos, _volumeSettings.size);
				SelectionBoxManager.Instance.SetUserData("SleeperVolume", name, _volumeSettings);
			}
		}
		else if (used)
		{
			SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(name);
		}
	}

	public void AddSleeperVolumeSelectionBox(PrefabSleeperVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").AddBox(_name, _pos, _volume.size).UserData = _volume;
	}

	public short FindSleeperVolumeFreeGroupId()
	{
		int num = 0;
		for (int i = 0; i < SleeperVolumes.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = SleeperVolumes[i];
			if (prefabSleeperVolume.groupId > num)
			{
				num = prefabSleeperVolume.groupId;
			}
		}
		return (short)(num + 1);
	}

	public int AddTeleportVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		PrefabTeleportVolume prefabTeleportVolume = new PrefabTeleportVolume();
		int count = TeleportVolumes.Count;
		TeleportVolumes.Add(prefabTeleportVolume);
		prefabTeleportVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count;
		AddTeleportVolumeSelectionBox(prefabTeleportVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("TraderTeleport", name, _bActive: true);
		return count;
	}

	public void SetTeleportVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		while (_index >= TeleportVolumes.Count)
		{
			TeleportVolumes.Add(new PrefabTeleportVolume());
		}
		if (!remove)
		{
			TeleportVolumes[_index] = _volumeSettings;
		}
		else
		{
			TeleportVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index;
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").RemoveBox(name);
		if (!remove)
		{
			AddTeleportVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("TraderTeleport", name, _bActive: true);
		}
	}

	public void AddTeleportVolumeSelectionBox(PrefabTeleportVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").AddBox(_name, _pos, _volume.size).UserData = _volume;
	}

	public int AddInfoVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		PrefabInfoVolume prefabInfoVolume = new PrefabInfoVolume();
		int count = InfoVolumes.Count;
		InfoVolumes.Add(prefabInfoVolume);
		prefabInfoVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count;
		AddInfoVolumeSelectionBox(prefabInfoVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("InfoVolume", name, _bActive: true);
		return count;
	}

	public void SetInfoVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		while (_index >= InfoVolumes.Count)
		{
			InfoVolumes.Add(new PrefabInfoVolume());
		}
		if (!remove)
		{
			InfoVolumes[_index] = _volumeSettings;
		}
		else
		{
			InfoVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index;
		SelectionBoxManager.Instance.GetCategory("InfoVolume").RemoveBox(name);
		if (!remove)
		{
			AddInfoVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("InfoVolume", name, _bActive: true);
		}
	}

	public void AddInfoVolumeSelectionBox(PrefabInfoVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("InfoVolume").AddBox(_name, _pos, _volume.size).UserData = _volume;
	}

	public int AddWallVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		PrefabWallVolume prefabWallVolume = new PrefabWallVolume();
		int count = WallVolumes.Count;
		WallVolumes.Add(prefabWallVolume);
		prefabWallVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count;
		AddWallVolumeSelectionBox(prefabWallVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("WallVolume", name, _bActive: true);
		return count;
	}

	public void SetWallVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, PrefabWallVolume _volumeSettings, bool remove = false)
	{
		while (_index >= WallVolumes.Count)
		{
			WallVolumes.Add(new PrefabWallVolume());
		}
		if (!remove)
		{
			WallVolumes[_index] = _volumeSettings;
		}
		else
		{
			WallVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index;
		SelectionBoxManager.Instance.GetCategory("WallVolume").RemoveBox(name);
		if (!remove)
		{
			AddWallVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("WallVolume", name, _bActive: true);
		}
	}

	public void AddWallVolumeSelectionBox(PrefabWallVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("WallVolume").AddBox(_name, _pos, _volume.size).UserData = _volume;
	}

	public int AddTriggerVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		PrefabTriggerVolume prefabTriggerVolume = new PrefabTriggerVolume();
		int count = TriggerVolumes.Count;
		TriggerVolumes.Add(prefabTriggerVolume);
		prefabTriggerVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count;
		AddTriggerVolumeSelectionBox(prefabTriggerVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("TriggerVolume", name, _bActive: true);
		return count;
	}

	public void SetTriggerVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		while (_index >= TriggerVolumes.Count)
		{
			TriggerVolumes.Add(new PrefabTriggerVolume());
		}
		if (!remove)
		{
			TriggerVolumes[_index] = _volumeSettings;
		}
		else
		{
			TriggerVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index;
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").RemoveBox(name);
		if (!remove)
		{
			AddTriggerVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("TriggerVolume", name, _bActive: true);
		}
	}

	public void AddTriggerVolumeSelectionBox(PrefabTriggerVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").AddBox(_name, _pos, _volume.size).UserData = _volume;
	}

	public void AddNewPOIMarker(string _prefabInstanceName, Vector3i bbPos, Vector3i _start, Vector3i _size, string _group, FastTags<TagGroup.Poi> _tags, Marker.MarkerTypes _type, bool isSelected = false)
	{
		POIMarkers.Add(new Marker(_start, _size, _type, _group, _tags));
		AddPOIMarker(_prefabInstanceName, bbPos, _start, _size, _group, _tags, _type, POIMarkers.Count - 1, isSelected);
	}

	public void AddPOIMarker(string _prefabInstanceName, Vector3i bbPos, Vector3i _start, Vector3i _size, string _group, FastTags<TagGroup.Poi> _tags, Marker.MarkerTypes _type, int _index, bool isSelected = false)
	{
		AddPOIMarkerSelectionBox(POIMarkers[_index], _index, bbPos + _start, isSelected);
	}

	public void AddPOIMarkerSelectionBox(Marker _marker, int _index, Vector3i _pos, bool isSelected = false)
	{
		string name = (_marker.Name = "POIMarker_" + _index);
		SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory("POIMarker").AddBox(name, _pos, _marker.Size);
		selectionBox.bDrawDirection = true;
		selectionBox.bAlwaysDrawDirection = true;
		SelectionBoxManager.Instance.SetUserData("POIMarker", name, _marker);
		SelectionBoxManager.Instance.SetActive("POIMarker", name, _bActive: true);
		float facing = 0f;
		switch (_marker.Rotations)
		{
		case 1:
			facing = ((_marker.MarkerType == Marker.MarkerTypes.PartSpawn) ? 90 : 270);
			break;
		case 2:
			facing = 180f;
			break;
		case 3:
			facing = ((_marker.MarkerType == Marker.MarkerTypes.PartSpawn) ? 270 : 90);
			break;
		}
		SelectionBoxManager.Instance.SetFacingDirection("POIMarker", name, facing);
		POIMarkerToolManager.RegisterPOIMarker(selectionBox);
		if (isSelected)
		{
			POIMarkerToolManager.SelectionChanged(selectionBox);
		}
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
	public bool doRaycast(Ray ray, out RaycastHit hitInfo, Vector3i _min)
	{
		bool flag = Physics.Raycast(ray, out hitInfo, 255f, 1073807360);
		if (!flag)
		{
			return false;
		}
		Vector3 vector = hitInfo.point + ray.direction * 0.01f;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(vector.x), Utils.Fastfloor(vector.y), Utils.Fastfloor(vector.z));
		Block block = GetBlock(vector3i.x - _min.x, vector3i.y - _min.y, vector3i.z - _min.z).Block;
		if (block.bImposterDontBlock || block.bImposterExclude)
		{
			ray.origin = hitInfo.point + ray.direction * 0.01f;
			flag = Physics.Raycast(ray, out hitInfo, 255f, 1073807360);
		}
		return flag;
	}

	public EnumInsideOutside[] UpdateInsideOutside(Vector3i _min, Vector3i _max)
	{
		EnumInsideOutside[] array = new EnumInsideOutside[GetBlockCount()];
		BlockValue air = BlockValue.Air;
		uint[] blocks = CellsToArrays().m_Blocks;
		RaycastHit hitInfo;
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
						if (doRaycast(ray, out hitInfo, _min))
						{
							num = Utils.FastMin(num, Utils.Fastfloor(hitInfo.point.y + ray.direction.y * 0.1f));
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
						if (doRaycast(ray, out hitInfo, _min))
						{
							num6 = Utils.FastMax(num6, Utils.Fastfloor(hitInfo.point.x + ray.direction.x * 0.1f));
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
						if (doRaycast(ray, out hitInfo, _min))
						{
							num10 = Utils.FastMin(num10, Utils.Fastfloor(hitInfo.point.x + ray.direction.x * 0.1f));
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
						if (doRaycast(ray, out hitInfo, _min))
						{
							num15 = Utils.FastMax(num15, Utils.Fastfloor(hitInfo.point.z + ray.direction.z * 0.1f));
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
						if (doRaycast(ray, out hitInfo, _min))
						{
							num20 = Utils.FastMin(num20, Utils.Fastfloor(hitInfo.point.z + ray.direction.z * 0.1f));
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

	public void RecalcInsideDevices(EnumInsideOutside[] eInsideOutside)
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
			if (!GetBlock(_x, _y, _z).Block.shape.IsTerrain() && eInsideOutside[i] == EnumInsideOutside.Inside)
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

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return GetChunk(chunkX, chunkZ);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return GetChunk(x / 16, z / 16);
	}

	public IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return GetChunk(_blockPos.x / 16, _blockPos.z / 16);
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
					gameObject.transform.parent = _go.transform;
					gameObject.name = $"Chunk[{x2},{z2}]";
					MeshFilter[][] array = new MeshFilter[MeshDescription.meshes.Length][];
					MeshRenderer[][] array2 = new MeshRenderer[MeshDescription.meshes.Length][];
					MeshCollider[][] array3 = new MeshCollider[MeshDescription.meshes.Length][];
					GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
					GameObject gameObject2 = new GameObject("_BlockEntities");
					GameObject gameObject3 = new GameObject("Meshes");
					gameObject2.transform.parent = gameObject.transform;
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

	public void HandleAddingTriggerLayers(BlockTrigger trigger)
	{
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			if (!TriggerLayers.Contains(trigger.TriggersIndices[i]))
			{
				TriggerLayers.Add(trigger.TriggersIndices[i]);
			}
		}
		for (int j = 0; j < trigger.TriggeredByIndices.Count; j++)
		{
			if (!TriggerLayers.Contains(trigger.TriggeredByIndices[j]))
			{
				TriggerLayers.Add(trigger.TriggeredByIndices[j]);
			}
		}
	}

	public void HandleAddingTriggerLayers(PrefabTriggerVolume trigger)
	{
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			if (!TriggerLayers.Contains(trigger.TriggersIndices[i]))
			{
				TriggerLayers.Add(trigger.TriggersIndices[i]);
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
		TriggerLayers = TriggerLayers.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (byte i) => i).ToList();
		if (TriggerLayers.Count > 0)
		{
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
	public static void LogPrefab(string format, params object[] args)
	{
		format = $"{GameManager.frameCount} Prefab {format}";
		Log.Warning(format, args);
	}
}
