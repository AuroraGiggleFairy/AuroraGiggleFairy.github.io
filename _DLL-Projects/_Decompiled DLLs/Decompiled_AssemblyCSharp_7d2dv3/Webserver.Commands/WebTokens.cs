using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Scripting;
using Webserver.Permissions;

namespace Webserver.Commands;

[Preserve]
public class WebTokens : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex validNameTokenMatcher = new Regex("^\\w+$");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "webtokens" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Manage web tokens";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set/get webtoken permission levels. A level of 0 is maximum permission.\nUsage:\n   webtokens add <tokenname> <tokensecret> <level>\n   webtokens remove <tokenname>\n   webtokens list";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (_params[0].EqualsCaseInsensitive("add"))
			{
				ExecuteAdd(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("remove"))
			{
				ExecuteRemove(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList();
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteAdd(List<string> _params)
	{
		if (_params.Count != 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrong number of arguments, expected 4, found {_params.Count}.");
			return;
		}
		if (string.IsNullOrEmpty(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokenname' is empty.");
			return;
		}
		if (!validNameTokenMatcher.IsMatch(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokenname' may only contain characters (A-Z, a-z), digits (0-9) and underscores (_).");
			return;
		}
		if (string.IsNullOrEmpty(_params[2]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokensecret' is empty.");
			return;
		}
		if (!validNameTokenMatcher.IsMatch(_params[2]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokensecret' may only contain characters (A-Z, a-z), digits (0-9) and underscores (_).");
			return;
		}
		if (!int.TryParse(_params[3], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'level' is not a valid integer.");
			return;
		}
		AdminApiTokens.Instance.AddToken(_params[1], _params[2], result);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Web API token with name={_params[1]} and secret={_params[2]} added with permission level of {result}.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteRemove(List<string> _params)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrong number of arguments, expected 2, found {_params.Count}.");
			return;
		}
		if (string.IsNullOrEmpty(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokenname' is empty.");
			return;
		}
		if (!validNameTokenMatcher.IsMatch(_params[1]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 'tokenname' may only contain characters (A-Z, a-z), digits (0-9) and underscores (_).");
			return;
		}
		AdminApiTokens.Instance.RemoveToken(_params[1]);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_params[1] + " removed from web API token permissions list.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Defined web API token permissions:");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  Level: Name / Secret");
		foreach (var (_, apiToken2) in AdminApiTokens.Instance.GetTokens())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"  {apiToken2.PermissionLevel,5}: {apiToken2.Name} / {apiToken2.Secret}");
		}
	}
}
