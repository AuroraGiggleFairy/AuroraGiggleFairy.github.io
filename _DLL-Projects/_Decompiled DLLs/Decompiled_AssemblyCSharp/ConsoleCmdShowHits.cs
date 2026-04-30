using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowHits : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "showhits" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Show hit entity info";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\ndamage - toggle damage numbers\nhits <time> <size> - toggle hits";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (!(text == "damage"))
		{
			if (text == "hits")
			{
				EntityAlive.ShowDebugDisplayHit = !EntityAlive.ShowDebugDisplayHit;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Hits " + (EntityAlive.ShowDebugDisplayHit ? "on" : "off"));
				if (_params.Count >= 2)
				{
					EntityAlive.DebugDisplayHitTime = StringParsers.ParseFloat(_params[1]);
				}
				if (_params.Count >= 3)
				{
					EntityAlive.DebugDisplayHitSize = StringParsers.ParseFloat(_params[2]);
				}
				ItemAction.ShowDebugDisplayHit = EntityAlive.ShowDebugDisplayHit;
				ItemAction.DebugDisplayHitTime = EntityAlive.DebugDisplayHitTime;
				ItemAction.DebugDisplayHitSize = EntityAlive.DebugDisplayHitSize;
			}
		}
		else
		{
			DamageText.Enabled = !DamageText.Enabled;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Damage " + (DamageText.Enabled ? "on" : "off"));
		}
	}
}
