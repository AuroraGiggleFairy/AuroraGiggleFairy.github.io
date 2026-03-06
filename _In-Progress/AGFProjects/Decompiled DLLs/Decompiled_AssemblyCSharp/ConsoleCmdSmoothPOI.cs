using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSmoothPOI : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "smoothpoi" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used in the prefab editor");
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on the host");
			return;
		}
		string text = "land";
		if (_params.Count >= 1)
		{
			text = _params[0];
		}
		int passes = 1;
		if (_params.Count == 2)
		{
			passes = int.Parse(_params[1]);
		}
		bool land = text.Equals("land");
		DateTime now = DateTime.Now;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Starting POI smoothing pass");
		PrefabHelpers.SmoothPOI(passes, land);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Finished POI smoothing at " + DateTime.Now.ToCultureInvariantString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Smoothing action took {(DateTime.Now - now).TotalMilliseconds} ms");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Smoothens the POI";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\n\t\t\t|Usage:\n\t\t\t|  smoothpoi [mode] [passes]\n\t\t\t|Mode defaults to \"land\", also accepts \"air\".\n            |Passes defaults to 1.\n            |Smoothed area can be restricted with the blue selection.\n\t\t\t".Unindent();
	}
}
