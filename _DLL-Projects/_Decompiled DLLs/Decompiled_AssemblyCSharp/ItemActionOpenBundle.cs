using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionOpenBundle : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public bool bEatingStarted;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public new string[] CreateItem;

	public string[] CreateItemCount;

	public new bool Consume;

	public HashSet<int> ConditionBlockTypes;

	public bool UniqueRandomOnly;

	public string[] RandomItem;

	public string[] RandomItemCount;

	public int RandomCount = 1;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
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
			CreateItem = _props.Values["Create_item"].Replace(" ", "").Split(',');
			if (_props.Values.ContainsKey("Create_item_count"))
			{
				CreateItemCount = _props.Values["Create_item_count"].Replace(" ", "").Split(',');
			}
			else
			{
				CreateItemCount = new string[0];
			}
		}
		else
		{
			CreateItem = null;
			CreateItemCount = null;
		}
		if (_props.Values.ContainsKey("Random_item"))
		{
			RandomItem = _props.Values["Random_item"].Replace(" ", "").Split(',');
			if (_props.Values.ContainsKey("Random_item_count"))
			{
				RandomItemCount = _props.Values["Random_item_count"].Replace(" ", "").Split(',');
			}
			else
			{
				RandomItemCount = new string[0];
			}
		}
		else
		{
			RandomItem = null;
			RandomItemCount = null;
		}
		if (_props.Values.ContainsKey("Random_count"))
		{
			RandomCount = StringParsers.ParseSInt32(_props.Values["Random_count"]);
		}
		_props.ParseBool("Unique_random_only", ref UniqueRandomOnly);
		if (_props.Values.ContainsKey("Condition_raycast_block"))
		{
			string[] array = _props.Values["Condition_raycast_block"].Trim().Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				int num = int.Parse(array[i].Trim());
				if (ConditionBlockTypes == null)
				{
					ConditionBlockTypes = new HashSet<int>();
				}
				ConditionBlockTypes.Add(num);
			}
			if (ConditionBlockTypes != null && ConditionBlockTypes.Count == 0)
			{
				ConditionBlockTypes = null;
			}
		}
		UseAnimation = false;
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((MyInventoryData)_data).bEatingStarted = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!_bReleased || Time.time - _actionData.lastUseTime < Delay || IsActionRunning(_actionData))
		{
			return;
		}
		EntityAlive holdingEntity = myInventoryData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return;
		}
		BlockValue air = BlockValue.Air;
		if (ConditionBlockTypes != null)
		{
			Ray lookRay = holdingEntity.GetLookRay();
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2);
			Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, 131, (holdingEntity is EntityPlayer) ? 0.2f : 0.4f);
			holdingEntity.SetModelLayer(modelLayer);
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!GameUtils.IsBlockOrTerrain(voxelRayHitInfo.tag))
			{
				return;
			}
			_ = voxelRayHitInfo.hit;
			air = voxelRayHitInfo.hit.blockValue;
			if (air.isair || !ConditionBlockTypes.Contains(air.type))
			{
				lookRay = myInventoryData.invData.holdingEntity.GetLookRay();
				lookRay.origin += lookRay.direction.normalized * 0.5f;
				if (!Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, -538480645, 4095, 0f))
				{
					return;
				}
				_ = voxelRayHitInfo.hit;
				air = voxelRayHitInfo.hit.blockValue;
				if (air.isair || !ConditionBlockTypes.Contains(air.type))
				{
					return;
				}
			}
		}
		_actionData.lastUseTime = Time.time;
		ExecuteInstantAction(myInventoryData.invData.holdingEntity, myInventoryData.invData.itemStack, isHeldItem: true, null);
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionStart, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionStart, useInventory: false);
		if (soundStart != null)
		{
			ent.PlayOneShot(soundStart);
		}
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
		}
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, useInventory: false);
		if (CreateItem != null && CreateItemCount != null)
		{
			for (int i = 0; i < CreateItem.Length; i++)
			{
				string text = ((CreateItemCount != null && CreateItemCount.Length > i) ? CreateItemCount[i] : "1");
				int num = 1;
				ItemClass itemClass = ItemClass.GetItemClass(CreateItem[i]);
				ItemValue itemValue = null;
				if (text.Contains("-"))
				{
					string[] array = text.Split('-');
					int min = StringParsers.ParseSInt32(array[0]);
					int maxExclusive = StringParsers.ParseSInt32(array[1]) + 1;
					num = ent.rand.RandomRange(min, maxExclusive);
				}
				else
				{
					num = StringParsers.ParseSInt32(text);
				}
				if (itemClass.HasQuality)
				{
					itemValue = new ItemValue(itemClass.Id, num, num);
					num = 1;
				}
				else
				{
					itemValue = new ItemValue(itemClass.Id);
				}
				ItemStack itemStack = new ItemStack(itemValue, num);
				if (!LocalPlayerUI.GetUIForPlayer(ent as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
				{
					ent.world.gameManager.ItemDropServer(itemStack, ent.GetPosition(), Vector3.zero);
				}
			}
		}
		if (RandomItem != null && RandomItemCount != null)
		{
			List<int> list = null;
			if (UniqueRandomOnly)
			{
				list = new List<int>();
				for (int j = 0; j < RandomItem.Length; j++)
				{
					list.Add(j);
				}
				for (int k = 0; k < list.Count * 3; k++)
				{
					int num2 = ent.rand.RandomRange(0, list.Count);
					int num3 = ent.rand.RandomRange(0, list.Count);
					if (num2 != num3)
					{
						int value = list[num2];
						list[num2] = list[num3];
						list[num3] = value;
					}
				}
			}
			int num4 = -1;
			int num5 = -1;
			for (int l = 0; l < RandomCount; l++)
			{
				if (UniqueRandomOnly)
				{
					num5++;
					if (num5 >= RandomItem.Length)
					{
						num5 = 0;
					}
					num4 = list[num5];
				}
				else
				{
					num4 = ent.rand.RandomRange(0, RandomItem.Length);
				}
				string text2 = ((RandomItemCount != null && RandomItemCount.Length > num4) ? RandomItemCount[num4] : "1");
				int num6 = 1;
				ItemClass itemClass2;
				while (true)
				{
					itemClass2 = ItemClass.GetItemClass(RandomItem[num4]);
					if (itemClass2 != null)
					{
						break;
					}
					num4++;
					if (num4 > RandomItem.Length)
					{
						num4 = 0;
					}
				}
				ItemValue itemValue2 = null;
				if (text2.Contains("-"))
				{
					string[] array2 = text2.Split('-');
					int min2 = StringParsers.ParseSInt32(array2[0]);
					int maxExclusive2 = StringParsers.ParseSInt32(array2[1]) + 1;
					num6 = ent.rand.RandomRange(min2, maxExclusive2);
				}
				else
				{
					num6 = StringParsers.ParseSInt32(text2);
				}
				if (itemClass2.HasQuality)
				{
					itemValue2 = new ItemValue(itemClass2.Id, num6, num6);
					num6 = 1;
				}
				else
				{
					itemValue2 = new ItemValue(itemClass2.Id);
				}
				XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(ent as EntityPlayerLocal).xui.PlayerInventory;
				while (num6 > 0)
				{
					ItemStack itemStack2 = new ItemStack(itemValue2, num6);
					num6 -= itemStack2.count;
					if (!playerInventory.AddItem(itemStack2))
					{
						ent.world.gameManager.ItemDropServer(itemStack2, ent.GetPosition(), Vector3.zero);
					}
				}
			}
		}
		return true;
	}
}
