using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeCraftCount : XUiC_Counter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxAllowed = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	public override void Init()
	{
		base.Init();
		XUiC_RecipeList childByType = windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (childByType != null)
		{
			childByType.CraftCount = this;
			childByType.RecipeChanged += HandleRecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeChanged(Recipe _recipe, XUiC_RecipeEntry recipeEntry)
	{
		if (recipe != _recipe)
		{
			recipe = _recipe;
			base.Count = 1;
			CalculateMaxCount();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleMaxCountOnPress(XUiController _sender, int _mouseButton)
	{
		base.Count = calcMaxCraftable();
		HandleCountChangedEvent();
	}

	public void CalculateMaxCount()
	{
		MaxCount = calcMaxCraftable();
		if (base.Count > MaxCount)
		{
			base.Count = MaxCount;
		}
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int calcMaxCraftable()
	{
		if (recipe == null)
		{
			return 1;
		}
		XUiC_WorkstationInputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		ItemStack[] array = ((childByType == null) ? base.xui.PlayerInventory.GetAllItemStacks().ToArray() : childByType.GetSlots());
		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = recipe.ingredients[i];
			if (itemStack != null && itemStack.itemValue.HasQuality)
			{
				return 1;
			}
		}
		int num = int.MaxValue;
		int craftingTier = ((recipe.craftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, recipe, recipe.tags)) : recipe.craftingTier);
		for (int j = 0; j < recipe.ingredients.Count; j++)
		{
			ItemStack itemStack2 = recipe.ingredients[j];
			if (itemStack2 == null || itemStack2.itemValue.type == 0)
			{
				continue;
			}
			int num2 = itemStack2.count;
			float num3 = ((!recipe.UseIngredientModifier) ? ((float)num2) : ((float)(int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, num2, base.xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(itemStack2.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier)));
			if (num3 < 1f)
			{
				continue;
			}
			int num4 = 0;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] != null && array[k].itemValue.type != 0 && itemStack2.itemValue.type == array[k].itemValue.type)
				{
					num4 += array[k].count;
				}
			}
			int num5 = Mathf.CeilToInt((float)num4 / num3);
			if (Mathf.FloorToInt(num3 * (float)num5) > num4)
			{
				num5--;
			}
			num = Mathf.Min(num5, num);
			if (num == 0)
			{
				break;
			}
		}
		return Mathf.Clamp(num, 1, 10000);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			CalculateMaxCount();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
			CalculateMaxCount();
		}
		XUiV_Label obj = (XUiV_Label)counter.ViewComponent;
		string text = (textInput.Text = base.Count.ToString());
		obj.Text = text;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		CalculateMaxCount();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		CalculateMaxCount();
	}

	public void RefreshCounts()
	{
		CalculateMaxCount();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (!(bindingName == "enablecountdown"))
		{
			if (bindingName == "enablecountup")
			{
				value = (base.Count < MaxCount).ToString();
				return true;
			}
			return false;
		}
		value = (base.Count > 1).ToString();
		return true;
	}
}
