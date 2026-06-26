using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorSpawnHorde : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "spawnwandering", "spawnw" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawn wandering entities";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\nb - bandits\nh - horde";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (!(text == "b"))
		{
			if (text == "h")
			{
				GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().StartSpawning(AIWanderingHordeSpawner.SpawnType.Horde);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
			}
		}
		else
		{
			GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().StartSpawning(AIWanderingHordeSpawner.SpawnType.Bandits);
		}
	}
}
