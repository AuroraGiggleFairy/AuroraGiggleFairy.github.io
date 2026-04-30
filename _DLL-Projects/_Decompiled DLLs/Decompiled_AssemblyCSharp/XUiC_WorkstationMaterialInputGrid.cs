using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationMaterialInputGrid : XUiC_WorkstationInputGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationMaterialInputWindow materialWindow;

	public override void Init()
	{
		base.Init();
		materialWindow = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		for (int i = 0; i < workstationData.TileEntity.Input.Length && i < itemControllers.Length; i++)
		{
			float timerForSlot = workstationData.TileEntity.GetTimerForSlot(i);
			if (timerForSlot > 0f)
			{
				itemControllers[i].timer.IsVisible = true;
				itemControllers[i].timer.Text = string.Format("{0}:{1}", Mathf.Floor((timerForSlot + 0.95f) / 60f).ToCultureInvariantString("00"), Mathf.Floor((timerForSlot + 0.95f) % 60f).ToCultureInvariantString("00"));
			}
			else
			{
				itemControllers[i].timer.IsVisible = false;
			}
		}
		workstationData.GetIsBurning();
	}

	public override ItemStack[] GetSlots()
	{
		return getUISlots();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ItemStack[] getUISlots()
	{
		return items;
	}

	public override bool HasRequirement(Recipe recipe)
	{
		if (materialWindow == null)
		{
			return false;
		}
		return materialWindow.HasRequirement(recipe);
	}

	public override void SetSlots(ItemStack[] stackList)
	{
		items = stackList;
		base.SetSlots(items);
		materialWindow.SetMaterialWeights(items);
	}

	public void SetWeight(ItemValue iv, int count)
	{
		ItemClass itemClass = iv.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		string forgeCategory = itemClass.MadeOfMaterial.ForgeCategory;
		if (forgeCategory == null)
		{
			return;
		}
		for (int i = 3; i < items.Length; i++)
		{
			ItemClass itemClass2 = items[i].itemValue.ItemClass;
			if (itemClass2 == null)
			{
				if (materialWindow.MaterialNames[i - 3].EqualsCaseInsensitive(forgeCategory))
				{
					ItemStack itemStack = new ItemStack(iv, count);
					items[i] = itemStack;
					break;
				}
			}
			else if (itemClass2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forgeCategory))
			{
				ItemStack itemStack2 = items[i].Clone();
				itemStack2.count += count;
				if (iv.ItemClass.Stacknumber.Value < itemStack2.count)
				{
					itemStack2.count = iv.ItemClass.Stacknumber.Value;
				}
				items[i] = itemStack2;
				break;
			}
		}
		materialWindow.SetMaterialWeights(items);
		UpdateBackend(items);
	}

	public int GetWeight(string materialName)
	{
		int result = 0;
		if (materialName == null)
		{
			return result;
		}
		for (int i = 3; i < items.Length; i++)
		{
			ItemClass itemClass = items[i].itemValue.ItemClass;
			if (itemClass != null && itemClass.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(materialName))
			{
				result = items[i].count;
				break;
			}
		}
		return result;
	}
}
