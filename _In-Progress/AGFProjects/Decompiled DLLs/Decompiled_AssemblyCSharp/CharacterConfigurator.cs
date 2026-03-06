using System;
using System.Collections.Generic;
using System.IO;
using Platform;
using UniLinq;
using UnityEngine;

public class CharacterConfigurator : MonoBehaviour
{
	public GameObject baseRig;

	[Header("Character Configuration")]
	public int selectedSexIndex;

	public int selectedRaceIndex;

	public int selectedVariantIndex;

	public int selectedHairIndex;

	public int selectedGearIndex = -1;

	public int selectedFacialHairIndex = -1;

	public int selectedHairColorIndex = 2;

	public int selectedEyeColorIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject characterInstance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog boneCatalog;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] sexes = new string[2] { "Male", "Female" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] races = new string[4] { "White", "Black", "Asian", "Native" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] variants = new int[4] { 1, 2, 3, 4 };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] hairColors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] eyeColors;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] hairs = new string[20]
	{
		"buzzcut", "comb_over", "slicked_back", "slicked_back_long", "pixie_cut", "ponytail", "midpart_karen_messy", "midpart_long", "midpart_mid", "midpart_short",
		"midpart_shoulder", "sidepart_short", "sidepart_mid", "sidepart_long", "cornrows", "mohawk", "flattop_fro", "small_fro", "dreads", "afro_curly"
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] facialHairIndexes = new int[5] { 1, 2, 3, 4, 5 };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] gear = new string[16]
	{
		"Assassin", "Biker", "Commando", "Enforcer", "Farmer", "Fiber", "Fitness", "LumberJack", "Miner", "Nerd",
		"Nomad", "Preacher", "Raider", "Ranger", "Scavenger", "Stealth"
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] dlcGear = new string[6] { "DarkKnight", "Desert", "Hoarder", "Marauder", "CrimsonWarlord", "Samurai" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] twitchDropGear = new string[3] { "Watcher", "PimpHatBlue", "PimpHatPurple" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allGearKeys = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> gearPathLookup = new Dictionary<string, string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> headgearPathLookup = new Dictionary<string, string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] headHiders = new string[4] { "Assassin", "Fiber", "Nomad", "Raider" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] hatHairGear = new string[15]
	{
		"Biker", "Commando", "Farmer", "Fitness", "LumberJack", "Miner", "Nerd", "Preacher", "Ranger", "Scavenger",
		"Stealth", "Desert", "Samurai", "PimpHatBlue", "PimpHatPurple"
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] noMorphs = new string[3] { "Hoarder", "Marauder", "Watcher" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] baseParts = new string[4] { "head", "body", "hands", "feet" };

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		LoadManager.Init();
		PlatformManager.Init();
		PrepareHairColorArray();
		PrepareEyeColorArray();
		PopulateGearLookups();
		CreateCharacter();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrepareHairColorArray()
	{
		string[] files = Directory.GetFiles("Assets/AssetBundles/Player/Common/HairColorSwatches", "*.asset");
		hairColors = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			hairColors[i] = Path.GetFileNameWithoutExtension(files[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrepareEyeColorArray()
	{
		string[] files = Directory.GetFiles("Assets/AssetBundles/Player/Common/Eyes/Materials", "*.mat");
		eyeColors = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			eyeColors[i] = Path.GetFileNameWithoutExtension(files[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateGearLookups()
	{
		allGearKeys.Clear();
		gearPathLookup.Clear();
		headgearPathLookup.Clear();
		for (int i = 0; i < gear.Length; i++)
		{
			allGearKeys.Add(gear[i]);
			gearPathLookup.Add(gear[i], "@:Entities/Player/{sex}/Gear/" + gear[i] + "/gear{sex}" + gear[i] + "Prefab.prefab");
			headgearPathLookup.Add(gear[i], "@:Entities/Player/{sex}/Gear/" + gear[i] + "/HeadgearMorphMatrix/{race}{variant}/gear" + gear[i] + "Head.asset");
		}
		for (int j = 0; j < dlcGear.Length; j++)
		{
			allGearKeys.Add(dlcGear[j]);
			gearPathLookup.Add(dlcGear[j], "@:DLC/" + dlcGear[j] + "Cosmetic/Entities/Player/{sex}/Gear/Prefabs/gear{sex}" + dlcGear[j] + "Prefab.prefab");
			headgearPathLookup.Add(dlcGear[j], "@:DLC/" + dlcGear[j] + "Cosmetic/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear" + dlcGear[j] + "{hair}Head.asset");
		}
		for (int k = 0; k < twitchDropGear.Length; k++)
		{
			allGearKeys.Add(twitchDropGear[k]);
			gearPathLookup.Add(twitchDropGear[k], "@:TwitchDrops/" + twitchDropGear[k] + "/Entities/Player/{sex}/Gear/Prefabs/gear{sex}" + twitchDropGear[k] + "Prefab.prefab");
			headgearPathLookup.Add(twitchDropGear[k], "@:TwitchDrops/" + twitchDropGear[k] + "/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear" + twitchDropGear[k] + "{hair}Head.asset");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateCharacter()
	{
		if (characterInstance != null)
		{
			UnityEngine.Object.DestroyImmediate(characterInstance);
		}
		characterInstance = UnityEngine.Object.Instantiate(baseRig, base.transform);
		boneCatalog = new SDCSUtils.TransformCatalog(characterInstance.transform);
		UpdateCharacter();
	}

	public void UpdateCharacter()
	{
		if (!(characterInstance == null))
		{
			CharacterConstructUtils.CreateViz(CreateCurrentArchetype(), ref characterInstance, ref boneCatalog);
			characterInstance.name = "Character";
			characterInstance.transform.localPosition = Vector3.zero;
			characterInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Archetype CreateCurrentArchetype()
	{
		Archetype archetype = new Archetype("", selectedSexIndex == 0, _canCustomize: true);
		archetype.Sex = sexes[selectedSexIndex];
		archetype.Race = races[selectedRaceIndex];
		archetype.Variant = variants[selectedVariantIndex];
		archetype.EyeColorName = eyeColors[selectedEyeColorIndex];
		if (selectedHairIndex >= 0 && selectedHairIndex < hairs.Length)
		{
			archetype.Hair = hairs[selectedHairIndex];
			archetype.HairColor = hairColors[selectedHairColorIndex];
		}
		else
		{
			archetype.Hair = "none";
		}
		if (selectedSexIndex == 0 && selectedFacialHairIndex >= 0 && selectedFacialHairIndex < facialHairIndexes.Length)
		{
			int num = facialHairIndexes[selectedFacialHairIndex];
			archetype.MustacheName = num.ToString();
			archetype.ChopsName = num.ToString();
			archetype.BeardName = num.ToString();
		}
		if (selectedGearIndex >= 0 && selectedGearIndex < allGearKeys.Count)
		{
			string text = allGearKeys[selectedGearIndex];
			archetype.Equipment = new List<SDCSUtils.SlotData>();
			for (int i = 0; i < baseParts.Length; i++)
			{
				SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
				slotData.PartName = baseParts[i];
				if (baseParts[i] != "head" || headHiders.Contains(text) || noMorphs.Contains(text))
				{
					slotData.PrefabName = gearPathLookup[text];
					slotData.BaseToTurnOff = ((baseParts[i] == "head" && !headHiders.Contains(text)) ? null : baseParts[i]);
				}
				else
				{
					slotData.PrefabName = headgearPathLookup[text];
					slotData.BaseToTurnOff = null;
				}
				if (slotData.PartName == "head")
				{
					slotData.HairMaskType = (hatHairGear.Contains(text) ? SDCSUtils.SlotData.HairMaskTypes.Hat : SDCSUtils.SlotData.HairMaskTypes.None);
				}
				if (slotData.BaseToTurnOff == "head")
				{
					archetype.Hair = "none";
					archetype.MustacheName = "-1";
					archetype.ChopsName = "-1";
					archetype.BeardName = "-1";
				}
				archetype.Equipment.Add(slotData);
			}
		}
		return archetype;
	}

	public string[] GetSexTypes()
	{
		return sexes;
	}

	public string[] GetRaceTypes()
	{
		return races;
	}

	public string[] GetVariantTypes()
	{
		string[] array = new string[variants.Length];
		for (int i = 0; i < variants.Length; i++)
		{
			array[i] = variants[i].ToString();
		}
		return array;
	}

	public string[] GetHairTypes()
	{
		string[] array = new string[hairs.Length + 1];
		array[0] = "None";
		for (int i = 0; i < hairs.Length; i++)
		{
			array[i + 1] = hairs[i];
		}
		return array;
	}

	public string[] GetHairColors()
	{
		PrepareHairColorArray();
		return hairColors;
	}

	public string[] GetEyeColors()
	{
		PrepareEyeColorArray();
		return eyeColors;
	}

	public string[] GetGearTypes()
	{
		PopulateGearLookups();
		string[] array = new string[allGearKeys.Count + 1];
		array[0] = "None";
		for (int i = 0; i < allGearKeys.Count; i++)
		{
			array[i + 1] = allGearKeys[i];
		}
		return array;
	}

	public string[] GetFacialHairTypes()
	{
		string[] array = new string[facialHairIndexes.Length + 1];
		array[0] = "None";
		for (int i = 0; i < facialHairIndexes.Length; i++)
		{
			array[i + 1] = "Style " + facialHairIndexes[i];
		}
		return array;
	}

	public void SetSex(int sexIndex)
	{
		if (sexIndex >= 0 && sexIndex < sexes.Length)
		{
			selectedSexIndex = sexIndex;
			if (sexIndex == 1)
			{
				selectedFacialHairIndex = -1;
			}
			UpdateCharacter();
		}
	}

	public void SetRace(int raceIndex)
	{
		if (raceIndex >= 0 && raceIndex < races.Length)
		{
			selectedRaceIndex = raceIndex;
			UpdateCharacter();
		}
	}

	public void SetVariant(int variantIndex)
	{
		if (variantIndex >= 0 && variantIndex < variants.Length)
		{
			selectedVariantIndex = variantIndex;
			UpdateCharacter();
		}
	}

	public void SetHair(int hairIndex)
	{
		selectedHairIndex = hairIndex - 1;
		UpdateCharacter();
	}

	public void SetHairColor(int colorIndex)
	{
		if (colorIndex >= 0 && colorIndex < hairColors.Length)
		{
			selectedHairColorIndex = colorIndex;
			UpdateCharacter();
		}
	}

	public void SetEyeColor(int colorIndex)
	{
		if (colorIndex >= 0 && colorIndex < eyeColors.Length)
		{
			selectedEyeColorIndex = colorIndex;
			UpdateCharacter();
		}
	}

	public void SetGear(int gearIndex)
	{
		selectedGearIndex = gearIndex - 1;
		UpdateCharacter();
	}

	public void SetFacialHair(int facialHairIndex)
	{
		if (selectedSexIndex == 0)
		{
			selectedFacialHairIndex = facialHairIndex - 1;
			UpdateCharacter();
		}
	}
}
