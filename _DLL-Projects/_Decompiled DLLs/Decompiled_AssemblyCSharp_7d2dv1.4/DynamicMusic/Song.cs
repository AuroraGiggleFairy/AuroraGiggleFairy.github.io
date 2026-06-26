using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Song : SingleClipPlayer, ISection, IPlayable, IFadeable, ICleanable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator PlayCoroutine()
	{
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => IsReady);
		src?.Play();
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => !src.isPlaying && !IsPaused);
		if (IsPlaying)
		{
			Stop();
		}
		IsDone = true;
		coroutines.Remove(MusicActionType.Play);
	}
}
