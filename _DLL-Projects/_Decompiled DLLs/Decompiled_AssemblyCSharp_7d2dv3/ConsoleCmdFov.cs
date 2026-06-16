using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdFov : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "fov" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Camera field of view";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1 && int.TryParse(_params[0], out var result))
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, result);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set FOV to " + result);
		}
	}
}
