using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StabilityCalculator
{
	public class UpdatePhysics : IEnumerator<object>, IEnumerator, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public MicroStopwatch sw;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i curPosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public int state;

		[PublicizedFrom(EAccessModifier.Private)]
		public object myEnumerator;

		[PublicizedFrom(EAccessModifier.Private)]
		public StabilityCalculator physicsCalculator;

		object IEnumerator<object>.Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return myEnumerator;
			}
		}

		object IEnumerator.Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return myEnumerator;
			}
		}

		public UpdatePhysics(StabilityCalculator _sp)
		{
			physicsCalculator = _sp;
			sw = MicroStopwatch.a();
		}

		public bool MoveNext()
		{
			int num = state;
			state = -1;
			if ((uint)num > 1u)
			{
				return false;
			}
			if (!GameStats.GetBool(EnumGameStats.ChunkStabilityEnabled))
			{
				myEnumerator = new WaitForSeconds(physicsCalculator.updatePeriod);
				state = 1;
				return true;
			}
			if (!physicsCalculator.bRunning)
			{
				state = -1;
				return false;
			}
			sw.ResetAndRestart();
			while (sw.ElapsedMicroseconds < physicsCalculator.updateTimeLimit && physicsCalculator.queueStabilityEmpty.Count > 0)
			{
				curPosition = physicsCalculator.queueStabilityEmpty.Dequeue();
				physicsCalculator.physicsIsolation(curPosition);
				foreach (Vector3i item in physicsCalculator.hashSetIsolation)
				{
					((World)world).AddFallingBlock(item);
				}
			}
			while (sw.ElapsedMicroseconds < physicsCalculator.updateTimeLimit && physicsCalculator.queueStabilityAvail.Count > 0)
			{
				Vector3i pos = physicsCalculator.queueStabilityAvail.Dequeue();
				float calculatedStability;
				List<Vector3i> list = physicsCalculator.CalcPhysicsStabilityToFall(pos, 20, out calculatedStability);
				if (list != null)
				{
					physicsCalculator.addToFallingBlocks(list);
				}
			}
			sw.Stop();
			state = 1;
			return true;
		}

		public void Dispose()
		{
			state = -1;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldBase world;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChannelCalculator channelCalculator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ienumCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxIterations = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updatePeriod = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int isolatedBlockLimit = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int stabilityQueueLimit = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public int updateTimeLimit = 3000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueStabilityAvail;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueStabilityEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> stab0Positions = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> hashSetIsolation = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> hashSetProcessed = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> queueIsolation = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<Vector3i> posChecked = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> posToCheck = new List<Vector3i>(6);

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3i> posToCheckNext = new List<Vector3i>(6);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, int> posPlaced = new Dictionary<Vector3i, int>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInfiniteSupport = 100000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSupportScale = 1.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> unstablePositions = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> positionsToCheck = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> uniqueUnstablePositions = new Queue<Vector3i>();

	public void Init(WorldBase _world)
	{
		bRunning = true;
		world = _world;
		channelCalculator = new ChannelCalculator(world);
		queueStabilityAvail = new Queue<Vector3i>();
		queueStabilityEmpty = new Queue<Vector3i>();
		ienumCoroutine = new UpdatePhysics(this);
		ThreadManager.StartCoroutine(ienumCoroutine);
	}

	public void Cleanup()
	{
		bRunning = false;
		if (ienumCoroutine != null)
		{
			ThreadManager.StopCoroutine(ienumCoroutine);
			ienumCoroutine = null;
		}
		channelCalculator = null;
	}

	public void BlockRemovedAt(Vector3i _pos)
	{
		if (_pos.y >= 255)
		{
			return;
		}
		stab0Positions.Clear();
		channelCalculator.BlockRemovedAt(_pos, stab0Positions);
		if (world.IsRemote())
		{
			return;
		}
		IChunk _chunk = null;
		Vector3i[] allDirections = Vector3i.AllDirections;
		foreach (Vector3i vector3i in allDirections)
		{
			Vector3i vector3i2 = _pos + vector3i;
			if (!world.GetChunkFromWorldPos(vector3i2, ref _chunk))
			{
				continue;
			}
			int x = World.toBlockXZ(vector3i2.x);
			int y = World.toBlockY(vector3i2.y);
			int z = World.toBlockXZ(vector3i2.z);
			BlockValue blockNoDamage;
			BlockValue blockValue = (blockNoDamage = _chunk.GetBlockNoDamage(x, y, z));
			if (blockValue.isair || blockNoDamage.Block.blockMaterial.IsLiquid)
			{
				continue;
			}
			int stability = _chunk.GetStability(x, y, z);
			if (stability == 0)
			{
				if (!stab0Positions.Contains(vector3i2))
				{
					queueStabilityEmpty.Enqueue(vector3i2);
				}
			}
			else if (stability < 15 && queueStabilityAvail.Count < 200)
			{
				queueStabilityAvail.Enqueue(vector3i2);
			}
		}
		foreach (Vector3i stab0Position in stab0Positions)
		{
			queueStabilityEmpty.Enqueue(stab0Position);
		}
	}

	public void BlockPlacedAt(Vector3i _pos, bool _bForceFullStabe = false)
	{
		channelCalculator.BlockPlacedAt(_pos, _bForceFullStabe);
		if (!world.IsRemote() && queueStabilityAvail.Count < 200)
		{
			queueStabilityAvail.Enqueue(_pos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void physicsIsolation(Vector3i _pos)
	{
		queueIsolation.Clear();
		queueIsolation.Enqueue(_pos);
		hashSetIsolation.Clear();
		hashSetIsolation.Add(_pos);
		hashSetProcessed.Clear();
		hashSetProcessed.Add(_pos);
		IChunk _chunk = null;
		Vector3i zero = Vector3i.zero;
		while (queueIsolation.Count > 0)
		{
			Vector3i vector3i = queueIsolation.Dequeue();
			Vector3i[] allDirections = Vector3i.AllDirections;
			for (int i = 0; i < allDirections.Length; i++)
			{
				zero = vector3i + allDirections[i];
				if (!world.GetChunkFromWorldPos(zero, ref _chunk))
				{
					continue;
				}
				Vector3i vector3i2 = World.toBlock(zero);
				BlockValue blockNoDamage = _chunk.GetBlockNoDamage(vector3i2.x, vector3i2.y, vector3i2.z);
				if (blockNoDamage.isair || blockNoDamage.Block.blockMaterial.IsLiquid || blockNoDamage.Block.StabilityIgnore || hashSetProcessed.Contains(zero))
				{
					continue;
				}
				hashSetProcessed.Add(zero);
				if (_chunk.GetStability(vector3i2.x, vector3i2.y, vector3i2.z) > 0)
				{
					continue;
				}
				if (!blockNoDamage.ischild)
				{
					hashSetIsolation.Add(zero);
					if (hashSetIsolation.Count >= 1000)
					{
						return;
					}
				}
				queueIsolation.Enqueue(zero);
			}
		}
	}

	public static float GetBlockStability(Vector3i _pos)
	{
		float blockStability = GetBlockStability(_pos, BlockValue.Air);
		posChecked.Clear();
		posToCheck.Clear();
		posToCheckNext.Clear();
		return blockStability;
	}

	public static float GetBlockStabilityIfPlaced(Vector3i _pos, BlockValue _bv)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		Block block = _bv.Block;
		int value = (block.StabilitySupport ? 1 : 0);
		float num2;
		if (block.isMultiBlock)
		{
			int type = _bv.type;
			int rotation = _bv.rotation;
			Vector3i v = Vector3i.max;
			Vector3i v2 = Vector3i.min;
			for (int num = block.multiBlockPos.Length - 1; num >= 0; num--)
			{
				Vector3i vector3i = block.multiBlockPos.Get(num, type, rotation) + _pos;
				if (!posPlaced.ContainsKey(vector3i))
				{
					posPlaced.Add(vector3i, value);
					v = Vector3i.Min(v, vector3i);
					v2 = Vector3i.Max(v2, vector3i);
				}
			}
			IChunk _chunk = null;
			Vector3i key = default(Vector3i);
			for (int i = v.z; i <= v2.z; i++)
			{
				key.z = i;
				for (int j = v.x; j <= v2.x; j++)
				{
					key.x = j;
					world.GetChunkFromWorldPos(j, i, ref _chunk);
					if (_chunk != null && _chunk.GetStability(j & 0xF, v.y - 1, i & 0xF) == 15)
					{
						for (int k = v.y; k <= v2.y; k++)
						{
							key.y = k;
							posPlaced[key] = 15;
						}
					}
				}
			}
			num2 = 1f;
			foreach (Vector3i key2 in posPlaced.Keys)
			{
				if (key2.x != v.x && key2.x != v2.x && key2.y != v.y && key2.y != v2.y && key2.z != v.z && key2.z != v2.z)
				{
					continue;
				}
				float blockStability = GetBlockStability(key2, _bv);
				if (blockStability < num2)
				{
					num2 = blockStability;
					if (blockStability == 0f)
					{
						break;
					}
				}
				if (microStopwatch.ElapsedMicroseconds > 25000)
				{
					break;
				}
			}
		}
		else
		{
			posPlaced.Add(_pos, value);
			num2 = GetBlockStability(_pos, _bv);
		}
		posPlaced.Clear();
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetBlockStability(Vector3i _pos, BlockValue _newBV)
	{
		posChecked.Clear();
		posToCheck.Clear();
		posToCheckNext.Clear();
		if (!Block.BlocksLoaded)
		{
			return 1f;
		}
		if (!GameManager.bPhysicsActive)
		{
			return 1f;
		}
		BlockValue blockValue = (posPlaced.ContainsKey(_pos) ? _newBV : world.GetBlock(_pos));
		int num = 0;
		int num2 = 0;
		posChecked.Add(_pos);
		posToCheck.Add(_pos);
		int num3 = 0;
		int num4 = 0;
		float num5 = 0f;
		new Vector3(0f, -1f, 0f);
		IChunk _chunk = null;
		while (num4 < 25 && posToCheck.Count > 0)
		{
			num4++;
			int num6 = num;
			for (int i = 0; i < posToCheck.Count; i++)
			{
				Vector3i vector3i = posToCheck[i];
				if (posPlaced.TryGetValue(vector3i, out var _))
				{
					blockValue = _newBV;
				}
				else
				{
					world.GetChunkFromWorldPos(vector3i.x, vector3i.z, ref _chunk);
					if (_chunk == null)
					{
						continue;
					}
					blockValue = _chunk.GetBlockNoDamage(vector3i.x & 0xF, vector3i.y, vector3i.z & 0xF);
				}
				num3 += 7;
				num2 += blockValue.Block.blockMaterial.Mass.Value;
				Vector3i[] allDirectionsShuffled = Vector3i.AllDirectionsShuffled;
				for (int j = 0; j < allDirectionsShuffled.Length; j++)
				{
					Vector3i vector3i2 = vector3i + allDirectionsShuffled[j];
					if (vector3i2.y < 0)
					{
						continue;
					}
					BlockValue other;
					if (posPlaced.TryGetValue(vector3i2, out var value2))
					{
						other = _newBV;
					}
					else
					{
						world.GetChunkFromWorldPos(vector3i2.x, vector3i2.z, ref _chunk);
						if (_chunk == null)
						{
							continue;
						}
						int x = vector3i2.x & 0xF;
						int z = vector3i2.z & 0xF;
						other = _chunk.GetBlockNoDamage(x, vector3i2.y, z);
						value2 = _chunk.GetStability(x, vector3i2.y, z);
					}
					if (value2 > 0)
					{
						if (value2 == 15)
						{
							_ = (Vector3)vector3i;
							int forceToOtherBlock = blockValue.GetForceToOtherBlock(other);
							num6 = ((allDirectionsShuffled[j].y != -1) ? (num6 + forceToOtherBlock) : 100000);
							num += forceToOtherBlock;
						}
						else if ((value2 > 1 || other.Block.StabilitySupport) && posChecked.Add(vector3i2))
						{
							posToCheckNext.Add(vector3i2);
							num6 = ((allDirectionsShuffled[j].y != -1) ? (num6 + blockValue.GetForceToOtherBlock(other)) : 100000);
						}
					}
				}
			}
			if (num2 > num6)
			{
				StabilityViewer.GetBlocks += num3;
				StabilityViewer.TotalIterations += num4;
				return 0f;
			}
			if (num6 > 0)
			{
				num5 = Mathf.Max(num5, (float)num2 / ((float)num6 * 1.01f));
			}
			List<Vector3i> list = posToCheck;
			posToCheck = posToCheckNext;
			posToCheckNext = list;
			posToCheckNext.Clear();
		}
		StabilityViewer.GetBlocks += num3;
		StabilityViewer.TotalIterations += num4;
		return 1f - num5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> CalcPhysicsStabilityToFall(Vector3i _pos, int maxBlocksToCheck, out float calculatedStability)
	{
		List<Vector3i> list = null;
		calculatedStability = 0f;
		unstablePositions.Clear();
		unstablePositions.Add(_pos);
		positionsToCheck.Clear();
		positionsToCheck.Enqueue(_pos);
		uniqueUnstablePositions.Clear();
		int num = 0;
		int num2 = 0;
		IChunk _chunk = null;
		for (int i = 0; i < maxBlocksToCheck; i++)
		{
			int num3 = num;
			foreach (Vector3i item in positionsToCheck)
			{
				world.GetChunkFromWorldPos(item, ref _chunk);
				BlockValue blockValue = _chunk?.GetBlockNoDamage(World.toBlockXZ(item.x), item.y, World.toBlockXZ(item.z)) ?? BlockValue.Air;
				Block block = blockValue.Block;
				num2 += block.blockMaterial.Mass.Value;
				Vector3i[] allDirectionsShuffled = Vector3i.AllDirectionsShuffled;
				for (int j = 0; j < allDirectionsShuffled.Length; j++)
				{
					Vector3i vector3i = allDirectionsShuffled[j];
					Vector3i vector3i2 = item + vector3i;
					if (_chunk == null || _chunk.X != World.toChunkXZ(vector3i2.x) || _chunk.Z != World.toChunkXZ(vector3i2.z))
					{
						_chunk = world.GetChunkFromWorldPos(vector3i2);
					}
					int x = World.toBlockXZ(vector3i2.x);
					int z = World.toBlockXZ(vector3i2.z);
					BlockValue other = _chunk?.GetBlockNoDamage(x, vector3i2.y, z) ?? BlockValue.Air;
					int num4 = ((!other.isair && _chunk != null) ? _chunk.GetStability(x, vector3i2.y, z) : 0);
					if (num4 == 15)
					{
						int forceToOtherBlock = blockValue.GetForceToOtherBlock(other);
						num3 = ((vector3i.y != -1) ? (num3 + forceToOtherBlock) : 100000);
						num += forceToOtherBlock;
					}
					else if (((num4 > 0 && other.Block.StabilitySupport) || num4 > 1) && unstablePositions.Add(vector3i2))
					{
						uniqueUnstablePositions.Enqueue(vector3i2);
						num3 = ((vector3i.y != -1) ? (num3 + blockValue.GetForceToOtherBlock(other)) : 100000);
					}
				}
			}
			if (num3 > 0)
			{
				calculatedStability = 1f - (float)num2 / (float)num3;
			}
			if (num2 > num3)
			{
				list = unstablePositions.Except(uniqueUnstablePositions).ToList();
				if (list.Count == 0)
				{
					calculatedStability = 1f;
				}
				break;
			}
			if (uniqueUnstablePositions.Count == 0)
			{
				break;
			}
			positionsToCheck.Clear();
			Queue<Vector3i> queue = uniqueUnstablePositions;
			uniqueUnstablePositions = positionsToCheck;
			positionsToCheck = queue;
			uniqueUnstablePositions.Clear();
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToFallingBlocks(IList<Vector3i> _list)
	{
		world.AddFallingBlocks(_list);
	}
}
