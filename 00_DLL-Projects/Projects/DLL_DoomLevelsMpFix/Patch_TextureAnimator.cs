using System.Collections;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class TextureAnimFix
	{
		private static readonly FieldInfo Sequences = AccessTools.Field(typeof(TextureAnimator), "Sequences");
		private static bool _ready;
		private static float _nextTry;

		internal static void Reset()
		{
			_ready = false;
			_nextTry = 0f;
		}

		internal static void Tick()
		{
			if (_ready || GameManager.IsDedicatedServer)
			{
				return;
			}

			if (Time.time < _nextTry)
			{
				return;
			}

			_nextTry = Time.time + 1f;
			TextureAnimator.Start();
			ICollection list = Sequences?.GetValue(null) as ICollection;
			_ready = list != null && list.Count > 0;
		}
	}
}
