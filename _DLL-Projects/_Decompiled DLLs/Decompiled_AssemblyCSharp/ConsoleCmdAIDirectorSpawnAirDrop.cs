using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorSpawnAirDrop : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "spawnairdrop" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().SpawnAirDrop();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns an air drop";
	}
}
