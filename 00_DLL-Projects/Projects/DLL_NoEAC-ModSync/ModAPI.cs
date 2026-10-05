using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ModSync;

public class ModAPI : IModApi
{
	private static bool _started;
	private static int _menuFrames;
	private static string _name = "Server";
	private static string _ip = "";
	private static int _port;

	public void InitMod(Mod modInstance)
	{
		string modPath = modInstance != null ? modInstance.Path : null;
		if (!string.IsNullOrEmpty(modPath))
		{
			ModSyncCommon.OwnModFolder = Path.GetFileName(modPath.TrimEnd('\\', '/'));
		}

		try
		{
			new Harmony("AGF.ModSync").PatchAll(Assembly.GetExecutingAssembly());
		}
		catch (Exception ex)
		{
			ModSyncCommon.Error("Could not apply patches: " + ex);
		}

		ModSyncServer.Init(modPath);

		if (!GameManager.IsDedicatedServer)
		{
			ReadJoinArguments();
			if (_ip.Length > 0 && _port > 0)
			{
				ModEvents.UnityUpdate.RegisterHandler(OnUpdate);
				ModSyncCommon.Info("Loaded. Will join " + _name + " at " + _ip + ":" + _port);
				return;
			}
		}

		ModSyncCommon.Info("Loaded.");
	}

	/// <summary>
	/// The address comes from the desktop shortcut only. A file inside Mods is loaded
	/// by every 7 Days to Die install that has this mod, so it cannot choose the server.
	/// </summary>
	private static void ReadJoinArguments()
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i];
			if (arg.Equals("-modsyncjoin", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				ApplyJoinTarget(args[++i]);
			}
			else if (arg.StartsWith("-modsyncjoin=", StringComparison.OrdinalIgnoreCase))
			{
				ApplyJoinTarget(arg.Substring("-modsyncjoin=".Length));
			}
			else if (arg.Equals("-modsyncname", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
			{
				ApplyJoinName(args[++i]);
			}
			else if (arg.StartsWith("-modsyncname=", StringComparison.OrdinalIgnoreCase))
			{
				ApplyJoinName(arg.Substring("-modsyncname=".Length));
			}
		}
	}

	private static void ApplyJoinName(string value)
	{
		value = value.Trim().Trim('"');
		if (value.Length > 0)
		{
			_name = value;
		}
	}

	private static void ApplyJoinTarget(string value)
	{
		value = value.Trim().Trim('"');
		int colon = value.LastIndexOf(':');
		if (colon <= 0)
		{
			return;
		}

		string ip = value.Substring(0, colon).Trim();
		if (ip.Length == 0 || !int.TryParse(value.Substring(colon + 1), out int port) || port <= 0)
		{
			return;
		}

		_ip = ip;
		_port = port;
	}

	private static void OnUpdate(ref ModEvents.SUnityUpdateData _)
	{
		if (_started)
		{
			return;
		}

		try
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || manager.IsServer || manager.IsConnected)
			{
				return;
			}

			LocalPlayerUI ui = LocalPlayerUI.primaryUI;
			if (ui == null || ui.windowManager == null || !ui.windowManager.IsWindowOpen(XUiC_MainMenu.ID))
			{
				_menuFrames++;
				return;
			}

			_started = true;
			GameServerInfo info = new GameServerInfo();
			info.SetValue(GameInfoString.IP, _ip);
			info.SetValue(GameInfoInt.Port, _port);
			info.SetValue(GameInfoString.GameHost, _name);
			ModSyncCommon.Info("Joining " + _name + " at " + _ip + ":" + _port);
			manager.Connect(info);
		}
		catch (Exception ex)
		{
			_started = true;
			ModSyncCommon.Error("Could not start join: " + ex);
		}
	}
}
