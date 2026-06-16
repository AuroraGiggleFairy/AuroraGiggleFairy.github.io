using System.Collections.Generic;
using UnityEngine.Scripting;

namespace MapRendering.Commands;

[Preserve]
public class RenderMap : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "render the current map to a file";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "rendermap" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!MapRenderer.HasInstance)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Renderer not enabled");
			return;
		}
		MapRenderer.Instance.RenderFullMap();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Render map done");
	}
}
