using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionQuest : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public bool bQuestAccept;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public string QuestGiven;

	public new string Title;

	public new string Description;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("QuestGiven"))
		{
			QuestGiven = "";
		}
		else
		{
			QuestGiven = _props.Values["QuestGiven"];
		}
		if (!_props.Values.ContainsKey("Title"))
		{
			Title = "The title is impossible to read.";
		}
		else
		{
			Title = _props.Values["Title"];
		}
		if (!_props.Values.ContainsKey("Description"))
		{
			Description = "The description is impossible to read.";
		}
		else
		{
			Description = _props.Values["Description"];
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((MyInventoryData)_data).bQuestAccept = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased || Time.time - _actionData.lastUseTime < Delay)
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
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			if (soundStart != null)
			{
				_actionData.invData.holdingEntity.PlayOneShot(soundStart);
			}
			((MyInventoryData)_actionData).bQuestAccept = true;
		}
		else
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_actionData.invData.holdingEntity as EntityPlayerLocal);
			if (_actionData.invData.slotIdx < uIForPlayer.entityPlayer.inventory.PUBLIC_SLOTS)
			{
				XUiC_Toolbelt childByType = uIForPlayer.xui.FindWindowGroupByName("toolbelt").GetChildByType<XUiC_Toolbelt>();
				ExecuteInstantAction(_actionData.invData.holdingEntity, _actionData.invData.itemStack, isHeldItem: true, childByType.GetSlotControl(_actionData.invData.slotIdx));
			}
		}
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		if (soundStart != null)
		{
			ent.PlayOneShot(soundStart);
		}
		EntityPlayerLocal entityPlayerLocal = ent as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (QuestGiven != "")
		{
			QuestClass quest = QuestClass.GetQuest(QuestGiven);
			if (quest != null)
			{
				Quest quest2 = entityPlayerLocal.QuestJournal.FindQuest(QuestGiven);
				if (quest2 == null || (quest.Repeatable && !quest2.Active))
				{
					if (!quest.CanActivate())
					{
						GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("questunavailable"));
						return false;
					}
					Quest q = quest.CreateQuest();
					XUiC_QuestOfferWindow xUiC_QuestOfferWindow = XUiC_QuestOfferWindow.OpenQuestOfferWindow(uIForPlayer.xui, q);
					xUiC_QuestOfferWindow.ItemStackController = stackController;
					xUiC_QuestOfferWindow.ItemStackController.QuestLock = true;
				}
				else
				{
					GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("questunavailable"));
				}
			}
		}
		return true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.bQuestAccept && Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!myInventoryData.bQuestAccept || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.bQuestAccept = false;
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!(QuestGiven != ""))
		{
			return;
		}
		QuestClass quest = QuestClass.GetQuest(QuestGiven);
		if (quest == null)
		{
			return;
		}
		Quest quest2 = entityPlayerLocal.QuestJournal.FindQuest(QuestGiven);
		if (quest2 == null || (quest.Repeatable && !quest2.Active))
		{
			Quest q = quest.CreateQuest();
			XUiC_QuestOfferWindow xUiC_QuestOfferWindow = XUiC_QuestOfferWindow.OpenQuestOfferWindow(uIForPlayer.xui, q);
			if (myInventoryData.invData.slotIdx < uIForPlayer.entityPlayer.inventory.PUBLIC_SLOTS)
			{
				XUiC_Toolbelt childByType = uIForPlayer.xui.FindWindowGroupByName("toolbelt").GetChildByType<XUiC_Toolbelt>();
				xUiC_QuestOfferWindow.ItemStackController = childByType.GetSlotControl(myInventoryData.invData.slotIdx);
				xUiC_QuestOfferWindow.ItemStackController.QuestLock = true;
			}
		}
		else
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("questunavailable"));
		}
	}
}
