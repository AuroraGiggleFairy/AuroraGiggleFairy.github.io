using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class Combat : LayeredSection<CombatLayerMixer>
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
}
