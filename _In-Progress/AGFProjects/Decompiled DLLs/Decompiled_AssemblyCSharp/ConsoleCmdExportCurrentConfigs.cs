using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdExportCurrentConfigs : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Exports the current game config XMLs";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Exports all game config XMLs as they are currently used (including applied\npatches from mods) to the folder \"Configs\" in the save folder of the game.\nIf run from the main menu it exports the XUi configs for the menu, if run\nfrom a game session will export all others.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "exportcurrentconfigs" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		string text;
		if (GameManager.Instance.World == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No game started, exporting XUi menu and rwgmixer configs");
			text = GameIO.GetUserGameDataDir() + "/ExportedConfigs";
			if (SdDirectory.Exists(text))
			{
				SdDirectory.Delete(text, recursive: true);
			}
			Thread.Sleep(50);
			SdDirectory.CreateDirectory(text);
			string[] array = new string[6] { "rwgmixer", "loadingscreen", "XUi_Menu/styles", "XUi_Menu/controls", "XUi_Menu/windows", "XUi_Menu/xui" };
			foreach (string text2 in array)
			{
				XmlFile xml = null;
				ThreadManager.RunCoroutineSync(XmlPatcher.LoadAndPatchConfig(text2, [PublicizedFrom(EAccessModifier.Internal)] (XmlFile _file) =>
				{
					xml = _file;
				}));
				string path = text + "/" + text2 + ".xml";
				if (text2.IndexOf('/') >= 0)
				{
					string directoryName = Path.GetDirectoryName(path);
					if (!SdDirectory.Exists(directoryName))
					{
						SdDirectory.CreateDirectory(directoryName);
					}
				}
				xml.SerializeToFile(path);
			}
		}
		else
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				string text3 = GameIO.GetSaveGameDir() + "/ConfigsDump";
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Patched XMLs are automatically dumped on game start to a ConfigsDump subdirectory of the save game.");
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("In this case you can find the folder at: " + text3);
				GameIO.OpenExplorer(text3);
				return;
			}
			text = GameIO.GetSaveGameLocalDir() + "/ConfigsDump";
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Configs exported to " + text);
		GameIO.OpenExplorer(text);
	}
}
