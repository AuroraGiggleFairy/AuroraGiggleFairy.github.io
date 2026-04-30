using System;
using HarmonyLib;

namespace AudioOptionsPlus;

public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            if (GameManager.IsDedicatedServer)
            {
                Console.WriteLine("[AudioOptionsPlus] Dedicated server detected. Client-only features are disabled.");
                return;
            }

            AudioOptionsPlusConfig.Load();
            new Harmony("com.auroragigglefairy.AudioOptionsPlus").PatchAll();
            ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData _) =>
            {
                AudioOptionsPlusConfig.Load();
                AudioOptionsPlusRuntime.ResetForGameStart();
                Console.WriteLine("[AudioOptionsPlus] GameStartDone: settings reloaded.");
            });
            Console.WriteLine($"[AudioOptionsPlus] Initialized. Enabled={AudioOptionsPlusConfig.Enabled}, PlayerOnly={AudioOptionsPlusConfig.PlayerOnly}, DefaultMultiplier={AudioOptionsPlusConfig.DefaultMultiplier:0.###}, Rules={AudioOptionsPlusConfig.Rules.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioOptionsPlus] Init failed: {ex}");
        }
    }
}

