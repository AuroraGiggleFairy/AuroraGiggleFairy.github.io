using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPIRS : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string sGameSuffix = "_perftest";

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject go;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "pirs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "tbd";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "tbd";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 2 && _params[0] == "reset")
		{
			string saveGameDir = GameIO.GetSaveGameDir("Navezgane", _params[1]);
			if (SdFile.Exists(saveGameDir + "/auto.rec"))
			{
				SdFile.Delete(saveGameDir + "/auto.rec");
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Deleted auto.rec from " + saveGameDir);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Savegame had no recordings");
			}
			return;
		}
		if (_params.Count == 1 && _params[0] == "play")
		{
			PlayerInputRecordingSystem.Instance.Reset();
			GameManager.bPlayRecordedSession = true;
			GameManager.bRecordNextSession = false;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start playing");
			return;
		}
		if (_params.Count == 1 && _params[0] == "record")
		{
			PlayerInputRecordingSystem.Instance.Reset(_bClearRecordings: true);
			GameManager.bPlayRecordedSession = false;
			GameManager.bRecordNextSession = true;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start recording");
			return;
		}
		if (_params.Count == 1 && _params[0] == "stop")
		{
			if (GameManager.bPlayRecordedSession)
			{
				GameManager.bPlayRecordedSession = false;
			}
			if (GameManager.bRecordNextSession)
			{
				GameManager.bRecordNextSession = false;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Stop recording");
			}
			return;
		}
		if (_params.Count == 2 && _params[0] == "save")
		{
			PlayerInputRecordingSystem.Instance.Save(_params[1]);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saving to " + _params[1]);
			return;
		}
		if (_params.Count == 2 && _params[0] == "load")
		{
			load(_params[1]);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loading from " + _params[1]);
			return;
		}
		if (GameManager.Instance.gameStateManager != null && GameManager.Instance.gameStateManager.IsGameStarted())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Please start recording from the main menu");
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Recording only possible in SP");
			return;
		}
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Please specify the savegame name that you want to use as start. A copy will be made to record. Only Navezgane is supported for now");
			return;
		}
		string saveGameDir2 = GameIO.GetSaveGameDir("Navezgane", _params[0]);
		if (!SdDirectory.Exists(saveGameDir2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"The specified savegame folder {saveGameDir2} does not exist");
			return;
		}
		string text = saveGameDir2 + "_perftest";
		if (SdDirectory.Exists(text))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Deleting existing game " + text);
			SdDirectory.Delete(text, recursive: true);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Copying savegame " + saveGameDir2 + " to " + text);
		GameIO.CopyDirectory(saveGameDir2, text);
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Navezgane");
		GamePrefs.Set(EnumGamePrefs.GameMode, EnumGameMode.Survival.ToStringCached());
		GamePrefs.Set(EnumGamePrefs.GameName, _params[0] + "_perftest");
		if (SdFile.Exists(saveGameDir2 + "/auto.rec"))
		{
			GameManager.bPlayRecordedSession = true;
			PlayerInputRecordingSystem.Instance.Reset(_bClearRecordings: true);
			PlayerInputRecordingSystem.Instance.Load("auto");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playing...");
		}
		else
		{
			GameManager.bRecordNextSession = true;
			PlayerInputRecordingSystem.Instance.Reset();
			PlayerInputRecordingSystem.Instance.SetAutoSaveTo(saveGameDir2 + "/auto");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Recording...");
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		ThreadManager.StartCoroutine(onCloseConsoleLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator onCloseConsoleLater()
	{
		yield return new WaitForSeconds(2f);
		GameManager.Instance.SetConsoleWindowVisible(_b: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void load(string _filename)
	{
		PlayerInputRecordingSystem.Instance.Load(_filename);
		PlayerInputRecordingSystem.Instance.SetStartPosition(GameManager.Instance.World.GetLocalPlayers()[0]);
	}
}
