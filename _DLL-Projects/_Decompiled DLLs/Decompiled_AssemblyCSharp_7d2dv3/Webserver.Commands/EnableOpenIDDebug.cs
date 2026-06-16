using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Webserver.Commands;

[Preserve]
public class EnableOpenIDDebug : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enable/disable OpenID debugging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "openiddebug" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Current state: {OpenID.debugOpenId}");
			return;
		}
		OpenID.debugOpenId = _params[0].Equals("1");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Set OpenID debugging to {0}", _params[0].Equals("1")));
	}
}
