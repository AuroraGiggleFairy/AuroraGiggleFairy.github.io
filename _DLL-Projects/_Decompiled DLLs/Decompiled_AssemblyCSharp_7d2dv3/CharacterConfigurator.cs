using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class CharacterConfigurator : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SDCSGearXmlCatalog
	{
		public sealed class Entry
		{
			public string ItemName;

			public string GearKey;

			public string PartName;

			public string EquipSlot;

			public string PrefabName;

			public string BaseToTurnOff;

			public string HairMaskType;

			public string FacialHairMaskType;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class GearSet
		{
			public string Key;

			public readonly Dictionary<string, Entry> Parts = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, GearSet> sets = new Dictionary<string, GearSet>(StringComparer.OrdinalIgnoreCase);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<string> orderedKeys = new List<string>();

		public static SDCSGearXmlCatalog BuildFromXml(string xmlText)
		{
			SDCSGearXmlCatalog sDCSGearXmlCatalog = new SDCSGearXmlCatalog();
			if (string.IsNullOrEmpty(xmlText))
			{
				return sDCSGearXmlCatalog;
			}
			foreach (XElement item in XDocument.Parse(xmlText, LoadOptions.PreserveWhitespace).Descendants("item"))
			{
				XElement xElement = item.Elements("property").FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XElement p) => EqualsIgnoreCase(Attr(p, "class"), "SDCS"));
				if (xElement == null)
				{
					continue;
				}
				string text = DirectPropertyValue(xElement, "Prefab");
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				string equipSlot = DirectPropertyValue(item, "EquipSlot");
				string text2 = NormalizePart(DirectPropertyValue(xElement, "TransformName"), equipSlot);
				if (!string.IsNullOrEmpty(text2))
				{
					string itemName = Attr(item, "name");
					string text3 = NormalizeGearKey(DirectPropertyValue(item, "ArmorGroup"), DirectPropertyValue(item, "DisplayType"), itemName);
					if (!string.IsNullOrEmpty(text3))
					{
						string excludes = DirectPropertyValue(xElement, "Excludes");
						Entry entry = new Entry();
						entry.ItemName = itemName;
						entry.GearKey = text3;
						entry.PartName = text2;
						entry.EquipSlot = equipSlot;
						entry.PrefabName = text;
						entry.BaseToTurnOff = NormalizeBaseToTurnOff(excludes, text2);
						entry.HairMaskType = NormalizeOptional(DirectPropertyValue(xElement, "HairMaskType"));
						entry.FacialHairMaskType = NormalizeOptional(DirectPropertyValue(xElement, "FacialHairMaskType"));
						sDCSGearXmlCatalog.Add(entry);
					}
				}
			}
			return sDCSGearXmlCatalog;
		}

		public List<string> GetOrderedKeys()
		{
			return new List<string>(orderedKeys);
		}

		public bool TryGetPart(string gearKey, string partName, out Entry entry)
		{
			entry = null;
			if (string.IsNullOrEmpty(gearKey) || string.IsNullOrEmpty(partName) || !sets.TryGetValue(gearKey, out var value))
			{
				return false;
			}
			return value.Parts.TryGetValue(partName, out entry);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Add(Entry entry)
		{
			if (entry != null && !string.IsNullOrEmpty(entry.GearKey) && !string.IsNullOrEmpty(entry.PartName))
			{
				if (!sets.TryGetValue(entry.GearKey, out var value))
				{
					value = new GearSet();
					value.Key = entry.GearKey;
					sets.Add(entry.GearKey, value);
					orderedKeys.Add(entry.GearKey);
				}
				value.Parts[entry.PartName] = entry;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string Attr(XElement e, string name)
		{
			return (e?.Attribute(name))?.Value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string DirectPropertyValue(XElement parent, string name)
		{
			if (parent == null)
			{
				return null;
			}
			return (parent.Elements("property").FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XElement p) => EqualsIgnoreCase(Attr(p, "name"), name))?.Attribute("value"))?.Value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeGearKey(string armorGroup, string displayType, string itemName)
		{
			if (!string.IsNullOrEmpty(armorGroup))
			{
				return StripPrefix(armorGroup.Trim(), "group");
			}
			return NormalizeOptional(StripSuffix(StripSuffix(StripSuffix(StripSuffix(StripPrefix(((!string.IsNullOrEmpty(displayType)) ? displayType : itemName) ?? string.Empty, "armor"), "Helmet"), "Outfit"), "Gloves"), "Boots"));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizePart(string transformName, string equipSlot)
		{
			string text = NormalizeOptional(transformName);
			if (!string.IsNullOrEmpty(text))
			{
				text = text.ToLowerInvariant();
				switch (text)
				{
				case "head":
				case "body":
				case "hands":
				case "feet":
					return text;
				}
			}
			string text2 = NormalizeOptional(equipSlot);
			if (string.IsNullOrEmpty(text2))
			{
				return null;
			}
			return text2.ToLowerInvariant() switch
			{
				"head" => "head", 
				"chest" => "body", 
				"hands" => "hands", 
				"feet" => "feet", 
				_ => null, 
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeBaseToTurnOff(string excludes, string preferredPart)
		{
			excludes = NormalizeOptional(excludes);
			if (string.IsNullOrEmpty(excludes))
			{
				return null;
			}
			string[] array = excludes.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text = NormalizePartName(array[i]);
				if (EqualsIgnoreCase(text, preferredPart))
				{
					return text;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				string text2 = NormalizePartName(array[j]);
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizePartName(string value)
		{
			value = NormalizeOptional(value);
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			value = value.ToLowerInvariant();
			switch (value)
			{
			case "head":
			case "body":
			case "hands":
			case "feet":
				return value;
			default:
				return null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeOptional(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}
			value = value.Trim();
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string StripPrefix(string value, string prefix)
		{
			if (!string.IsNullOrEmpty(value) && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return value.Substring(prefix.Length);
			}
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string StripSuffix(string value, string suffix)
		{
			if (!string.IsNullOrEmpty(value) && value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				return value.Substring(0, value.Length - suffix.Length);
			}
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool EqualsIgnoreCase(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}

	public GameObject baseRig;

	[Header("Character Configuration")]
	public int selectedSexIndex;

	public int selectedRaceIndex;

	public int selectedVariantIndex;

	public int selectedHairIndex;

	public int selectedGearIndex = -1;

	public int selectedHeadGearIndex = -1;

	public int selectedBodyGearIndex = -1;

	public int selectedHandsGearIndex = -1;

	public int selectedFeetGearIndex = -1;

	public int selectedFacialHairIndex = -1;

	public int selectedHairColorIndex = 2;

	public int selectedEyeColorIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject characterInstance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog boneCatalog;

	public bool isJiggling;

	public float rotationValue = 180f;

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
	public List<string> allGearKeys = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSGearXmlCatalog gearCatalog;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] baseParts = new string[4] { "head", "body", "hands", "feet" };

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
	public string GetItemsXmlPath()
	{
		return GameIO.GetGameDir("Data/Config") + "/items.xml";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnsureGearCatalogLoaded()
	{
		if (gearCatalog == null)
		{
			PopulateGearLookups();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateGearLookups()
	{
		allGearKeys.Clear();
		gearCatalog = new SDCSGearXmlCatalog();
		string itemsXmlPath = GetItemsXmlPath();
		if (!File.Exists(itemsXmlPath))
		{
			Debug.LogWarning("CharacterConfigurator could not find items.xml at: " + itemsXmlPath);
			return;
		}
		try
		{
			string xmlText = File.ReadAllText(itemsXmlPath);
			gearCatalog = SDCSGearXmlCatalog.BuildFromXml(xmlText);
			allGearKeys = gearCatalog.GetOrderedKeys();
		}
		catch (Exception ex)
		{
			Debug.LogError("CharacterConfigurator failed to read SDCS gear data from items.xml: " + ex);
			gearCatalog = new SDCSGearXmlCatalog();
			allGearKeys.Clear();
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
			EnsureGearCatalogLoaded();
			CharacterConstructUtils.CreateViz(CreateCurrentArchetype(), ref characterInstance, ref boneCatalog);
			characterInstance.name = "Character";
			characterInstance.transform.localPosition = Vector3.zero;
			characterInstance.transform.localRotation = Quaternion.Euler(0f, rotationValue, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Archetype CreateCurrentArchetype()
	{
		EnsureGearCatalogLoaded();
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
			archetype.Hair = "";
		}
		if (selectedSexIndex == 0 && selectedFacialHairIndex >= 0 && selectedFacialHairIndex < facialHairIndexes.Length)
		{
			int num = facialHairIndexes[selectedFacialHairIndex];
			archetype.MustacheName = num.ToString();
			archetype.ChopsName = num.ToString();
			archetype.BeardName = num.ToString();
		}
		if (IsValidGearIndex(selectedFeetGearIndex) || IsValidGearIndex(selectedHandsGearIndex) || IsValidGearIndex(selectedBodyGearIndex) || IsValidGearIndex(selectedHeadGearIndex))
		{
			archetype.Equipment = new List<SDCSUtils.SlotData>();
			string[] array = new string[4]
			{
				IsValidGearIndex(selectedHeadGearIndex) ? allGearKeys[selectedHeadGearIndex] : string.Empty,
				IsValidGearIndex(selectedBodyGearIndex) ? allGearKeys[selectedBodyGearIndex] : string.Empty,
				IsValidGearIndex(selectedHandsGearIndex) ? allGearKeys[selectedHandsGearIndex] : string.Empty,
				IsValidGearIndex(selectedFeetGearIndex) ? allGearKeys[selectedFeetGearIndex] : string.Empty
			};
			for (int i = 0; i < baseParts.Length; i++)
			{
				string text = array[i];
				string partName = baseParts[i];
				if (string.IsNullOrEmpty(text) || !gearCatalog.TryGetPart(text, partName, out var entry))
				{
					continue;
				}
				SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
				slotData.PartName = entry.PartName;
				slotData.PrefabName = entry.PrefabName;
				slotData.BaseToTurnOff = entry.BaseToTurnOff;
				slotData.HairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
				slotData.FacialHairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
				if (slotData.PartName == "head")
				{
					slotData.HairMaskType = ParseHairMaskType(entry.HairMaskType);
					slotData.FacialHairMaskType = ParseHairMaskType(entry.FacialHairMaskType);
					if (slotData.HairMaskType == SDCSUtils.SlotData.HairMaskTypes.None)
					{
						archetype.Hair = "";
					}
					if (slotData.FacialHairMaskType == SDCSUtils.SlotData.HairMaskTypes.None)
					{
						archetype.MustacheName = "";
						archetype.ChopsName = "";
						archetype.BeardName = "";
					}
				}
				if (slotData.BaseToTurnOff == "head")
				{
					archetype.Hair = "";
					archetype.MustacheName = "";
					archetype.ChopsName = "";
					archetype.BeardName = "";
				}
				archetype.Equipment.Add(slotData);
			}
		}
		return archetype;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidGearIndex(int gearIndex)
	{
		if (gearIndex >= 0)
		{
			return gearIndex < allGearKeys.Count;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.SlotData.HairMaskTypes ParseHairMaskType(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return SDCSUtils.SlotData.HairMaskTypes.Full;
		}
		if (Enum.TryParse<SDCSUtils.SlotData.HairMaskTypes>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		Debug.LogWarning("Unknown SDCS HairMaskType '" + value + "'. Falling back to None.");
		return SDCSUtils.SlotData.HairMaskTypes.None;
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
		EnsureGearCatalogLoaded();
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
		selectedFeetGearIndex = (selectedHandsGearIndex = (selectedBodyGearIndex = (selectedHeadGearIndex = selectedGearIndex)));
		UpdateCharacter();
	}

	public void SetFeetGear(int gearIndex)
	{
		selectedFeetGearIndex = gearIndex - 1;
		UpdateCharacter();
	}

	public void SetHandsGear(int gearIndex)
	{
		selectedHandsGearIndex = gearIndex - 1;
		UpdateCharacter();
	}

	public void SetBodyGear(int gearIndex)
	{
		selectedBodyGearIndex = gearIndex - 1;
		UpdateCharacter();
	}

	public void SetHeadGear(int gearIndex)
	{
		selectedHeadGearIndex = gearIndex - 1;
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

	public void ToggleJiggle()
	{
		isJiggling = !isJiggling;
		if (isJiggling)
		{
			StartCoroutine(JiggleCharacter());
		}
	}

	public IEnumerator JiggleCharacter()
	{
		float angle = 20f;
		float speed = 6f;
		while (isJiggling)
		{
			float num = angle * Mathf.SmoothStep(-1f, 1f, Mathf.Sin(Time.time * speed) * 0.5f + 0.5f);
			characterInstance.transform.localRotation = Quaternion.Euler(0f, rotationValue + num, 0f);
			yield return null;
		}
		characterInstance.transform.localRotation = Quaternion.Euler(0f, rotationValue, 0f);
	}

	public bool IsJiggleEnabled()
	{
		return isJiggling;
	}

	public void RotateCharacter(float value)
	{
		rotationValue = 0f - value;
		characterInstance.transform.localRotation = Quaternion.Euler(0f, rotationValue, 0f);
	}
}
