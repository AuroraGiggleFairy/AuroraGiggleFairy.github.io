using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using XMLData.Item;

[Preserve]
public class ItemClass : ItemData
{
	public enum EnumCrosshairType
	{
		None,
		Plus,
		Crosshair,
		CrosshairOnAiming,
		Damage,
		Upgrade,
		Repair,
		PowerSource,
		Heal,
		PowerItem,
		Blocked
	}

	public delegate bool FilterItem(ItemClass _class, Block _block);

	public static readonly FastTags<TagGroup.Global> EatDistractionTag = FastTags<TagGroup.Global>.GetTag("eat");

	public static readonly FastTags<TagGroup.Global> RequiresContactDistractionTag = FastTags<TagGroup.Global>.GetTag("requires_contact");

	public static string PropSoundIdle = "SoundIdle";

	public static string PropSoundDestroy = "SoundDestroy";

	public static string PropSoundJammed = "SoundJammed";

	public static string PropSoundHolster = "SoundHolster";

	public static string PropSoundUnholster = "SoundUnholster";

	public static string PropSoundStick = "SoundStick";

	public static string PropSoundTick = "SoundTick";

	public static string PropSoundPickup = "SoundPickup";

	public static string PropSoundPlace = "SoundPlace";

	public static string PropHasReloadAnim = "HasReloadAnim";

	public static string PropFuelValue = "FuelValue";

	public static string PropWeight = "Weight";

	public static string PropMoldTarget = "MoldTarget";

	public static string PropSmell = "Smell";

	public static string PropLightSource = "LightSource";

	public static string PropLightValue = "LightValue";

	public static string PropMatEmission = "MatEmission";

	public static string PropActivateObject = "ActivateObject";

	public static string PropThrowableDecoy = "ThrowableDecoy";

	public static string PropGroupName = "Group";

	public static string PropCritChance = "CritChance";

	public static string PropCustomIcon = "CustomIcon";

	public static string PropCustomIconTint = "CustomIconTint";

	public static string PropPartType = "PartType";

	public static string PropImageEffectOnActive = "ImageEffectOnActive";

	public static string PropPlaySoundOnActive = "PlaySoundOnActive";

	public static string PropActive = "Active";

	public static string PropAlwaysActive = "AlwaysActive";

	public static string PropHoldingItemHidden = "HoldingItemHidden";

	public static string PropVehicleSlotType = "VehicleSlotType";

	public static string PropGetQualityFromWeapon = "GetQualityFromWeapon";

	public static string PropAttributes = "Attributes";

	public static string PropCraftExpValue = "CraftingIngredientExp";

	public static string PropCraftTimeValue = "CraftingIngredientTime";

	public static string PropLootExpValue = "LootExpValue";

	public static string PropEconomicValue = "EconomicValue";

	public static string PropEconomicSellScale = "EconomicSellScale";

	public static string PropEconomicBundleSize = "EconomicBundleSize";

	public static string PropSellableToTrader = "SellableToTrader";

	public static string PropCraftingSkillExp = "CraftingSkillExp";

	public static string PropActionSkillExp = "ActionSkillExp";

	public static string PropInsulation = "Insulation";

	public static string PropWaterproof = "Waterproof";

	public static string PropEncumbrance = "Encumbrance";

	public static string PropDescriptionKey = "DescriptionKey";

	public static string PropResourceUnit = "ResourceUnit";

	public static string PropMeltTimePerUnit = "MeltTimePerUnit";

	public static string PropActionSkillGroup = "ActionSkillGroup";

	public static string PropCraftingSkillGroup = "CraftingSkillGroup";

	public static string PropCrosshairOnAim = "CrosshairOnAim";

	public static string PropCrosshairUpAfterShot = "CrosshairUpAfterShot";

	public static string PropUsableUnderwater = "UsableUnderwater";

	public static string PropRepairExpMultiplier = "RepairExpMultiplier";

	public static string PropTags = "Tags";

	public static string PropShowQuality = "ShowQuality";

	public static string PropSoundSightIn = "Sound_Sight_In";

	public static string PropSoundSightOut = "Sound_Sight_Out";

	public static string PropIgnoreKeystoneSound = "IgnoreKeystoneSound";

	public static string PropCreativeMode = "CreativeMode";

	public static string PropCreativeSort1 = "SortOrder1";

	public static string PropCreativeSort2 = "SortOrder2";

	public static string PropDistractionTags = "DistractionTags";

	public static string PropIsSticky = "IsSticky";

	public static string PropDisplayType = "DisplayType";

	public static string PropItemTypeIcon = "ItemTypeIcon";

	public static string PropAltItemTypeIcon = "AltItemTypeIcon";

	public static string PropAltItemTypeIconColor = "AltItemTypeIconColor";

	public static string PropUnlockedBy = "UnlockedBy";

	public static string PropUnlocks = "Unlocks";

	public static string PropNoScrapping = "NoScrapping";

	public static string PropScrapTimeOverride = "ScrapTimeOverride";

	public static string PropNavObject = "NavObject";

	public static string PropQuestItem = "IsQuestItem";

	public static string PropTrackerIndexName = "TrackerIndexName";

	public static string PropTrackerNavObject = "TrackerNavObject";

	public static string PropTraderStageTemplate = "TraderStageTemplate";

	public static string PropMaxModsAllowed = "MaxModsAllowed";

	public static int MAX_ITEMS = Block.MAX_BLOCKS + 16384;

	public static NameIdMapping nameIdMapping;

	public static byte[] fullMappingDataForClients;

	public static ItemClass[] list;

	public static string[] itemActionNames;

	public const int cMaxActionNames = 5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, ItemClass> nameToItem = new Dictionary<string, ItemClass>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, ItemClass> nameToItemCaseInsensitive = new CaseInsensitiveStringDictionary<ItemClass>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<string> itemNames = new List<string>();

	public static readonly ReadOnlyCollection<string> ItemNames = new ReadOnlyCollection<string>(itemNames);

	public DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	public float Smell;

	public string MeshFile;

	public string DropMeshFile;

	public string HandMeshFile;

	public string StickyMaterial;

	public float StickyOffset = 0.15f;

	public int StickyColliderUp = -1;

	public float StickyColliderRadius = 0.2f;

	public float StickyColliderLength = -1f;

	public bool IsSticky;

	public const int cActionUpdateCount = 3;

	public ItemAction[] Actions;

	public MaterialBlock MadeOfMaterial;

	public DataItem<int> HoldType = new DataItem<int>(0);

	public DataItem<int> Stacknumber = new DataItem<int>(500);

	public DataItem<int> MaxUseTimes = new DataItem<int>(0);

	public DataItem<bool> MaxUseTimesBreaksAfter = new DataItem<bool>(_startValue: false);

	public ItemClass MoldTarget;

	public DataItem<string> LightSource;

	public DataItem<string> ActivateObject;

	public DataItem<bool> ThrowableDecoy;

	public DataItemArrayRepairTools RepairTools;

	public DataItem<int> RepairAmount;

	public DataItem<float> RepairTime;

	public DataItem<float> CritChance;

	public string[] Groups = new string[1] { "Decor/Miscellaneous" };

	public DataItem<string> CustomIcon;

	public Color CustomIconTint;

	public DataItem<bool> UserHidden = new DataItem<bool>(_startValue: false);

	public string VehicleSlotType;

	public bool GetQualityFromWeapon;

	public string DescriptionKey;

	public bool IsResourceUnit;

	public float MeltTimePerUnit;

	public string ActionSkillGroup = "";

	public string CraftingSkillGroup = "";

	public bool UsableUnderwater = true;

	public float lightValue;

	public string soundSightIn = "";

	public string soundSightOut = "";

	public bool ignoreKeystoneSound;

	public bool HoldingItemHidden;

	public List<int> PartParentId;

	public PreviewData Preview;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject renderGameObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCanHold;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCanDrop;

	public bool HasSubItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] attachments;

	public bool bCraftingTool;

	public float CraftComponentExp = 3f;

	public float CraftComponentTime = 1f;

	public float LootExp = 1f;

	public float EconomicValue;

	public float EconomicSellScale = 1f;

	public int EconomicBundleSize = 1;

	public bool SellableToTrader = true;

	public string TraderStageTemplate;

	public int MaxModsAllowed = 1;

	public int CraftingSkillExp = 10;

	public int ActionSkillExp = 10;

	public float RepairExpMultiplier = 10f;

	public float Insulation;

	public float WaterProof;

	public float Encumbrance;

	public string SoundUnholster = "generic_unholster";

	public string SoundHolster = "generic_holster";

	public string SoundStick;

	public string SoundTick;

	public float SoundTickDelay = 1f;

	public string SoundPickup = "craft_take_item";

	public string SoundPlace = "craft_place_item";

	public bool HasReloadAnim = true;

	public bool bShowCrosshairOnAiming;

	public bool bCrosshairUpAfterShot;

	public EnumCreativeMode CreativeMode;

	public string SortOrder;

	public FastTags<TagGroup.Global> ItemTags;

	public MinEffectController Effects;

	public FastTags<TagGroup.Global> DistractionTags;

	public SDCSUtils.SlotData SDCSData;

	public string DisplayType;

	public string ItemTypeIcon = "";

	public string AltItemTypeIcon;

	public Color AltItemTypeIconColor;

	public bool ShowQualityBar;

	public bool NoScrapping;

	public float ScrapTimeOverride;

	public string Unlocks = "";

	public string NavObject = "";

	public bool IsQuestItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> stopBleed = FastTags<TagGroup.Global>.Parse("stopsBleeding");

	public string TrackerIndexName;

	public string TrackerNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData[] unlockedBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ActionProfilerNames = new string[3] { "action0", "action1", "action2" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> fixedItemIds = new Dictionary<string, int>();

	public string[] Attachments => attachments;

	public bool HasQuality
	{
		get
		{
			if (Effects != null)
			{
				return Effects.IsOwnerTiered();
			}
			return false;
		}
	}

	public virtual bool IsEquipment => false;

	public RecipeUnlockData[] UnlockedBy
	{
		get
		{
			if (unlockedBy == null)
			{
				if (Properties.Values.ContainsKey(PropUnlockedBy))
				{
					string[] array = Properties.Values[PropUnlockedBy].Split(',');
					if (array.Length != 0)
					{
						unlockedBy = new RecipeUnlockData[array.Length];
						for (int i = 0; i < array.Length; i++)
						{
							unlockedBy[i] = new RecipeUnlockData(array[i]);
						}
					}
				}
				else
				{
					unlockedBy = new RecipeUnlockData[0];
				}
			}
			return unlockedBy;
		}
	}

	public bool IsEatDistraction => DistractionTags.Test_AnySet(EatDistractionTag);

	public bool IsRequireContactDistraction => DistractionTags.Test_AnySet(RequiresContactDistractionTag);

	public static ItemClass MissingItem => GetItemClass("missingItem");

	public ItemClass()
	{
		bCanHold = true;
		bCanDrop = true;
		bCraftingTool = false;
		Properties = new DynamicProperties();
		attachments = null;
		Actions = new ItemAction[5];
		for (int i = 0; i < Actions.Length; i++)
		{
			Actions[i] = null;
		}
	}

	public void SetId(int _id)
	{
		pId = _id;
		if (Effects != null)
		{
			Effects.ParentPointer = _id;
		}
	}

	public virtual void Init()
	{
		string itemName = GetItemName();
		nameToItem[itemName] = this;
		nameToItemCaseInsensitive[itemName] = this;
		itemNames.Add(itemName);
		if (Properties.Values.ContainsKey(PropTags))
		{
			ItemTags = FastTags<TagGroup.Global>.Parse(Properties.Values[PropTags]);
		}
		if (Properties.Values.ContainsKey(PropDistractionTags))
		{
			DistractionTags = FastTags<TagGroup.Global>.Parse(Properties.Values[PropDistractionTags]);
		}
		if (Properties.Values.ContainsKey(PropFuelValue))
		{
			int result = 0;
			int.TryParse(Properties.Values[PropFuelValue], out result);
			base.FuelValue = new DataItem<int>(result);
		}
		if (Properties.Values.ContainsKey(PropWeight))
		{
			int.TryParse(Properties.Values[PropWeight], out var result2);
			base.Weight = new DataItem<int>(result2);
		}
		if (Properties.Values.ContainsKey(PropImageEffectOnActive))
		{
			string startValue = Properties.Values[PropImageEffectOnActive].ToString();
			base.ImageEffectOnActive = new DataItem<string>(startValue);
		}
		if (Properties.Values.ContainsKey(PropActive))
		{
			bool _result = false;
			StringParsers.TryParseBool(Properties.Values[PropActive], out _result);
			base.Active = new DataItem<bool>(_result);
		}
		if (Properties.Values.ContainsKey(PropAlwaysActive))
		{
			bool _result2 = false;
			StringParsers.TryParseBool(Properties.Values[PropAlwaysActive], out _result2);
			base.AlwaysActive = new DataItem<bool>(_result2);
		}
		if (Properties.Values.ContainsKey(PropPlaySoundOnActive))
		{
			string startValue2 = Properties.Values[PropPlaySoundOnActive].ToString();
			base.PlaySoundOnActive = new DataItem<string>(startValue2);
		}
		if (Properties.Values.ContainsKey(PropLightSource))
		{
			LightSource = new DataItem<string>(Properties.Values[PropLightSource]);
		}
		if (Properties.Values.ContainsKey(PropLightValue))
		{
			lightValue = StringParsers.ParseFloat(Properties.Values[PropLightValue]);
		}
		if (Properties.Values.ContainsKey(PropSoundSightIn))
		{
			soundSightIn = Properties.Values[PropSoundSightIn].ToString();
		}
		if (Properties.Values.ContainsKey(PropSoundSightOut))
		{
			soundSightOut = Properties.Values[PropSoundSightOut].ToString();
		}
		Properties.ParseBool(PropIgnoreKeystoneSound, ref ignoreKeystoneSound);
		if (Properties.Values.ContainsKey(PropActivateObject))
		{
			ActivateObject = new DataItem<string>(Properties.Values[PropActivateObject]);
		}
		if (Properties.Values.ContainsKey(PropThrowableDecoy))
		{
			StringParsers.TryParseBool(Properties.Values[PropThrowableDecoy], out var _result3);
			ThrowableDecoy = new DataItem<bool>(_result3);
		}
		else
		{
			ThrowableDecoy = new DataItem<bool>(_startValue: false);
		}
		if (Properties.Values.ContainsKey(PropCustomIcon))
		{
			CustomIcon = new DataItem<string>(Properties.Values[PropCustomIcon]);
		}
		if (Properties.Values.ContainsKey(PropCustomIconTint))
		{
			CustomIconTint = StringParsers.ParseHexColor(Properties.Values[PropCustomIconTint]);
		}
		else
		{
			CustomIconTint = Color.white;
		}
		if (Properties.Values.ContainsKey(PropGroupName))
		{
			string[] array = Properties.Values[PropGroupName].Split(',');
			if (array.Length != 0)
			{
				Groups = new string[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					Groups[i] = array[i].Trim();
				}
			}
		}
		if (Properties.Values.ContainsKey(PropCritChance))
		{
			CritChance = new DataItem<float>(StringParsers.ParseFloat(Properties.Values[PropCritChance]));
		}
		else
		{
			CritChance = new DataItem<float>(0f);
		}
		if (Properties.Values.ContainsKey(PropVehicleSlotType))
		{
			VehicleSlotType = Properties.Values[PropVehicleSlotType];
		}
		else
		{
			VehicleSlotType = string.Empty;
		}
		if (Properties.Values.ContainsKey(PropHoldingItemHidden))
		{
			HoldingItemHidden = StringParsers.ParseBool(Properties.Values[PropHoldingItemHidden]);
		}
		else
		{
			HoldingItemHidden = false;
		}
		if (Properties.Values.ContainsKey(PropCraftExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropCraftExpValue], out CraftComponentExp);
		}
		if (Properties.Values.ContainsKey(PropRepairExpMultiplier))
		{
			StringParsers.TryParseFloat(Properties.Values[PropRepairExpMultiplier], out RepairExpMultiplier);
		}
		if (Properties.Values.ContainsKey(PropCraftTimeValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropCraftTimeValue], out CraftComponentTime);
		}
		if (Properties.Values.ContainsKey(PropLootExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropLootExpValue], out LootExp);
		}
		Properties.ParseFloat(PropEconomicValue, ref EconomicValue);
		Properties.ParseFloat(PropEconomicSellScale, ref EconomicSellScale);
		Properties.ParseInt(PropEconomicBundleSize, ref EconomicBundleSize);
		Properties.ParseBool(PropSellableToTrader, ref SellableToTrader);
		if (Properties.Values.ContainsKey(PropCreativeMode))
		{
			CreativeMode = EnumUtils.Parse<EnumCreativeMode>(Properties.Values[PropCreativeMode]);
		}
		SortOrder = Properties.GetString(PropCreativeSort1);
		SortOrder += Properties.GetString(PropCreativeSort2);
		if (Properties.Values.ContainsKey(PropCraftingSkillExp) && !int.TryParse(Properties.Values[PropCraftingSkillExp], out CraftingSkillExp))
		{
			CraftingSkillExp = 10;
		}
		if (Properties.Values.ContainsKey(PropActionSkillExp) && !int.TryParse(Properties.Values[PropActionSkillExp], out ActionSkillExp))
		{
			ActionSkillExp = 10;
		}
		if (Properties.Values.ContainsKey(PropInsulation) && !StringParsers.TryParseFloat(Properties.Values[PropInsulation], out Insulation))
		{
			Insulation = 0f;
		}
		if (Properties.Values.ContainsKey(PropWaterproof) && !StringParsers.TryParseFloat(Properties.Values[PropWaterproof], out WaterProof))
		{
			WaterProof = 0f;
		}
		if (Properties.Values.ContainsKey(PropEncumbrance) && !StringParsers.TryParseFloat(Properties.Values[PropEncumbrance], out Encumbrance))
		{
			Encumbrance = 0f;
		}
		Properties.ParseString(PropSoundPickup, ref SoundPickup);
		Properties.ParseString(PropSoundPlace, ref SoundPlace);
		Properties.ParseString(PropSoundHolster, ref SoundHolster);
		Properties.ParseString(PropSoundUnholster, ref SoundUnholster);
		Properties.ParseString(PropSoundStick, ref SoundStick);
		Properties.ParseString(PropSoundTick, ref SoundTick);
		Properties.ParseBool(PropHasReloadAnim, ref HasReloadAnim);
		Properties.ParseFloat(PropSmell, ref Smell);
		if (SoundTick != null)
		{
			string[] array2 = SoundTick.Split(',');
			SoundTick = array2[0];
			if (array2.Length >= 2)
			{
				SoundTickDelay = StringParsers.ParseFloat(array2[1]);
			}
		}
		if (Properties.Values.ContainsKey(PropDescriptionKey))
		{
			DescriptionKey = Properties.Values[PropDescriptionKey];
		}
		else
		{
			DescriptionKey = base.Name + "Desc";
		}
		if (Properties.Values.ContainsKey(PropResourceUnit))
		{
			IsResourceUnit = StringParsers.ParseBool(Properties.Values[PropResourceUnit]);
		}
		if (Properties.Values.ContainsKey(PropMeltTimePerUnit))
		{
			MeltTimePerUnit = StringParsers.ParseFloat(Properties.Values[PropMeltTimePerUnit]);
		}
		if (Properties.Values.ContainsKey(PropActionSkillGroup))
		{
			ActionSkillGroup = Properties.Values[PropActionSkillGroup];
		}
		else
		{
			ActionSkillGroup = "";
		}
		if (Properties.Values.ContainsKey(PropCraftingSkillGroup))
		{
			CraftingSkillGroup = Properties.Values[PropCraftingSkillGroup];
		}
		else
		{
			CraftingSkillGroup = "";
		}
		if (Properties.Values.ContainsKey(PropCrosshairOnAim))
		{
			bShowCrosshairOnAiming = StringParsers.ParseBool(Properties.Values[PropCrosshairOnAim]);
		}
		if (Properties.Values.ContainsKey(PropCrosshairUpAfterShot))
		{
			bCrosshairUpAfterShot = StringParsers.ParseBool(Properties.Values[PropCrosshairUpAfterShot]);
		}
		else
		{
			bCrosshairUpAfterShot = true;
		}
		if (Properties.Values.ContainsKey(PropUsableUnderwater))
		{
			UsableUnderwater = StringParsers.ParseBool(Properties.Values[PropUsableUnderwater]);
		}
		if (Properties.Values.ContainsKey(PropItemTypeIcon))
		{
			ItemTypeIcon = Properties.Values[PropItemTypeIcon];
		}
		if (Properties.Values.ContainsKey(PropAltItemTypeIcon))
		{
			AltItemTypeIcon = Properties.Values[PropAltItemTypeIcon];
		}
		if (Properties.Values.ContainsKey(PropAltItemTypeIconColor))
		{
			AltItemTypeIconColor = StringParsers.ParseHexColor(Properties.Values[PropAltItemTypeIconColor]);
		}
		else
		{
			AltItemTypeIconColor = Color.white;
		}
		if (Properties.Values.ContainsKey(PropUnlocks))
		{
			Unlocks = Properties.Values[PropUnlocks];
		}
		if (Properties.Values.ContainsKey(PropNavObject))
		{
			NavObject = Properties.Values[PropNavObject];
		}
		if (Properties.Values.ContainsKey(PropQuestItem))
		{
			IsQuestItem = StringParsers.ParseBool(Properties.Values[PropQuestItem]);
		}
		if (Properties.Values.ContainsKey(PropShowQuality))
		{
			ShowQualityBar = StringParsers.ParseBool(Properties.Values[PropShowQuality]);
		}
		if (Properties.Values.ContainsKey(PropNoScrapping))
		{
			NoScrapping = StringParsers.ParseBool(Properties.Values[PropNoScrapping]);
		}
		if (Properties.Values.ContainsKey(PropScrapTimeOverride))
		{
			ScrapTimeOverride = StringParsers.ParseFloat(Properties.Values[PropScrapTimeOverride]);
		}
		if (Properties.Classes.ContainsKey("SDCS"))
		{
			SDCSData = new SDCSUtils.SlotData();
			if (Properties.Values.ContainsKey("SDCS.Prefab"))
			{
				SDCSData.PrefabName = Properties.Values["SDCS.Prefab"];
			}
			if (Properties.Values.ContainsKey("SDCS.TransformName"))
			{
				SDCSData.PartName = Properties.Values["SDCS.TransformName"];
			}
			if (Properties.Values.ContainsKey("SDCS.Excludes"))
			{
				SDCSData.BaseToTurnOff = Properties.Values["SDCS.Excludes"];
			}
			if (Properties.Values.ContainsKey("SDCS.HairMaskType"))
			{
				SDCSData.HairMaskType = (SDCSUtils.SlotData.HairMaskTypes)Enum.Parse(typeof(SDCSUtils.SlotData.HairMaskTypes), Properties.Values["SDCS.HairMaskType"]);
			}
			if (Properties.Values.ContainsKey("SDCS.FacialHairMaskType"))
			{
				SDCSData.FacialHairMaskType = (SDCSUtils.SlotData.HairMaskTypes)Enum.Parse(typeof(SDCSUtils.SlotData.HairMaskTypes), Properties.Values["SDCS.FacialHairMaskType"]);
			}
		}
		if (Properties.Values.ContainsKey(PropDisplayType))
		{
			DisplayType = Properties.Values[PropDisplayType];
		}
		else
		{
			DisplayType = "";
		}
		Properties.ParseString(PropTraderStageTemplate, ref TraderStageTemplate);
		Properties.ParseString(PropTrackerIndexName, ref TrackerIndexName);
		Properties.ParseString(PropTrackerNavObject, ref TrackerNavObject);
		Properties.ParseInt(PropMaxModsAllowed, ref MaxModsAllowed);
	}

	public void LateInit()
	{
		if (HasQuality)
		{
			Stacknumber.Value = 1;
		}
	}

	public static void InitStatic()
	{
		list = new ItemClass[MAX_ITEMS];
		itemActionNames = new string[5];
		for (int i = 0; i < 5; i++)
		{
			itemActionNames[i] = "Action" + i;
		}
	}

	public static void LateInitAll()
	{
		for (int i = 0; i < MAX_ITEMS; i++)
		{
			if (list[i] != null)
			{
				list[i].LateInit();
			}
		}
	}

	public static void Cleanup()
	{
		list = null;
		nameToItem.Clear();
		nameToItemCaseInsensitive.Clear();
		itemNames.Clear();
		itemActionNames = null;
	}

	public virtual int GetInitialMetadata(ItemValue _itemValue)
	{
		if (Actions[0] == null)
		{
			return 0;
		}
		return Actions[0].GetInitialMeta(_itemValue);
	}

	public static ItemClass GetForId(int _id)
	{
		if (list == null || (uint)_id >= list.Length)
		{
			return null;
		}
		return list[_id];
	}

	public static ItemValue GetItem(string _itemName, bool _caseInsensitive = false)
	{
		ItemClass itemClass = GetItemClass(_itemName, _caseInsensitive);
		if (itemClass != null)
		{
			return new ItemValue(itemClass.Id);
		}
		return ItemValue.None.Clone();
	}

	public static ItemClass GetItemClass(string _itemName, bool _caseInsensitive = false)
	{
		ItemClass value;
		if (_caseInsensitive)
		{
			nameToItemCaseInsensitive.TryGetValue(_itemName, out value);
		}
		else
		{
			nameToItem.TryGetValue(_itemName, out value);
		}
		return value;
	}

	public static void GetItemsAndBlocks(List<ItemClass> _targetList, int _idStart = -1, int _idEndExcl = -1, FilterItem[] _filterExprs = null, string _nameFilter = null, bool _bShowUserHidden = false, EnumCreativeMode _currentCreativeMode = EnumCreativeMode.Player, bool _showFavorites = false, bool _sortBySortOrder = false, XUi _xui = null)
	{
		_targetList.Clear();
		if (_idStart < 0)
		{
			_idStart = 0;
		}
		if (_idEndExcl < 0)
		{
			_idEndExcl = list.Length;
		}
		if (string.IsNullOrEmpty(_nameFilter))
		{
			_nameFilter = null;
		}
		int result = -1;
		if (_nameFilter != null)
		{
			int.TryParse(_nameFilter, out result);
		}
		for (int i = _idStart; i < _idEndExcl; i++)
		{
			Block block = null;
			if (i < Block.ItemsStartHere)
			{
				block = Block.list[i];
				if (block == null)
				{
					continue;
				}
			}
			ItemClass forId = GetForId(i);
			if (forId == null)
			{
				continue;
			}
			EnumCreativeMode creativeMode = forId.CreativeMode;
			if (creativeMode == EnumCreativeMode.None || creativeMode == EnumCreativeMode.Test || (creativeMode != EnumCreativeMode.All && _currentCreativeMode != creativeMode && !_bShowUserHidden) || (creativeMode == EnumCreativeMode.Console && !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) || (_showFavorites && _xui != null && !_xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)i)))
			{
				continue;
			}
			if (_filterExprs != null)
			{
				bool flag = false;
				for (int j = 0; j < _filterExprs.Length; j++)
				{
					if (_filterExprs[j] != null)
					{
						flag = _filterExprs[j](forId, block);
						if (flag)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (_nameFilter != null)
			{
				string a = forId.GetLocalizedItemName() ?? Localization.Get(forId.Name);
				if ((result < 0 || forId.Id != result) && !forId.Name.ContainsCaseInsensitive(_nameFilter) && !a.ContainsCaseInsensitive(_nameFilter))
				{
					continue;
				}
			}
			_targetList.Add(forId);
		}
		if (_sortBySortOrder)
		{
			_targetList.Sort([PublicizedFrom(EAccessModifier.Internal)] (ItemClass _icA, ItemClass _icB) =>
			{
				int num = string.CompareOrdinal(_icA.SortOrder, _icB.SortOrder);
				return (num != 0) ? num : _icA.Id.CompareTo(_icB.Id);
			});
		}
	}

	public static void CreateItemStacks(IEnumerable<ItemClass> _itemClassList, List<ItemStack> _targetList)
	{
		_targetList.Clear();
		foreach (ItemClass _itemClass in _itemClassList)
		{
			ItemValue itemValue = new ItemValue(_itemClass.Id, _bCreateDefaultParts: true);
			itemValue.Meta = itemValue.ItemClass.GetInitialMetadata(itemValue);
			ItemStack item = new ItemStack(itemValue, _itemClass.Stacknumber.Value);
			_targetList.Add(item);
		}
	}

	public virtual bool IsHUDDisabled(ItemInventoryData _data)
	{
		if (Actions[0] != null && Actions[0].IsHUDDisabled(_data?.actionData[0]))
		{
			return true;
		}
		if (Actions[1] != null && Actions[1].IsHUDDisabled(_data?.actionData[1]))
		{
			return true;
		}
		return false;
	}

	public virtual bool IsLightSource()
	{
		return LightSource != null;
	}

	public virtual Transform CloneModel(GameObject _reuseThisGO, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		return CloneModel(_world, _blockValue.ToItemValue(), _position, _parent, _purpose, _textureFullArray);
	}

	public virtual Transform CloneModel(World _world, ItemValue _itemValue, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		GameObject gameObject = null;
		if (CanHold())
		{
			string text = null;
			if (_purpose == BlockShape.MeshPurpose.Drop)
			{
				text = _itemValue.GetPropertyOverride("DropMeshFile", DropMeshFile);
			}
			if (_purpose == BlockShape.MeshPurpose.Hold)
			{
				text = _itemValue.GetPropertyOverride("HandMeshfile", HandMeshFile);
			}
			if (text == null)
			{
				text = _itemValue.GetPropertyOverride("Meshfile", MeshFile);
			}
			string text2 = ((text != null) ? GameIO.GetFilenameFromPathWithoutExtension(text) : null);
			if (renderGameObject == null || (text != null && !text2.Equals(renderGameObject.name)))
			{
				renderGameObject = DataLoader.LoadAsset<GameObject>(text);
			}
			gameObject = renderGameObject;
			if (gameObject == null)
			{
				gameObject = LoadManager.LoadAsset<GameObject>("@:Other/Items/Crafting/leather.fbx", null, null, false, true).Asset;
			}
		}
		if (gameObject == null)
		{
			return null;
		}
		try
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			Transform transform = gameObject2.transform;
			transform.SetParent(_parent, worldPositionStays: false);
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			gameObject2.SetActive(value: false);
			if (_purpose == BlockShape.MeshPurpose.Hold)
			{
				Collider[] componentsInChildren = gameObject2.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
				if (gameObject2 != null)
				{
					Animator[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Animator>();
					for (int i = 0; i < componentsInChildren2.Length; i++)
					{
						componentsInChildren2[i].writeDefaultValuesOnDisable = true;
					}
				}
			}
			UpdateLightOnAllMaterials updateLightOnAllMaterials = gameObject2.GetComponent<UpdateLightOnAllMaterials>();
			if (updateLightOnAllMaterials == null)
			{
				updateLightOnAllMaterials = gameObject2.AddComponent<UpdateLightOnAllMaterials>();
			}
			string optionalValue = "255,255,255";
			Properties.ParseString(Block.PropTintColor, ref optionalValue);
			Vector3 tintColorForItem = Block.StringToVector3(_itemValue.GetPropertyOverride(Block.PropTintColor, optionalValue));
			updateLightOnAllMaterials.SetTintColorForItem(tintColorForItem);
			return transform;
		}
		catch (Exception ex)
		{
			Log.Error("Instantiate of '" + MeshFile + "' led to error: " + ex.Message);
			Log.Error(ex.StackTrace);
		}
		return null;
	}

	public void setLocalizedItemName(string _localizedName)
	{
		localizedName = _localizedName;
	}

	public virtual string GetLocalizedItemName()
	{
		return localizedName;
	}

	public void SetName(string _name)
	{
		pName = _name;
	}

	public virtual string GetItemName()
	{
		return base.Name;
	}

	public virtual string GetItemDescriptionKey()
	{
		return DescriptionKey;
	}

	public virtual string GetIconName()
	{
		if (CustomIcon != null && CustomIcon.Value.Length > 0)
		{
			return CustomIcon.Value;
		}
		return base.Name;
	}

	public virtual Color GetIconTint(ItemValue _instance = null)
	{
		if (_instance != null)
		{
			string text = "NONE";
			string propertyOverride = _instance.GetPropertyOverride("CustomIconTint", text);
			if (!propertyOverride.Equals(text))
			{
				return StringParsers.ParseHexColor(propertyOverride);
			}
		}
		return CustomIconTint;
	}

	public virtual bool IsGun()
	{
		return Actions[0] is ItemActionAttack;
	}

	public virtual bool IsDynamicMelee()
	{
		return Actions[0] is ItemActionDynamic;
	}

	public virtual bool CanStack()
	{
		return Stacknumber.Value > 1;
	}

	public void SetCanHold(bool _b)
	{
		bCanHold = _b;
	}

	public virtual bool CanHold()
	{
		return bCanHold;
	}

	public void SetCanDrop(bool _b)
	{
		bCanDrop = _b;
	}

	public virtual bool CanDrop(ItemValue _iv = null)
	{
		return bCanDrop;
	}

	public virtual void Deactivate(ItemValue _iv)
	{
	}

	public virtual bool KeepOnDeath()
	{
		return false;
	}

	public virtual bool CanPlaceInContainer()
	{
		return true;
	}

	public virtual string CanInteract(ItemInventoryData _data)
	{
		return Actions[2]?.CanInteract(_data.actionData[2]);
	}

	public void Interact(ItemInventoryData _data)
	{
		ExecuteAction(2, _data, _bReleased: false, null);
		ExecuteAction(2, _data, _bReleased: true, null);
	}

	public bool CanExecuteAction(int actionIdx, EntityAlive holdingEntity, ItemValue itemValue)
	{
		if (Actions[actionIdx] == null || Actions[actionIdx].ExecutionRequirements == null)
		{
			return true;
		}
		holdingEntity.MinEventContext.ItemValue = itemValue;
		return Actions[actionIdx].ExecutionRequirements.IsValid(holdingEntity.MinEventContext);
	}

	public virtual void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
	{
		ItemAction curAction = Actions[_actionIdx];
		if (curAction == null)
		{
			return;
		}
		if (curAction is ItemActionDynamicMelee)
		{
			bool flag = _bReleased;
			if (Actions.Length >= 2 && Actions[0] != null && Actions[1] != null)
			{
				flag = !Actions[1].IsActionRunning(_data.actionData[1]) && !Actions[0].IsActionRunning(_data.actionData[0]);
			}
			if (!flag)
			{
				return;
			}
			if (!_bReleased)
			{
				flag &= curAction.CanExecute(_data.actionData[_actionIdx]);
			}
			if (flag)
			{
				if (_data != null && _data.holdingEntity.emodel != null && _data.holdingEntity.emodel.avatarController != null)
				{
					_data.holdingEntity.emodel.avatarController.UpdateInt(AvatarController.itemActionIndexHash, _actionIdx);
				}
				curAction.ExecuteAction((_data != null) ? _data.actionData[_actionIdx] : null, _bReleased);
			}
			return;
		}
		ItemActionData actionData = _data.actionData[_actionIdx];
		bool flag2 = _bReleased || !curAction.IsActionRunning(actionData);
		if (!flag2)
		{
			return;
		}
		if (!_bReleased)
		{
			flag2 &= curAction.CanExecute(actionData);
		}
		if (!flag2)
		{
			GameManager.ShowTooltip(_data.holdingEntity as EntityPlayerLocal, Localization.Get("ttCannotUseAtThisTime"), string.Empty, "ui_denied");
			return;
		}
		if (_data != null && _data.holdingEntity.emodel != null && _data.holdingEntity.emodel.avatarController != null)
		{
			_data.holdingEntity.emodel.avatarController.UpdateInt(AvatarController.itemActionIndexHash, _actionIdx);
		}
		if (!_bReleased)
		{
			if (!actionData.HasExecuted)
			{
				_data.holdingEntity.MinEventContext.ItemValue = _data.itemValue;
				_data.holdingEntity.FireEvent(MinEvent.Start[_actionIdx]);
			}
			if (curAction is ItemActionRanged && !(curAction is ItemActionLauncher) && !(curAction is ItemActionCatapult))
			{
				actionData.HasExecuted = true;
				curAction.ExecuteAction(actionData, _bReleased);
			}
			else
			{
				if (actionData.HasExecuted)
				{
					return;
				}
				if (curAction is ItemActionEat itemActionEat)
				{
					LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_data.holdingEntity as EntityPlayerLocal);
					XUi xui = ((uIForPlayer != null) ? uIForPlayer.xui : null);
					if (itemActionEat.UsePrompt && !xui.isUsingItemActionEntryPromptComplete)
					{
						XUiC_MessageBoxWindowGroup.ShowMessageBox(xui, Localization.Get(itemActionEat.PromptTitle), Localization.Get(itemActionEat.PromptDescription), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, [PublicizedFrom(EAccessModifier.Internal)] () =>
						{
							_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
							actionData.HasExecuted = false;
							curAction.ExecuteAction(actionData, _bReleased: true);
							xui.isUsingItemActionEntryPromptComplete = false;
						});
						return;
					}
					xui.isUsingItemActionEntryPromptComplete = false;
				}
				_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
				actionData.HasExecuted = true;
				curAction.ExecuteAction(actionData, _bReleased);
			}
		}
		else
		{
			if (!actionData.HasExecuted)
			{
				return;
			}
			if (!(curAction is ItemActionUseOther))
			{
				ItemValue itemValue = _data.itemValue;
				if (!curAction.IsEndDelayed() || !curAction.UseAnimation)
				{
					_data.holdingEntity.MinEventContext.ItemValue = itemValue;
					_data.holdingEntity.FireEvent(MinEvent.End[_actionIdx]);
				}
				if (_data.holdingEntity as EntityPlayerLocal != null)
				{
					ItemClass itemClass = itemValue.ItemClass;
					if (itemClass != null && itemClass.HasAnyTags(stopBleed) && _data.holdingEntity.Buffs.HasBuff("buffInjuryBleeding"))
					{
						PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.BleedOutStopped, 1);
					}
				}
			}
			if (curAction is ItemActionActivate && _bReleased && !(flag2 & curAction.CanExecute(actionData)))
			{
				GameManager.ShowTooltip(_data.holdingEntity as EntityPlayerLocal, Localization.Get("ttCannotUseAtThisTime"), string.Empty, "ui_denied");
				actionData.HasExecuted = true;
			}
			else
			{
				_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
				actionData.HasExecuted = false;
				curAction.ExecuteAction(actionData, _bReleased);
			}
		}
	}

	public virtual bool IsActionRunning(ItemInventoryData _data)
	{
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = Actions[i];
			if (itemAction != null && itemAction.IsActionRunning(_data.actionData[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnHoldingItemActivated(ItemInventoryData _data)
	{
	}

	public virtual void StartHolding(ItemInventoryData _data, Transform _modelTransform)
	{
		for (int i = 0; i < 3; i++)
		{
			Actions[i]?.StartHolding(_data.actionData[i]);
		}
		if (Actions[0] != null || Actions[1] != null)
		{
			_data.holdingEntitySoundID = -1;
		}
	}

	public virtual void CleanupHoldingActions(ItemInventoryData _data)
	{
		if (_data != null)
		{
			for (int i = 0; i < 3; i++)
			{
				Actions[i]?.Cleanup(_data.actionData[i]);
			}
		}
	}

	public virtual void OnHoldingUpdate(ItemInventoryData _data)
	{
		EntityAlive holdingEntity = _data.holdingEntity;
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = Actions[i];
			if (itemAction != null)
			{
				holdingEntity.MinEventContext.ItemValue = holdingEntity.inventory.holdingItemItemValue;
				holdingEntity.MinEventContext.ItemActionData = _data.actionData[i];
				holdingEntity.FireEvent(MinEvent.Update[i]);
				itemAction.OnHoldingUpdate(_data?.actionData[i]);
			}
		}
		if (Properties.Values.ContainsKey(PropSoundIdle) && !_data.holdingEntity.isEntityRemote)
		{
			if (_data.holdingEntitySoundID == 0 && _data.itemValue.Meta == 0)
			{
				Manager.BroadcastStop(_data.holdingEntity.entityId, Properties.Values[PropSoundIdle]);
			}
			else if (_data.holdingEntitySoundID == -1 && _data.itemValue.Meta > 0)
			{
				Manager.BroadcastPlay(_data.holdingEntity, Properties.Values[PropSoundIdle]);
				_data.holdingEntitySoundID = 0;
			}
		}
	}

	public virtual void OnHoldingReset(ItemInventoryData _data)
	{
	}

	public void StopHoldingAudio(ItemInventoryData _data)
	{
		if (Properties.Values[PropSoundIdle] != null && _data.holdingEntitySoundID == 0)
		{
			Manager.BroadcastStop(_data.holdingEntity.entityId, Properties.Values[PropSoundIdle]);
		}
		_data.holdingEntitySoundID = -2;
	}

	public virtual void StopHolding(ItemInventoryData _data, Transform _modelTransform)
	{
		StopHoldingAudio(_data);
		if (_data.holdingEntity is EntityPlayer && !_data.holdingEntity.isEntityRemote && _data.holdingEntity.AimingGun)
		{
			_data.holdingEntity.AimingGun = false;
		}
		for (int i = 0; i < 3; i++)
		{
			Actions[i]?.StopHolding(_data.actionData[i]);
		}
	}

	public virtual void OnMeshCreated(ItemWorldData _data)
	{
	}

	public virtual void OnDroppedUpdate(ItemWorldData _data)
	{
	}

	public virtual BlockValue OnConvertToBlockValue(ItemValue _itemValue, BlockValue _blueprintBlockValue)
	{
		return _blueprintBlockValue;
	}

	public ItemInventoryData CreateInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdxInInventory)
	{
		ItemInventoryData itemInventoryData = createItemInventoryData(_itemStack, _gameManager, _holdingEntity, _slotIdxInInventory);
		itemInventoryData.actionData[0] = ((Actions[0] != null) ? Actions[0].CreateModifierData(itemInventoryData, 0) : null);
		itemInventoryData.actionData[1] = ((Actions[1] != null) ? Actions[1].CreateModifierData(itemInventoryData, 1) : null);
		if (Actions[2] != null)
		{
			itemInventoryData.actionData.Add(Actions[2].CreateModifierData(itemInventoryData, 2));
		}
		return itemInventoryData;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemInventoryData createItemInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdxInInventory)
	{
		return new ItemInventoryData(this, _itemStack, _gameManager, _holdingEntity, _slotIdxInInventory);
	}

	public virtual ItemWorldData CreateWorldData(IGameManager _gm, EntityItem _entityItem, ItemValue _itemValue, int _belongsEntityId)
	{
		return new ItemWorldData(_gm, _itemValue, _entityItem, _belongsEntityId);
	}

	public virtual void OnHUD(ItemInventoryData _data, int _x, int _y)
	{
		if (Actions[0] != null)
		{
			Actions[0].OnHUD(_data.actionData[0], _x, _y);
		}
		if (Actions[1] != null)
		{
			Actions[1].OnHUD(_data.actionData[1], _x, _y);
		}
	}

	public virtual void OnScreenOverlay(ItemInventoryData _data)
	{
		if (Actions[0] != null)
		{
			Actions[0].OnScreenOverlay(_data.actionData[0]);
		}
		if (Actions[1] != null)
		{
			Actions[1].OnScreenOverlay(_data.actionData[1]);
		}
	}

	public virtual RenderCubeType GetFocusType(ItemInventoryData _data)
	{
		if (!CanHold())
		{
			return RenderCubeType.None;
		}
		RenderCubeType renderCubeType = RenderCubeType.None;
		if (Actions[0] != null)
		{
			renderCubeType = Actions[0].GetFocusType(_data?.actionData[0]);
		}
		RenderCubeType renderCubeType2 = RenderCubeType.None;
		if (Actions[1] != null)
		{
			renderCubeType2 = Actions[1].GetFocusType(_data?.actionData[1]);
		}
		if (renderCubeType <= renderCubeType2)
		{
			return renderCubeType2;
		}
		return renderCubeType;
	}

	public virtual float GetFocusRange()
	{
		if (Actions[0] != null && Actions[0] is ItemActionAttack)
		{
			return ((ItemActionAttack)Actions[0]).Range;
		}
		return 0f;
	}

	public virtual bool IsFocusBlockInside()
	{
		bool flag = false;
		if (Actions[0] != null)
		{
			flag = Actions[0].IsFocusBlockInside();
		}
		bool flag2 = false;
		if (Actions[1] != null)
		{
			flag2 = Actions[1].IsFocusBlockInside();
		}
		return flag2 && flag;
	}

	public virtual bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		bool flag = false;
		if (Actions[0] != null)
		{
			flag = Actions[0].ConsumeScrollWheel(_data.actionData[0], _scrollWheelInput, _playerInput);
		}
		if (!flag && Actions[1] != null)
		{
			flag = Actions[1].ConsumeScrollWheel(_data.actionData[1], _scrollWheelInput, _playerInput);
		}
		return flag;
	}

	public virtual bool ConsumeCameraFunction(ItemInventoryData _data)
	{
		bool flag = false;
		if (Actions[0] != null)
		{
			flag = Actions[0].ConsumeCameraFunction(_data.actionData[0]);
		}
		if (!flag && Actions[1] != null)
		{
			flag = Actions[1].ConsumeCameraFunction(_data.actionData[1]);
		}
		return flag;
	}

	public virtual void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo)
	{
	}

	public virtual float GetLifetimeOnDrop()
	{
		return 60f;
	}

	public virtual Block GetBlock()
	{
		return null;
	}

	public virtual bool IsBlock()
	{
		return false;
	}

	public virtual EnumCrosshairType GetCrosshairType(ItemInventoryData _holdingData)
	{
		EnumCrosshairType enumCrosshairType = EnumCrosshairType.Plus;
		EnumCrosshairType enumCrosshairType2 = EnumCrosshairType.Plus;
		if (Actions[0] != null)
		{
			enumCrosshairType = Actions[0].GetCrosshairType(_holdingData.actionData[0]);
		}
		if (Actions[1] != null)
		{
			enumCrosshairType2 = Actions[1].GetCrosshairType(_holdingData.actionData[1]);
		}
		EnumCrosshairType enumCrosshairType3 = ((enumCrosshairType > enumCrosshairType2) ? enumCrosshairType : enumCrosshairType2);
		string propertyOverride = _holdingData.itemValue.GetPropertyOverride(PropCrosshairOnAim, string.Empty);
		if (propertyOverride.Length > 0)
		{
			bShowCrosshairOnAiming = StringParsers.ParseBool(propertyOverride);
		}
		if (enumCrosshairType3 == EnumCrosshairType.Crosshair && bShowCrosshairOnAiming)
		{
			enumCrosshairType3 = EnumCrosshairType.CrosshairOnAiming;
		}
		return enumCrosshairType3;
	}

	public virtual void GetIronSights(ItemInventoryData _invData, out float _fov)
	{
		if (Actions[0] != null)
		{
			Actions[0].GetIronSights(_invData.actionData[0], out _fov);
			if (_fov != 0f)
			{
				return;
			}
		}
		if (Actions[1] != null)
		{
			Actions[1].GetIronSights(_invData.actionData[1], out _fov);
			if (_fov != 0f)
			{
				return;
			}
		}
		_fov = _invData.holdingEntity.GetCameraFOV();
	}

	public virtual EnumCameraShake GetCameraShakeType(ItemInventoryData _invData)
	{
		EnumCameraShake enumCameraShake = EnumCameraShake.None;
		EnumCameraShake enumCameraShake2 = EnumCameraShake.None;
		if (Actions[0] != null)
		{
			enumCameraShake = Actions[0].GetCameraShakeType(_invData.actionData[0]);
		}
		if (Actions[1] != null)
		{
			enumCameraShake2 = Actions[1].GetCameraShakeType(_invData.actionData[1]);
		}
		if (enumCameraShake > enumCameraShake2)
		{
			return enumCameraShake;
		}
		return enumCameraShake2;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		if (Actions[0] != null)
		{
			return Actions[0].GetControllerTriggerEffectPull();
		}
		if (Actions[1] != null)
		{
			return Actions[1].GetControllerTriggerEffectPull();
		}
		return TriggerEffectManager.NoneEffect;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		if (Actions[0] != null)
		{
			return Actions[0].GetControllerTriggerEffectShoot();
		}
		if (Actions[1] != null)
		{
			return Actions[1].GetControllerTriggerEffectShoot();
		}
		return TriggerEffectManager.NoneEffect;
	}

	public virtual bool IsActivated(ItemValue _value)
	{
		return _value.Activated != 0;
	}

	public virtual void SetActivated(ref ItemValue _value, bool _activated)
	{
		_value.Activated = (byte)(_activated ? 1u : 0u);
	}

	public virtual Vector3 GetDroppedCorrectionRotation()
	{
		return new Vector3(-90f, 0f, 0f);
	}

	public virtual Vector3 GetCorrectionRotation()
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetCorrectionPosition()
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetCorrectionScale()
	{
		return Vector3.zero;
	}

	public virtual void OnDamagedByExplosion(ItemWorldData _data)
	{
	}

	public static int GetFuelValue(ItemValue _itemValue)
	{
		ItemClass itemClass = list[_itemValue.type];
		if (itemClass == null)
		{
			return 0;
		}
		if (itemClass.IsBlock())
		{
			return Block.list[_itemValue.type].FuelValue;
		}
		if (itemClass.FuelValue == null)
		{
			return 0;
		}
		return itemClass.FuelValue.Value;
	}

	public int GetWeight()
	{
		if (IsBlock())
		{
			return GetBlock().GetWeight();
		}
		if (base.Weight != null)
		{
			return base.Weight.Value;
		}
		return 0;
	}

	public string GetImageEffect()
	{
		if (base.ImageEffectOnActive != null)
		{
			return base.ImageEffectOnActive.Value;
		}
		return "";
	}

	public bool GetActive()
	{
		if (base.Active != null)
		{
			return base.Active.Value;
		}
		return false;
	}

	public string GetSoundOnActive()
	{
		if (base.PlaySoundOnActive != null)
		{
			return base.PlaySoundOnActive.Value;
		}
		return "";
	}

	public void SetWeight(int _w)
	{
		if (IsBlock())
		{
			GetBlock().Weight = new DataItem<int>(_w);
		}
		else
		{
			base.Weight = new DataItem<int>(_w);
		}
	}

	public void SetImageEffect(string _str)
	{
		base.ImageEffectOnActive = new DataItem<string>(_str);
	}

	public void SetActive(bool _bOn)
	{
		base.Active = new DataItem<bool>(_bOn);
	}

	public void SetSoundOnActive(string _str)
	{
		base.PlaySoundOnActive = new DataItem<string>(_str);
	}

	public int AutoCalcWeight(Dictionary<string, List<Recipe>> _recipesByName)
	{
		Block block = (IsBlock() ? GetBlock() : null);
		if (block != null)
		{
			if (block.Weight != null)
			{
				if (block.Weight.Value != -1)
				{
					return block.Weight.Value;
				}
				return 0;
			}
			block.Weight = new DataItem<int>(-1);
		}
		else
		{
			if (base.Weight != null)
			{
				if (base.Weight.Value != -1)
				{
					return base.Weight.Value;
				}
				return 0;
			}
			base.Weight = new DataItem<int>(-1);
		}
		int num = 0;
		int num2 = 0;
		if (_recipesByName.TryGetValue(GetItemName(), out var value))
		{
			Recipe recipe = value[0];
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack itemStack = recipe.ingredients[i];
				ItemClass forId = GetForId(itemStack.itemValue.type);
				ItemClass forId2 = GetForId(recipe.itemValueType);
				if (recipe.materialBasedRecipe)
				{
					if (forId2.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory != null && forId2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forId.MadeOfMaterial.ForgeCategory))
					{
						num += forId.AutoCalcWeight(_recipesByName) * itemStack.count;
						num2++;
						break;
					}
				}
				else if (forId2.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory != null && forId2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forId.MadeOfMaterial.ForgeCategory))
				{
					if (GetForId(itemStack.itemValue.type).GetWeight() > 0)
					{
						num += GetForId(itemStack.itemValue.type).GetWeight() * itemStack.count;
						num2++;
					}
					else
					{
						num += forId.AutoCalcWeight(_recipesByName) * itemStack.count;
						num2++;
					}
				}
			}
			num /= ((num2 <= 1) ? 1 : recipe.count);
		}
		if (block != null)
		{
			block.Weight = new DataItem<int>(num);
		}
		else
		{
			base.Weight = new DataItem<int>(num);
		}
		return num;
	}

	public virtual bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return ItemTags.Test_AnySet(_tags);
	}

	public virtual bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return ItemTags.Test_AllSet(_tags);
	}

	public static ItemClass GetItemWithTag(FastTags<TagGroup.Global> _tags)
	{
		if (list != null)
		{
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i] != null && list[i].HasAllTags(_tags))
				{
					return list[i];
				}
			}
		}
		return null;
	}

	public static List<ItemClass> GetItemsWithTag(FastTags<TagGroup.Global> _tags)
	{
		List<ItemClass> list = new List<ItemClass>();
		if (ItemClass.list != null)
		{
			for (int i = 0; i < ItemClass.list.Length; i++)
			{
				if (ItemClass.list[i] != null && ItemClass.list[i].HasAllTags(_tags))
				{
					list.Add(ItemClass.list[i]);
				}
			}
		}
		return list;
	}

	public virtual bool CanCollect(ItemValue _iv)
	{
		return true;
	}

	public float AutoCalcEcoVal(Dictionary<string, List<Recipe>> _recipesByName, List<string> _recipeCalcStack)
	{
		string itemName = GetItemName();
		if (_recipeCalcStack.ContainsWithComparer(itemName, StringComparer.Ordinal))
		{
			return -1f;
		}
		Block block = (IsBlock() ? GetBlock() : null);
		float num = block?.EconomicValue ?? EconomicValue;
		if (num > 0f)
		{
			return num;
		}
		if ((double)num < -0.1)
		{
			return 0f;
		}
		_recipeCalcStack.Add(itemName);
		float num2 = 0f;
		int num3 = 0;
		if (_recipesByName.TryGetValue(itemName, out var value))
		{
			foreach (Recipe item in value)
			{
				for (int i = 0; i < item.ingredients.Count; i++)
				{
					ItemStack itemStack = item.ingredients[i];
					float num4 = GetForId(itemStack.itemValue.type).AutoCalcEcoVal(_recipesByName, _recipeCalcStack);
					if (num4 < 0f)
					{
						num2 = -1f;
						break;
					}
					num2 += (float)itemStack.count * num4;
					num3++;
				}
				if (!(num2 < 0f))
				{
					num2 /= (float)((num3 <= 1) ? 1 : item.count);
					break;
				}
			}
		}
		_recipeCalcStack.RemoveAt(_recipeCalcStack.Count - 1);
		if (num2 < 0f)
		{
			return -1f;
		}
		if (num2 == 0f)
		{
			num2 = 1f;
		}
		if (block != null)
		{
			block.EconomicValue = num2;
		}
		EconomicValue = num2;
		return num2;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (Effects != null)
		{
			Effects.FireEvent(_eventType, _eventParms);
		}
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		if (Effects == null)
		{
			return false;
		}
		return Effects.HasTrigger(_eventType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsFromXml()
	{
		Log.Out("ItemIDs from XML");
		foreach (KeyValuePair<string, ItemClass> item in nameToItem)
		{
			if (!item.Value.IsBlock())
			{
				assignId(item.Value, item.Value.Id, null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsLinear()
	{
		Log.Out("ItemIDs linear");
		bool[] usedIds = new bool[MAX_ITEMS];
		List<ItemClass> list = new List<ItemClass>(nameToItem.Count);
		nameToItem.CopyValuesTo(list);
		assignLeftOverItems(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignId(ItemClass _b, int _id, bool[] _usedIds)
	{
		list[_id] = _b;
		_b.SetId(_id);
		if (_usedIds != null)
		{
			_usedIds[_id] = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignLeftOverItems(bool[] _usedIds, List<ItemClass> _unassignedItems)
	{
		foreach (KeyValuePair<string, int> fixedItemId in fixedItemIds)
		{
			if (nameToItem.ContainsKey(fixedItemId.Key))
			{
				ItemClass itemClass = nameToItem[fixedItemId.Key];
				if (_unassignedItems.Contains(itemClass))
				{
					_unassignedItems.Remove(itemClass);
					assignId(itemClass, fixedItemId.Value + Block.ItemsStartHere, _usedIds);
				}
			}
		}
		int num = Block.ItemsStartHere;
		foreach (ItemClass _unassignedItem in _unassignedItems)
		{
			if (!_unassignedItem.IsBlock())
			{
				while (_usedIds[++num])
				{
				}
				assignId(_unassignedItem, num, _usedIds);
			}
		}
		Log.Out("ItemClass assignLeftOverItems {0} of {1}", num, MAX_ITEMS);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsFromMapping()
	{
		Log.Out("ItemIDs from Mapping");
		List<ItemClass> list = new List<ItemClass>();
		bool[] usedIds = new bool[MAX_ITEMS];
		foreach (KeyValuePair<string, ItemClass> item in nameToItem)
		{
			if (!item.Value.IsBlock())
			{
				int idForName = nameIdMapping.GetIdForName(item.Key);
				if (idForName >= 0)
				{
					assignId(item.Value, idForName, usedIds);
				}
				else
				{
					list.Add(item.Value);
				}
			}
		}
		assignLeftOverItems(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createFullMappingForClients()
	{
		NameIdMapping nameIdMapping = new NameIdMapping(null, MAX_ITEMS);
		foreach (KeyValuePair<string, ItemClass> item in nameToItem)
		{
			nameIdMapping.AddMapping(item.Value.Id, item.Key);
		}
		fullMappingDataForClients = nameIdMapping.SaveToArray();
	}

	public static void AssignIds()
	{
		if (nameIdMapping != null)
		{
			Log.Out("Item IDs with mapping");
			assignIdsFromMapping();
		}
		else
		{
			Log.Out("Item IDs withOUT mapping");
			assignIdsLinear();
		}
		createFullMappingForClients();
	}
}
