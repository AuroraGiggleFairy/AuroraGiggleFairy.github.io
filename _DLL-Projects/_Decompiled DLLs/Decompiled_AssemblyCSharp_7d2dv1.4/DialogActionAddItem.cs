using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddItem : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override ActionTypes ActionType => ActionTypes.AddItem;

	public override void PerformAction(EntityPlayer player)
	{
		ItemValue item = ItemClass.GetItem(base.ID);
		ItemValue itemValue = new ItemValue(ItemClass.GetItem(base.ID).type, _bCreateDefaultParts: true);
		int result = 1;
		if (base.Value != null && base.Value != "")
		{
			if (int.TryParse(base.Value, out result))
			{
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, result, result, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
				}
			}
			else if (base.Value.Contains(","))
			{
				string[] array = base.Value.Split(',');
				int num = Convert.ToInt32(array[0]);
				int num2 = Convert.ToInt32(array[1]);
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, num, num2, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
					result = UnityEngine.Random.Range(num, num2);
				}
			}
		}
		LocalPlayerUI.primaryUI.xui.PlayerInventory.AddItem(new ItemStack(itemValue, result));
	}
}
