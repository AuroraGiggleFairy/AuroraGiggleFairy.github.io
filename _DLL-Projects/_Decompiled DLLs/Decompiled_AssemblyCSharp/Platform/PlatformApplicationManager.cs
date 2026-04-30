using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform;

public static class PlatformApplicationManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isRestartRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isRestarting;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EPlatformLoadSaveGameState loadSaveGameState;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static IPlatformApplication Application
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool IsRestartRequired => isRestartRequired;

	public static bool Init()
	{
		Application = IPlatformApplication.Create();
		return true;
	}

	public static void SetRestartRequired()
	{
		isRestartRequired = PlatformOptimizations.RestartProcessSupported;
		Log.Out($"[PlatformApplication] restart required = {isRestartRequired}");
	}

	public static bool CheckRestartCoroutineReady()
	{
		if (isRestartRequired && !isRestarting)
		{
			return !InviteManager.Instance.IsConnectingToInvite();
		}
		return false;
	}

	public static IEnumerator CheckRestartCoroutine(bool loadSaveGame = false)
	{
		if (CheckRestartCoroutineReady())
		{
			isRestartRequired = false;
			isRestarting = true;
			try
			{
				yield return GameManager.Instance.ShowExitingGameUICoroutine();
				RestartProcess(loadSaveGame);
			}
			finally
			{
				Log.Error("[PlatformApplication] failed to restart process.");
				isRestarting = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] RemoveFirstRunArguments(string[] argv)
	{
		return argv.Where([PublicizedFrom(EAccessModifier.Internal)] (string arg) => !arg.StartsWith("-LoadSaveGame=", StringComparison.OrdinalIgnoreCase)).ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RestartProcess(bool loadSaveGame)
	{
		List<string> list = new List<string>();
		list.AddRange(RemoveFirstRunArguments(GameStartupHelper.RemoveTemporaryArguments(GameStartupHelper.GetCommandLineArgs())));
		list.Add("[REMOVE_ON_RESTART]");
		list.Add("-skipintro");
		list.Add(LaunchPrefs.SkipNewsScreen.ToCommandLine(value: true));
		if (PlatformOptimizations.RestartAfterRwg && loadSaveGame)
		{
			Log.Out("[LoadSaveGame] After restart should load: worldName=" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + " saveName=" + GamePrefs.GetString(EnumGamePrefs.GameName));
			list.Add(LaunchPrefs.LoadSaveGame.ToCommandLine(value: true));
		}
		list.AddRange(InviteManager.Instance.GetCommandLineArguments());
		list.AddRange(PlatformManager.NativePlatform.GetArgumentsForRelaunch());
		try
		{
			GamePrefs.Instance.Save();
			SaveDataUtils.Destroy();
			PlatformManager.Destroy();
		}
		catch (Exception e)
		{
			Log.Error("Exception thrown while preparing for process restart. This may cause errors in the next run");
			Log.Exception(e);
		}
		Application.RestartProcess(list.ToArray());
	}

	public static EPlatformLoadSaveGameState GetLoadSaveGameState()
	{
		if (!LaunchPrefs.LoadSaveGame.Value)
		{
			return EPlatformLoadSaveGameState.Done;
		}
		if (loadSaveGameState == EPlatformLoadSaveGameState.Init)
		{
			string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			if (!GameIO.DoesWorldExist(worldName))
			{
				Log.Warning("[LoadSaveGame] World does not exist: " + worldName);
				return loadSaveGameState = EPlatformLoadSaveGameState.Done;
			}
			string gameName = GamePrefs.GetString(EnumGamePrefs.GameName);
			bool found = false;
			bool isArchived = false;
			GameIO.GetPlayerSaves([PublicizedFrom(EAccessModifier.Internal)] (string foundSaveName, string foundWorldName, DateTime _, WorldState _, bool foundIsArchived) =>
			{
				if (foundSaveName.EqualsCaseInsensitive(gameName) && foundWorldName.EqualsCaseInsensitive(worldName))
				{
					found = true;
					isArchived = foundIsArchived;
				}
			}, includeArchived: true);
			if (!found)
			{
				Log.Out("[LoadSaveGame] Creating new save game '" + gameName + "' from the world '" + worldName + "'.");
				return loadSaveGameState = EPlatformLoadSaveGameState.NewGameOpen;
			}
			if (isArchived)
			{
				Log.Warning("[LoadSaveGame] Can not load archived save '" + gameName + "' (world '" + worldName + "').");
				return loadSaveGameState = EPlatformLoadSaveGameState.Done;
			}
			Log.Out("[LoadSaveGame] Loading existing save game '" + gameName + "' (world '" + worldName + "').");
			return loadSaveGameState = EPlatformLoadSaveGameState.ContinueGameOpen;
		}
		return loadSaveGameState;
	}

	public static void AdvanceLoadSaveGameStateFrom(EPlatformLoadSaveGameState previousState)
	{
		if (loadSaveGameState != previousState)
		{
			Log.Error($"[LoadSaveGame] Expected advance from {loadSaveGameState} but was {previousState}");
			loadSaveGameState = EPlatformLoadSaveGameState.Done;
		}
		loadSaveGameState = previousState switch
		{
			EPlatformLoadSaveGameState.Init => throw new NotSupportedException("Init state should be manually advanced from because it branches."), 
			EPlatformLoadSaveGameState.NewGameOpen => EPlatformLoadSaveGameState.NewGameSelect, 
			EPlatformLoadSaveGameState.NewGameSelect => EPlatformLoadSaveGameState.NewGamePlay, 
			EPlatformLoadSaveGameState.NewGamePlay => EPlatformLoadSaveGameState.Done, 
			EPlatformLoadSaveGameState.ContinueGameOpen => EPlatformLoadSaveGameState.ContinueGameSelect, 
			EPlatformLoadSaveGameState.ContinueGameSelect => EPlatformLoadSaveGameState.ContinueGamePlay, 
			EPlatformLoadSaveGameState.ContinueGamePlay => EPlatformLoadSaveGameState.Done, 
			EPlatformLoadSaveGameState.Done => throw new NotSupportedException("Can't advance from the final state."), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		Log.Out($"[LoadSaveGame] Advanced to state {loadSaveGameState} (was {previousState})");
	}

	public static void SetFailedLoadSaveGame()
	{
		if (loadSaveGameState != EPlatformLoadSaveGameState.Done)
		{
			Log.Warning($"[LoadSaveGame] Failed to automate creating or loading the save game. State: {loadSaveGameState}");
			loadSaveGameState = EPlatformLoadSaveGameState.Done;
		}
	}
}
