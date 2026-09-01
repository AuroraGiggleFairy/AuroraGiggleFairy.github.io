using System.Reflection;
using FullautoLauncher.Scripts.ProjectileManager;
using HarmonyLib;

public class FullautoLauncherInit : IModApi
{
	private static bool inited;

	public void InitMod(Mod _modInstance)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (!inited)
		{
			inited = true;
			Log.Out(" Loading Patch: " + GetType());
			Harmony val = new Harmony(GetType().ToString());
			val.PatchAll(Assembly.GetExecutingAssembly());
			((ModEventAbs<ModEventHandlerDelegate<SUnityUpdateData>>)(object)ModEvents.UnityUpdate).RegisterHandler((ModEventHandlerDelegate<SUnityUpdateData>)CustomProjectileManager.Update);
		}
	}
}
