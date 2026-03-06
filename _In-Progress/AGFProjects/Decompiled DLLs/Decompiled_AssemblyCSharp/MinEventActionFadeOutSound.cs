using Audio;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionFadeOutSound : MinEventActionSoundBase
{
	public override void Execute(MinEventParams _params)
	{
		if ((localPlayerOnly && targets[0] as EntityPlayerLocal != null) || (!localPlayerOnly && targets[0] != null))
		{
			string soundGroupForTarget = GetSoundGroupForTarget();
			Manager.FadeOut(targets[0].entityId, soundGroupForTarget);
		}
	}
}
