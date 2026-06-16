using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdExportCurrentConfigs : ConsoleCmdAbstract
{
	public override int DefaultPermissionLevel => 1000;

	public override bool AllowedInMainMenu => true;

	public override bool IsExecuteOnClient => true;

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
		if (GameManager.Instance.World == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No game started, exporting XUi menu and rwgmixer configs");
			string text = Path.Combine(GameIO.GetApplicationTempPath(), "ExportedConfigs");
			if (SdDirectory.Exists(text))
			{
				SdDirectory.Delete(text, recursive: true);
			}
			Thread.Sleep(50);
			SdDirectory.CreateDirectory(text);
			string[] array = new string[6] { "rwgmixer", "loadingscreen", "XUi_Menu/styles", "XUi_Menu/templates", "XUi_Menu/windows", "XUi_Menu/xui" };
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
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Configs exports to " + text);
			GameIO.OpenExplorer(text);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string text3 = Path.Combine(GameIO.GetSaveGameDir(), "ConfigsDump");
			if (!GameIO.IsRoamingUserDataPath(text3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Patched XMLs are automatically dumped on game start to a ConfigsDump subdirectory of the save game.");
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("In this case you can find the folder at: " + text3);
				GameIO.OpenExplorer(text3);
			}
			else
			{
				string path2 = GamePrefs.GetString(EnumGamePrefs.GameWorld);
				string path3 = GamePrefs.GetString(EnumGamePrefs.GameName);
				DumpLoadedXmls(Path.Combine(GameIO.GetApplicationTempPath(), path2, path3, "ConfigsDump"));
			}
		}
		else
		{
			string path4 = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
			DumpLoadedXmls(Path.Combine(GameIO.GetApplicationTempPath(), "SavesLocal", path4, "ConfigsDump"));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DumpLoadedXmls(string tempPath)
	{
		if (SdDirectory.Exists(tempPath))
		{
			SdDirectory.Delete(tempPath, recursive: true);
		}
		SdDirectory.CreateDirectory(tempPath);
		WorldStaticData.SaveXmlsToFolder(tempPath);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Configs exports to " + tempPath);
		GameIO.OpenExplorer(tempPath);
	}
}
