using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnScreen : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "SpawnScreen" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Display SpawnScreen";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "SpawnScreen on/off";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		foreach (EntityPlayerLocal localPlayer in GameManager.Instance.World.GetLocalPlayers())
		{
			if (!localPlayer)
			{
				continue;
			}
			localPlayer.spawnInTime = Time.time;
			localPlayer.bPlayingSpawnIn = true;
			if (_params.Count > 0)
			{
				int result = 0;
				if (int.TryParse(_params[0], out result))
				{
					EntityPlayerLocal.spawnInEffectSpeed = result;
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("SpawnEffect initiated...");
		}
	}
}
