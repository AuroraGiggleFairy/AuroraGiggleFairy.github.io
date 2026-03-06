using Audio;
using DynamicMusic;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionStopSound : MinEventActionSoundBase
{
	public override void Execute(MinEventParams _params)
	{
		string soundGroupForTarget = GetSoundGroupForTarget();
		if (localPlayerOnly && targets[0] as EntityPlayerLocal != null)
		{
			if (loop)
			{
				Manager.StopLoopInsidePlayerHead(soundGroupForTarget, targets[0].entityId);
				if (toggleDMS)
				{
					SectionSelector.IsDMSTempDisabled = false;
				}
			}
			else
			{
				Manager.Stop(targets[0].entityId, soundGroupForTarget);
			}
		}
		else if (!localPlayerOnly && targets[0] != null)
		{
			Manager.BroadcastStop(targets[0].entityId, soundGroupForTarget);
		}
	}
}
