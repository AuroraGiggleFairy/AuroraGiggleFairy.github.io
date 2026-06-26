using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowSwings : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "showswings" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		EntityAlive.ShowDebugDisplayHit = !EntityAlive.ShowDebugDisplayHit;
		ItemActionDynamic.ShowDebugSwing = EntityAlive.ShowDebugDisplayHit;
		if (EntityAlive.ShowDebugDisplayHit)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Show Swings (ON)");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Show Swings (OFF)");
		for (int i = 0; i < ItemActionDynamic.DebugDisplayHits.Count; i++)
		{
			Object.DestroyImmediate(ItemActionDynamic.DebugDisplayHits[i]);
		}
		ItemActionDynamic.DebugDisplayHits.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Show melee swing arc rays";
	}
}
