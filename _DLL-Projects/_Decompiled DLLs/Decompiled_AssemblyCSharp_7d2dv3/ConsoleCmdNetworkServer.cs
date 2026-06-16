using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNetworkServer : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "networkserver", "nets" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server side network commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\nlatencysim <min> <max> - sets simulation in millisecs (0 min disables)\npacketlosssim <chance> - sets simulation in percent (0 - 50)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		switch (text)
		{
		case "ls":
		case "latencysim":
		{
			int result2 = 0;
			int result3 = 100;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result2);
			}
			if (_params.Count >= 3)
			{
				int.TryParse(_params[2], out result3);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SetLatencySimulation(result2 > 0, result2, result3);
			break;
		}
		case "pls":
		case "packetlosssim":
		{
			int result = 0;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result);
			}
			if (result > 50)
			{
				result = 50;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SetPacketLossSimulation(result > 0, result);
			break;
		}
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text + ".");
			break;
		}
	}
}
