using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_IngredientList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> ingredientEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int craftingTier = -1;

	public Recipe Recipe
	{
		get
		{
			return recipe;
		}
		set
		{
			recipe = value;
			IsDirty = true;
		}
	}

	public int CraftingTier
	{
		get
		{
			return craftingTier;
		}
		set
		{
			craftingTier = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_IngredientEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				ingredientEntries.Add(array[i]);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
		xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			if (recipe != null)
			{
				int count = ingredientEntries.Count;
				int count2 = recipe.ingredients.Count;
				for (int i = 0; i < count; i++)
				{
					if (!(ingredientEntries[i] is XUiC_IngredientEntry))
					{
						continue;
					}
					ItemStack itemStack = ((i < count2) ? recipe.ingredients[i].Clone() : null);
					if (itemStack != null && recipe.UseIngredientModifier)
					{
						itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
						if (itemStack.count > 0)
						{
							itemStack.count = (int)((float)itemStack.count * XUiM_Recipes.GetCraftingInputModifier(recipe));
						}
					}
					if (itemStack == null || (itemStack != null && itemStack.count > 0))
					{
						((XUiC_IngredientEntry)ingredientEntries[i]).Ingredient = itemStack;
					}
					else
					{
						((XUiC_IngredientEntry)ingredientEntries[i]).Ingredient = null;
					}
				}
			}
			else
			{
				int count3 = ingredientEntries.Count;
				for (int j = 0; j < count3; j++)
				{
					if (ingredientEntries[j] is XUiC_IngredientEntry)
					{
						((XUiC_IngredientEntry)ingredientEntries[j]).Ingredient = null;
					}
				}
			}
			IsDirty = false;
		}
		base.Update(_dt);
	}
}
