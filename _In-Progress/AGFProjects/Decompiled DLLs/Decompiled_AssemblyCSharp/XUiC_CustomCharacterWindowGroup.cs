using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_CustomCharacterWindowGroup : XUiController
{
	public enum Gender
	{
		Male,
		Female
	}

	public enum Race
	{
		White,
		Black,
		Asian,
		Hispanic
	}

	public struct NameData(string _internalName, string _formattedName)
	{
		public string InternalName = _internalName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string FormattedName = _formattedName;

		public override string ToString()
		{
			return FormattedName;
		}
	}

	public static string ID = "";

	public bool IsMale = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasChanges;

	public PlayerProfile playerProfile;

	public Archetype archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> races;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> genders;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> eyeColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> hairs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> hairColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> mustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> chops;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> beards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<NameData> variants;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SDCSPreviewWindow previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRandomize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedRace = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedGenders = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedHairs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedHairColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedVariants;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedEyeColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedMustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedChops;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockRace;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockGender;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockEyeColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockHairStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockHairColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockMustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockChops;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CustomCharacterWindow CustomCharacterWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom gr;

	public override void Init()
	{
		base.Init();
		ID = windowGroup.ID;
		previewWindow = GetChildByType<XUiC_SDCSPreviewWindow>();
		btnBack = (XUiC_SimpleButton)GetChildById("btnBack");
		btnBack.OnPressed += BtnBack_OnPress;
		btnApply = (XUiC_SimpleButton)GetChildById("btnApply");
		btnApply.OnPressed += BtnApply_OnPress;
		RefreshApplyLabel();
		btnRandomize = (XUiC_SimpleButton)GetChildById("btnRandomize");
		btnRandomize.OnPressed += BtnRandomize_OnPressed;
		races = (XUiC_ComboBoxList<NameData>)GetChildById("cbxRace");
		races.OnValueChanged += Races_OnValueChanged;
		genders = (XUiC_ComboBoxList<NameData>)GetChildById("cbxGender");
		genders.OnValueChanged += Genders_OnValueChanged;
		eyeColors = (XUiC_ComboBoxList<NameData>)GetChildById("cbxEyeColor");
		eyeColors.OnValueChanged += EyeColors_OnValueChanged;
		hairs = (XUiC_ComboBoxList<NameData>)GetChildById("cbxHairStyle");
		hairs.OnValueChanged += Hairs_OnValueChanged;
		hairColors = (XUiC_ComboBoxList<NameData>)GetChildById("cbxHairColor");
		hairColors.OnValueChanged += HairColors_OnValueChanged;
		variants = (XUiC_ComboBoxList<NameData>)GetChildById("cbxFace");
		variants.OnValueChanged += Variants_OnValueChanged;
		mustaches = (XUiC_ComboBoxList<NameData>)GetChildById("cbxMustaches");
		mustaches.OnValueChanged += Mustaches_OnValueChanged;
		chops = (XUiC_ComboBoxList<NameData>)GetChildById("cbxChops");
		chops.OnValueChanged += Chops_OnValueChanged;
		beards = (XUiC_ComboBoxList<NameData>)GetChildById("cbxBeards");
		beards.OnValueChanged += Beards_OnValueChanged;
		XUiController childById = GetChildById("btnLockRace");
		if (childById != null)
		{
			btnLockRace = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockRace_OnPress;
		}
		childById = GetChildById("btnLockGender");
		if (childById != null)
		{
			btnLockGender = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockGender_OnPress;
		}
		childById = GetChildById("btnLockFace");
		if (childById != null)
		{
			btnLockFace = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockFace_OnPress;
		}
		childById = GetChildById("btnLockEyeColor");
		if (childById != null)
		{
			btnLockEyeColor = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockEyeColor_OnPress;
		}
		childById = GetChildById("btnLockHairStyle");
		if (childById != null)
		{
			btnLockHairStyle = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockHairStyle_OnPress;
		}
		childById = GetChildById("btnLockHairColor");
		if (childById != null)
		{
			btnLockHairColor = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockHairColor_OnPress;
		}
		childById = GetChildById("btnLockMustaches");
		if (childById != null)
		{
			btnLockMustaches = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockMustache_OnPress;
		}
		childById = GetChildById("btnLockChops");
		if (childById != null)
		{
			btnLockChops = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockChops_OnPress;
		}
		childById = GetChildById("btnLockBeards");
		if (childById != null)
		{
			btnLockBeards = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += BtnLockBeards_OnPress;
		}
		CustomCharacterWindow = GetChildByType<XUiC_CustomCharacterWindow>();
		gr = GameRandomManager.Instance.CreateGameRandom();
		SDCSDataUtils.SetupData();
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLockButtonState(XUiV_Button btn, bool isLocked)
	{
		btn.DefaultSpriteName = (isLocked ? CustomCharacterWindow.lockedSprite : CustomCharacterWindow.unlockedSprite);
		btn.DefaultSpriteColor = (isLocked ? CustomCharacterWindow.lockedColor : CustomCharacterWindow.unlockedColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockRace_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedRace = !lockedRace;
		SetLockButtonState(btnLockRace, lockedRace);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockGender_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedGenders = !lockedGenders;
		SetLockButtonState(btnLockGender, lockedGenders);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockFace_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedVariants = !lockedVariants;
		SetLockButtonState(btnLockFace, lockedVariants);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockEyeColor_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedEyeColors = !lockedEyeColors;
		SetLockButtonState(btnLockEyeColor, lockedEyeColors);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockHairStyle_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedHairs = !lockedHairs;
		SetLockButtonState(btnLockHairStyle, lockedHairs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockHairColor_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedHairColors = !lockedHairColors;
		SetLockButtonState(btnLockHairColor, lockedHairColors);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockMustache_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedMustaches = !lockedMustaches;
		SetLockButtonState(btnLockMustaches, lockedMustaches);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockChops_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedChops = !lockedChops;
		SetLockButtonState(btnLockChops, lockedChops);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockBeards_OnPress(XUiController _sender, int _mouseButton)
	{
		lockedBeards = !lockedBeards;
		SetLockButtonState(btnLockBeards, lockedBeards);
	}

	public override void OnOpen()
	{
		windowGroup.openWindowOnEsc = XUiC_OptionsProfiles.ID;
		base.OnOpen();
		playerProfile = null;
		archetype = null;
		archetype = Archetype.GetArchetype(ProfileSDF.CurrentProfileName());
		if (archetype != null)
		{
			archetype = archetype.Clone();
		}
		else
		{
			string profileName = ProfileSDF.CurrentProfileName();
			playerProfile = PlayerProfile.LoadProfile(profileName).Clone();
		}
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "lblChangeView", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoRotateLeft", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoRotateRight", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		SetInitialOptions();
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetInitialOptions()
	{
		if (playerProfile != null)
		{
			SetupGenders();
			genders.SelectedIndex = (playerProfile.IsMale ? 1 : 0);
			SetupRaces(playerProfile.IsMale);
			SetupVariants(playerProfile.IsMale, playerProfile.RaceName);
			SetupHairStyles(playerProfile.IsMale);
			SetupMustaches(playerProfile.IsMale);
			SetupChops(playerProfile.IsMale);
			SetupBeards(playerProfile.IsMale);
			SetupEyeColors();
			SetupHairColors();
			SetSelectedRace(playerProfile.RaceName);
			SetSelectedVariant(playerProfile.VariantNumber);
			SetSelectedEyeColor(playerProfile.EyeColor);
			SetSelectedHair(playerProfile.HairName);
			SetSelectedMustache(playerProfile.MustacheName);
			SetSelectedChops(playerProfile.ChopsName);
			SetSelectedBeard(playerProfile.BeardName);
			SetSelectedHairColor(playerProfile.HairColor);
		}
		else
		{
			genders.SelectedIndex = (playerProfile.IsMale ? 1 : 0);
			SetupRaces(archetype.IsMale);
			SetupVariants(archetype.IsMale, archetype.Race);
			SetupHairStyles(archetype.IsMale);
			SetupMustaches(archetype.IsMale);
			SetupChops(archetype.IsMale);
			SetupBeards(archetype.IsMale);
			SetupEyeColors();
			SetupHairColors();
			SetSelectedRace(archetype.Race);
			SetSelectedVariant(archetype.Variant);
			SetSelectedEyeColor(archetype.EyeColorName);
			SetSelectedHair(archetype.Hair);
			SetSelectedMustache(archetype.MustacheName);
			SetSelectedChops(archetype.ChopsName);
			SetSelectedBeard(archetype.BeardName);
			SetSelectedHairColor(playerProfile.HairColor);
		}
		ResetLocks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetLocks()
	{
		lockedBeards = (lockedChops = (lockedEyeColors = (lockedGenders = (lockedHairColors = (lockedHairs = (lockedMustaches = (lockedRace = (lockedVariants = false))))))));
		SetLockButtonState(btnLockGender, isLocked: false);
		SetLockButtonState(btnLockRace, isLocked: false);
		SetLockButtonState(btnLockFace, isLocked: false);
		SetLockButtonState(btnLockEyeColor, isLocked: false);
		SetLockButtonState(btnLockHairStyle, isLocked: false);
		SetLockButtonState(btnLockHairColor, isLocked: false);
		SetLockButtonState(btnLockMustaches, isLocked: false);
		SetLockButtonState(btnLockChops, isLocked: false);
		SetLockButtonState(btnLockBeards, isLocked: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPress(XUiController _sender, int _mouseButton)
	{
		if (HasChanges)
		{
			if (playerProfile != null)
			{
				ProfileSDF.SaveProfile(ProfileSDF.CurrentProfileName(), playerProfile.ProfileArchetype, playerProfile.IsMale, playerProfile.RaceName, playerProfile.VariantNumber, playerProfile.EyeColor, playerProfile.HairName, playerProfile.HairColor, playerProfile.MustacheName, playerProfile.ChopsName, playerProfile.BeardName);
			}
			else if (archetype != null)
			{
				Archetype.SetArchetype(archetype);
				Archetype.SaveArchetypesToFile();
			}
			HasChanges = false;
		}
		OpenOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPress(XUiController _sender, int _mouseButton)
	{
		OpenOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRandomize_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!lockedGenders)
		{
			genders.SelectedIndex = gr.RandomRange(2);
			genders.TriggerValueChangedEvent(genders.Elements[0]);
		}
		if (!lockedRace)
		{
			races.SelectedIndex = gr.RandomRange(races.Elements.Count);
			races.TriggerValueChangedEvent(races.Elements[0]);
		}
		if (!lockedVariants)
		{
			variants.SelectedIndex = gr.RandomRange(variants.Elements.Count);
			variants.TriggerValueChangedEvent(variants.Elements[0]);
		}
		if (!lockedEyeColors)
		{
			eyeColors.SelectedIndex = gr.RandomRange(eyeColors.Elements.Count);
			eyeColors.TriggerValueChangedEvent(eyeColors.Elements[0]);
		}
		if (!lockedHairs)
		{
			hairs.SelectedIndex = gr.RandomRange(hairs.Elements.Count);
			hairs.TriggerValueChangedEvent(hairs.Elements[0]);
		}
		if (!lockedHairColors)
		{
			hairColors.SelectedIndex = gr.RandomRange(hairColors.Elements.Count);
			hairColors.TriggerValueChangedEvent(hairColors.Elements[0]);
		}
		if (!lockedMustaches)
		{
			mustaches.SelectedIndex = gr.RandomRange(mustaches.Elements.Count);
			mustaches.TriggerValueChangedEvent(mustaches.Elements[0]);
		}
		if (!lockedChops)
		{
			chops.SelectedIndex = gr.RandomRange(chops.Elements.Count);
			chops.TriggerValueChangedEvent(chops.Elements[0]);
		}
		if (!lockedBeards)
		{
			beards.SelectedIndex = gr.RandomRange(beards.Elements.Count);
			beards.TriggerValueChangedEvent(beards.Elements[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenOptions()
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsProfiles.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupGenders()
	{
		genders.Elements.Clear();
		genders.Elements.Add(new NameData("female", Localization.Get("xuiBoolMaleOff")));
		genders.Elements.Add(new NameData("male", Localization.Get("xuiBoolMaleOn")));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupRaces(bool isMale)
	{
		races.Elements.Clear();
		int num = 1;
		foreach (string race in SDCSDataUtils.GetRaceList(isMale))
		{
			races.Elements.Add(new NameData(race, Localization.Get("xuiRace" + race.ToLower())));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupVariants(bool isMale, string raceName)
	{
		variants.Elements.Clear();
		int num = 1;
		foreach (string variant in SDCSDataUtils.GetVariantList(isMale, raceName))
		{
			variants.Elements.Add(new NameData(variant, Localization.Get("lblFace") + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupEyeColors()
	{
		eyeColors.Elements.Clear();
		int num = 1;
		foreach (string eyeColorName in SDCSDataUtils.GetEyeColorNames())
		{
			eyeColors.Elements.Add(new NameData(eyeColorName, Localization.Get("xuiCharacterColorSlotEyes") + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHairStyles(bool isMale)
	{
		hairs.Elements.Clear();
		hairs.Elements.Add(new NameData("", Localization.Get("xuiCharacterHairStyle") + " 00"));
		int num = 1;
		foreach (string hairName in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Hair))
		{
			hairs.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterHairStyle") + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHairColors()
	{
		hairColors.Elements.Clear();
		int num = 1;
		foreach (SDCSDataUtils.HairColorData hairColorName in SDCSDataUtils.GetHairColorNames())
		{
			hairColors.Elements.Add(new NameData(hairColorName.PrefabName, Localization.Get("xuiCharacterHairColor") + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMustaches(bool isMale)
	{
		mustaches.Elements.Clear();
		mustaches.Elements.Add(new NameData("", Localization.Get("xuiCharacterMustaches") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Mustache))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			mustaches.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterMustaches") + " " + Localization.Get(text)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupChops(bool isMale)
	{
		chops.Elements.Clear();
		chops.Elements.Add(new NameData("", Localization.Get("xuiCharacterChops") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Chops))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			chops.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterChops") + " " + Localization.Get(text)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupBeards(bool isMale)
	{
		beards.Elements.Clear();
		beards.Elements.Add(new NameData("", Localization.Get("xuiCharacterBeards") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Beard))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			beards.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterBeards") + " " + Localization.Get(text)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Genders_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.Sex = internalName;
		bool isMale = internalName == "male";
		if (playerProfile != null)
		{
			playerProfile.IsMale = isMale;
		}
		string internalName2 = races.Value.InternalName;
		int variant = StringParsers.ParseSInt32(variants.Value.InternalName);
		SetupRaces(isMale);
		SetSelectedRace(internalName2, applyToPreview: true);
		SetupVariants(isMale, races.Value.InternalName);
		SetSelectedVariant(variant, applyToPreview: true);
		SetupHairStyles(isMale);
		SetupMustaches(isMale);
		SetupChops(isMale);
		SetupBeards(isMale);
		SetSelectedMustache("", applyToPreview: true);
		SetSelectedChops("", applyToPreview: true);
		SetSelectedBeard("", applyToPreview: true);
		RefreshBindings();
		previewWindow.MakePreview();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Races_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.Race = internalName;
		if (playerProfile != null)
		{
			playerProfile.RaceName = internalName;
		}
		int variant = StringParsers.ParseSInt32(variants.Value.InternalName);
		SetupVariants(previewWindow.Archetype.IsMale, races.Value.InternalName);
		SetSelectedVariant(variant, applyToPreview: true);
		previewWindow.MakePreview();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Variants_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		int num = StringParsers.ParseSInt32(_newValue.InternalName);
		previewWindow.Archetype.Variant = num;
		if (playerProfile != null)
		{
			playerProfile.VariantNumber = num;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Hairs_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.Hair = internalName;
		previewWindow.Archetype.HairColor = hairColors.Value.InternalName;
		if (playerProfile != null)
		{
			playerProfile.HairName = internalName;
			playerProfile.HairColor = hairColors.Value.InternalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HairColors_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.HairColor = internalName;
		if (playerProfile != null)
		{
			playerProfile.HairColor = internalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EyeColors_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.EyeColorName = internalName;
		if (playerProfile != null)
		{
			playerProfile.EyeColor = internalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToEye();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Mustaches_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.MustacheName = internalName;
		if (playerProfile != null)
		{
			playerProfile.MustacheName = internalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Chops_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.ChopsName = internalName;
		if (playerProfile != null)
		{
			playerProfile.ChopsName = internalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Beards_OnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.BeardName = internalName;
		if (playerProfile != null)
		{
			playerProfile.BeardName = internalName;
		}
		previewWindow.MakePreview();
		previewWindow.ZoomToHead();
		HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedRace(string raceName, bool applyToPreview = false)
	{
		if (raceName == "")
		{
			races.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < races.Elements.Count; i++)
			{
				if (races.Elements[i].InternalName.EqualsCaseInsensitive(raceName))
				{
					num = (races.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				races.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.Race = races.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.RaceName = previewWindow.Archetype.Race;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedVariant(int variant, bool applyToPreview = false)
	{
		if (variant == -1)
		{
			variants.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < variants.Elements.Count; i++)
			{
				if (StringParsers.ParseSInt32(variants.Elements[i].InternalName) == variant)
				{
					num = (variants.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				variants.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.Variant = StringParsers.ParseSInt32(variants.Value.InternalName);
			if (playerProfile != null)
			{
				playerProfile.VariantNumber = previewWindow.Archetype.Variant;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedEyeColor(string eyeColorName, bool applyToPreview = false)
	{
		if (eyeColorName == "")
		{
			eyeColors.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < eyeColors.Elements.Count; i++)
			{
				if (eyeColors.Elements[i].InternalName.EqualsCaseInsensitive(eyeColorName))
				{
					num = (eyeColors.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				eyeColors.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.EyeColorName = eyeColors.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.EyeColor = previewWindow.Archetype.EyeColorName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedHair(string hairName, bool applyToPreview = false)
	{
		if (hairName == "")
		{
			hairs.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < hairs.Elements.Count; i++)
			{
				if (hairs.Elements[i].InternalName.EqualsCaseInsensitive(hairName))
				{
					num = (hairs.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				hairs.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.Hair = hairs.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.HairName = previewWindow.Archetype.Hair;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedHairColor(string hairColorName, bool applyToPreview = false)
	{
		if (hairColorName == "")
		{
			hairColors.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < hairColors.Elements.Count; i++)
			{
				if (hairColors.Elements[i].InternalName.EqualsCaseInsensitive(hairColorName))
				{
					num = (hairColors.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				hairColors.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.HairColor = hairColors.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.HairColor = previewWindow.Archetype.HairColor;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedMustache(string mustacheName, bool applyToPreview = false)
	{
		if (mustacheName == "")
		{
			mustaches.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < mustaches.Elements.Count; i++)
			{
				if (mustaches.Elements[i].InternalName.EqualsCaseInsensitive(mustacheName))
				{
					num = (mustaches.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				mustaches.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.MustacheName = mustaches.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.MustacheName = previewWindow.Archetype.MustacheName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedChops(string chopsName, bool applyToPreview = false)
	{
		if (chopsName == "")
		{
			chops.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < chops.Elements.Count; i++)
			{
				if (chops.Elements[i].InternalName.EqualsCaseInsensitive(chopsName))
				{
					num = (chops.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				chops.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.ChopsName = chops.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.ChopsName = previewWindow.Archetype.ChopsName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedBeard(string beardName, bool applyToPreview = false)
	{
		if (beardName == "")
		{
			beards.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < beards.Elements.Count; i++)
			{
				if (beards.Elements[i].InternalName.EqualsCaseInsensitive(beardName))
				{
					num = (beards.SelectedIndex = i);
					return;
				}
			}
			if (num == -1)
			{
				beards.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			previewWindow.Archetype.BeardName = beards.Value.InternalName;
			if (playerProfile != null)
			{
				playerProfile.BeardName = previewWindow.Archetype.BeardName;
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (HasChanges && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			BtnApply_OnPress(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "isMale")
		{
			value = (genders != null && genders.SelectedIndex == 1).ToString();
			return true;
		}
		return false;
	}
}
