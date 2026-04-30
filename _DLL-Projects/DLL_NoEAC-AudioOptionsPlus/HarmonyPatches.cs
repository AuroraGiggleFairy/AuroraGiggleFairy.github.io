using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AudioOptionsPlus;

internal static class AudioOptionsPlusConfig
{
    private static readonly object Sync = new object();

    internal sealed class SoundRule
    {
        public string Target = "any";
        public string Keyword = string.Empty;
        public float Multiplier = 1f;
    }

    public static bool Enabled { get; private set; } = true;
    public static bool PlayerOnly { get; private set; } = true;
    public static float DefaultMultiplier { get; private set; } = 1f;
    public static float AugerChainsawMultiplier { get; private set; } = -1f;
    public static float ImpactHarvestSurfaceMultiplier { get; private set; } = -1f;
    public static float GunFireMultiplier { get; private set; } = -1f;
    public static float ExplosionsMultiplier { get; private set; } = -1f;
    public static float VehiclesNoHornMultiplier { get; private set; } = -1f;
    public static float ElectricalBlockLoopsMultiplier { get; private set; } = -1f;
    public static float CraftingCompleteUiMultiplier { get; private set; } = -1f;
    public static float SpiderZombieMultiplier { get; private set; } = -1f;
    public static float AnimalPainDeathMultiplier { get; private set; } = -1f;
    public static float PlaceUpgradeRepairMultiplier { get; private set; } = -1f;
    public static float ProtectionDingsMultiplier { get; private set; } = -1f;
    public static float InteractionPromptsMultiplier { get; private set; } = -1f;
    public static float TwitchMultiplier { get; private set; } = -1f;
    public static float DoorsHatchesVaultsCellarsBridgeMultiplier { get; private set; } = -1f;
    public static float TraderBobMultiplier { get; private set; } = -1f;
    public static float TraderHughMultiplier { get; private set; } = -1f;
    public static float TraderJenMultiplier { get; private set; } = -1f;
    public static float TraderJoelMultiplier { get; private set; } = -1f;
    public static float TraderRektMultiplier { get; private set; } = -1f;
    public static float PlayerMadeSoundsMultiplier { get; private set; } = -1f;
    public static bool SillySoundsEnabled { get; private set; } = false;
    public static bool SoundSwapEnabled { get; private set; } = false;
    public static string SoundSwapPlayerMode { get; private set; } = "None";
    public static string SoundSwapBearMode { get; private set; } = "None";
    public static string SoundSwapBoarMode { get; private set; } = "None";
    public static string SoundSwapChickenMode { get; private set; } = "None";
    public static string SoundSwapRabbitMode { get; private set; } = "None";
    public static string SoundSwapSnakeMode { get; private set; } = "None";
    public static string SoundSwapStagMode { get; private set; } = "None";
    public static string SoundSwapVultureMode { get; private set; } = "None";
    public static string SoundSwapWolfMode { get; private set; } = "None";
    public static string SoundSwapDireWolfMode { get; private set; } = "None";
    public static string SoundSwapMountainLionMode { get; private set; } = "None";
    public static string SoundSwapZombieDogMode { get; private set; } = "None";

    // Compatibility aliases for older helper code paths.
    public static float AugerMultiplier => AugerChainsawMultiplier;
    public static float ImpactMultiplier => ImpactHarvestSurfaceMultiplier;
    public static float WorkstationDingMultiplier => CraftingCompleteUiMultiplier;
    public static float GunfireMultiplier => GunFireMultiplier;
    public static float ExplosionMultiplier => ExplosionsMultiplier;
    public static float VehicleMultiplier => VehiclesNoHornMultiplier;
    public static float ChainsawMultiplier => SpiderZombieMultiplier;
    public static float GeneratorMultiplier => ElectricalBlockLoopsMultiplier;
    public static float TurretMultiplier => AnimalPainDeathMultiplier;
    public static float RepairMultiplier => PlaceUpgradeRepairMultiplier;
    public static float UpgradeMultiplier => InteractionPromptsMultiplier;
    public static IReadOnlyList<SoundRule> Rules { get; private set; } = new[]
    {
        new SoundRule { Target = "any", Keyword = "forge", Multiplier = 0.35f },
        new SoundRule { Target = "any", Keyword = "workbench", Multiplier = 0.45f },
        new SoundRule { Target = "any", Keyword = "chem", Multiplier = 0.45f },
        new SoundRule { Target = "any", Keyword = "cement", Multiplier = 0.45f },
        new SoundRule { Target = "any", Keyword = "generator", Multiplier = 0.4f },
        new SoundRule { Target = "any", Keyword = "mixer", Multiplier = 0.45f },
        new SoundRule { Target = "any", Keyword = "turret", Multiplier = 0.6f },
        new SoundRule { Target = "any", Keyword = "drill", Multiplier = 0.6f },
        new SoundRule { Target = "any", Keyword = "pump", Multiplier = 0.55f },
        new SoundRule { Target = "any", Keyword = "jack", Multiplier = 0.6f }
    };

    public static void Load()
    {
        lock (Sync)
        {
            EnsureInitialized();

            Enabled = PlayerPrefs.GetInt("AudioOptionsPlus.Enabled", 1) != 0;
            PlayerOnly = PlayerPrefs.GetInt("AudioOptionsPlus.PlayerOnly", 1) != 0;
            // Legacy global default multiplier caused old config values (e.g. 0.2) to mute all sounds.
            // AudioOptionsPlus now treats category sliders/profiles as the source of truth.
            DefaultMultiplier = 1f;
            if (PlayerPrefs.GetFloat("AudioOptionsPlus.DefaultMultiplier", 1f) != 1f)
            {
                PlayerPrefs.SetFloat("AudioOptionsPlus.DefaultMultiplier", 1f);
            }
            AugerChainsawMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.AugerChainsawMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.AugerMultiplier", -1f)), -1f, 1f);
            ImpactHarvestSurfaceMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.ImpactHarvestSurfaceMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.ImpactMultiplier", -1f)), -1f, 1f);
            GunFireMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.GunFireMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.GunfireMultiplier", -1f)), -1f, 1f);
            ExplosionsMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.ExplosionsMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.ExplosionMultiplier", -1f)), -1f, 1f);
            VehiclesNoHornMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.VehiclesNoHornMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.VehicleMultiplier", -1f)), -1f, 1f);
            ElectricalBlockLoopsMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.ElectricalBlockLoopsMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.GeneratorMultiplier", -1f)), -1f, 1f);
            CraftingCompleteUiMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.CraftingCompleteUiMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.WorkstationDingMultiplier", -1f)), -1f, 1f);
            SpiderZombieMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.SpiderZombieMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.ChainsawMultiplier", -1f)), -1f, 1f);
            AnimalPainDeathMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.AnimalPainDeathMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.TurretMultiplier", -1f)), -1f, 1f);
            PlaceUpgradeRepairMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.PlaceUpgradeRepairMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.RepairMultiplier", -1f)), -1f, 1f);
            ProtectionDingsMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.ProtectionDingsMultiplier", -1f), -1f, 1f);
            InteractionPromptsMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.InteractionPromptsMultiplier", PlayerPrefs.GetFloat("AudioOptionsPlus.UpgradeMultiplier", -1f)), -1f, 1f);
            TwitchMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TwitchMultiplier", -1f), -1f, 1f);
            DoorsHatchesVaultsCellarsBridgeMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.DoorsHatchesVaultsCellarsBridgeMultiplier", -1f), -1f, 1f);
            TraderBobMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TraderBobMultiplier", -1f), -1f, 1f);
            TraderHughMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TraderHughMultiplier", -1f), -1f, 1f);
            TraderJenMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TraderJenMultiplier", -1f), -1f, 1f);
            TraderJoelMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TraderJoelMultiplier", -1f), -1f, 1f);
            TraderRektMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.TraderRektMultiplier", -1f), -1f, 1f);
            PlayerMadeSoundsMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("AudioOptionsPlus.PlayerMadeSoundsMultiplier", -1f), -1f, 1f);
            SillySoundsEnabled = PlayerPrefs.GetInt("AudioOptionsPlus.SoundSwap.SillySoundsEnabled", 0) != 0;
            SoundSwapEnabled = PlayerPrefs.GetInt("AudioOptionsPlus.SoundSwap.Enabled", 0) != 0;
            SoundSwapPlayerMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.PlayerMode", "None"));
            SoundSwapBearMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.BearMode", "None"));
            SoundSwapBoarMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.BoarMode", "None"));
            SoundSwapChickenMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.ChickenMode", "None"));
            SoundSwapRabbitMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.RabbitMode", "None"));
            SoundSwapSnakeMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.SnakeMode", "None"));
            SoundSwapStagMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.StagMode", "None"));
            SoundSwapVultureMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.VultureMode", "None"));
            SoundSwapWolfMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.WolfMode", "None"));
            SoundSwapDireWolfMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.DireWolfMode", "None"));
            SoundSwapMountainLionMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.MountainLionMode", "None"));
            SoundSwapZombieDogMode = NormalizeSoundSwapMode(PlayerPrefs.GetString("AudioOptionsPlus.SoundSwap.ZombieDogMode", "None"));
        }
    }

    public static string NormalizeSoundSwapMode(string raw)
    {
        string mode = (raw ?? "None").Trim();
        if (mode.Length == 0)
        {
            return "None";
        }

        if (string.Equals(mode, "Pain", StringComparison.OrdinalIgnoreCase))
        {
            return "Pain";
        }

        if (string.Equals(mode, "Death", StringComparison.OrdinalIgnoreCase))
        {
            return "Death";
        }

        if (string.Equals(mode, "Both", StringComparison.OrdinalIgnoreCase))
        {
            return "Both";
        }

        return "None";
    }

    public static void SetUiFollowOverallDefaults()
    {
        lock (Sync)
        {
            PlayerPrefs.SetFloat("AudioOptionsPlus.AugerChainsawMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.ImpactHarvestSurfaceMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.GunFireMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.ExplosionsMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.VehiclesNoHornMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.ElectricalBlockLoopsMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.CraftingCompleteUiMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.SpiderZombieMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.AnimalPainDeathMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.PlaceUpgradeRepairMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.ProtectionDingsMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.InteractionPromptsMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TwitchMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.DoorsHatchesVaultsCellarsBridgeMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderBobMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderHughMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJenMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJoelMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderRektMultiplier", -1f);
            PlayerPrefs.SetFloat("AudioOptionsPlus.PlayerMadeSoundsMultiplier", -1f);
            PlayerPrefs.Save();
            Load();
            AudioOptionsPlusRuntime.RefreshRuntimeAfterSettingsChange();
        }
    }

    public static void SetUiVolumeOverrides(
        float augerChainsaw,
        float impactHarvestSurface,
        float gunFire,
        float explosions,
        float vehiclesNoHorn,
        float electricalBlockLoops,
        float craftingCompleteUi,
        float spiderZombie,
        float animalPainDeath,
        float placeUpgradeRepair,
        float protectionDings,
        float interactionPrompts,
        float twitch,
        float doorsHatchesVaultsCellarsBridge,
        float traderBob,
        float traderHugh,
        float traderJen,
        float traderJoel,
        float traderRekt,
        float playerMadeSounds)
    {
        lock (Sync)
        {
            PlayerPrefs.SetFloat("AudioOptionsPlus.AugerChainsawMultiplier", Mathf.Clamp01(augerChainsaw));
            PlayerPrefs.SetFloat("AudioOptionsPlus.ImpactHarvestSurfaceMultiplier", Mathf.Clamp01(impactHarvestSurface));
            PlayerPrefs.SetFloat("AudioOptionsPlus.GunFireMultiplier", Mathf.Clamp01(gunFire));
            PlayerPrefs.SetFloat("AudioOptionsPlus.ExplosionsMultiplier", Mathf.Clamp01(explosions));
            PlayerPrefs.SetFloat("AudioOptionsPlus.VehiclesNoHornMultiplier", Mathf.Clamp01(vehiclesNoHorn));
            PlayerPrefs.SetFloat("AudioOptionsPlus.ElectricalBlockLoopsMultiplier", Mathf.Clamp01(electricalBlockLoops));
            PlayerPrefs.SetFloat("AudioOptionsPlus.CraftingCompleteUiMultiplier", Mathf.Clamp01(craftingCompleteUi));
            PlayerPrefs.SetFloat("AudioOptionsPlus.SpiderZombieMultiplier", Mathf.Clamp01(spiderZombie));
            PlayerPrefs.SetFloat("AudioOptionsPlus.AnimalPainDeathMultiplier", Mathf.Clamp01(animalPainDeath));
            PlayerPrefs.SetFloat("AudioOptionsPlus.PlaceUpgradeRepairMultiplier", Mathf.Clamp01(placeUpgradeRepair));
            PlayerPrefs.SetFloat("AudioOptionsPlus.ProtectionDingsMultiplier", Mathf.Clamp01(protectionDings));
            PlayerPrefs.SetFloat("AudioOptionsPlus.InteractionPromptsMultiplier", Mathf.Clamp01(interactionPrompts));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TwitchMultiplier", Mathf.Clamp01(twitch));
            PlayerPrefs.SetFloat("AudioOptionsPlus.DoorsHatchesVaultsCellarsBridgeMultiplier", Mathf.Clamp01(doorsHatchesVaultsCellarsBridge));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderBobMultiplier", Mathf.Clamp01(traderBob));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderHughMultiplier", Mathf.Clamp01(traderHugh));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJenMultiplier", Mathf.Clamp01(traderJen));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJoelMultiplier", Mathf.Clamp01(traderJoel));
            PlayerPrefs.SetFloat("AudioOptionsPlus.TraderRektMultiplier", Mathf.Clamp01(traderRekt));
            PlayerPrefs.SetFloat("AudioOptionsPlus.PlayerMadeSoundsMultiplier", Mathf.Clamp01(playerMadeSounds));
            PlayerPrefs.Save();
            Load();
            AudioOptionsPlusRuntime.RefreshRuntimeAfterSettingsChange();
        }
    }

    public static void SetUiSoundSwapSettings(
        bool sillySoundsEnabled,
        bool bearEnabled,
        bool boarEnabled,
        bool chickenEnabled,
        bool rabbitEnabled,
        bool snakeEnabled,
        bool stagEnabled,
        bool vultureEnabled,
        bool wolfEnabled,
        bool direWolfEnabled,
        bool mountainLionEnabled,
        bool zombieDogEnabled)
    {
        lock (Sync)
        {
            PlayerPrefs.SetInt("AudioOptionsPlus.SoundSwap.SillySoundsEnabled", sillySoundsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("AudioOptionsPlus.SoundSwap.Enabled", 1);
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.PlayerMode", "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.BearMode", bearEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.BoarMode", boarEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.ChickenMode", chickenEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.RabbitMode", rabbitEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.SnakeMode", snakeEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.StagMode", stagEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.VultureMode", vultureEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.WolfMode", wolfEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.DireWolfMode", direWolfEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.MountainLionMode", mountainLionEnabled ? "Both" : "None");
            PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.ZombieDogMode", zombieDogEnabled ? "Both" : "None");
            PlayerPrefs.Save();
            Load();
            AudioOptionsPlusRuntime.RefreshRuntimeAfterSettingsChange();
        }
    }

    private static void EnsureInitialized()
    {
        if (PlayerPrefs.GetInt("AudioOptionsPlus.Initialized", 0) != 0)
        {
            return;
        }

        // Default profile follows overall game volume until user changes sliders.
        PlayerPrefs.SetInt("AudioOptionsPlus.Initialized", 1);
        PlayerPrefs.SetFloat("AudioOptionsPlus.DefaultMultiplier", 1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.AugerChainsawMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.ImpactHarvestSurfaceMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.GunFireMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.ExplosionsMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.VehiclesNoHornMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.ElectricalBlockLoopsMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.CraftingCompleteUiMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.SpiderZombieMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.AnimalPainDeathMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.PlaceUpgradeRepairMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.ProtectionDingsMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.InteractionPromptsMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TwitchMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.DoorsHatchesVaultsCellarsBridgeMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TraderBobMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TraderHughMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJenMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TraderJoelMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.TraderRektMultiplier", -1f);
        PlayerPrefs.SetFloat("AudioOptionsPlus.PlayerMadeSoundsMultiplier", -1f);
        PlayerPrefs.SetInt("AudioOptionsPlus.SoundSwap.SillySoundsEnabled", 0);
        PlayerPrefs.SetInt("AudioOptionsPlus.SoundSwap.Enabled", 0);
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.PlayerMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.BearMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.BoarMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.ChickenMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.RabbitMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.SnakeMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.StagMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.VultureMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.WolfMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.DireWolfMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.MountainLionMode", "None");
        PlayerPrefs.SetString("AudioOptionsPlus.SoundSwap.ZombieDogMode", "None");
        PlayerPrefs.Save();
    }

    private static bool TryParseRule(string value, out SoundRule rule)
    {
        rule = null;
        string[] parts = value.Split(new[] { '|' }, StringSplitOptions.None);
        if (parts.Length != 3)
        {
            return false;
        }

        string target = parts[0].Trim().ToLowerInvariant();
        string keyword = parts[1].Trim();
        if (!float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float multiplier))
        {
            return false;
        }

        if (target != "any" && target != "clip" && target != "source" && target != "group")
        {
            return false;
        }

        if (keyword.Length == 0)
        {
            return false;
        }

        rule = new SoundRule
        {
            Target = target,
            Keyword = keyword,
            Multiplier = Mathf.Clamp(multiplier, 0f, 1f)
        };

        return true;
    }

}

internal static class AudioOptionsPlusRuntime
{
    private const string RabbitSwapSound = "stagalert";

    private enum RepairActionContext
    {
        None,
        Repair,
        Upgrade
    }

    private static readonly Dictionary<int, float> OriginalVolumesBySourceId = new Dictionary<int, float>();
    private static readonly Dictionary<int, float> ForcedSourceVolumesById = new Dictionary<int, float>();
    private static readonly FieldInfo SequenceOnEntityField = typeof(Audio.Manager).GetField("sequenceOnEntity", BindingFlags.Static | BindingFlags.NonPublic);
    private static readonly FieldInfo PlayingOnEntityField = typeof(Audio.Manager).GetField("playingOnEntity", BindingFlags.Static | BindingFlags.NonPublic);
    private static readonly FieldInfo LoopingOnEntityField = typeof(Audio.Manager).GetField("loopingOnEntity", BindingFlags.Static | BindingFlags.NonPublic);
    [ThreadStatic]
    private static Stack<Entity> _entityAudioContext;
    [ThreadStatic]
    private static Stack<string> _soundGroupContext;
    [ThreadStatic]
    private static Stack<string> _soundBasisContext;
    [ThreadStatic]
    private static Stack<RepairActionContext> _repairActionContext;
    [ThreadStatic]
    private static Stack<bool> _managerPlayScaledContext;

    public static void ResetForGameStart()
    {
        OriginalVolumesBySourceId.Clear();
        ForcedSourceVolumesById.Clear();
        ApplyBuiltInSillySoundsSetting();
        if (_entityAudioContext != null)
        {
            _entityAudioContext.Clear();
        }
        if (_soundGroupContext != null)
        {
            _soundGroupContext.Clear();
        }
        if (_soundBasisContext != null)
        {
            _soundBasisContext.Clear();
        }
        if (_repairActionContext != null)
        {
            _repairActionContext.Clear();
        }
        if (_managerPlayScaledContext != null)
        {
            _managerPlayScaledContext.Clear();
        }
    }

    public static void RefreshRuntimeAfterSettingsChange()
    {
        OriginalVolumesBySourceId.Clear();
        ForcedSourceVolumesById.Clear();
        ApplyBuiltInSillySoundsSetting();
    }

    private static void ApplyBuiltInSillySoundsSetting()
    {
        try
        {
            if (Audio.Manager.Instance != null)
            {
                Audio.Manager.Instance.bUseAltSounds = AudioOptionsPlusConfig.SillySoundsEnabled;
            }
        }
        catch
        {
        }
    }

    public static void BeginRepairActionContext(ItemActionData actionData)
    {
        if (_repairActionContext == null)
        {
            _repairActionContext = new Stack<RepairActionContext>();
        }

        _repairActionContext.Push(GetRepairActionContext(actionData));
    }

    public static void EndRepairActionContext()
    {
        if (_repairActionContext != null && _repairActionContext.Count > 0)
        {
            _repairActionContext.Pop();
        }
    }

    public static void BeginManagerPlayScaleContext(bool wasScaledInPrefix)
    {
        if (_managerPlayScaledContext == null)
        {
            _managerPlayScaledContext = new Stack<bool>();
        }

        _managerPlayScaledContext.Push(wasScaledInPrefix);
    }

    public static bool WasManagerPlayScaledInPrefix()
    {
        if (_managerPlayScaledContext == null || _managerPlayScaledContext.Count == 0)
        {
            return false;
        }

        return _managerPlayScaledContext.Peek();
    }

    public static void EndManagerPlayScaleContext()
    {
        if (_managerPlayScaledContext != null && _managerPlayScaledContext.Count > 0)
        {
            _managerPlayScaledContext.Pop();
        }
    }

    private static RepairActionContext GetCurrentRepairActionContext()
    {
        if (_repairActionContext == null || _repairActionContext.Count == 0)
        {
            return RepairActionContext.None;
        }

        return _repairActionContext.Peek();
    }

    private static RepairActionContext GetRepairActionContext(ItemActionData actionData)
    {
        if (actionData == null)
        {
            return RepairActionContext.None;
        }

        try
        {
            FieldInfo repairTypeField = actionData.GetType().GetField("repairType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object repairTypeValue = repairTypeField?.GetValue(actionData);
            string text = repairTypeValue?.ToString() ?? string.Empty;
            if (text.IndexOf("Upgrade", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RepairActionContext.Upgrade;
            }

            if (text.IndexOf("Repair", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RepairActionContext.Repair;
            }
        }
        catch
        {
            // Ignore reflection failures and fall back to name-based matching.
        }

        return RepairActionContext.None;
    }

    public static void BeginEntityAudioContext(Entity entity)
    {
        if (_entityAudioContext == null)
        {
            _entityAudioContext = new Stack<Entity>();
        }

        _entityAudioContext.Push(entity);
    }

    public static void BeginEntityAudioContext(Entity entity, string soundGroupName, string soundBasisName = null)
    {
        BeginEntityAudioContext(entity);
        if (_soundGroupContext == null)
        {
            _soundGroupContext = new Stack<string>();
        }

        if (_soundBasisContext == null)
        {
            _soundBasisContext = new Stack<string>();
        }

        _soundGroupContext.Push(soundGroupName ?? string.Empty);
        _soundBasisContext.Push(soundBasisName ?? soundGroupName ?? string.Empty);
    }

    public static void EndEntityAudioContext()
    {
        if (_entityAudioContext != null && _entityAudioContext.Count > 0)
        {
            _entityAudioContext.Pop();
        }

        if (_soundGroupContext != null && _soundGroupContext.Count > 0)
        {
            _soundGroupContext.Pop();
        }

        if (_soundBasisContext != null && _soundBasisContext.Count > 0)
        {
            _soundBasisContext.Pop();
        }
    }

    public static void ApplyLoadAudioOverride(string clipName, AudioSource source)
    {
        if (!AudioOptionsPlusConfig.Enabled || source == null)
        {
            return;
        }

        string clip = clipName ?? string.Empty;
        string group = (_soundGroupContext != null && _soundGroupContext.Count > 0)
            ? (_soundGroupContext.Peek() ?? string.Empty)
            : string.Empty;

        Entity contextEntity = (_entityAudioContext != null && _entityAudioContext.Count > 0)
            ? _entityAudioContext.Peek()
            : null;

        string sourceName = source.name ?? string.Empty;
        if (!TryResolveCategoryOverride(clip, sourceName, group, contextEntity, out float overrideVolume, source))
        {
            return;
        }

        // Category sliders apply consistently regardless of ownership context to prevent layered volume mismatch.

        int sourceId = source.GetInstanceID();
        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * overrideVolume);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    public static string ResolveSoundSwapForGroup(Entity entity, string soundGroupName)
    {
        ApplyBuiltInSillySoundsSetting();

        if (string.IsNullOrEmpty(soundGroupName))
        {
            return soundGroupName;
        }

        return TryResolveSoundSwap(entity, soundGroupName, out string replacement)
            ? replacement
            : soundGroupName;
    }

    public static string ResolveSoundSwapForClip(string clipName)
    {
        ApplyBuiltInSillySoundsSetting();

        if (string.IsNullOrEmpty(clipName))
        {
            return clipName;
        }

        Entity contextEntity = (_entityAudioContext != null && _entityAudioContext.Count > 0)
            ? _entityAudioContext.Peek()
            : null;

        string contextGroup = (_soundGroupContext != null && _soundGroupContext.Count > 0)
            ? (_soundGroupContext.Peek() ?? string.Empty)
            : string.Empty;

        string contextBasis = (_soundBasisContext != null && _soundBasisContext.Count > 0)
            ? (_soundBasisContext.Peek() ?? string.Empty)
            : string.Empty;

        string basis = contextBasis.Length > 0 ? contextBasis : (contextGroup.Length > 0 ? contextGroup : clipName);
        return TryResolveSoundSwap(contextEntity, basis, out string replacement)
            ? replacement
            : clipName;
    }

    public static void EnforceOnHandleSources(AudioSource nearSource, AudioSource farSource)
    {
        if (!AudioOptionsPlusConfig.Enabled)
        {
            return;
        }

        if (nearSource != null && ForcedSourceVolumesById.TryGetValue(nearSource.GetInstanceID(), out float nearOverride))
        {
            nearSource.volume = nearOverride;
        }

        if (farSource != null && ForcedSourceVolumesById.TryGetValue(farSource.GetInstanceID(), out float farOverride))
        {
            farSource.volume = farOverride;
        }
    }

    public static void ApplyToSource(AudioSource source, AudioClip explicitClip = null)
    {
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();

        if (!AudioOptionsPlusConfig.Enabled)
        {
            RestoreSource(sourceId, source);
            return;
        }

        // Keep manager-derived category scaling stable across Play/PlayDelayed hooks.
        if (ForcedSourceVolumesById.TryGetValue(sourceId, out float forcedVolume))
        {
            source.volume = forcedVolume;
            return;
        }

        AudioClip clip = explicitClip ?? source.clip;
        if (TryGetAugerOverrideVolume(source, clip, out float augerOverride))
        {
            source.volume = augerOverride;
            return;
        }

        float? multiplier = GetResolvedMultiplierForSource(source, clip);
        if (!multiplier.HasValue)
        {
            // If we have a remembered original but no local match, do not aggressively restore.
            // Another manager path may already own this source's intended scaled value.
            if (ForcedSourceVolumesById.TryGetValue(sourceId, out float stickyForcedVolume))
            {
                source.volume = stickyForcedVolume;
                return;
            }

            RestoreSource(sourceId, source);
            return;
        }

        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * multiplier.Value);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    public static float ScaleOneShot(AudioSource source, AudioClip clip, float requestedVolumeScale)
    {
        if (!AudioOptionsPlusConfig.Enabled)
        {
            return requestedVolumeScale;
        }

        if (TryGetAugerOverrideVolume(source, clip, out float augerOverride))
        {
            if (source != null)
            {
                source.volume = augerOverride;
            }

            return 1f;
        }

        float? multiplier = GetResolvedMultiplierForSource(source, clip);
        return multiplier.HasValue ? Mathf.Clamp01(requestedVolumeScale * multiplier.Value) : requestedVolumeScale;
    }

    public static bool TryScaleSourceVolumeForOneShot(AudioSource source, AudioClip clip, out float originalVolume)
    {
        originalVolume = 0f;

        if (!AudioOptionsPlusConfig.Enabled || source == null)
        {
            return false;
        }

        if (TryGetAugerOverrideVolume(source, clip, out float augerOverride))
        {
            originalVolume = source.volume;
            source.volume = augerOverride;
            return true;
        }

        float? multiplier = GetResolvedMultiplierForSource(source, clip);
        if (!multiplier.HasValue)
        {
            return false;
        }

        originalVolume = source.volume;
        source.volume = Mathf.Clamp01(source.volume * multiplier.Value);
        return true;
    }

    public static float ScaleClipAtPoint(AudioClip clip, float volume)
    {
        if (!AudioOptionsPlusConfig.Enabled)
        {
            return volume;
        }

        if (TryGetAugerOverrideVolume(null, clip, out float augerOverride))
        {
            return augerOverride;
        }

        float? multiplier = GetResolvedMultiplierForSource(null, clip);
        return multiplier.HasValue ? Mathf.Clamp01(volume * multiplier.Value) : volume;
    }

    private static float? GetResolvedMultiplierForSource(AudioSource source, AudioClip clip)
    {
        string clipName = clip != null ? (clip.name ?? string.Empty) : string.Empty;
        string sourceName = source != null ? (source.name ?? string.Empty) : string.Empty;
        string groupName = (_soundGroupContext != null && _soundGroupContext.Count > 0)
            ? (_soundGroupContext.Peek() ?? string.Empty)
            : string.Empty;
        Entity contextEntity = (_entityAudioContext != null && _entityAudioContext.Count > 0)
            ? _entityAudioContext.Peek()
            : null;

        if (TryResolveCategoryOverride(clipName, sourceName, groupName, contextEntity, out float categoryOverride, source))
        {
            return Mathf.Clamp01(categoryOverride);
        }

        // Restrict scaling to explicit category matches only.
        return null;
    }

    public static float ScaleManagerPlayVolume(Entity entity, string soundGroupName, float requestedVolumeScale)
    {
        if (!AudioOptionsPlusConfig.Enabled)
        {
            return requestedVolumeScale;
        }

        if (TryResolveCategoryOverride(string.Empty, string.Empty, soundGroupName ?? string.Empty, entity, out float categoryOverride))
        {
            return Mathf.Clamp01(requestedVolumeScale * categoryOverride);
        }

        return requestedVolumeScale;
    }

    public static void ApplyToSequence(Entity entity, string requestedSoundGroupName)
    {
        if (!AudioOptionsPlusConfig.Enabled || entity == null)
        {
            return;
        }

        if (SequenceOnEntityField == null)
        {
            return;
        }

        try
        {
            object allSequencesObj = SequenceOnEntityField.GetValue(null);
            if (!(allSequencesObj is IDictionary allSequences) || !allSequences.Contains(entity.entityId))
            {
                return;
            }

            object perEntityObj = allSequences[entity.entityId];
            if (!(perEntityObj is IDictionary perEntity))
            {
                return;
            }

            foreach (DictionaryEntry entry in perEntity)
            {
                string groupName = entry.Key as string;
                if (string.IsNullOrEmpty(groupName))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(requestedSoundGroupName)
                    && groupName.IndexOf(requestedSoundGroupName, StringComparison.OrdinalIgnoreCase) < 0
                    && requestedSoundGroupName.IndexOf(groupName, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (TryGetAugerOverrideVolumeForGroup(entity, groupName, out float augerOverride))
                {
                    ApplyMultiplierToSequenceObject(entry.Value, augerOverride);
                    continue;
                }

                if (TryResolveCategoryOverride(string.Empty, string.Empty, groupName, entity, out float categoryOverride))
                {
                    ApplyMultiplierToSequenceObject(entry.Value, categoryOverride);
                    continue;
                }
            }
        }
        catch
        {
            // Ignore reflection failures and keep vanilla behavior.
        }
    }

    public static void ApplyToActiveEntityGroupSources(Entity entity, string requestedSoundGroupName)
    {
        if (!AudioOptionsPlusConfig.Enabled || entity == null || string.IsNullOrEmpty(requestedSoundGroupName))
        {
            return;
        }

        if (TryGetAugerOverrideVolumeForGroup(entity, requestedSoundGroupName, out float augerOverride))
        {
            TryScalePlayingOnEntitySources(entity.entityId, requestedSoundGroupName, augerOverride);
            TryScaleLoopingOnEntitySources(entity.entityId, requestedSoundGroupName, augerOverride);
            return;
        }

        if (TryResolveCategoryOverride(string.Empty, string.Empty, requestedSoundGroupName, entity, out float categoryOverride))
        {
            TryScalePlayingOnEntitySources(entity.entityId, requestedSoundGroupName, categoryOverride);
            TryScaleLoopingOnEntitySources(entity.entityId, requestedSoundGroupName, categoryOverride);
            return;
        }
    }

    public static void EnforceAugerOverridePerFrame()
    {
        if (!AudioOptionsPlusConfig.Enabled)
        {
            return;
        }

        List<AudioSource> sources = Audio.Manager.playingAudioSources;
        if (sources == null || sources.Count == 0)
        {
                    return; // Early exit if there are no sources
        }

        for (int i = 0; i < sources.Count; i++)
        {
            AudioSource source = sources[i];
            if (source == null)
            {
                continue;
            }

            int sourceId = source.GetInstanceID();
            string clipName = source.clip != null ? (source.clip.name ?? string.Empty) : string.Empty;
            string sourceName = source.name ?? string.Empty;
            if (TryResolveCategoryOverride(clipName, sourceName, string.Empty, null, out float resolvedOverride, source))
            {
                if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
                {
                    OriginalVolumesBySourceId[sourceId] = source.volume;
                }

                float original = OriginalVolumesBySourceId[sourceId];
                float finalVolume = Mathf.Clamp01(original * resolvedOverride);
                source.volume = finalVolume;
                ForcedSourceVolumesById[sourceId] = finalVolume;
            }
            else
            {
                // Source may be reused for a different sound later; drop stale forced overrides.
                ForcedSourceVolumesById.Remove(sourceId);
                RestoreSource(sourceId, source);
            }
        }
    }

    private static float? GetMultiplierFor(AudioSource source, AudioClip clip)
    {
        string clipName = clip != null ? (clip.name ?? string.Empty) : string.Empty;
        string sourceName = source != null ? (source.name ?? string.Empty) : string.Empty;

        if (AudioOptionsPlusConfig.AugerMultiplier < 1f
            && (clipName.IndexOf("auger", StringComparison.OrdinalIgnoreCase) >= 0
                || sourceName.IndexOf("auger", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            return AudioOptionsPlusConfig.AugerMultiplier;
        }

        foreach (AudioOptionsPlusConfig.SoundRule rule in AudioOptionsPlusConfig.Rules)
        {
            if (rule == null || rule.Keyword.Length == 0)
            {
                continue;
            }

            bool clipMatch = clipName.IndexOf(rule.Keyword, StringComparison.OrdinalIgnoreCase) >= 0;
            bool sourceMatch = sourceName.IndexOf(rule.Keyword, StringComparison.OrdinalIgnoreCase) >= 0;

            if (rule.Target == "clip" && clipMatch)
            {
                return rule.Multiplier;
            }

            if (rule.Target == "source" && sourceMatch)
            {
                return rule.Multiplier;
            }

            if (rule.Target == "any" && (clipMatch || sourceMatch))
            {
                return rule.Multiplier;
            }
        }

        return null;
    }

    private static float? GetMultiplierForGroup(Entity entity, string soundGroupName)
    {
        string group = soundGroupName ?? string.Empty;

        if (TryResolveCategoryOverride(string.Empty, string.Empty, group, entity, out float categoryOverride))
        {
            return Mathf.Clamp01(categoryOverride);
        }

        if (AudioOptionsPlusConfig.AugerMultiplier < 1f
            && group.IndexOf("auger", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return AudioOptionsPlusConfig.AugerMultiplier;
        }

        foreach (AudioOptionsPlusConfig.SoundRule rule in AudioOptionsPlusConfig.Rules)
        {
            if (rule == null || rule.Keyword.Length == 0)
            {
                continue;
            }

            bool groupMatch = group.IndexOf(rule.Keyword, StringComparison.OrdinalIgnoreCase) >= 0;
            if (!groupMatch)
            {
                continue;
            }

            if (rule.Target == "group" || rule.Target == "any")
            {
                return rule.Multiplier;
            }
        }

        return null;
    }

    private static bool IsFromLocalPlayer(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }

        try
        {
            EntityPlayer player = source.GetComponentInParent<EntityPlayer>();
            if (player == null)
            {
                // Non-player-attached sources are allowed when playerOnly is enabled.
                return true;
            }

            if (player is EntityPlayerLocal)
            {
                return true;
            }

            EntityPlayer primaryPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            return primaryPlayer != null && primaryPlayer.entityId == player.entityId;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsFromLocalPlayerEntity(Entity entity)
    {
        if (entity == null)
        {
            return false;
        }

        if (!(entity is EntityPlayer))
        {
            // Non-player entities (vehicles, stations, world emitters) should be eligible.
            return true;
        }

        if (entity is EntityPlayerLocal)
        {
            return true;
        }

        try
        {
            EntityPlayer primaryPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            return primaryPlayer != null && primaryPlayer.entityId == entity.entityId;
        }
        catch
        {
            return false;
        }
    }

    private static void RestoreSource(int sourceId, AudioSource source)
    {
        if (!OriginalVolumesBySourceId.TryGetValue(sourceId, out float original))
        {
            return;
        }

        source.volume = original;
        OriginalVolumesBySourceId.Remove(sourceId);
    }

    private static void ApplyMultiplierToSequenceObject(object sequenceObj, float multiplier)
    {
        if (sequenceObj == null)
        {
            return;
        }

        ApplyToSequenceField(sequenceObj, "nearStart", multiplier);
        ApplyToSequenceField(sequenceObj, "nearLoop", multiplier);
        ApplyToSequenceField(sequenceObj, "nearEnd", multiplier);
        ApplyToSequenceField(sequenceObj, "farStart", multiplier);
        ApplyToSequenceField(sequenceObj, "farLoop", multiplier);
        ApplyToSequenceField(sequenceObj, "farEnd", multiplier);
    }

    private static void SetSequenceObjectAbsoluteVolume(object sequenceObj, float absoluteVolume)
    {
        if (sequenceObj == null)
        {
            return;
        }

        SetSequenceFieldAbsoluteVolume(sequenceObj, "nearStart", absoluteVolume);
        SetSequenceFieldAbsoluteVolume(sequenceObj, "nearLoop", absoluteVolume);
        SetSequenceFieldAbsoluteVolume(sequenceObj, "nearEnd", absoluteVolume);
        SetSequenceFieldAbsoluteVolume(sequenceObj, "farStart", absoluteVolume);
        SetSequenceFieldAbsoluteVolume(sequenceObj, "farLoop", absoluteVolume);
        SetSequenceFieldAbsoluteVolume(sequenceObj, "farEnd", absoluteVolume);
    }

    private static void ApplyToSequenceField(object sequenceObj, string fieldName, float multiplier)
    {
        FieldInfo field = sequenceObj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        GameObject go = field.GetValue(sequenceObj) as GameObject;
        if (go == null)
        {
            return;
        }

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * multiplier);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    private static void SetSequenceFieldAbsoluteVolume(object sequenceObj, string fieldName, float absoluteVolume)
    {
        FieldInfo field = sequenceObj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        GameObject go = field.GetValue(sequenceObj) as GameObject;
        if (go == null)
        {
            return;
        }

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * absoluteVolume);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    private static void TryScalePlayingOnEntitySources(int entityId, string requestedSoundGroupName, float multiplier)
    {
        if (PlayingOnEntityField == null)
        {
            return;
        }

        try
        {
            object playingObj = PlayingOnEntityField.GetValue(null);
            if (!(playingObj is IDictionary playingOnEntity) || !playingOnEntity.Contains(entityId))
            {
                return;
            }

            object channelsObj = playingOnEntity[entityId];
            if (channelsObj == null)
            {
                return;
            }

            FieldInfo environmentField = channelsObj.GetType().GetField("environment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object environmentObj = environmentField?.GetValue(channelsObj);
            FieldInfo dictField = environmentObj?.GetType().GetField("dict", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object dictObj = dictField?.GetValue(environmentObj);
            if (!(dictObj is IDictionary envDict))
            {
                return;
            }

            foreach (DictionaryEntry envEntry in envDict)
            {
                string groupName = envEntry.Key as string;
                if (!SoundGroupMatches(requestedSoundGroupName, groupName))
                {
                    continue;
                }

                if (!(envEntry.Value is IEnumerable sourceList))
                {
                    continue;
                }

                foreach (object sourceObj in sourceList)
                {
                    if (sourceObj is AudioSource source && source != null)
                    {
                        int sourceId = source.GetInstanceID();
                        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
                        {
                            OriginalVolumesBySourceId[sourceId] = source.volume;
                        }

                        float original = OriginalVolumesBySourceId[sourceId];
                        float finalVolume = Mathf.Clamp01(original * multiplier);
                        source.volume = finalVolume;
                        ForcedSourceVolumesById[sourceId] = finalVolume;
                    }
                }
            }
        }
        catch
        {
            // Best-effort only.
        }
    }

    private static void TryScaleLoopingOnEntitySources(int entityId, string requestedSoundGroupName, float multiplier)
    {
        if (LoopingOnEntityField == null)
        {
            return;
        }

        try
        {
            object loopingObj = LoopingOnEntityField.GetValue(null);
            if (!(loopingObj is IDictionary loopingOnEntity) || !loopingOnEntity.Contains(entityId))
            {
                return;
            }

            object perEntityObj = loopingOnEntity[entityId];
            if (!(perEntityObj is IDictionary perEntity))
            {
                return;
            }

            foreach (DictionaryEntry entry in perEntity)
            {
                string groupName = entry.Key as string;
                if (!SoundGroupMatches(requestedSoundGroupName, groupName))
                {
                    continue;
                }

                object nearAndFarObj = entry.Value;
                if (nearAndFarObj == null)
                {
                    continue;
                }

                ApplyToNearFarField(nearAndFarObj, "near", multiplier);
                ApplyToNearFarField(nearAndFarObj, "far", multiplier);
            }
        }
        catch
        {
            // Best-effort only.
        }
    }

    private static void TrySetPlayingOnEntityAbsolute(int entityId, string requestedSoundGroupName, float absoluteVolume)
    {
        if (PlayingOnEntityField == null)
        {
            return;
        }

        try
        {
            object playingObj = PlayingOnEntityField.GetValue(null);
            if (!(playingObj is IDictionary playingOnEntity) || !playingOnEntity.Contains(entityId))
            {
                return;
            }

            object channelsObj = playingOnEntity[entityId];
            if (channelsObj == null)
            {
                return;
            }

            FieldInfo environmentField = channelsObj.GetType().GetField("environment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object environmentObj = environmentField?.GetValue(channelsObj);
            FieldInfo dictField = environmentObj?.GetType().GetField("dict", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object dictObj = dictField?.GetValue(environmentObj);
            if (!(dictObj is IDictionary envDict))
            {
                return;
            }

            foreach (DictionaryEntry envEntry in envDict)
            {
                string groupName = envEntry.Key as string;
                if (!SoundGroupMatches(requestedSoundGroupName, groupName))
                {
                    continue;
                }

                if (!(envEntry.Value is IEnumerable sourceList))
                {
                    continue;
                }

                foreach (object sourceObj in sourceList)
                {
                    if (sourceObj is AudioSource source && source != null)
                    {
                        int sourceId = source.GetInstanceID();
                        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
                        {
                            OriginalVolumesBySourceId[sourceId] = source.volume;
                        }

                        float original = OriginalVolumesBySourceId[sourceId];
                        float finalVolume = Mathf.Clamp01(original * absoluteVolume);
                        source.volume = finalVolume;
                        ForcedSourceVolumesById[sourceId] = finalVolume;
                    }
                }
            }
        }
        catch
        {
            // Best-effort only.
        }
    }

    private static void TrySetLoopingOnEntityAbsolute(int entityId, string requestedSoundGroupName, float absoluteVolume)
    {
        if (LoopingOnEntityField == null)
        {
            return;
        }

        try
        {
            object loopingObj = LoopingOnEntityField.GetValue(null);
            if (!(loopingObj is IDictionary loopingOnEntity) || !loopingOnEntity.Contains(entityId))
            {
                return;
            }

            object perEntityObj = loopingOnEntity[entityId];
            if (!(perEntityObj is IDictionary perEntity))
            {
                return;
            }

            foreach (DictionaryEntry entry in perEntity)
            {
                string groupName = entry.Key as string;
                if (!SoundGroupMatches(requestedSoundGroupName, groupName))
                {
                    continue;
                }

                object nearAndFarObj = entry.Value;
                if (nearAndFarObj == null)
                {
                    continue;
                }

                SetNearFarAbsoluteVolume(nearAndFarObj, "near", absoluteVolume);
                SetNearFarAbsoluteVolume(nearAndFarObj, "far", absoluteVolume);
            }
        }
        catch
        {
            // Best-effort only.
        }
    }

    private static void ApplyToNearFarField(object nearAndFarObj, string fieldName, float multiplier)
    {
        FieldInfo field = nearAndFarObj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        GameObject go = field.GetValue(nearAndFarObj) as GameObject;
        if (go == null)
        {
            return;
        }

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * multiplier);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    private static void SetNearFarAbsoluteVolume(object nearAndFarObj, string fieldName, float absoluteVolume)
    {
        FieldInfo field = nearAndFarObj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        GameObject go = field.GetValue(nearAndFarObj) as GameObject;
        if (go == null)
        {
            return;
        }

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!OriginalVolumesBySourceId.ContainsKey(sourceId))
        {
            OriginalVolumesBySourceId[sourceId] = source.volume;
        }

        float original = OriginalVolumesBySourceId[sourceId];
        float finalVolume = Mathf.Clamp01(original * absoluteVolume);
        source.volume = finalVolume;
        ForcedSourceVolumesById[sourceId] = finalVolume;
    }

    private static bool TryGetAugerOverrideVolume(AudioSource source, AudioClip clip, out float absoluteVolume)
    {
        absoluteVolume = 0f;

        if (AudioOptionsPlusConfig.AugerMultiplier < 0f || AudioOptionsPlusConfig.AugerMultiplier > 1f)
        {
            return false;
        }

        string clipName = clip != null ? (clip.name ?? string.Empty) : string.Empty;
        string sourceName = source != null ? (source.name ?? string.Empty) : string.Empty;

        bool isToolsMachinery = MatchesAny(
            clipName,
            sourceName,
            string.Empty,
            "auger",
            "auger_fire",
            "impactdriver",
            "drill",
            "nailgun",
            "chainsaw",
            "chainsaw_fire",
            "fire_rev",
            "motorweap",
            "motorized");

        if (!isToolsMachinery)
        {
            return false;
        }

        absoluteVolume = AudioOptionsPlusConfig.AugerMultiplier;
        return true;
    }

    private static bool TryResolveCategoryOverride(string clipName, string sourceName, string groupName, Entity contextEntity, out float absoluteVolume, AudioSource sourceObject = null)
    {
        absoluteVolume = 0f;

        string clip = clipName ?? string.Empty;
        string source = sourceName ?? string.Empty;
        string group = groupName ?? string.Empty;
        RepairActionContext repairContext = GetCurrentRepairActionContext();
        bool isRepairUpgradeContext = repairContext != RepairActionContext.None;

        // Do not filter by source ownership here; mixed-source events can otherwise bypass category scaling.

        // Repair/upgrade action sounds can come through alternate source paths. Force them to the
        // Place / Upgrade / Repair slider while action context is active to avoid unscaled layers.
        if (isRepairUpgradeContext && AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier >= 0f)
        {
            absoluteVolume = AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier;
            return true;
        }

        // 1) Auger / Chainsaw tools and heavy machinery.
        if (AudioOptionsPlusConfig.AugerChainsawMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "auger",
                "auger_fire",
                "chainsaw",
                "chainsaw_fire",
                "fire_rev",
                "impactdriver",
                "drill",
                "nailgun",
                "motorweap",
                "motorized")
            && !MatchesAny(clip, source, group, "chainsawhit", "chainsawgraze", "augerhit", "augergraze"))
        {
            absoluteVolume = AudioOptionsPlusConfig.AugerChainsawMultiplier;
            return true;
        }

        // 2) Impact / Harvest / Surface.
        if (AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "impact",
                "hit",
                "destroy",
                "harvest",
                "graze",
                "collision",
                "collide",
                "crash",
                "thud",
                "thump",
                "bounce",
                "slide",
                "audiosource_impact",
                "hitentitysound",
                "vomitimpact",
                "zpack_impact")
            && !MatchesAny(clip, source, group, "impactbody", "impact_driver", "waterblockimpact", "keystone_impact_overlay", "keystone_destroyed", "trapdoor_trigger", "playerland", "player1jump", "player2jump", "player1land", "player2land", "playerlandthump", "zombielandthump"))
        {
            absoluteVolume = AudioOptionsPlusConfig.ImpactHarvestSurfaceMultiplier;
            return true;
        }

        // 3) Gun Fire.
        if (AudioOptionsPlusConfig.GunFireMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "weaponfire",
                "weapon_fire",
                "_fire",
                "fire_end",
                "_s_fire",
                "pistol_fire",
                "smg_fire",
                "shotgun_fire",
                "rifle_fire",
                "ak47_fire",
                "m60_fire",
                "sniperrifle_fire",
                "turret_fire",
                "junkturret_fire",
                "darttrap_fire")
            && !MatchesAny(clip, source, group, "auger", "chainsaw", "nailgun", "forge_fire", "fire_small", "fire_medium"))
        {
            absoluteVolume = AudioOptionsPlusConfig.GunFireMultiplier;
            return true;
        }

        // 4) Explosions.
        if (AudioOptionsPlusConfig.ExplosionsMultiplier >= 0f
            && MatchesAny(clip, source, group, "explosion", "explode", "rocket", "molotov"))
        {
            absoluteVolume = AudioOptionsPlusConfig.ExplosionsMultiplier;
            return true;
        }

        // 5) Vehicles (No Horn).
        if (AudioOptionsPlusConfig.VehiclesNoHornMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "vehicle",
                "engine_fire",
                "motorcycle",
                "motorbike",
                "minibike",
                "suv",
                "gyro",
                "gyrocopter",
                "bicycle",
                "vwheel",
                "vehicle_turbo",
                "service_vehicle")
            && !MatchesAny(clip, source, group, "horn"))
        {
            absoluteVolume = AudioOptionsPlusConfig.VehiclesNoHornMultiplier;
            return true;
        }

        // 6) Electrical / Block Loops.
        if (AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "generator",
                "electric",
                "fence",
                "trip_wire",
                "tripwire",
                "forge_burn_fuel",
                "forge_fire_die",
                "flametrap",
                "bladetrap",
                "fuse_lp",
                "buff_burn_lp",
                "underwater_lp",
                "tvcrt_lp",
                "drone_fly",
                "drone_idle_hover",
                "turret_idle_lp",
                "turret_overheat_lp",
                "turret_retarget_lp"))
        {
            absoluteVolume = AudioOptionsPlusConfig.ElectricalBlockLoopsMultiplier;
            return true;
        }

        // 7) Crafting Complete UI.
        if (AudioOptionsPlusConfig.CraftingCompleteUiMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "campfire_complete_item",
                "chem_station_complete_item",
                "cement_mixer_complete",
                "collector_complete_item",
                "forge_item_complete",
                "craft_complete_item",
                "workbench_complete",
                "campfire_complete",
                "chem_station_complete",
                "collector_complete",
                "item_complete",
                "_complete_item",
                "_item_complete"))
        {
            absoluteVolume = AudioOptionsPlusConfig.CraftingCompleteUiMultiplier;
            return true;
        }

        // 8) Spider Zombie Specific.
        if (AudioOptionsPlusConfig.SpiderZombieMultiplier >= 0f
            && MatchesAny(clip, source, group, "spider"))
        {
            absoluteVolume = AudioOptionsPlusConfig.SpiderZombieMultiplier;
            return true;
        }

        string basis = (_soundBasisContext != null && _soundBasisContext.Count > 0)
            ? (_soundBasisContext.Peek() ?? string.Empty)
            : string.Empty;

        // 9) Animal Pain / Death.
        if (AudioOptionsPlusConfig.AnimalPainDeathMultiplier >= 0f
            && MatchesAny(
                clip,
                source,
                group,
                "beardeath",
                "beargiveup",
                "bearpain",
                "boardeath",
                "boargiveup",
                "boarpain",
                "chickendeath",
                "chickenpain",
                "rabbitdeath",
                "rabbitpain",
                "snakepain",
                "stagdeath",
                "stagpain",
                "vulturedeath",
                "vulturegiveup",
                "vulturepain",
                "wolfdeath",
                "wolfdiredeath",
                "wolfdiregiveup",
                "wolfdirepain",
                "wolfgiveup",
                "wolfpain",
                "mliondeath",
                "mliongiveup",
                "mlionpain",
                "zombiebear",
                "zombievulture",
                "zombiewolf",
                "zombieboar",
                "zombieanimal",
                "zombiedogdeath",
                "zombiedoggiveup",
                "zombiedogpain")
            || (AudioOptionsPlusConfig.AnimalPainDeathMultiplier >= 0f
                && MatchesAny(
                    basis,
                    string.Empty,
                    string.Empty,
                    "beardeath",
                    "beargiveup",
                    "bearpain",
                    "boardeath",
                    "boargiveup",
                    "boarpain",
                    "chickendeath",
                    "chickenpain",
                    "rabbitdeath",
                    "rabbitpain",
                    "snakepain",
                    "stagdeath",
                    "stagpain",
                    "vulturedeath",
                    "vulturegiveup",
                    "vulturepain",
                    "wolfdeath",
                    "wolfdiredeath",
                    "wolfdiregiveup",
                    "wolfdirepain",
                    "wolfgiveup",
                    "wolfpain",
                    "mliondeath",
                    "mliongiveup",
                    "mlionpain",
                    "zombiebear",
                    "zombievulture",
                    "zombiewolf",
                    "zombieboar",
                    "zombieanimal",
                    "zombiedogdeath",
                    "zombiedoggiveup",
                    "zombiedogpain")))
        {
            absoluteVolume = AudioOptionsPlusConfig.AnimalPainDeathMultiplier;
            return true;
        }

        // 10) Place / Upgrade / Repair Blocks.
        if (AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier >= 0f
            && MatchesAny(clip, source, group, "keystone_placed", "repair_block", "place_block_", "place_cobblestone", "placeblock"))
        {
            absoluteVolume = AudioOptionsPlusConfig.PlaceUpgradeRepairMultiplier;
            return true;
        }

        // 11) Protection dings (land claim / trader protected block warning).
        if (AudioOptionsPlusConfig.ProtectionDingsMultiplier >= 0f
            && MatchesAny(clip, source, group, "keystone_build_warning", "keystone_impact_overlay", "world_border_collision"))
        {
            absoluteVolume = AudioOptionsPlusConfig.ProtectionDingsMultiplier;
            return true;
        }

        // 12) Interaction Prompts.
        if (AudioOptionsPlusConfig.InteractionPromptsMultiplier >= 0f
            && (MatchesAny(clip, source, group, "_grab", "_place", "open_", "close_", "_open", "_close", "_click", "_craft")
                || MatchesAny(clip, source, group, "itemneedsrepair", "missingitemtorepair", "craft_place_item", "craft_repair_item", "ui_trader_inv_reset", "ui_trader_purchase", "item_pickup", "item_plant_pickup", "pickup_meat"))
            && !MatchesAny(clip, source, group, "door", "hatch", "vault", "cellar", "bridge", "garage", "manhole", "rollup_gate", "gate_chainlink", "gate_wood_large")
            && !MatchesAny(clip, source, group, "ui_menu_", "ui_hover", "ui_tab", "ui_waypoint_", "ui_skill_purchase", "ui_weather_alert", "ui_challenge_", "ui_loot_", "ui_vending_purchase", "tooltip_popup", "buttonclick", "map_zoom_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.InteractionPromptsMultiplier;
            return true;
        }

        // 13) Twitch sounds.
        if (AudioOptionsPlusConfig.TwitchMultiplier >= 0f
            && MatchesAny(clip, source, group, "twitch_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TwitchMultiplier;
            return true;
        }

        // 14) Doors / Hatches / Vaults / Cellars / Bridge.
        if (AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier >= 0f
            && MatchesAny(clip, source, group, "door", "hatch", "vault", "cellar", "bridge", "garage", "manhole", "rollup_gate", "gate_chainlink", "gate_wood_large"))
        {
            absoluteVolume = AudioOptionsPlusConfig.DoorsHatchesVaultsCellarsBridgeMultiplier;
            return true;
        }

        // 15-19) Trader-specific categories.
        if (AudioOptionsPlusConfig.TraderBobMultiplier >= 0f && MatchesAny(clip, source, group, "trader_bob_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TraderBobMultiplier;
            return true;
        }

        if (AudioOptionsPlusConfig.TraderHughMultiplier >= 0f && MatchesAny(clip, source, group, "trader_hugh_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TraderHughMultiplier;
            return true;
        }

        if (AudioOptionsPlusConfig.TraderJenMultiplier >= 0f && MatchesAny(clip, source, group, "trader_jen_", "trader_jenlike"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TraderJenMultiplier;
            return true;
        }

        if (AudioOptionsPlusConfig.TraderJoelMultiplier >= 0f && MatchesAny(clip, source, group, "trader_joel_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TraderJoelMultiplier;
            return true;
        }

        if (AudioOptionsPlusConfig.TraderRektMultiplier >= 0f && MatchesAny(clip, source, group, "trader_rekt_"))
        {
            absoluteVolume = AudioOptionsPlusConfig.TraderRektMultiplier;
            return true;
        }

        // 20) Player-made sounds.
        bool isLocalPlayerEntityContext = contextEntity is EntityPlayer entityPlayer && IsLocalOrPrimaryPlayer(entityPlayer);
        bool isLocalPlayerSourceContext = IsAttachedToLocalPlayer(sourceObject);
        if (AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier >= 0f
            && (MatchesAny(clip, source, group, "player", "playermale", "playerfemale", "playerspawn", "playerland", "player_death_stinger", "player1jump", "player2jump", "player1land", "player2land", "landsoft", "landhard", "landthump", "a_")
                || MatchesAny(clip, source, group, "player1", "player2")
                || ((isLocalPlayerEntityContext || isLocalPlayerSourceContext)
                    && MatchesAny(clip, source, group, "runloop", "footstep", "step", "heelstep", "barestep", "swing", "swinglight", "swingheavy", "swoosh", "slowswoosh", "fpv_motion_light", "fpv_motion_heavy", "holster", "unholster", "equip", "weapon_holster", "weapon_unholster", "generic_holster", "generic_unholster"))))
        {
            absoluteVolume = AudioOptionsPlusConfig.PlayerMadeSoundsMultiplier;
            return true;
        }

        return false;
    }

    private static bool TryGetAugerOverrideVolumeForGroup(Entity entity, string soundGroupName, out float absoluteVolume)
    {
        absoluteVolume = 0f;

        if (AudioOptionsPlusConfig.AugerMultiplier < 0f || AudioOptionsPlusConfig.AugerMultiplier > 1f)
        {
            return false;
        }

        string group = soundGroupName ?? string.Empty;

        bool isToolsMachineryGroup = MatchesAny(group, string.Empty, string.Empty, "auger", "impactdriver", "drill", "nailgun");
        bool isImpactGroupFromAuger = IsAugerImpactGroup(group) && IsEntityUsingAuger(entity);
        if (!isToolsMachineryGroup && !isImpactGroupFromAuger)
        {
            return false;
        }

        if (isImpactGroupFromAuger && AudioOptionsPlusConfig.ImpactMultiplier >= 0f)
        {
            absoluteVolume = AudioOptionsPlusConfig.ImpactMultiplier;
            return true;
        }

        absoluteVolume = AudioOptionsPlusConfig.AugerMultiplier;
        return true;
    }

    private static bool MatchesAny(string clip, string source, string group, params string[] keywords)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            string k = keywords[i];
            if (string.IsNullOrEmpty(k))
            {
                continue;
            }

            if (clip.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0
                || source.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0
                || group.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttachedToLocalPlayer(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }

        try
        {
            EntityPlayer player = source.GetComponentInParent<EntityPlayer>();
            return player != null && IsLocalOrPrimaryPlayer(player);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsLocalOrPrimaryPlayer(EntityPlayer player)
    {
        if (player == null)
        {
            return false;
        }

        if (player is EntityPlayerLocal)
        {
            return true;
        }

        try
        {
            EntityPlayer primaryPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            return primaryPlayer != null && primaryPlayer.entityId == player.entityId;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAugerImpactGroup(string group)
    {
        if (string.IsNullOrEmpty(group))
        {
            return false;
        }

        return group.StartsWith("metalhit", StringComparison.OrdinalIgnoreCase)
            || group.StartsWith("impact_metal_on_", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEntityUsingAuger(Entity entity)
    {
        if (!(entity is EntityAlive alive) || alive.inventory == null)
        {
            return false;
        }

        try
        {
            ItemValue held = alive.inventory.holdingItemItemValue;
            if (held == null || held.ItemClass == null)
            {
                return false;
            }

            string className = held.ItemClass.GetItemName();
            if (!string.IsNullOrEmpty(className) && className.IndexOf("auger", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            string valueText = held.ToString();
            return !string.IsNullOrEmpty(valueText) && valueText.IndexOf("auger", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool SoundGroupMatches(string requestedSoundGroupName, string groupName)
    {
        if (string.IsNullOrEmpty(requestedSoundGroupName) || string.IsNullOrEmpty(groupName))
        {
            return false;
        }

        return groupName.IndexOf(requestedSoundGroupName, StringComparison.OrdinalIgnoreCase) >= 0
            || requestedSoundGroupName.IndexOf(groupName, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool TryResolveSoundSwap(Entity entity, string basisName, out string replacement)
    {
        replacement = null;

        if (!TryResolveSwapEventType(basisName, out string eventType))
        {
            return false;
        }

        if (!TryResolveSoundSwapMode(entity, basisName, out string mode))
        {
            return false;
        }

        if (!ModeAllowsEvent(mode, eventType))
        {
            return false;
        }

        if (TryResolvePerAnimalSwapReplacement(basisName, eventType, out replacement))
        {
            return !string.IsNullOrEmpty(replacement);
        }

        return false;
    }

    private static bool TryResolvePerAnimalSwapReplacement(string basisName, string eventType, out string replacement)
    {
        replacement = null;

        string name = basisName ?? string.Empty;
        bool isDeath = string.Equals(eventType, "Death", StringComparison.OrdinalIgnoreCase);

        if (name.IndexOf("rabbit", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = RabbitSwapSound;
            return true;
        }

        if (name.IndexOf("bear", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "bearattack" : "bearsense";
            return true;
        }

        if (name.IndexOf("boar", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "boarattack" : "boarsense";
            return true;
        }

        if (name.IndexOf("chicken", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = "chickenroam";
            return true;
        }

        if (name.IndexOf("snake", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "snakeattack" : "snakesense";
            return true;
        }

        if (name.IndexOf("stag", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = "stagalert";
            return true;
        }

        if (name.IndexOf("vulture", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "vultureattack" : "vulturesense";
            return true;
        }

        if (name.IndexOf("wolfdire", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "wolfdireattack" : "wolfdiresense";
            return true;
        }

        if (name.IndexOf("wolf", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "wolfattack" : "wolfsense";
            return true;
        }

        if (name.IndexOf("mlion", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("mountainlion", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "mlionattack" : "mlionsense";
            return true;
        }

        if (name.IndexOf("zombiedog", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            replacement = isDeath ? "zombiedogattack" : "zombiedogsense";
            return true;
        }

        return false;
    }

    private static bool TryResolveSwapEventType(string basisName, out string eventType)
    {
        eventType = null;
        string name = basisName ?? string.Empty;
        if (name.Length == 0)
        {
            return false;
        }

        if (name.IndexOf("death", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            eventType = "Death";
            return true;
        }

        if (name.IndexOf("pain", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            eventType = "Pain";
            return true;
        }

        // Treat giveup as the non-death animal reaction bucket for accessibility swaps.
        if (name.IndexOf("giveup", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            eventType = "Pain";
            return true;
        }

        return false;
    }

    private static bool ModeAllowsEvent(string mode, string eventType)
    {
        string normalizedMode = AudioOptionsPlusConfig.NormalizeSoundSwapMode(mode);
        if (string.Equals(normalizedMode, "Both", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(normalizedMode, "None", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(normalizedMode, eventType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveSoundSwapMode(Entity entity, string basisName, out string mode)
    {
        mode = "None";

        string text = basisName ?? string.Empty;

        if (text.IndexOf("bear", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapBearMode;
            return true;
        }

        if (text.IndexOf("boar", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapBoarMode;
            return true;
        }

        if (text.IndexOf("chicken", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapChickenMode;
            return true;
        }

        if (text.IndexOf("rabbit", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapRabbitMode;
            return true;
        }

        if (text.IndexOf("snake", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapSnakeMode;
            return true;
        }

        if (text.IndexOf("stag", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapStagMode;
            return true;
        }

        if (text.IndexOf("vulture", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapVultureMode;
            return true;
        }

        if (text.IndexOf("wolfdire", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapDireWolfMode;
            return true;
        }

        if (text.IndexOf("wolf", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapWolfMode;
            return true;
        }

        if (text.IndexOf("mlion", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("mountainlion", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapMountainLionMode;
            return true;
        }

        if (text.IndexOf("zombiedog", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            mode = AudioOptionsPlusConfig.SoundSwapZombieDogMode;
            return true;
        }

        return false;
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.Play), new Type[] { })]
internal static class Patch_AudioSource_Play
{
    private static void Prefix(AudioSource __instance)
    {
        AudioOptionsPlusRuntime.ApplyToSource(__instance);
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.Play), new[] { typeof(ulong) })]
internal static class Patch_AudioSource_PlayDelayedSamples
{
    private static void Prefix(AudioSource __instance)
    {
        AudioOptionsPlusRuntime.ApplyToSource(__instance);
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.PlayDelayed), new[] { typeof(float) })]
internal static class Patch_AudioSource_PlayDelayedSeconds
{
    private static void Prefix(AudioSource __instance)
    {
        AudioOptionsPlusRuntime.ApplyToSource(__instance);
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.PlayOneShot), new[] { typeof(AudioClip), typeof(float) })]
internal static class Patch_AudioSource_PlayOneShot
{
    private static void Prefix(AudioSource __instance, AudioClip clip, ref float volumeScale)
    {
        volumeScale = AudioOptionsPlusRuntime.ScaleOneShot(__instance, clip, volumeScale);
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.PlayOneShot), new[] { typeof(AudioClip) })]
internal static class Patch_AudioSource_PlayOneShot_NoScale
{
    private struct OneShotState
    {
        public bool Changed;
        public float OriginalVolume;
    }

    private static void Prefix(AudioSource __instance, AudioClip clip, ref OneShotState __state)
    {
        if (AudioOptionsPlusRuntime.TryScaleSourceVolumeForOneShot(__instance, clip, out float originalVolume))
        {
            __state = new OneShotState
            {
                Changed = true,
                OriginalVolume = originalVolume
            };
        }
    }

    private static void Postfix(AudioSource __instance, OneShotState __state)
    {
        if (!__state.Changed || __instance == null)
        {
            return;
        }

        __instance.volume = __state.OriginalVolume;
    }
}

[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.PlayClipAtPoint), new[] { typeof(AudioClip), typeof(Vector3), typeof(float) })]
internal static class Patch_AudioSource_PlayClipAtPoint
{
    private static void Prefix(AudioClip clip, ref float volume)
    {
        volume = AudioOptionsPlusRuntime.ScaleClipAtPoint(clip, volume);
    }
}

[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.Play), new[] { typeof(Entity), typeof(string), typeof(float), typeof(bool) })]
internal static class Patch_Audio_Manager_Play_Entity
{
    private static void Prefix(Entity _entity, ref string soundGroupName, ref float volumeScale)
    {
        string originalSoundGroupName = soundGroupName;
        soundGroupName = AudioOptionsPlusRuntime.ResolveSoundSwapForGroup(_entity, soundGroupName);
        AudioOptionsPlusRuntime.BeginEntityAudioContext(_entity, soundGroupName, originalSoundGroupName);
        volumeScale = AudioOptionsPlusRuntime.ScaleManagerPlayVolume(_entity, soundGroupName, volumeScale);
    }

    private static void Postfix(Entity _entity, string soundGroupName)
    {
        AudioOptionsPlusRuntime.ApplyToActiveEntityGroupSources(_entity, soundGroupName);
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }

    private static void Finalizer()
    {
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }
}

[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.Play), new[] { typeof(Vector3), typeof(string), typeof(int), typeof(bool) })]
internal static class Patch_Audio_Manager_Play_Position
{
    private static void Prefix(Vector3 position, ref string soundGroupName, int entityId, ref bool wantHandle)
    {
        string originalSoundGroupName = soundGroupName;
        Entity contextEntity = null;
        try
        {
            if (entityId >= 0)
            {
                contextEntity = GameManager.Instance?.World?.GetEntity(entityId);
            }
        }
        catch
        {
            contextEntity = null;
        }

        soundGroupName = AudioOptionsPlusRuntime.ResolveSoundSwapForGroup(contextEntity, soundGroupName);
        AudioOptionsPlusRuntime.BeginEntityAudioContext(contextEntity, soundGroupName, originalSoundGroupName);
    }

    private static void Postfix()
    {
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }

    private static void Finalizer()
    {
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }
}

[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.PlaySequence), new[] { typeof(Entity), typeof(string) })]
internal static class Patch_Audio_Manager_PlaySequence
{
    private static void Prefix(Entity entity, ref string soundGroupName)
    {
        string originalSoundGroupName = soundGroupName;
        soundGroupName = AudioOptionsPlusRuntime.ResolveSoundSwapForGroup(entity, soundGroupName);
        AudioOptionsPlusRuntime.BeginEntityAudioContext(entity, soundGroupName, originalSoundGroupName);
    }

    private static void Postfix(Entity entity, string soundGroupName)
    {
        AudioOptionsPlusRuntime.ApplyToSequence(entity, soundGroupName);
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }

    private static void Finalizer()
    {
        AudioOptionsPlusRuntime.EndEntityAudioContext();
    }
}

[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.LoadAudio), new[] { typeof(bool), typeof(float), typeof(string), typeof(string) })]
internal static class Patch_Audio_Manager_LoadAudio
{
    private static void Prefix(ref string _clipName)
    {
        _clipName = AudioOptionsPlusRuntime.ResolveSoundSwapForClip(_clipName);
    }

    private static void Postfix(string _clipName, AudioSource __result)
    {
        AudioOptionsPlusRuntime.ApplyLoadAudioOverride(_clipName, __result);
    }
}

[HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.OnHoldingUpdate), new[] { typeof(ItemActionData) })]
internal static class Patch_ItemActionRepair_OnHoldingUpdate
{
    private static void Prefix(ItemActionData _actionData)
    {
        AudioOptionsPlusRuntime.BeginRepairActionContext(_actionData);
    }

    private static void Postfix()
    {
        AudioOptionsPlusRuntime.EndRepairActionContext();
    }

    private static void Finalizer()
    {
        AudioOptionsPlusRuntime.EndRepairActionContext();
    }
}

[HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.ExecuteAction), new[] { typeof(ItemActionData), typeof(bool) })]
internal static class Patch_ItemActionRepair_ExecuteAction
{
    private static void Prefix(ItemActionData _actionData)
    {
        AudioOptionsPlusRuntime.BeginRepairActionContext(_actionData);
    }

    private static void Postfix()
    {
        AudioOptionsPlusRuntime.EndRepairActionContext();
    }

    private static void Finalizer()
    {
        AudioOptionsPlusRuntime.EndRepairActionContext();
    }
}

[HarmonyPatch(typeof(Audio.Manager), nameof(Audio.Manager.FrameUpdate), new Type[] { })]
internal static class Patch_Audio_Manager_FrameUpdate
{
    private static void Postfix()
    {
        AudioOptionsPlusRuntime.EnforceAugerOverridePerFrame();
    }
}

[HarmonyPatch(typeof(Audio.Handle), nameof(Audio.Handle.SetVolume), new[] { typeof(float) })]
internal static class Patch_Audio_Handle_SetVolume
{
    private static void Postfix(AudioSource ___nearSource, AudioSource ___farSource)
    {
        AudioOptionsPlusRuntime.EnforceOnHandleSources(___nearSource, ___farSource);
    }
}

