using System.Collections;
using System.Collections.Generic;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionReplaceBlock : ItemActionRanged
{
	public enum EnumReplaceMode
	{
		SingleBlock,
		AllIdenticalBlocks
	}

	public enum EnumReplacePaintMode
	{
		KeepCurrentPaint,
		RemoveCurrentPaint,
		UseNewPaint,
		ReplaceWithAirBlocks
	}

	public class ItemActionReplaceBlockData : ItemActionDataRanged
	{
		public BlockValue? Block;

		public TextureFullArray PaintTextures;

		public sbyte Density;

		public EnumReplaceMode ReplaceMode;

		public EnumReplacePaintMode ReplacePaintMode;

		public Block ReplaceBlockClass
		{
			get
			{
				if (ReplacePaintMode != EnumReplacePaintMode.ReplaceWithAirBlocks)
				{
					if (!Block.HasValue)
					{
						return null;
					}
					return Block.Value.Block;
				}
				return global::Block.GetBlockByName("air", _caseInsensitive: true);
			}
		}

		public ItemActionReplaceBlockData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHUDDirty = true;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionReplaceBlockData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("CopyBlock"))
		{
			bCopyBlock = StringParsers.ParseBool(_props.Values["CopyBlock"]);
		}
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool checkAmmo(ItemActionData _actionData)
	{
		return true;
	}

	public override bool IsAmmoUsableUnderwater(EntityAlive holdingEntity)
	{
		return true;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		AnimationDelayData.AnimationDelays animationDelays = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value];
		rayCastDelay = (((double)_actionData.invData.holdingEntity.speedForward > 0.009) ? animationDelays.RayCastMoving : animationDelays.RayCast);
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
	public bool checkBlockCanBeChanged(World _world, Vector3i _blockPos, int _entityId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_entityId);
		return _world.CanPlaceBlockAt(_blockPos, playerDataFromEntityID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator fireShotLater(int _shotIdx, ItemActionDataRanged _actionData)
	{
		yield return new WaitForSeconds(rayCastDelay);
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_actionData.invData.holdingEntity;
		if (!GetHitBlock(_actionData, out var _blockPos, out var _bv, out var _hitInfo) || _hitInfo == null || !_hitInfo.bHitValid)
		{
			yield break;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			yield break;
		}
		ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlockData)_actionData;
		if (bCopyBlock)
		{
			int index = 1 - _actionData.indexInEntityOfAction;
			if (_actionData.invData.actionData[index] != null)
			{
				ItemActionReplaceBlockData obj = (ItemActionReplaceBlockData)_actionData.invData.actionData[index];
				obj.Block = _bv;
				obj.PaintTextures = chunkCluster.GetTextureFullArray(_blockPos);
				obj.Density = chunkCluster.GetDensity(_blockPos);
				isHUDDirty = true;
			}
		}
		else if (itemActionReplaceBlockData.ReplaceBlockClass == null)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiReplaceBlockNoBlockCopied"));
		}
		else
		{
			if (!checkBlockCanBeChanged(GameManager.Instance.World, _blockPos, entityPlayerLocal.entityId))
			{
				yield break;
			}
			if (itemActionReplaceBlockData.ReplaceMode == EnumReplaceMode.SingleBlock)
			{
				BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
				GameManager.Instance.SetBlocksRPC(new List<BlockChangeInfo> { replaceSingleBlock(_hitInfo.hit.clrIdx, chunkCluster, _blockPos, itemActionReplaceBlockData) });
				BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx);
				yield break;
			}
			Vector3i startPos;
			Vector3i endPos;
			if (!(GameManager.Instance.GetActiveBlockTool() is BlockToolSelection { SelectionActive: not false } blockToolSelection))
			{
				if (PrefabEditModeManager.Instance == null || !PrefabEditModeManager.Instance.IsActive())
				{
					GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiReplaceBlockRequiresSelection"));
					yield break;
				}
				PrefabEditModeManager.Instance.UpdateMinMax();
				startPos = PrefabEditModeManager.Instance.minPos;
				endPos = PrefabEditModeManager.Instance.maxPos;
			}
			else
			{
				startPos = blockToolSelection.SelectionStart;
				endPos = blockToolSelection.SelectionEnd;
			}
			BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
			replace(_hitInfo.hit.clrIdx, chunkCluster, itemActionReplaceBlockData, _bv, startPos, endPos);
			BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockChangeInfo replaceSingleBlock(int _hitClrIdx, ChunkCluster _cc, Vector3i _blockPos, ItemActionReplaceBlockData _actionData)
	{
		BlockValue block = _cc.GetBlock(_blockPos);
		BlockChangeInfo blockChangeInfo = new BlockChangeInfo
		{
			pos = _blockPos,
			clrIdx = _hitClrIdx,
			bChangeBlockValue = true
		};
		if (_actionData.ReplacePaintMode == EnumReplacePaintMode.ReplaceWithAirBlocks)
		{
			blockChangeInfo.blockValue = BlockValue.Air;
			blockChangeInfo.bChangeDensity = true;
			blockChangeInfo.density = MarchingCubes.DensityAir;
		}
		else
		{
			blockChangeInfo.blockValue = _actionData.Block.Value;
			blockChangeInfo.blockValue.rotation = block.rotation;
			Block block2 = block.Block;
			Block replaceBlockClass = _actionData.ReplaceBlockClass;
			if (block2.shape.IsTerrain() != replaceBlockClass.shape.IsTerrain())
			{
				blockChangeInfo.bChangeDensity = true;
				blockChangeInfo.density = _actionData.Density;
			}
			blockChangeInfo.bChangeTexture = true;
			switch (_actionData.ReplacePaintMode)
			{
			case EnumReplacePaintMode.RemoveCurrentPaint:
				blockChangeInfo.textureFull.Fill(0L);
				break;
			case EnumReplacePaintMode.UseNewPaint:
				blockChangeInfo.textureFull = _actionData.PaintTextures;
				break;
			case EnumReplacePaintMode.KeepCurrentPaint:
				blockChangeInfo.textureFull = _cc.GetTextureFullArray(_blockPos);
				break;
			}
		}
		return blockChangeInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replace(int _hitClrIdx, ChunkCluster _cc, ItemActionReplaceBlockData _actionData, BlockValue _srcBlock, Vector3i _startPos, Vector3i _endPos)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		Vector3i.SortBoundingBoxEdges(ref _startPos, ref _endPos);
		for (int i = _startPos.x; i <= _endPos.x; i++)
		{
			for (int j = _startPos.z; j <= _endPos.z; j++)
			{
				for (int k = _startPos.y; k <= _endPos.y; k++)
				{
					Vector3i vector3i = new Vector3i(i, k, j);
					BlockValue block = _cc.GetBlock(vector3i);
					if (!block.ischild && block.type == _srcBlock.type)
					{
						list.Add(replaceSingleBlock(_hitClrIdx, _cc, vector3i, _actionData));
					}
				}
			}
		}
		GameManager.Instance.SetBlocksRPC(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetHitBlock(ItemActionDataRanged _actionData, out Vector3i _blockPos, out BlockValue _bv, out WorldRayHitInfo _hitInfo)
	{
		_bv = BlockValue.Air;
		_hitInfo = null;
		_blockPos = Vector3i.zero;
		_hitInfo = GetExecuteActionTarget(_actionData);
		if (_hitInfo == null || !_hitInfo.bHitValid || _hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(_hitInfo.tag))
		{
			return false;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		_bv = _hitInfo.hit.blockValue;
		_blockPos = _hitInfo.hit.blockPos;
		Block block = _bv.Block;
		if (_bv.ischild)
		{
			_blockPos = block.multiBlockPos.GetParentPos(_blockPos, _bv);
			_bv = chunkCluster.GetBlock(_blockPos);
		}
		return true;
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

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public override bool IsEditingTool()
	{
		return true;
	}

	public override string GetStat(ItemActionData _data)
	{
		Block replaceBlockClass = ((ItemActionReplaceBlockData)_data).ReplaceBlockClass;
		if (replaceBlockClass == null)
		{
			return "No Block";
		}
		return replaceBlockClass.GetLocalizedBlockName();
	}

	public override bool IsStatChanged()
	{
		bool result = isHUDDirty;
		isHUDDirty = false;
		return result;
	}

	public override bool HasRadial()
	{
		return true;
	}

	public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
		ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlockData)_epl.inventory.holdingItemData.actionData[1];
		_xuiRadialWindow.ResetRadialEntries();
		_xuiRadialWindow.CreateRadialEntry(0, "ui_game_symbol_paint_brush", "UIAtlas", "", Localization.Get("xuiReplaceBlockSingle"), itemActionReplaceBlockData.ReplaceMode == EnumReplaceMode.SingleBlock);
		_xuiRadialWindow.CreateRadialEntry(1, "ui_game_symbol_paint_spraygun", "UIAtlas", "", Localization.Get("xuiReplaceBlockMulti"), itemActionReplaceBlockData.ReplaceMode == EnumReplaceMode.AllIdenticalBlocks);
		_xuiRadialWindow.CreateRadialEntry(2, "ui_game_symbol_brick", "UIAtlas", "", Localization.Get("xuiReplaceBlockKeepPaint"), itemActionReplaceBlockData.ReplacePaintMode == EnumReplacePaintMode.KeepCurrentPaint);
		_xuiRadialWindow.CreateRadialEntry(3, "ui_game_symbol_destruction", "UIAtlas", "", Localization.Get("xuiReplaceBlockRemovePaint"), itemActionReplaceBlockData.ReplacePaintMode == EnumReplacePaintMode.RemoveCurrentPaint);
		_xuiRadialWindow.CreateRadialEntry(4, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiReplaceBlockUseNewPaint"), itemActionReplaceBlockData.ReplacePaintMode == EnumReplacePaintMode.UseNewPaint);
		_xuiRadialWindow.CreateRadialEntry(5, "ui_game_symbol_x", "UIAtlas", "", Localization.Get("xuiReplaceBlockPlaceAir"), itemActionReplaceBlockData.ReplacePaintMode == EnumReplacePaintMode.ReplaceWithAirBlocks);
		_xuiRadialWindow.SetCommonData(UIUtils.GetButtonIconForAction(_epl.playerInput.Activate), handleRadialCommand, new XUiC_Radial.RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, _hasSpecialActionPriorToRadialVisibility: false, XUiC_Radial.RadialValidSameHoldingSlotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		ItemClass holdingItem = entityPlayer.inventory.holdingItem;
		ItemInventoryData holdingItemData = entityPlayer.inventory.holdingItemData;
		_ = (ItemActionReplaceBlock)holdingItem.Actions[0];
		_ = (ItemActionReplaceBlock)holdingItem.Actions[1];
		_ = (ItemActionReplaceBlockData)holdingItemData.actionData[0];
		ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlockData)holdingItemData.actionData[1];
		switch (_commandIndex)
		{
		case 0:
			itemActionReplaceBlockData.ReplaceMode = EnumReplaceMode.SingleBlock;
			break;
		case 1:
			itemActionReplaceBlockData.ReplaceMode = EnumReplaceMode.AllIdenticalBlocks;
			break;
		case 2:
			itemActionReplaceBlockData.ReplacePaintMode = EnumReplacePaintMode.KeepCurrentPaint;
			break;
		case 3:
			itemActionReplaceBlockData.ReplacePaintMode = EnumReplacePaintMode.RemoveCurrentPaint;
			break;
		case 4:
			itemActionReplaceBlockData.ReplacePaintMode = EnumReplacePaintMode.UseNewPaint;
			break;
		case 5:
			itemActionReplaceBlockData.ReplacePaintMode = EnumReplacePaintMode.ReplaceWithAirBlocks;
			break;
		}
		isHUDDirty = true;
	}
}
