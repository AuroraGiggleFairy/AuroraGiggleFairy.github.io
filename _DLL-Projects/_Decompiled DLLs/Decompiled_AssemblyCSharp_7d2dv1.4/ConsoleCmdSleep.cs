using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSleep : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "sleep" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Makes the main thread sleep for the given number of seconds (allows decimals)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		float _result = 1f;
		if (_params.Count >= 1 && !StringParsers.TryParseFloat(_params[0], out _result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument is not a valid float");
			return;
		}
		Thread.Sleep((int)(_result * 1000f));
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Slept for {_result} seconds");
	}
}
