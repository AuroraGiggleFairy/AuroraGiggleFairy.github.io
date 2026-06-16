using System;
using System.Collections.Generic;
using Platform;
using UniLinq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CharacterConstruct : MonoBehaviour
{
	public GameObject baseRig;

	public int hairColorIndex;

	public int hatHairGearIndex;

	public int selectedRaceIndex;

	public int selectedVariantIndex;

	public float labelHeight = 2f;

	public float labelScale = 0.1f;

	public Color labelColor = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showCharacters = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showGear;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showHair;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showHatHair;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFacialHair;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject selectedCharacter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool allCharactersVisible = true;

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
	public string[] headHiders = new string[5] { "Assassin", "Fiber", "Nomad", "Raider", "Watcher" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] noMorphs = new string[2] { "Hoarder", "Marauder" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] baseParts = new string[4] { "head", "body", "hands", "feet" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject characterRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject gearRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject hairRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject hatHairRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject facialHairRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> allCharacters = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> allGear = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> allHair = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> allHatHair = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> allFacialHair = new List<GameObject>();

	public bool ShowCharacters
	{
		get
		{
			return showCharacters;
		}
		set
		{
			showCharacters = value;
			ShowAllCharacters();
		}
	}

	public bool ShowGear
	{
		get
		{
			return showGear;
		}
		set
		{
			showGear = value;
			ShowAllCharacters();
		}
	}

	public bool ShowHair
	{
		get
		{
			return showHair;
		}
		set
		{
			showHair = value;
			ShowAllCharacters();
		}
	}

	public bool ShowHatHair
	{
		get
		{
			return showHatHair;
		}
		set
		{
			showHatHair = value;
			ShowAllCharacters();
		}
	}

	public bool ShowFacialHair
	{
		get
		{
			return showFacialHair;
		}
		set
		{
			showFacialHair = value;
			ShowAllCharacters();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string EntityClassName(string sex)
	{
		return $"player{sex}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		LoadManager.Init();
		PlatformManager.Init();
		characterRoot = new GameObject("CharacterRoot");
		characterRoot.transform.SetParent(base.transform);
		characterRoot.transform.position = new Vector3(0f, 0f, 0f);
		gearRoot = new GameObject("GearRoot");
		gearRoot.transform.SetParent(base.transform);
		gearRoot.transform.position = new Vector3(0f, 0f, 0f);
		hairRoot = new GameObject("HairRoot");
		hairRoot.transform.SetParent(base.transform);
		hairRoot.transform.position = new Vector3(0f, 0f, 0f);
		hatHairRoot = new GameObject("HatHairRoot");
		hatHairRoot.transform.SetParent(base.transform);
		hatHairRoot.transform.position = new Vector3(0f, 0f, 0f);
		facialHairRoot = new GameObject("FacialHairRoot");
		facialHairRoot.transform.SetParent(base.transform);
		facialHairRoot.transform.position = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < sexes.Length; i++)
		{
			GameObject gameObject = new GameObject((i == 0) ? "Male" : "Female");
			gameObject.transform.SetParent(characterRoot.transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = 0; j < races.Length; j++)
			{
				for (int k = 0; k < variants.Length; k++)
				{
					Archetype archetype = new Archetype("", i == 0, _canCustomize: true);
					archetype.Sex = sexes[i];
					archetype.Race = races[j];
					archetype.Variant = variants[k];
					GameObject baseRigUI = UnityEngine.Object.Instantiate(baseRig, gameObject.transform);
					SDCSUtils.TransformCatalog boneCatalogUI = new SDCSUtils.TransformCatalog(baseRigUI.transform);
					CharacterConstructUtils.CreateViz(archetype, ref baseRigUI, ref boneCatalogUI);
					baseRigUI.name = archetype.Race + variants[k].ToString("00");
					Vector3 localPosition = new Vector3(k + i * 4, 0f, j);
					localPosition -= new Vector3(3.5f, 0f, 0f);
					baseRigUI.transform.SetParent(gameObject.transform);
					baseRigUI.transform.localPosition = localPosition;
					baseRigUI.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
					Create3DTextLabel(baseRigUI, sexes[i] + "_" + baseRigUI.name);
					AddClickableCollider(baseRigUI);
					allCharacters.Add(baseRigUI);
				}
			}
		}
		for (int l = 0; l < gear.Length; l++)
		{
			allGearKeys.Add(gear[l]);
			gearPathLookup.Add(gear[l], "@:Entities/Player/{sex}/Gear/" + gear[l] + "/gear{sex}" + gear[l] + "Prefab.prefab");
			headgearPathLookup.Add(gear[l], "@:Entities/Player/{sex}/Gear/" + gear[l] + "/HeadgearMorphMatrix/{race}{variant}/gear" + gear[l] + "Head.asset");
		}
		for (int m = 0; m < dlcGear.Length; m++)
		{
			allGearKeys.Add(dlcGear[m]);
			gearPathLookup.Add(dlcGear[m], "@:DLC/" + dlcGear[m] + "Cosmetic/Entities/Player/{sex}/Gear/Prefabs/gear{sex}" + dlcGear[m] + "Prefab.prefab");
			headgearPathLookup.Add(dlcGear[m], "@:DLC/" + dlcGear[m] + "Cosmetic/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear" + dlcGear[m] + "{hair}Head.asset");
		}
		for (int n = 0; n < twitchDropGear.Length; n++)
		{
			string text = twitchDropGear[n].Replace("StoreGear", "");
			allGearKeys.Add(twitchDropGear[n]);
			gearPathLookup.Add(twitchDropGear[n], "@:TwitchDrops/" + twitchDropGear[n] + "/Entities/Player/{sex}/Gear/Prefabs/gear{sex}" + text + "Prefab.prefab");
			headgearPathLookup.Add(twitchDropGear[n], "@:TwitchDrops/" + twitchDropGear[n] + "/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear" + text + "{hair}Head.asset");
		}
		CreateGearInstances();
		CreateHairInstances();
		CreateHatHairInstances();
		CreateFacialHairInstances();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateGearInstances()
	{
		foreach (GameObject item in allGear)
		{
			UnityEngine.Object.Destroy(item);
		}
		allGear.Clear();
		foreach (Transform item2 in gearRoot.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		for (int i = 0; i < sexes.Length; i++)
		{
			GameObject gameObject = new GameObject((i == 0) ? "Male" : "Female");
			gameObject.transform.SetParent(gearRoot.transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = 0; j < allGearKeys.Count; j++)
			{
				Archetype archetype = new Archetype("", i == 0, _canCustomize: true);
				archetype.Sex = sexes[i];
				archetype.Race = races[selectedRaceIndex];
				archetype.Variant = variants[selectedVariantIndex];
				archetype.Equipment = new List<SDCSUtils.SlotData>();
				for (int k = 0; k < baseParts.Length; k++)
				{
					SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
					slotData.PartName = baseParts[k];
					if (baseParts[k] != "head" || headHiders.Contains(allGearKeys[j]) || noMorphs.Contains(allGearKeys[j]))
					{
						slotData.PrefabName = gearPathLookup[allGearKeys[j]];
						slotData.BaseToTurnOff = ((baseParts[k] == "head" && !headHiders.Contains(allGearKeys[j])) ? null : baseParts[k]);
					}
					else
					{
						slotData.PrefabName = headgearPathLookup[allGearKeys[j]];
						slotData.BaseToTurnOff = null;
					}
					archetype.Equipment.Add(slotData);
				}
				GameObject baseRigUI = UnityEngine.Object.Instantiate(baseRig, gameObject.transform);
				SDCSUtils.TransformCatalog boneCatalogUI = new SDCSUtils.TransformCatalog(baseRigUI.transform);
				CharacterConstructUtils.CreateViz(archetype, ref baseRigUI, ref boneCatalogUI);
				baseRigUI.name = allGearKeys[j];
				Vector3 localPosition = new Vector3(j + 5, 0f, i);
				baseRigUI.transform.SetParent(gameObject.transform);
				baseRigUI.transform.localPosition = localPosition;
				baseRigUI.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				Create3DTextLabel(baseRigUI, archetype.Sex + "_" + allGearKeys[j]);
				AddClickableCollider(baseRigUI);
				allGear.Add(baseRigUI);
			}
		}
		foreach (GameObject item3 in allGear)
		{
			item3.SetActive(showGear);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateHairInstances()
	{
		foreach (GameObject item in allHair)
		{
			UnityEngine.Object.Destroy(item);
		}
		allHair.Clear();
		foreach (Transform item2 in hairRoot.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		for (int i = 0; i < sexes.Length; i++)
		{
			GameObject gameObject = new GameObject((i == 0) ? "Male" : "Female");
			gameObject.transform.SetParent(hairRoot.transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = -1; j < hairs.Length; j++)
			{
				Archetype archetype = new Archetype("", i == 0, _canCustomize: true);
				archetype.Sex = sexes[i];
				archetype.Race = races[selectedRaceIndex];
				archetype.Variant = variants[selectedVariantIndex];
				archetype.Hair = ((j == -1) ? "none" : hairs[j]);
				archetype.HairColor = "04 Brown";
				GameObject baseRigUI = UnityEngine.Object.Instantiate(baseRig, gameObject.transform);
				SDCSUtils.TransformCatalog boneCatalogUI = new SDCSUtils.TransformCatalog(baseRigUI.transform);
				CharacterConstructUtils.CreateViz(archetype, ref baseRigUI, ref boneCatalogUI);
				baseRigUI.name = ((j == -1) ? "none" : archetype.Hair);
				Vector3 localPosition = new Vector3(j + 6, 0f, i + 2);
				baseRigUI.transform.SetParent(gameObject.transform);
				baseRigUI.transform.localPosition = localPosition;
				baseRigUI.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				Create3DTextLabel(baseRigUI, (j == -1) ? "none" : (archetype.Sex + "_" + archetype.Hair));
				AddClickableCollider(baseRigUI);
				allHair.Add(baseRigUI);
			}
		}
		foreach (GameObject item3 in allHair)
		{
			item3.SetActive(showHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateHatHairInstances()
	{
		foreach (GameObject item in allHatHair)
		{
			UnityEngine.Object.Destroy(item);
		}
		allHatHair.Clear();
		foreach (Transform item2 in hatHairRoot.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		for (int i = 0; i < sexes.Length; i++)
		{
			GameObject gameObject = new GameObject((i == 0) ? "Male" : "Female");
			gameObject.transform.SetParent(hatHairRoot.transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = -1; j < hairs.Length; j++)
			{
				Archetype archetype = new Archetype("", i == 0, _canCustomize: true);
				archetype.Sex = sexes[i];
				archetype.Race = races[selectedRaceIndex];
				archetype.Variant = variants[selectedVariantIndex];
				archetype.Hair = ((j == -1) ? "none" : hairs[j]);
				archetype.HairColor = "04 Brown";
				archetype.Equipment = new List<SDCSUtils.SlotData>();
				SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
				slotData.PartName = baseParts[0];
				if (headHiders.Contains(allGearKeys[hatHairGearIndex]) || noMorphs.Contains(allGearKeys[hatHairGearIndex]))
				{
					slotData.PrefabName = gearPathLookup[allGearKeys[hatHairGearIndex]];
					slotData.BaseToTurnOff = ((!headHiders.Contains(allGearKeys[hatHairGearIndex])) ? null : baseParts[0]);
				}
				else
				{
					slotData.PrefabName = headgearPathLookup[allGearKeys[hatHairGearIndex]];
					slotData.BaseToTurnOff = null;
				}
				slotData.HairMaskType = SDCSUtils.SlotData.HairMaskTypes.Hat;
				archetype.Equipment.Add(slotData);
				GameObject baseRigUI = UnityEngine.Object.Instantiate(baseRig, gameObject.transform);
				SDCSUtils.TransformCatalog boneCatalogUI = new SDCSUtils.TransformCatalog(baseRigUI.transform);
				CharacterConstructUtils.CreateViz(archetype, ref baseRigUI, ref boneCatalogUI);
				baseRigUI.name = ((j == -1) ? "none" : (archetype.Hair + "_hat"));
				Vector3 localPosition = new Vector3(j + 6, 0f, i + 4);
				baseRigUI.transform.SetParent(gameObject.transform);
				baseRigUI.transform.localPosition = localPosition;
				baseRigUI.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				Create3DTextLabel(baseRigUI, (j == -1) ? "none" : (archetype.Sex + "_" + archetype.Hair));
				AddClickableCollider(baseRigUI);
				allHatHair.Add(baseRigUI);
			}
		}
		foreach (GameObject item3 in allHatHair)
		{
			item3.SetActive(showHatHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateFacialHairInstances()
	{
		foreach (GameObject item in allFacialHair)
		{
			UnityEngine.Object.Destroy(item);
		}
		allFacialHair.Clear();
		foreach (Transform item2 in facialHairRoot.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		Archetype archetype = new Archetype("", _isMale: true, _canCustomize: true);
		archetype.Sex = sexes[0];
		archetype.Race = races[selectedRaceIndex];
		archetype.Variant = variants[selectedVariantIndex];
		for (int i = 0; i < facialHairIndexes.Length; i++)
		{
			archetype.Hair = hairs[0];
			archetype.HairColor = "04 Brown";
			archetype.MustacheName = facialHairIndexes[i].ToString();
			archetype.ChopsName = facialHairIndexes[i].ToString();
			archetype.BeardName = facialHairIndexes[i].ToString();
			GameObject baseRigUI = UnityEngine.Object.Instantiate(baseRig, facialHairRoot.transform);
			SDCSUtils.TransformCatalog boneCatalogUI = new SDCSUtils.TransformCatalog(baseRigUI.transform);
			CharacterConstructUtils.CreateViz(archetype, ref baseRigUI, ref boneCatalogUI);
			baseRigUI.name = "FacialHair " + i.ToString("00");
			Vector3 localPosition = new Vector3(i + 5, 0f, 6f);
			baseRigUI.transform.SetParent(facialHairRoot.transform);
			baseRigUI.transform.localPosition = localPosition;
			baseRigUI.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			Create3DTextLabel(baseRigUI, "FacialHair " + i.ToString("00"));
			AddClickableCollider(baseRigUI);
			allFacialHair.Add(baseRigUI);
		}
		foreach (GameObject item3 in allFacialHair)
		{
			item3.SetActive(showFacialHair);
		}
	}

	public void RespawnAllGroups()
	{
		ShowAllCharacters();
		CreateGearInstances();
		CreateHairInstances();
		CreateHatHairInstances();
		CreateFacialHairInstances();
	}

	public void RespawnHatHairGroup()
	{
		ShowAllCharacters();
		CreateHatHairInstances();
	}

	public string[] GetGearTypes()
	{
		return allGearKeys.ToArray();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Create3DTextLabel(GameObject instance, string labelText)
	{
		GameObject gameObject = new GameObject(instance.name + "_Label");
		gameObject.transform.SetParent(instance.transform);
		gameObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
		obj.name = "LabelBackground";
		obj.transform.SetParent(gameObject.transform);
		obj.transform.localPosition = new Vector3(0f, 0f, 0.01f);
		obj.transform.localScale = new Vector3((float)labelText.Length * 0.175f, 0.4f, 1f);
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		component.material = new Material(Shader.Find("Unlit/Color"));
		component.material.color = new Color(0f, 0f, 0f, 0.7f);
		component.shadowCastingMode = ShadowCastingMode.Off;
		component.receiveShadows = false;
		GameObject obj2 = new GameObject("Text");
		obj2.transform.SetParent(gameObject.transform);
		obj2.transform.localPosition = Vector3.zero;
		TextMesh textMesh = obj2.AddComponent<TextMesh>();
		textMesh.text = labelText;
		textMesh.fontSize = 30;
		textMesh.characterSize = 0.1f;
		textMesh.alignment = TextAlignment.Center;
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.color = labelColor;
		MeshRenderer component2 = textMesh.GetComponent<MeshRenderer>();
		component2.shadowCastingMode = ShadowCastingMode.Off;
		component2.receiveShadows = false;
		component2.sortingOrder = 1;
		gameObject.transform.localScale = new Vector3(labelScale, labelScale, labelScale);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddClickableCollider(GameObject instance)
	{
		Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("No colliders found in " + instance.name + ". Character won't be clickable.");
			return;
		}
		instance.AddComponent<CharacterClickHandler>().parentScript = this;
		Collider[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.AddComponent<CharacterColliderHelper>().mainInstance = instance;
		}
	}

	public void OnCharacterClicked(GameObject clickedCharacter)
	{
		if (selectedCharacter == clickedCharacter)
		{
			ShowAllCharacters();
		}
		else
		{
			HideAllExcept(clickedCharacter);
		}
	}

	public void ShowAllCharacters()
	{
		selectedCharacter = null;
		allCharactersVisible = true;
		foreach (GameObject allCharacter in allCharacters)
		{
			allCharacter.SetActive(showCharacters);
		}
		foreach (GameObject item in allGear)
		{
			item.SetActive(showGear);
		}
		foreach (GameObject item2 in allHair)
		{
			item2.SetActive(showHair);
		}
		foreach (GameObject item3 in allHatHair)
		{
			item3.SetActive(showHatHair);
		}
		foreach (GameObject item4 in allFacialHair)
		{
			item4.SetActive(showFacialHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HideAllExcept(GameObject exception)
	{
		selectedCharacter = exception;
		allCharactersVisible = false;
		foreach (GameObject allCharacter in allCharacters)
		{
			allCharacter.SetActive(allCharacter == exception);
		}
		foreach (GameObject item in allGear)
		{
			item.SetActive(item == exception);
		}
		foreach (GameObject item2 in allHair)
		{
			item2.SetActive(item2 == exception);
		}
		foreach (GameObject item3 in allHatHair)
		{
			item3.SetActive(item3 == exception);
		}
		foreach (GameObject item4 in allFacialHair)
		{
			item4.SetActive(item4 == exception);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (allCharactersVisible)
		{
			characterRoot.SetActive(showCharacters);
			gearRoot.SetActive(showGear);
			hairRoot.SetActive(showHair);
			hatHairRoot.SetActive(showHatHair);
			facialHairRoot.SetActive(showFacialHair);
		}
		if (Input.GetMouseButtonDown(0))
		{
			CheckMouseClick();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckMouseClick()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (EventSystem.current.IsPointerOverGameObject() || !Physics.Raycast(ray, out var hitInfo))
		{
			return;
		}
		CharacterColliderHelper component = hitInfo.collider.gameObject.GetComponent<CharacterColliderHelper>();
		if (component != null && component.mainInstance != null)
		{
			CharacterClickHandler component2 = component.mainInstance.GetComponent<CharacterClickHandler>();
			if (component2 != null)
			{
				component2.HandleClick();
			}
		}
	}
}
