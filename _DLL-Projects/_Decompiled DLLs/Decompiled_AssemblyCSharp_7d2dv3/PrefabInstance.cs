using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrefabVolumes;
using UnityEngine;

public class PrefabInstance
{
	public struct Serializable
	{
		public int id;

		public string prefabName;

		public Vector3i position;

		public byte rotation;

		public Serializable(PooledBinaryReader _br)
		{
			id = _br.ReadInt32();
			prefabName = _br.ReadString();
			position = StreamUtils.ReadVector3i(_br);
			rotation = _br.ReadByte();
		}

		public Serializable(PrefabInstance _pi)
		{
			id = _pi.id;
			prefabName = _pi.prefab.PrefabName;
			position = _pi.boundingBoxPosition;
			rotation = _pi.rotation;
		}

		public void Write(PooledBinaryWriter _bw)
		{
			_bw.Write(id);
			_bw.Write(prefabName);
			StreamUtils.Write(_bw, position);
			_bw.Write(rotation);
		}

		public int GetLength()
		{
			return 17 + ((prefabName != null) ? prefabName.Length : 0);
		}
	}

	public int id;

	public byte rotation;

	public byte imposterBaseRotation;

	public Prefab prefab;

	public byte lastCopiedRotation;

	public Vector3i lastCopiedPrefabPosition;

	public bool bPrefabCopiedIntoWorld;

	public Vector3i boundingBoxPosition;

	public Vector3i boundingBoxSize;

	public string name;

	public PathAbstractions.AbstractedLocation location;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool imposterLookupDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathAbstractions.AbstractedLocation imposterLocation = PathAbstractions.AbstractedLocation.None;

	public int standaloneBlockSize;

	public float yOffsetOfPrefab;

	public QuestLockInstance lockInstance;

	public readonly List<SleeperVolume> sleeperVolumes = new List<SleeperVolume>();

	public readonly List<TriggerVolume> triggerVolumes = new List<TriggerVolume>();

	public readonly List<WallVolume> wallVolumes = new List<WallVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityInstanceIds = new List<int>();

	public FastTags<TagGroup.Global> LastRefreshType = FastTags<TagGroup.Global>.none;

	public QuestClass LastQuestClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong occupiedChunks;

	public float RotationAngle => (rotation & 3) * 90;

	public Quaternion RotationAroundY => Quaternion.AngleAxis(RotationAngle, Vector3.up);

	public PrefabInstance(int _id, PathAbstractions.AbstractedLocation _location, Vector3i _position, byte _rotation, Prefab _bad, int _standaloneBlockSize)
	{
		id = _id;
		if (_bad != null)
		{
			_bad.location = _location;
			boundingBoxSize = _bad.size;
		}
		name = _location.Name + "." + _id;
		location = _location;
		boundingBoxPosition = _position;
		lastCopiedPrefabPosition = Vector3i.zero;
		bPrefabCopiedIntoWorld = false;
		rotation = (lastCopiedRotation = _rotation);
		prefab = _bad;
		standaloneBlockSize = _standaloneBlockSize;
	}

	public Bounds GetAABB()
	{
		return BoundsUtils.BoundsForMinMax(boundingBoxPosition.x, boundingBoxPosition.y, boundingBoxPosition.z, boundingBoxPosition.x + boundingBoxSize.x, boundingBoxPosition.y + boundingBoxSize.y, boundingBoxPosition.z + boundingBoxSize.z);
	}

	public Vector2 GetCenterXZ()
	{
		return new Vector2((float)boundingBoxPosition.x + (float)boundingBoxSize.x * 0.5f, (float)boundingBoxPosition.z + (float)boundingBoxSize.z * 0.5f);
	}

	public bool IsBBInSyncWithPrefab()
	{
		if (bPrefabCopiedIntoWorld && lastCopiedPrefabPosition.Equals(boundingBoxPosition) && prefab.size.Equals(boundingBoxSize))
		{
			return lastCopiedRotation == rotation;
		}
		return false;
	}

	public void CopyIntoWorld(World _world, bool _copyEntities, bool _bOverwriteExistingBlocks, FastTags<TagGroup.Global> _tags)
	{
		if (lastCopiedRotation != rotation)
		{
			if (lastCopiedRotation < rotation)
			{
				int rotCount = rotation - lastCopiedRotation;
				prefab.RotateY(_bLeft: true, rotCount);
			}
			else
			{
				int rotCount2 = lastCopiedRotation - rotation;
				prefab.RotateY(_bLeft: false, rotCount2);
			}
			lastCopiedRotation = rotation;
			UpdateBoundingBoxPosAndScale(boundingBoxPosition, prefab.size);
		}
		prefab.CopyIntoLocal(_world.ChunkCache, boundingBoxPosition, _bOverwriteExistingBlocks, _bSetChunkToRegenerate: true, _tags);
		if (_copyEntities)
		{
			bool bSpawnEnemies = _world.IsEditor() || GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
			entityInstanceIds.Clear();
			prefab.CopyEntitiesIntoWorld(_world, boundingBoxPosition, entityInstanceIds, bSpawnEnemies);
		}
		lastCopiedPrefabPosition = boundingBoxPosition;
		bPrefabCopiedIntoWorld = true;
	}

	public static void RefreshSwitchesInContainingPoi(Quest _q)
	{
		if (GameManager.Instance.World.IsEditor() || !_q.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition))
		{
			return;
		}
		World world = GameManager.Instance.World;
		PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(pos);
		if (prefabAtPosition == null)
		{
			return;
		}
		for (int i = 0; i < prefabAtPosition.boundingBoxSize.z; i++)
		{
			for (int j = 0; j < prefabAtPosition.boundingBoxSize.x; j++)
			{
				for (int k = 0; k < prefabAtPosition.boundingBoxSize.y; k++)
				{
					Vector3i vector3i = World.worldToBlockPos(prefabAtPosition.boundingBoxPosition + new Vector3i(j, k, i));
					BlockValue block = world.GetBlock(vector3i);
					if (block.Block is BlockActivateSwitch || block.Block is BlockActivateSingle)
					{
						block.Block.Refresh(world, null, vector3i, block);
					}
				}
			}
		}
	}

	public static void RefreshTriggersInContainingPoi(Vector3 _v)
	{
		if (!GameManager.Instance.World.IsEditor())
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(_v);
			if (prefabAtPosition != null)
			{
				GameManager.Instance.World.triggerManager.RefreshTriggers(prefabAtPosition, prefabAtPosition.LastRefreshType);
			}
		}
	}

	public void CleanFromWorld(World _world, bool _bRemoveEntities)
	{
		if (!bPrefabCopiedIntoWorld)
		{
			return;
		}
		BlockTools.ClearRPC(_world, lastCopiedPrefabPosition, prefab.size.x, prefab.size.y, prefab.size.z, _bClearLight: true);
		if (_bRemoveEntities)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < entityInstanceIds.Count; i++)
			{
				int num = entityInstanceIds[i];
				Entity entity = _world.GetEntity(num);
				if (entity != null && !entity.IsDead())
				{
					_world.RemoveEntity(num, EnumRemoveEntityReason.Unloaded);
				}
				else
				{
					list.Add(num);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				entityInstanceIds.Remove(list[j]);
			}
		}
		lastCopiedPrefabPosition = Vector3i.zero;
		bPrefabCopiedIntoWorld = false;
	}

	public void AddSleeperVolume(SleeperVolume _volume)
	{
		if (!sleeperVolumes.Contains(_volume))
		{
			sleeperVolumes.Add(_volume);
		}
	}

	public void AddTriggerVolume(TriggerVolume _volume)
	{
		if (!triggerVolumes.Contains(_volume))
		{
			triggerVolumes.Add(_volume);
		}
	}

	public void AddWallVolume(WallVolume _volume)
	{
		if (!wallVolumes.Contains(_volume))
		{
			wallVolumes.Add(_volume);
		}
	}

	public void ResizeBoundingBox(Vector3i _deltaVec)
	{
		Vector3i size = boundingBoxSize + _deltaVec;
		if (size.x <= 1)
		{
			size.x = 1;
		}
		if (size.y <= 1)
		{
			size.y = 1;
		}
		if (size.z <= 1)
		{
			size.z = 1;
		}
		UpdateBoundingBoxPosAndScale(boundingBoxPosition, size);
	}

	public void MoveBoundingBox(Vector3i _deltaVec)
	{
		UpdateBoundingBoxPosAndScale(boundingBoxPosition + _deltaVec, boundingBoxSize);
	}

	public void SetBoundingBoxPosition(Vector3i _position)
	{
		UpdateBoundingBoxPosAndScale(_position, boundingBoxSize);
	}

	public void SetBoundingBoxSize(World _world, Vector3i _size)
	{
		UpdateBoundingBoxPosAndScale(boundingBoxPosition, _size);
	}

	public void CreateBoundingBox(bool _alsoCreateOtherBoxes = true)
	{
		SelectionBox selectionBox = SelectionBoxManager.Instance.CategoryDynamicPrefab.AddBox(name, boundingBoxPosition, boundingBoxSize, _bDrawDirection: true, _bAlwaysDrawDirection: true);
		selectionBox.FacingDirection = -((prefab.rotationToFaceNorth + rotation) % 4) * 90;
		selectionBox.UserData = this;
		selectionBox.SetCaption(prefab.PrefabName);
		if (!_alsoCreateOtherBoxes)
		{
			return;
		}
		foreach (PrefabVolumeListAbs allVolumeList in prefab.AllVolumeLists)
		{
			allVolumeList.CreateSelectionBoxes(this);
		}
	}

	public void UpdateBoundingBoxPosAndScale(Vector3i _pos, Vector3i _size, bool _moveVolumes = true)
	{
		if (_moveVolumes)
		{
			prefab.MoveVolumes(boundingBoxPosition - _pos);
		}
		boundingBoxPosition = _pos;
		boundingBoxSize = _size;
		SelectionBox box = GetBox();
		box.SetPositionAndSize(boundingBoxPosition, boundingBoxSize);
		box.FacingDirection = (prefab.rotationToFaceNorth + rotation) % 4 * 90;
		if (box.FacingDirection == 90f)
		{
			box.FacingDirection = 270f;
		}
		else if (box.FacingDirection == 270f)
		{
			box.FacingDirection = 90f;
		}
		foreach (PrefabVolumeListAbs allVolumeList in prefab.AllVolumeLists)
		{
			allVolumeList.ApplyVolumesToSelectionBoxes(this);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.GetDynamicPrefabDecorator()?.CallPrefabChangedEvent(this);
		}
	}

	public SelectionBox GetBox()
	{
		SelectionBoxManager.Instance.CategoryDynamicPrefab.TryGetBox(name, out var _box);
		return _box;
	}

	public void RotateAroundY()
	{
		rotation = (byte)((rotation + 1) % 4);
		MathUtils.Swap(ref boundingBoxSize.x, ref boundingBoxSize.z);
		UpdateBoundingBoxPosAndScale(boundingBoxPosition, boundingBoxSize);
	}

	public void SetRotation(byte _rotation)
	{
		while (rotation != (byte)(_rotation & 3))
		{
			rotation = (byte)((rotation + 1) & 3);
			prefab.RotateY(_bLeft: false, 1);
			boundingBoxSize = prefab.size;
		}
	}

	public bool Overlaps(Chunk _chunk)
	{
		Vector3i worldPosIMax = _chunk.worldPosIMax;
		if (worldPosIMax.x >= boundingBoxPosition.x && worldPosIMax.y >= boundingBoxPosition.y && worldPosIMax.z >= boundingBoxPosition.z)
		{
			Vector3i worldPosIMin = _chunk.worldPosIMin;
			Vector3i vector3i = boundingBoxPosition + boundingBoxSize;
			if (worldPosIMin.x < vector3i.x && worldPosIMin.y < vector3i.y)
			{
				return worldPosIMin.z < vector3i.z;
			}
			return false;
		}
		return false;
	}

	public bool Overlaps(Vector3 _pos, float _expandBounds = 0f)
	{
		Bounds aABB = GetAABB();
		aABB.Expand(_expandBounds);
		Vector3 max = aABB.max;
		Vector3 min = aABB.min;
		if (_pos.x <= max.x && _pos.x >= min.x && _pos.y <= max.y && _pos.y >= min.y && _pos.z <= max.z)
		{
			return _pos.z >= min.z;
		}
		return false;
	}

	public bool IsWithinInfoArea(Vector3 _pos)
	{
		if (prefab.InfoVolumeList.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < prefab.InfoVolumeList.Count; i++)
		{
			PrefabInfoVolume prefabInfoVolume = prefab.InfoVolumeList[i];
			Vector3i vector3i = boundingBoxPosition + prefabInfoVolume.startPos;
			if ((float)(vector3i.x - 1) <= _pos.x && _pos.x <= (float)(vector3i.x + prefabInfoVolume.size.x + 1) && (float)(vector3i.y - 1) <= _pos.y && _pos.y <= (float)(vector3i.y + prefabInfoVolume.size.y + 1) && (float)(vector3i.z - 1) <= _pos.z && _pos.z <= (float)(vector3i.z + prefabInfoVolume.size.z + 1))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyIntoChunk(World _world, Chunk _chunk, bool _bForceOverwriteBlocks = false, FastTags<TagGroup.Global> _questTags = default(FastTags<TagGroup.Global>))
	{
		prefab.CopyBlocksIntoChunkNoEntities(_world, _chunk, boundingBoxPosition, _bForceOverwriteBlocks, _questTags);
		bool bSpawnEnemies = _world.IsEditor() || GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
		prefab.CopyEntitiesIntoChunkStub(_chunk, boundingBoxPosition, entityInstanceIds, bSpawnEnemies);
		lastCopiedPrefabPosition = boundingBoxPosition;
		bPrefabCopiedIntoWorld = true;
	}

	public HashSetLong GetOccupiedChunks()
	{
		if (occupiedChunks != null)
		{
			return occupiedChunks;
		}
		occupiedChunks = new HashSetLong();
		int num = World.toChunkXZ(boundingBoxPosition.x);
		int num2 = World.toChunkXZ(boundingBoxPosition.x + boundingBoxSize.x);
		int num3 = World.toChunkXZ(boundingBoxPosition.z);
		int num4 = World.toChunkXZ(boundingBoxPosition.z + boundingBoxSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				occupiedChunks.Add(WorldChunkCache.MakeChunkKey(i, j));
			}
		}
		return occupiedChunks;
	}

	public IEnumerator ResetBlocksAndRebuild(World _world, FastTags<TagGroup.Global> _questTags, long _initialChunkKey = 0L)
	{
		LastRefreshType = _questTags;
		ChunkCluster cc = _world.ChunkCache;
		HashSetLong allChunks = GetOccupiedChunks();
		LockManager.Instance.ForceUnlockByChunk(allChunks);
		cc.ClearStabilityForChunks(allChunks);
		prefab.DestroyAllMultiblocks(cc, boundingBoxPosition);
		long[] neighbors = new long[4];
		List<TileEntity> teToRemove = new List<TileEntity>(10);
		HashSetLong seen = new HashSetLong();
		HashSetLong copied = new HashSetLong();
		HashSetLong regenerated = new HashSetLong();
		Queue<long> chunksToCopy = new Queue<long>();
		Queue<(long child, long parent)> chunksToRegen = new Queue<(long, long)>();
		if (_initialChunkKey == 0L || !allChunks.Contains(_initialChunkKey))
		{
			_initialChunkKey = allChunks.First();
		}
		chunksToCopy.Enqueue(_initialChunkKey);
		seen.Add(_initialChunkKey);
		while (chunksToCopy.Count > 0)
		{
			long num = chunksToCopy.Dequeue();
			Chunk chunkSync = cc.GetChunkSync(num);
			if (chunkSync != null && !CopyChunk(num))
			{
				Log.Error("[PrefabInstance] Chunk not found in cache during rebuild.");
				yield break;
			}
			chunkSync.ResetStability();
			copied.Add(num);
			cc.GetNeighborChunkKeys(num, _includeDiagonals: false, ref neighbors);
			long item = num;
			long[] array = neighbors;
			foreach (long num2 in array)
			{
				if (allChunks.Contains(num2) && !seen.Contains(num2) && !copied.Contains(num2))
				{
					seen.Add(num2);
					chunksToCopy.Enqueue(num2);
					item = num2;
				}
			}
			chunksToRegen.Enqueue((item, num));
			if (copied.Contains(chunksToRegen.Peek().child))
			{
				long item2 = chunksToRegen.Dequeue().parent;
				if (RegenerateChunk(item2))
				{
					regenerated.Add(item2);
				}
			}
			yield return null;
		}
		if (copied.Count != allChunks.Count)
		{
			Log.Warning("[PrefabInstance] Some chunks were not copied during ResetBlocksAndRebuild. Copying them to world now.");
			foreach (long item3 in allChunks)
			{
				if (!copied.Contains(item3))
				{
					CopyChunk(item3);
				}
			}
		}
		foreach (long item4 in allChunks)
		{
			if (!regenerated.Contains(item4))
			{
				RegenerateChunk(item4);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool CopyChunk(long chunkKey)
		{
			Chunk chunkSync2 = cc.GetChunkSync(chunkKey);
			if (chunkSync2 == null)
			{
				return false;
			}
			cc.ChunkPosNeedsRegeneration_DelayedStart();
			chunkSync2.ResetWaterDebugHandle();
			chunkSync2.ResetWaterSimHandle();
			chunkSync2.StopStabilityCalculation = true;
			CopyIntoChunk(_world, chunkSync2, _bForceOverwriteBlocks: true, _questTags);
			cc.ChunkPosNeedsRegeneration_DelayedStop();
			return true;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool RegenerateChunk(long chunkKey)
		{
			Chunk chunkSync2 = cc.GetChunkSync(chunkKey);
			if (chunkSync2 == null)
			{
				return false;
			}
			chunkSync2.NeedsDecoration = true;
			chunkSync2.NeedsLightDecoration = true;
			chunkSync2.NeedsLightCalculation = true;
			teToRemove.Clear();
			List<TileEntity> list = chunkSync2.GetTileEntities().list;
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				if (!chunkSync2.GetBlock(list[num3].localChunkPos).Block.HasTileEntity)
				{
					teToRemove.Add(list[num3]);
				}
				else
				{
					Vector3i vector3i = list[num3].ToWorldPos();
					if (boundingBoxPosition.x <= vector3i.x && boundingBoxPosition.y <= vector3i.y && boundingBoxPosition.z <= vector3i.z && boundingBoxPosition.x + boundingBoxSize.x > vector3i.x && boundingBoxPosition.y + boundingBoxSize.y > vector3i.y && boundingBoxPosition.z + boundingBoxSize.z > vector3i.z)
					{
						list[num3].Reset(_questTags);
					}
				}
			}
			foreach (TileEntity item5 in teToRemove)
			{
				chunkSync2.RemoveTileEntity(_world, item5);
			}
			_world.m_ChunkManager.ResendChunksToClients(new HashSetLong { chunkSync2.Key });
			return true;
		}
	}

	public GameUtils.EPlayerHomeType CheckForAnyPlayerHome(World _world)
	{
		return GameUtils.CheckForAnyPlayerHome(_world, boundingBoxPosition, boundingBoxPosition + boundingBoxSize);
	}

	public bool AddChunksToUncull(World _world, HashSetList<Chunk> _chunksToUncull)
	{
		bool result = false;
		foreach (long occupiedChunk in GetOccupiedChunks())
		{
			Chunk chunkSync = _world.ChunkCache.GetChunkSync(occupiedChunk);
			if (chunkSync != null && chunkSync.IsInternalBlocksCulled && !_chunksToUncull.hashSet.Contains(chunkSync))
			{
				_chunksToUncull.Add(chunkSync);
				result = true;
			}
		}
		return result;
	}

	public PathAbstractions.AbstractedLocation GetImposterLocation()
	{
		if (imposterLookupDone)
		{
			return imposterLocation;
		}
		string text = prefab?.distantPOIOverride ?? location.Name;
		imposterLocation = PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text);
		imposterLookupDone = true;
		return imposterLocation;
	}

	public bool Contains(int _entityId)
	{
		return entityInstanceIds.Contains(_entityId);
	}

	public override bool Equals(object _obj)
	{
		if (_obj is PrefabInstance prefabInstance)
		{
			return boundingBoxPosition == prefabInstance.boundingBoxPosition;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return boundingBoxPosition.GetHashCode();
	}

	public override string ToString()
	{
		return "[DynamicPrefabDecorator " + id + " " + ((prefab != null) ? prefab.PrefabName : string.Empty) + "]";
	}

	public void UpdateImposterView()
	{
		if (!GameManager.Instance.IsEditMode() || PrefabEditModeManager.Instance.IsActive())
		{
			return;
		}
		SelectionBox box = GetBox();
		if (box == null)
		{
			Log.Error("PrefabInstance has not SelectionBox! (UIV)");
			return;
		}
		Transform transform = box.transform.Find("PrefabImposter");
		if (transform == null)
		{
			ThreadManager.RunCoroutineSync(prefab.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, box.transform, "PrefabImposter", new Vector3((float)(-boundingBoxSize.x) / 2f, 0.15f, (float)(-boundingBoxSize.z) / 2f), DynamicPrefabDecorator.PrefabPreviewLimit));
			transform = box.transform.Find("PrefabImposter");
			imposterBaseRotation = lastCopiedRotation;
		}
		int num = MathUtils.Mod(rotation - imposterBaseRotation, 4);
		Vector3 localEulerAngles = transform.localEulerAngles;
		localEulerAngles.y = -90f * (float)num;
		transform.localEulerAngles = localEulerAngles;
		Vector3 localPosition = transform.localPosition;
		localPosition.x = (float)boundingBoxSize.x / 2f * (float)((num % 3 != 0) ? 1 : (-1));
		localPosition.z = (float)boundingBoxSize.z / 2f * (float)((num >= 2) ? 1 : (-1));
		transform.localPosition = localPosition;
		transform.gameObject.SetActive(!IsBBInSyncWithPrefab());
	}

	public void DestroyImposterView()
	{
		SelectionBox box = GetBox();
		if (box == null)
		{
			Log.Error("PrefabInstance has not SelectionBox! (DIV)");
			return;
		}
		Transform transform = box.transform.Find("PrefabImposter");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
	}

	public Vector3i GetPositionRelativeToPoi(Vector3i _pos)
	{
		Vector3i vector3i = _pos - boundingBoxPosition;
		if ((rotation & 1) != 0)
		{
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
		return (rotation & 3) switch
		{
			0 => vector3i, 
			1 => new Vector3i(vector3i.x, vector3i.y, boundingBoxSize.z - 1 - vector3i.z), 
			2 => new Vector3i(boundingBoxSize.x - 1 - vector3i.x, vector3i.y, boundingBoxSize.z - 1 - vector3i.z), 
			3 => new Vector3i(boundingBoxSize.x - 1 - vector3i.x, vector3i.y, vector3i.z), 
			_ => vector3i, 
		};
	}

	public Vector3i GetWorldPositionOfPoiOffset(Vector3i _offset)
	{
		Vector3i vector3i = boundingBoxSize;
		_offset = (rotation & 3) switch
		{
			1 => new Vector3i(_offset.x, _offset.y, vector3i.z - 1 - _offset.z), 
			2 => new Vector3i(vector3i.x - 1 - _offset.x, _offset.y, vector3i.z - 1 - _offset.z), 
			3 => new Vector3i(vector3i.x - 1 - _offset.x, _offset.y, _offset.z), 
			_ => _offset, 
		};
		if ((rotation & 1) != 0)
		{
			MathUtils.Swap(ref _offset.x, ref _offset.z);
		}
		_offset += boundingBoxPosition;
		return _offset;
	}

	public Serializable GetSerializable()
	{
		return new Serializable(this);
	}
}
