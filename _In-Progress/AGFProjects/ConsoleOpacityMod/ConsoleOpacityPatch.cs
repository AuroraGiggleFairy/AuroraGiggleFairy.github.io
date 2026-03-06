using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml.Linq;

public static class ConsoleOpacityConfig
{
    public static float Opacity = 0.8f;
    public static void Log(string msg)
    {
        try
        {
            File.AppendAllText(Path.Combine("Mods", "AGFProjects", "ConsoleOpacityMod", "console_opacity_debug.log"), msg+"\n");
        }
        catch { }
    }

    public static void LoadConfig()
    {
        // Get the directory of the currently executing assembly (the mod DLL)
        string modDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string configPath = Path.Combine(modDir, "Config", "ConsoleOpacity.xml");
        Log($"[DEBUG] Loading config from: {configPath}");
        if (File.Exists(configPath))
        {
            try
            {
                string raw = File.ReadAllText(configPath);
                Log($"[DEBUG] Raw config contents: {raw.Replace("\n", " ").Replace("\r", " ")}");
                var xml = XDocument.Load(configPath);
                var opacityElem = xml.Root.Element("Opacity");
                Log($"Opacity element: {opacityElem?.Value}");
                if (opacityElem != null && float.TryParse(opacityElem.Value, out float val))
                {
                    Opacity = Mathf.Clamp(val, 0f, 1f);
                    Log($"Parsed opacity: {Opacity}");
                }
            }
            catch (System.Exception ex) { Log($"Config load error: {ex.Message}"); }
        }
        else
        {
            Log("Config file not found.");
        }
    }
}

// Patch OnOpen to set the background opacity
[HarmonyPatch(typeof(GUIWindowConsole), "OnOpen")]
public class ConsoleOpacityPatch_OnOpen
{
    static void Postfix(GUIWindowConsole __instance)
    {
        try
        {
            // Find all Image components and set opacity only for unnamed 'Image' with alpha 1
            var images = __instance.components.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                // Build the full hierarchy path for this Image
                string path = img.name;
                Transform t = img.transform.parent;
                while (t != null && t != __instance.components.transform)
                {
                    path = t.name + "/" + path;
                    t = t.parent;
                }
                path = __instance.components.name + "/" + path;
                ConsoleOpacityConfig.Log($"Image found: {img.name}, alpha: {img.color.a}, path: {path}");
                // Set opacity for 'Scroll View', 'CommandField', and log 'CloseButton'
                if (img.name == "Scroll View")
                {
                    var c = img.color;
                    c.a = ConsoleOpacityConfig.Opacity;
                    img.color = c;
                    ConsoleOpacityConfig.Log($"Set alpha to {ConsoleOpacityConfig.Opacity} for {img.name}, path: {path}");
                }
                else if (img.name == "CommandField")
                {
                    var c = img.color;
                    float fieldAlpha = Mathf.Clamp01(ConsoleOpacityConfig.Opacity + 0.3f);
                    c.a = fieldAlpha;
                    img.color = c;
                    ConsoleOpacityConfig.Log($"Set alpha to {fieldAlpha} for {img.name}, path: {path}");
                }
                else if (img.name == "CloseButton")
                {
                    bool isActive = img.gameObject.activeInHierarchy;
                    ConsoleOpacityConfig.Log($"[DEBUG] CloseButton active: {isActive}, path: {path}");
                    var c = img.color;
                    c.a = 1.0f;
                    img.color = c;
                    ConsoleOpacityConfig.Log($"Set alpha to 1.0 for {img.name}, path: {path}");
                }
            }
        }
        catch (System.Exception ex)
        {
            ConsoleOpacityConfig.Log($"Error setting console opacity: {ex.Message}");
        }
    }
}
