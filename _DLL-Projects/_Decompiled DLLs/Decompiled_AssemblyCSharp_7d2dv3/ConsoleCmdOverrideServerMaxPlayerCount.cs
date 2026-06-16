using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdOverrideServerMaxPlayerCount : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "overridemaxplayercount" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Override Max Server Player Count";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1 && int.TryParse(_params[0], out var result))
		{
			GameModeSurvival.OverrideMaxPlayerCount = result;
			Log.Out($"Survival Max Player Count Override set to {result}");
		}
		else
		{
			Log.Out("Incorrect param, expected an integer for max player count.");
		}
	}
}
