using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(GameStats), nameof(GameStats.Set), new[] { typeof(EnumGameStats), typeof(int) })]
	internal static class Patch_OverworldEnemyCount
	{
		private static void Prefix(EnumGameStats _eProperty, ref int _value)
		{
			if (_eProperty != EnumGameStats.EnemyCount)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Entities?.list == null)
			{
				return;
			}

			int doom = 0;
			var list = world.Entities.list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is EntityAlive alive && !alive.IsDead() && Monster.Tagged(alive))
				{
					doom++;
				}
			}

			_value = Mathf.Max(0, _value - doom);
		}
	}
}
