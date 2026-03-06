using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public class PathingUtils
{
	public struct Data
	{
		public int WorldSize;

		public NativeArray<sbyte> pathingGrid;

		public int pathingGridSize;

		public PathNodePool nodePool;

		public MinHeapBinned minHeapBinned;

		public NativeArray<byte> closedList;

		public int closedListWidth;

		public int closedListMinY;

		public int closedListMaxY;
	}

	public struct MinHeapBinned
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const int cBins = 32768;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cScale = 0.07f;

		[PublicizedFrom(EAccessModifier.Private)]
		public NativeArray<int> nodeBins;

		[PublicizedFrom(EAccessModifier.Private)]
		public int lowBin;

		[PublicizedFrom(EAccessModifier.Private)]
		public int highBin;

		public void Init()
		{
			if (!nodeBins.IsCreated)
			{
				nodeBins = new NativeArray<int>(32768, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				lowBin = 0;
				highBin = 32767;
			}
			Reset();
		}

		public unsafe void Reset()
		{
			if (lowBin <= highBin)
			{
				int num = UnsafeUtility.SizeOf<int>();
				void* destination = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nodeBins) + lowBin * num;
				UnsafeUtility.MemSet(destination, byte.MaxValue, (highBin - lowBin + 1) * num);
			}
			lowBin = 32768;
			highBin = 0;
		}

		public int ExtractFirst(ref Data data)
		{
			if (lowBin <= highBin)
			{
				int num = nodeBins[lowBin];
				data.nodePool.Node(num, out var _node);
				nodeBins[lowBin] = _node.listNext;
				if (_node.listNext < 0)
				{
					while (++lowBin <= highBin && nodeBins[lowBin] < 0)
					{
					}
					if (lowBin > highBin)
					{
						lowBin = 32768;
						highBin = 0;
					}
				}
				return num;
			}
			return -1;
		}

		public void Add(ref Data data, int _nodeIndex)
		{
			ref PathNode reference = ref data.nodePool.Node(_nodeIndex);
			int num = (int)(reference.totalCost * 0.07f);
			if (num >= 32768)
			{
				num = 32767;
			}
			if (num < lowBin)
			{
				lowBin = num;
			}
			if (num > highBin)
			{
				highBin = num;
			}
			int num2 = nodeBins[num];
			if (num2 < 0)
			{
				reference.listNext = -1;
				nodeBins[num] = _nodeIndex;
				return;
			}
			ref PathNode reference2 = ref data.nodePool.Node(num2);
			if (reference.totalCost <= reference2.totalCost)
			{
				reference.listNext = num2;
				nodeBins[num] = _nodeIndex;
				return;
			}
			ref PathNode reference3 = ref reference2;
			int listNext;
			for (listNext = reference3.listNext; listNext >= 0; listNext = reference3.listNext)
			{
				ref PathNode reference4 = ref data.nodePool.Node(listNext);
				if (reference.totalCost <= reference4.totalCost)
				{
					break;
				}
				reference3 = ref reference4;
			}
			reference.listNext = listNext;
			reference3.listNext = _nodeIndex;
		}

		public void Cleanup()
		{
			nodeBins.Dispose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PathNodeType
	{
		Free = 0,
		Road = 1,
		Prefab = 2,
		CityLimits = 4,
		Blocked = 8
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate int FindDetailedPath_0000A282_0024PostfixBurstDelegate(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class FindDetailedPath_0000A282_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FindDetailedPath_0000A282_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static FindDetailedPath_0000A282_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static int Invoke(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref WorldBuilder.Data, ref Data, ref Vector2i, ref Vector2i, bool, bool, int>)functionPointer)(ref wd, ref data, ref startPos, ref endPos, _isCountryRoad, _isRiver);
				}
			}
			return FindDetailedPath_0024BurstManaged(ref wd, ref data, in startPos, in endPos, _isCountryRoad, _isRiver);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate int FindDetailedPath_0000A284_0024PostfixBurstDelegate(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class FindDetailedPath_0000A284_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FindDetailedPath_0000A284_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static FindDetailedPath_0000A284_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static int Invoke(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref WorldBuilder.Data, ref Data, ref Vector2i, ref NativeList<Vector2i>, bool, bool, int>)functionPointer)(ref wd, ref data, ref startPos, ref _endPath, _isCountryRoad, _isRiver);
				}
			}
			return FindDetailedPath_0024BurstManaged(ref wd, ref data, in startPos, in _endPath, _isCountryRoad, _isRiver);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void CalcPathBounds_0000A285_0024PostfixBurstDelegate(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class CalcPathBounds_0000A285_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalcPathBounds_0000A285_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static CalcPathBounds_0000A285_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeList<Vector2i>, ref Vector2i, ref Vector2i, void>)functionPointer)(ref _path, ref _min, ref _max);
					return;
				}
			}
			CalcPathBounds_0024BurstManaged(in _path, out _min, out _max);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate float FindClosestPathPoint_0000A286_0024PostfixBurstDelegate(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class FindClosestPathPoint_0000A286_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FindClosestPathPoint_0000A286_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static FindClosestPathPoint_0000A286_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static float Invoke(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeList<Vector2>, ref Vector2, ref Vector2, int, float>)functionPointer)(ref _path, ref _startPos, ref _destPoint, _step);
				}
			}
			return FindClosestPathPoint_0024BurstManaged(in _path, in _startPos, out _destPoint, _step);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate float FindClosestPathPoint_0000A287_0024PostfixBurstDelegate(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class FindClosestPathPoint_0000A287_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FindClosestPathPoint_0000A287_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static FindClosestPathPoint_0000A287_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static float Invoke(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeList<Vector2i>, ref Vector2i, ref Vector2i, float>)functionPointer)(ref _path, ref _startPos, ref _destPoint);
				}
			}
			return FindClosestPathPoint_0024BurstManaged(in _path, in _startPos, out _destPoint);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate bool IsPointOnPath_0000A288_0024PostfixBurstDelegate(in NativeList<Vector2i> _path, in Vector2i _point);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class IsPointOnPath_0000A288_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(IsPointOnPath_0000A288_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static IsPointOnPath_0000A288_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static bool Invoke(in NativeList<Vector2i> _path, in Vector2i _point)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeList<Vector2i>, ref Vector2i, bool>)functionPointer)(ref _path, ref _point);
				}
			}
			return IsPointOnPath_0024BurstManaged(in _path, in _point);
		}
	}

	public const int PATHING_GRID_TILE_SIZE = 10;

	public const int stepSize = 10;

	public static readonly Vector2i stepHalf = new Vector2i(5, 5);

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRoadCountryMaxStepH = 12f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRoadHighwayMaxStepH = 11f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightCostScale = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNeighborsCount = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] neighborOffset8Ways = new Vector2i[8]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public NativeList<Vector2i> pathTemp = new NativeList<Vector2i>(200, Allocator.Persistent);

	[PublicizedFrom(EAccessModifier.Private)]
	public Data data;

	public PathingUtils(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
		data.nodePool = new PathNodePool(100000);
	}

	public int GetPathCost(Vector2i start, Vector2i end, bool isCountryRoad = false)
	{
		InitPathData();
		int num = 0;
		int num2 = FindDetailedPath(ref worldBuilder.data, ref data, start / 10, end / 10, isCountryRoad, _isRiver: false);
		while (num2 >= 0)
		{
			data.nodePool.Node(num2, out var _node);
			num++;
			num2 = _node.pathNext;
		}
		data.nodePool.ReturnAll();
		return num;
	}

	public NativeList<Vector2i> GetPath(Vector2i _start, Vector2i _end, bool _isCountryRoad, bool _isRiver = false)
	{
		InitPathData();
		int num = FindDetailedPath(ref worldBuilder.data, ref data, _start / 10, _end / 10, _isCountryRoad, _isRiver);
		pathTemp.Clear();
		while (num >= 0)
		{
			data.nodePool.Node(num, out var _node);
			Vector2i value = _node.position * 10 + stepHalf;
			pathTemp.Add(in value);
			num = _node.pathNext;
		}
		data.nodePool.ReturnAll();
		return pathTemp;
	}

	public Vector2i GetPathPoint(Vector2i _start, ref NativeList<Vector2> _endPath, bool _isCountryRoad, bool _isRiver, out int _cost)
	{
		InitPathData();
		ConvertPathToTemp(ref _endPath);
		Vector2i result = Vector2i.min;
		int num = 0;
		int num2 = FindDetailedPath(ref worldBuilder.data, ref data, _start / 10, in pathTemp, _isCountryRoad, _isRiver);
		if (num2 >= 0)
		{
			data.nodePool.Node(num2, out var _node);
			result = _node.position * 10 + stepHalf;
			if (FindClosestPathPoint(in _endPath, result.AsVector2(), out var _destPoint) < 400f)
			{
				result = new Vector2i(_destPoint);
			}
			while (num2 >= 0)
			{
				data.nodePool.Node(num2, out _node);
				num++;
				num2 = _node.pathNext;
			}
		}
		data.nodePool.ReturnAll();
		_cost = num;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPathData()
	{
		data.WorldSize = worldBuilder.WorldSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConvertPathToTemp(ref NativeList<Vector2> _path)
	{
		pathTemp.Clear();
		Vector2i value = default(Vector2i);
		for (int i = 0; i < _path.Length; i++)
		{
			value.x = ((int)_path[i].x + stepHalf.x) / 10;
			value.y = ((int)_path[i].y + stepHalf.y) / 10;
			pathTemp.Add(in value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void ClosedListInit(ref Data data)
	{
		int num = data.WorldSize / 10 + 1;
		if (data.closedList.IsCreated)
		{
			int num2 = data.closedListMinY * data.closedListWidth;
			int num3 = (data.closedListMaxY + 1) * data.closedListWidth;
			void* destination = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data.closedList) + num2;
			UnsafeUtility.MemSet(destination, 0, num3 - num2);
		}
		if (!data.closedList.IsCreated || data.closedListWidth != num)
		{
			data.closedListWidth = num;
			data.closedList = new NativeArray<byte>(num * num, Allocator.Persistent);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int FindDetailedPath(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
	{
		return FindDetailedPath_0000A282_0024BurstDirectCall.Invoke(ref wd, ref data, in startPos, in endPos, _isCountryRoad, _isRiver);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float DiagonalDist(Vector2i v1, Vector2i v2)
	{
		float num = Utils.FastAbs(v1.x - v2.x);
		float num2 = Utils.FastAbs(v1.y - v2.y);
		return num + num2 + -0.585786f * Utils.FastMin(num, num2);
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int FindDetailedPath(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
	{
		return FindDetailedPath_0000A284_0024BurstDirectCall.Invoke(ref wd, ref data, in startPos, in _endPath, _isCountryRoad, _isRiver);
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalcPathBounds(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
	{
		CalcPathBounds_0000A285_0024BurstDirectCall.Invoke(in _path, out _min, out _max);
	}

	[BurstCompile(CompileSynchronously = true)]
	public static float FindClosestPathPoint(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
	{
		return FindClosestPathPoint_0000A286_0024BurstDirectCall.Invoke(in _path, in _startPos, out _destPoint, _step);
	}

	[BurstCompile(CompileSynchronously = true)]
	public static float FindClosestPathPoint(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
	{
		return FindClosestPathPoint_0000A287_0024BurstDirectCall.Invoke(in _path, in _startPos, out _destPoint);
	}

	[BurstCompile(CompileSynchronously = true)]
	public static bool IsPointOnPath(in NativeList<Vector2i> _path, in Vector2i _point)
	{
		return IsPointOnPath_0000A288_0024BurstDirectCall.Invoke(in _path, in _point);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsBlocked(ref WorldBuilder.Data wd, int pathX, int pathY, bool isRiver = false)
	{
		Vector2i vector2i = pathPositionToWorldCenter(pathX, pathY);
		if (!wd.InWorldBounds(vector2i.x, vector2i.y))
		{
			return true;
		}
		if (wd.GetStreetTileDataWorld(vector2i.x, vector2i.y).IsCity)
		{
			return true;
		}
		if (!isRiver && IsWater(ref wd, pathX, pathY))
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool InBounds(ref WorldBuilder.Data wd, Vector2i pos)
	{
		return InBounds(ref wd, pos.x, pos.y);
	}

	public static bool InBounds(ref WorldBuilder.Data wd, int pathX, int pathY)
	{
		Vector2i vector2i = pathPositionToWorldCenter(pathX, pathY);
		if ((uint)vector2i.x < wd.WorldSize)
		{
			return (uint)vector2i.y < wd.WorldSize;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsWater(ref WorldBuilder.Data wd, int pathX, int pathY)
	{
		Vector2i vector2i = pathPositionToWorldMin(pathX, pathY);
		if (wd.GetStreetTileDataWorld(vector2i.x, vector2i.y).OverlapsWater)
		{
			for (int i = vector2i.y; i < vector2i.y + 10; i++)
			{
				for (int j = vector2i.x; j < vector2i.x + 10; j++)
				{
					if (wd.GetWater(j, i) > 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetHeight(ref WorldBuilder.Data wd, Vector2i pos)
	{
		return GetHeight(ref wd, pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetHeight(ref WorldBuilder.Data wd, int pathX, int pathY)
	{
		return wd.GetHeight(pathPositionToWorldCenter(pathX, pathY));
	}

	public BiomeType GetBiome(Vector2i pos)
	{
		return GetBiome(pos.x, pos.y);
	}

	public BiomeType GetBiome(int pathX, int pathY)
	{
		return worldBuilder.GetBiome(pathPositionToWorldCenter(pathX, pathY));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i pathPositionToWorldCenter(int pathX, int pathY)
	{
		Vector2i result = default(Vector2i);
		result.x = pathX * 10 + 5;
		result.y = pathY * 10 + 5;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i pathPositionToWorldMin(int pathX, int pathY)
	{
		Vector2i result = default(Vector2i);
		result.x = pathX * 10;
		result.y = pathY * 10;
		return result;
	}

	public void AddMoveLimitArea(Rect r)
	{
		int num = (int)r.xMin;
		int num2 = (int)r.yMin;
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				if (j != 7 && i != 7)
				{
					SetPathBlocked(num + j, num2 + i, isBlocked: true);
				}
			}
		}
	}

	public void RemoveFullyBlockedArea(Rect r)
	{
		int num = (int)r.xMin;
		int num2 = (int)r.yMin;
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				SetPathBlocked(num + j, num2 + i, isBlocked: false);
			}
		}
	}

	public void AddFullyBlockedArea(Rect r)
	{
		int num = (int)(r.xMin + 0.5f);
		int num2 = (int)(r.yMin + 0.5f);
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				SetPathBlocked(num + j, num2 + i, isBlocked: true);
			}
		}
	}

	public void SetPathBlocked(Vector2i pos, bool isBlocked)
	{
		SetPathBlocked(pos.x, pos.y, isBlocked);
	}

	public void SetPathBlocked(int x, int y, bool isBlocked)
	{
		SetPathBlocked(x, y, (sbyte)(isBlocked ? sbyte.MinValue : 0));
	}

	public void SetPathBlocked(int x, int y, sbyte costMult)
	{
		if (!data.pathingGrid.IsCreated)
		{
			SetupPathingGrid();
		}
		if ((uint)x < data.pathingGridSize && (uint)y < data.pathingGridSize)
		{
			data.pathingGrid[x + y * data.pathingGridSize] = costMult;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsPathBlocked(ref Data data, int x, int y)
	{
		if ((uint)x >= data.pathingGridSize || (uint)y >= data.pathingGridSize)
		{
			return true;
		}
		return data.pathingGrid[x + y * data.pathingGridSize] == sbyte.MinValue;
	}

	public bool IsPointOnHighwayWorld(int x, int y)
	{
		int index = x / 10 + y / 10 * worldBuilder.data.PathTileGridWidth;
		return worldBuilder.data.PathTileGrid[index].TileState == PathTile.PathTileStates.Highway;
	}

	public void SetupPathingGrid()
	{
		data.pathingGridSize = worldBuilder.WorldSize / 10;
		data.pathingGrid = new NativeArray<sbyte>(data.pathingGridSize * data.pathingGridSize, Allocator.Persistent);
	}

	public void Cleanup()
	{
		data.pathingGrid.Dispose();
		data.pathingGridSize = 0;
		data.nodePool.Cleanup();
		data.minHeapBinned.Cleanup();
		data.closedList.Dispose();
		pathTemp.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int FindDetailedPath_0024BurstManaged(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
	{
		if (!InBounds(ref wd, startPos) || !InBounds(ref wd, endPos))
		{
			return -1;
		}
		ClosedListInit(ref data);
		data.closedList[startPos.x + startPos.y * data.closedListWidth] = 1;
		data.closedListMinY = startPos.y;
		data.closedListMaxY = startPos.y;
		data.minHeapBinned.Init();
		int num = data.nodePool.Alloc();
		data.nodePool.Node(num).Set(startPos, 0f, 0f, -1);
		data.minHeapBinned.Add(ref data, num);
		Vector2i vector2i = new Vector2i(Utils.FastMin(startPos.x, endPos.x), Utils.FastMin(startPos.y, endPos.y));
		Vector2i vector2i2 = new Vector2i(Utils.FastMax(startPos.x, endPos.x), Utils.FastMax(startPos.y, endPos.y));
		int num2 = Utils.FastMax(0, vector2i.x - 200);
		int num3 = Utils.FastMax(0, vector2i.y - 200);
		int num4 = Utils.FastMin(vector2i2.x + 200, data.closedListWidth - 1);
		int num5 = Utils.FastMin(vector2i2.y + 200, data.closedListWidth - 1);
		float num6 = (_isCountryRoad ? 12f : 11f);
		int num7 = 20000;
		int num8;
		while ((num8 = data.minHeapBinned.ExtractFirst(ref data)) >= 0 && --num7 >= 0)
		{
			ref PathNode reference = ref data.nodePool.Node(num8);
			Vector2i position = reference.position;
			if (position == endPos)
			{
				return num8;
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2i vector2i3 = neighborOffset8Ways[i];
				Vector2i vector2i4 = reference.position + vector2i3;
				if (vector2i4.x < num2 || vector2i4.y < num3 || vector2i4.x >= num4 || vector2i4.y >= num5)
				{
					continue;
				}
				int index = vector2i4.x + vector2i4.y * data.closedListWidth;
				if (data.closedList[index] != 0)
				{
					continue;
				}
				bool flag = vector2i4 == endPos;
				if (!flag)
				{
					bool flag2 = IsPathBlocked(ref data, vector2i4.x, vector2i4.y);
					if (!flag2)
					{
						flag2 = IsBlocked(ref wd, vector2i4.x, vector2i4.y, _isRiver);
					}
					if (flag2)
					{
						data.closedList[index] = 1;
						data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
						data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
						continue;
					}
				}
				float num9 = Utils.FastAbs(GetHeight(ref wd, position) - GetHeight(ref wd, vector2i4));
				if (num9 > num6)
				{
					continue;
				}
				float num10 = DiagonalDist(vector2i4, endPos);
				num10 *= 1.4f;
				bool flag3 = vector2i3.x != 0 && vector2i3.y != 0;
				bool flag4 = true;
				if (!_isCountryRoad)
				{
					float num11 = Vector2i.DistanceSqr(vector2i4, startPos);
					float num12 = Vector2i.DistanceSqr(vector2i4, endPos);
					if (num11 <= 8.410001f || num12 <= 8.410001f)
					{
						flag4 = false;
					}
					Vector2i vector2i5 = vector2i4 * 10;
					if (wd.GetStreetTileDataWorld(vector2i5.x, vector2i5.y).ConnectedHighwayCount >= 3)
					{
						continue;
					}
					if (!flag)
					{
						bool flag5 = wd.PathTileGrid[vector2i4.x + vector2i4.y * wd.PathTileGridWidth].TileState == PathTile.PathTileStates.Highway;
						if (flag5)
						{
							continue;
						}
						if (flag3)
						{
							for (int j = 0; j < 2; j++)
							{
								int num13 = reference.position.x;
								int num14 = reference.position.y;
								if (j == 0)
								{
									num13 += vector2i3.x;
								}
								else
								{
									num14 += vector2i3.y;
								}
								flag5 = IsPathBlocked(ref data, num13, num14);
								if (flag5)
								{
									break;
								}
								flag5 = IsBlocked(ref wd, num13, num14);
								if (flag5)
								{
									break;
								}
								if (wd.PathTileGrid[num13 + num14 * wd.PathTileGridWidth].TileState == PathTile.PathTileStates.Highway)
								{
									flag5 = true;
									break;
								}
							}
							if (flag5)
							{
								continue;
							}
						}
					}
					num9 = Utils.FastMax(0f, num9 - 0.5f);
					num9 *= 3f;
				}
				num9 *= 0.2f;
				float num15 = 1f;
				if (flag3)
				{
					num15 += 0.414214f;
				}
				if (flag4 && data.pathingGrid.IsCreated)
				{
					int num16 = data.pathingGrid[vector2i4.x + vector2i4.y * data.pathingGridSize];
					if (num16 > 0)
					{
						num15 *= (float)num16;
					}
				}
				data.closedList[index] = 1;
				data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
				data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
				int num17 = data.nodePool.Alloc();
				ref PathNode reference2 = ref data.nodePool.Node(num17);
				float num18 = reference.travelledCost + num15 + num9;
				float totalCost = num18 + num10;
				reference2.Set(vector2i4, num18, totalCost, num8);
				data.minHeapBinned.Add(ref data, num17);
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int FindDetailedPath_0024BurstManaged(ref WorldBuilder.Data wd, ref Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
	{
		ClosedListInit(ref data);
		data.closedList[startPos.x + startPos.y * data.closedListWidth] = 1;
		data.closedListMinY = startPos.y;
		data.closedListMaxY = startPos.y;
		data.minHeapBinned.Init();
		int num = data.nodePool.Alloc();
		data.nodePool.Node(num).Set(startPos, 0f, 0f, -1);
		data.minHeapBinned.Add(ref data, num);
		CalcPathBounds(in _endPath, out var _min, out var _max);
		int num2 = Utils.FastMax(0, _min.x - 200);
		int num3 = Utils.FastMax(0, _min.y - 200);
		int num4 = Utils.FastMin(_max.x + 200, data.closedListWidth - 1);
		int num5 = Utils.FastMin(_max.y + 200, data.closedListWidth - 1);
		float num6 = (_isCountryRoad ? 12f : 11f);
		int num7 = 20000;
		int num8;
		while ((num8 = data.minHeapBinned.ExtractFirst(ref data)) >= 0 && --num7 >= 0)
		{
			ref PathNode reference = ref data.nodePool.Node(num8);
			Vector2i _point = reference.position;
			if (IsPointOnPath(in _endPath, in _point))
			{
				return num8;
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2i vector2i = neighborOffset8Ways[i];
				Vector2i _startPos = reference.position + vector2i;
				if (_startPos.x < num2 || _startPos.y < num3 || _startPos.x >= num4 || _startPos.y >= num5)
				{
					continue;
				}
				int index = _startPos.x + _startPos.y * data.closedListWidth;
				if (data.closedList[index] != 0)
				{
					continue;
				}
				FindClosestPathPoint(in _endPath, in _startPos, out var _destPoint);
				if (_startPos != _destPoint)
				{
					bool flag = IsPathBlocked(ref data, _startPos.x, _startPos.y);
					if (!flag)
					{
						flag = IsBlocked(ref wd, _startPos.x, _startPos.y, _isRiver);
					}
					if (flag)
					{
						data.closedList[index] = 1;
						data.closedListMinY = Utils.FastMin(data.closedListMinY, _startPos.y);
						data.closedListMaxY = Utils.FastMax(data.closedListMaxY, _startPos.y);
						continue;
					}
				}
				float num9 = Utils.FastAbs(GetHeight(ref wd, _point) - GetHeight(ref wd, _startPos));
				if (num9 > num6)
				{
					continue;
				}
				float num10 = Vector2i.Distance(_startPos, _destPoint);
				num10 *= 1.4f;
				num9 *= 0.2f;
				num9 += 1f;
				if (vector2i.x != 0 && vector2i.y != 0)
				{
					num9 += 0.414214f;
				}
				if (data.pathingGrid.IsCreated)
				{
					int num11 = data.pathingGrid[_startPos.x + _startPos.y * data.pathingGridSize];
					if (num11 > 0)
					{
						num9 *= (float)num11;
					}
				}
				data.closedList[index] = 1;
				data.closedListMinY = Utils.FastMin(data.closedListMinY, _startPos.y);
				data.closedListMaxY = Utils.FastMax(data.closedListMaxY, _startPos.y);
				int num12 = data.nodePool.Alloc();
				ref PathNode reference2 = ref data.nodePool.Node(num12);
				float num13 = reference.travelledCost + num9;
				float totalCost = num13 + num10;
				reference2.Set(_startPos, num13, totalCost, num8);
				data.minHeapBinned.Add(ref data, num12);
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void CalcPathBounds_0024BurstManaged(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
	{
		_min = Vector2i.max;
		_max = Vector2i.min;
		foreach (Vector2i item in _path)
		{
			_min.x = Utils.FastMin(_min.x, item.x);
			_min.y = Utils.FastMin(_min.y, item.y);
			_max.x = Utils.FastMax(_max.x, item.x);
			_max.y = Utils.FastMax(_max.y, item.y);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static float FindClosestPathPoint_0024BurstManaged(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
	{
		_destPoint = Vector2.zero;
		float num = float.MaxValue;
		int length = _path.Length;
		for (int i = 0; i < length; i += _step)
		{
			Vector2 vector = _path[i];
			float sqrMagnitude = (_startPos - vector).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				_destPoint = vector;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static float FindClosestPathPoint_0024BurstManaged(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
	{
		_destPoint = Vector2i.zero;
		float num = float.MaxValue;
		foreach (Vector2i item in _path)
		{
			float num2 = Vector2i.DistanceSqr(_startPos, item);
			if (num2 < num)
			{
				num = num2;
				_destPoint = item;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool IsPointOnPath_0024BurstManaged(in NativeList<Vector2i> _path, in Vector2i _point)
	{
		foreach (Vector2i item in _path)
		{
			if (item.x == _point.x && item.y == _point.y)
			{
				return true;
			}
		}
		return false;
	}
}
