using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdConfig : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "config" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Import/export config data from/to external file";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Imports/exports config data from/to external file\nUsage:\n   config import [filename]\n   config export [filename]\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		try
		{
			string text = _params[1].ToLower();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			string text2 = _params[0];
			if (!(text2 == "import"))
			{
				if (text2 == "export")
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.WriteAllText(text, GameOptionsManager.ExportControls());
				}
			}
			else if (File.Exists(text))
			{
				GameOptionsManager.ImportControls(File.ReadAllText(text));
			}
		}
		catch
		{
		}
	}
}
