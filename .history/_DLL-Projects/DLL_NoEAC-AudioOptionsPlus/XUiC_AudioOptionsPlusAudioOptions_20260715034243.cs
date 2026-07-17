using System;
using System.Reflection;
using UnityEngine.Scripting;
using AudioOptionsPlus;
using System.Collections.Generic;

[Preserve]
public class XUiC_AudioOptions : XUiController
{
    private static readonly List<XUiC_AudioOptions> LiveControllers = new List<XUiC_AudioOptions>();
    private static readonly MethodInfo InvokeValueChangedMethod = typeof(XUiController).GetMethod("invokeValueChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly MethodInfo SetChangedMethod = typeof(XUiC_OptionsDialogBase).GetMethod("SetChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private const string PresetOff = "Off";
    private const string PresetLow = "Low";
    private const string PresetMedium = "Medium";
    private const string PresetHigh = "High";
    private const string PresetDefault = "Default";
    private const string PresetCustom = "Custom";
    private const string SwapDefault = "Default";
    private const string SwapSwapped = "Swapped";
    private const string SwapPresetNone = "None";
    private const string SwapPresetAll = "All";
    private const string SwapPresetCustom = "Custom";

    private XUiC_ComboBoxList<string> _overallPreset;
    private XUiC_ComboBoxList<string> _animalSwapPreset;
    private XUiC_ComboBoxList<string> _bearSwap;
    private XUiC_ComboBoxList<string> _boarSwap;
    private XUiC_ComboBoxList<string> _caninesSwap;
    private XUiC_ComboBoxList<string> _chickenSwap;
    private XUiC_ComboBoxList<string> _stagSwap;
    private XUiC_ComboBoxList<string> _mountainLionSwap;
    private XUiC_ComboBoxList<string> _rabbitSwap;
    private XUiC_ComboBoxList<string> _snakeSwap;
    private XUiC_ComboBoxList<string> _vultureSwap;

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
        RegisterInstance(this);

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

        if (_animalSwapPreset != null)
        {
            _animalSwapPreset.OnValueChanged += OnSwapPresetChanged;
            _animalSwapPreset.OnValueChangedGeneric += OnSwapPresetChangedGeneric;
        }

        foreach (XUiC_ComboBoxList<string> combo in GetAnimalSwapCombos())
        {
            if (combo != null)
            {
                combo.OnValueChanged += OnAnimalSwapChanged;
                combo.OnValueChangedGeneric += OnAnimalSwapChangedGeneric;
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

    public override void OnClose()
    {
        base.OnClose();
        UnregisterInstance(this);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        UnregisterInstance(this);
    }

    public static void ReloadAllOpenControllers()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.ReloadFromConfigForUi();
        }
    }

    public void ReloadFromConfigForUi()
    {
        _isReady = false;
        EnsureControlsResolved();
        RefreshFromConfig();
        _isReady = true;
        _lastObservedPreset = _overallPreset?.Value ?? PresetCustom;
        RefreshBindingsSelfAndChildren();
    }

    private static void RegisterInstance(XUiC_AudioOptions controller)
    {
        if (controller != null && !LiveControllers.Contains(controller))
        {
            LiveControllers.Add(controller);
        }
    }

    private static void UnregisterInstance(XUiC_AudioOptions controller)
    {
        if (controller != null)
        {
            LiveControllers.Remove(controller);
        }
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

        MarkOptionsEntryDirty(_sender);
    }

    private void OnSwapPresetChanged(XUiController _sender, string _oldValue, string _newValue)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        string preset = NormalizeSwapPreset(_newValue);
        if (string.Equals(preset, SwapPresetCustom, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool enable = string.Equals(preset, SwapPresetAll, StringComparison.OrdinalIgnoreCase);
        _suppressEvents = true;
        try
        {
            foreach (XUiC_ComboBoxList<string> combo in GetAnimalSwapCombos())
            {
                if (combo != null)
                {
                    combo.Value = enable ? SwapSwapped : SwapDefault;
                }
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        PersistSwapSettings();
        MarkOptionsEntryDirty(_sender);
    }

    private void OnSwapPresetChangedGeneric(XUiController _sender)
    {
        if (_suppressEvents || !_isReady || _animalSwapPreset == null)
        {
            return;
        }

        OnSwapPresetChanged(_sender, string.Empty, _animalSwapPreset.Value);
    }

    private void OnAnimalSwapChanged(XUiController _sender, string _oldValue, string _newValue)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        SyncSwapPresetFromAnimals();
        PersistSwapSettings();
        MarkOptionsEntryDirty(_sender);
    }

    private void OnAnimalSwapChangedGeneric(XUiController _sender)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        SyncSwapPresetFromAnimals();
        PersistSwapSettings();
        MarkOptionsEntryDirty(_sender);
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

        MarkOptionsEntryDirty(_overallPreset);

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

            if (_animalSwapPreset != null)
            {
                SetSwap(_bearSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBearMode));
                SetSwap(_boarSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBoarMode));
                SetSwap(_chickenSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapChickenMode));
                SetSwap(_rabbitSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapRabbitMode));
                SetSwap(_snakeSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapSnakeMode));
                SetSwap(_stagSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapStagMode));
                SetSwap(_vultureSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapVultureMode));
                SetSwap(_caninesSwap, ResolveCaninesSwapValue());
                SetSwap(_mountainLionSwap, ToSwapValue(AudioOptionsPlusConfig.SoundSwapMountainLionMode));
                _animalSwapPreset.Value = ResolveSwapPresetFromAnimals();
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

    private XUiC_ComboBoxList<string>[] GetAnimalSwapCombos()
    {
        return new[]
        {
            _bearSwap,
            _boarSwap,
            _caninesSwap,
            _chickenSwap,
            _stagSwap,
            _mountainLionSwap,
            _rabbitSwap,
            _snakeSwap,
            _vultureSwap
        };
    }

    private static string ToSwapValue(string mode)
    {
        return string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(mode), "Both", StringComparison.OrdinalIgnoreCase)
            ? SwapSwapped
            : SwapDefault;
    }

    private string ResolveCaninesSwapValue()
    {
        bool anyOn = string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapWolfMode), "Both", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapDireWolfMode), "Both", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapZombieDogMode), "Both", StringComparison.OrdinalIgnoreCase);
        return anyOn ? SwapSwapped : SwapDefault;
    }

    private static bool IsSwapOn(XUiC_ComboBoxList<string> combo)
    {
        return string.Equals(combo?.Value ?? SwapDefault, SwapSwapped, StringComparison.OrdinalIgnoreCase);
    }

    private static void SetSwap(XUiC_ComboBoxList<string> combo, string value)
    {
        if (combo != null)
        {
            combo.Value = value;
        }
    }

    private static void MarkOptionsEntryDirty(XUiController source)
    {
        if (source == null)
        {
            return;
        }

        XUiC_OptionsDialogBase optionsDialog = source.windowGroup?.Controller as XUiC_OptionsDialogBase;
        if (optionsDialog != null && SetChangedMethod != null)
        {
            try
            {
                SetChangedMethod.Invoke(optionsDialog, null);
                return;
            }
            catch
            {
            }
        }

        if (InvokeValueChangedMethod == null)
        {
            return;
        }

        try
        {
            InvokeValueChangedMethod.Invoke(source, null);
        }
        catch
        {
        }
    }

    private string ResolveSwapPresetFromAnimals()
    {
        bool anyOn = false;
        bool anyOff = false;
        foreach (XUiC_ComboBoxList<string> combo in GetAnimalSwapCombos())
        {
            if (IsSwapOn(combo)) anyOn = true; else anyOff = true;
            if (anyOn && anyOff) return SwapPresetCustom;
        }

        if (anyOn) return SwapPresetAll;
        return SwapPresetNone;
    }

    private static string NormalizeSwapPreset(string raw)
    {
        if (string.Equals(raw, SwapPresetAll, StringComparison.OrdinalIgnoreCase)) return SwapPresetAll;
        if (string.Equals(raw, SwapPresetNone, StringComparison.OrdinalIgnoreCase)) return SwapPresetNone;
        return SwapPresetCustom;
    }

    private void SyncSwapPresetFromAnimals()
    {
        if (_animalSwapPreset == null)
        {
            return;
        }

        string resolved = ResolveSwapPresetFromAnimals();
        if (string.Equals(_animalSwapPreset.Value ?? string.Empty, resolved, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _suppressEvents = true;
        try
        {
            _animalSwapPreset.Value = resolved;
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private void PersistSwapSettings()
    {
        AudioOptionsPlusConfig.SetUiSoundSwapSettings(
            false,
            IsSwapOn(_bearSwap),
            IsSwapOn(_boarSwap),
            IsSwapOn(_chickenSwap),
            IsSwapOn(_rabbitSwap),
            IsSwapOn(_snakeSwap),
            IsSwapOn(_stagSwap),
            IsSwapOn(_vultureSwap),
            IsSwapOn(_caninesSwap),
            IsSwapOn(_caninesSwap),
            IsSwapOn(_mountainLionSwap),
            IsSwapOn(_caninesSwap));
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
        _animalSwapPreset = ResolveStringListCombo("AOPSoundSwapAnimalPainDeathSoundPreset");
        _bearSwap = ResolveStringListCombo("AOPSoundSwapBear");
        _boarSwap = ResolveStringListCombo("AOPSoundSwapBoar");
        _caninesSwap = ResolveStringListCombo("AOPSoundSwapCanines");
        _chickenSwap = ResolveStringListCombo("AOPSoundSwapChicken");
        _stagSwap = ResolveStringListCombo("AOPSoundSwapStag");
        _mountainLionSwap = ResolveStringListCombo("AOPSoundSwapMountainLion");
        _rabbitSwap = ResolveStringListCombo("AOPSoundSwapRabbit");
        _snakeSwap = ResolveStringListCombo("AOPSoundSwapSnake");
        _vultureSwap = ResolveStringListCombo("AOPSoundSwapVulture");
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

        XUiController byWindow = windowGroup?.Controller?.GetChildById(id);
        resolved = (byWindow as XUiC_ComboBoxList<string>) ?? byWindow?.GetChildByType<XUiC_ComboBoxList<string>>();
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

        XUiController byWindow = windowGroup?.Controller?.GetChildById(id);
        resolved = (byWindow as XUiC_ComboBoxFloat) ?? byWindow?.GetChildByType<XUiC_ComboBoxFloat>();
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
