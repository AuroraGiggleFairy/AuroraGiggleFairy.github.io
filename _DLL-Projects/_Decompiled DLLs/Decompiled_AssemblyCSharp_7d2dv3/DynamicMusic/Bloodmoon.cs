using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Bloodmoon : LayeredSection<BloodmoonLayerMixer>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator PlayCoroutine()
	{
		yield return base.PlayCoroutine();
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => !src.isPlaying && !IsPaused);
		Reset();
		Mixer.Unload();
		coroutines.Remove(MusicActionType.Play);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FillStream(float[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = Mixer[cursor++];
			cursor %= Content.SamplesFor[base.Sect] * 2;
		}
	}
}
