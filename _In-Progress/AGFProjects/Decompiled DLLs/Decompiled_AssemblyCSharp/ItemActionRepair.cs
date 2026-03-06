using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionRepair : ItemActionAttack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum EnumRepairType
	{
		None,
		Repair,
		Upgrade
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class InventoryDataRepair(ItemInventoryData _invData, int _indexInEntityOfAction) : ItemActionAttackData(_invData, _indexInEntityOfAction)
	{
		public new bool uiOpenedByMe;

		public EnumRepairType repairType;

		public float blockDamagePerc;

		public bool bUseStarted;

		public float upgradePerc;

		public BlockValue lastHitBlockValue;

		public Vector3i lastHitPosition = Vector3i.zero;

		public List<Block.SItemNameCount> lastRepairItems;

		public float[] lastRepairItemsPercents;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue targetBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float repairAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitCountOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float soundAnimActionSyncTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const float SOUND_LENGTH = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUpgradeItem = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockTargetClrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lastBlockTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockUpgradeCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpgradeCountChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public string repairActionSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string upgradeActionSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string allowedUpgradeItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string restrictedUpgradeItems;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new InventoryDataRepair(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		repairAmount = 0f;
		_props.ParseFloat("Repair_amount", ref repairAmount);
		hitCountOffset = 0f;
		_props.ParseFloat("Upgrade_hit_offset", ref hitCountOffset);
		repairActionSound = _props.GetString("Repair_action_sound");
		upgradeActionSound = _props.GetString("Upgrade_action_sound");
		allowedUpgradeItems = _props.GetString("Allowed_upgrade_items");
		restrictedUpgradeItems = _props.GetString("Restricted_upgrade_items");
		soundAnimActionSyncTimer = 0.3f;
	}

	public override void StopHolding(ItemActionData _data)
	{
		((InventoryDataRepair)_data).bUseStarted = false;
		bUpgradeCountChanged = false;
		blockUpgradeCount = 0;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
		if (uIForPlayer != null)
		{
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		((InventoryDataRepair)_data).bUseStarted = false;
		bUpgradeCountChanged = false;
		blockUpgradeCount = 0;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		if (_actionData.invData.hitInfo.bHitValid && _actionData.invData.hitInfo.hit.distanceSq > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		GUIWindowManager windowManager = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).windowManager;
		InventoryDataRepair inventoryDataRepair = (InventoryDataRepair)_actionData;
		if (windowManager.IsModalWindowOpen())
		{
			inventoryDataRepair.bUseStarted = false;
			inventoryDataRepair.repairType = EnumRepairType.None;
		}
		else
		{
			if (_actionData.invData.holdingEntity != _actionData.invData.world.GetPrimaryPlayer() || !inventoryDataRepair.bUseStarted)
			{
				return;
			}
			if (bUpgradeCountChanged)
			{
				BlockValue block = _actionData.invData.world.GetBlock(blockTargetPos);
				Block block2 = block.Block;
				if (int.TryParse(block2.Properties.Values["UpgradeBlock.UpgradeHitCount"], out var result))
				{
					result = (int)(((float)result + hitCountOffset < 1f) ? 1f : ((float)result + hitCountOffset));
					inventoryDataRepair.upgradePerc = (float)blockUpgradeCount / (float)result;
					if (blockUpgradeCount >= result)
					{
						if (RemoveRequiredResource(_actionData.invData, block))
						{
							BlockValue blockValue = Block.GetBlockValue(block2.Properties.Values[Block.PropUpgradeBlockClassToBlock]);
							blockValue.rotation = block.rotation;
							blockValue.meta = block.meta;
							QuestEventManager.Current.BlockUpgraded(block2.GetBlockName(), blockTargetPos);
							_actionData.invData.holdingEntity.MinEventContext.ItemActionData = _actionData;
							_actionData.invData.holdingEntity.MinEventContext.BlockValue = blockValue;
							_actionData.invData.holdingEntity.MinEventContext.Position = blockTargetPos.ToVector3();
							_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfUpgradedBlock);
							Block block3 = block.Block;
							block3.DamageBlock(_actionData.invData.world, blockTargetClrIdx, blockTargetPos, block, -1, _actionData.invData.holdingEntity.entityId);
							if (int.TryParse(block2.Properties.Values[Block.PropUpgradeBlockClassItemCount], out var result2))
							{
								_actionData.invData.holdingEntity.Progression.AddLevelExp((int)(blockValue.Block.blockMaterial.Experience * (float)result2), "_xpFromUpgradeBlock", Progression.XPTypes.Upgrading);
							}
							if (block3.UpgradeSound != null)
							{
								_actionData.invData.holdingEntity.PlayOneShot(block3.UpgradeSound);
							}
						}
						blockUpgradeCount = 0;
					}
				}
				string text = upgradeActionSound;
				string upgradeItemName = GetUpgradeItemName(block2);
				if (text.Length == 0 && item != null && upgradeItemName != null && upgradeItemName.Length > 0)
				{
					text = $"ImpactSurface/{_actionData.invData.holdingEntity.inventory.holdingItem.MadeOfMaterial.SurfaceCategory}hit{ItemClass.GetForId(ItemClass.GetItem(upgradeItemName).type).MadeOfMaterial.SurfaceCategory}";
				}
				if (text.Length > 0)
				{
					_actionData.invData.holdingEntity.PlayOneShot(text);
				}
				bUpgradeCountChanged = false;
			}
			else
			{
				ExecuteAction(_actionData, _bReleased: false);
			}
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (_bReleased)
		{
			((InventoryDataRepair)_actionData).bUseStarted = false;
			((InventoryDataRepair)_actionData).repairType = EnumRepairType.None;
			return;
		}
		if ((bool)entityPlayerLocal)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
		}
		if (Time.time - _actionData.lastUseTime < Delay)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (invData.hitInfo.bHitValid && invData.hitInfo.hit.distanceSq > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			return;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, entityPlayerLocal.inventory.holdingItemItemValue, 0f, entityPlayerLocal, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return;
		}
		_actionData.lastUseTime = Time.time;
		if ((invData.hitInfo.bHitValid && _actionData.invData.world.IsWithinTraderArea(invData.hitInfo.hit.blockPos)) || !invData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(invData.hitInfo.tag))
		{
			return;
		}
		blockTargetPos = invData.hitInfo.hit.blockPos;
		blockTargetClrIdx = invData.hitInfo.hit.clrIdx;
		BlockValue block = invData.world.GetBlock(blockTargetPos);
		if (block.ischild)
		{
			blockTargetPos = block.Block.multiBlockPos.GetParentPos(blockTargetPos, block);
			block = _actionData.invData.world.GetBlock(blockTargetPos);
		}
		if ((invData.itemValue.MaxUseTimes > 0 && invData.itemValue.UseTimes >= (float)invData.itemValue.MaxUseTimes) || (invData.itemValue.UseTimes == 0f && invData.itemValue.MaxUseTimes == 0))
		{
			if (item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(item.Properties.Values[ItemClass.PropSoundJammed]);
			}
			GameManager.ShowTooltip(entityPlayerLocal, "ttItemNeedsRepair");
			return;
		}
		InventoryDataRepair inventoryDataRepair = (InventoryDataRepair)_actionData;
		Block block2 = block.Block;
		if (block2.CanRepair(block))
		{
			int num = Utils.FastMin((int)repairAmount, block.damage);
			float num2 = (float)num / (float)block2.MaxDamage;
			List<Block.SItemNameCount> list = block2.RepairItems;
			if (block2.RepairItemsMeshDamage != null && block2.shape.UseRepairDamageState(block))
			{
				num = 1;
				num2 = 1f;
				list = block2.RepairItemsMeshDamage;
			}
			if (list == null)
			{
				return;
			}
			if (inventoryDataRepair.lastHitPosition != blockTargetPos || inventoryDataRepair.lastHitBlockValue.type != block.type || inventoryDataRepair.lastRepairItems != list)
			{
				inventoryDataRepair.lastHitPosition = blockTargetPos;
				inventoryDataRepair.lastHitBlockValue = block;
				inventoryDataRepair.lastRepairItems = list;
				inventoryDataRepair.lastRepairItemsPercents = new float[list.Count];
			}
			inventoryDataRepair.blockDamagePerc = (float)block.damage / (float)block2.MaxDamage;
			EntityPlayerLocal entityPlayerLocal2 = inventoryDataRepair.invData.holdingEntity as EntityPlayerLocal;
			if (entityPlayerLocal2 == null)
			{
				return;
			}
			inventoryDataRepair.repairType = EnumRepairType.Repair;
			float resourceScale = block2.ResourceScale;
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				string itemName = list[i].ItemName;
				float num3 = (float)list[i].Count * num2 * resourceScale;
				if (!(inventoryDataRepair.lastRepairItemsPercents[i] <= 0f))
				{
					continue;
				}
				int count = Utils.FastMax((int)num3, 1);
				ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemName), count);
				if (!canRemoveRequiredItem(inventoryDataRepair.invData, itemStack))
				{
					itemStack.count = 0;
					entityPlayerLocal2.AddUIHarvestingItem(itemStack, _bAddOnlyIfNotExisting: true);
					if (!flag)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				return;
			}
			inventoryDataRepair.invData.holdingEntity.RightArmAnimationUse = true;
			float num4 = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				float num5 = (float)list[j].Count * num2 * resourceScale;
				if (inventoryDataRepair.lastRepairItemsPercents[j] <= 0f)
				{
					string itemName2 = list[j].ItemName;
					int num6 = Utils.FastMax((int)num5, 1);
					inventoryDataRepair.lastRepairItemsPercents[j] += num6;
					inventoryDataRepair.lastRepairItemsPercents[j] -= num5;
					ItemStack itemStack2 = new ItemStack(ItemClass.GetItem(itemName2), num6);
					num4 += itemStack2.itemValue.ItemClass.MadeOfMaterial.Experience * (float)num6;
					removeRequiredItem(inventoryDataRepair.invData, itemStack2);
					itemStack2.count *= -1;
					entityPlayerLocal2.AddUIHarvestingItem(itemStack2);
				}
				else
				{
					inventoryDataRepair.lastRepairItemsPercents[j] -= num5;
				}
			}
			if (repairActionSound != null && repairActionSound.Length > 0)
			{
				invData.holdingEntity.PlayOneShot(repairActionSound);
			}
			else if (soundStart != null && soundStart.Length > 0)
			{
				invData.holdingEntity.PlayOneShot(soundStart);
			}
			if (invData.itemValue.MaxUseTimes > 0)
			{
				invData.itemValue.UseTimes += 1f;
			}
			int num7 = block.Block.DamageBlock(invData.world, invData.hitInfo.hit.clrIdx, blockTargetPos, block, -num, invData.holdingEntity.entityId);
			inventoryDataRepair.bUseStarted = true;
			inventoryDataRepair.blockDamagePerc = (float)num7 / (float)block.Block.MaxDamage;
			inventoryDataRepair.invData.holdingEntity.MinEventContext.ItemActionData = inventoryDataRepair;
			inventoryDataRepair.invData.holdingEntity.MinEventContext.BlockValue = block;
			inventoryDataRepair.invData.holdingEntity.MinEventContext.Position = blockTargetPos.ToVector3();
			inventoryDataRepair.invData.holdingEntity.FireEvent(MinEventTypes.onSelfRepairBlock);
			entityPlayerLocal2.Progression.AddLevelExp((int)num4, "_xpFromRepairBlock", Progression.XPTypes.Repairing);
		}
		else if (isUpgradeItem)
		{
			if (!CanRemoveRequiredResource(_actionData.invData, block))
			{
				string upgradeItemName = GetUpgradeItemName(block.Block);
				if (upgradeItemName != null)
				{
					ItemStack itemStack3 = new ItemStack(ItemClass.GetItem(upgradeItemName), 0);
					(_actionData.invData.holdingEntity as EntityPlayerLocal).AddUIHarvestingItem(itemStack3, _bAddOnlyIfNotExisting: true);
				}
				inventoryDataRepair.upgradePerc = 0f;
				return;
			}
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			inventoryDataRepair.repairType = EnumRepairType.Upgrade;
			if (blockTargetPos == lastBlockTargetPos)
			{
				blockUpgradeCount++;
			}
			else
			{
				blockUpgradeCount = 1;
			}
			lastBlockTargetPos = blockTargetPos;
			bUpgradeCountChanged = true;
			inventoryDataRepair.bUseStarted = true;
		}
		else
		{
			inventoryDataRepair.bUseStarted = false;
			inventoryDataRepair.repairType = EnumRepairType.None;
		}
	}

	public float GetRepairAmount()
	{
		return repairAmount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetUpgradeItemName(Block block)
	{
		string text = block.Properties.Values["UpgradeBlock.Item"];
		if (text != null && text.Length == 1 && text[0] == 'r')
		{
			text = block.RepairItems[0].ItemName;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CanRemoveRequiredResource(ItemInventoryData data, BlockValue blockValue)
	{
		Block block = blockValue.Block;
		string upgradeItemName = GetUpgradeItemName(block);
		bool flag = upgradeItemName != null && upgradeItemName.Length > 0;
		if (flag)
		{
			if (allowedUpgradeItems.Length > 0 && !allowedUpgradeItems.ContainsCaseInsensitive(upgradeItemName))
			{
				return false;
			}
			if (restrictedUpgradeItems.Length > 0 && restrictedUpgradeItems.ContainsCaseInsensitive(upgradeItemName))
			{
				return false;
			}
		}
		if (!int.TryParse(block.Properties.Values["UpgradeBlock.UpgradeHitCount"], out var _))
		{
			return false;
		}
		if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out var result2) && flag)
		{
			return false;
		}
		if (block.GetBlockName() != null && flag)
		{
			ItemValue itemValue = ItemClass.GetItem(upgradeItemName);
			if (data.holdingEntity.inventory.GetItemCount(itemValue) >= result2)
			{
				return true;
			}
			if (data.holdingEntity.bag.GetItemCount(itemValue) >= result2)
			{
				return true;
			}
		}
		else if (!flag)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RemoveRequiredResource(ItemInventoryData data, BlockValue blockValue)
	{
		if (!CanRemoveRequiredResource(data, blockValue))
		{
			return false;
		}
		Block block = blockValue.Block;
		ItemValue itemValue = ItemClass.GetItem(GetUpgradeItemName(block));
		if (!int.TryParse(block.Properties.Values[Block.PropUpgradeBlockClassItemCount], out var result))
		{
			return false;
		}
		if (data.holdingEntity.inventory.DecItem(itemValue, result) == result)
		{
			EntityPlayerLocal entityPlayerLocal = data.holdingEntity as EntityPlayerLocal;
			if (entityPlayerLocal != null && result != 0)
			{
				entityPlayerLocal.AddUIHarvestingItem(new ItemStack(itemValue, -result));
			}
			return true;
		}
		if (data.holdingEntity.bag.DecItem(itemValue, result) == result)
		{
			EntityPlayerLocal entityPlayerLocal2 = data.holdingEntity as EntityPlayerLocal;
			if (entityPlayerLocal2 != null)
			{
				entityPlayerLocal2.AddUIHarvestingItem(new ItemStack(itemValue, -result));
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRemoveRequiredItem(ItemInventoryData _data, ItemStack _itemStack)
	{
		if (_data.holdingEntity.inventory.GetItemCount(_itemStack.itemValue) >= _itemStack.count)
		{
			return true;
		}
		if (_data.holdingEntity.bag.GetItemCount(_itemStack.itemValue) >= _itemStack.count)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeRequiredItem(ItemInventoryData _data, ItemStack _itemStack)
	{
		if (_data.holdingEntity.inventory.DecItem(_itemStack.itemValue, _itemStack.count) == _itemStack.count)
		{
			return true;
		}
		if (_data.holdingEntity.bag.DecItem(_itemStack.itemValue, _itemStack.count) == _itemStack.count)
		{
			return true;
		}
		return false;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (!CharacterCameraAngleValid(_actionData, out var _))
		{
			return ItemClass.EnumCrosshairType.None;
		}
		return ((InventoryDataRepair)_actionData).repairType switch
		{
			EnumRepairType.Repair => ItemClass.EnumCrosshairType.Repair, 
			EnumRepairType.Upgrade => ItemClass.EnumCrosshairType.Upgrade, 
			_ => ItemClass.EnumCrosshairType.Plus, 
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData _actionData)
	{
		WorldRayHitInfo hitInfo = _actionData.invData.hitInfo;
		if (hitInfo.bHitValid && hitInfo.hit.distanceSq > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			return false;
		}
		bool result = false;
		InventoryDataRepair inventoryDataRepair = (InventoryDataRepair)_actionData;
		if (inventoryDataRepair.repairType == EnumRepairType.None)
		{
			if (hitInfo.bHitValid)
			{
				int damage;
				if (!hitInfo.hit.blockValue.ischild)
				{
					damage = hitInfo.hit.blockValue.damage;
				}
				else
				{
					Vector3i parentPos = hitInfo.hit.blockValue.Block.multiBlockPos.GetParentPos(hitInfo.hit.blockPos, hitInfo.hit.blockValue);
					damage = _actionData.invData.world.GetBlock(parentPos).damage;
				}
				result = damage > 0;
			}
		}
		else if (inventoryDataRepair.repairType == EnumRepairType.Repair)
		{
			EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
			result = entityPlayerLocal != null && entityPlayerLocal.HitInfo.bHitValid && Time.time - _actionData.lastUseTime <= 1.5f;
		}
		else if (inventoryDataRepair.repairType == EnumRepairType.Upgrade)
		{
			EntityPlayerLocal entityPlayerLocal2 = _actionData.invData.holdingEntity as EntityPlayerLocal;
			result = entityPlayerLocal2 != null && entityPlayerLocal2.HitInfo.bHitValid && Time.time - _actionData.lastUseTime <= 1.5f && inventoryDataRepair.upgradePerc > 0f;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getOverlayData(ItemActionData _actionData, out float _perc, out string _text)
	{
		InventoryDataRepair inventoryDataRepair = (InventoryDataRepair)_actionData;
		if (inventoryDataRepair.repairType == EnumRepairType.None)
		{
			BlockValue blockValue = _actionData.invData.hitInfo.hit.blockValue;
			if (blockValue.ischild)
			{
				Vector3i parentPos = blockValue.Block.multiBlockPos.GetParentPos(_actionData.invData.hitInfo.hit.blockPos, blockValue);
				blockValue = _actionData.invData.world.GetBlock(parentPos);
			}
			int shownMaxDamage = blockValue.Block.GetShownMaxDamage();
			_perc = ((float)shownMaxDamage - (float)blockValue.damage) / (float)shownMaxDamage;
			_text = $"{Utils.FastMax(0, shownMaxDamage - blockValue.damage)}/{shownMaxDamage}";
		}
		else if (inventoryDataRepair.repairType == EnumRepairType.Repair)
		{
			_perc = 1f - inventoryDataRepair.blockDamagePerc;
			_text = string.Format("{0}%", (_perc * 100f).ToCultureInvariantString("0"));
		}
		else if (inventoryDataRepair.repairType == EnumRepairType.Upgrade)
		{
			_perc = inventoryDataRepair.upgradePerc;
			_text = string.Format("{0}%", (_perc * 100f).ToCultureInvariantString("0"));
		}
		else
		{
			_perc = 0f;
			_text = string.Empty;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		InventoryDataRepair inventoryDataRepair = (InventoryDataRepair)_actionData;
		if (Time.time - inventoryDataRepair.lastUseTime < Delay + 0.1f)
		{
			return true;
		}
		return false;
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		base.GetItemValueActionInfo(ref _infoList, _itemValue, _xui, _actionIndex);
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblBlkRpr"), GetRepairAmount().ToCultureInvariantString()));
	}
}
