using System.Collections.Generic;
using UnityEngine;

public class ProgressionClass
{
	public enum DisplayTypes
	{
		Standard,
		Book,
		Crafting
	}

	public class ListSortOrderComparer : IComparer<ProgressionValue>
	{
		public static ListSortOrderComparer Instance = new ListSortOrderComparer();

		[PublicizedFrom(EAccessModifier.Private)]
		public ListSortOrderComparer()
		{
		}

		public int Compare(ProgressionValue _x, ProgressionValue _y)
		{
			return _x.ProgressionClass.ListSortOrder.CompareTo(_y.ProgressionClass.ListSortOrder);
		}
	}

	public class DisplayData
	{
		public class UnlockData
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public ItemClass item;

			public string[] RecipeList;

			public string ItemName = "";

			public int UnlockTier;

			public ItemClass Item
			{
				get
				{
					if (item == null)
					{
						item = ItemClass.GetItemClass(ItemName);
					}
					return item;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass item;

		public string ItemName = "";

		public string[] CustomIcon;

		public string[] CustomIconTint;

		public string[] CustomName;

		public bool CustomHasQuality;

		public int[] QualityStarts;

		public List<UnlockData> UnlockDataList = new List<UnlockData>();

		public static string CompletionSound = "";

		public ProgressionClass Owner;

		public ItemClass Item
		{
			get
			{
				if (item == null)
				{
					item = ItemClass.GetItemClass(ItemName);
				}
				return item;
			}
		}

		public bool HasQuality
		{
			get
			{
				if (ItemName != "")
				{
					return Item.HasQuality;
				}
				return CustomHasQuality;
			}
		}

		public string GetName(int level)
		{
			if (ItemName != "")
			{
				return Item.GetLocalizedItemName();
			}
			if (CustomName == null)
			{
				return "";
			}
			int num = GetQualityLevel(level);
			if (num >= CustomName.Length)
			{
				num = 0;
			}
			return CustomName[num];
		}

		public string GetIcon(int level)
		{
			if (ItemName != "")
			{
				return Item.GetIconName();
			}
			if (CustomIcon == null)
			{
				return "";
			}
			int num = GetQualityLevel(level);
			if (num >= CustomIcon.Length)
			{
				num = 0;
			}
			return CustomIcon[num];
		}

		public string GetIconTint(int level)
		{
			if (ItemName != "")
			{
				return Utils.ColorToHex(Item.GetIconTint());
			}
			if (CustomIconTint == null)
			{
				return "FFFFFF";
			}
			int num = GetQualityLevel(level);
			if (num >= CustomName.Length)
			{
				num = 0;
			}
			return CustomIconTint[num];
		}

		public int GetQualityLevel(int level)
		{
			for (int i = 0; i < QualityStarts.Length; i++)
			{
				if (QualityStarts[i] > level)
				{
					return i;
				}
			}
			return QualityStarts.Length;
		}

		public int GetNextPoints(int level)
		{
			for (int i = 0; i < QualityStarts.Length; i++)
			{
				if (QualityStarts[i] > level)
				{
					return QualityStarts[i];
				}
			}
			return 0;
		}

		public bool IsComplete(int level)
		{
			for (int i = 0; i < QualityStarts.Length; i++)
			{
				if (QualityStarts[i] > level)
				{
					return false;
				}
			}
			return true;
		}

		public void AddUnlockData(string itemName, int unlockTier, string[] recipeList)
		{
			if (UnlockDataList == null)
			{
				UnlockDataList = new List<UnlockData>();
			}
			UnlockDataList.Add(new UnlockData
			{
				ItemName = itemName,
				UnlockTier = unlockTier,
				RecipeList = recipeList
			});
		}

		public ItemClass GetUnlockItem(int index)
		{
			if (UnlockDataList == null)
			{
				return null;
			}
			if (index >= UnlockDataList.Count)
			{
				return null;
			}
			return UnlockDataList[index].Item;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public UnlockData GetUnlockData(int index)
		{
			if (UnlockDataList == null)
			{
				return null;
			}
			if (index >= UnlockDataList.Count)
			{
				return null;
			}
			return UnlockDataList[index];
		}

		public string GetUnlockItemIconName(int index)
		{
			ItemClass unlockItem = GetUnlockItem(index);
			if (unlockItem != null)
			{
				return unlockItem.GetIconName();
			}
			return "";
		}

		public string GetUnlockItemName(int index)
		{
			ItemClass unlockItem = GetUnlockItem(index);
			if (unlockItem != null)
			{
				return unlockItem.GetLocalizedItemName();
			}
			return "";
		}

		public List<int> GetUnlockItemRecipes(int index)
		{
			if (UnlockDataList == null)
			{
				return null;
			}
			if (index >= UnlockDataList.Count)
			{
				return null;
			}
			List<int> list = new List<int>();
			if (Item != null)
			{
				list.Add(Item.Id);
			}
			else
			{
				UnlockData unlockData = UnlockDataList[index];
				if (unlockData != null)
				{
					if (unlockData.RecipeList != null)
					{
						for (int i = 0; i < unlockData.RecipeList.Length; i++)
						{
							list.Add(ItemClass.GetItemClass(unlockData.RecipeList[i], _caseInsensitive: true).Id);
						}
					}
					else if (unlockData.Item != null)
					{
						list.Add(unlockData.Item.Id);
					}
				}
			}
			return list;
		}

		public string GetUnlockItemIconAtlas(EntityPlayerLocal player, int index)
		{
			UnlockData unlockData = GetUnlockData(index);
			if (unlockData != null)
			{
				if (GetQualityLevel(player.Progression.GetProgressionValue(Owner.Name).Level) <= unlockData.UnlockTier)
				{
					return "ItemIconAtlasGreyscale";
				}
				return "ItemIconAtlas";
			}
			return "ItemIconAtlas";
		}

		public bool GetUnlockItemLocked(EntityPlayerLocal player, int index)
		{
			UnlockData unlockData = GetUnlockData(index);
			if (unlockData != null)
			{
				return GetQualityLevel(player.Progression.GetProgressionValue(Owner.Name).Level) <= unlockData.UnlockTier;
			}
			return false;
		}

		public void HandleCheckCrafting(EntityPlayerLocal _player, int _oldLevel, int _newLevel)
		{
			if (UnlockDataList == null)
			{
				return;
			}
			for (int i = 0; i < QualityStarts.Length; i++)
			{
				int num = QualityStarts[i];
				if (_oldLevel < num && _newLevel >= num)
				{
					if (HasQuality)
					{
						GameManager.ShowTooltip(_player, Localization.Get("ttCraftingSkillUnlockQuality"), new string[3]
						{
							Localization.Get(Owner.NameKey),
							GetName(_newLevel),
							(i + 1).ToString()
						}, CompletionSound);
					}
					else
					{
						GameManager.ShowTooltip(_player, Localization.Get("ttCraftingSkillUnlock"), new string[2]
						{
							Localization.Get(Owner.NameKey),
							GetName(_oldLevel)
						}, CompletionSound);
					}
				}
			}
		}
	}

	public readonly string Name;

	public readonly FastTags<TagGroup.Global> NameTag;

	public float ParentMaxLevelRatio = 1f;

	public string NameKey;

	public string DescKey;

	public string LongDescKey;

	public string Icon;

	public int MinLevel;

	public int MaxLevel;

	public int BaseCostToLevel;

	public float CostMultiplier;

	public bool Hidden;

	public int[] OverrideCost;

	public DisplayTypes DisplayType;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryList<int, LevelRequirement> LevelRequirements = new DictionaryList<int, LevelRequirement>();

	public readonly List<ProgressionClass> Children = new List<ProgressionClass>();

	public string ParentName;

	public ProgressionType Type;

	[PublicizedFrom(EAccessModifier.Private)]
	public float listSortOrder;

	public List<DisplayData> DisplayDataList;

	public bool IsBookGroup => Type == ProgressionType.BookGroup;

	public bool IsBook => Type == ProgressionType.Book;

	public ProgressionCurrencyType CurrencyType => Type switch
	{
		ProgressionType.Attribute => ProgressionCurrencyType.SP, 
		ProgressionType.Skill => ProgressionCurrencyType.XP, 
		ProgressionType.Perk => ProgressionCurrencyType.SP, 
		_ => ProgressionCurrencyType.None, 
	};

	public ProgressionClass Parent
	{
		get
		{
			if (ParentName == null)
			{
				return this;
			}
			if (Progression.ProgressionClasses.TryGetValue(ParentName, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public bool IsPerk => Type == ProgressionType.Perk;

	public bool IsSkill => Type == ProgressionType.Skill;

	public bool IsAttribute => Type == ProgressionType.Attribute;

	public bool IsCrafting => Type == ProgressionType.Crafting;

	public float ListSortOrder
	{
		get
		{
			if (IsPerk)
			{
				return Parent.ListSortOrder + listSortOrder * 0.001f;
			}
			if (IsSkill)
			{
				return Parent.ListSortOrder + listSortOrder;
			}
			return listSortOrder * 100f;
		}
		set
		{
			listSortOrder = value;
		}
	}

	public bool ValidDisplay(DisplayTypes displayType)
	{
		switch (displayType)
		{
		case DisplayTypes.Standard:
			if (Type != ProgressionType.BookGroup)
			{
				return Type != ProgressionType.Crafting;
			}
			return false;
		case DisplayTypes.Book:
			return Type == ProgressionType.BookGroup;
		case DisplayTypes.Crafting:
			return Type == ProgressionType.Crafting;
		default:
			return false;
		}
	}

	public DisplayData AddDisplayData(string _item, int[] _qualityStarts, string[] _customIcon, string[] _customIconTint, string[] _customName, bool _customHasQuality)
	{
		if (DisplayDataList == null)
		{
			DisplayDataList = new List<DisplayData>();
		}
		DisplayData displayData = new DisplayData
		{
			ItemName = _item,
			QualityStarts = _qualityStarts,
			Owner = this,
			CustomIcon = _customIcon,
			CustomIconTint = _customIconTint,
			CustomName = _customName,
			CustomHasQuality = _customHasQuality
		};
		DisplayDataList.Add(displayData);
		return displayData;
	}

	public ProgressionClass(string _name)
	{
		Name = _name;
		NameKey = Name;
		NameTag = FastTags<TagGroup.Global>.GetTag(_name);
		DescKey = "";
		ListSortOrder = float.MaxValue;
		ParentName = null;
		Type = ProgressionType.None;
	}

	public void ModifyValue(EntityAlive _ea, ProgressionValue _pv, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (Effects != null)
		{
			Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, _pv.GetCalculatedLevel(_ea), _tags);
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _ea, ProgressionValue _pv, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (Effects != null)
		{
			Effects.GetModifiedValueData(_modValueSources, _sourceType, _ea, _effect, ref _base_value, ref _perc_value, _pv.GetCalculatedLevel(_ea), _tags);
		}
	}

	public bool HasEvents()
	{
		if (Effects != null)
		{
			return Effects.HasEvents();
		}
		return false;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (Effects != null)
		{
			Effects.FireEvent(_eventType, _params);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canRun(RequirementGroup Requirements, MinEventParams _params)
	{
		return Requirements?.IsValid(_params) ?? true;
	}

	public LevelRequirement GetRequirementsForLevel(int _level)
	{
		if (LevelRequirements.Count <= 0)
		{
			return new LevelRequirement(_level);
		}
		if (LevelRequirements.dict.TryGetValue(_level, out var value))
		{
			return value;
		}
		return new LevelRequirement(_level);
	}

	public void AddLevelRequirement(LevelRequirement _lr)
	{
		LevelRequirements.Add(_lr.Level, _lr);
	}

	public void PostInit()
	{
		LevelRequirements.list.Sort([PublicizedFrom(EAccessModifier.Internal)] (LevelRequirement a, LevelRequirement b) => a.Level - b.Level);
	}

	public static int GetCalculatedMaxLevel(EntityAlive _ea, ProgressionValue _pv)
	{
		ProgressionClass progressionClass = _pv.ProgressionClass;
		int num = 0;
		if (progressionClass.LevelRequirements.Count <= 0)
		{
			num = ((!progressionClass.IsAttribute) ? progressionClass.MaxLevel : 20);
		}
		else
		{
			List<LevelRequirement> list = progressionClass.LevelRequirements.list;
			int num2 = 0;
			int num3 = list.Count;
			while (num2 < num3)
			{
				int num4 = (num3 + num2) / 2;
				LevelRequirement levelRequirement = list[num4];
				if (canRun(levelRequirement.Requirements, _ea.MinEventContext))
				{
					num = levelRequirement.Level;
					num2 = num4 + 1;
				}
				else
				{
					num3 = num4;
				}
			}
			if (!progressionClass.IsAttribute)
			{
				if (num < progressionClass.MinLevel)
				{
					num = progressionClass.MinLevel;
				}
				if (num > progressionClass.MaxLevel)
				{
					num = progressionClass.MaxLevel;
				}
			}
		}
		return num;
	}

	public int CalculatedCostForLevel(int _level)
	{
		if (OverrideCost == null)
		{
			return (int)(Mathf.Pow(CostMultiplier, _level) * (float)BaseCostToLevel);
		}
		if (_level - 1 < OverrideCost.Length)
		{
			return OverrideCost[_level - 1];
		}
		return 0;
	}

	public float GetPercentThisLevel(ProgressionValue _pv)
	{
		if (Type == ProgressionType.Skill)
		{
			if (_pv.Level == MaxLevel)
			{
				return 0f;
			}
			float num = (float)((int)(Mathf.Pow(CostMultiplier, _pv.Level) * (float)BaseCostToLevel) - _pv.CostForNextLevel) / (Mathf.Pow(CostMultiplier, _pv.Level) * (float)BaseCostToLevel);
			if (!float.IsNaN(num))
			{
				return num;
			}
			return 0f;
		}
		return 0f;
	}

	public void HandleCheckCrafting(EntityPlayerLocal _player, int _oldLevel, int _newLevel)
	{
		if (DisplayDataList != null)
		{
			for (int i = 0; i < DisplayDataList.Count; i++)
			{
				DisplayDataList[i].HandleCheckCrafting(_player, _oldLevel, _newLevel);
			}
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()}, {Name}, lvl {MinLevel} to {MaxLevel}";
	}
}
