using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCover : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "tcs", "testCoverSystem" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "CoverSystem queries.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.World.GetPrimaryPlayer();
		if (_params.Count == 0)
		{
			EntityCoverManager.DebugModeEnabled = !EntityCoverManager.DebugModeEnabled;
			Log.Warning("coverSystem" + $" - enabled:{EntityCoverManager.DebugModeEnabled}");
		}
		else if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out("coverSystem help:" + Environment.NewLine);
		}
	}
}
