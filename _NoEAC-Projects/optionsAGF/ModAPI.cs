using System;
using HarmonyLib;

namespace optionsAGF
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                OptionsRegistry.EnsureInitialized();
                _ = OptionsRegistry.GetScreamerModeCached(OptionMode.OnWithNumbers);
                bool hasOnPressed = HarmonyLib.AccessTools.Method(typeof(XUiController), "OnPressed") != null;
                bool hasOnPress = HarmonyLib.AccessTools.Method(typeof(XUiController), "OnPress") != null;
                bool hasSimpleButtonOnPressed = HarmonyLib.AccessTools.Method("XUiV_SimpleButton:OnPressed") != null;
                bool hasSimpleButtonOnPress = HarmonyLib.AccessTools.Method("XUiV_SimpleButton:OnPress") != null;
                bool hasButtonOnPressed = HarmonyLib.AccessTools.Method("XUiV_Button:OnPressed") != null;
                bool hasButtonOnPress = HarmonyLib.AccessTools.Method("XUiV_Button:OnPress") != null;
                var harmony = new Harmony("agf.optionsagf");
                harmony.PatchAll();
                bool screamerLoaded = OptionsRegistry.IsScreamerAlertPresent();
                Console.WriteLine("[optionsAGF] Initialized. Harmony patches applied. screamer_alert_present=" + screamerLoaded
                    + ", hasOnPressed=" + hasOnPressed
                    + ", hasOnPress=" + hasOnPress
                    + ", hasSimpleButtonOnPressed=" + hasSimpleButtonOnPressed
                    + ", hasSimpleButtonOnPress=" + hasSimpleButtonOnPress
                    + ", hasButtonOnPressed=" + hasButtonOnPressed
                    + ", hasButtonOnPress=" + hasButtonOnPress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[optionsAGF] Failed to initialize: " + ex);
            }
        }
    }
}
