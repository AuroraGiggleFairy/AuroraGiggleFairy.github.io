using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMumblePositionalAudio : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "mumblepositionalaudio", "mpa" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Mumble Positional Audio related tools";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\r\n\t\t\t|Usage:\r\n\t\t\t|  1. mpa enable\r\n\t\t\t|  2. mpa disable\r\n\t\t\t|  3. mpa reinit\r\n\t\t\t".Unindent();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, found " + _params.Count + ".");
		}
		else if (_params[0].EqualsCaseInsensitive("enable"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance != null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio already enabled");
			}
			else
			{
				MumblePositionalAudio.Init();
			}
		}
		else if (_params[0].EqualsCaseInsensitive("disable"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio already disabled");
			}
			else
			{
				MumblePositionalAudio.Destroy();
			}
		}
		else if (_params[0].EqualsCaseInsensitive("reinit"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio not enabled");
			}
			else
			{
				SingletonMonoBehaviour<MumblePositionalAudio>.Instance.ReinitShm();
			}
		}
		else if (_params[0].EqualsCaseInsensitive("uiversion"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio not enabled");
			}
			else
			{
				SingletonMonoBehaviour<MumblePositionalAudio>.Instance.printUiVersion();
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid subcommand");
		}
	}
}
