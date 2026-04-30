using System;
using System.Collections;
using System.Collections.Generic;
using GamePath;
using Pathfinding;
using UnityEngine;

public class AstarManager : MonoBehaviour
{
	public class Area
	{
		public Area next;

		public Vector2i pos;

		public Bounds2i bounds;

		public bool hasBlocks;

		public bool isPartial;

		public bool isSlowUpdate;

		public float updateDelay;
	}

	public struct Bounds2i
	{
		public Vector2i min;

		public Vector2i max;

		public bool Contains(Vector2i pos)
		{
			if (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
			{
				return true;
			}
			return false;
		}

		public void Encapsulate(Vector2i pos)
		{
			if (pos.x < min.x)
			{
				min.x = pos.x;
			}
			if (pos.x > max.x)
			{
				max.x = pos.x;
			}
			if (pos.y < min.y)
			{
				min.y = pos.y;
			}
			if (pos.y > max.y)
			{
				max.y = pos.y;
			}
		}

		public Bounds ToBounds()
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3(min.x, 0f, min.y), new Vector3((float)max.x + 0.999999f, 0f, (float)max.y + 0.999999f));
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Location
	{
		public Vector2 pos;

		public int size;

		public float duration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct MergedLocations
	{
		public Vector2 pos;

		public int size;
	}

	public static AstarManager Instance;

	public const float cGridHeight = 320f;

	public const float cGridY = -32f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cGridXZSize = 76;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoveDist = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCharHeight = 1.8f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCharDiameter = 0.3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLocationFindPer = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLocationDuration = 4f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerMergeDist = 19f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerMergeDistSq = 361f;

	public const float cUpdateDeltaTime = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AstarPath astar;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastWorkTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldOrigin;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 worldOriginXZ;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Area> areaList = new List<Area>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AstarVoxelGrid> graphList = new List<AstarVoxelGrid>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Location> locations = new List<Location>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<MergedLocations> mergedLocations = new List<MergedLocations>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AstarVoxelGrid> moveList = new List<AstarVoxelGrid>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AstarVoxelGrid moveCurrent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] updateBlockOffsets = new int[8] { -1, 0, 1, 0, 0, -1, 0, 1 };

	public static void Init(GameObject obj)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !(GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"))
		{
			Log.Out("AstarManager Init");
			obj.AddComponent<AstarManager>();
			new ASPPathFinderThread().StartWorkerThreads();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Instance = this;
		if (!AstarPath.active)
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/AStarPath"), Vector3.zero, Quaternion.identity).transform.SetParent(GameManager.Instance.transform, worldPositionStays: false);
		}
		astar = AstarPath.active;
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		chunkCache.OnBlockChangedDelegates += OnBlockChanged;
		chunkCache.OnBlockDamagedDelegates += OnBlockDamaged;
		OriginChanged();
	}

	public static PathNavigate CreateNavigator(EntityAlive _entity)
	{
		return new ASPPathNavigate(_entity);
	}

	public static void Cleanup()
	{
		if ((bool)Instance)
		{
			Log.Out("AstarManager Cleanup");
			ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
			chunkCache.OnBlockChangedDelegates -= Instance.OnBlockChanged;
			chunkCache.OnBlockDamagedDelegates -= Instance.OnBlockDamaged;
			PathFinderThread.Instance.Cleanup();
			if ((bool)AstarPath.active)
			{
				AstarPath.active.enabled = false;
				UnityEngine.Object.Destroy(AstarPath.active.gameObject);
			}
			UnityEngine.Object.Destroy(Instance);
			Instance = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		float elapsedTime = 0f;
		while (astar != null)
		{
			if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
			{
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			yield return new WaitForSeconds(0.1f);
			if (GameManager.Instance == null || GameManager.Instance.World == null)
			{
				continue;
			}
			elapsedTime += 0.1f;
			if (astar.IsAnyWorkItemInProgress)
			{
				lastWorkTime = Time.time;
			}
			else
			{
				if (Time.time - lastWorkTime < 0.4f || astar.IsAnyGraphUpdateInProgress)
				{
					continue;
				}
				UpdateGraphs(elapsedTime);
				int count = areaList.Count;
				if (count > 0)
				{
					count = Mathf.Min(20, count);
					int num = 0;
					for (int i = 0; i < count; i++)
					{
						Area area = areaList[num];
						area.updateDelay -= elapsedTime;
						if (area.updateDelay > 0f)
						{
							num++;
							continue;
						}
						if (area.next == null)
						{
							areaList.RemoveAt(num);
						}
						else
						{
							areaList[num] = area.next;
							num++;
						}
						Bounds bounds;
						if (!area.isPartial)
						{
							bounds = default(Bounds);
							Vector3 vector = new Vector3(area.pos.x, 0f, area.pos.y);
							Vector3 max = vector;
							max.x += 16f;
							max.z += 16f;
							bounds.SetMinMax(vector, max);
						}
						else
						{
							if (!area.hasBlocks)
							{
								continue;
							}
							bounds = area.bounds.ToBounds();
						}
						Vector3 center = bounds.center;
						center.y = 128f;
						center -= worldOrigin;
						bounds.center = center;
						Vector3 size = bounds.size;
						size.y = 320f;
						bounds.size = size;
						if (graphList.Count > 0)
						{
							LayerGridGraphUpdate layerGridGraphUpdate = new LayerGridGraphUpdate();
							layerGridGraphUpdate.bounds = bounds;
							layerGridGraphUpdate.recalculateNodes = true;
							astar.UpdateGraphs(layerGridGraphUpdate);
						}
					}
				}
				elapsedTime = 0f;
			}
		}
	}

	public void AddLocation(Vector3 pos3d, int size)
	{
		Vector2 vector = default(Vector2);
		vector.x = pos3d.x;
		vector.y = pos3d.z;
		Location location = FindLocation(vector, size);
		if (location == null)
		{
			location = new Location();
			location.pos = vector;
			location.size = size;
			locations.Add(location);
		}
		else
		{
			location.pos = (location.pos + vector) * 0.5f;
		}
		location.duration = 4f;
	}

	public void AddLocationLine(Vector3 startPos, Vector3 endPos, int size)
	{
		startPos.y = 0f;
		endPos.y = 0f;
		Vector3 normalized = (endPos - startPos).normalized;
		Vector3 pos3d = startPos + normalized * ((float)size * 0.4f);
		AddLocation(pos3d, size);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Location FindLocation(Vector2 pos, int size)
	{
		Location result = null;
		float num = (float)(size * size) * 0.040000003f;
		for (int i = 0; i < locations.Count; i++)
		{
			Location location = locations[i];
			if (location.size >= size)
			{
				float sqrMagnitude = (location.pos - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = location;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGraphs(float deltaTime)
	{
		World world = GameManager.Instance.World;
		this.mergedLocations.Clear();
		List<EntityPlayer> list = world.Players.list;
		Vector2 pos = default(Vector2);
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			pos.x = entityPlayer.position.x;
			pos.y = entityPlayer.position.z;
			Merge(pos, 76);
		}
		for (int j = 0; j < locations.Count; j++)
		{
			Location location = locations[j];
			location.duration -= deltaTime;
			if (location.duration <= 0f)
			{
				locations.RemoveAt(j);
				j--;
			}
			else
			{
				Merge(location.pos, location.size);
			}
		}
		for (int k = 0; k < graphList.Count; k++)
		{
			graphList[k].IsUsed = false;
		}
		for (int l = 0; l < this.mergedLocations.Count; l++)
		{
			MergedLocations mergedLocations = this.mergedLocations[l];
			AstarVoxelGrid astarVoxelGrid = FindClosestGraph(mergedLocations.pos, mergedLocations.size);
			if (astarVoxelGrid == null)
			{
				astarVoxelGrid = AddGraph(mergedLocations.size);
				astarVoxelGrid.SetPos(LocalPosToGridPos(mergedLocations.pos - worldOriginXZ));
			}
			astarVoxelGrid.IsUsed = true;
			UpdateGraphPos(astarVoxelGrid, mergedLocations.pos);
		}
		UpdateMoveGraph();
		for (int m = 0; m < graphList.Count; m++)
		{
			AstarVoxelGrid astarVoxelGrid2 = graphList[m];
			if (!astarVoxelGrid2.IsUsed)
			{
				MoveGraphRemove(astarVoxelGrid2);
				astar.data.RemoveGraph(astarVoxelGrid2);
				graphList.RemoveAt(m);
				m--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge(Vector2 pos, int size)
	{
		bool flag = false;
		for (int i = 0; i < mergedLocations.Count; i++)
		{
			MergedLocations value = mergedLocations[i];
			if (size <= value.size && (value.pos - pos).sqrMagnitude <= 361f)
			{
				value.pos = (value.pos + pos) * 0.5f;
				mergedLocations[i] = value;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			MergedLocations item = default(MergedLocations);
			item.pos = pos;
			item.size = size;
			mergedLocations.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindMoveIndex(AstarVoxelGrid graph)
	{
		for (int i = 0; i < moveList.Count; i++)
		{
			if (moveList[i] == graph)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGraphPos(AstarVoxelGrid graph, Vector2 pos)
	{
		if (graph.IsMoving())
		{
			return;
		}
		Vector2 vector = pos - worldOriginXZ;
		if (graph.IsFullUpdateNeeded)
		{
			Vector3 pos2 = LocalPosToGridPos(vector);
			graph.SetPos(pos2);
			return;
		}
		Vector2 a = vector;
		a.x -= graph.center.x;
		a.y -= graph.center.z;
		if (Vector2.SqrMagnitude(a) > 100f)
		{
			graph.GridMovePendingPos = pos;
			if (FindMoveIndex(graph) < 0)
			{
				moveList.Insert(moveList.Count, graph);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMoveGraph()
	{
		if (moveCurrent != null)
		{
			if (moveCurrent.IsMoving())
			{
				return;
			}
			moveCurrent = null;
		}
		if (moveList.Count > 0)
		{
			AstarVoxelGrid astarVoxelGrid = moveList[0];
			moveList.RemoveAt(0);
			MoveGraph(astarVoxelGrid, astarVoxelGrid.GridMovePendingPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveGraphRemove(AstarVoxelGrid graph)
	{
		if (moveCurrent == graph)
		{
			moveCurrent = null;
		}
		int num = FindMoveIndex(graph);
		if (num >= 0)
		{
			moveList.RemoveAt(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveGraph(AstarVoxelGrid graph, Vector2 pos)
	{
		moveCurrent = graph;
		Vector2 pos2 = pos - worldOriginXZ;
		Vector3 targetPos = LocalPosToGridPos(pos2);
		graph.Move(targetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LocalPosToGridPos(Vector2 pos)
	{
		Vector3 result = default(Vector3);
		result.x = Mathf.Round(pos.x);
		result.z = Mathf.Round(pos.y);
		result.y = -32f - worldOrigin.y;
		return result;
	}

	public void OriginChanged()
	{
		worldOrigin = Origin.position;
		worldOriginXZ.x = worldOrigin.x;
		worldOriginXZ.y = worldOrigin.z;
		for (int i = 0; i < graphList.Count; i++)
		{
			graphList[i].IsFullUpdateNeeded = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Scan()
	{
		if (!astar.isScanning)
		{
			astar.Scan();
		}
		yield return null;
	}

	public void OnBlockChanged(Vector3i pos, BlockValue bvOld, sbyte densOld, TextureFullArray texOld, BlockValue bvNew)
	{
		Block block = bvNew.Block;
		bool isSlowUpdate = block is BlockDoor;
		if (!block.isMultiBlock)
		{
			UpdateBlock(pos, isSlowUpdate);
			return;
		}
		int rotation = bvNew.rotation;
		int length = block.multiBlockPos.Length;
		for (int i = 0; i < length; i++)
		{
			Vector3i blockPos = block.multiBlockPos.Get(i, bvNew.type, rotation);
			blockPos += pos;
			UpdateBlock(blockPos, isSlowUpdate);
		}
	}

	public void OnBlockDamaged(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId)
	{
		UpdateBlock(_blockPos, isSlowUpdate: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBlock(Vector3i blockPos, bool isSlowUpdate)
	{
		Vector2i vector2i = new Vector2i(blockPos.x, blockPos.z);
		Vector2i vector2i2 = vector2i;
		vector2i2.x &= -16;
		vector2i2.y &= -16;
		Area area = AddAreaBlock(vector2i);
		area.hasBlocks = true;
		area.isSlowUpdate = isSlowUpdate;
		for (int i = 0; i < 4; i++)
		{
			Vector2i vector2i3 = vector2i;
			int num = i * 2;
			vector2i3.x += updateBlockOffsets[num];
			vector2i3.y += updateBlockOffsets[num + 1];
			Vector2i vector2i4 = vector2i3;
			vector2i4.x &= -16;
			vector2i4.y &= -16;
			if (vector2i4.x != vector2i2.x || vector2i4.y != vector2i2.y)
			{
				AddAreaBlock(vector2i3);
			}
		}
	}

	public static void AddBoundsToUpdate(Bounds _bounds)
	{
		if (!(Instance == null))
		{
			Vector2i pos = new Vector2i(Mathf.FloorToInt(_bounds.min.x), Mathf.FloorToInt(_bounds.min.z));
			Area area = Instance.AddArea(pos, noNext: true);
			if (!area.isSlowUpdate)
			{
				area.updateDelay = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Area AddAreaBlock(Vector2i pos)
	{
		Area area = AddArea(pos, noNext: false);
		if (!area.isPartial)
		{
			area.isPartial = true;
			area.bounds.min = pos;
			area.bounds.max = pos;
		}
		else
		{
			area.bounds.Encapsulate(pos);
		}
		return area;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Area AddArea(Vector2i pos, bool noNext)
	{
		pos.x &= -16;
		pos.y &= -16;
		Area area = FindArea(pos);
		if (area == null)
		{
			area = new Area();
			area.pos = pos;
			area.updateDelay = 2f;
			areaList.Add(area);
			return area;
		}
		if (noNext)
		{
			return area;
		}
		if (area.next != null)
		{
			return area.next;
		}
		if (area.updateDelay < 1.5f)
		{
			Area area2 = new Area();
			area2.pos = pos;
			area2.updateDelay = 2f - area.updateDelay;
			area.next = area2;
			return area2;
		}
		return area;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Area FindArea(Vector2i pos)
	{
		for (int i = 0; i < areaList.Count; i++)
		{
			Area area = areaList[i];
			if (area.pos == pos)
			{
				return area;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarVoxelGrid AddGraph(int size)
	{
		AstarVoxelGrid astarVoxelGrid = astar.data.AddGraph(typeof(AstarVoxelGrid)) as AstarVoxelGrid;
		graphList.Add(astarVoxelGrid);
		astarVoxelGrid.Init();
		astarVoxelGrid.neighbours = NumNeighbours.Four;
		astarVoxelGrid.uniformEdgeCosts = false;
		astarVoxelGrid.inspectorGridMode = InspectorGridMode.Grid;
		astarVoxelGrid.characterHeight = 1.8f;
		astarVoxelGrid.SetDimensions(size, size, 1f);
		astarVoxelGrid.maxClimb = 1.3f;
		astarVoxelGrid.maxSlope = 60f;
		astarVoxelGrid.mergeSpanRange = 0.1f;
		GraphCollision collision = astarVoxelGrid.collision;
		collision.collisionCheck = true;
		collision.type = ColliderType.Capsule;
		collision.diameter = 0.3f;
		collision.height = 1.5f;
		collision.collisionOffset = 0.15f;
		collision.mask = 65536;
		return astarVoxelGrid;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AstarVoxelGrid FindClosestGraph(Vector2 pos, int size)
	{
		Vector2 vector = pos - worldOriginXZ;
		AstarVoxelGrid result = null;
		float num = float.MaxValue;
		for (int i = 0; i < graphList.Count; i++)
		{
			AstarVoxelGrid astarVoxelGrid = graphList[i];
			if (!astarVoxelGrid.IsUsed && astarVoxelGrid.size.x >= (float)size)
			{
				Vector2 a = vector;
				a.x -= astarVoxelGrid.center.x;
				a.y -= astarVoxelGrid.center.z;
				float num2 = Vector2.SqrMagnitude(a);
				if (num2 < num)
				{
					num = num2;
					result = astarVoxelGrid;
				}
			}
		}
		return result;
	}
}
