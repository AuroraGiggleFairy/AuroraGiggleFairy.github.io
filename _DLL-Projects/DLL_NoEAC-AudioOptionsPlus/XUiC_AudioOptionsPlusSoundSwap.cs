using System;
using UnityEngine.Scripting;
using AudioOptionsPlus;

[Preserve]
public class XUiC_AudioOptionsPlusSoundSwap : XUiController
{
    private const string ToggleOn = "xuiOptionsAudioAOPSwapSettingSwapped";
    private const string ToggleOff = "xuiOptionsAudioAOPSwapSettingDefault";
    private const string SillySoundsOn = "xuiOptionsAudioAOPSoundSwap1SillySoundsSettingOn";
    private const string SillySoundsOff = "xuiOptionsAudioAOPSoundSwap1SillySoundsSettingOff";
    private const string PresetAll = "All";
    private const string PresetNone = "None";
    private const string PresetCustom = "Custom";
    private const string ModeNone = "None";
    private const string ModeBoth = "Both";

    private XUiC_ComboBoxList<string> _preset;
    private XUiC_ComboBoxList<string> _sillySoundsEnabled;
    private XUiC_ComboBoxList<string> _bearMode;
    private XUiC_ComboBoxList<string> _boarMode;
    private XUiC_ComboBoxList<string> _chickenMode;
    private XUiC_ComboBoxList<string> _rabbitMode;
    private XUiC_ComboBoxList<string> _snakeMode;
    private XUiC_ComboBoxList<string> _stagMode;
    private XUiC_ComboBoxList<string> _vultureMode;
    private XUiC_ComboBoxList<string> _caninesMode;
    private XUiC_ComboBoxList<string> _mountainLionMode;

    private bool _suppressEvents;
    private bool _isReady;
    private bool _caninesTouched;

    public override void Init()
    {
        base.Init();
        ResolveControls();

        foreach (XUiC_ComboBoxList<string> combo in GetCombos())
        {
            if (combo != null)
            {
                combo.OnValueChanged += OnAnyValueChanged;
                combo.OnValueChangedGeneric += OnAnyValueChangedGeneric;
            }
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        EnsureControlsResolved();
        _isReady = false;
        _caninesTouched = false;
        RefreshFromConfig();
        _isReady = true;
    }

    private void OnAnyValueChanged(XUiController _sender, string _oldValue, string _newValue)
    {
        HandleValueChanged(_sender);
    }

    private void OnAnyValueChangedGeneric(XUiController _sender)
    {
        HandleValueChanged(_sender);
    }

    private void HandleValueChanged(XUiController sender)
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        EnsureControlsResolved();

        if (ReferenceEquals(sender, _preset))
        {
            ApplyPresetSelection();
        }
        else if (IsAnimalCombo(sender))
        {
            if (ReferenceEquals(sender, _caninesMode))
            {
                _caninesTouched = true;
            }

            SyncPresetFromAnimals();
        }

        Persist();
    }

    private void Persist()
    {
        if (_suppressEvents || !_isReady)
        {
            return;
        }

        EnsureControlsResolved();

        bool legacyWolfEnabled = string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapWolfMode), ModeBoth, StringComparison.OrdinalIgnoreCase);
        bool legacyDireWolfEnabled = string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapDireWolfMode), ModeBoth, StringComparison.OrdinalIgnoreCase);
        bool legacyZombieDogEnabled = string.Equals(AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapZombieDogMode), ModeBoth, StringComparison.OrdinalIgnoreCase);

        bool caninesEnabled = IsOn(_caninesMode);
        bool wolfEnabled = _caninesTouched ? caninesEnabled : legacyWolfEnabled;
        bool direWolfEnabled = _caninesTouched ? caninesEnabled : legacyDireWolfEnabled;
        bool zombieDogEnabled = _caninesTouched ? caninesEnabled : legacyZombieDogEnabled;

        AudioOptionsPlusConfig.SetUiSoundSwapSettings(
            IsSillyOn(_sillySoundsEnabled),
            IsOn(_bearMode),
            IsOn(_boarMode),
            IsOn(_chickenMode),
            IsOn(_rabbitMode),
            IsOn(_snakeMode),
            IsOn(_stagMode),
            IsOn(_vultureMode),
            wolfEnabled,
            direWolfEnabled,
            IsOn(_mountainLionMode),
            zombieDogEnabled);

        RefreshBindingsSelfAndChildren();
    }

    private void RefreshFromConfig()
    {
        EnsureControlsResolved();

        _suppressEvents = true;
        try
        {
            AudioOptionsPlusConfig.Load();

            Set(_sillySoundsEnabled, AudioOptionsPlusConfig.SillySoundsEnabled ? SillySoundsOn : SillySoundsOff);
            Set(_bearMode, ToToggle(AudioOptionsPlusConfig.SoundSwapBearMode));
            Set(_boarMode, ToToggle(AudioOptionsPlusConfig.SoundSwapBoarMode));
            Set(_chickenMode, ToToggle(AudioOptionsPlusConfig.SoundSwapChickenMode));
            Set(_rabbitMode, ToToggle(AudioOptionsPlusConfig.SoundSwapRabbitMode));
            Set(_snakeMode, ToToggle(AudioOptionsPlusConfig.SoundSwapSnakeMode));
            Set(_stagMode, ToToggle(AudioOptionsPlusConfig.SoundSwapStagMode));
            Set(_vultureMode, ToToggle(AudioOptionsPlusConfig.SoundSwapVultureMode));
            Set(_caninesMode, ResolveCaninesToggle());
            Set(_mountainLionMode, ToToggle(AudioOptionsPlusConfig.SoundSwapMountainLionMode));
            Set(_preset, ResolvePresetFromAnimals());

            // Upgrade any legacy value text (e.g. None/Pain/Death/Both) to the current Off/On model.
            CanonicalizeSillyToggle(_sillySoundsEnabled);
            CanonicalizeToggle(_bearMode);
            CanonicalizeToggle(_boarMode);
            CanonicalizeToggle(_chickenMode);
            CanonicalizeToggle(_rabbitMode);
            CanonicalizeToggle(_snakeMode);
            CanonicalizeToggle(_stagMode);
            CanonicalizeToggle(_vultureMode);
            CanonicalizeToggle(_caninesMode);
            CanonicalizeToggle(_mountainLionMode);
            CanonicalizePreset(_preset);
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private XUiC_ComboBoxList<string>[] GetCombos()
    {
        return new[]
        {
            _preset,
            _sillySoundsEnabled,
            _bearMode,
            _boarMode,
            _chickenMode,
            _rabbitMode,
            _snakeMode,
            _stagMode,
            _vultureMode,
            _caninesMode,
            _mountainLionMode,
        };
    }

    private static bool IsOn(XUiC_ComboBoxList<string> combo)
    {
        string value = combo?.Value ?? ToggleOff;
        return string.Equals(value, ToggleOn, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSillyOn(XUiC_ComboBoxList<string> combo)
    {
        string value = combo?.Value ?? SillySoundsOff;
        return string.Equals(value, SillySoundsOn, StringComparison.OrdinalIgnoreCase);
    }

    private static void CanonicalizeSillyToggle(XUiC_ComboBoxList<string> combo)
    {
        if (combo == null) return;
        string value = combo.Value ?? string.Empty;
        if (string.Equals(value, SillySoundsOn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, SillySoundsOff, StringComparison.OrdinalIgnoreCase))
            return;
        combo.Value = SillySoundsOff;
    }

    private static string ToToggle(string mode)
    {
        string normalized = AudioOptionsPlusConfig.NormalizeSoundSwapMode(mode);
        return string.Equals(normalized, ModeBoth, StringComparison.OrdinalIgnoreCase)
            ? ToggleOn
            : ToggleOff;
    }

    private static void Set(XUiC_ComboBoxList<string> combo, string value)
    {
        if (combo == null)
        {
            return;
        }

        combo.Value = value;
    }

    private void ApplyPresetSelection()
    {
        string preset = NormalizePreset(_preset?.Value);
        if (string.Equals(preset, PresetCustom, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool enableAnimals = string.Equals(preset, PresetAll, StringComparison.OrdinalIgnoreCase);
        _caninesTouched = true;
        _suppressEvents = true;
        try
        {
            foreach (XUiC_ComboBoxList<string> combo in GetAnimalCombos())
            {
                Set(combo, enableAnimals ? ToggleOn : ToggleOff);
            }
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private void SyncPresetFromAnimals()
    {
        string resolved = ResolvePresetFromAnimals();
        if (string.Equals(_preset?.Value ?? string.Empty, resolved, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _suppressEvents = true;
        try
        {
            Set(_preset, resolved);
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    private string ResolvePresetFromAnimals()
    {
        XUiC_ComboBoxList<string>[] animals = GetAnimalCombos();
        bool anyOn = false;
        bool anyOff = false;

        for (int i = 0; i < animals.Length; i++)
        {
            if (IsOn(animals[i]))
            {
                anyOn = true;
            }
            else
            {
                anyOff = true;
            }

            if (anyOn && anyOff)
            {
                return PresetCustom;
            }
        }

        if (anyOn)
        {
            return PresetAll;
        }

        return PresetNone;
    }

    private static string NormalizePreset(string raw)
    {
        if (string.Equals(raw, PresetAll, StringComparison.OrdinalIgnoreCase))
        {
            return PresetAll;
        }

        if (string.Equals(raw, PresetNone, StringComparison.OrdinalIgnoreCase))
        {
            return PresetNone;
        }

        return PresetCustom;
    }

    private static void CanonicalizePreset(XUiC_ComboBoxList<string> combo)
    {
        if (combo == null)
        {
            return;
        }

        combo.Value = NormalizePreset(combo.Value);
    }

    private XUiC_ComboBoxList<string>[] GetAnimalCombos()
    {
        return new[]
        {
            _bearMode,
            _boarMode,
            _chickenMode,
            _rabbitMode,
            _snakeMode,
            _stagMode,
            _vultureMode,
            _caninesMode,
            _mountainLionMode,
        };
    }

    private string ResolveCaninesToggle()
    {
        string wolf = AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapWolfMode);
        string direWolf = AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapDireWolfMode);
        string zombieDog = AudioOptionsPlusConfig.NormalizeSoundSwapMode(AudioOptionsPlusConfig.SoundSwapZombieDogMode);

        bool anyOn = string.Equals(wolf, ModeBoth, StringComparison.OrdinalIgnoreCase)
            || string.Equals(direWolf, ModeBoth, StringComparison.OrdinalIgnoreCase)
            || string.Equals(zombieDog, ModeBoth, StringComparison.OrdinalIgnoreCase);

        return anyOn ? ToggleOn : ToggleOff;
    }

    private bool IsAnimalCombo(XUiController sender)
    {
        if (sender == null)
        {
            return false;
        }

        XUiC_ComboBoxList<string>[] animals = GetAnimalCombos();
        for (int i = 0; i < animals.Length; i++)
        {
            if (ReferenceEquals(sender, animals[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static void CanonicalizeToggle(XUiC_ComboBoxList<string> combo)
    {
        if (combo == null)
        {
            return;
        }

        string value = combo.Value ?? string.Empty;
        if (string.Equals(value, ToggleOn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, ToggleOff, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        combo.Value = ToggleOff;
    }

    private void EnsureControlsResolved()
    {
        if (_sillySoundsEnabled == null
            || _bearMode == null
            || _boarMode == null
            || _chickenMode == null
            || _rabbitMode == null
            || _snakeMode == null
            || _stagMode == null
            || _vultureMode == null
            || _caninesMode == null
            || _mountainLionMode == null
            )
        {
            ResolveControls();
        }
    }

    private void ResolveControls()
    {
        _preset = ResolveStringListCombo("AOPSoundSwapAnimalPainDeathSoundPreset");
        _sillySoundsEnabled = ResolveStringListCombo("AOPSoundSwap1SillySoundsEnabled");
        _bearMode = ResolveStringListCombo("AOPSoundSwapBear");
        _boarMode = ResolveStringListCombo("AOPSoundSwapBoar");
        _chickenMode = ResolveStringListCombo("AOPSoundSwapChicken");
        _rabbitMode = ResolveStringListCombo("AOPSoundSwapRabbit");
        _snakeMode = ResolveStringListCombo("AOPSoundSwapSnake");
        _stagMode = ResolveStringListCombo("AOPSoundSwapStag");
        _vultureMode = ResolveStringListCombo("AOPSoundSwapVulture");
        _caninesMode = ResolveStringListCombo("AOPSoundSwapCanines");
        _mountainLionMode = ResolveStringListCombo("AOPSoundSwapMountainLion");
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
}
