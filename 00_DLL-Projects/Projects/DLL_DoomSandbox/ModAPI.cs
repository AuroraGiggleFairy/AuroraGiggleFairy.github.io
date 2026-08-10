using System;
using System.Linq;
using HarmonyLib;

public class ModAPI : IModApi
{
	public void InitMod(Mod modInstance)
	{
		try
		{
			DoomSandbox.DoomSandboxMod.ModInstance = modInstance;
			var harmony = new Harmony("com.doom.doomsandbox");
			harmony.PatchAll();
			int patchCount = 0;
			try { patchCount = harmony.GetPatchedMethods().Count(); } catch { /* ignore */ }
			var line = "[DoomSandbox] InitMod OK. Patches=" + patchCount + " Config=" + DoomSandbox.DoomSandboxMod.ConfigPath;
			Console.WriteLine(line);
			UnityEngine.Debug.Log(line);
		}
		catch (Exception ex)
		{
			Console.WriteLine("[DoomSandbox] InitMod FAILED: " + ex);
			try { UnityEngine.Debug.LogError("[DoomSandbox] InitMod FAILED: " + ex); } catch { }
		}
	}
}
