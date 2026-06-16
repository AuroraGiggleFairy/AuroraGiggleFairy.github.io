using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

	[Tooltip("Dropdown for selecting head gear")]
	public TMP_Dropdown gearHeadDropdown;

	[Tooltip("Dropdown for selecting body gear")]
	public TMP_Dropdown gearBodyDropdown;

	[Tooltip("Dropdown for selecting hands gear")]
	public TMP_Dropdown gearHandsDropdown;

	[Tooltip("Dropdown for selecting feet gear")]
	public TMP_Dropdown gearFeetDropdown;

	[Tooltip("Dropdown for selecting facial hair")]
	public TMP_Dropdown facialHairDropdown;

	[Tooltip("Button to toggle Jiggle")]
	public Button toggleJiggleButton;

	[Tooltip("Slider to Rotate Character")]
	public Slider rotationSlider;

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
		if (toggleJiggleButton != null)
		{
			Image image = toggleJiggleButton.GetComponent<Image>();
			toggleJiggleButton.onClick.AddListener([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				if (characterConfigurator != null)
				{
					characterConfigurator.ToggleJiggle();
					image.color = (characterConfigurator.IsJiggleEnabled() ? new Color(1f, 0.85f, 0.75f) : new Color(0.7f, 0.7f, 0.7f));
				}
			});
		}
		if (!(rotationSlider != null))
		{
			return;
		}
		rotationSlider.minValue = 0f;
		rotationSlider.maxValue = 360f;
		rotationSlider.value = 180f;
		rotationSlider.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (float value) =>
		{
			if (characterConfigurator != null)
			{
				characterConfigurator.RotateCharacter(value);
			}
		});
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
		if (gearHeadDropdown != null)
		{
			gearHeadDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetGearTypes();
			foreach (string text8 in sexTypes)
			{
				gearHeadDropdown.options.Add(new TMP_Dropdown.OptionData(text8));
			}
			gearHeadDropdown.value = characterConfigurator.selectedHeadGearIndex + 1;
			gearHeadDropdown.RefreshShownValue();
		}
		if (gearBodyDropdown != null)
		{
			gearBodyDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetGearTypes();
			foreach (string text9 in sexTypes)
			{
				gearBodyDropdown.options.Add(new TMP_Dropdown.OptionData(text9));
			}
			gearBodyDropdown.value = characterConfigurator.selectedBodyGearIndex + 1;
			gearBodyDropdown.RefreshShownValue();
		}
		if (gearHandsDropdown != null)
		{
			gearHandsDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetGearTypes();
			foreach (string text10 in sexTypes)
			{
				gearHandsDropdown.options.Add(new TMP_Dropdown.OptionData(text10));
			}
			gearHandsDropdown.value = characterConfigurator.selectedHandsGearIndex + 1;
			gearHandsDropdown.RefreshShownValue();
		}
		if (gearFeetDropdown != null)
		{
			gearFeetDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetGearTypes();
			foreach (string text11 in sexTypes)
			{
				gearFeetDropdown.options.Add(new TMP_Dropdown.OptionData(text11));
			}
			gearFeetDropdown.value = characterConfigurator.selectedFeetGearIndex + 1;
			gearFeetDropdown.RefreshShownValue();
		}
		if (facialHairDropdown != null)
		{
			facialHairDropdown.ClearOptions();
			string[] sexTypes = characterConfigurator.GetFacialHairTypes();
			foreach (string text12 in sexTypes)
			{
				facialHairDropdown.options.Add(new TMP_Dropdown.OptionData(text12));
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
			raceDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetRace(index);
			});
		}
		if (variantDropdown != null)
		{
			variantDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetVariant(index);
			});
		}
		if (hairDropdown != null)
		{
			hairDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetHair(index);
			});
		}
		if (hairColorDropdown != null)
		{
			hairColorDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetHairColor(index);
			});
		}
		if (eyeColorDropdown != null)
		{
			eyeColorDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetEyeColor(index);
			});
		}
		if (gearDropdown != null)
		{
			gearDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetGear(index);
			});
			gearDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				if (gearHeadDropdown != null)
				{
					gearHeadDropdown.SetValueWithoutNotify(index);
				}
				if (gearBodyDropdown != null)
				{
					gearBodyDropdown.SetValueWithoutNotify(index);
				}
				if (gearHandsDropdown != null)
				{
					gearHandsDropdown.SetValueWithoutNotify(index);
				}
				if (gearFeetDropdown != null)
				{
					gearFeetDropdown.SetValueWithoutNotify(index);
				}
			});
		}
		if (gearHeadDropdown != null)
		{
			gearHeadDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetHeadGear(index);
			});
		}
		if (gearBodyDropdown != null)
		{
			gearBodyDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetBodyGear(index);
			});
		}
		if (gearHandsDropdown != null)
		{
			gearHandsDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetHandsGear(index);
			});
		}
		if (gearFeetDropdown != null)
		{
			gearFeetDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetFeetGear(index);
			});
		}
		if (facialHairDropdown != null)
		{
			facialHairDropdown.onValueChanged.AddListener([PublicizedFrom(EAccessModifier.Private)] (int index) =>
			{
				characterConfigurator.SetFacialHair(index);
			});
		}
		WireUpArrowButtons(sexDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(sexDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(sexDropdown, 1);
		});
		WireUpArrowButtons(raceDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(raceDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(raceDropdown, 1);
		});
		WireUpArrowButtons(variantDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(variantDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(variantDropdown, 1);
		});
		WireUpArrowButtons(hairDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(hairDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(hairDropdown, 1);
		});
		WireUpArrowButtons(hairColorDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(hairColorDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(hairColorDropdown, 1);
		});
		WireUpArrowButtons(eyeColorDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(eyeColorDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(eyeColorDropdown, 1);
		});
		WireUpArrowButtons(gearDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearDropdown, 1);
		});
		WireUpArrowButtons(gearHeadDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearHeadDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearHeadDropdown, 1);
		});
		WireUpArrowButtons(gearBodyDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearBodyDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearBodyDropdown, 1);
		});
		WireUpArrowButtons(gearHandsDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearHandsDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearHandsDropdown, 1);
		});
		WireUpArrowButtons(gearFeetDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearFeetDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(gearFeetDropdown, 1);
		});
		WireUpArrowButtons(facialHairDropdown, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(facialHairDropdown, -1);
		}, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			CycleDropdown(facialHairDropdown, 1);
		});
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
