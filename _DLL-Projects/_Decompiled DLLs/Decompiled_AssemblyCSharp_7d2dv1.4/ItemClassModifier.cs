using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassModifier : ItemClass
{
	public enum ModifierTypes
	{
		Mod,
		Attachment
	}

	public static ItemClassModifier[] modList = new ItemClassModifier[1000];

	public FastTags<TagGroup.Global> InstallableTags;

	public FastTags<TagGroup.Global> DisallowedTags;

	public ModifierTypes Type;

	public Dictionary<string, DynamicProperties> PropertyOverrides;

	public float CosmeticInstallChance;

	public static FastTags<TagGroup.Global> CosmeticModTypes = FastTags<TagGroup.Global>.Parse("dye,nametag,charm");

	public static FastTags<TagGroup.Global> CosmeticItemTags = FastTags<TagGroup.Global>.Parse("canHaveCosmetic");

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> modIds = new List<int>();

	public static ItemClassModifier GetItemModWithAnyTags(FastTags<TagGroup.Global> tags, FastTags<TagGroup.Global> installedModTypes, GameRandom random)
	{
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			if (ItemClass.list[i] is ItemClassModifier itemClassModifier && !itemClassModifier.HasAnyTags(installedModTypes) && itemClassModifier.InstallableTags.Test_AnySet(tags) && !itemClassModifier.DisallowedTags.Test_AnySet(tags))
			{
				modIds.Add(itemClassModifier.Id);
			}
		}
		if (modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier result = ItemClass.GetForId(modIds[random.RandomRange(modIds.Count)]) as ItemClassModifier;
		modIds.Clear();
		return result;
	}

	public static ItemClassModifier GetCosmeticItemMod(FastTags<TagGroup.Global> itemTags, FastTags<TagGroup.Global> installedModTypes, GameRandom random)
	{
		bool isEmpty = installedModTypes.IsEmpty;
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			if (ItemClass.list[i] is ItemClassModifier itemClassModifier && (isEmpty || !itemClassModifier.HasAnyTags(installedModTypes)) && itemClassModifier.HasAnyTags(CosmeticModTypes) && itemClassModifier.InstallableTags.Test_AnySet(itemTags) && !itemClassModifier.DisallowedTags.Test_AnySet(itemTags) && random.RandomFloat <= itemClassModifier.CosmeticInstallChance)
			{
				modIds.Add(itemClassModifier.Id);
			}
		}
		if (modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier result = ItemClass.GetForId(modIds[random.RandomRange(modIds.Count)]) as ItemClassModifier;
		modIds.Clear();
		return result;
	}

	public static ItemClassModifier GetDesiredItemModWithAnyTags(FastTags<TagGroup.Global> tags, FastTags<TagGroup.Global> installedModTypes, FastTags<TagGroup.Global> desiredModTypes, GameRandom random)
	{
		bool isEmpty = installedModTypes.IsEmpty;
		bool isEmpty2 = desiredModTypes.IsEmpty;
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			if (ItemClass.list[i] is ItemClassModifier itemClassModifier && (isEmpty || !itemClassModifier.HasAnyTags(installedModTypes)) && (isEmpty2 || itemClassModifier.HasAnyTags(desiredModTypes)) && itemClassModifier.InstallableTags.Test_AnySet(tags) && !itemClassModifier.DisallowedTags.Test_AnySet(tags))
			{
				modIds.Add(itemClassModifier.Id);
			}
		}
		if (modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier result = ItemClass.GetForId(modIds[random.RandomRange(modIds.Count)]) as ItemClassModifier;
		modIds.Clear();
		return result;
	}

	public bool GetPropertyOverride(string _propertyName, string _itemName, ref string _value)
	{
		if (PropertyOverrides.ContainsKey(_itemName) && PropertyOverrides[_itemName].Values.ContainsKey(_propertyName))
		{
			_value = PropertyOverrides[_itemName].Values[_propertyName];
			return true;
		}
		if (PropertyOverrides.ContainsKey("*") && PropertyOverrides["*"].Values.ContainsKey(_propertyName))
		{
			_value = PropertyOverrides["*"].Values[_propertyName];
			return true;
		}
		return false;
	}
}
