using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "junkDrone", "jd" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Local player junk drone commands.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return buildHelpLog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildHelpLog()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("JunkDrone help:" + Environment.NewLine, "[Client Commands]", Environment.NewLine), "jd, log - logs out local player owned drones", Environment.NewLine), "debuglog - toggles extended data logging", Environment.NewLine), "debugcam, dcam - toggles debug camera", Environment.NewLine), "unstuck - triggers teleport to player", Environment.NewLine), "friendlyfire, ff - toggles friendly fire", Environment.NewLine);
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0 && _params[0].ContainsCaseInsensitive("debuglog"))
		{
			DroneManager.DebugLogEnabled = !DroneManager.DebugLogEnabled;
			Log.Out("Drone debug log enabled: " + DroneManager.DebugLogEnabled);
		}
		if (_params.Count == 0)
		{
			Log.Out(logLocalPlayerOwnedDrones());
			return;
		}
		if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out(buildHelpLog());
			return;
		}
		if (_params[0].ContainsCaseInsensitive("log"))
		{
			Log.Out(logLocalPlayerOwnedDrones());
		}
		if (_params[0].ContainsCaseInsensitive("debug"))
		{
			EntityDrone.DebugModeEnabled = !EntityDrone.DebugModeEnabled;
			OwnedEntityData[] ownedEntityClass = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass.Length != 0)
			{
				(GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone).SetDebugCameraEnabled(EntityDrone.DebugModeEnabled);
			}
			Log.Out("drone debug mode enabled: " + EntityDrone.DebugModeEnabled);
			return;
		}
		if (_params[0].ContainsCaseInsensitive("debugcam") || _params[0] == "dcam")
		{
			toggleDebugCam();
			return;
		}
		if (_params[0].ContainsCaseInsensitive("debugrecon") || _params[0] == "drc")
		{
			OwnedEntityData[] ownedEntityClass2 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass2.Length != 0)
			{
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass2[0].Id) as EntityDrone;
				entityDrone.DebugToggleDebugCamera();
				Log.Out("JunkDrone debugcam {0}", entityDrone.IsDebugCameraEnabled);
			}
			return;
		}
		if (_params[0].ContainsCaseInsensitive("unstuck"))
		{
			OwnedEntityData[] ownedEntityClass3 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass3.Length != 0)
			{
				EntityDrone entityDrone2 = GameManager.Instance.World.GetEntity(ownedEntityClass3[0].Id) as EntityDrone;
				if (entityDrone2 != null)
				{
					entityDrone2.TeleportUnstuck();
					Log.Out("drone unstuck complete");
				}
				else
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						entityDrone2 = DroneManager.Instance?.LoadDrone(ownedEntityClass3[0].Id, GameManager.Instance.World);
						if ((bool)entityDrone2)
						{
							entityDrone2.TeleportUnstuck();
							return;
						}
					}
					Log.Warning("Client drone unstuck failed. Try server side unstuck using command \"jds unstuck\"");
				}
			}
		}
		if (_params[0].ContainsCaseInsensitive("friendlyfire") || _params[0] == "ff")
		{
			OwnedEntityData[] ownedEntityClass4 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass4.Length != 0)
			{
				EntityDrone entityDrone3 = GameManager.Instance.World.GetEntity(ownedEntityClass4[0].Id) as EntityDrone;
				entityDrone3.DebugToggleFriendlyFire();
				Log.Out("JunkDrone friendlyfire {0}", entityDrone3.IsFrendlyFireEnabled);
			}
		}
		else if (_params[0].ContainsCaseInsensitive("teir"))
		{
			toggleDebugCam();
			toggleEnemiesInRange();
		}
		else if (_params[0].ContainsCaseInsensitive("eir"))
		{
			toggleEnemiesInRange();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleDebugCam()
	{
		OwnedEntityData[] ownedEntityClass = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
		if (ownedEntityClass.Length != 0)
		{
			EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone;
			entityDrone.DebugToggleDebugCamera();
			Log.Out("JunkDrone debugcam {0}", entityDrone.IsDebugCameraEnabled);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleEnemiesInRange()
	{
		OwnedEntityData[] ownedEntityClass = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
		if (ownedEntityClass.Length != 0)
		{
			EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone;
			entityDrone.DebugEnemiesInRange = !entityDrone.DebugEnemiesInRange;
			Log.Out("JunkDrone DebugEnemiesInRange {0}", entityDrone.DebugEnemiesInRange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logLocalPlayerOwnedDrones()
	{
		string text = string.Empty;
		int num = 0;
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			OwnedEntityData[] ownedEntities = primaryPlayer.GetOwnedEntities();
			string text2 = string.Empty;
			foreach (OwnedEntityData ownedEntityData in ownedEntities)
			{
				if (ownedEntityData != null && EntityClass.list[ownedEntityData.ClassId].entityClassName.ContainsCaseInsensitive("entityJunkDrone"))
				{
					text2 = text2 + string.Format("entityId: {0}, classId: {1}, lastKnownPosition: {2}", ownedEntityData.Id, EntityClass.GetEntityClassName(ownedEntityData.ClassId), ownedEntityData.hasLastKnownPosition ? ownedEntityData.LastKnownPosition.ToString() : "none") + Environment.NewLine;
					num++;
				}
			}
			text += string.Format("[{0} - count({1})]" + Environment.NewLine + "{2}", primaryPlayer.EntityName, num, text2);
		}
		return text;
	}
}
