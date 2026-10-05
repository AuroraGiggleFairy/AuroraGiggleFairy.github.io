using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// DoomScreenFx registers on GameStartDone. On a client that often runs before
	/// ScreenEffects exists, so pickup, berserk, and light-amp never get a slot.
	/// Register again once the effect list is actually there.
	/// </summary>
	internal static class ScreenFxRetry
	{
		private static ScreenEffects _hooked;

		internal static void Tick()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			ScreenEffects effects = ScreenEffects.Instance;
			if (effects == null || effects == _hooked)
			{
				return;
			}

			try
			{
				Type api = AccessTools.TypeByName("DoomScreenFx.DoomScreenFxApi");
				Type screen = AccessTools.TypeByName("DoomScreenFx.ScreenFx");
				FieldInfo defs = api != null ? AccessTools.Field(api, "_defs") : null;
				MethodInfo register = screen != null ? AccessTools.Method(screen, "Register") : null;
				object list = defs != null ? defs.GetValue(null) : null;
				if (register == null || list == null || (list is ICollection count && count.Count == 0))
				{
					return;
				}

				if (Has(effects, "doomPickupFlash") && Has(effects, "doomBerserk") && Has(effects, "doomLightAmp"))
				{
					_hooked = effects;
					return;
				}

				register.Invoke(null, new[] { list });
				if (Has(effects, "doomPickupFlash") && Has(effects, "doomBerserk") && Has(effects, "doomLightAmp"))
				{
					_hooked = effects;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] screen effect register: " + e.Message);
				_hooked = effects;
			}
		}

		private static bool Has(ScreenEffects effects, string name)
		{
			if (effects == null)
			{
				return false;
			}

			object loaded = AccessTools.Field(typeof(ScreenEffects), "loadedEffects")?.GetValue(effects);
			if (!(loaded is IList list))
			{
				return false;
			}

			for (int i = 0; i < list.Count; i++)
			{
				object effect = list[i];
				if (effect == null)
				{
					continue;
				}

				object effectName = AccessTools.Field(effect.GetType(), "Name")?.GetValue(effect);
				if (effectName is string text && text == name)
				{
					return true;
				}
			}

			return false;
		}
	}
}
