using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveItems : ActionBaseItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		if (!stack.IsEmpty() && (itemTags == "" || stack.itemValue.ItemClass.HasAnyTags(fastItemTags)))
		{
			if (count != -1)
			{
				if (countType == CountTypes.Items)
				{
					if (stack.count >= count)
					{
						stack.count -= count;
						count = 0;
						isFinished = true;
						if (stack.count == 0)
						{
							stack = ItemStack.Empty.Clone();
						}
					}
					else
					{
						count -= stack.count;
						stack = ItemStack.Empty.Clone();
					}
				}
				else
				{
					stack = ItemStack.Empty.Clone();
					count--;
					if (count == 0)
					{
						isFinished = true;
					}
				}
				return true;
			}
			stack = ItemStack.Empty.Clone();
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
	{
		if (!itemValue.IsEmpty() && (itemTags == "" || itemValue.ItemClass.HasAnyTags(fastItemTags)))
		{
			itemValue = ItemValue.None.Clone();
			if (count != -1)
			{
				count--;
				if (count == 0)
				{
					isFinished = true;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveItems();
	}
}
