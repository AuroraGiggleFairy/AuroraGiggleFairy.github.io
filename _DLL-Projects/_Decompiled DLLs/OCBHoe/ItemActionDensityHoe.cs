using System.Collections.Generic;

public class ItemActionDensityHoe : ItemActionDynamicMelee
{
	public enum DensityAction : byte
	{
		LevelAray,
		FillDensity
	}

	private string SoundHoe = string.Empty;

	private DensityAction ActionType;

	public float GetBlockRange()
	{
		return BlockRange;
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		_props.ParseString("SoundHoe", ref SoundHoe);
		string optionalValue = DensityAction.LevelAray.ToString();
		_props.ParseString("ActionType", ref optionalValue);
		ActionType = EnumUtils.Parse(optionalValue, DensityAction.LevelAray, _ignoreCase: true);
		base.ReadFrom(_props);
	}

	public override void hitTarget(ItemActionData action, WorldRayHitInfo hitInfo, bool isGrazingHit)
	{
		ItemInventoryData invData = action.invData;
		if (!IsHitValid(invData, out var density) || !ExecuteDensityHoe(invData, density))
		{
			base.hitTarget(action, hitInfo, isGrazingHit);
		}
	}

	public override RenderCubeType GetFocusType(ItemActionData action)
	{
		if (!IsHitValid(action))
		{
			return base.GetFocusType(action);
		}
		return RenderCubeType.FaceTop;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData action)
	{
		if (!IsHitValid(action))
		{
			return base.GetCrosshairType(action);
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override bool isShowOverlay(ItemActionData action)
	{
		if (!IsHitValid(action))
		{
			return base.isShowOverlay(action);
		}
		return true;
	}

	public override void getOverlayData(ItemActionData action, out float _perc, out string _text)
	{
		if (IsHitValid(action.invData, out var density))
		{
			_text = Localization.Get(IsPlayerCrouching(action.invData) ? "ttDensityHoeActionCrouched" : "ttDensityHoeAction");
			if (density < 0)
			{
				_perc = (float)density / -128f;
			}
			else if (density > 0)
			{
				_perc = (float)density / 127f;
			}
			else
			{
				_perc = 0f;
			}
		}
		else
		{
			base.getOverlayData(action, out _perc, out _text);
		}
	}

	private static bool IsPlayerCrouching(ItemInventoryData invData)
	{
		return ((invData.holdingEntity as EntityPlayerLocal)?.vp_FPController?.Player?.Crouch.Active).Value;
	}

	private bool ExecuteDensityHoe(ItemInventoryData invData, sbyte hitDensity)
	{
		WorldRayHitInfo hitInfo = invData.hitInfo;
		List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
		int clrIdx = hitInfo.hit.clrIdx;
		Vector3i blockPos = hitInfo.hit.blockPos;
		BlockValue blockValue = hitInfo.hit.blockValue;
		if (ActionType == DensityAction.LevelAray)
		{
			changes.Add(new BlockChangeInfo(clrIdx, blockPos, blockValue, invData.world.GetDensity(clrIdx, blockPos)));
		}
		GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.forward, ref changes, ActionType == DensityAction.FillDensity);
		GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.right, ref changes, ActionType == DensityAction.FillDensity);
		GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.back, ref changes, ActionType == DensityAction.FillDensity);
		GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.left, ref changes, ActionType == DensityAction.FillDensity);
		if (((invData.holdingEntity as EntityPlayerLocal)?.vp_FPController?.Player?.Crouch.Active).Value)
		{
			GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.forward + Vector3i.right, ref changes, ActionType == DensityAction.FillDensity);
			GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.back + Vector3i.right, ref changes, ActionType == DensityAction.FillDensity);
			GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.back + Vector3i.left, ref changes, ActionType == DensityAction.FillDensity);
			GatherNeighbours(invData.world, clrIdx, blockPos + Vector3i.forward + Vector3i.left, ref changes, ActionType == DensityAction.FillDensity);
		}
		if (ActionType == DensityAction.LevelAray)
		{
			int num = 0;
			foreach (BlockChangeInfo item in changes)
			{
				num += item.density;
			}
			float num2 = num / changes.Count;
			foreach (BlockChangeInfo item2 in changes)
			{
				item2.density = (sbyte)(0.35f * num2 + 0.65f * (float)item2.density);
				num -= item2.density;
			}
			int num3 = changes[0].density + num;
			if (num3 < -128)
			{
				changes[0].density = sbyte.MinValue;
			}
			else if (num3 > 127)
			{
				changes[0].density = sbyte.MaxValue;
			}
			else
			{
				changes[0].density = (sbyte)num3;
			}
		}
		if (ActionType == DensityAction.FillDensity)
		{
			foreach (BlockChangeInfo item3 in changes)
			{
				item3.density = (sbyte)((item3.density + -55) / 2);
			}
		}
		if (changes.Count > 0)
		{
			invData.world.SetBlocksRPC(changes);
		}
		if (changes.Count > 0 && SoundHoe != null)
		{
			invData.holdingEntity.PlayOneShot(SoundHoe);
		}
		return true;
	}

	private void GatherNeighbours(World world, int clrIdx, Vector3i position, ref List<BlockChangeInfo> changes, bool isFillAction)
	{
		BlockValue block = world.GetBlock(position);
		if (!block.isair && !block.isWater)
		{
			bool flag = block.Block.shape.IsTerrain();
			if (!(isFillAction && flag) && (isFillAction || flag) && IsDensitySupported(block.Block))
			{
				sbyte density = world.GetDensity(clrIdx, position);
				changes.Add(new BlockChangeInfo(position, block, density));
			}
		}
	}

	private static bool IsDensitySupported(Block block)
	{
		if (block.shape.IsSolidCube)
		{
			return true;
		}
		string blockName = block.GetBlockName();
		string autoShapeShapeName = block.GetAutoShapeShapeName();
		if (autoShapeShapeName != null && autoShapeShapeName.StartsWith("cube"))
		{
			return true;
		}
		if (!blockName.StartsWith("farmPlotBlock") && !blockName.EqualsCaseInsensitive("glassBusinessBlock"))
		{
			return block.Properties.GetBool("DensitySupport");
		}
		return true;
	}

	private bool IsHitValid(ItemInventoryData invData, out sbyte density)
	{
		density = MarchingCubes.DensityAir;
		WorldRayHitInfo hitInfo = invData.hitInfo;
		if (!hitInfo.bHitValid)
		{
			return false;
		}
		if (!GameUtils.IsBlockOrTerrain(hitInfo.tag))
		{
			return false;
		}
		int clrIdx = hitInfo.hit.clrIdx;
		Vector3i blockPos = hitInfo.hit.blockPos;
		BlockValue blockValue = hitInfo.hit.blockValue;
		density = invData.world.GetDensity(clrIdx, blockPos);
		if (density >= 0 || !blockValue.Block.shape.IsTerrain())
		{
			return false;
		}
		return hitInfo.hit.distanceSq <= BlockRange * BlockRange;
	}

	private bool IsHitValid(ItemActionData action)
	{
		sbyte density;
		return IsHitValid(action.invData, out density);
	}
}
