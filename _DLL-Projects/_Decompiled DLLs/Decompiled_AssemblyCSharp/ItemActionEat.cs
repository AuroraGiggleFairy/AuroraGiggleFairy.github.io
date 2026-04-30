using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEat : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public bool bEatingStarted;

		public bool bPromptChecked;

		public bool bEatingFinished;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public new string CreateItem;

	public int CreateItemCount;

	public new bool Consume;

	public HashSet<int> ConditionBlockTypes;

	public bool UsePrompt;

	public bool UseJarRefund;

	public string PromptDescription;

	public string PromptTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellUse;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		Consume = true;
		_props.ParseBool("Consume", ref Consume);
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
			if (_props.Values.ContainsKey("Use_jar_refund"))
			{
				UseJarRefund = StringParsers.ParseBool(_props.Values["Use_jar_refund"]);
			}
		}
		else
		{
			CreateItem = null;
			CreateItemCount = 0;
			UseJarRefund = false;
		}
		string text = _props.GetString("BlocksAllowed");
		if (text.Length > 0)
		{
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				Block blockByName = Block.GetBlockByName(text2, _caseInsensitive: true);
				if (blockByName == null)
				{
					Log.Error("ItemActionEat BlocksAllowed invalid {0}", text2);
					continue;
				}
				if (ConditionBlockTypes == null)
				{
					ConditionBlockTypes = new HashSet<int>();
				}
				ConditionBlockTypes.Add(blockByName.blockID);
			}
			if (ConditionBlockTypes != null && ConditionBlockTypes.Count == 0)
			{
				ConditionBlockTypes = null;
			}
		}
		_props.ParseString("PromptDescription", ref PromptDescription);
		_props.ParseString("PromptTitle", ref PromptTitle);
		_props.ParseFloat("SmellUse", ref smellUse);
		if (PromptDescription != null)
		{
			UsePrompt = true;
		}
	}

	public override string CanInteract(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.isHeadUnderwater && IsValidConditions(_actionData))
		{
			return "lblContextActionDrink";
		}
		return null;
	}

	public bool NeedPrompt(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (UsePrompt)
		{
			return !myInventoryData.bPromptChecked;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidConditions(ItemActionData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (ConditionBlockTypes != null)
		{
			Ray lookRay = holdingEntity.GetLookRay();
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2);
			Voxel.Raycast(_actionData.invData.world, lookRay, 2.5f, 131, (holdingEntity is EntityPlayer) ? 0.2f : 0.4f);
			holdingEntity.SetModelLayer(modelLayer);
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!GameUtils.IsBlockOrTerrain(voxelRayHitInfo.tag))
			{
				return false;
			}
			BlockValue blockValue = voxelRayHitInfo.hit.blockValue;
			bool flag = false;
			foreach (int conditionBlockType in ConditionBlockTypes)
			{
				if (conditionBlockType == 240)
				{
					flag = true;
					break;
				}
			}
			if (flag ? (!voxelRayHitInfo.hit.waterValue.HasMass()) : (!ConditionBlockTypes.Contains(blockValue.type)))
			{
				return false;
			}
		}
		return true;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased || Time.time - _actionData.lastUseTime < Delay || IsActionRunning(_actionData) || !IsValidConditions(_actionData))
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
		_actionData.lastUseTime = Time.time;
		if (UseAnimation)
		{
			if (holdingEntity.emodel != null && holdingEntity.emodel.avatarController != null)
			{
				holdingEntity.emodel.avatarController.SetMeleeAttackSpeed(1f);
			}
			holdingEntity.RightArmAnimationUse = true;
			if (soundStart != null)
			{
				holdingEntity.PlayOneShot(soundStart);
			}
			((MyInventoryData)_actionData).bEatingStarted = true;
			((MyInventoryData)_actionData).bEatingFinished = false;
		}
		else
		{
			ExecuteInstantAction(holdingEntity, _actionData.invData.itemStack, isHeldItem: true, null);
		}
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionStart, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionStart, useInventory: false);
		if (soundStart != null)
		{
			ent.PlayOneShot(soundStart, Sound_in_head);
		}
		EntityPlayer entityPlayer = ent as EntityPlayer;
		if (Consume)
		{
			if (stack.itemValue.MaxUseTimes > 0 && stack.itemValue.UseTimes + 1f < (float)stack.itemValue.MaxUseTimes)
			{
				stack.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, stack.itemValue, 1f, ent, null, stack.itemValue.ItemClass.ItemTags);
				return true;
			}
			if (isHeldItem)
			{
				ent.inventory.DecHoldingItem(1);
			}
			else
			{
				stack.count--;
			}
			if (stackController != null)
			{
				stackController.ItemStack.count--;
				if (stackController.ItemStack.count == 0)
				{
					stackController.ItemStack = ItemStack.Empty.Clone();
				}
				stackController.ForceRefreshItemStack();
			}
		}
		if (stackController != null)
		{
			ent.MinEventContext.ItemValue = stack.itemValue;
			ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, ent.MinEventContext);
			ent.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, useInventory: false);
		}
		QuestEventManager.Current.UsedItem(stack.itemValue);
		if (CreateItem != null && CreateItemCount > 0 && (!UseJarRefund || (float)GameStats.GetInt(EnumGameStats.JarRefund) * 0.01f > entityPlayer.rand.RandomRange(1f)))
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(CreateItem), CreateItemCount);
			if (!LocalPlayerUI.GetUIForPlayer(entityPlayer as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				ent.world.gameManager.ItemDropServer(itemStack, ent.GetPosition(), Vector3.zero);
			}
		}
		return true;
	}

	public float PercentDone(ItemActionData _actionData)
	{
		float result = 0f;
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.bEatingStarted)
		{
			result = (Time.time - myInventoryData.lastUseTime) / AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
		}
		return result;
	}

	public bool EatingDone(ItemActionData _actionData)
	{
		bool result = false;
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.bEatingStarted)
		{
			result = myInventoryData.bEatingFinished || 1f <= (Time.time - myInventoryData.lastUseTime) / AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
		}
		return result;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.bEatingStarted && (!myInventoryData.bEatingFinished || Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast))
		{
			return true;
		}
		if (!UseAnimation && Time.time - myInventoryData.lastUseTime < Delay)
		{
			return true;
		}
		return false;
	}

	public override bool IsEndDelayed()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void consume(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = _actionData as MyInventoryData;
		if (myInventoryData != null)
		{
			myInventoryData.bEatingStarted = false;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		MinEventTypes eventType = MinEvent.End[_actionData.indexInEntityOfAction];
		holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemStack.itemValue;
		QuestEventManager.Current.UsedItem(holdingEntity.MinEventContext.ItemValue);
		holdingEntity.FireEvent(eventType);
		if (Consume)
		{
			if (_actionData.invData.itemValue.MaxUseTimes > 0 && _actionData.invData.itemValue.UseTimes + 1f < (float)_actionData.invData.itemValue.MaxUseTimes)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, myInventoryData.invData.itemValue, 1f, holdingEntity, null, myInventoryData.invData.itemValue.ItemClass.ItemTags);
				return;
			}
			holdingEntity.inventory.DecHoldingItem(1);
		}
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if ((bool)entityPlayerLocal && smellUse > 0f)
		{
			entityPlayerLocal.Stealth.SetSmellEat(smellUse);
		}
		if (CreateItem != null && CreateItemCount > 0 && (!UseJarRefund || (float)GameStats.GetInt(EnumGameStats.JarRefund) * 0.01f > holdingEntity.rand.RandomRange(1f)))
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(CreateItem), CreateItemCount);
			if (!LocalPlayerUI.GetUIForPlayer(holdingEntity as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				holdingEntity.world.gameManager.ItemDropServer(itemStack, holdingEntity.GetPosition(), Vector3.zero);
			}
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		MyInventoryData actionData = (MyInventoryData)_actionData;
		if (EatingDone(_actionData))
		{
			consume(actionData);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((MyInventoryData)_data).bEatingStarted = false;
	}

	public void Completed(ItemActionData _actionData)
	{
		consume(_actionData);
	}
}
