using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterConstructUIController : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Reference to the CharacterConstruct script")]
	public CharacterConstruct characterConstruct;

	[Header("UI Elements")]
	[Tooltip("Toggle button for showing characters")]
	public Toggle showCharactersToggle;

	[Tooltip("Toggle button for showing gear")]
	public Toggle showGearToggle;

	[Tooltip("Toggle button for showing hair")]
	public Toggle showHairToggle;

	[Tooltip("Toggle button for showing hat hair")]
	public Toggle showHatHairToggle;

	[Tooltip("Toggle button for showing facial hair")]
	public Toggle showFacialHairToggle;

	[Header("Race and Variant Controls")]
	[Tooltip("Panel containing race and variant controls")]
	public GameObject raceVariantControlsPanel;

	[Tooltip("Dropdown for selecting race")]
	public TMP_Dropdown raceDropdown;

	[Tooltip("Dropdown for selecting variant")]
	public TMP_Dropdown variantDropdown;

	[Header("Hat Hair Controls")]
	[Tooltip("Panel containing hat gear controls")]
	public GameObject hatGearControlsPanel;

	[Tooltip("Dropdown for selecting hat gear type")]
	public TMP_Dropdown hatGearDropdown;

	[Header("UI Panel")]
	[Tooltip("Panel that contains all UI controls")]
	public GameObject controlPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (characterConstruct != null)
		{
			InitializeToggles();
			InitializeDropdowns();
		}
		else
		{
			Debug.LogError("CharacterConstruct reference is missing! Please assign it in the inspector.");
		}
		SetupToggleListeners();
		SetupDropdownListeners();
		UpdateHatGearControlsVisibility(showHatHairToggle.isOn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeToggles()
	{
		showCharactersToggle.isOn = characterConstruct.ShowCharacters;
		showGearToggle.isOn = characterConstruct.ShowGear;
		showHairToggle.isOn = characterConstruct.ShowHair;
		showHatHairToggle.isOn = characterConstruct.ShowHatHair;
		showFacialHairToggle.isOn = characterConstruct.ShowFacialHair;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDropdowns()
	{
		if (raceDropdown != null)
		{
			raceDropdown.ClearOptions();
			string[] raceTypes = characterConstruct.GetRaceTypes();
			foreach (string text in raceTypes)
			{
				raceDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
			raceDropdown.value = characterConstruct.selectedRaceIndex;
			raceDropdown.RefreshShownValue();
		}
		if (variantDropdown != null)
		{
			variantDropdown.ClearOptions();
			string[] raceTypes = characterConstruct.GetVariantTypes();
			foreach (string text2 in raceTypes)
			{
				variantDropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			}
			variantDropdown.value = characterConstruct.selectedVariantIndex;
			variantDropdown.RefreshShownValue();
		}
		if (hatGearDropdown != null)
		{
			hatGearDropdown.ClearOptions();
			string[] raceTypes = characterConstruct.GetGearTypes();
			foreach (string text3 in raceTypes)
			{
				hatGearDropdown.options.Add(new TMP_Dropdown.OptionData(text3));
			}
			hatGearDropdown.value = characterConstruct.hatHairGearIndex;
			hatGearDropdown.RefreshShownValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupToggleListeners()
	{
		showCharactersToggle.onValueChanged.AddListener(OnShowCharactersToggled);
		showGearToggle.onValueChanged.AddListener(OnShowGearToggled);
		showHairToggle.onValueChanged.AddListener(OnShowHairToggled);
		showHatHairToggle.onValueChanged.AddListener(OnShowHatHairToggled);
		showFacialHairToggle.onValueChanged.AddListener(OnShowFacialHairToggled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDropdownListeners()
	{
		if (raceDropdown != null)
		{
			raceDropdown.onValueChanged.AddListener(OnRaceDropdownChanged);
		}
		if (variantDropdown != null)
		{
			variantDropdown.onValueChanged.AddListener(OnVariantDropdownChanged);
		}
		if (hatGearDropdown != null)
		{
			hatGearDropdown.onValueChanged.AddListener(OnHatGearDropdownChanged);
		}
	}

	public void OnShowCharactersToggled(bool isOn)
	{
		if (characterConstruct != null)
		{
			characterConstruct.ShowCharacters = isOn;
		}
	}

	public void OnShowGearToggled(bool isOn)
	{
		if (characterConstruct != null)
		{
			characterConstruct.ShowGear = isOn;
		}
	}

	public void OnShowHairToggled(bool isOn)
	{
		if (characterConstruct != null)
		{
			characterConstruct.ShowHair = isOn;
		}
	}

	public void OnShowHatHairToggled(bool isOn)
	{
		if (characterConstruct != null)
		{
			characterConstruct.ShowHatHair = isOn;
			UpdateHatGearControlsVisibility(isOn);
		}
	}

	public void OnShowFacialHairToggled(bool isOn)
	{
		if (characterConstruct != null)
		{
			characterConstruct.ShowFacialHair = isOn;
		}
	}

	public void OnRaceDropdownChanged(int index)
	{
		if (characterConstruct != null)
		{
			characterConstruct.selectedRaceIndex = index;
			characterConstruct.RespawnAllGroups();
		}
	}

	public void OnVariantDropdownChanged(int index)
	{
		if (characterConstruct != null)
		{
			characterConstruct.selectedVariantIndex = index;
			characterConstruct.RespawnAllGroups();
		}
	}

	public void OnHatGearDropdownChanged(int index)
	{
		if (characterConstruct != null)
		{
			characterConstruct.hatHairGearIndex = index;
			characterConstruct.RespawnHatHairGroup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHatGearControlsVisibility(bool isHatHairVisible)
	{
		if (hatGearControlsPanel != null)
		{
			hatGearControlsPanel.SetActive(isHatHairVisible);
		}
	}
}
