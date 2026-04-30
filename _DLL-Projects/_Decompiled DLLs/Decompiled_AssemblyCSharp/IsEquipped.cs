using UnityEngine.Scripting;

[Preserve]
public class IsEquipped : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (base.IsValid(_params))
		{
			ItemValue itemValue = _params.ItemValue;
			if (itemValue != null && target != null)
			{
				if (itemValue.IsMod)
				{
					ItemValue[] items = target.equipment.GetItems();
					foreach (ItemValue itemValue2 in items)
					{
						if (itemValue2 == null || !itemValue2.HasModSlots)
						{
							continue;
						}
						ItemValue[] modifications = itemValue2.Modifications;
						foreach (ItemValue itemValue3 in modifications)
						{
							if (itemValue3 != null && itemValue3 == _params.ItemValue)
							{
								return true;
							}
						}
					}
				}
				else
				{
					ItemValue[] items = target.equipment.GetItems();
					for (int i = 0; i < items.Length; i++)
					{
						if (items[i] == itemValue)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
