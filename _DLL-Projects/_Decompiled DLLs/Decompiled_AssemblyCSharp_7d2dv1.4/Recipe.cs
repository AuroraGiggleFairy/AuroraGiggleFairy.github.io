using System.Collections.Generic;

public class Recipe
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte Version = 3;

	public static FastTags<TagGroup.Global> MaterialBased = FastTags<TagGroup.Global>.Parse("materialbased");

	public static FastTags<TagGroup.Global> LearnableRecipe = FastTags<TagGroup.Global>.Parse("learnable");

	public int itemValueType;

	public int count;

	public bool scrapable;

	public List<ItemStack> ingredients = new List<ItemStack>();

	public bool wildcardForgeCategory;

	public bool wildcardCampfireCategory;

	public bool materialBasedRecipe;

	public int craftingToolType;

	public float craftingTime;

	public string craftingArea;

	public string tooltip;

	public int unlockExpGain;

	public int craftExpGain;

	public bool UseIngredientModifier = true;

	public FastTags<TagGroup.Global> tags;

	public bool IsTrackable = true;

	public bool isQuest;

	public bool isChallenge;

	public bool IsTracked;

	public bool IsScrap;

	public int craftingTier = -1;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hashcode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hashCodeSetup;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLearnable
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public List<ItemStack> GetIngredientsSummedUp()
	{
		return ingredients;
	}

	public void Init()
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemStack itemStack = ingredients[i];
			if (itemStack.itemValue.ItemClass != null)
			{
				num += itemStack.itemValue.ItemClass.CraftComponentExp * (float)itemStack.count;
				num2 += itemStack.itemValue.ItemClass.CraftComponentTime * (float)itemStack.count;
			}
		}
		if (unlockExpGain < 0)
		{
			unlockExpGain = (int)(num * 2f);
		}
		if (craftExpGain < 0)
		{
			craftExpGain = (int)num;
		}
		if (craftingTime < 0f)
		{
			craftingTime = num2;
		}
		IsLearnable = tags.Test_AnySet(LearnableRecipe);
	}

	public void AddIngredient(ItemValue _itemValue, int _count)
	{
		for (int i = 0; i < ingredients.Count - 1; i++)
		{
			if (ingredients[i].itemValue.type == _itemValue.type)
			{
				ingredients[i].count += _count;
				return;
			}
		}
		ingredients.Add(new ItemStack(_itemValue, _count));
	}

	public void AddIngredients(List<ItemStack> _items)
	{
		ingredients.AddRange(_items);
	}

	public string GetName()
	{
		ItemClass forId = ItemClass.GetForId(itemValueType);
		if (forId == null)
		{
			return string.Empty;
		}
		return forId.GetItemName();
	}

	public string GetIcon()
	{
		ItemClass forId = ItemClass.GetForId(itemValueType);
		if (forId == null)
		{
			return string.Empty;
		}
		return forId.GetIconName();
	}

	public bool CanCraft(IList<ItemStack> _itemStack, EntityAlive _ea = null, int _craftingTier = -1)
	{
		craftingTier = (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, _ea, this, tags);
		if (_craftingTier != -1 && _craftingTier < craftingTier)
		{
			craftingTier = _craftingTier;
		}
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemStack itemStack = ingredients[i];
			int num = itemStack.count;
			if (UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, _ea, this, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
			}
			if (num == 0)
			{
				continue;
			}
			int num2 = 0;
			while (num > 0 && num2 < _itemStack.Count)
			{
				if ((!_itemStack[num2].itemValue.HasModSlots || !_itemStack[num2].itemValue.HasMods()) && _itemStack[num2].itemValue.type == itemStack.itemValue.type)
				{
					num -= _itemStack[num2].count;
				}
				num2++;
			}
			if (num > 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanCraftAny(IList<ItemStack> _itemStack, EntityAlive _ea = null)
	{
		for (int num = (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, _ea, this, tags); num >= 0; num--)
		{
			bool flag = true;
			for (int i = 0; i < ingredients.Count; i++)
			{
				ItemStack itemStack = ingredients[i];
				int num2 = itemStack.count;
				if (UseIngredientModifier)
				{
					num2 = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, _ea, this, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, num);
				}
				if (num2 == 0)
				{
					continue;
				}
				int num3 = 0;
				while (num2 > 0 && num3 < _itemStack.Count)
				{
					if ((!_itemStack[num3].itemValue.HasModSlots || !_itemStack[num3].itemValue.HasMods()) && _itemStack[num3].itemValue.type == itemStack.itemValue.type)
					{
						num2 -= _itemStack[num3].count;
					}
					num3++;
				}
				if (num2 > 0)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUnlocked(EntityPlayer _ep)
	{
		if (IsLearnable)
		{
			return EffectManager.GetValue(PassiveEffects.RecipeTagUnlocked, null, _ep.GetCVar(GetName()), _ep, null, tags) > 0f;
		}
		return true;
	}

	public int GetCraftingTier(EntityPlayer _ep)
	{
		return (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, _ep, this, tags);
	}

	public ItemClass GetOutputItemClass()
	{
		return ItemClass.GetForId(itemValueType);
	}

	public bool ContainsIngredients(ItemValue[] _items)
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			for (int j = 0; j < _items.Length; j++)
			{
				if (ingredients[i].itemValue.type == _items[j].type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBeCraftedWith(Dictionary<ItemValue, int> _items)
	{
		return false;
	}

	public override bool Equals(object _other)
	{
		return _other.GetHashCode() == GetHashCode();
	}

	public override int GetHashCode()
	{
		if (!hashCodeSetup)
		{
			int num = 0;
			for (int i = 0; i < ingredients.Count; i++)
			{
				num += ingredients[i].count;
			}
			hashcode = (itemValueType + "_" + craftingArea + "_" + num).GetHashCode();
			hashCodeSetup = true;
		}
		return hashcode;
	}

	public override string ToString()
	{
		return string.Format("[Recipe: " + GetName() + "]");
	}

	public void ModifyValue(PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags, int _craftingTier = 1)
	{
		if (Effects != null)
		{
			Effects.ModifyValue(null, _passiveEffect, ref _base_val, ref _perc_val, _craftingTier, tags);
		}
	}
}
