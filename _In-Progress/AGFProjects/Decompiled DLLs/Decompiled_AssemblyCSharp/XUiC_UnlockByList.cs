using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UnlockByList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> unlockByEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	public Recipe Recipe
	{
		get
		{
			return recipe;
		}
		set
		{
			recipe = value;
			isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_UnlockByEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				unlockByEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			if (recipe != null)
			{
				ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
				RecipeUnlockData[] array = null;
				array = ((!forId.IsBlock()) ? forId.UnlockedBy : forId.GetBlock().UnlockedBy);
				int num = 0;
				int count = unlockByEntries.Count;
				_ = base.xui.playerUI.entityPlayer.Progression;
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (unlockByEntries[i] is XUiC_UnlockByEntry xUiC_UnlockByEntry)
						{
							xUiC_UnlockByEntry.UnlockData = array[i];
							xUiC_UnlockByEntry.Recipe = recipe;
							num++;
						}
					}
				}
				for (int j = num; j < count; j++)
				{
					if (unlockByEntries[j] is XUiC_UnlockByEntry xUiC_UnlockByEntry2)
					{
						xUiC_UnlockByEntry2.UnlockData = null;
					}
				}
			}
			else
			{
				int count2 = unlockByEntries.Count;
				for (int k = 0; k < count2; k++)
				{
					if (unlockByEntries[k] is XUiC_UnlockByEntry xUiC_UnlockByEntry3)
					{
						xUiC_UnlockByEntry3.UnlockData = null;
					}
				}
			}
			isDirty = false;
		}
		base.Update(_dt);
	}
}
