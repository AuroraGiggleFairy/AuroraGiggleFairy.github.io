using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using GUI_2;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionTextureBlock : ItemActionRanged
{
	public enum EnumPaintMode
	{
		Single,
		Multiple,
		Spray,
		Fill
	}

	public struct ChannelMask(int channel)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int channelMask = 1 << channel;

		public bool IncludesChannel(int _channel)
		{
			return (channelMask & (1 << _channel)) != 0;
		}

		public void ToggleChannel(int _channel)
		{
			if (channelMask != 1 << _channel)
			{
				channelMask ^= 1 << _channel;
			}
		}

		public void SetExclusiveChannel(int _channel)
		{
			channelMask = 1 << _channel;
		}
	}

	public class ItemActionTextureBlockData(ItemInventoryData _invData, int _indexInEntityOfAction, string _particleTransform) : ItemActionDataRanged(_invData, _indexInEntityOfAction)
	{
		public int idx = 1;

		public bool bAutoChannel;

		public ChannelMask channelMask = new ChannelMask(0);

		public EnumPaintMode paintMode;

		public bool bReplacePaintNextTime;

		public bool bPaintAllSides;

		public float lastTimeReplacePaintShown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EPaintResult
	{
		CanNotPaint,
		Painted,
		SamePaint,
		NoPaintAvailable
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MarkerGetHitBlockFace = new ProfilerMarker("ItemActionTextureBlock.getHitBlockFace");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MarkerAutoChannel = new ProfilerMarker("ItemActionTextureBlock.autoChannel");

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoveTexture;

	public int DefaultTextureID = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector3i, bool> visitedPositions = new Dictionary<Vector3i, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, bool> visitedRays = new Dictionary<Vector2i, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Stack<Vector2i> positionsToCheck = new Stack<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldRayHitInfo worldRayHitInfo = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue currentMagazineItem;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionTextureBlockData(_invData, _indexInEntityOfAction, "Muzzle/Particle1");
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("RemoveTexture"))
		{
			bRemoveTexture = StringParsers.ParseBool(_props.Values["RemoveTexture"]);
		}
		if (_props.Values.ContainsKey("DefaultTextureID"))
		{
			DefaultTextureID = Convert.ToInt32(_props.Values["DefaultTextureID"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int getUserData(ItemActionData _actionData)
	{
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)_actionData;
		int textureID = BlockTextureData.list[itemActionTextureBlockData.idx].TextureID;
		Color color = ((textureID != 0) ? MeshDescription.meshes[0].textureAtlas.uvMapping[textureID].color : Color.gray);
		return ((int)(color.r * 255f) & 0xFF) | (((int)(color.g * 255f) << 8) & 0xFF00) | (((int)(color.b * 255f) << 16) & 0xFF0000);
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState == 0 || !(_actionData.invData.model != null))
		{
			return;
		}
		ParticleSystem[] componentsInChildren = _actionData.invData.model.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer component = componentsInChildren[i].GetComponent<Renderer>();
			if (component != null)
			{
				component.material.SetColor("_Color", new Color32((byte)(_userData & 0xFF), (byte)((_userData >> 8) & 0xFF), (byte)((_userData >> 16) & 0xFF), byte.MaxValue));
			}
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		ItemActionTextureBlockData obj = (ItemActionTextureBlockData)_data;
		obj.idx = obj.invData.itemValue.Meta;
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool checkAmmo(ItemActionData _actionData)
	{
		if (InfiniteAmmo || GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
		{
			return true;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (holdingEntity.bag.GetItemCount(currentMagazineItem) <= 0)
		{
			return holdingEntity.inventory.GetItemCount(currentMagazineItem) > 0;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool decreaseAmmo(ItemActionData _actionData)
	{
		if (InfiniteAmmo)
		{
			return true;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
		{
			return true;
		}
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)_actionData;
		int paintCost = BlockTextureData.list[itemActionTextureBlockData.idx].PaintCost;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = currentMagazineItem;
		int itemCount = holdingEntity.bag.GetItemCount(itemValue);
		int itemCount2 = holdingEntity.inventory.GetItemCount(itemValue);
		if (itemCount + itemCount2 >= paintCost)
		{
			paintCost -= holdingEntity.bag.DecItem(itemValue, paintCost);
			if (paintCost > 0)
			{
				holdingEntity.inventory.DecItem(itemValue, paintCost);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ConsumeAmmo(ItemActionData _actionData)
	{
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		base.OnHoldingUpdate(_actionData);
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)_actionData;
		if (itemActionTextureBlockData.bReplacePaintNextTime && Time.time - itemActionTextureBlockData.lastTimeReplacePaintShown > 5f)
		{
			itemActionTextureBlockData.lastTimeReplacePaintShown = Time.time;
			GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("ttPaintedTextureReplaced"));
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		currentMagazineItem = ItemClass.GetItem(MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex]);
		if ((double)_actionData.invData.holdingEntity.speedForward > 0.009)
		{
			rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCastMoving;
		}
		else
		{
			rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCast;
		}
		base.ExecuteAction(_actionData, _bReleased);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		GameManager.Instance.StartCoroutine(fireShotLater(_shotIdx, _actionData));
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{
		return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkBlockCanBeChanged(World _world, Vector3i _blockPos, PersistentPlayerData lpRelative)
	{
		return _world.CanPlaceBlockAt(_blockPos, lpRelative);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkBlockCanBePainted(World _world, Vector3i _blockPos, BlockValue _blockValue, PersistentPlayerData _lpRelative)
	{
		if (_blockValue.isair)
		{
			return false;
		}
		Block block = _blockValue.Block;
		if (!(block.shape is BlockShapeNew) || block.MeshIndex != 0)
		{
			return false;
		}
		return checkBlockCanBeChanged(_world, _blockPos, _lpRelative);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getParentBlock(ref BlockValue _blockValue, ref Vector3i _blockPos, ChunkCluster _cc)
	{
		Block block = _blockValue.Block;
		if (_blockValue.ischild)
		{
			Log.Warning("Trying to paint multiblock block: " + _blockValue.Block.GetBlockName());
			_blockPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			_blockValue = _cc.GetBlock(_blockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCurrentPaintIdx(ChunkCluster _cc, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, int _channel)
	{
		int blockFaceTexture = _cc.GetBlockFaceTexture(_blockPos, _blockFace, _channel);
		if (blockFaceTexture != 0)
		{
			return blockFaceTexture;
		}
		string _name;
		return GameUtils.FindPaintIdForBlockFace(_blockValue, _blockFace, out _name, _channel);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EPaintResult paintFace(ChunkCluster _cc, int _entityId, ItemActionTextureBlockData _actionData, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, ChannelMask _channelMask)
	{
		EPaintResult result = EPaintResult.SamePaint;
		for (int i = 0; i < 1; i++)
		{
			if (!_channelMask.IncludesChannel(i))
			{
				continue;
			}
			int currentPaintIdx = getCurrentPaintIdx(_cc, _blockPos, _blockFace, _blockValue, i);
			if (_actionData.idx != currentPaintIdx)
			{
				if (!decreaseAmmo(_actionData))
				{
					return EPaintResult.NoPaintAvailable;
				}
				GameManager.Instance.SetBlockTextureServer(_blockPos, _blockFace, _actionData.idx, _entityId, (byte)i);
				result = EPaintResult.Painted;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EPaintResult paintBlock(World _world, ChunkCluster _cc, int _entityId, ItemActionTextureBlockData _actionData, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, PersistentPlayerData _lpRelative, ChannelMask _channelMask)
	{
		getParentBlock(ref _blockValue, ref _blockPos, _cc);
		if (!checkBlockCanBePainted(_world, _blockPos, _blockValue, _lpRelative))
		{
			return EPaintResult.CanNotPaint;
		}
		if (BlockToolSelection.Instance.SelectionActive && !new BoundsInt(BlockToolSelection.Instance.SelectionMin, BlockToolSelection.Instance.SelectionSize).Contains(_blockPos))
		{
			return EPaintResult.CanNotPaint;
		}
		if (!_actionData.bPaintAllSides)
		{
			return paintFace(_cc, _entityId, _actionData, _blockPos, _blockFace, _blockValue, _channelMask);
		}
		int num = 0;
		for (int i = 0; i <= 5; i++)
		{
			_blockFace = (BlockFace)i;
			EPaintResult ePaintResult = paintFace(_cc, _entityId, _actionData, _blockPos, _blockFace, _blockValue, _channelMask);
			switch (ePaintResult)
			{
			case EPaintResult.NoPaintAvailable:
				return ePaintResult;
			case EPaintResult.Painted:
				num++;
				break;
			}
		}
		if (num == 0)
		{
			return EPaintResult.SamePaint;
		}
		return EPaintResult.Painted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void floodFill(World _world, ChunkCluster _cc, int _entityId, ItemActionTextureBlockData _actionData, PersistentPlayerData _lpRelative, int _sourcePaint, Vector3 _hitPosition, Vector3 _hitFaceNormal, Vector3 _dir1, Vector3 _dir2, int _channel)
	{
		visitedPositions.Clear();
		visitedRays.Clear();
		positionsToCheck.Clear();
		positionsToCheck.Push(new Vector2i(0, 0));
		while (positionsToCheck.Count > 0)
		{
			Vector2i vector2i = positionsToCheck.Pop();
			if (visitedRays.ContainsKey(vector2i))
			{
				continue;
			}
			visitedRays.Add(vector2i, value: true);
			Vector3 origin = _hitPosition + _hitFaceNormal * 0.2f + vector2i.x * _dir1 + vector2i.y * _dir2;
			Vector3 direction = -_hitFaceNormal * 0.3f;
			float magnitude = direction.magnitude;
			if (!Voxel.Raycast(_world, new Ray(origin, direction), magnitude, -555528197, 69, 0f))
			{
				continue;
			}
			worldRayHitInfo.CopyFrom(Voxel.voxelRayHitInfo);
			BlockValue blockValue = worldRayHitInfo.hit.blockValue;
			Vector3i blockPos = worldRayHitInfo.hit.blockPos;
			bool flag;
			if (worldRayHitInfo.hitTriangleIdx < 0 || ((flag = visitedPositions.TryGetValue(blockPos, out var value)) && !value))
			{
				continue;
			}
			if (!flag)
			{
				Vector3 _hitFaceCenter;
				Vector3 _hitFaceNormal2;
				BlockFace blockFaceFromHitInfo = GameUtils.GetBlockFaceFromHitInfo(blockPos, blockValue, worldRayHitInfo.hitCollider, worldRayHitInfo.hitTriangleIdx, out _hitFaceCenter, out _hitFaceNormal2);
				if (blockFaceFromHitInfo == BlockFace.None)
				{
					continue;
				}
				_hitFaceNormal2 = _hitFaceNormal2.normalized;
				if ((double)(_hitFaceNormal2 - _hitFaceNormal).sqrMagnitude > 0.01)
				{
					continue;
				}
				if (getCurrentPaintIdx(_cc, blockPos, blockFaceFromHitInfo, blockValue, _channel) != _sourcePaint)
				{
					visitedPositions.Add(blockPos, value: false);
					continue;
				}
				EPaintResult ePaintResult = paintBlock(_world, _cc, _entityId, _actionData, blockPos, blockFaceFromHitInfo, blockValue, _lpRelative, new ChannelMask(_channel));
				if (ePaintResult == EPaintResult.CanNotPaint || ePaintResult == EPaintResult.NoPaintAvailable)
				{
					visitedPositions.Add(blockPos, value: false);
					continue;
				}
				visitedPositions.Add(blockPos, value: true);
			}
			positionsToCheck.Push(vector2i + Vector2i.down);
			positionsToCheck.Push(vector2i + Vector2i.up);
			positionsToCheck.Push(vector2i + Vector2i.left);
			positionsToCheck.Push(vector2i + Vector2i.right);
		}
		visitedPositions.Clear();
		visitedRays.Clear();
		positionsToCheck.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator fireShotLater(int _shotIdx, ItemActionDataRanged _actionData)
	{
		yield return new WaitForSeconds(rayCastDelay);
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(holdingEntity.entityId);
		holdingEntity.GetLookVector((_actionData.muzzle != null) ? _actionData.muzzle.forward : Vector3.zero);
		if (getHitBlockFace(_actionData, out var blockPos, out var bv, out var blockFace, out var hitInfo) == -1 || hitInfo == null || !hitInfo.bHitValid)
		{
			yield break;
		}
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)_actionData;
		_ = itemActionTextureBlockData.invData;
		if (bRemoveTexture)
		{
			itemActionTextureBlockData.idx = 0;
		}
		World world = GameManager.Instance.World;
		ChunkCluster chunkCluster = world.ChunkClusters[hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			yield break;
		}
		BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
		string _name;
		if (itemActionTextureBlockData.bReplacePaintNextTime)
		{
			itemActionTextureBlockData.bReplacePaintNextTime = false;
			if (!checkBlockCanBeChanged(world, blockPos, playerDataFromEntityID))
			{
				yield break;
			}
			for (int i = 0; i < 1; i++)
			{
				if (!itemActionTextureBlockData.channelMask.IncludesChannel(i))
				{
					continue;
				}
				int num = chunkCluster.GetBlockFaceTexture(blockPos, blockFace, i);
				if (itemActionTextureBlockData.idx == num)
				{
					continue;
				}
				if (num == 0)
				{
					num = GameUtils.FindPaintIdForBlockFace(bv, blockFace, out _name, i);
				}
				if (num != itemActionTextureBlockData.idx)
				{
					if (GameManager.Instance.GetActiveBlockTool() is BlockToolSelection { SelectionActive: not false })
					{
						replacePaintInCurrentSelection(blockPos, blockFace, num, itemActionTextureBlockData.idx, i);
					}
					else
					{
						replacePaintInCurrentPrefab(blockPos, blockFace, num, itemActionTextureBlockData.idx, i);
					}
				}
			}
			yield break;
		}
		switch (itemActionTextureBlockData.paintMode)
		{
		case EnumPaintMode.Single:
			paintBlock(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, blockPos, blockFace, bv, playerDataFromEntityID, itemActionTextureBlockData.channelMask);
			break;
		case EnumPaintMode.Fill:
		{
			Vector3 _hitFaceCenter2;
			Vector3 normalized2 = GameUtils.GetNormalFromHitInfo(blockPos, hitInfo.hitCollider, hitInfo.hitTriangleIdx, out _hitFaceCenter2).normalized;
			Vector3 vector3;
			Vector3 vector4;
			if (Utils.FastAbs(normalized2.x) >= Utils.FastAbs(normalized2.y) && Utils.FastAbs(normalized2.x) >= Utils.FastAbs(normalized2.z))
			{
				vector3 = Vector3.up;
				vector4 = Vector3.forward;
			}
			else if (Utils.FastAbs(normalized2.y) >= Utils.FastAbs(normalized2.x) && Utils.FastAbs(normalized2.y) >= Utils.FastAbs(normalized2.z))
			{
				vector3 = Vector3.right;
				vector4 = Vector3.forward;
			}
			else
			{
				vector3 = Vector3.right;
				vector4 = Vector3.up;
			}
			vector3 = ProjectVectorOnPlane(normalized2, vector3).normalized * 0.3f;
			vector4 = ProjectVectorOnPlane(normalized2, vector4).normalized * 0.3f;
			for (int j = 0; j < 1; j++)
			{
				if (!itemActionTextureBlockData.channelMask.IncludesChannel(j))
				{
					continue;
				}
				int num5 = chunkCluster.GetBlockFaceTexture(blockPos, blockFace, j);
				if (itemActionTextureBlockData.idx != num5)
				{
					if (num5 == 0)
					{
						num5 = GameUtils.FindPaintIdForBlockFace(bv, blockFace, out _name, j);
					}
					if (itemActionTextureBlockData.idx != num5)
					{
						floodFill(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, playerDataFromEntityID, num5, hitInfo.hit.pos, normalized2, vector3, vector4, j);
					}
				}
			}
			break;
		}
		case EnumPaintMode.Multiple:
		case EnumPaintMode.Spray:
		{
			float num2 = ((itemActionTextureBlockData.paintMode == EnumPaintMode.Spray) ? 7.5f : 1.25f);
			if (hitInfo.hitTriangleIdx == -1)
			{
				break;
			}
			Vector3 _hitFaceCenter;
			Vector3 _hitFaceNormal = GameUtils.GetNormalFromHitInfo(blockPos, hitInfo.hitCollider, hitInfo.hitTriangleIdx, out _hitFaceCenter);
			Vector3 normalized = _hitFaceNormal.normalized;
			Vector3 vector;
			Vector3 vector2;
			if (Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.y) && Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.z))
			{
				vector = Vector3.up;
				vector2 = Vector3.forward;
			}
			else if (Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.x) && Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.z))
			{
				vector = Vector3.right;
				vector2 = Vector3.forward;
			}
			else
			{
				vector = Vector3.right;
				vector2 = Vector3.up;
			}
			vector = ProjectVectorOnPlane(normalized, vector).normalized;
			_hitFaceCenter = ProjectVectorOnPlane(normalized, vector2);
			vector2 = _hitFaceCenter.normalized;
			Vector3 pos = hitInfo.hit.pos;
			Vector3 origin = hitInfo.ray.origin;
			for (float num3 = 0f - num2; num3 <= num2; num3 += 0.5f)
			{
				for (float num4 = 0f - num2; num4 <= num2; num4 += 0.5f)
				{
					Vector3 direction = pos + num3 * vector + num4 * vector2 - origin;
					int hitMask = 69;
					if (Voxel.Raycast(world, new Ray(origin, direction), Range, -555528197, hitMask, 0f))
					{
						WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
						bv = worldRayHitInfo.hit.blockValue;
						blockPos = worldRayHitInfo.hit.blockPos;
						blockFace = GameUtils.GetBlockFaceFromHitInfo(blockPos, bv, worldRayHitInfo.hitCollider, worldRayHitInfo.hitTriangleIdx, out _hitFaceCenter, out _hitFaceNormal);
						if (blockFace != BlockFace.None)
						{
							paintBlock(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, blockPos, blockFace, bv, playerDataFromEntityID, itemActionTextureBlockData.channelMask);
						}
					}
				}
			}
			break;
		}
		}
		BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getHitBlockFace(ItemActionDataRanged _actionData, out Vector3i blockPos, out BlockValue bv, out BlockFace blockFace, out WorldRayHitInfo hitInfo)
	{
		using (s_MarkerGetHitBlockFace.Auto())
		{
			bv = BlockValue.Air;
			blockFace = BlockFace.None;
			hitInfo = null;
			blockPos = Vector3i.zero;
			hitInfo = GetExecuteActionTarget(_actionData);
			if (hitInfo == null || !hitInfo.bHitValid || hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(hitInfo.tag))
			{
				return -1;
			}
			ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
			if (chunkCluster == null)
			{
				return -1;
			}
			bv = hitInfo.hit.blockValue;
			blockPos = hitInfo.hit.blockPos;
			Block block = bv.Block;
			if (bv.ischild)
			{
				blockPos = block.multiBlockPos.GetParentPos(blockPos, bv);
				bv = chunkCluster.GetBlock(blockPos);
			}
			if (bv.Block.MeshIndex != 0)
			{
				return -1;
			}
			blockFace = BlockFace.Top;
			BlockShapeNew blockShapeNew = bv.Block.shape as BlockShapeNew;
			if (blockShapeNew != null)
			{
				blockFace = GameUtils.GetBlockFaceFromHitInfo(blockPos, bv, hitInfo.hitCollider, hitInfo.hitTriangleIdx, out var _, out var _);
			}
			if (blockFace == BlockFace.None)
			{
				return -1;
			}
			if (_actionData is ItemActionTextureBlockData itemActionTextureBlockData)
			{
				using (s_MarkerAutoChannel.Auto())
				{
					if (itemActionTextureBlockData.bAutoChannel && blockShapeNew != null)
					{
						int visualMeshChannelFromHitInfo = blockShapeNew.GetVisualMeshChannelFromHitInfo(blockPos, bv, blockFace, hitInfo);
						if (visualMeshChannelFromHitInfo < 0)
						{
							return -1;
						}
						itemActionTextureBlockData.channelMask.SetExclusiveChannel(visualMeshChannelFromHitInfo);
						return chunkCluster.GetBlockFaceTexture(blockPos, blockFace, visualMeshChannelFromHitInfo);
					}
				}
				ChannelMask channelMask = itemActionTextureBlockData.channelMask;
				for (int i = 0; i < 1; i++)
				{
					if (channelMask.IncludesChannel(i))
					{
						int blockFaceTexture = chunkCluster.GetBlockFaceTexture(blockPos, blockFace, i);
						if (blockFaceTexture != -1)
						{
							return blockFaceTexture;
						}
					}
				}
				return -1;
			}
			return chunkCluster.GetBlockFaceTexture(blockPos, blockFace, 0);
		}
	}

	public void CopyTextureFromWorld(ItemActionTextureBlockData _actionData)
	{
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		Vector3i blockPos;
		BlockValue bv;
		BlockFace blockFace;
		WorldRayHitInfo hitInfo;
		int num = getHitBlockFace(_actionData, out blockPos, out bv, out blockFace, out hitInfo);
		switch (num)
		{
		case -1:
			return;
		case 0:
		{
			for (int i = 0; i < 1; i++)
			{
				if (_actionData.channelMask.IncludesChannel(i))
				{
					num = GameUtils.FindPaintIdForBlockFace(bv, blockFace, out var _, i);
					if (num != 0)
					{
						break;
					}
				}
			}
			break;
		}
		}
		EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
		BlockTextureData blockTextureData = BlockTextureData.list[num];
		if (blockTextureData != null && !blockTextureData.GetLocked(player))
		{
			_actionData.idx = num;
			_actionData.invData.itemValue.Meta = num;
			_actionData.invData.itemValue = _actionData.invData.itemValue;
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(player, Localization.Get("ttPaintTextureIsLocked"));
		}
	}

	public void CopyBlockFromWorld(ItemActionDataRanged _actionData)
	{
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(_actionData);
		if (executeActionTarget == null || !executeActionTarget.bHitValid || executeActionTarget.tag == null || !GameUtils.IsBlockOrTerrain(executeActionTarget.tag))
		{
			return;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[executeActionTarget.hit.clrIdx];
		if (chunkCluster != null)
		{
			BlockValue blockValue = executeActionTarget.hit.blockValue;
			Vector3i vector3i = executeActionTarget.hit.blockPos;
			Block block = blockValue.Block;
			if (blockValue.ischild)
			{
				vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
				blockValue = chunkCluster.GetBlock(vector3i);
			}
			if (blockValue.Block.MeshIndex == 0)
			{
				ItemValue itemValue = executeActionTarget.hit.blockValue.ToItemValue();
				itemValue.TextureFullArray = chunkCluster.GetTextureFullArray(vector3i);
				ItemStack itemStack = new ItemStack(itemValue, 99);
				_actionData.invData.holdingEntity.inventory.AddItem(itemStack);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onHoldingEntityFired(ItemActionData _actionData)
	{
		if (_actionData.indexInEntityOfAction == 0)
		{
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
		}
		else
		{
			_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaintInCurrentPrefab(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId, int _channel)
	{
		World world = GameManager.Instance.World;
		DynamicPrefabDecorator dynamicPrefabDecorator = world.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _blockPos);
		if (prefabInstance == null)
		{
			return;
		}
		for (int i = prefabInstance.boundingBoxPosition.x; i <= prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x; i++)
		{
			for (int j = prefabInstance.boundingBoxPosition.z; j <= prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					BlockValue block = world.GetBlock(i, k, j);
					if (block.isair)
					{
						continue;
					}
					long num = world.GetTexture(i, k, j, _channel);
					bool flag = false;
					for (int l = 0; l < 6; l++)
					{
						int num2 = (int)((num >> l * 8) & 0xFF);
						if (num2 == 0)
						{
							num2 = GameUtils.FindPaintIdForBlockFace(block, (BlockFace)l, out var _, _channel);
						}
						if (num2 == _searchPaintId)
						{
							num &= ~(255L << l * 8);
							num |= (long)_replacePaintId << l * 8;
							flag = true;
						}
					}
					if (flag)
					{
						world.SetTexture(0, i, k, j, num, _channel);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaintInCurrentSelection(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId, int _channel)
	{
		if (!(GameManager.Instance.GetActiveBlockTool() is BlockToolSelection blockToolSelection))
		{
			return;
		}
		World world = GameManager.Instance.World;
		Vector3i selectionMin = blockToolSelection.SelectionMin;
		for (int i = selectionMin.x; i < selectionMin.x + blockToolSelection.SelectionSize.x; i++)
		{
			for (int j = selectionMin.z; j < selectionMin.z + blockToolSelection.SelectionSize.z; j++)
			{
				for (int k = selectionMin.y; k < selectionMin.y + blockToolSelection.SelectionSize.y; k++)
				{
					BlockValue block = world.GetBlock(i, k, j);
					if (block.isair)
					{
						continue;
					}
					long num = world.GetTexture(i, k, j, _channel);
					bool flag = false;
					for (int l = 0; l < 6; l++)
					{
						int num2 = (int)((num >> l * 8) & 0xFF);
						if (num2 == 0)
						{
							num2 = GameUtils.FindPaintIdForBlockFace(block, (BlockFace)l, out var _, _channel);
						}
						if (num2 == _searchPaintId)
						{
							num &= ~(255L << l * 8);
							num |= (long)_replacePaintId << l * 8;
							flag = true;
						}
					}
					if (flag)
					{
						world.SetTexture(0, i, k, j, num, _channel);
					}
				}
			}
		}
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public override bool ShowAmmoInUI()
	{
		return true;
	}

	public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)_epl.inventory.holdingItemData.actionData[1];
		_xuiRadialWindow.ResetRadialEntries();
		bool num = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		_xuiRadialWindow.CreateRadialEntry(0, "ui_game_symbol_paint_bucket", "UIAtlas", "", Localization.Get("xuiMaterials"));
		_xuiRadialWindow.CreateRadialEntry(1, "ui_game_symbol_paint_brush", "UIAtlas", "", Localization.Get("xuiPaintBrush"), itemActionTextureBlockData.paintMode == EnumPaintMode.Single);
		_xuiRadialWindow.CreateRadialEntry(2, "ui_game_symbol_paint_roller", "UIAtlas", "", Localization.Get("xuiPaintRoller"), itemActionTextureBlockData.paintMode == EnumPaintMode.Multiple);
		_xuiRadialWindow.CreateRadialEntry(8, "ui_game_symbol_flood_fill", "UIAtlas", "", Localization.Get("xuiPaintFill"), itemActionTextureBlockData.paintMode == EnumPaintMode.Fill);
		if (num)
		{
			_xuiRadialWindow.CreateRadialEntry(3, "ui_game_symbol_paint_spraygun", "UIAtlas", "", Localization.Get("xuiSprayGun"), itemActionTextureBlockData.paintMode == EnumPaintMode.Spray);
		}
		_xuiRadialWindow.CreateRadialEntry(4, "ui_game_symbol_paint_allsides", "UIAtlas", "", Localization.Get("xuiPaintAllSides"), itemActionTextureBlockData.bPaintAllSides);
		_xuiRadialWindow.CreateRadialEntry(5, "ui_game_symbol_paint_eyedropper", "UIAtlas", "", Localization.Get("xuiTexturePicker"));
		if (num)
		{
			_xuiRadialWindow.CreateRadialEntry(6, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiCopyBlock"));
			_xuiRadialWindow.CreateRadialEntry(7, "ui_game_symbol_book", "UIAtlas", "", Localization.Get("xuiReplacePaint"), itemActionTextureBlockData.bReplacePaintNextTime);
		}
		_xuiRadialWindow.SetCommonData(UIUtils.ButtonIcon.FaceButtonNorth, handleRadialCommand, new XUiC_Radial.RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, _hasSpecialActionPriorToRadialVisibility: false, XUiC_Radial.RadialValidSameHoldingSlotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		ItemClass holdingItem = entityPlayer.inventory.holdingItem;
		ItemInventoryData holdingItemData = entityPlayer.inventory.holdingItemData;
		if (!(holdingItem.Actions[0] is ItemActionTextureBlock) || !(holdingItem.Actions[1] is ItemActionTextureBlock))
		{
			return;
		}
		ItemActionTextureBlock itemActionTextureBlock = (ItemActionTextureBlock)holdingItem.Actions[0];
		ItemActionTextureBlock itemActionTextureBlock2 = (ItemActionTextureBlock)holdingItem.Actions[1];
		ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlockData)holdingItemData.actionData[0];
		ItemActionTextureBlockData itemActionTextureBlockData2 = (ItemActionTextureBlockData)holdingItemData.actionData[1];
		if (_commandIndex != 0 && _commandIndex != 7)
		{
			itemActionTextureBlockData2.bReplacePaintNextTime = false;
		}
		switch (_commandIndex)
		{
		case 0:
			_sender.xui.playerUI.windowManager.Open("materials", _bModal: true);
			return;
		case 1:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = EnumPaintMode.Single);
			return;
		case 2:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = EnumPaintMode.Multiple);
			return;
		case 3:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = EnumPaintMode.Spray);
			return;
		case 4:
			itemActionTextureBlockData.bPaintAllSides = (itemActionTextureBlockData2.bPaintAllSides = !itemActionTextureBlockData2.bPaintAllSides);
			return;
		case 5:
			itemActionTextureBlock.CopyTextureFromWorld(itemActionTextureBlockData);
			itemActionTextureBlock2.CopyTextureFromWorld(itemActionTextureBlockData2);
			return;
		case 6:
			itemActionTextureBlock.CopyBlockFromWorld(itemActionTextureBlockData);
			itemActionTextureBlock2.CopyBlockFromWorld(itemActionTextureBlockData2);
			return;
		case 7:
			itemActionTextureBlockData2.bReplacePaintNextTime = !itemActionTextureBlockData2.bReplacePaintNextTime;
			return;
		case 8:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = EnumPaintMode.Fill);
			return;
		case 9:
			itemActionTextureBlockData.bAutoChannel = (itemActionTextureBlockData2.bAutoChannel = !itemActionTextureBlockData2.bAutoChannel);
			return;
		}
		int num = _commandIndex - 10;
		if (num >= 0 && num < 1)
		{
			itemActionTextureBlockData.bAutoChannel = (itemActionTextureBlockData2.bAutoChannel = false);
			if (InputUtils.ShiftKeyPressed)
			{
				itemActionTextureBlockData2.channelMask.ToggleChannel(num);
			}
			else
			{
				itemActionTextureBlockData2.channelMask.SetExclusiveChannel(num);
			}
			itemActionTextureBlockData.channelMask = itemActionTextureBlockData2.channelMask;
		}
	}
}
