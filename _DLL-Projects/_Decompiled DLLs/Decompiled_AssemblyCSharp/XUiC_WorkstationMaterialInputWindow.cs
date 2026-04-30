using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationMaterialInputWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float OUNCES_IN_POUND = 16f;

	public string[] MaterialNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] weights;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] materialTitles;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] materialWeights;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationMaterialInputGrid inputGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color baseTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color validColor = Color.green;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color invalidColor = Color.red;

	public event XuiEvent_WorkstationItemsChanged OnWorkstationMaterialWeightsChanged;

	public override void Init()
	{
		base.Init();
		materialTitles = GetChildrenById("material");
		materialWeights = GetChildrenById("weight");
		inputGrid = GetChildByType<XUiC_WorkstationMaterialInputGrid>();
		if (inputGrid == null)
		{
			Log.Error("Input Grid not found!");
		}
		if (materialWeights[0] != null)
		{
			baseTextColor = ((XUiV_Label)materialWeights[0].ViewComponent).Color;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		MaterialNames = inputGrid.WorkstationData.GetMaterialNames();
		for (int i = 0; i < MaterialNames.Length; i++)
		{
			string text = XUi.UppercaseFirst(MaterialNames[i]);
			if (Localization.Exists("lbl" + MaterialNames[i]))
			{
				text = Localization.Get("lbl" + MaterialNames[i]);
			}
			((XUiV_Label)materialTitles[i].ViewComponent).Text = text + ":";
		}
		XUiC_RecipeList childByType = windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (childByType != null)
		{
			childByType.RecipeChanged += RecipeList_RecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onForgeValuesChanged()
	{
		if (this.OnWorkstationMaterialWeightsChanged != null)
		{
			this.OnWorkstationMaterialWeightsChanged();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_RecipeList childByType = windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (childByType != null)
		{
			childByType.RecipeChanged -= RecipeList_RecipeChanged;
		}
	}

	public override void Update(float _dt)
	{
		if (windowGroup.isShowing)
		{
			if (weights == null)
			{
				weights = new int[MaterialNames.Length];
				SetMaterialWeights(inputGrid.WorkstationData.GetInputStacks());
			}
			for (int i = 0; i < weights.Length; i++)
			{
				((XUiV_Label)materialWeights[i].ViewComponent).Text = $"{weights[i].ToString()}";
			}
			base.Update(_dt);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecipeList_RecipeChanged(Recipe _recipe, XUiC_RecipeEntry recipeEntry)
	{
		ResetWeightColors();
	}

	public bool HasRequirement(Recipe recipe)
	{
		if (weights == null)
		{
			return true;
		}
		if (recipe == null)
		{
			ResetWeightColors();
			return true;
		}
		for (int i = 0; i < weights.Length; i++)
		{
			((XUiV_Label)materialWeights[i].ViewComponent).Color = baseTextColor;
			for (int j = 0; j < recipe.ingredients.Count; j++)
			{
				int num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, recipe.ingredients[j].count, base.xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.ingredients[j].itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, recipe.GetCraftingTier(base.xui.playerUI.entityPlayer));
				ItemClass forId = ItemClass.GetForId(recipe.ingredients[j].itemValue.type);
				if (forId == null)
				{
					continue;
				}
				if (forId.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(MaterialNames[i]))
				{
					if (num <= weights[i])
					{
						((XUiV_Label)materialWeights[i].ViewComponent).Color = Color.green;
					}
					else
					{
						((XUiV_Label)materialWeights[i].ViewComponent).Color = Color.red;
					}
					break;
				}
				((XUiV_Label)materialWeights[i].ViewComponent).Color = baseTextColor;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetWeightColors()
	{
		for (int i = 0; i < weights.Length; i++)
		{
			((XUiV_Label)materialWeights[i].ViewComponent).Color = baseTextColor;
		}
	}

	public void SetMaterialWeights(ItemStack[] stackList)
	{
		for (int i = 3; i < stackList.Length; i++)
		{
			if (weights != null && stackList[i] != null)
			{
				weights[i - 3] = stackList[i].count;
			}
		}
		onForgeValuesChanged();
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "materials":
				if (value.Contains(","))
				{
					MaterialNames = value.Replace(" ", "").Split(',');
					weights = new int[MaterialNames.Length];
				}
				else
				{
					MaterialNames = new string[1] { value };
				}
				return true;
			case "valid_materials_color":
				validColor = StringParsers.ParseColor32(value);
				return true;
			case "invalid_materials_color":
				invalidColor = StringParsers.ParseColor32(value);
				return true;
			default:
				return false;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float calculateWeightOunces(int materialIndex)
	{
		return weights[materialIndex];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float calculateWeightPounds(int materialIndex)
	{
		return calculateWeightOunces(materialIndex) / 16f;
	}
}
