using System.Collections.Generic;
using UniLinq;

public class Archetype
{
	public static Dictionary<string, Archetype> s_Archetypes = new CaseInsensitiveStringDictionary<Archetype>();

	public string Name;

	public string Race;

	public int Variant;

	public string Hair = "";

	public string HairColor = "";

	public string MustacheName = "";

	public string ChopsName = "";

	public string BeardName = "";

	public string EyeColorName = "Blue01";

	public bool IsMale;

	public bool CanCustomize;

	public List<SDCSUtils.SlotData> Equipment;

	public string Sex
	{
		get
		{
			if (!IsMale)
			{
				return "Female";
			}
			return "Male";
		}
		set
		{
			IsMale = value.ToLower() == "male";
		}
	}

	public bool ShowInList
	{
		get
		{
			if (Name != "BaseMale")
			{
				return Name != "BaseFemale";
			}
			return false;
		}
	}

	public Archetype(string _name, bool _isMale, bool _canCustomize)
	{
		Name = _name;
		IsMale = _isMale;
		CanCustomize = _canCustomize;
	}

	public static void SetArchetype(Archetype archetype)
	{
		if (s_Archetypes.ContainsKey(archetype.Name))
		{
			s_Archetypes[archetype.Name] = archetype;
			return;
		}
		s_Archetypes[archetype.Name] = archetype;
		if (!archetype.CanCustomize)
		{
			ProfileSDF.SaveArchetype(archetype.Name, archetype.IsMale);
		}
	}

	public static Archetype GetArchetype(string name)
	{
		if (!s_Archetypes.ContainsKey(name))
		{
			return null;
		}
		return s_Archetypes[name];
	}

	public void AddEquipmentSlot(SDCSUtils.SlotData slotData)
	{
		if (Equipment == null)
		{
			Equipment = new List<SDCSUtils.SlotData>();
		}
		Equipment.Add(slotData);
	}

	public Archetype Clone()
	{
		return new Archetype(Name, IsMale, CanCustomize)
		{
			CanCustomize = CanCustomize,
			IsMale = IsMale,
			Race = Race,
			Variant = Variant,
			Hair = Hair,
			HairColor = HairColor,
			MustacheName = MustacheName,
			ChopsName = ChopsName,
			BeardName = BeardName,
			EyeColorName = EyeColorName
		};
	}

	public static void SaveArchetypesToFile()
	{
		SDCSArchetypesFromXml.Save("archetypes", s_Archetypes.Values.ToList());
	}
}
