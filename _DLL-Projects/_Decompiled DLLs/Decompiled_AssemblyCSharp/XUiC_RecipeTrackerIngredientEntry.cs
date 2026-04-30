using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerIngredientEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack ingredient;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	public bool IsComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> countFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i1, int _i2) => _i1 + "/" + _i2);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeTrackerIngredientsList Owner { get; set; }

	public ItemStack Ingredient
	{
		get
		{
			return ingredient;
		}
		set
		{
			ingredient = value;
			if (ingredient != null && Owner.Recipe.materialBasedRecipe)
			{
				ingredient = null;
			}
			if (ingredient != null)
			{
				currentCount = base.xui.PlayerInventory.GetItemCount(ingredient.itemValue);
				IsComplete = currentCount >= ingredient.count * Owner.Count;
			}
			else
			{
				currentCount = 0;
				IsComplete = false;
			}
			isDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = ingredient != null;
		switch (bindingName)
		{
		case "hasingredient":
			value = flag.ToString();
			return true;
		case "itemname":
			value = (flag ? ingredient.itemValue.ItemClass.GetLocalizedItemName() : "");
			return true;
		case "itemicon":
			value = (flag ? ingredient.itemValue.ItemClass.GetIconName() : "");
			return true;
		case "itemicontint":
		{
			Color32 v2 = Color.white;
			if (flag)
			{
				ItemClass itemClass = ingredient.itemValue.ItemClass;
				if (itemClass != null)
				{
					v2 = itemClass.GetIconTint(ingredient.itemValue);
				}
			}
			value = itemicontintcolorFormatter.Format(v2);
			return true;
		}
		case "itemcount":
			if (flag)
			{
				int v = ingredient.count * Owner.Count;
				value = countFormatter.Format(currentCount, v);
			}
			return true;
		case "ingredientcompletehexcolor":
			if (flag)
			{
				value = ((currentCount >= ingredient.count * Owner.Count) ? Owner.completeHexColor : Owner.incompleteHexColor);
			}
			else
			{
				value = "FFFFFF";
			}
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		RefreshBindings(_forceAll: true);
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			RefreshBindings();
			base.ViewComponent.IsVisible = true;
			isDirty = false;
		}
		base.Update(_dt);
	}
}
