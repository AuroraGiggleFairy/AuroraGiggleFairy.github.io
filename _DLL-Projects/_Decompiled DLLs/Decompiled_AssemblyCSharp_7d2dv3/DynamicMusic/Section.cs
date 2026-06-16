using System.Collections;
using System.Text;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public abstract class Section : ContentPlayer, ISection, IPlayable, IFadeable, ICleanable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameObject parent = new GameObject("Music");

	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource src;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDictionary<MusicActionType, Coroutine> coroutines;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Coroutine LoadRoutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float fadeTime = 3f;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SectionType Sect { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public override float Volume
	{
		get
		{
			if (!src)
			{
				return 0f;
			}
			return src.volume;
		}
		set
		{
			if ((bool)src)
			{
				src.volume = value;
			}
		}
	}

	public override void Init()
	{
		coroutines = new EnumDictionary<MusicActionType, Coroutine>();
	}

	public override void Play()
	{
		if (!coroutines.ContainsKey(MusicActionType.Play))
		{
			base.Play();
			coroutines.Add(MusicActionType.Play, GameManager.Instance.StartCoroutine(PlayCoroutine()));
			Log.Out($"Played {Sect}");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Attempted to play {Sect}, while play was running");
		stringBuilder.AppendLine("Currently running coroutines: ");
		foreach (MusicActionType key in coroutines.Keys)
		{
			stringBuilder.AppendLine(key.ToString());
		}
		Log.Warning(stringBuilder.ToString());
	}

	public override void Pause()
	{
		base.Pause();
		src?.Pause();
		Log.Out($"Paused {Sect}");
	}

	public override void UnPause()
	{
		base.UnPause();
		src?.UnPause();
		Log.Out($"Unpaused {Sect}");
	}

	public override void Stop()
	{
		base.Stop();
		src?.Stop();
		Log.Out($"Stopped {Sect}");
	}

	public virtual void FadeIn()
	{
		if (coroutines.TryGetValue(MusicActionType.FadeOut, out var value))
		{
			GameManager.Instance.StopCoroutine(value);
			coroutines.Remove(MusicActionType.FadeOut);
		}
		if (IsPaused || coroutines.ContainsKey(MusicActionType.Play))
		{
			UnPause();
		}
		else
		{
			Play();
		}
		Log.Out($"Fading in {Sect}");
		if (coroutines.TryGetValue(MusicActionType.FadeIn, out var value2))
		{
			GameManager.Instance.StopCoroutine(value2);
			coroutines.Remove(MusicActionType.FadeIn);
		}
		coroutines.Add(MusicActionType.FadeIn, GameManager.Instance.StartCoroutine(FadeInCoroutine()));
	}

	public virtual void FadeOut()
	{
		if (coroutines.TryGetValue(MusicActionType.FadeIn, out var value))
		{
			GameManager.Instance.StopCoroutine(value);
			coroutines.Remove(MusicActionType.FadeIn);
		}
		Log.Out($"Fading out {Sect}");
		if (coroutines.TryGetValue(MusicActionType.FadeOut, out var value2))
		{
			GameManager.Instance.StopCoroutine(value2);
			coroutines.Remove(MusicActionType.FadeOut);
		}
		coroutines.Add(MusicActionType.FadeOut, GameManager.Instance.StartCoroutine(FadeOutCoroutine()));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator FadeInCoroutine()
	{
		_ = AudioSettings.dspTime;
		double endTime = AudioSettings.dspTime + 3.0;
		double num = endTime - AudioSettings.dspTime;
		double perc = 1.0 - num / 3.0;
		float startVol = Volume;
		while (perc <= 1.0)
		{
			Volume = Mathf.Lerp(startVol, 1f, (float)perc);
			num = endTime - AudioSettings.dspTime;
			perc = 1.0 - num / 3.0;
			yield return null;
		}
		Volume = 1f;
		coroutines.Remove(MusicActionType.FadeIn);
		Log.Out($"fadeInCo complete on {Sect}");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator FadeOutCoroutine()
	{
		_ = AudioSettings.dspTime;
		double endTime = AudioSettings.dspTime + 3.0;
		double num = endTime - AudioSettings.dspTime;
		double perc = 1.0 - num / 3.0;
		float startVol = Volume;
		while (perc <= 1.0)
		{
			Volume = Mathf.Lerp(startVol, 0f, (float)perc);
			num = endTime - AudioSettings.dspTime;
			perc = 1.0 - num / 3.0;
			yield return null;
		}
		Volume = 0f;
		Pause();
		double timerStart = AudioSettings.dspTime;
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => AudioSettings.dspTime - timerStart >= 60.0 || !IsPaused);
		if (IsPaused)
		{
			Stop();
		}
		else
		{
			Log.Out($"{Sect} was resumed. FadeOut coroutine has been exited.");
		}
		coroutines.Remove(MusicActionType.FadeOut);
		Log.Out($"fadeOutCo complete on {Sect}");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract IEnumerator PlayCoroutine();

	public virtual IEnumerator PreloadRoutine()
	{
		yield return null;
	}

	public virtual void CleanUp()
	{
		src?.Stop();
		foreach (Coroutine value in coroutines.Values)
		{
			GameManager.Instance.StopCoroutine(value);
		}
		if (LoadRoutine != null)
		{
			GameManager.Instance.StopCoroutine(LoadRoutine);
			LoadRoutine = null;
		}
		coroutines.Clear();
		coroutines = null;
		if ((bool)src)
		{
			Object.Destroy(src.gameObject);
			src = null;
		}
		if (parent != null)
		{
			parent.transform.DetachChildren();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Section()
	{
	}
}
