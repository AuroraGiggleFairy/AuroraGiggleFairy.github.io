using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Webserver.FileCache;

[Preserve]
public class InvalidateCachesCmd : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "invalidatecaches" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Invalidate contents of web file caches";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "TODO";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		var (num, num2) = AbstractCache.InvalidateAllCaches();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Caches invalidated, dropped {num} files with {num2} Bytes");
	}
}
