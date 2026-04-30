using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdUIOptions : ConsoleCmdAbstract
{
	public override int DefaultPermissionLevel => 1000;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "uioptions", "uio" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Allows overriding of some options that control the presentation of the UI";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\noptionsvideowindow <value> - set the options window to use for video settings\n[no parameters] - toggles video settings between simplified and detailed modes\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			UIOptions.OptionsVideoWindow = ((UIOptions.OptionsVideoWindow == OptionsVideoWindowMode.Simplified) ? OptionsVideoWindowMode.Detailed : OptionsVideoWindowMode.Simplified);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Set UIOptions.OptionsVideoWindow: {UIOptions.OptionsVideoWindow}");
		}
		else
		{
			if (!(_params[0].ToLowerInvariant() == "optionsvideowindow"))
			{
				return;
			}
			bool flag = false;
			if (_params.Count > 1)
			{
				if (EnumUtils.TryParse<OptionsVideoWindowMode>(_params[1], out var _result, _ignoreCase: true))
				{
					UIOptions.OptionsVideoWindow = _result;
					flag = true;
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown window type " + _params[1]);
				}
			}
			if (!flag)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Valid values: " + string.Join(',', EnumUtils.Values<OptionsVideoWindowMode>()));
			}
		}
	}
}
