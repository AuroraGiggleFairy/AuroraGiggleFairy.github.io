using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLearnRecipe : ItemAction
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

	public new string[] RecipesToLearn;

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
		if (!_props.Values.ContainsKey("Recipes_to_learn"))
		{
			RecipesToLearn = new string[0];
		}
		else
		{
			RecipesToLearn = _props.Values["Recipes_to_learn"].Replace(" ", "").Split(',');
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
		for (int i = 0; i < RecipesToLearn.Length; i++)
		{
			CraftingManager.LockRecipe(RecipesToLearn[i]);
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
		EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!myInventoryData.bReadingStarted || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.bReadingStarted = false;
		bool flag = false;
		for (int i = 0; i < RecipesToLearn.Length; i++)
		{
			if (CraftingManager.GetRecipe(RecipesToLearn[i]).tags.Equals(FastTags<TagGroup.Global>.Parse("learnable")) && myInventoryData.invData.holdingEntity.GetCVar(RecipesToLearn[i]) == 0f)
			{
				flag = true;
				myInventoryData.invData.holdingEntity.SetCVar(RecipesToLearn[i], 1f);
				GameManager.ShowTooltip(player, string.Format(Localization.Get("ttRecipeUnlocked"), Localization.Get(RecipesToLearn[i])));
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
		else
		{
			GameManager.ShowTooltip(player, Localization.Get("alreadyKnown"));
		}
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		for (int i = 0; i < RecipesToLearn.Length; i++)
		{
			if (!XUiM_Recipes.GetRecipeIsUnlocked(_xui, RecipesToLearn[i]))
			{
				_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeRecipe"), Localization.Get(RecipesToLearn[i])));
			}
			else
			{
				_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeRecipe"), Localization.Get("lblKnown")));
			}
		}
	}
}
