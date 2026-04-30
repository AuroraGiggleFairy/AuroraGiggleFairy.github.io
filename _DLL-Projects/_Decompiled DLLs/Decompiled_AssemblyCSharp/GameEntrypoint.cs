using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Platform;
using UnityEngine;

public class GameEntrypoint : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_entrypointEntered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_entrypointFinished;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroStopwatch s_profileTotal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroStopwatch s_profileSection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string s_profileIdentifier;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool EntrypointSuccess
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (!s_entrypointEntered)
		{
			Log.Error("[GameEntrypoint] Blocking initialization in Awake!");
		}
		ThreadManager.RunCoroutineSync(EntrypointCoroutine());
	}

	public static bool FirstFrameInit()
	{
		BacktraceUtils.InitializeBacktrace();
		Cursor.visible = false;
		ThreadManager.SetMainThreadRef(Thread.CurrentThread);
		PlatformOptimizations.Init();
		if (HasPrefCollisions())
		{
			return false;
		}
		GamePrefs.InitPropertyDeclarations();
		if (!GameStartupHelper.Instance.InitCommandLine())
		{
			return false;
		}
		if (!PlatformApplicationManager.Init())
		{
			return false;
		}
		if (!PlatformManager.Init())
		{
			return false;
		}
		Application.targetFrameRate = (int)PlatformApplicationManager.Application.GetCurrentRefreshRate().value;
		return true;
	}

	public static IEnumerator EntrypointCoroutine()
	{
		if (s_entrypointEntered)
		{
			while (!s_entrypointFinished)
			{
				yield return null;
			}
			yield break;
		}
		s_entrypointEntered = true;
		try
		{
			if (FirstFrameInit())
			{
				yield return EntrypointCoroutineInternal();
			}
		}
		finally
		{
			s_entrypointFinished = true;
			if (!EntrypointSuccess)
			{
				Log.Error("[GameEntrypoint] Failed initializing core systems, shutting down");
				Application.Quit();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator EntrypointCoroutineInternal()
	{
		yield return SaveDataUtils.InitStaticCoroutine();
		yield return null;
		GamePrefs.InitPrefs();
		if (GameStartupHelper.Instance.InitGamePrefs())
		{
			yield return null;
			try
			{
				Localization.Init();
			}
			catch (Exception ex)
			{
				Log.Error($"[GameEntrypoint] Failed initializing localization: {ex.GetType()}");
				Log.Exception(ex);
				yield break;
			}
			EntrypointSuccess = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool HasPrefCollisions()
	{
		foreach (string item in EnumUtils.Names<EnumGamePrefs>())
		{
			if (LaunchPrefs.All.TryGetValue(item, out var value))
			{
				Log.Error("Name collision between LaunchPref '" + value.Name + "' and GamePref '" + item + "'.");
				return true;
			}
		}
		return false;
	}

	[Conditional("NEVER_DEFINED")]
	public static void ProfileSection(string identifier)
	{
		if (s_profileTotal == null)
		{
			s_profileTotal = new MicroStopwatch(_bStart: true);
		}
		if (s_profileSection == null)
		{
			s_profileSection = new MicroStopwatch(_bStart: true);
		}
		s_profileIdentifier = identifier;
	}

	[Conditional("PROFILE_GAME_ENTRYPOINT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProfileSectionEnd()
	{
		if (s_profileIdentifier != null)
		{
			Log.Out($"[GameEntrypoint: Profile] Section {s_profileIdentifier} {s_profileSection.Elapsed.TotalMilliseconds:F3} ms");
			s_profileSection.Restart();
			s_profileIdentifier = null;
		}
	}

	[Conditional("PROFILE_GAME_ENTRYPOINT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProfileEnd()
	{
		if (s_profileTotal != null)
		{
			Log.Out($"[GameEntrypoint: Profile] TOTAL {s_profileTotal.Elapsed.TotalMilliseconds:F3} ms");
			s_profileTotal = null;
		}
	}
}
