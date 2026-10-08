using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AGF.Compat.DoomSurvival
{
	/// <summary>
	/// Dan's HUD has no stamina bar, so the vanilla sprint icon never gets a true/false.
	/// The autorun sprite follows Auto Run's cvar. The sprint sprite shows only on a
	/// vehicle, while auto-run or flight assist is using turbo.
	/// </summary>
	public class DoomHudMoveIcon : XUiController
	{
		const string AutoRunCVar = "agf_autorun_active";

		static bool flightAssistLookupDone;
		static MethodInfo flightAssistEnabled;

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (ViewComponent == null)
			{
				return;
			}

			EntityPlayerLocal player = xui?.playerUI?.entityPlayer;
			bool show = false;
			if (player != null)
			{
				show = ViewComponent.ID == "autorun" ? player.GetCVar(AutoRunCVar) > 0.5f : IsAssistTurbo(player);
			}

			if (ViewComponent.IsVisible != show)
			{
				ViewComponent.IsVisible = show;
			}
		}

		static bool IsAssistTurbo(EntityPlayerLocal player)
		{
			EntityVehicle vehicle = player.AttachedToEntity as EntityVehicle;
			if (vehicle == null || vehicle.vehicle == null || !vehicle.vehicle.IsTurbo)
			{
				return false;
			}

			if (player.GetCVar(AutoRunCVar) > 0.5f)
			{
				return true;
			}

			return IsFlightAssistEnabled(vehicle);
		}

		static bool IsFlightAssistEnabled(EntityVehicle vehicle)
		{
			if (!flightAssistLookupDone)
			{
				flightAssistLookupDone = true;
				Type type = AccessTools.TypeByName("AutoRun.Patch_FlightAssist_MoveByAttachedEntity");
				flightAssistEnabled = type?.GetMethod("IsEnabledFor", BindingFlags.Public | BindingFlags.Static);
			}

			if (flightAssistEnabled == null)
			{
				return false;
			}

			try
			{
				return (bool)flightAssistEnabled.Invoke(null, new object[] { vehicle });
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
