using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionGainSkill : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public bool bReadingStarted;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public string[] SkillsToGain;

	public new string Title;

	public new string Description;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Skills_to_gain"))
		{
			SkillsToGain = new string[0];
		}
		else
		{
			SkillsToGain = _props.Values["Skills_to_gain"].Split(',');
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
		((MyInventoryData)_data).bReadingStarted = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (_bReleased && !(Time.time - _actionData.lastUseTime < Delay))
		{
			_actionData.lastUseTime = Time.time;
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			((MyInventoryData)_actionData).bReadingStarted = true;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.bReadingStarted && Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!myInventoryData.bReadingStarted || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.bReadingStarted = false;
		bool flag = false;
		for (int i = 0; i < SkillsToGain.Length; i++)
		{
			if (!(myInventoryData.invData.holdingEntity is EntityPlayer))
			{
				continue;
			}
			EntityPlayerLocal entityPlayerLocal = myInventoryData.invData.holdingEntity as EntityPlayerLocal;
			ProgressionValue progressionValue = myInventoryData.invData.holdingEntity.Progression.GetProgressionValue(SkillsToGain[i]);
			if (progressionValue != null)
			{
				if (progressionValue.Level + 1 <= progressionValue.ProgressionClass.MaxLevel)
				{
					progressionValue.Level++;
					entityPlayerLocal.MinEventContext.ProgressionValue = progressionValue;
					entityPlayerLocal.FireEvent(MinEventTypes.onPerkLevelChanged);
					string arg = Localization.Get(progressionValue.ProgressionClass.NameKey);
					GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttSkillLevelUp"), arg, progressionValue.Level));
					(myInventoryData.invData.holdingEntity as EntityPlayer).bPlayerStatsChanged = true;
					flag = true;
				}
				else
				{
					GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttSkillMaxLevel"));
				}
			}
		}
		if (flag)
		{
			_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
			if (soundStart != null)
			{
				Manager.PlayInsidePlayerHead(soundStart);
			}
		}
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		for (int i = 0; i < SkillsToGain.Length; i++)
		{
			_infoList.Add(ItemAction.StringFormatHandler(Localization.Get(_xui.playerUI.entityPlayer.Progression.GetProgressionValue(SkillsToGain[i]).ProgressionClass.NameKey), "+1"));
		}
	}
}
