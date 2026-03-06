using System.Collections;
using UnityEngine;

namespace Audio;

public class PlayAndCleanup
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject go;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource src;

	[PublicizedFrom(EAccessModifier.Private)]
	public LoopingPair lp;

	public PlayAndCleanup(LoopingPair _lp)
	{
		lp = _lp;
		double num;
		lp.sgoBegin.src.PlayScheduled(num = AudioSettings.dspTime + 0.05);
		lp.sgoLoop.src.PlayScheduled(num + (double)lp.sgoBegin.src.clip.samples / 44100.0);
		Manager.AddPlayingAudioSource(lp.sgoBegin.src);
		Manager.AddPlayingAudioSource(lp.sgoLoop.src);
		GameManager.Instance.StartCoroutine(StopBeginWhenDone(lp.sgoBegin.src.clip.length));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopBeginWhenDone(float waitTime)
	{
		yield return new WaitForSeconds(waitTime + 0.1f);
		if (GameManager.Instance.IsPaused())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (lp.sgoBegin.src == null)
		{
			yield break;
		}
		if (lp.sgoBegin.src.isPlaying)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!(lp.sgoBegin.src == null))
		{
			Manager.RemovePlayingAudioSource(lp.sgoBegin.src);
			if ((bool)lp.sgoBegin.go)
			{
				Object.Destroy(lp.sgoBegin.go);
			}
		}
	}

	public PlayAndCleanup(GameObject _go, AudioSource _source, float _occlusion = 0f, float delay = 0f, bool isLooping = false, bool hasLoopingAnalog = false)
	{
		go = _go;
		src = _source;
		float num = 1f - _occlusion;
		float num2 = Utils.FastAbs(Manager.currentListenerPosition.y - go.transform.position.y);
		num2 = Utils.FastClamp01(num2 / 30f);
		src.volume *= (1f - num2) * num;
		if (num < 0.95f)
		{
			go.AddComponent<AudioLowPassFilter>().cutoffFrequency = Utils.FastLerp(10f, 5000f, Mathf.Pow(num, 2f));
		}
		if (delay > 0f)
		{
			src.PlayDelayed(delay);
		}
		else
		{
			Manager.PlaySource(src);
		}
		Manager.AddPlayingAudioSource(src);
		if (!isLooping)
		{
			float waitTime = _source.clip.length * (1f + Utils.FastClamp01(1f - _source.pitch)) + delay;
			GameManager.Instance.StartCoroutine(StopWhenDone(waitTime));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopWhenDone(float waitTime)
	{
		yield return new WaitForSeconds(waitTime + 0.001f);
		if (GameManager.Instance.IsPaused())
		{
			yield return new WaitForSeconds(0.1f);
		}
		Manager.RemovePlayingAudioSource(src);
		if ((bool)go)
		{
			Object.Destroy(go);
		}
	}
}
