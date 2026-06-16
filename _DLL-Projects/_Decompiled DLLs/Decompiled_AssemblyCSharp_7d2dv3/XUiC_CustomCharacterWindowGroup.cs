using System;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_CustomCharacterWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct NameData(string _internalName, string _formattedName)
	{
		public readonly string InternalName = _internalName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string formattedName = _formattedName;

		public override string ToString()
		{
			return formattedName;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasChanges;

	public PlayerProfile PlayerProfile;

	public Archetype Archetype;

	[XuiBindComponent("cbxRace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxRace;

	[XuiBindComponent("cbxGender", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxGender;

	[XuiBindComponent("cbxEyeColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxEyeColor;

	[XuiBindComponent("cbxHairStyle", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxHairStyle;

	[XuiBindComponent("cbxHairColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxHairColor;

	[XuiBindComponent("cbxMustaches", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxMustaches;

	[XuiBindComponent("cbxChops", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxChops;

	[XuiBindComponent("cbxBeards", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxBeards;

	[XuiBindComponent("cbxFace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<NameData> cbxVariant;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SDCSPreviewWindow previewWindow;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnApply", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnApply;

	[XuiBindComponent("btnRandomize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnRandomize;

	[XuiBindComponent("btnLockRace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockRace;

	[XuiBindComponent("btnLockGender", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockGender;

	[XuiBindComponent("btnLockFace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockFace;

	[XuiBindComponent("btnLockEyeColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockEyeColor;

	[XuiBindComponent("btnLockHairStyle", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockHairStyle;

	[XuiBindComponent("btnLockHairColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockHairColor;

	[XuiBindComponent("btnLockMustaches", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockMustaches;

	[XuiBindComponent("btnLockChops", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockChops;

	[XuiBindComponent("btnLockBeards", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom gr;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClosed;

	[XuiXmlBinding("ismale")]
	public bool IsMale
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (cbxGender != null)
			{
				return cbxGender.SelectedIndex == 1;
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		gr = GameRandomManager.Instance.CreateGameRandom();
		SDCSDataUtils.SetupData();
	}

	public override void OnOpen()
	{
		windowGroup.openWindowOnEsc = XUiC_PlayerProfile.ID;
		base.OnOpen();
		PlayerProfile = null;
		Archetype = null;
		Archetype = Archetype.GetArchetype(ProfileSDF.CurrentProfileName());
		if (Archetype != null)
		{
			Archetype = Archetype.Clone();
		}
		else
		{
			string profileName = ProfileSDF.CurrentProfileName();
			PlayerProfile = PlayerProfile.LoadProfile(profileName).Clone();
		}
		setInitialOptions();
	}

	public override void OnClose()
	{
		base.OnClose();
		onClosed?.Invoke();
		onClosed = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (!XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			return;
		}
		if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				BtnApply_OnPress(null, 0);
			});
		}
		if (xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				BtnBack_OnPress(null, 0);
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setInitialOptions()
	{
		if (PlayerProfile != null)
		{
			setupGenders();
			cbxGender.SelectedIndex = (PlayerProfile.IsMale ? 1 : 0);
			setupRaces(PlayerProfile.IsMale);
			setupVariants(PlayerProfile.IsMale, PlayerProfile.RaceName);
			setupHairStyles(PlayerProfile.IsMale);
			setupMustaches(PlayerProfile.IsMale);
			setupChops(PlayerProfile.IsMale);
			setupBeards(PlayerProfile.IsMale);
			setupEyeColors();
			setupHairColors();
			setSelectedRace(PlayerProfile.RaceName);
			setSelectedVariant(PlayerProfile.VariantNumber);
			setSelectedGeneric(PlayerProfile.EyeColor, cbxEyeColor, applyEyeColor);
			setSelectedGeneric(PlayerProfile.HairName, cbxHairStyle, applyHairStyle);
			setSelectedGeneric(PlayerProfile.MustacheName, cbxMustaches, applyMustache);
			setSelectedGeneric(PlayerProfile.ChopsName, cbxChops, applyChops);
			setSelectedGeneric(PlayerProfile.BeardName, cbxBeards, applyBeard);
			setSelectedGeneric(PlayerProfile.HairColor, cbxHairColor, applyHairColor);
			resetLocks();
		}
		else
		{
			Log.Error("PlayerProfile is null");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetLocks()
	{
		btnLockRace.IsSelected = true;
		btnLockGender.IsSelected = true;
		btnLockFace.IsSelected = false;
		btnLockEyeColor.IsSelected = false;
		btnLockHairStyle.IsSelected = false;
		btnLockHairColor.IsSelected = false;
		btnLockMustaches.IsSelected = false;
		btnLockChops.IsSelected = false;
		btnLockBeards.IsSelected = false;
	}

	[XuiBindEvent("OnPress", "btnApply")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPress(XUiController _sender, int _mouseButton)
	{
		if (hasChanges)
		{
			if (PlayerProfile != null)
			{
				ProfileSDF.SaveProfile(ProfileSDF.CurrentProfileName(), PlayerProfile.ProfileArchetype, PlayerProfile.IsMale, PlayerProfile.RaceName, PlayerProfile.VariantNumber, PlayerProfile.EyeColor, PlayerProfile.HairName, PlayerProfile.HairColor, PlayerProfile.MustacheName, PlayerProfile.ChopsName, PlayerProfile.BeardName);
			}
			else if (Archetype != null)
			{
				Archetype.SetArchetype(Archetype);
				Archetype.SaveArchetypesToFile();
			}
			hasChanges = false;
		}
		close();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPress(XUiController _sender, int _mouseButton)
	{
		close();
	}

	[XuiBindEvent("OnPress", "btnRandomize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRandomize_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnLockGender.IsSelected)
		{
			cbxGender.SelectedIndex = gr.RandomRange(2);
			cbxGender.TriggerValueChangedEvent(cbxGender.Elements[0]);
		}
		if (!btnLockRace.IsSelected)
		{
			cbxRace.SelectedIndex = gr.RandomRange(cbxRace.Elements.Count);
			cbxRace.TriggerValueChangedEvent(cbxRace.Elements[0]);
		}
		if (!btnLockFace.IsSelected)
		{
			cbxVariant.SelectedIndex = gr.RandomRange(cbxVariant.Elements.Count);
			cbxVariant.TriggerValueChangedEvent(cbxVariant.Elements[0]);
		}
		if (!btnLockEyeColor.IsSelected)
		{
			cbxEyeColor.SelectedIndex = gr.RandomRange(cbxEyeColor.Elements.Count);
			cbxEyeColor.TriggerValueChangedEvent(cbxEyeColor.Elements[0]);
		}
		if (!btnLockHairStyle.IsSelected)
		{
			cbxHairStyle.SelectedIndex = gr.RandomRange(cbxHairStyle.Elements.Count);
			cbxHairStyle.TriggerValueChangedEvent(cbxHairStyle.Elements[0]);
		}
		if (!btnLockHairColor.IsSelected)
		{
			cbxHairColor.SelectedIndex = gr.RandomRange(cbxHairColor.Elements.Count);
			cbxHairColor.TriggerValueChangedEvent(cbxHairColor.Elements[0]);
		}
		if (!btnLockMustaches.IsSelected)
		{
			cbxMustaches.SelectedIndex = gr.RandomRange(cbxMustaches.Elements.Count);
			cbxMustaches.TriggerValueChangedEvent(cbxMustaches.Elements[0]);
		}
		if (!btnLockChops.IsSelected)
		{
			cbxChops.SelectedIndex = gr.RandomRange(cbxChops.Elements.Count);
			cbxChops.TriggerValueChangedEvent(cbxChops.Elements[0]);
		}
		if (!btnLockBeards.IsSelected)
		{
			cbxBeards.SelectedIndex = gr.RandomRange(cbxBeards.Elements.Count);
			cbxBeards.TriggerValueChangedEvent(cbxBeards.Elements[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(bool _zoomToHead = true)
	{
		previewWindow.MakePreview();
		if (_zoomToHead)
		{
			previewWindow.ZoomToHead();
		}
		hasChanges = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedGeneric(string _name, XUiC_ComboBoxList<NameData> _combo, Action<string> _applyFunc, bool _applyToPreview = false)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < _combo.Elements.Count; i++)
			{
				if (_combo.Elements[i].InternalName.EqualsCaseInsensitive(_name))
				{
					_combo.SelectedIndex = i;
					return;
				}
			}
		}
		_combo.SelectedIndex = 0;
		if (_applyToPreview)
		{
			_applyFunc(_combo.Value.InternalName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupGenders()
	{
		cbxGender.Elements.Clear();
		cbxGender.Elements.Add(new NameData("female", Localization.Get("xuiBoolMaleOff")));
		cbxGender.Elements.Add(new NameData("male", Localization.Get("xuiBoolMaleOn")));
	}

	[XuiBindEvent("OnValueChanged", "cbxGender")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxGenderOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		previewWindow.Archetype.Sex = internalName;
		bool isMale = internalName == "male";
		if (PlayerProfile != null)
		{
			PlayerProfile.IsMale = isMale;
		}
		string internalName2 = cbxRace.Value.InternalName;
		int variant = StringParsers.ParseSInt32(cbxVariant.Value.InternalName);
		setupRaces(isMale);
		setSelectedRace(internalName2, _applyToPreview: true);
		setupVariants(isMale, cbxRace.Value.InternalName);
		setSelectedVariant(variant, _applyToPreview: true);
		setupHairStyles(isMale);
		setupMustaches(isMale);
		setupChops(isMale);
		setupBeards(isMale);
		setSelectedGeneric("", cbxMustaches, applyMustache, _applyToPreview: true);
		setSelectedGeneric("", cbxChops, applyChops, _applyToPreview: true);
		setSelectedGeneric("", cbxBeards, applyBeard, _applyToPreview: true);
		updatePreview(_zoomToHead: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupRaces(bool _isMale)
	{
		cbxRace.Elements.Clear();
		foreach (string race in SDCSDataUtils.GetRaceList(_isMale))
		{
			cbxRace.Elements.Add(new NameData(race, Localization.Get("xuiRace" + race.ToLower())));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedRace(string _raceName, bool _applyToPreview = false)
	{
		if (!string.IsNullOrEmpty(_raceName))
		{
			for (int i = 0; i < cbxRace.Elements.Count; i++)
			{
				if (cbxRace.Elements[i].InternalName.EqualsCaseInsensitive(_raceName))
				{
					cbxRace.SelectedIndex = i;
					return;
				}
			}
		}
		cbxRace.SelectedIndex = 0;
		if (_applyToPreview)
		{
			applyRace(cbxRace.Value.InternalName);
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxRace")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxRaceOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyRace(_newValue.InternalName);
		int variant = StringParsers.ParseSInt32(cbxVariant.Value.InternalName);
		setupVariants(previewWindow.Archetype.IsMale, cbxRace.Value.InternalName);
		setSelectedVariant(variant, _applyToPreview: true);
		updatePreview(_zoomToHead: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyRace(string _name)
	{
		previewWindow.Archetype.Race = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.RaceName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupVariants(bool _isMale, string _raceName)
	{
		cbxVariant.Elements.Clear();
		int num = 1;
		foreach (string variant in SDCSDataUtils.GetVariantList(_isMale, _raceName))
		{
			cbxVariant.Elements.Add(new NameData(variant, string.Format("{0} {1:00}", Localization.Get("lblFace"), num)));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedVariant(int _variant, bool _applyToPreview = false)
	{
		if (_variant != -1)
		{
			for (int i = 0; i < cbxVariant.Elements.Count; i++)
			{
				if (StringParsers.ParseSInt32(cbxVariant.Elements[i].InternalName) == _variant)
				{
					cbxVariant.SelectedIndex = i;
					return;
				}
			}
		}
		cbxVariant.SelectedIndex = 0;
		if (_applyToPreview)
		{
			applyVariant(StringParsers.ParseSInt32(cbxVariant.Value.InternalName));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxVariant")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxVariantOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyVariant(StringParsers.ParseSInt32(_newValue.InternalName));
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyVariant(int _variant)
	{
		previewWindow.Archetype.Variant = _variant;
		if (PlayerProfile != null)
		{
			PlayerProfile.VariantNumber = _variant;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupEyeColors()
	{
		cbxEyeColor.Elements.Clear();
		int num = 1;
		foreach (string eyeColorName in SDCSDataUtils.GetEyeColorNames())
		{
			cbxEyeColor.Elements.Add(new NameData(eyeColorName, string.Format("{0} {1:00}", Localization.Get("xuiCharacterColorSlotEyes"), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxEyeColor")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxEyeColorOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyEyeColor(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyEyeColor(string _name)
	{
		previewWindow.Archetype.EyeColorName = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.EyeColor = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupHairStyles(bool _isMale)
	{
		cbxHairStyle.Elements.Clear();
		cbxHairStyle.Elements.Add(new NameData("", Localization.Get("xuiCharacterHairStyle") + " 00"));
		int num = 1;
		foreach (string hairName in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Hair))
		{
			cbxHairStyle.Elements.Add(new NameData(hairName, string.Format("{0} {1:00}", Localization.Get("xuiCharacterHairStyle"), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxHairStyle")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxHairsOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyHairStyle(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyHairStyle(string _name)
	{
		previewWindow.Archetype.Hair = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.HairName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupHairColors()
	{
		cbxHairColor.Elements.Clear();
		int num = 1;
		foreach (SDCSDataUtils.HairColorData hairColorName in SDCSDataUtils.GetHairColorNames())
		{
			cbxHairColor.Elements.Add(new NameData(hairColorName.PrefabName, string.Format("{0} {1:00}", Localization.Get("xuiCharacterHairColor"), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxHairColor")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxHairColorOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyHairColor(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyHairColor(string _name)
	{
		previewWindow.Archetype.HairColor = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.HairColor = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupMustaches(bool _isMale)
	{
		cbxMustaches.Elements.Clear();
		cbxMustaches.Elements.Add(new NameData("", Localization.Get("xuiCharacterMustaches") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Mustache))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			cbxMustaches.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterMustaches") + " " + Localization.Get(text)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxMustaches")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxMustachesOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyMustache(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyMustache(string _name)
	{
		previewWindow.Archetype.MustacheName = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.MustacheName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupChops(bool _isMale)
	{
		cbxChops.Elements.Clear();
		cbxChops.Elements.Add(new NameData("", Localization.Get("xuiCharacterChops") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Chops))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			cbxChops.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterChops") + " " + Localization.Get(text)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxChops")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxChopsOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyChops(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChops(string _name)
	{
		previewWindow.Archetype.ChopsName = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.ChopsName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupBeards(bool _isMale)
	{
		cbxBeards.Elements.Clear();
		cbxBeards.Elements.Add(new NameData("", Localization.Get("xuiCharacterBeards") + " 00"));
		foreach (string hairName in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Beard))
		{
			string text = hairName;
			if (text.Length == 1)
			{
				text = text.Insert(0, "0");
			}
			cbxBeards.Elements.Add(new NameData(hairName, Localization.Get("xuiCharacterBeards") + " " + Localization.Get(text)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxBeards")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxBeardsOnValueChanged(XUiController _sender, NameData _oldValue, NameData _newValue)
	{
		applyBeard(_newValue.InternalName);
		updatePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyBeard(string _name)
	{
		previewWindow.Archetype.BeardName = _name;
		if (PlayerProfile != null)
		{
			PlayerProfile.BeardName = _name;
		}
	}

	public static void Open(XUi _xui, Action _onClosed)
	{
		XUiC_CustomCharacterWindowGroup childByType = _xui.GetChildByType<XUiC_CustomCharacterWindowGroup>();
		childByType.onClosed = _onClosed;
		_xui.playerUI.windowManager.Open(childByType.WindowGroup, _bModal: false);
	}
}
