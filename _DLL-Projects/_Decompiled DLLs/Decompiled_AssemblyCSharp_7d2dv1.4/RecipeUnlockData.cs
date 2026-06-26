using UnityEngine;

public class RecipeUnlockData
{
	public enum UnlockTypes
	{
		None,
		Perk,
		Book,
		Skill,
		Schematic
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UnlockTypes unlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass item;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass perk;

	public UnlockTypes UnlockType
	{
		get
		{
			return unlockType;
		}
		set
		{
			unlockType = value;
		}
	}

	public ItemClass Item
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
			unlockType = UnlockTypes.Schematic;
		}
	}

	public ProgressionClass Perk
	{
		get
		{
			return perk;
		}
		set
		{
			perk = value;
			if (perk.IsBook)
			{
				unlockType = UnlockTypes.Book;
			}
			else if (perk.IsCrafting)
			{
				unlockType = UnlockTypes.Skill;
			}
			else
			{
				unlockType = UnlockTypes.Perk;
			}
		}
	}

	public RecipeUnlockData(string unlock)
	{
		if (Progression.ProgressionClasses.ContainsKey(unlock))
		{
			Perk = Progression.ProgressionClasses[unlock];
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(unlock, _caseInsensitive: true);
		if (itemClass != null)
		{
			Item = itemClass;
		}
		else
		{
			UnlockType = UnlockTypes.None;
		}
	}

	public string GetName()
	{
		if (unlockType == UnlockTypes.Schematic)
		{
			return item.GetLocalizedItemName();
		}
		return Localization.Get(perk.NameKey);
	}

	public string GetIcon()
	{
		if (unlockType == UnlockTypes.Schematic)
		{
			return "ui_game_symbol_book";
		}
		if (unlockType == UnlockTypes.Perk)
		{
			return "ui_game_symbol_skills";
		}
		if (unlockType == UnlockTypes.Skill)
		{
			return "ui_game_symbol_hammer";
		}
		return "ui_game_symbol_book";
	}

	public string GetLevel(EntityPlayerLocal player, string recipeName)
	{
		if (unlockType == UnlockTypes.Skill)
		{
			for (int i = 0; i < perk.DisplayDataList.Count; i++)
			{
				ProgressionClass.DisplayData displayData = perk.DisplayDataList[i];
				for (int j = 0; j < displayData.UnlockDataList.Count; j++)
				{
					ProgressionClass.DisplayData.UnlockData unlockData = displayData.UnlockDataList[j];
					if (unlockData.ItemName == recipeName || (unlockData.RecipeList != null && unlockData.RecipeList.ContainsCaseInsensitive(recipeName)))
					{
						int unlockTier = unlockData.UnlockTier;
						ProgressionValue progressionValue = player.Progression.GetProgressionValue(perk.Name);
						return $"{progressionValue.Level}/{displayData.QualityStarts[unlockTier].ToString()}";
					}
				}
			}
			return "--";
		}
		return "--";
	}

	public string GetIconAtlas()
	{
		return "UIAtlas";
	}

	public Color GetItemTint()
	{
		if (unlockType == UnlockTypes.Schematic && item != null)
		{
			return item.GetIconTint();
		}
		return Color.white;
	}
}
