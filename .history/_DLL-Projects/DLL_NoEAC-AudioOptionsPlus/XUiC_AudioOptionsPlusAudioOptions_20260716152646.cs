using System;
using System.Reflection;
using UnityEngine;
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
    private int _pendingCheckFrame = -1;
    private bool _pendingCheckCached;

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
        MarkOptionsEntryDirty(_sender);
    }

    private void OnAnimalSwapChangedGeneric(XUiController _sender)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        SyncSwapPresetFromAnimals();
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

    public static bool ApplyAllOpenControllersToConfig()
    {
        bool applied = false;
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.ApplyAllUiValuesToConfig();
            controller.ReloadFromConfigForUi();
            applied = true;
        }

        return applied;
    }

    public static bool ApplyUsingOptionsAudio(XUiC_OptionsAudio optionsAudio)
    {
        XUiC_AudioOptions controller = ResolveForOptionsAudio(optionsAudio);
        if (controller == null)
        {
            return false;
        }

        controller.ApplyAllUiValuesToConfig();
        controller.ReloadFromConfigForUi();
        return true;
    }

    public static void ResetAllOpenControllersToDefaults()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            controller.ResetUiToDefaults();
            controller.RefreshBindingsSelfAndChildren();
        }
    }

    public static bool ResetUsingOptionsAudio(XUiC_OptionsAudio optionsAudio)
    {
        XUiC_AudioOptions controller = ResolveForOptionsAudio(optionsAudio);
        if (controller == null)
        {
            return false;
        }

        controller.ResetUiToDefaults();
        controller.RefreshBindingsSelfAndChildren();
        return true;
    }

    public static bool HasAnyOpenControllerPendingChanges()
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            if (controller.HasPendingChangesAgainstConfig())
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasPendingChangesUsingOptionsAudio(XUiC_OptionsAudio optionsAudio)
    {
        XUiC_AudioOptions controller = ResolveForOptionsAudio(optionsAudio);
        if (controller == null)
        {
            return false;
        }

        return controller.HasPendingChangesAgainstConfig();
    }

    public static bool HasPendingChangeForOption(XUiC_OptionsAudio optionsAudio, string optionName)
    {
        XUiC_AudioOptions controller = ResolveForOptionsAudio(optionsAudio);
        if (controller == null)
        {
            return HasAnyOpenControllerPendingChanges();
        }

        return controller.HasPendingChangeForOption(optionName);
    }

    public static bool AreAllOpenControllersAtDefaults()
    {
        bool hasController = false;
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            hasController = true;
            if (!controller.IsAtDefaultValues())
            {
                return false;
            }
        }

        return hasController;
    }

    public static bool IsAtDefaultsUsingOptionsAudio(XUiC_OptionsAudio optionsAudio)
    {
        XUiC_AudioOptions controller = ResolveForOptionsAudio(optionsAudio);
        if (controller == null)
        {
            return true;
        }

        return controller.IsAtDefaultValues();
    }

    private static XUiC_AudioOptions ResolveForOptionsAudio(XUiC_OptionsAudio optionsAudio)
    {
        for (int i = LiveControllers.Count - 1; i >= 0; i--)
        {
            XUiC_AudioOptions controller = LiveControllers[i];
            if (controller == null)
            {
                LiveControllers.RemoveAt(i);
                continue;
            }

            return controller;
        }

        if (optionsAudio == null)
        {
            return null;
        }

        XUiC_AudioOptions byType = optionsAudio.GetChildByType<XUiC_AudioOptions>();
        if (byType != null)
        {
            RegisterInstance(byType);
        }

        return byType;
    }

    private bool HasPendingChangeForOption(string optionName)
    {
        EnsureControlsResolved();
        AudioOptionsPlusConfig.Load();

        switch (optionName)
        {
            case "AOPVolumeProfilesOverallVolumePreset":
                return !string.Equals(NormalizePreset(_overallPreset?.Value), ResolvePresetFromConfigValues(), StringComparison.OrdinalIgnoreCase);
            case "AOPVolumeProfilesMotorTools":
                return !Nearly(GetValue(_augerChainsaw), NormalizeConfigUiValue(AudioOptionsPlusConfig.AugerChainsawMultiplier));
            case "AOPVolumeProfilesSurfaceImpact":
                return !Nearly(GetValue(_impactHarvestSurface), NormalizeConfigUiValue(AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier));
            case "AOPVolumeProfilesGunFire":
                return !Nearly(GetValue(_gunFire), NormalizeConfigUiValue(AudioOptionsPlusConfig.GunFireMultiplier));
            case "AOPVolumeProfilesExplosions":
                return !Nearly(GetValue(_explosions), NormalizeConfigUiValue(AudioOptionsPlusConfig.ExplosionsMultiplier));
            case "AOPVolumeProfilesVehicles":
                return !Nearly(GetValue(_vehiclesNoHorn), NormalizeConfigUiValue(AudioOptionsPlusConfig.VehiclesNoHornMultiplier));
            case "AOPVolumeProfilesElectrical":
                return !Nearly(GetValue(_electricalBlockLoops), NormalizeConfigUiValue(AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier));
            case "AOPVolumeProfilesCraftingCompleteSound":
                return !Nearly(GetValue(_craftingCompleteUi), NormalizeConfigUiValue(AudioOptionsPlusConfig.CraftingCompleteUiMultiplier));
            case "AOPVolumeProfilesSpiderZombie":
                return !Nearly(GetValue(_spiderZombie), NormalizeConfigUiValue(AudioOptionsPlusConfig.SpiderZombieMultiplier));
            case "AOPVolumeProfilesAnimalPainDeath":
                return !Nearly(GetValue(_animalPainDeath), NormalizeConfigUiValue(AudioOptionsPlusConfig.AnimalPainDeathMultiplier));
            case "AOPVolumeProfilesBlockBuilding":
                return !Nearly(GetValue(_placeUpgradeRepair), NormalizeConfigUiValue(AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier));
            case "AOPVolumeProfilesProtectedBlockDings":
                return !Nearly(GetValue(_protectionDings), NormalizeConfigUiValue(AudioOptionsPlusConfig.ProtectionDingsMultiplier));
            case "AOPVolumeProfilesItemInteractionSounds":
                return !Nearly(GetValue(_interactionPrompts), NormalizeConfigUiValue(AudioOptionsPlusConfig.InteractionPromptsMultiplier));
            case "AOPVolumeProfilesTwitchSounds":
                return !Nearly(GetValue(_twitch), NormalizeConfigUiValue(AudioOptionsPlusConfig.TwitchMultiplier));
            case "AOPVolumeProfilesAllDoorTypes":
                return !Nearly(GetValue(_doorsHatchesVaultsCellarsBridge), NormalizeConfigUiValue(AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier));
            case "AOPVolumeProfilesTraderBob":
                return !Nearly(GetValue(_traderBob), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderBobMultiplier));
            case "AOPVolumeProfilesTraderHugh":
                return !Nearly(GetValue(_traderHugh), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderHughMultiplier));
            case "AOPVolumeProfilesTraderJen":
                return !Nearly(GetValue(_traderJen), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJenMultiplier));
            case "AOPVolumeProfilesTraderJoel":
                return !Nearly(GetValue(_traderJoel), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJoelMultiplier));
            case "AOPVolumeProfilesTraderRekt":
                return !Nearly(GetValue(_traderRekt), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderRektMultiplier));
            case "AOPVolumeProfilesPlayerCharacterSounds":
                return !Nearly(GetValue(_playerMadeSounds), NormalizeConfigUiValue(AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier));
            case "AOPSoundSwapAnimalPainDeathSoundPreset":
                return !string.Equals(NormalizeSwapPreset(_animalSwapPreset?.Value), ResolveSwapPresetFromConfig(), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapBear":
                return !string.Equals(_bearSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBearMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapBoar":
                return !string.Equals(_boarSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBoarMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapCanines":
                return !string.Equals(_caninesSwap?.Value ?? SwapDefault, ResolveCaninesSwapValue(), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapChicken":
                return !string.Equals(_chickenSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapChickenMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapStag":
                return !string.Equals(_stagSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapStagMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapMountainLion":
                return !string.Equals(_mountainLionSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapMountainLionMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapRabbit":
                return !string.Equals(_rabbitSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapRabbitMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapSnake":
                return !string.Equals(_snakeSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapSnakeMode), StringComparison.OrdinalIgnoreCase);
            case "AOPSoundSwapVulture":
                return !string.Equals(_vultureSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapVultureMode), StringComparison.OrdinalIgnoreCase);
            default:
                return HasPendingChangesAgainstConfig();
        }
    }

    private static string ResolvePresetFromConfigValues()
    {
        float auger = NormalizeConfigUiValue(AudioOptionsPlusConfig.AugerChainsawMultiplier);
        float impact = NormalizeConfigUiValue(AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier);
        float gun = NormalizeConfigUiValue(AudioOptionsPlusConfig.GunFireMultiplier);
        float explosions = NormalizeConfigUiValue(AudioOptionsPlusConfig.ExplosionsMultiplier);
        float vehicles = NormalizeConfigUiValue(AudioOptionsPlusConfig.VehiclesNoHornMultiplier);
        float electrical = NormalizeConfigUiValue(AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier);
        float crafting = NormalizeConfigUiValue(AudioOptionsPlusConfig.CraftingCompleteUiMultiplier);
        float spider = NormalizeConfigUiValue(AudioOptionsPlusConfig.SpiderZombieMultiplier);
        float animal = NormalizeConfigUiValue(AudioOptionsPlusConfig.AnimalPainDeathMultiplier);
        float place = NormalizeConfigUiValue(AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier);
        float ding = NormalizeConfigUiValue(AudioOptionsPlusConfig.ProtectionDingsMultiplier);
        float interaction = NormalizeConfigUiValue(AudioOptionsPlusConfig.InteractionPromptsMultiplier);
        float twitch = NormalizeConfigUiValue(AudioOptionsPlusConfig.TwitchMultiplier);
        float doors = NormalizeConfigUiValue(AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier);
        float bob = NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderBobMultiplier);
        float hugh = NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderHughMultiplier);
        float jen = NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJenMultiplier);
        float joel = NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJoelMultiplier);
        float rekt = NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderRektMultiplier);
        float player = NormalizeConfigUiValue(AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier);

        if (NearlyAll(0f, auger, impact, gun, explosions, vehicles, electrical, crafting, spider, animal, place, ding, interaction, twitch, doors, bob, hugh, jen, joel, rekt, player)) return PresetOff;
        if (NearlyAll(0.25f, auger, impact, gun, explosions, vehicles, electrical, crafting, spider, animal, place, ding, interaction, twitch, doors, bob, hugh, jen, joel, rekt, player)) return PresetLow;
        if (NearlyAll(0.5f, auger, impact, gun, explosions, vehicles, electrical, crafting, spider, animal, place, ding, interaction, twitch, doors, bob, hugh, jen, joel, rekt, player)) return PresetMedium;
        if (NearlyAll(0.75f, auger, impact, gun, explosions, vehicles, electrical, crafting, spider, animal, place, ding, interaction, twitch, doors, bob, hugh, jen, joel, rekt, player)) return PresetHigh;
        if (NearlyAll(1f, auger, impact, gun, explosions, vehicles, electrical, crafting, spider, animal, place, ding, interaction, twitch, doors, bob, hugh, jen, joel, rekt, player)) return PresetDefault;
        return PresetCustom;
    }

    private static bool NearlyAll(float expected, params float[] values)
    {
        if (values == null)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!Nearly(values[i], expected))
            {
                return false;
            }
        }

        return true;
    }

    private static string ResolveSwapPresetFromConfig()
    {
        bool bear = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapBearMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool boar = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapBoarMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool chicken = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapChickenMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool rabbit = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapRabbitMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool snake = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapSnakeMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool stag = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapStagMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool vulture = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapVultureMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool mountainLion = string.Equals(ToSwapValue(AudioOptionsPlusConfig.SoundSwapMountainLionMode), SwapSwapped, StringComparison.OrdinalIgnoreCase);
        bool canines =
            string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapWolfMode), "Both", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapDireWolfMode), "Both", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapZombieDogMode), "Both", StringComparison.OrdinalIgnoreCase);

        bool anyOn = bear || boar || chicken || rabbit || snake || stag || vulture || mountainLion || canines;
        bool anyOff = !bear || !boar || !chicken || !rabbit || !snake || !stag || !vulture || !mountainLion || !canines;

        if (anyOn && anyOff) return SwapPresetCustom;
        return anyOn ? SwapPresetAll : SwapPresetNone;
    }

    private void ApplyAllUiValuesToConfig()
    {
        EnsureControlsResolved();
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
        PersistSwapSettings();
    }

    private void ResetUiToDefaults()
    {
        _suppressEvents = true;
        try
        {
            foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
            {
                Set(combo, 1f);
            }

            if (_overallPreset != null)
            {
                _overallPreset.Value = PresetDefault;
            }

            foreach (XUiC_ComboBoxList<string> combo in GetAnimalSwapCombos())
            {
                SetSwap(combo, SwapDefault);
            }

            if (_animalSwapPreset != null)
            {
                _animalSwapPreset.Value = SwapPresetNone;
            }
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private bool HasPendingChangesAgainstConfig()
    {
        int frame = Time.frameCount;
        if (_pendingCheckFrame == frame)
        {
            return _pendingCheckCached;
        }

        EnsureControlsResolved();
        AudioOptionsPlusConfig.Load();

        bool volumeChanged =
            !Nearly(GetValue(_augerChainsaw), NormalizeConfigUiValue(AudioOptionsPlusConfig.AugerChainsawMultiplier)) ||
            !Nearly(GetValue(_impactHarvestSurface), NormalizeConfigUiValue(AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier)) ||
            !Nearly(GetValue(_gunFire), NormalizeConfigUiValue(AudioOptionsPlusConfig.GunFireMultiplier)) ||
            !Nearly(GetValue(_explosions), NormalizeConfigUiValue(AudioOptionsPlusConfig.ExplosionsMultiplier)) ||
            !Nearly(GetValue(_vehiclesNoHorn), NormalizeConfigUiValue(AudioOptionsPlusConfig.VehiclesNoHornMultiplier)) ||
            !Nearly(GetValue(_electricalBlockLoops), NormalizeConfigUiValue(AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier)) ||
            !Nearly(GetValue(_craftingCompleteUi), NormalizeConfigUiValue(AudioOptionsPlusConfig.CraftingCompleteUiMultiplier)) ||
            !Nearly(GetValue(_spiderZombie), NormalizeConfigUiValue(AudioOptionsPlusConfig.SpiderZombieMultiplier)) ||
            !Nearly(GetValue(_animalPainDeath), NormalizeConfigUiValue(AudioOptionsPlusConfig.AnimalPainDeathMultiplier)) ||
            !Nearly(GetValue(_placeUpgradeRepair), NormalizeConfigUiValue(AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier)) ||
            !Nearly(GetValue(_protectionDings), NormalizeConfigUiValue(AudioOptionsPlusConfig.ProtectionDingsMultiplier)) ||
            !Nearly(GetValue(_interactionPrompts), NormalizeConfigUiValue(AudioOptionsPlusConfig.InteractionPromptsMultiplier)) ||
            !Nearly(GetValue(_twitch), NormalizeConfigUiValue(AudioOptionsPlusConfig.TwitchMultiplier)) ||
            !Nearly(GetValue(_doorsHatchesVaultsCellarsBridge), NormalizeConfigUiValue(AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier)) ||
            !Nearly(GetValue(_traderBob), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderBobMultiplier)) ||
            !Nearly(GetValue(_traderHugh), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderHughMultiplier)) ||
            !Nearly(GetValue(_traderJen), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJenMultiplier)) ||
            !Nearly(GetValue(_traderJoel), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderJoelMultiplier)) ||
            !Nearly(GetValue(_traderRekt), NormalizeConfigUiValue(AudioOptionsPlusConfig.TraderRektMultiplier)) ||
            !Nearly(GetValue(_playerMadeSounds), NormalizeConfigUiValue(AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier));

        bool swapChanged =
            !string.Equals(_bearSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBearMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_boarSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapBoarMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_chickenSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapChickenMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_rabbitSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapRabbitMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_snakeSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapSnakeMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_stagSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapStagMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_vultureSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapVultureMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_mountainLionSwap?.Value ?? SwapDefault, ToSwapValue(AudioOptionsPlusConfig.SoundSwapMountainLionMode), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_caninesSwap?.Value ?? SwapDefault, ResolveCaninesSwapValue(), StringComparison.OrdinalIgnoreCase);

        _pendingCheckCached = volumeChanged || swapChanged;
        _pendingCheckFrame = frame;
        return _pendingCheckCached;
    }

    private bool IsAtDefaultValues()
    {
        foreach (XUiC_ComboBoxFloat combo in GetCategoryCombos())
        {
            if (!Nearly(GetValue(combo), 1f))
            {
                return false;
            }
        }

        foreach (XUiC_ComboBoxList<string> combo in GetAnimalSwapCombos())
        {
            if (!string.Equals(combo?.Value ?? SwapDefault, SwapDefault, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (!string.Equals(_overallPreset?.Value ?? PresetDefault, PresetDefault, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(_animalSwapPreset?.Value ?? SwapPresetNone, SwapPresetNone, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static float NormalizeConfigUiValue(float configValue)
    {
        return configValue < 0f ? 1f : configValue;
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
