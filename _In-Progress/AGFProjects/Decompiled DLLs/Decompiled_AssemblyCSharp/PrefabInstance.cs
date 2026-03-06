using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstance
{
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

	public List<SleeperVolume> sleeperVolumes = new List<SleeperVolume>();

	public List<TriggerVolume> triggerVolumes = new List<TriggerVolume>();

	public List<WallVolume> wallVolumes = new List<WallVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> entityInstanceIds = new List<int>();

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

	public void CopyIntoWorld(World _world, bool _CopyEntities, bool _bOverwriteExistingBlocks, FastTags<TagGroup.Global> _tags)
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
		prefab.CopyIntoLocal(_world.ChunkClusters[0], boundingBoxPosition, _bOverwriteExistingBlocks, _bSetChunkToRegenerate: true, _tags);
		if (_CopyEntities)
		{
			bool bSpawnEnemies = _world.IsEditor() || GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
			entityInstanceIds.Clear();
			prefab.CopyEntitiesIntoWorld(_world, boundingBoxPosition, entityInstanceIds, bSpawnEnemies);
		}
		lastCopiedPrefabPosition = boundingBoxPosition;
		bPrefabCopiedIntoWorld = true;
	}

	public static void RefreshSwitchesInContainingPoi(Quest q)
	{
		if (GameManager.Instance.World.IsEditor() || !q.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition))
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
						block.Block.Refresh(world, null, 0, vector3i, block);
					}
				}
			}
		}
	}

	public static void RefreshTriggersInContainingPoi(Vector3 v)
	{
		if (!GameManager.Instance.World.IsEditor())
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(v);
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
		BlockTools.ClearRPC(_world, 0, lastCopiedPrefabPosition, prefab.size.x, prefab.size.y, prefab.size.z, _bClearLight: true);
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
				int item = list[j];
				entityInstanceIds.Remove(item);
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
		SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").AddBox(name, boundingBoxPosition, boundingBoxSize, _bDrawDirection: true, _bAlwaysDrawDirection: true);
		selectionBox.facingDirection = -((prefab.rotationToFaceNorth + rotation) % 4) * 90;
		selectionBox.UserData = this;
		selectionBox.SetCaption(prefab.PrefabName);
		if (!_alsoCreateOtherBoxes)
		{
			return;
		}
		if (prefab.bTraderArea)
		{
			for (int i = 0; i < prefab.TeleportVolumes.Count; i++)
			{
				Prefab.PrefabTeleportVolume prefabTeleportVolume = prefab.TeleportVolumes[i];
				prefab.AddTeleportVolumeSelectionBox(prefabTeleportVolume, name + "_" + i, boundingBoxPosition + prefabTeleportVolume.startPos);
			}
		}
		if (prefab.bSleeperVolumes)
		{
			for (int j = 0; j < prefab.SleeperVolumes.Count; j++)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = prefab.SleeperVolumes[j];
				prefab.AddSleeperVolumeSelectionBox(prefabSleeperVolume, name + "_" + j, boundingBoxPosition + prefabSleeperVolume.startPos);
			}
		}
		if (prefab.bInfoVolumes)
		{
			for (int k = 0; k < prefab.InfoVolumes.Count; k++)
			{
				Prefab.PrefabInfoVolume prefabInfoVolume = prefab.InfoVolumes[k];
				prefab.AddInfoVolumeSelectionBox(prefabInfoVolume, name + "_" + k, boundingBoxPosition + prefabInfoVolume.startPos);
			}
		}
		if (prefab.bWallVolumes)
		{
			for (int l = 0; l < prefab.WallVolumes.Count; l++)
			{
				Prefab.PrefabWallVolume prefabWallVolume = prefab.WallVolumes[l];
				prefab.AddWallVolumeSelectionBox(prefabWallVolume, name + "_" + l, boundingBoxPosition + prefabWallVolume.startPos);
			}
		}
		if (prefab.bTriggerVolumes)
		{
			for (int m = 0; m < prefab.TriggerVolumes.Count; m++)
			{
				Prefab.PrefabTriggerVolume prefabTriggerVolume = prefab.TriggerVolumes[m];
				prefab.AddTriggerVolumeSelectionBox(prefabTriggerVolume, name + "_" + m, boundingBoxPosition + prefabTriggerVolume.startPos);
			}
		}
		if (prefab.bPOIMarkers)
		{
			for (int n = 0; n < prefab.POIMarkers.Count; n++)
			{
				Prefab.Marker marker = prefab.POIMarkers[n];
				prefab.AddPOIMarker(marker.GroupName + "_" + n, boundingBoxPosition, marker.Start, marker.Size, marker.GroupName, marker.Tags, marker.MarkerType, n);
			}
		}
	}

	public void UpdateBoundingBoxPosAndScale(Vector3i _pos, Vector3i _size, bool _moveSleepers = true)
	{
		if (_moveSleepers)
		{
			prefab.MoveVolumes(boundingBoxPosition - _pos);
		}
		boundingBoxPosition = _pos;
		boundingBoxSize = _size;
		SelectionBox box = SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(name);
		box.SetPositionAndSize(boundingBoxPosition, boundingBoxSize);
		box.facingDirection = (prefab.rotationToFaceNorth + rotation) % 4 * 90;
		if (box.facingDirection == 90f)
		{
			box.facingDirection = 270f;
		}
		else if (box.facingDirection == 270f)
		{
			box.facingDirection = 90f;
		}
		if (prefab.bSleeperVolumes)
		{
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("SleeperVolume");
			bool visible = category.IsVisible();
			for (int i = 0; i < prefab.SleeperVolumes.Count; i++)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = prefab.SleeperVolumes[i];
				if (prefabSleeperVolume.used)
				{
					string text = name + "_" + i;
					SelectionBox box2 = category.GetBox(text);
					if (box2 != null)
					{
						box2.SetPositionAndSize(boundingBoxPosition + prefabSleeperVolume.startPos, prefabSleeperVolume.size);
						box2.SetVisible(visible);
					}
				}
			}
		}
		if (prefab.bTraderArea)
		{
			SelectionCategory category2 = SelectionBoxManager.Instance.GetCategory("TraderTeleport");
			for (int j = 0; j < prefab.TeleportVolumes.Count; j++)
			{
				if (prefab.TeleportVolumes[j].used)
				{
					SelectionBox box3 = category2.GetBox(name + "_" + j);
					if (box3 != null)
					{
						box3.SetPositionAndSize(boundingBoxPosition + prefab.TeleportVolumes[j].startPos, prefab.TeleportVolumes[j].size);
						box3.SetVisible(category2.IsVisible());
					}
				}
			}
		}
		if (prefab.bTriggerVolumes)
		{
			SelectionCategory category3 = SelectionBoxManager.Instance.GetCategory("TriggerVolume");
			for (int k = 0; k < prefab.TriggerVolumes.Count; k++)
			{
				if (prefab.TriggerVolumes[k].used)
				{
					SelectionBox box4 = category3.GetBox(name + "_" + k);
					if (box4 != null)
					{
						box4.SetPositionAndSize(boundingBoxPosition + prefab.TriggerVolumes[k].startPos, prefab.TriggerVolumes[k].size);
						box4.SetVisible(category3.IsVisible());
					}
				}
			}
		}
		if (prefab.bInfoVolumes)
		{
			SelectionCategory category4 = SelectionBoxManager.Instance.GetCategory("InfoVolume");
			for (int l = 0; l < prefab.InfoVolumes.Count; l++)
			{
				if (prefab.InfoVolumes[l].used)
				{
					SelectionBox box5 = category4.GetBox(name + "_" + l);
					if (box5 != null)
					{
						box5.SetPositionAndSize(boundingBoxPosition + prefab.InfoVolumes[l].startPos, prefab.InfoVolumes[l].size);
						box5.SetVisible(category4.IsVisible());
					}
				}
			}
		}
		if (prefab.bWallVolumes)
		{
			SelectionCategory category5 = SelectionBoxManager.Instance.GetCategory("WallVolume");
			for (int m = 0; m < prefab.WallVolumes.Count; m++)
			{
				SelectionBox box6 = category5.GetBox(name + "_" + m);
				if (box6 != null)
				{
					box6.SetPositionAndSize(boundingBoxPosition + prefab.WallVolumes[m].startPos, prefab.WallVolumes[m].size);
					box6.SetVisible(category5.IsVisible());
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.GetDynamicPrefabDecorator()?.CallPrefabChangedEvent(this);
		}
	}

	public SelectionBox GetBox()
	{
		return SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(name);
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
		if (prefab.InfoVolumes.Count == 0)
		{
			return true;
		}
		foreach (Prefab.PrefabInfoVolume infoVolume in prefab.InfoVolumes)
		{
			Vector3i vector3i = boundingBoxPosition + infoVolume.startPos;
			if ((float)(vector3i.x - 1) <= _pos.x && _pos.x <= (float)(vector3i.x + infoVolume.size.x + 1) && (float)(vector3i.y - 1) <= _pos.y && _pos.y <= (float)(vector3i.y + infoVolume.size.y + 1) && (float)(vector3i.z - 1) <= _pos.z && _pos.z <= (float)(vector3i.z + infoVolume.size.z + 1))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyIntoChunk(World _world, Chunk _chunk, bool _bForceOverwriteBlocks = false)
	{
		prefab.CopyBlocksIntoChunkNoEntities(_world, _chunk, boundingBoxPosition, _bForceOverwriteBlocks);
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

	public IEnumerator ResetTerrain(World _world)
	{
		HashSetLong chunks = GetOccupiedChunks();
		yield return GameManager.Instance.ResetWindowsAndLocksByChunks(chunks);
		ChunkCluster chunkCluster = _world.ChunkClusters[0];
		foreach (long item in chunks)
		{
			Chunk chunkSync = chunkCluster.GetChunkSync(item);
			if (chunkSync != null)
			{
				chunkSync.ResetWaterDebugHandle();
				chunkSync.ResetWaterSimHandle();
			}
		}
		_world.RebuildTerrain(chunks, boundingBoxPosition, boundingBoxSize, _bStopStabilityUpdate: true, _bRegenerateChunk: false, _bFillEmptyBlocks: false, _isReset: true);
	}

	public void ResetBlocksAndRebuild(World _world, FastTags<TagGroup.Global> questTags)
	{
		LastRefreshType = questTags;
		ChunkCluster chunkCluster = _world.ChunkClusters[0];
		chunkCluster.ChunkPosNeedsRegeneration_DelayedStart();
		HashSetLong hashSetLong = GetOccupiedChunks();
		foreach (long item in hashSetLong)
		{
			Chunk chunkSync = chunkCluster.GetChunkSync(item);
			if (chunkSync != null)
			{
				chunkSync.StopStabilityCalculation = true;
				chunkSync.ResetWaterDebugHandle();
				chunkSync.ResetWaterSimHandle();
			}
		}
		CopyIntoWorld(_world, _CopyEntities: false, _bOverwriteExistingBlocks: true, questTags);
		foreach (long item2 in hashSetLong)
		{
			Chunk chunkSync2 = chunkCluster.GetChunkSync(item2);
			if (chunkSync2 != null)
			{
				chunkSync2.NeedsDecoration = true;
				chunkSync2.NeedsLightDecoration = true;
				chunkSync2.NeedsLightCalculation = true;
			}
		}
		List<TileEntity> list = new List<TileEntity>(10);
		foreach (long item3 in hashSetLong)
		{
			Chunk chunkSync3 = chunkCluster.GetChunkSync(item3);
			if (chunkSync3 == null)
			{
				continue;
			}
			list.Clear();
			List<TileEntity> list2 = chunkSync3.GetTileEntities().list;
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				TileEntity tileEntity = list2[num];
				if (!chunkSync3.GetBlock(tileEntity.localChunkPos).Block.HasTileEntity)
				{
					list.Add(tileEntity);
				}
				else
				{
					Vector3i vector3i = tileEntity.ToWorldPos();
					if (boundingBoxPosition.x <= vector3i.x && boundingBoxPosition.y <= vector3i.y && boundingBoxPosition.z <= vector3i.z && boundingBoxPosition.x + boundingBoxSize.x > vector3i.x && boundingBoxPosition.y + boundingBoxSize.y > vector3i.y && boundingBoxPosition.z + boundingBoxSize.z > vector3i.z)
					{
						tileEntity.Reset(questTags);
					}
				}
			}
			foreach (TileEntity item4 in list)
			{
				chunkSync3.RemoveTileEntity(_world, item4);
			}
		}
		chunkCluster.ChunkPosNeedsRegeneration_DelayedStop();
		_world.m_ChunkManager.ResendChunksToClients(hashSetLong);
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

	public override bool Equals(object obj)
	{
		if (obj is PrefabInstance)
		{
			return boundingBoxPosition == ((PrefabInstance)obj).boundingBoxPosition;
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
}
