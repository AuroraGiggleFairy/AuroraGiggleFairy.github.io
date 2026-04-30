using Challenges;
using UnityEngine;

public class RecipeUnlockData
{
	public enum UnlockTypes
	{
		None,
		Perk,
		Book,
		Skill,
		Schematic,
		ChallengeGroup,
		Challenge,
		PrefabEditorInvalid
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UnlockTypes unlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unlockText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass item;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass perk;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroup challengeGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challenge;

	public UnlockTypes UnlockType
	{
		get
		{
			if (unlockText != "")
			{
				Init();
			}
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

	public ChallengeGroup ChallengeGroup
	{
		get
		{
			return challengeGroup;
		}
		set
		{
			challengeGroup = value;
			unlockType = UnlockTypes.ChallengeGroup;
		}
	}

	public ChallengeClass Challenge
	{
		get
		{
			return challenge;
		}
		set
		{
			challenge = value;
			unlockType = UnlockTypes.Challenge;
		}
	}

	public RecipeUnlockData(string unlock)
	{
		unlockText = unlock;
	}

	public void Init()
	{
		ChallengeGroup challengeGroup = null;
		ChallengeClass challengeClass = null;
		if (Progression.ProgressionClasses.ContainsKey(unlockText))
		{
			Perk = Progression.ProgressionClasses[unlockText];
			return;
		}
		if ((challengeGroup = ChallengeGroup.GetGroup(unlockText)) != null)
		{
			ChallengeGroup = challengeGroup;
			return;
		}
		if ((challengeClass = ChallengeClass.GetChallenge(unlockText)) != null)
		{
			Challenge = challengeClass;
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(unlockText, _caseInsensitive: true);
		if (itemClass != null)
		{
			Item = itemClass;
		}
		else
		{
			UnlockType = (GameManager.Instance.IsEditMode() ? UnlockTypes.PrefabEditorInvalid : UnlockTypes.None);
		}
	}

	public string GetName()
	{
		if (unlockText != "")
		{
			Init();
		}
		if (unlockType == UnlockTypes.Schematic)
		{
			return item.GetLocalizedItemName();
		}
		if (unlockType == UnlockTypes.ChallengeGroup)
		{
			return ChallengeGroup.Title;
		}
		if (unlockType == UnlockTypes.Challenge)
		{
			return Challenge.Title;
		}
		return Localization.Get(perk.NameKey);
	}

	public string GetIcon()
	{
		if (unlockText != "")
		{
			Init();
		}
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
		if (unlockType == UnlockTypes.ChallengeGroup || unlockType == UnlockTypes.Challenge)
		{
			return "ui_game_symbol_challenge";
		}
		return "ui_game_symbol_book";
	}

	public string GetLevel(EntityPlayerLocal player, string recipeName)
	{
		if (unlockText != "")
		{
			Init();
		}
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
		if (unlockText != "")
		{
			Init();
		}
		if (unlockType == UnlockTypes.Schematic && item != null)
		{
			return item.GetIconTint();
		}
		return Color.white;
	}
}
