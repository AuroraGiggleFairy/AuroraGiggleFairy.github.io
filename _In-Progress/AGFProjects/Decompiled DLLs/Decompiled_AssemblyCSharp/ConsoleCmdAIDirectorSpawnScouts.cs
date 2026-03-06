using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorSpawnScouts : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "spawnscouts" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 1 && _params.Count != 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected none, 1 or 3, found " + _params.Count + ".");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" ");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		Vector3 targetPos = default(Vector3);
		if (_params.Count == 0)
		{
			if (!_senderInfo.IsLocalGame && _senderInfo.RemoteClientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires a parameter if not executed by a player.");
				return;
			}
			targetPos = ((!_senderInfo.IsLocalGame) ? GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].GetPosition() : GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
		}
		else if (_params.Count == 1)
		{
			ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
			if (clientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity/steamid id not found.");
				return;
			}
			targetPos = GameManager.Instance.World.Players.dict[clientInfo.entityId].GetPosition();
		}
		else if (_params.Count == 3)
		{
			int result = int.MinValue;
			int result2 = int.MinValue;
			int result3 = int.MinValue;
			int.TryParse(_params[0], out result);
			int.TryParse(_params[1], out result2);
			int.TryParse(_params[2], out result3);
			if (result == int.MinValue || result2 == int.MinValue || result3 == int.MinValue)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x:" + result);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y:" + result2);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z:" + result3);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("At least one of the given coordinates is not a valid integer");
				return;
			}
			targetPos = new Vector3(result, result2, result3);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Scouts spawning at " + targetPos.x.ToCultureInvariantString() + ", " + targetPos.y.ToCultureInvariantString() + ", " + targetPos.z.ToCultureInvariantString());
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>().SpawnScouts(targetPos);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns zombie scouts";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Spawn scouts near a player.Usage:\n   1. spawnscouts\n   2. spawnscouts <player name/steam id/entity id>\n   3. spawnscouts <x> <y> <z>\n1. Will spawn the scouts near the issuing player. Can only be used by a player, not a remote console.\n2. Spawn scouts near the given player.\n3. Spawn scouts at the given coordinates.";
	}
}
