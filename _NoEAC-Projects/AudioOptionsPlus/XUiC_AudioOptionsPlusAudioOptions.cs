using System;
using UnityEngine.Scripting;
using AudioOptionsPlus;

[Preserve]
public class XUiC_AudioOptions : XUiController
{
    private const string PresetOff = "Off";
    private const string PresetLow = "Low";
    private const string PresetMedium = "Medium";
    private const string PresetHigh = "High";
    private const string PresetDefault = "Default";
    private const string PresetCustom = "Custom";

    private XUiC_ComboBoxList<string> _overallPreset;

    private XUiC_ComboBoxFloat _augerChainsaw;
    private XUiC_ComboBoxFloat _impactHarvestSurface;
    private XUiC_ComboBoxFloat _gunFire;
    private XUiC_ComboBoxFloat _explosions;
    private XUiC_ComboBoxFloat _vehiclesNoHorn;
    private XUiC_ComboBoxFloat _electricalBlockLoops;
    private XUiC_ComboBoxFloat _craftingCompleteUi;
    private XUiC_ComboBoxFloat _spiderZombie;
    private XUiC_ComboBoxFloat _animalPainDeath;
    private XUiC_ComboBoxFloat _placeUpgradeRepair;
    private XUiC_ComboBoxFloat _protectionDings;
    private XUiC_ComboBoxFloat _interactionPrompts;
    private XUiC_ComboBoxFloat _twitch;
    private XUiC_ComboBoxFloat _doorsHatchesVaultsCellarsBridge;
    private XUiC_ComboBoxFloat _traderBob;
    private XUiC_ComboBoxFloat _traderHugh;
    private XUiC_ComboBoxFloat _traderJen;
    private XUiC_ComboBoxFloat _traderJoel;
    private XUiC_ComboBoxFloat _traderRekt;
    private XUiC_ComboBoxFloat _playerMadeSounds;

    private bool _suppressEvents;
    private bool _isReady;
    private string _lastObservedPreset = PresetCustom;

    public override void Init()
    {
        base.Init();

        ResolveControls();

        if (_overallPreset != null)
        {
            _overallPreset.OnValueChanged += OnPresetChanged;
            _overallPreset.OnValueChangedGeneric += OnPresetChangedGeneric;
        }
        else
        {
            Console.WriteLine("[AudioOptionsPlus] Profile control not found (AOPVolumeProfilesOverallVolumePreset).");
        }

        foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
        {
            if (combo != null)
            {
                combo.OnValueChanged += OnCategoryValueChanged;
            }
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        _isReady = false;
        RefreshFromConfig();
        _isReady = true;
        _lastObservedPreset = _overallPreset?.Value ?? PresetCustom;
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);

        if (!_isReady || _suppressEvents || _overallPreset == null)
        {
            return;
        }

        string currentPreset = _overallPreset.Value ?? PresetCustom;
        if (currentPreset == _lastObservedPreset)
        {
            return;
        }

        _lastObservedPreset = currentPreset;
        ApplyPresetSelection(currentPreset);
    }

    private void OnCategoryValueChanged(XUiController _sender, double _oldValue, double _newValue)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        AudioOptionsPlusConfig.SetUiVolumeOverrides(
            GetValue(_augerChainsaw),
            GetValue(_impactHarvestSurface),
            GetValue(_gunFire),
            GetValue(_explosions),
            GetValue(_vehiclesNoHorn),
            GetValue(_electricalBlockLoops),
            GetValue(_craftingCompleteUi),
            GetValue(_spiderZombie),
            GetValue(_animalPainDeath),
            GetValue(_placeUpgradeRepair),
            GetValue(_protectionDings),
            GetValue(_interactionPrompts),
            GetValue(_twitch),
            GetValue(_doorsHatchesVaultsCellarsBridge),
            GetValue(_traderBob),
            GetValue(_traderHugh),
            GetValue(_traderJen),
            GetValue(_traderJoel),
            GetValue(_traderRekt),
            GetValue(_playerMadeSounds));

        if (_overallPreset != null)
        {
            string selectedPreset = _overallPreset.Value ?? PresetCustom;
            if (selectedPreset != PresetCustom)
            {
                float presetValue = GetPresetValue(selectedPreset);
                if (!AllValuesMatch(presetValue))
                {
                    _suppressEvents = true;
                    _overallPreset.Value = PresetCustom;
                    _suppressEvents = false;
                }
            }
        }
    }

    private void OnPresetChanged(XUiController _sender, string _oldValue, string _newValue)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        string preset = NormalizePreset(_newValue);
        _lastObservedPreset = preset;
        ApplyPresetSelection(preset);
    }

    private void OnPresetChangedGeneric(XUiController _sender)
    {
        if (_suppressEvents || !_isReady || _overallPreset == null)
        {
            return;
        }

        string preset = NormalizePreset(_overallPreset.Value);
        if (preset == _lastObservedPreset)
        {
            return;
        }

        _lastObservedPreset = preset;
        ApplyPresetSelection(preset);
    }

    private void ApplyPresetSelection(string preset)
    {
        EnsureControlsResolved();

        if (string.Equals(preset, PresetCustom, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        float presetValue = GetPresetValue(preset);

        _suppressEvents = true;
        try
        {
            foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
            {
                Set(combo, presetValue);
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        AudioOptionsPlusConfig.SetUiVolumeOverrides(
            presetValue, presetValue, presetValue, presetValue, presetValue,
            presetValue, presetValue, presetValue, presetValue, presetValue,
            presetValue, presetValue, presetValue, presetValue, presetValue,
            presetValue, presetValue, presetValue, presetValue, presetValue);

        RefreshBindingsSelfAndChildren();
        Console.WriteLine("[AudioOptionsPlus] Applied profile: " + preset + " (" + presetValue.ToString("0.##") + ")");
    }

    private void RefreshFromConfig()
    {
        _suppressEvents = true;
        try
        {
            AudioOptionsPlusConfig.Load();

            Set(_augerChainsaw, AudioOptionsPlusConfig.AugerChainsawMultiplier);
            Set(_impactHarvestSurface, AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier);
            Set(_gunFire, AudioOptionsPlusConfig.GunFireMultiplier);
            Set(_explosions, AudioOptionsPlusConfig.ExplosionsMultiplier);
            Set(_vehiclesNoHorn, AudioOptionsPlusConfig.VehiclesNoHornMultiplier);
            Set(_electricalBlockLoops, AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier);
            Set(_craftingCompleteUi, AudioOptionsPlusConfig.CraftingCompleteUiMultiplier);
            Set(_spiderZombie, AudioOptionsPlusConfig.SpiderZombieMultiplier);
            Set(_animalPainDeath, AudioOptionsPlusConfig.AnimalPainDeathMultiplier);
            Set(_placeUpgradeRepair, AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier);
            Set(_protectionDings, AudioOptionsPlusConfig.ProtectionDingsMultiplier);
            Set(_interactionPrompts, AudioOptionsPlusConfig.InteractionPromptsMultiplier);
            Set(_twitch, AudioOptionsPlusConfig.TwitchMultiplier);
            Set(_doorsHatchesVaultsCellarsBridge, AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier);
            Set(_traderBob, AudioOptionsPlusConfig.TraderBobMultiplier);
            Set(_traderHugh, AudioOptionsPlusConfig.TraderHughMultiplier);
            Set(_traderJen, AudioOptionsPlusConfig.TraderJenMultiplier);
            Set(_traderJoel, AudioOptionsPlusConfig.TraderJoelMultiplier);
            Set(_traderRekt, AudioOptionsPlusConfig.TraderRektMultiplier);
            Set(_playerMadeSounds, AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier);

            if (_overallPreset != null)
            {
                _overallPreset.Value = ResolvePresetFromValues();
            }
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private string ResolvePresetFromValues()
    {
        if (AllValuesMatch(0f))
        {
            return PresetOff;
        }

        if (AllValuesMatch(0.25f))
        {
            return PresetLow;
        }

        if (AllValuesMatch(0.5f))
        {
            return PresetMedium;
        }

        if (AllValuesMatch(0.75f))
        {
            return PresetHigh;
        }

        if (AllValuesMatch(1f))
        {
            return PresetDefault;
        }

        return PresetCustom;
    }

    private bool AllValuesMatch(float expected)
    {
        foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
        {
            if (!Nearly((float)(combo?.Value ?? 1.0), expected))
            {
                return false;
            }
        }

        return true;
    }

    private XUiC_ComboBoxFloat[] GetCategoryCombos()
    {
        return new[]
        {
            _augerChainsaw,
            _impactHarvestSurface,
            _gunFire,
            _explosions,
            _vehiclesNoHorn,
            _electricalBlockLoops,
            _craftingCompleteUi,
            _spiderZombie,
            _animalPainDeath,
            _placeUpgradeRepair,
            _protectionDings,
            _interactionPrompts,
            _twitch,
            _doorsHatchesVaultsCellarsBridge,
            _traderBob,
            _traderHugh,
            _traderJen,
            _traderJoel,
            _traderRekt,
            _playerMadeSounds
        };
    }

    private static float GetValue(XUiC_ComboBoxFloat combo)
    {
        return (float)(combo?.Value ?? 1.0);
    }

    private static bool Nearly(float a, float b)
    {
        return Math.Abs(a - b) <= 0.001f;
    }

    private static float GetPresetValue(string preset)
    {
        switch (preset)
        {
            case PresetOff:
                return 0f;
            case PresetLow:
                return 0.25f;
            case PresetMedium:
                return 0.5f;
            case PresetHigh:
                return 0.75f;
            default:
                return 1f;
        }
    }

    private static string NormalizePreset(string raw)
    {
        string preset = (raw ?? PresetCustom).Trim();
        if (preset.Length == 0)
        {
            return PresetCustom;
        }

        if (string.Equals(preset, PresetOff, StringComparison.OrdinalIgnoreCase)) return PresetOff;
        if (string.Equals(preset, PresetLow, StringComparison.OrdinalIgnoreCase)) return PresetLow;
        if (string.Equals(preset, PresetMedium, StringComparison.OrdinalIgnoreCase)) return PresetMedium;
        if (string.Equals(preset, PresetHigh, StringComparison.OrdinalIgnoreCase)) return PresetHigh;
        if (string.Equals(preset, PresetDefault, StringComparison.OrdinalIgnoreCase)) return PresetDefault;
        if (string.Equals(preset, PresetCustom, StringComparison.OrdinalIgnoreCase)) return PresetCustom;

        return PresetCustom;
    }

    private static void Set(XUiC_ComboBoxFloat combo, float value)
    {
        if (combo == null)
        {
            return;
        }

        combo.Value = value < 0f ? 1f : value;
    }

    private void EnsureControlsResolved()
    {
        if (_overallPreset == null)
        {
            ResolveControls();
            return;
        }

        foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
        {
            if (combo == null)
            {
                ResolveControls();
                return;
            }
        }
    }

    private void ResolveControls()
    {
        _overallPreset = ResolveStringListCombo("AOPVolumeProfilesOverallVolumePreset");
        _augerChainsaw = ResolveFloatCombo("AOPVolumeProfilesMotorTools");
        _impactHarvestSurface = ResolveFloatCombo("AOPVolumeProfilesSurfaceImpact");
        _gunFire = ResolveFloatCombo("AOPVolumeProfilesGunFire");
        _explosions = ResolveFloatCombo("AOPVolumeProfilesExplosions");
        _vehiclesNoHorn = ResolveFloatCombo("AOPVolumeProfilesVehicles");
        _electricalBlockLoops = ResolveFloatCombo("AOPVolumeProfilesElectrical");
        _craftingCompleteUi = ResolveFloatCombo("AOPVolumeProfilesCraftingCompleteSound");
        _spiderZombie = ResolveFloatCombo("AOPVolumeProfilesSpiderZombie");
        _animalPainDeath = ResolveFloatCombo("AOPVolumeProfilesAnimalPainDeath");
        _placeUpgradeRepair = ResolveFloatCombo("AOPVolumeProfilesBlockBuilding");
        _protectionDings = ResolveFloatCombo("AOPVolumeProfilesProtectedBlockDings");
        _interactionPrompts = ResolveFloatCombo("AOPVolumeProfilesItemInteractionSounds");
        _twitch = ResolveFloatCombo("AOPVolumeProfilesTwitchSounds");
        _doorsHatchesVaultsCellarsBridge = ResolveFloatCombo("AOPVolumeProfilesAllDoorTypes");
        _traderBob = ResolveFloatCombo("AOPVolumeProfilesTraderBob");
        _traderHugh = ResolveFloatCombo("AOPVolumeProfilesTraderHugh");
        _traderJen = ResolveFloatCombo("AOPVolumeProfilesTraderJen");
        _traderJoel = ResolveFloatCombo("AOPVolumeProfilesTraderJoel");
        _traderRekt = ResolveFloatCombo("AOPVolumeProfilesTraderRekt");
        _playerMadeSounds = ResolveFloatCombo("AOPVolumeProfilesPlayerCharacterSounds");

        LogMissingControl("AOPVolumeProfilesOverallVolumePreset", _overallPreset);
        LogMissingControl("AOPVolumeProfilesMotorTools", _augerChainsaw);
        LogMissingControl("AOPVolumeProfilesSurfaceImpact", _impactHarvestSurface);
        LogMissingControl("AOPVolumeProfilesGunFire", _gunFire);
        LogMissingControl("AOPVolumeProfilesExplosions", _explosions);
        LogMissingControl("AOPVolumeProfilesVehicles", _vehiclesNoHorn);
        LogMissingControl("AOPVolumeProfilesElectrical", _electricalBlockLoops);
        LogMissingControl("AOPVolumeProfilesCraftingCompleteSound", _craftingCompleteUi);
        LogMissingControl("AOPVolumeProfilesSpiderZombie", _spiderZombie);
        LogMissingControl("AOPVolumeProfilesAnimalPainDeath", _animalPainDeath);
        LogMissingControl("AOPVolumeProfilesBlockBuilding", _placeUpgradeRepair);
        LogMissingControl("AOPVolumeProfilesProtectedBlockDings", _protectionDings);
        LogMissingControl("AOPVolumeProfilesItemInteractionSounds", _interactionPrompts);
        LogMissingControl("AOPVolumeProfilesTwitchSounds", _twitch);
        LogMissingControl("AOPVolumeProfilesAllDoorTypes", _doorsHatchesVaultsCellarsBridge);
        LogMissingControl("AOPVolumeProfilesTraderBob", _traderBob);
        LogMissingControl("AOPVolumeProfilesTraderHugh", _traderHugh);
        LogMissingControl("AOPVolumeProfilesTraderJen", _traderJen);
        LogMissingControl("AOPVolumeProfilesTraderJoel", _traderJoel);
        LogMissingControl("AOPVolumeProfilesTraderRekt", _traderRekt);
        LogMissingControl("AOPVolumeProfilesPlayerCharacterSounds", _playerMadeSounds);
    }

    private XUiC_ComboBoxList<string> ResolveStringListCombo(string id)
    {
        XUiController byId = GetChildById(id);
        XUiC_ComboBoxList<string> resolved = (byId as XUiC_ComboBoxList<string>) ?? byId?.GetChildByType<XUiC_ComboBoxList<string>>();
        if (resolved != null)
        {
            return resolved;
        }

        XUiC_ComboBoxList<string>[] all = GetChildrenByType<XUiC_ComboBoxList<string>>();
        for (int i = 0; i < all.Length; i++)
        {
            string candidateId = all[i]?.ViewComponent?.ID;
            if (string.Equals(candidateId, id, StringComparison.OrdinalIgnoreCase))
            {
                return all[i];
            }
        }

        return null;
    }

    private XUiC_ComboBoxFloat ResolveFloatCombo(string id)
    {
        XUiController byId = GetChildById(id);
        XUiC_ComboBoxFloat resolved = (byId as XUiC_ComboBoxFloat) ?? byId?.GetChildByType<XUiC_ComboBoxFloat>();
        if (resolved != null)
        {
            return resolved;
        }

        XUiC_ComboBoxFloat[] all = GetChildrenByType<XUiC_ComboBoxFloat>();
        for (int i = 0; i < all.Length; i++)
        {
            string candidateId = all[i]?.ViewComponent?.ID;
            if (string.Equals(candidateId, id, StringComparison.OrdinalIgnoreCase))
            {
                return all[i];
            }
        }

        return null;
    }

    private static void LogMissingControl(string id, object control)
    {
        if (control == null)
        {
            Console.WriteLine("[AudioOptionsPlus] Missing control: " + id);
        }
    }
}
