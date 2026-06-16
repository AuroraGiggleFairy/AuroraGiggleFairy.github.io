using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdStab : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "stab" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "stability";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Running stability");
		GameManager.Instance.CreateStabilityViewer();
		if (_params.Count == 0)
		{
			GameManager.Instance.stabilityViewer.StartSearch();
		}
		else if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("Clear"))
			{
				GameManager.Instance.ClearStabilityViewer();
				return;
			}
			if (_params[0].EqualsCaseInsensitive("Redo"))
			{
				GameManager.Instance.stabilityViewer.StartSearch();
				return;
			}
			int result = 31;
			int.TryParse(_params[0], out result);
			GameManager.Instance.stabilityViewer.StartSearch(result);
		}
	}
}
