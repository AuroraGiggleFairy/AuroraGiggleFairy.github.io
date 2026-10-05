using UnityEngine;

public static class SmeltTimerCalculator
{
	/// <summary>
	/// Matches TileEntityWorkstation.HandleMaterialInput per-item melt duration,
	/// including bellows / CraftingSmeltTime tool modifiers.
	/// </summary>
	public static float GetPerItemMeltSeconds(ItemClass itemClass, ItemStack[] tools, bool toolsModuleUsed)
	{
		if (itemClass == null)
		{
			return 0f;
		}

		float meltSeconds = itemClass.GetWeight() * ((itemClass.MeltTimePerUnit > 0f) ? itemClass.MeltTimePerUnit : 1f);
		if (!toolsModuleUsed || tools == null)
		{
			return Mathf.Max(0f, meltSeconds);
		}

		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse(itemClass.Name);
		for (int i = 0; i < tools.Length; i++)
		{
			if (tools[i] == null || tools[i].IsEmpty())
			{
				continue;
			}

			ItemValue toolValue = StackAccess.Value(tools[i]);
			if (toolValue == null)
			{
				continue;
			}

			float perc = 1f;
			toolValue.ModifyValue(null, null, PassiveEffects.CraftingSmeltTime, ref meltSeconds, ref perc, tags);
			meltSeconds *= perc;
		}

		return Mathf.Max(0f, meltSeconds);
	}

	public static float GetTotalMeltSeconds(float remainingForCurrentItem, int stackCount, float perItemSeconds)
	{
		if (stackCount <= 0 || perItemSeconds <= 0f)
		{
			return 0f;
		}

		if (remainingForCurrentItem > 0f)
		{
			return remainingForCurrentItem + (stackCount - 1) * perItemSeconds;
		}

		return stackCount * perItemSeconds;
	}

	public static string FormatTimer(float seconds)
	{
		float display = seconds + 0.95f;
		return string.Format(
			"{0}:{1}",
			Mathf.Floor(display / 60f).ToCultureInvariantString("00"),
			Mathf.Floor(display % 60f).ToCultureInvariantString("00"));
	}
}
