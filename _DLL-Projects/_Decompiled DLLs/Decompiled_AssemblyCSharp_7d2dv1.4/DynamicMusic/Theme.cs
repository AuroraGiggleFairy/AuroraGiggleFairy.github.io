using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Theme : SingleClipPlayer, ISection, IPlayable, IFadeable, ICleanable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator InitializationCoroutine()
	{
		yield return base.InitializationCoroutine();
		if (src != null)
		{
			src.loop = true;
		}
		IsReady = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator PlayCoroutine()
	{
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => IsReady);
		src?.Play();
		coroutines.Remove(MusicActionType.Play);
	}
}
