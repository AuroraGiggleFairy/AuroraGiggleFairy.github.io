using Audio;
using DynamicMusic;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionPlaySound : MinEventActionSoundBase
{
	public override void Execute(MinEventParams _params)
	{
		if (silentOnEquip && ((_params.Self != null) & _params.Self.IsEquipping))
		{
			return;
		}
		string soundGroupForTarget = GetSoundGroupForTarget();
		if (localPlayerOnly && targets[0] as EntityPlayerLocal != null)
		{
			if (loop)
			{
				Manager.PlayInsidePlayerHead(soundGroupForTarget, targets[0].entityId, 0f, isLooping: true);
				if (toggleDMS)
				{
					SectionSelector.IsDMSTempDisabled = true;
				}
			}
			else
			{
				Manager.Play(playAtSelf ? _params.Self : targets[0], soundGroupForTarget);
			}
		}
		else if (!localPlayerOnly && !playAtSelf && targets[0] != null)
		{
			Manager.BroadcastPlay(targets[0], soundGroupForTarget);
		}
		else if (!localPlayerOnly && playAtSelf && _params.Self != null)
		{
			Manager.BroadcastPlay(_params.Self, soundGroupForTarget);
		}
	}
}
