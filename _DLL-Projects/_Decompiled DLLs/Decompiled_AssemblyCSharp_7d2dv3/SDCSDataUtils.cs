using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

public static class SDCSDataUtils
{
	public struct HairColorData
	{
		public int Index;

		public string Name;

		public string PrefabName;
	}

	public struct HairData
	{
		public string Name;

		public bool IsMale;
	}

	public struct GenderKey(string name, bool isMale)
	{
		public string Name = name;

		public bool IsMale = isMale;
	}

	public enum HairTypes
	{
		Hair,
		Mustache,
		Chops,
		Beard
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<GenderKey, List<int>> VariantData = new Dictionary<GenderKey, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<GenderKey, HairData> HairDictionary = new Dictionary<GenderKey, HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<GenderKey, HairData> MustacheDictionary = new Dictionary<GenderKey, HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<GenderKey, HairData> ChopsDictionary = new Dictionary<GenderKey, HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<GenderKey, HairData> BeardDictionary = new Dictionary<GenderKey, HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> EyeColorList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, HairColorData> HairColorDictionary = new Dictionary<string, HairColorData>();

	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "AssetBundles/Player/Common/HairColorSwatches";
		}
	}

	public static void SetupData()
	{
		Load();
	}

	public static List<string> GetRaceList(bool isMale)
	{
		List<string> list = new List<string>();
		foreach (GenderKey key in VariantData.Keys)
		{
			if (key.IsMale == isMale)
			{
				list.Add(key.Name);
			}
		}
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (string a, string b) => b.CompareTo(a));
		return list;
	}

	public static List<string> GetVariantList(bool isMale, string raceName)
	{
		List<string> list = new List<string>();
		foreach (GenderKey key in VariantData.Keys)
		{
			if (key.IsMale == isMale && key.Name.EqualsCaseInsensitive(raceName))
			{
				for (int i = 0; i < VariantData[key].Count; i++)
				{
					list.Add(VariantData[key][i].ToString());
				}
			}
		}
		return list;
	}

	public static List<string> GetHairNames(bool isMale, HairTypes hairType)
	{
		List<string> list = new List<string>();
		Dictionary<GenderKey, HairData> dictionary = null;
		switch (hairType)
		{
		case HairTypes.Hair:
			dictionary = HairDictionary;
			break;
		case HairTypes.Mustache:
			dictionary = MustacheDictionary;
			break;
		case HairTypes.Chops:
			dictionary = ChopsDictionary;
			break;
		case HairTypes.Beard:
			dictionary = BeardDictionary;
			break;
		}
		foreach (GenderKey key in dictionary.Keys)
		{
			_ = dictionary[key];
			if (key.IsMale == isMale)
			{
				list.Add(key.Name);
			}
		}
		return list;
	}

	public static List<string> GetEyeColorNames()
	{
		return EyeColorList;
	}

	public static List<HairColorData> GetHairColorNames()
	{
		List<HairColorData> list = new List<HairColorData>();
		foreach (HairColorData value in HairColorDictionary.Values)
		{
			list.Add(value);
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupDataFromResources()
	{
		LoadRaceDataFromResources();
		EyeColorList = GetEyeColorNamesFromResources();
		LoadHairTypeFromResources(HairDictionary, HairTypes.Hair);
		LoadHairTypeFromResources(MustacheDictionary, HairTypes.Mustache);
		LoadHairTypeFromResources(ChopsDictionary, HairTypes.Chops);
		LoadHairTypeFromResources(BeardDictionary, HairTypes.Beard);
		LoadHairColorFromResources(HairColorDictionary);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadRaceDataFromResources()
	{
		VariantData.Clear();
		ParseRaceVariantFromResources(isMale: true);
		ParseRaceVariantFromResources(isMale: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRaceVariantFromResources(bool isMale)
	{
		string text = (isMale ? "Male" : "Female");
		DirectoryInfo[] directories = new DirectoryInfo(Application.dataPath + "/AssetBundles/Player/" + text + "/Heads/").GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			GenderKey key = new GenderKey(directoryInfo.Name, isMale);
			if (!VariantData.ContainsKey(key))
			{
				VariantData.Add(key, new List<int>());
			}
			DirectoryInfo[] directories2 = new DirectoryInfo(directoryInfo.FullName).GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories2)
			{
				VariantData[key].Add(StringParsers.ParseSInt32(directoryInfo2.Name));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadHairTypeFromResources(Dictionary<GenderKey, HairData> dict, HairTypes hairType)
	{
		dict.Clear();
		List<string> hairNamesFromResources = GetHairNamesFromResources(isMale: true, hairType);
		for (int i = 0; i < hairNamesFromResources.Count; i++)
		{
			dict.Add(new GenderKey(hairNamesFromResources[i], isMale: true), new HairData
			{
				Name = hairNamesFromResources[i],
				IsMale = true
			});
		}
		hairNamesFromResources = GetHairNamesFromResources(isMale: false, hairType);
		for (int j = 0; j < hairNamesFromResources.Count; j++)
		{
			dict.Add(new GenderKey(hairNamesFromResources[j], isMale: false), new HairData
			{
				Name = hairNamesFromResources[j],
				IsMale = false
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadHairColorFromResources(Dictionary<string, HairColorData> dict)
	{
		dict.Clear();
		List<string> hairColorNamesFromResources = GetHairColorNamesFromResources();
		for (int i = 0; i < hairColorNamesFromResources.Count; i++)
		{
			string text = hairColorNamesFromResources[i];
			int index = StringParsers.ParseSInt32(text.Substring(0, 2));
			string name = text.Substring(3);
			dict.Add(hairColorNamesFromResources[i], new HairColorData
			{
				Index = index,
				Name = name,
				PrefabName = text
			});
		}
	}

	public static List<string> GetHairNamesFromResources(bool isMale, HairTypes hairType)
	{
		List<string> list = new List<string>();
		string text = (isMale ? "Male" : "Female");
		string text2 = Application.dataPath + "/AssetBundles/Player/" + text + "/";
		string path = ((hairType == HairTypes.Hair) ? (text2 + "Hair/") : $"{text2}FacialHair/{hairType}/");
		if (Directory.Exists(path))
		{
			DirectoryInfo[] directories = new DirectoryInfo(path).GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				list.Add(directoryInfo.Name);
			}
		}
		return list;
	}

	public static List<string> GetEyeColorNamesFromResources()
	{
		List<string> list = new List<string>();
		FileInfo[] files = new DirectoryInfo(Application.dataPath + "/AssetBundles/Player/Common/Eyes/Materials/").GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (!fileInfo.Name.EndsWith(".meta"))
			{
				list.Add(fileInfo.Name.Replace(".mat", ""));
			}
		}
		return list;
	}

	public static List<string> GetHairColorNamesFromResources()
	{
		List<string> list = new List<string>();
		FileInfo[] files = new DirectoryInfo(Application.dataPath + "/" + baseHairColorLoc + "/").GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (!fileInfo.Name.EndsWith(".meta"))
			{
				list.Add(fileInfo.Name.Replace(".asset", ""));
			}
		}
		return list;
	}

	public static void Save()
	{
		SetupDataFromResources();
		StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/Resources/sdcs.xml");
		string text = "\t";
		streamWriter.WriteLine("<sdcs>");
		foreach (GenderKey key in VariantData.Keys)
		{
			if (VariantData[key].Count > 0)
			{
				for (int i = 0; i < VariantData[key].Count; i++)
				{
					streamWriter.WriteLine($"{text}<variant race=\"{key.Name}\" index=\"{VariantData[key][i]}\" is_male=\"{key.IsMale}\" />");
				}
			}
		}
		for (int j = 0; j < EyeColorList.Count; j++)
		{
			streamWriter.WriteLine($"{text}<eye_color name=\"{EyeColorList[j]}\" />");
		}
		foreach (HairColorData value in HairColorDictionary.Values)
		{
			streamWriter.WriteLine($"{text}<hair_color index=\"{value.Index}\" name=\"{value.Name}\" prefab_name=\"{value.PrefabName}\" />");
		}
		foreach (HairData value2 in HairDictionary.Values)
		{
			streamWriter.WriteLine($"{text}<hair name=\"{value2.Name}\" is_male=\"{value2.IsMale}\" />");
		}
		foreach (HairData value3 in MustacheDictionary.Values)
		{
			streamWriter.WriteLine($"{text}<mustache name=\"{value3.Name}\" is_male=\"{value3.IsMale}\" />");
		}
		foreach (HairData value4 in ChopsDictionary.Values)
		{
			streamWriter.WriteLine($"{text}<chops name=\"{value4.Name}\" is_male=\"{value4.IsMale}\" />");
		}
		foreach (HairData value5 in BeardDictionary.Values)
		{
			streamWriter.WriteLine($"{text}<beard name=\"{value5.Name}\" is_male=\"{value5.IsMale}\" />");
		}
		streamWriter.WriteLine("</sdcs>");
		streamWriter.Flush();
		streamWriter.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Load()
	{
		XElement root = XDocument.Parse(((TextAsset)Resources.Load("sdcs")).text, LoadOptions.SetLineInfo).Root;
		if (root == null || !root.HasElements)
		{
			return;
		}
		VariantData.Clear();
		EyeColorList.Clear();
		HairDictionary.Clear();
		HairColorDictionary.Clear();
		MustacheDictionary.Clear();
		ChopsDictionary.Clear();
		BeardDictionary.Clear();
		foreach (XElement item2 in root.Elements())
		{
			if (item2.Name == "variant")
			{
				int item = -1;
				string name = "";
				bool isMale = false;
				if (item2.HasAttribute("index"))
				{
					item = StringParsers.ParseSInt32(item2.GetAttribute("index"));
				}
				if (item2.HasAttribute("race"))
				{
					name = item2.GetAttribute("race");
				}
				if (item2.HasAttribute("is_male"))
				{
					isMale = StringParsers.ParseBool(item2.GetAttribute("is_male"));
				}
				GenderKey key = new GenderKey(name, isMale);
				if (!VariantData.ContainsKey(key))
				{
					VariantData.Add(key, new List<int>());
				}
				VariantData[key].Add(item);
			}
			else if (item2.Name == "eye_color")
			{
				EyeColorList.Add(item2.GetAttribute("name"));
			}
			else if (item2.Name == "hair_color")
			{
				int index = -1;
				string text = "";
				string prefabName = "";
				if (item2.HasAttribute("index"))
				{
					index = StringParsers.ParseSInt32(item2.GetAttribute("index"));
				}
				if (item2.HasAttribute("name"))
				{
					text = item2.GetAttribute("name");
				}
				if (item2.HasAttribute("prefab_name"))
				{
					prefabName = item2.GetAttribute("prefab_name");
				}
				HairColorDictionary.Add(text, new HairColorData
				{
					Index = index,
					Name = text,
					PrefabName = prefabName
				});
			}
			else if (item2.Name == "hair")
			{
				HairData value = ParseHair(item2);
				HairDictionary.Add(new GenderKey(value.Name, value.IsMale), value);
			}
			else if (item2.Name == "mustache")
			{
				HairData value2 = ParseHair(item2);
				MustacheDictionary.Add(new GenderKey(value2.Name, value2.IsMale), value2);
			}
			else if (item2.Name == "chops")
			{
				HairData value3 = ParseHair(item2);
				ChopsDictionary.Add(new GenderKey(value3.Name, value3.IsMale), value3);
			}
			else if (item2.Name == "beard")
			{
				HairData value4 = ParseHair(item2);
				BeardDictionary.Add(new GenderKey(value4.Name, value4.IsMale), value4);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HairData ParseHair(XElement element)
	{
		string name = "";
		bool isMale = false;
		if (element.HasAttribute("name"))
		{
			name = element.GetAttribute("name");
		}
		if (element.HasAttribute("is_male"))
		{
			isMale = StringParsers.ParseBool(element.GetAttribute("is_male"));
		}
		return new HairData
		{
			Name = name,
			IsMale = isMale
		};
	}
}
