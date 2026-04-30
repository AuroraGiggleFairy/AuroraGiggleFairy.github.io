using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Adventure : LayeredSection<FixedLayerMixer>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator PlayCoroutine()
	{
		yield return base.PlayCoroutine();
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => Mixer.IsFinished || ((bool)src && !src.isPlaying && !IsPaused));
		Log.Out($"Mixer IsFinished: {Mixer.IsFinished}\n AudioSource is not playing: {(bool)src && !src.isPlaying}\n IsPaused: {IsPaused}\n IsPlaying: {IsPlaying}");
		if ((bool)src)
		{
			src.loop = false;
			if (IsPlaying)
			{
				Stop();
			}
		}
		IsDone = true;
		Reset();
		Mixer.Unload();
		coroutines.Remove(MusicActionType.Play);
	}
}
