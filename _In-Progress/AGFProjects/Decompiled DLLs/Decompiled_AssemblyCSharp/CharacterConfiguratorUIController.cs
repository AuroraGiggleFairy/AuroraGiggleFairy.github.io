using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CharacterConfiguratorUIController : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Reference to the CharacterConstruct script")]
	public CharacterConfigurator characterConfigurator;

	[Header("Character Configuration")]
	[Tooltip("Dropdown for selecting sex")]
	public TMP_Dropdown sexDropdown;

	[Tooltip("Dropdown for selecting race")]
	public TMP_Dropdown raceDropdown;

	[Tooltip("Dropdown for selecting variant")]
	public TMP_Dropdown variantDropdown;

	[Header("Appearance Configuration")]
	[Tooltip("Dropdown for selecting hair style")]
	public TMP_Dropdown hairDropdown;

	[Tooltip("Dropdown for selecting hair color")]
	public TMP_Dropdown hairColorDropdown;

	[Tooltip("Dropdown for selecting eye color")]
	public TMP_Dropdown eyeColorDropdown;

	[Tooltip("Dropdown for selecting gear")]
	public TMP_Dropdown gearDropdown;

	[Tooltip("Dropdown for selecting facial hair")]
	public TMP_Dropdown facialHairDropdown;

	[Header("UI Panels")]
	[Tooltip("Panel that contains all UI controls")]
	public GameObject controlPanel;

	[Tooltip("Panel containing facial hair controls (shown only for males)")]
	public GameObject facialHairPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (characterConfigurator == null)
		{
			Debug.LogError("CharacterConstruct reference is missing! Please assign it in the inspector.");
			return;
		}
		InitializeDropdowns();
		SetupDropdownListeners();
		UpdateFacialHairPanelVisibility();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDropdowns()
	{
		if (sexDropdown != null)
		{
			sexDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetSexTypes();
			foreach (string text in sexTypes)
			{
				sexDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
			sexDropdown.value = characterConfigurator.selectedSexIndex;
			sexDropdown.RefreshShownValue();
		}
		if (raceDropdown != null)
		{
			raceDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetRaceTypes();
			foreach (string text2 in sexTypes)
			{
				raceDropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			}
			raceDropdown.value = characterConfigurator.selectedRaceIndex;
			raceDropdown.RefreshShownValue();
		}
		if (variantDropdown != null)
		{
			variantDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetVariantTypes();
			foreach (string text3 in sexTypes)
			{
				variantDropdown.options.Add(new TMP_Dropdown.OptionData(text3));
			}
			variantDropdown.value = characterConfigurator.selectedVariantIndex;
			variantDropdown.RefreshShownValue();
		}
		if (hairDropdown != null)
		{
			hairDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetHairTypes();
			foreach (string text4 in sexTypes)
			{
				hairDropdown.options.Add(new TMP_Dropdown.OptionData(text4));
			}
			hairDropdown.value = characterConfigurator.selectedHairIndex + 1;
			hairDropdown.RefreshShownValue();
		}
		if (hairColorDropdown != null)
		{
			hairColorDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetHairColors();
			foreach (string text5 in sexTypes)
			{
				hairColorDropdown.options.Add(new TMP_Dropdown.OptionData(text5));
			}
			hairColorDropdown.value = characterConfigurator.selectedHairColorIndex;
			hairColorDropdown.RefreshShownValue();
		}
		if (eyeColorDropdown != null)
		{
			eyeColorDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetEyeColors();
			foreach (string text6 in sexTypes)
			{
				eyeColorDropdown.options.Add(new TMP_Dropdown.OptionData(text6));
			}
			eyeColorDropdown.value = characterConfigurator.selectedEyeColorIndex;
			eyeColorDropdown.RefreshShownValue();
		}
		if (gearDropdown != null)
		{
			gearDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetGearTypes();
			foreach (string text7 in sexTypes)
			{
				gearDropdown.options.Add(new TMP_Dropdown.OptionData(text7));
			}
			gearDropdown.value = characterConfigurator.selectedGearIndex + 1;
			gearDropdown.RefreshShownValue();
		}
		if (facialHairDropdown != null)
		{
			facialHairDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetFacialHairTypes();
			foreach (string text8 in sexTypes)
			{
				facialHairDropdown.options.Add(new TMP_Dropdown.OptionData(text8));
			}
			facialHairDropdown.value = characterConfigurator.selectedFacialHairIndex + 1;
			facialHairDropdown.RefreshShownValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDropdownListeners()
	{
		if (sexDropdown != null)
		{
			sexDropdown.onValueChanged.AddListener(OnSexDropdownChanged);
		}
		if (raceDropdown != null)
		{
			raceDropdown.onValueChanged.AddListener(OnRaceDropdownChanged);
		}
		if (variantDropdown != null)
		{
			variantDropdown.onValueChanged.AddListener(OnVariantDropdownChanged);
		}
		if (hairDropdown != null)
		{
			hairDropdown.onValueChanged.AddListener(OnHairDropdownChanged);
		}
		if (hairColorDropdown != null)
		{
			hairColorDropdown.onValueChanged.AddListener(OnHairColorDropdownChanged);
		}
		if (eyeColorDropdown != null)
		{
			eyeColorDropdown.onValueChanged.AddListener(OnEyeColorDropdownChanged);
		}
		if (gearDropdown != null)
		{
			gearDropdown.onValueChanged.AddListener(OnGearDropdownChanged);
		}
		if (facialHairDropdown != null)
		{
			facialHairDropdown.onValueChanged.AddListener(OnFacialHairDropdownChanged);
		}
		WireUpArrowButtons(sexDropdown, OnSexLeftArrow, OnSexRightArrow);
		WireUpArrowButtons(raceDropdown, OnRaceLeftArrow, OnRaceRightArrow);
		WireUpArrowButtons(variantDropdown, OnVariantLeftArrow, OnVariantRightArrow);
		WireUpArrowButtons(hairDropdown, OnHairLeftArrow, OnHairRightArrow);
		WireUpArrowButtons(hairColorDropdown, OnHairColorLeftArrow, OnHairColorRightArrow);
		WireUpArrowButtons(eyeColorDropdown, OnEyeColorLeftArrow, OnEyeColorRightArrow);
		WireUpArrowButtons(gearDropdown, OnGearLeftArrow, OnGearRightArrow);
		WireUpArrowButtons(facialHairDropdown, OnFacialHairLeftArrow, OnFacialHairRightArrow);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WireUpArrowButtons(TMP_Dropdown dropdown, UnityAction leftAction, UnityAction rightAction)
	{
		if (!(dropdown != null))
		{
			return;
		}
		DropdownArrowButtons component = dropdown.gameObject.GetComponent<DropdownArrowButtons>();
		if (component != null)
		{
			if (component.leftButton != null)
			{
				component.leftButton.onClick.AddListener(leftAction);
			}
			if (component.rightButton != null)
			{
				component.rightButton.onClick.AddListener(rightAction);
			}
		}
	}

	public void OnSexDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetSex(index);
			UpdateFacialHairPanelVisibility();
			if (index == 1 && facialHairDropdown != null)
			{
				facialHairDropdown.value = 0;
			}
		}
	}

	public void OnRaceDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetRace(index);
		}
	}

	public void OnVariantDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetVariant(index);
		}
	}

	public void OnHairDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetHair(index);
		}
	}

	public void OnHairColorDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetHairColor(index);
		}
	}

	public void OnEyeColorDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetEyeColor(index);
		}
	}

	public void OnGearDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetGear(index);
		}
	}

	public void OnFacialHairDropdownChanged(int index)
	{
		if (characterConfigurator != null)
		{
			characterConfigurator.SetFacialHair(index);
		}
	}

	public void OnSexLeftArrow()
	{
		if (sexDropdown != null)
		{
			CycleDropdown(sexDropdown, -1);
		}
	}

	public void OnSexRightArrow()
	{
		if (sexDropdown != null)
		{
			CycleDropdown(sexDropdown, 1);
		}
	}

	public void OnRaceLeftArrow()
	{
		if (raceDropdown != null)
		{
			CycleDropdown(raceDropdown, -1);
		}
	}

	public void OnRaceRightArrow()
	{
		if (raceDropdown != null)
		{
			CycleDropdown(raceDropdown, 1);
		}
	}

	public void OnVariantLeftArrow()
	{
		if (variantDropdown != null)
		{
			CycleDropdown(variantDropdown, -1);
		}
	}

	public void OnVariantRightArrow()
	{
		if (variantDropdown != null)
		{
			CycleDropdown(variantDropdown, 1);
		}
	}

	public void OnHairLeftArrow()
	{
		if (hairDropdown != null)
		{
			CycleDropdown(hairDropdown, -1);
		}
	}

	public void OnHairRightArrow()
	{
		if (hairDropdown != null)
		{
			CycleDropdown(hairDropdown, 1);
		}
	}

	public void OnHairColorLeftArrow()
	{
		if (hairColorDropdown != null)
		{
			CycleDropdown(hairColorDropdown, -1);
		}
	}

	public void OnHairColorRightArrow()
	{
		if (hairColorDropdown != null)
		{
			CycleDropdown(hairColorDropdown, 1);
		}
	}

	public void OnEyeColorLeftArrow()
	{
		if (eyeColorDropdown != null)
		{
			CycleDropdown(eyeColorDropdown, -1);
		}
	}

	public void OnEyeColorRightArrow()
	{
		if (eyeColorDropdown != null)
		{
			CycleDropdown(eyeColorDropdown, 1);
		}
	}

	public void OnGearLeftArrow()
	{
		if (gearDropdown != null)
		{
			CycleDropdown(gearDropdown, -1);
		}
	}

	public void OnGearRightArrow()
	{
		if (gearDropdown != null)
		{
			CycleDropdown(gearDropdown, 1);
		}
	}

	public void OnFacialHairLeftArrow()
	{
		if (facialHairDropdown != null)
		{
			CycleDropdown(facialHairDropdown, -1);
		}
	}

	public void OnFacialHairRightArrow()
	{
		if (facialHairDropdown != null)
		{
			CycleDropdown(facialHairDropdown, 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CycleDropdown(TMP_Dropdown dropdown, int direction)
	{
		if (!(dropdown == null) && dropdown.options.Count != 0)
		{
			int num = dropdown.value + direction;
			if (num < 0)
			{
				num = dropdown.options.Count - 1;
			}
			else if (num >= dropdown.options.Count)
			{
				num = 0;
			}
			dropdown.value = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFacialHairPanelVisibility()
	{
		if (facialHairPanel != null && characterConfigurator != null)
		{
			facialHairPanel.SetActive(characterConfigurator.selectedSexIndex == 0);
		}
	}

	public void RefreshAllDropdowns()
	{
		InitializeDropdowns();
	}
}
