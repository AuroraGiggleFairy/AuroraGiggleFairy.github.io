using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_IngredientEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack ingredient;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool materialBased;

	[PublicizedFrom(EAccessModifier.Private)]
	public string material = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblHaveCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblNeedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt havecountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt needcountFormatter = new CachedStringFormatterInt();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Recipe Recipe { get; set; }

	public ItemStack Ingredient
	{
		get
		{
			return ingredient;
		}
		set
		{
			ingredient = value;
			if (ingredient != null)
			{
				materialBased = ((XUiC_IngredientList)parent).Recipe.materialBasedRecipe;
				if (ingredient.itemValue.ItemClass != null)
				{
					material = ingredient.itemValue.ItemClass.MadeOfMaterial.ForgeCategory;
					if (material == null)
					{
						material = "";
					}
				}
			}
			isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("icon");
		if (childById != null)
		{
			icoItem = childById.ViewComponent as XUiV_Sprite;
		}
		XUiController childById2 = GetChildById("name");
		if (childById2 != null)
		{
			lblName = childById2.ViewComponent as XUiV_Label;
		}
		childById2 = GetChildById("havecount");
		if (childById2 != null)
		{
			lblHaveCount = childById2.ViewComponent as XUiV_Label;
		}
		childById2 = GetChildById("needcount");
		if (childById2 != null)
		{
			lblNeedCount = childById2.ViewComponent as XUiV_Label;
		}
		craftCountControl = windowGroup.Controller.GetChildByType<XUiC_RecipeCraftCount>();
		craftCountControl.OnCountChanged += HandleOnCountChanged;
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = ingredient != null;
		switch (bindingName)
		{
		case "itemname":
		{
			string text2 = "";
			if (flag && materialBased)
			{
				text2 = ((!Localization.Exists("lbl" + material)) ? XUi.UppercaseFirst(material) : Localization.Get("lbl" + material));
			}
			value = ((!flag) ? "" : (materialBased ? text2 : ingredient.itemValue.ItemClass.GetLocalizedItemName()));
			return true;
		}
		case "itemicon":
			value = (flag ? ingredient.itemValue.ItemClass.GetIconName() : "");
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (flag)
			{
				ItemClass itemClass = ingredient.itemValue.ItemClass;
				if (itemClass != null)
				{
					v = itemClass.GetIconTint(ingredient.itemValue);
				}
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "havecount":
		{
			XUiC_WorkstationMaterialInputGrid childByType3 = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
			if (childByType3 != null)
			{
				if (materialBased)
				{
					value = (flag ? havecountFormatter.Format(childByType3.GetWeight(material)) : "");
				}
				else
				{
					value = (flag ? havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(ingredient.itemValue)) : "");
				}
			}
			else
			{
				XUiC_WorkstationInputGrid childByType4 = windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
				if (childByType4 != null)
				{
					value = (flag ? havecountFormatter.Format(childByType4.GetItemCount(ingredient.itemValue)) : "");
				}
				else
				{
					value = (flag ? havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(ingredient.itemValue)) : "");
				}
			}
			return true;
		}
		case "needcount":
			value = (flag ? needcountFormatter.Format(ingredient.count * craftCountControl.Count) : "");
			return true;
		case "haveneedcount":
		{
			string text = (flag ? needcountFormatter.Format(ingredient.count * craftCountControl.Count) : "");
			XUiC_WorkstationMaterialInputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
			if (childByType != null)
			{
				if (materialBased)
				{
					value = (flag ? (havecountFormatter.Format(childByType.GetWeight(material)) + "/" + text) : "");
				}
				else
				{
					value = (flag ? (havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(ingredient.itemValue)) + "/" + text) : "");
				}
			}
			else
			{
				XUiC_WorkstationInputGrid childByType2 = windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
				if (childByType2 != null)
				{
					value = (flag ? (havecountFormatter.Format(childByType2.GetItemCount(ingredient.itemValue)) + "/" + text) : "");
				}
				else
				{
					value = (flag ? (havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(ingredient.itemValue)) + "/" + text) : "");
				}
			}
			return true;
		}
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		isDirty = true;
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
