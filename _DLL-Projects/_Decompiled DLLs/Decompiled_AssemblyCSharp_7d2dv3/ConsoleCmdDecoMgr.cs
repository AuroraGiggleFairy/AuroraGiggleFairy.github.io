using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDecoMgr : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "decomgr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "\"decomgr\": Saves a debug texture visualising the DecoOccupiedMap.\n\"decomgr state\": Saves a debug texture visualising the location/state of all of the DecoObjects saved in decorations.7dtd.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0 && _params[0] == "state")
		{
			DecoManager.Instance.SaveStateDebugTexture(Path.Join(GameIO.GetApplicationTempPath(), "decostate.png"));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved decostate.png to temp directory.");
		}
		else
		{
			DecoManager.Instance.SaveDebugTexture(Path.Join(GameIO.GetApplicationTempPath(), "deco.png"));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved deco.png to temp directory.");
		}
	}
}
