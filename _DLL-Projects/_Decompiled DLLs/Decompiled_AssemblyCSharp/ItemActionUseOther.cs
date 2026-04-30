using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionUseOther : ItemAction
{
	public class FeedInventoryData : ItemActionAttackData
	{
		public bool bFeedingStarted;

		public EntityAlive TargetEntity;

		public Ray ray;

		public FeedInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public new string CreateItem;

	public int CreateItemCount;

	public new bool Consume;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> noMedBuffsTag = FastTags<TagGroup.Global>.Parse("noMedBuffs");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> medicalItemTag = FastTags<TagGroup.Global>.Parse("medical");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> stopBleed = FastTags<TagGroup.Global>.Parse("stopsBleeding");

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new FeedInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Consume"))
		{
			Consume = StringParsers.ParseBool(_props.Values["Consume"]);
		}
		else
		{
			Consume = true;
		}
		if (_props.Values.ContainsKey("Create_item"))
		{
			CreateItem = _props.Values["Create_item"];
			if (_props.Values.ContainsKey("Create_item_count"))
			{
				CreateItemCount = int.Parse(_props.Values["Create_item_count"]);
			}
			else
			{
				CreateItemCount = 1;
			}
		}
		else
		{
			CreateItem = null;
			CreateItemCount = 0;
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		FeedInventoryData obj = (FeedInventoryData)_data;
		obj.bFeedingStarted = false;
		obj.TargetEntity = null;
		if (_data.invData.holdingEntity is EntityPlayerLocal)
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
			_ = uIForPlayer.nguiWindowManager;
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
		}
	}

	public override bool CanExecute(ItemActionData _actionData)
	{
		if (Time.time - _actionData.lastUseTime < Delay)
		{
			return false;
		}
		FeedInventoryData feedInventoryData = (FeedInventoryData)_actionData;
		EntityAlive holdingEntity = feedInventoryData.invData.holdingEntity;
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2);
		float distance = 4f;
		feedInventoryData.ray = holdingEntity.GetLookRay();
		EntityAlive entityAlive = null;
		if (Voxel.Raycast(feedInventoryData.invData.world, feedInventoryData.ray, distance, -538750981, 128, SphereRadius))
		{
			entityAlive = GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
		}
		if (entityAlive == null || !entityAlive.IsAlive() || !(entityAlive is EntityPlayer))
		{
			Voxel.Raycast(feedInventoryData.invData.world, feedInventoryData.ray, distance, -538488837, 128, SphereRadius);
		}
		holdingEntity.SetModelLayer(modelLayer);
		if (feedInventoryData.TargetEntity == null)
		{
			feedInventoryData.TargetEntity = entityAlive;
		}
		_actionData.invData.holdingEntity.MinEventContext.Other = feedInventoryData.TargetEntity;
		_actionData.invData.holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemValue;
		return base.CanExecute(_actionData);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return;
		}
		FeedInventoryData feedInventoryData = (FeedInventoryData)_actionData;
		_actionData.lastUseTime = Time.time;
		feedInventoryData.bFeedingStarted = true;
		if (feedInventoryData.TargetEntity == null)
		{
			return;
		}
		if (feedInventoryData.invData.item.HasAnyTags(medicalItemTag) && feedInventoryData.TargetEntity as EntityPlayer == null)
		{
			feedInventoryData.bFeedingStarted = false;
			feedInventoryData.TargetEntity = null;
			return;
		}
		if (feedInventoryData.invData.item.HasAnyTags(medicalItemTag) && feedInventoryData.TargetEntity.HasAnyTags(noMedBuffsTag))
		{
			feedInventoryData.bFeedingStarted = false;
			feedInventoryData.TargetEntity = null;
			return;
		}
		holdingEntity.RightArmAnimationUse = true;
		if (soundStart != null)
		{
			holdingEntity.PlayOneShot(soundStart);
		}
		holdingEntity.MinEventContext.Other = feedInventoryData.TargetEntity;
		holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemValue;
		holdingEntity.FireEvent(MinEventTypes.onSelfHealedOther);
		holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionEnd : MinEventTypes.onSelfSecondaryActionEnd);
		if (_actionData.invData.itemValue.ItemClass.HasAnyTags(stopBleed) && feedInventoryData.TargetEntity.entityType == EntityType.Player && feedInventoryData.TargetEntity.Buffs.HasBuff("buffInjuryBleeding"))
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.BleedOutStopped, 1);
		}
		ItemAction.ExecuteBuffActions(getBuffActions(_actionData), feedInventoryData.TargetEntity.entityId, feedInventoryData.TargetEntity, isCritical: false, EnumBodyPartHit.None, null);
		EntityPlayer entityPlayer = holdingEntity as EntityPlayer;
		if (Consume)
		{
			if (_actionData.invData.itemValue.MaxUseTimes > 0 && _actionData.invData.itemValue.UseTimes + 1f < (float)_actionData.invData.itemValue.MaxUseTimes)
			{
				ItemValue itemValue = _actionData.invData.itemValue;
				itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, feedInventoryData.invData.itemValue, 1f, holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags);
				feedInventoryData.invData.itemValue = itemValue;
				return;
			}
			holdingEntity.inventory.DecHoldingItem(1);
		}
		if (CreateItem != null && CreateItemCount > 0)
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(CreateItem), CreateItemCount);
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayer as EntityPlayerLocal);
			if (null != uIForPlayer && !uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
			{
				holdingEntity.world.gameManager.ItemDropServer(itemStack, holdingEntity.GetPosition(), Vector3.zero);
			}
		}
		feedInventoryData.bFeedingStarted = false;
		feedInventoryData.TargetEntity = null;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		FeedInventoryData feedInventoryData = (FeedInventoryData)_actionData;
		if (feedInventoryData.bFeedingStarted && Time.time - feedInventoryData.lastUseTime < Delay)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		_ = (FeedInventoryData)_actionData;
	}

	public static Entity GetEntityFromHit(WorldRayHitInfo hitInfo)
	{
		Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
		if (hitRootTransform != null)
		{
			return hitRootTransform.GetComponent<Entity>();
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData _actionData)
	{
		return isValidEntityToHeal((FeedInventoryData)_actionData);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (isValidEntityToHeal((FeedInventoryData)_actionData))
		{
			return ItemClass.EnumCrosshairType.Heal;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData _actionData)
	{
		return isValidEntityToHeal((FeedInventoryData)_actionData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getOverlayData(ItemActionData _actionData, out float _perc, out string _text)
	{
		FeedInventoryData feedInventoryData = (FeedInventoryData)_actionData;
		if (!isValidEntityToHeal(feedInventoryData))
		{
			base.getOverlayData(_actionData, out _perc, out _text);
			return;
		}
		_perc = feedInventoryData.TargetEntity.Stats.Health.Value / feedInventoryData.TargetEntity.Stats.Health.Max;
		_text = $"{feedInventoryData.TargetEntity.Stats.Health.Value.ToCultureInvariantString()}/{feedInventoryData.TargetEntity.Stats.Health.Max.ToCultureInvariantString()}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidEntityToHeal(FeedInventoryData _actionData)
	{
		return _actionData.TargetEntity != null;
	}

	public override void OnHUD(ItemActionData _actionData, int _x, int _y)
	{
		FeedInventoryData feedInventoryData = (FeedInventoryData)_actionData;
		if (feedInventoryData == null)
		{
			return;
		}
		if (!canShowOverlay(feedInventoryData))
		{
			XUiC_FocusedBlockHealth.SetData(LocalPlayerUI.GetUIForPrimaryPlayer(), null, 0f);
		}
		else
		{
			if (!(feedInventoryData.invData.holdingEntity is EntityPlayerLocal))
			{
				return;
			}
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer((EntityPlayerLocal)feedInventoryData.invData.holdingEntity);
			if (!isShowOverlay(feedInventoryData))
			{
				if (feedInventoryData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
				{
					XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
					feedInventoryData.uiOpenedByMe = false;
				}
				return;
			}
			if (!XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
			{
				feedInventoryData.uiOpenedByMe = true;
			}
			getOverlayData(feedInventoryData, out var _perc, out var _text);
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, _text, _perc);
		}
	}
}
