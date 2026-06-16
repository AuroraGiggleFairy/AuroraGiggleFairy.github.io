using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Pathfinding;
using UnityEngine;

public class AstarVoxelGrid : LayerGridGraph
{
	[Flags]
	public enum HitDataFlags : ushort
	{
		None = 0,
		InvalidCover = 1,
		InsideOversized = 2
	}

	public struct HitData
	{
		public Vector3 point;

		public ushort blockerFlags;

		public HitDataFlags hitFlags;
	}

	public class VoxelNode : LevelGridNode, IMemoryPoolableObject
	{
		public int PenaltyHigh;

		public int PenaltyLow;

		public ushort BlockerFlags;

		public void Reset()
		{
		}

		public void Cleanup()
		{
		}

		public override void ClearCustomConnections(bool alsoReverse)
		{
			AstarVoxelGrid.ClearConnections((GridNodeBase)this);
		}

		public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
		{
			handler.heap.Add(pathNode);
			pathNode.UpdateG(path);
			LayerGridGraph gridGraph = LevelGridNode.GetGridGraph(base.GraphIndex);
			int[] neighbourOffsets = gridGraph.neighbourOffsets;
			LevelGridNode[] nodes = gridGraph.nodes;
			int num = base.NodeInGridIndex;
			for (int i = 0; i < 4; i++)
			{
				int connectionValue = GetConnectionValue(i);
				if (connectionValue != 255)
				{
					LevelGridNode levelGridNode = nodes[num + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue];
					PathNode pathNode2 = handler.GetPathNode(levelGridNode);
					if (pathNode2 != null && pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
					{
						levelGridNode.UpdateRecursiveG(path, pathNode2, handler);
					}
				}
			}
			if (connections == null)
			{
				return;
			}
			ushort pathID = handler.PathID;
			for (int j = 0; j < connections.Length; j++)
			{
				if (connections[j].cost != uint.MaxValue)
				{
					GraphNode node = connections[j].node;
					PathNode pathNode3 = handler.GetPathNode(node);
					if (pathNode3.parent == pathNode && pathNode3.pathID == pathID)
					{
						node.UpdateRecursiveG(path, pathNode3, handler);
					}
				}
			}
		}
	}

	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_procedural_grid_mover.php")]
	public class ProceduralGridMover
	{
		public float updateDistance = 10f;

		public Vector3 targetPosition;

		public GridGraph graph;

		[PublicizedFrom(EAccessModifier.Private)]
		public GridNodeBase[] buffer;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool updatingGraph
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 PointToGraphSpace(Vector3 p)
		{
			return graph.transform.InverseTransform(p);
		}

		public void UpdateGraph()
		{
			if (updatingGraph)
			{
				return;
			}
			updatingGraph = true;
			IEnumerator ie = UpdateGraphCoroutine();
			AstarPath.active.AddWorkItem(new AstarWorkItem([PublicizedFrom(EAccessModifier.Internal)] (IWorkItemContext context, bool force) =>
			{
				if (force)
				{
					while (ie.MoveNext())
					{
					}
				}
				bool flag;
				try
				{
					flag = !ie.MoveNext();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					flag = true;
				}
				if (flag)
				{
					updatingGraph = false;
				}
				return flag;
			}));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator UpdateGraphCoroutine()
		{
			Vector3 vector = PointToGraphSpace(targetPosition) - PointToGraphSpace(graph.center);
			vector.x = Mathf.Round(vector.x);
			vector.z = Mathf.Round(vector.z);
			vector.y = 0f;
			if (vector == Vector3.zero)
			{
				yield break;
			}
			Int2 offset = new Int2(-Mathf.RoundToInt(vector.x), -Mathf.RoundToInt(vector.z));
			graph.center = targetPosition;
			graph.UpdateTransform();
			int width = graph.width;
			int depth = graph.depth;
			int layers = graph.LayerCount;
			GridNodeBase[] nodes2;
			if (graph is LayerGridGraph layerGridGraph)
			{
				GridNodeBase[] nodes = layerGridGraph.nodes;
				nodes2 = nodes;
			}
			else
			{
				GridNodeBase[] nodes = graph.nodes;
				nodes2 = nodes;
			}
			if (buffer == null || buffer.Length != width * depth)
			{
				buffer = new GridNodeBase[width * depth];
			}
			int counter;
			int yieldEvery;
			if (Mathf.Abs(offset.x) <= width && Mathf.Abs(offset.y) <= depth)
			{
				IntRect recalculateRect = new IntRect(0, 0, offset.x, offset.y);
				if (recalculateRect.xmin > recalculateRect.xmax)
				{
					int xmax = recalculateRect.xmax;
					recalculateRect.xmax = width + recalculateRect.xmin;
					recalculateRect.xmin = width + xmax;
				}
				if (recalculateRect.ymin > recalculateRect.ymax)
				{
					int ymax = recalculateRect.ymax;
					recalculateRect.ymax = depth + recalculateRect.ymin;
					recalculateRect.ymin = depth + ymax;
				}
				IntRect connectionRect = recalculateRect.Expand(1);
				connectionRect = IntRect.Intersection(connectionRect, new IntRect(0, 0, width, depth));
				int widthStart = width - offset.x;
				int destOffset = offset.x;
				if (offset.x < 0)
				{
					widthStart = -offset.x;
					destOffset += width;
				}
				for (int l = 0; l < layers; l++)
				{
					int num = l * width * depth;
					for (int i = 0; i < depth; i++)
					{
						int num2 = i * width;
						int num3 = (i + offset.y + depth) % depth * width;
						int num4 = num + num2;
						Array.Copy(nodes2, num4, buffer, num3 + destOffset, widthStart);
						Array.Copy(nodes2, num4 + widthStart, buffer, num3, width - widthStart);
					}
					for (int j = 0; j < depth; j++)
					{
						int num5 = j * width;
						for (int k = 0; k < width; k++)
						{
							int num6 = num5 + k;
							GridNodeBase gridNodeBase = buffer[num6];
							if (gridNodeBase != null)
							{
								gridNodeBase.NodeInGridIndex = num6;
							}
							nodes2[num + num6] = gridNodeBase;
						}
						int num7;
						int num8;
						if (j >= recalculateRect.ymin && j < recalculateRect.ymax)
						{
							num7 = 0;
							num8 = depth;
						}
						else
						{
							num7 = recalculateRect.xmin;
							num8 = recalculateRect.xmax;
						}
						for (int m = num7; m < num8; m++)
						{
							buffer[num5 + m]?.ClearConnections(alsoReverse: false);
						}
					}
					if ((l & 7) == 7)
					{
						yield return null;
					}
				}
				yieldEvery = 160;
				int num9 = Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)) * Mathf.Max(width, depth);
				yieldEvery = Mathf.Max(yieldEvery, num9 / 10);
				counter = 0;
				for (int l = 0; l < depth; l++)
				{
					int num10;
					int num11;
					if (l >= recalculateRect.ymin && l < recalculateRect.ymax)
					{
						num10 = 0;
						num11 = width;
					}
					else
					{
						num10 = recalculateRect.xmin;
						num11 = recalculateRect.xmax;
					}
					for (int n = num10; n < num11; n++)
					{
						graph.RecalculateCell(n, l, resetPenalties: false, resetTags: false);
					}
					counter += num11 - num10;
					if (counter > yieldEvery)
					{
						counter = 0;
						yield return null;
					}
				}
				yieldEvery *= 48;
				for (int l = 0; l < depth; l++)
				{
					int num12;
					int num13;
					if (l >= connectionRect.ymin && l < connectionRect.ymax)
					{
						num12 = 0;
						num13 = width;
					}
					else
					{
						num12 = connectionRect.xmin;
						num13 = connectionRect.xmax;
					}
					for (int num14 = num12; num14 < num13; num14++)
					{
						graph.CalculateConnections(num14, l);
					}
					counter += (num13 - num12) * layers;
					if (counter > yieldEvery)
					{
						counter = 0;
						yield return null;
					}
				}
				yield return null;
				for (int num15 = 0; num15 < depth; num15++)
				{
					for (int num16 = 0; num16 < width; num16++)
					{
						if (num16 == 0 || num15 == 0 || num16 == width - 1 || num15 == depth - 1)
						{
							graph.CalculateConnections(num16, num15);
						}
					}
				}
				yield break;
			}
			counter = Mathf.Max(depth * width / 20, 1000);
			yieldEvery = 0;
			for (int destOffset = 0; destOffset < depth; destOffset++)
			{
				for (int num17 = 0; num17 < width; num17++)
				{
					graph.RecalculateCell(num17, destOffset);
				}
				yieldEvery += width;
				if (yieldEvery > counter)
				{
					yieldEvery = 0;
					yield return null;
				}
			}
			for (int destOffset = 0; destOffset < depth; destOffset++)
			{
				for (int num18 = 0; num18 < width; num18++)
				{
					graph.CalculateConnections(num18, destOffset);
				}
				yieldEvery += width;
				if (yieldEvery > counter)
				{
					yieldEvery = 0;
					yield return null;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cGridHeight = 320f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cGridHeightPadded = 320.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1073807360;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLayerMinHeight = 0.7f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cClimbMinHeight = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cClimbMaxHeight = 1.51f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDropOnTopHeight = 0.95f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDropMaxHeight = 9.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cJumpPenalty = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDoorPenalty = 2;

	public const int cPenaltyPerMeter = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPenaltyHealthBase = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPenaltyHealthScale = 20;

	public const uint cDummyPenalty = uint.MaxValue;

	public const int cTagOpen = 0;

	public const int cTagBreak = 1;

	public const int cTagLowHeight = 2;

	public const int cTagDoor = 3;

	public const int cTagLadder = 4;

	public const int cTagTest = 8;

	public const ushort cBlockerFlagLow0 = 1;

	public const ushort cBlockerFlagLow = 15;

	public const ushort cBlockerFlagHigh0 = 16;

	public const ushort cBlockerFlagHigh = 240;

	public const ushort cBlockerFlagHighLow0 = 17;

	public const ushort cBlockerFlagHighLow = 255;

	public const ushort cBlockerFlagSlopeDir0 = 256;

	public const ushort cBlockerFlagFloor = 4096;

	public const ushort cBlockerFlagLadder = 8192;

	public const ushort cBlockerFlagDoor = 16384;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] neighboursOffsetV2 = new Vector2i[4]
	{
		new Vector2i(0, -1),
		new Vector2i(1, 0),
		new Vector2i(0, 1),
		new Vector2i(-1, 0)
	};

	public bool IsUsed;

	public bool IsFullUpdateNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProceduralGridMover gridMover;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRayHitsMax = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HitData[] cellHits = new HitData[512];

	[PublicizedFrom(EAccessModifier.Private)]
	public HitData[] heights = new HitData[512];

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightsUsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryPooledObject<VoxelNode> levelGridNodePool = new MemoryPooledObject<VoxelNode>(100000);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cConnectionPoolMax = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Connection[]>[] connectionsPool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldRayHitInfo localWorldRayHit = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<int> validHeights = new HashSet<int>();

	public Vector2 GridMovePendingPos;

	public void Init()
	{
		if (connectionsPool == null)
		{
			connectionsPool = new List<Connection[]>[16];
			for (int i = 0; i < 16; i++)
			{
				connectionsPool[i] = new List<Connection[]>();
			}
		}
		gridMover = new ProceduralGridMover();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitScan()
	{
		Scan();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerable<Progress> ScanInternal()
	{
		IEnumerator<Progress> scan = base.ScanInternal().GetEnumerator();
		while (scan.MoveNext())
		{
			yield return scan.Current;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateArea(GraphUpdateObject o)
	{
		CalculateAffectedRegions(o, out var _, out var affectRect, out var physicsRect, out var _, out var _);
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect = IntRect.Intersection(affectRect, b);
		collision.Initialize(base.transform, nodeSize);
		intRect = IntRect.Intersection(physicsRect, b);
		for (int i = intRect.xmin; i <= intRect.xmax; i++)
		{
			for (int j = intRect.ymin; j <= intRect.ymax; j++)
			{
				RecalculateCell(i, j, resetPenalties: true, resetTags: false);
			}
		}
		affectRect.Expand(1);
		intRect = IntRect.Intersection(affectRect, b);
		for (int k = intRect.xmin; k <= intRect.xmax; k++)
		{
			for (int l = intRect.ymin; l <= intRect.ymax; l++)
			{
				CalculateConnections(k, l);
			}
		}
	}

	[Conditional("DEBUG_PATHGRIDVALIDATE")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Validate()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SampleHeights(Vector3 pos)
	{
		CheckHeights(pos);
		int num = heightsUsed / 2;
		for (int i = 0; i < num; i++)
		{
			HitData hitData = heights[i];
			heights[i] = heights[heightsUsed - 1 - i];
			heights[heightsUsed - 1 - i] = hitData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckHeights(Vector3 position)
	{
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		Vector3 position2 = Origin.position;
		int type = BlockValue.Air.type;
		heightsUsed = 0;
		int num = 0;
		Vector3 vector = position;
		vector.y += 320f;
		PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
		Vector3 down = Vector3.down;
		float num2 = float.MaxValue;
		HitData hitData = default(HitData);
		while (true)
		{
			hitData.hitFlags = HitDataFlags.None;
			hitData.blockerFlags = 4096;
			if (!defaultPhysicsScene.Raycast(vector, down, out var hitInfo, 320.01f, 1073807360, QueryTriggerInteraction.Ignore))
			{
				break;
			}
			vector.y = hitInfo.point.y - 0.11f;
			if (hitInfo.point.y > num2)
			{
				hitData.hitFlags |= HitDataFlags.InsideOversized;
			}
			else if (hitInfo.collider.CompareTag("T_Block"))
			{
				localWorldRayHit.Clear();
				Transform hitTransform = hitInfo.collider.transform;
				Vector3 hitPointPos = hitInfo.point + Origin.position;
				if (GameUtils.FindMasterBlockForEntityModelBlock(world, down, string.Empty, hitPointPos, hitTransform, localWorldRayHit))
				{
					BlockValue blockValue = localWorldRayHit.hit.blockValue;
					if (blockValue.Block.isOversized)
					{
						hitData.hitFlags |= HitDataFlags.InsideOversized;
						Quaternion quaternion = blockValue.Block.shape.GetRotation(blockValue);
						OversizedBlockUtils.GetWorldAlignedBoundsExtents(localWorldRayHit.hit.blockPos, quaternion, blockValue.Block.oversizedBounds, out var min, out var _);
						num2 = min.y;
					}
				}
			}
			hitData.point = hitInfo.point;
			hitData.point.y += 0.05f;
			if (Vector3.Dot(hitInfo.normal, Vector3.up) < 0.707f)
			{
				hitData.hitFlags |= HitDataFlags.InvalidCover;
			}
			cellHits[num] = hitData;
			if (++num >= 512)
			{
				Log.Warning("AstarVoxelGrid CheckHeights too many hits");
				break;
			}
		}
		int num3 = Utils.Fastfloor(position.x + position2.x);
		int num4 = Utils.Fastfloor(position.z + position2.z);
		Vector3i vector3i = new Vector3i(num3, 0, num4);
		Vector3i vector3i2 = vector3i;
		IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(vector3i);
		if (chunkFromWorldPos == null)
		{
			return;
		}
		int x = World.toBlockXZ(num3);
		int z = World.toBlockXZ(num4);
		int num5 = 0;
		float num6 = 257f;
		Vector3 origin = default(Vector3);
		while (num5 < num)
		{
			float num7 = num6;
			HitData hitData2 = cellHits[num5++];
			num6 = hitData2.point.y;
			float num8 = num7 - num6;
			vector3i.y = Utils.Fastfloor(num6 + position2.y);
			BlockValue block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
			int type2 = block.type;
			Block block2 = block.Block;
			if (block2.shape.IsTerrain())
			{
				heights[heightsUsed++] = hitData2;
				continue;
			}
			if (num8 > 0.95f)
			{
				if (block2.PathType > 0)
				{
					float num9 = Utils.Fastfloor(hitData2.point.y);
					if (hitData2.point.y - num9 > 0.4f)
					{
						vector3i.y++;
						block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
						type2 = block.type;
						block2 = block.Block;
						num6 = num9 + 1.01f;
						hitData2.point.y = num6;
					}
				}
				if (block2.PathType > 0)
				{
					hitData2.blockerFlags = 4111;
				}
				else
				{
					if (type2 != type)
					{
						hitData2.blockerFlags |= CalcBlockingFlags(hitData2.point, 0.2f);
						Vector2 pathOffset = block2.GetPathOffset(block.rotation);
						hitData2.point.x += pathOffset.x;
						hitData2.point.z += pathOffset.y;
					}
					vector3i2.y = vector3i.y + 1;
					BlockValue block3 = GetBlock(chunkFromWorldPos, x, vector3i2.y, z);
					Block block4 = block3.Block;
					bool flag = block2.HasTag(BlockTags.Door) || block2.HasTag(BlockTags.ClosetDoor);
					bool flag2 = false;
					if (flag && block2.MaxDamagePlusDowngrades - block.damage == 1 && block2.shape is BlockShapeModelEntity blockShapeModelEntity && !blockShapeModelEntity.IsObstructionForDamageState(block))
					{
						flag2 = true;
					}
					if (!flag2)
					{
						if (flag)
						{
							if (!block2.isMultiBlock || !block.ischild || block.parenty == 0)
							{
								hitData2.blockerFlags |= 16384;
							}
						}
						else if (block4.HasTag(BlockTags.Door) && (!block4.isMultiBlock || !block3.ischild || block3.parenty == 0))
						{
							hitData2.blockerFlags |= 16384;
						}
					}
					if (num8 > 2.95f && (block2.IsElevator(block.rotation) || block4.IsElevator(block3.rotation)))
					{
						hitData2.blockerFlags |= 8192;
						Vector3i vector3i3 = vector3i2;
						BlockValue blockValue2 = block3;
						Block block5 = block4;
						int num10 = (int)(num7 - 1f + position2.y);
						int num11 = 0;
						while (vector3i3.y <= num10)
						{
							if (block5.IsElevator(blockValue2.rotation))
							{
								num11 = 0;
							}
							else
							{
								if (!blockValue2.isair || num11 >= 1)
								{
									break;
								}
								num11++;
							}
							vector3i3.y++;
							blockValue2 = GetBlock(chunkFromWorldPos, x, vector3i3.y, z);
							block5 = blockValue2.Block;
						}
						vector3i3.y -= num11;
						HitData hitData3 = default(HitData);
						Vector3 pos = vector;
						float num12 = num6 + position2.y - -0.2f;
						while ((float)vector3i3.y > num12)
						{
							pos.y = (float)vector3i3.y - position2.y;
							hitData3.blockerFlags = (ushort)(0x2000 | CalcBlockingFlags(pos, 0f));
							hitData3.point.x = pos.x;
							hitData3.point.z = pos.z;
							hitData3.point.y = pos.y + -0.2f;
							hitData3.hitFlags |= HitDataFlags.InvalidCover;
							heights[heightsUsed++] = hitData3;
							vector3i3.y--;
						}
					}
				}
				heights[heightsUsed++] = hitData2;
			}
			else
			{
				num6 = num7;
			}
			float num13 = float.MinValue;
			if (num5 < num)
			{
				num13 = cellHits[num5].point.y;
			}
			while (true)
			{
				vector3i.y--;
				vector.y = (float)vector3i.y - position2.y;
				if (vector.y <= num13)
				{
					break;
				}
				if (vector3i.y < 0)
				{
					num5 = int.MaxValue;
					break;
				}
				block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
				type2 = block.type;
				if (type2 == type)
				{
					break;
				}
				block2 = block.Block;
				if (block2.shape.IsTerrain() || block2.IsElevator())
				{
					break;
				}
				if (block2.HasTag(BlockTags.Door) || block2.PathType <= 0 || !block2.IsMovementBlocked(world, vector3i, block, BlockFace.Top))
				{
					continue;
				}
				bool flag3 = true;
				for (int i = 0; i < 4; i++)
				{
					Vector2i vector2i = neighboursOffsetV2[i];
					Vector3i vector3i4 = vector3i;
					vector3i4.x += vector2i.x;
					vector3i4.z += vector2i.y;
					block = chunkCache.GetBlock(vector3i4);
					block2 = block.Block;
					if (block2.PathType > 0 && block2.IsMovementBlocked(world, vector3i4, block, BlockFace.Top))
					{
						continue;
					}
					vector3i4.y--;
					block = chunkCache.GetBlock(vector3i4);
					block2 = block.Block;
					if (block2.PathType > 0)
					{
						flag3 = false;
					}
					else if (block2.IsMovementBlocked(world, vector3i4, block, BlockFace.Top))
					{
						origin.x = (float)vector3i4.x - position2.x;
						origin.y = vector.y + 0.51f;
						origin.z = (float)vector3i4.z - position2.z;
						if (defaultPhysicsScene.Raycast(origin, down, out var _, 1.6f, 1073807360, QueryTriggerInteraction.Ignore))
						{
							flag3 = false;
						}
						break;
					}
				}
				if (!flag3)
				{
					hitData2.point.y = vector.y + 0.03f;
					hitData2.blockerFlags = 4111;
					heights[heightsUsed++] = hitData2;
				}
			}
		}
	}

	public override void RecalculateCell(int x, int z, bool resetPenalties = true, bool resetTags = true)
	{
		World world = GameManager.Instance.World;
		if (world == null || world.ChunkCache == null)
		{
			return;
		}
		Vector3 pos = base.transform.Transform(new Vector3((float)x + 0.5f, 0f, (float)z + 0.5f));
		SampleHeights(pos);
		if (heightsUsed > layerCount)
		{
			if (heightsUsed > 255)
			{
				UnityEngine.Debug.LogError("Too many layers " + heightsUsed);
				return;
			}
			AddLayers(heightsUsed - layerCount);
		}
		Vector3 position = Origin.position;
		int num = Utils.Fastfloor(pos.x + position.x);
		int num2 = Utils.Fastfloor(pos.z + position.z);
		Vector3i vector3i = new Vector3i(num, 0, num2);
		IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(vector3i);
		if (chunkFromWorldPos == null)
		{
			return;
		}
		int x2 = World.toBlockXZ(num);
		int z2 = World.toBlockXZ(num2);
		int num3 = width * depth;
		int num4 = x + z * width;
		int i = 0;
		validHeights.Clear();
		for (; i < heightsUsed; i++)
		{
			int num5 = num4 + num3 * i;
			VoxelNode voxelNode = (VoxelNode)nodes[num5];
			if (voxelNode == null)
			{
				voxelNode = (VoxelNode)(nodes[num5] = levelGridNodePool.Alloc(_bReset: false));
				voxelNode.Init(active);
				voxelNode.NodeInGridIndex = num4;
				voxelNode.LayerCoordinateInGrid = i;
				voxelNode.GraphIndex = graphIndex;
			}
			Vector3 point = heights[i].point;
			voxelNode.position = (Int3)point;
			vector3i.y = Utils.Fastfloor(point.y + position.y);
			validHeights.Add(vector3i.y);
			voxelNode.ClearCustomConnections(alsoReverse: true);
			voxelNode.Walkable = true;
			int num6 = 0;
			int num7 = 0;
			voxelNode.PenaltyHigh = 0;
			voxelNode.PenaltyLow = 0;
			voxelNode.BlockerFlags = heights[i].blockerFlags;
			if ((heights[i].hitFlags & HitDataFlags.InsideOversized) != HitDataFlags.None)
			{
				voxelNode.Walkable = false;
			}
			if ((voxelNode.BlockerFlags & 0x4000) > 0)
			{
				voxelNode.Tag = 3u;
				voxelNode.Penalty = (uint)num6;
				BlockValue block = GetBlock(chunkFromWorldPos, x2, vector3i.y, z2);
				int num8 = block.Block.MaxDamagePlusDowngrades - block.damage;
				voxelNode.PenaltyLow = (num8 + 10) * 20 / 3;
				continue;
			}
			vector3i.y++;
			BlockValue block2;
			Block block3;
			if (point.y - (float)Utils.Fastfloor(point.y) > 0.4f)
			{
				vector3i.y++;
				block2 = GetBlock(chunkFromWorldPos, x2, vector3i.y, z2);
				block3 = block2.Block;
				if (block3.IsMovementBlocked(world, vector3i, block2, BlockFace.None))
				{
					int num9 = block3.MaxDamagePlusDowngrades - block2.damage;
					voxelNode.PenaltyHigh = (num9 + 10) * 20;
					if (block3.PathType > 0)
					{
						num6 += voxelNode.PenaltyHigh;
						voxelNode.BlockerFlags |= 240;
					}
					else
					{
						int num10 = CalcBlockingFlags(point, 1.5f);
						voxelNode.BlockerFlags |= (ushort)((num10 & 0xF) << 4);
					}
				}
				vector3i.y--;
			}
			block2 = GetBlock(chunkFromWorldPos, x2, vector3i.y, z2);
			block3 = block2.Block;
			bool flag = false;
			if (block3.IsMovementBlocked(world, vector3i, block2, BlockFace.None))
			{
				int num11 = block3.MaxDamagePlusDowngrades - block2.damage;
				voxelNode.PenaltyHigh += (num11 + 10) * 20;
				if (block3.PathType > 0)
				{
					num6 += voxelNode.PenaltyHigh;
					voxelNode.BlockerFlags |= 240;
				}
				else
				{
					bool flag2 = false;
					int num12 = i + 1;
					if (num12 < heightsUsed && Utils.Fastfloor(heights[num12].point.y + position.y) == vector3i.y)
					{
						flag2 = true;
						int blockerFlags = heights[num12].blockerFlags;
						if ((blockerFlags & 0x1000) > 0)
						{
							num6 += voxelNode.PenaltyHigh;
							voxelNode.BlockerFlags |= 240;
						}
						else
						{
							voxelNode.BlockerFlags |= (ushort)((((blockerFlags >> 8) | blockerFlags) & 0xF) << 4);
						}
					}
					if (!flag2)
					{
						int num13 = CalcBlockingFlags(point, 1f);
						voxelNode.BlockerFlags |= (ushort)((num13 & 0xF) << 4);
					}
				}
			}
			vector3i.y--;
			block2 = GetBlock(chunkFromWorldPos, x2, vector3i.y, z2);
			block3 = block2.Block;
			if (block3.IsMovementBlocked(world, vector3i, block2, BlockFace.None))
			{
				int num14 = block3.MaxDamagePlusDowngrades - block2.damage;
				voxelNode.PenaltyLow = (num14 + 10) * 20;
				if (block3.PathType > 0)
				{
					num6 += voxelNode.PenaltyLow;
				}
			}
			if (num7 > 0)
			{
				voxelNode.Tag = 1u;
			}
			else if (flag)
			{
				voxelNode.Tag = 2u;
			}
			else
			{
				voxelNode.Tag = 0u;
			}
			if (block3.IsElevator(block2.rotation))
			{
				voxelNode.Tag = 4u;
			}
			if ((uint)num6 > 268435455u)
			{
				Log.Warning("RecalculateCell {0}, id{1} {2}, pen {3}", vector3i, block3.blockID, block3.GetBlockName(), num6);
				num6 = ((num6 >= 0) ? 268435455 : 0);
			}
			voxelNode.Penalty = (uint)num6;
		}
		int num15 = num4 + num3 * i;
		for (; i < layerCount; i++)
		{
			LevelGridNode levelGridNode = nodes[num15];
			if (levelGridNode != null)
			{
				levelGridNode.Destroy();
				nodes[num15] = null;
				levelGridNodePool.Free((VoxelNode)levelGridNode);
			}
			num15 += num3;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort CalcBlockingFlags(Vector3 pos, float offsetY)
	{
		PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
		int num = 0;
		pos.y += 0.2f + offsetY;
		Vector3 vector = default(Vector3);
		vector.y = 0f;
		for (int i = 0; i < 4; i++)
		{
			Vector2i vector2i = neighboursOffsetV2[i];
			vector.x = vector2i.x;
			vector.z = vector2i.y;
			Vector3 origin = pos - vector * 0.2f;
			if (defaultPhysicsScene.SphereCast(origin, 0.1f, vector, out var hitInfo, 0.59f, 1073807360, QueryTriggerInteraction.Ignore))
			{
				num = ((!(offsetY > 0.5f) && !(hitInfo.normal.y < 0.643f)) ? ((!(Vector3.Dot(vector, hitInfo.normal) > -0.35f)) ? (num | (256 << i)) : (num | (1 << i))) : (num | (1 << i)));
			}
		}
		return (ushort)num;
	}

	public override void CalculateConnections(int x, int z, int layerIndex)
	{
		int num = width * depth;
		int num2 = z * width + x + num * layerIndex;
		VoxelNode voxelNode = (VoxelNode)nodes[num2];
		if (voxelNode == null)
		{
			return;
		}
		voxelNode.ResetAllGridConnections();
		if (!voxelNode.Walkable)
		{
			return;
		}
		Vector3 vector = (Vector3)voxelNode.position;
		World.worldToBlockPos(vector + Origin.position);
		float y = vector.y;
		float num3 = y + characterHeight;
		vector.y += 0.5f;
		Vector3i pos = World.worldToBlockPos(vector + Origin.position);
		World world = GameManager.Instance.World;
		world.GetBlock(pos);
		if ((voxelNode.BlockerFlags & 0x2000) > 0 && layerIndex + 1 < layerCount)
		{
			LevelGridNode levelGridNode = nodes[num2 + num];
			if (levelGridNode != null && (float)levelGridNode.position.y * 0.001f - y < 2.1f)
			{
				AddConnection(voxelNode, levelGridNode, 500u, 4u, null);
				AddConnection(levelGridNode, voxelNode, 250u, 4u, null);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			Vector2i vector2i = neighboursOffsetV2[i];
			int num4 = x + vector2i.x;
			if ((uint)num4 >= width)
			{
				continue;
			}
			int num5 = z + vector2i.y;
			if ((uint)num5 >= depth)
			{
				continue;
			}
			int num6 = num5 * width + num4;
			int num7 = 255;
			float num8 = 0f;
			for (int j = 0; j < layerCount; j++)
			{
				int num9 = num6 + j * num;
				VoxelNode voxelNode2 = (VoxelNode)nodes[num9];
				if (voxelNode2 == null || !voxelNode2.Walkable)
				{
					continue;
				}
				float num10 = (float)voxelNode2.position.y * 0.001f;
				float num11;
				if (j == layerCount - 1 || nodes[num9 + num] == null)
				{
					num11 = float.PositiveInfinity;
				}
				else
				{
					num11 = (float)nodes[num9 + num].position.y * 0.001f - num10;
					if (num11 <= -0.001f)
					{
						LevelGridNode levelGridNode2 = nodes[num9 + num];
						Utils.DrawLine((Vector3)voxelNode.position, (Vector3)voxelNode2.position, new Color(1f, 0f, 1f), new Color(1f, 0.5f, 0f), 3, 5f);
						Utils.DrawLine((Vector3)voxelNode2.position, (Vector3)levelGridNode2.position, new Color(1f, 0f, 0f), new Color(1f, 1f, 0f), 2, 5f);
						Log.Warning("Path node otherHeight bad {0}, {1}, {2}", num11, levelGridNode2.position, voxelNode2.position);
					}
				}
				float num12 = num10 - y;
				if (num12 < -0.1f)
				{
					if (num12 >= -9.4f && num3 < num10 + num11)
					{
						num7 = j;
						num8 = num12;
					}
					continue;
				}
				if (num12 >= 1.51f)
				{
					break;
				}
				if (!(num11 >= 0.7f))
				{
					continue;
				}
				if (num12 >= 0.6f)
				{
					if ((voxelNode.BlockerFlags & 0xF) == 15 || (voxelNode.BlockerFlags & (16 << i)) != 0 || (voxelNode2.BlockerFlags & (17 << (i ^ 2))) != 0)
					{
						continue;
					}
					if (num12 >= 1.05f || (voxelNode2.BlockerFlags & (256 << i)) == 0)
					{
						if ((voxelNode2.BlockerFlags & (256 << (i ^ 2))) == 0)
						{
							AddConnection(voxelNode, voxelNode2, (uint)(num12 * 8000f), 0u, null);
							AddDummyConnection(voxelNode2, voxelNode);
							if ((voxelNode.BlockerFlags & 0x2000) == 0)
							{
								num7 = 255;
							}
							break;
						}
					}
					else
					{
						num7 = j;
						num8 = 0f;
					}
					continue;
				}
				if (voxelNode2.Tag == 3)
				{
					Vector3i vector3i = World.worldToBlockPos((Vector3)voxelNode2.position + Origin.position);
					Block block = world.GetBlock(vector3i).Block;
					TEFeatureDoor _typedTe = null;
					if (block.HasTag(BlockTags.Door) && block is BlockCompositeTileEntity && world.GetTileEntity(vector3i) is TileEntityComposite te)
					{
						te.TryGetSelfOrFeature<TEFeatureDoor>(out _typedTe);
					}
					AddConnection(voxelNode, voxelNode2, 2000u, 3u, _typedTe);
					AddDummyConnection(voxelNode2, voxelNode);
				}
				if ((voxelNode2.BlockerFlags & 0x3000) > 0)
				{
					num7 = j;
					num8 = 0f;
				}
			}
			if (num7 == 255)
			{
				continue;
			}
			int num13 = num6 + num7 * num;
			VoxelNode voxelNode3 = (VoxelNode)nodes[num13];
			bool flag = false;
			int num14 = voxelNode.BlockerFlags & 0xF;
			if (num14 == 0 || num14 == 15)
			{
				num14 = voxelNode.BlockerFlags & 0xF0;
				if (num14 == 0 || num14 == 240)
				{
					num14 = voxelNode3.BlockerFlags & 0xF;
					if (num8 <= -0.95f || num14 == 0 || num14 == 15)
					{
						num14 = voxelNode3.BlockerFlags & 0xF0;
						if (num14 == 0 || num14 == 240)
						{
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				int num15 = 0;
				if ((voxelNode.BlockerFlags & (16 << i)) > 0)
				{
					num15 += voxelNode.PenaltyHigh;
				}
				if ((voxelNode3.BlockerFlags & (16 << (i ^ 2))) > 0)
				{
					num15 += voxelNode3.PenaltyHigh;
				}
				if ((voxelNode.BlockerFlags & (1 << i)) > 0)
				{
					num15 += voxelNode.PenaltyLow;
				}
				if ((voxelNode3.BlockerFlags & (1 << (i ^ 2))) > 0 && num8 > -0.95f)
				{
					num15 += voxelNode3.PenaltyLow;
				}
				if (num15 > 0)
				{
					AddConnection(voxelNode, voxelNode3, (uint)num15, 1u, null);
					AddDummyConnection(voxelNode3, voxelNode);
					num7 = 255;
				}
			}
			voxelNode.SetConnectionValue(i, num7);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddConnection(GridNodeBase node, GridNodeBase other, uint cost, uint tag, object payload)
	{
		Connection[] connections = node.connections;
		int num = 0;
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].node == other)
				{
					connections[i].cost = cost;
					connections[i].tag = tag;
					connections[i].payload = payload;
					return;
				}
			}
			num = connections.Length;
		}
		num++;
		Connection[] array = AllocConnection(num);
		for (int j = 0; j < num - 1; j++)
		{
			array[j] = connections[j];
		}
		array[num - 1] = new Connection(other, cost, tag, payload);
		node.connections = array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddDummyConnection(GridNodeBase node, GridNodeBase other)
	{
		Connection[] connections = node.connections;
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].node == other)
				{
					return;
				}
			}
		}
		AddConnection(node, other, uint.MaxValue, 0u, null);
	}

	public static void ClearConnections(GridNodeBase node)
	{
		Connection[] connections = node.connections;
		if (connections != null)
		{
			node.connections = null;
			int num = connections.Length;
			for (int i = 0; i < num; i++)
			{
				RemoveConnection((GridNodeBase)connections[i].node, node);
			}
			if (num < 16)
			{
				connectionsPool[num].Add(connections);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RemoveConnection(GridNodeBase node, GridNodeBase other)
	{
		Connection[] connections = node.connections;
		if (connections == null)
		{
			return;
		}
		int num = connections.Length;
		for (int i = 0; i < num; i++)
		{
			if (connections[i].node != other)
			{
				continue;
			}
			if (num <= 1)
			{
				node.connections = null;
			}
			else
			{
				Connection[] array = AllocConnection(num - 1);
				int j;
				for (j = 0; j < i; j++)
				{
					array[j] = connections[j];
				}
				for (; j < array.Length; j++)
				{
					array[j] = connections[j + 1];
				}
				node.connections = array;
			}
			if (num < 16)
			{
				connectionsPool[num].Add(connections);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Connection[] AllocConnection(int count)
	{
		Connection[] array = null;
		if (count < 16)
		{
			List<Connection[]> list = connectionsPool[count];
			int count2 = list.Count;
			if (count2 > 0)
			{
				array = list[count2 - 1];
				list.RemoveAt(count2 - 1);
			}
		}
		if (array == null)
		{
			array = new Connection[count];
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue GetBlock(IChunk chunk, int _x, int _y, int _z)
	{
		if ((uint)_y >= 256u)
		{
			return BlockValue.Air;
		}
		return chunk.GetBlock(_x, _y, _z);
	}

	public void SetPos(Vector3 pos)
	{
		center = pos;
		IsFullUpdateNeeded = false;
		InitScan();
	}

	public void Move(Vector3 targetPos)
	{
		gridMover.graph = this;
		gridMover.targetPosition = targetPos;
		gridMover.UpdateGraph();
	}

	public bool IsMoving()
	{
		return gridMover.updatingGraph;
	}
}
