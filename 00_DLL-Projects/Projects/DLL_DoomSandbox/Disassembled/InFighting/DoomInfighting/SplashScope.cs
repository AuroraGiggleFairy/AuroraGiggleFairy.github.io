using System;
using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(Explosion), "AttackEntites")]
public static class SplashScope
{
	public static bool Active;

	public static void Prefix()
	{
		Active = true;
	}

	public static Exception Finalizer(Exception __exception)
	{
		Active = false;
		return __exception;
	}
}
