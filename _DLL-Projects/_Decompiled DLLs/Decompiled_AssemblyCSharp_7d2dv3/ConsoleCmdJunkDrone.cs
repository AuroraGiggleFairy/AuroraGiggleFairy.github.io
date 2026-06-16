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
		string text = "JunkDrone help:" + Environment.NewLine;
		text = text + "[Client Commands]" + Environment.NewLine;
		text = text + "jd, log - logs out local player owned drones" + Environment.NewLine;
		text = text + "unstuck - triggers teleport to player" + Environment.NewLine;
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			text = text + "debuglog - toggles extended data logging" + Environment.NewLine;
			text = text + "clear - Clears local player drone type owned entities" + Environment.NewLine;
			text = text + "debugcam, dcam - toggles debug camera" + Environment.NewLine;
			text = text + "friendlyfire, ff - toggles friendly fire" + Environment.NewLine;
		}
		return text;
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Log.Out(logLocalPlayerOwnedDrones());
			return;
		}
		string text = _params[0];
		if (text.Equals("debuglog"))
		{
			DroneManager.DebugLogEnabled = !DroneManager.DebugLogEnabled;
			Log.Out("Drone debug log enabled: " + DroneManager.DebugLogEnabled);
		}
		if (text.Equals("help"))
		{
			Log.Out(buildHelpLog());
			return;
		}
		if (text.Equals("log"))
		{
			Log.Out(logLocalPlayerOwnedDrones());
		}
		if (text.Equals("unstuck"))
		{
			EntityPlayer entityPlayer = ((_senderInfo.RemoteClientInfo == null) ? GameManager.Instance.World.GetPrimaryPlayer() : (GameManager.Instance.World.GetEntity(_senderInfo.RemoteClientInfo.entityId) as EntityPlayer));
			if (entityPlayer == null)
			{
				return;
			}
			OwnedEntityData[] ownedEntityClass = entityPlayer.GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass.Length != 0)
			{
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone;
				if (entityDrone != null)
				{
					entityDrone.DebugTeleportUnstuck();
					Log.Out("drone unstuck complete");
				}
				else
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						entityDrone = DroneManager.Instance?.LoadDrone(ownedEntityClass[0].Id, GameManager.Instance.World);
						if ((bool)entityDrone)
						{
							entityDrone.DebugTeleportUnstuck();
							return;
						}
					}
					Log.Warning("Client drone unstuck failed. Try server side unstuck using command \"jds unstuck\"");
				}
			}
		}
		if (text.Equals("clear"))
		{
			EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if ((bool)primaryPlayer)
			{
				clearDroneOwnedEntitiesForPlayer(primaryPlayer);
				Log.Out("JunkDrone data cleared for {0}.", primaryPlayer.EntityName);
			}
		}
		else
		{
			if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				return;
			}
			if (text.Equals("debugcam") || text.Equals("dcam"))
			{
				toggleDebugCam();
			}
			else if (text.Equals("friendlyfire") || text.Equals("ff"))
			{
				OwnedEntityData[] ownedEntityClass2 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
				if (ownedEntityClass2.Length != 0)
				{
					EntityDrone entityDrone2 = GameManager.Instance.World.GetEntity(ownedEntityClass2[0].Id) as EntityDrone;
					entityDrone2.DebugToggleFriendlyFire();
					Log.Out("JunkDrone friendlyfire {0}", entityDrone2.DebugFrendlyFireEnabled);
				}
			}
			else if (text.Equals("debugrecon") || text.Equals("drc"))
			{
				OwnedEntityData[] ownedEntityClass3 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
				if (ownedEntityClass3.Length != 0)
				{
					EntityDrone entityDrone3 = GameManager.Instance.World.GetEntity(ownedEntityClass3[0].Id) as EntityDrone;
					entityDrone3.DebugToggleDebugCamera();
					Log.Out("JunkDrone debugcam {0}", entityDrone3.IsDebugCameraEnabled);
				}
			}
			else if (text.Equals("debug"))
			{
				EntityDrone.DebugModeEnabled = !EntityDrone.DebugModeEnabled;
				OwnedEntityData[] ownedEntityClass4 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
				if (ownedEntityClass4.Length != 0)
				{
					(GameManager.Instance.World.GetEntity(ownedEntityClass4[0].Id) as EntityDrone).SetDebugCameraEnabled(EntityDrone.DebugModeEnabled);
				}
				Log.Out("drone debug mode enabled: " + EntityDrone.DebugModeEnabled);
			}
			else if (text.Equals("teir"))
			{
				toggleDebugCam();
				toggleEnemiesInRange();
			}
			else if (text.Equals("eir"))
			{
				toggleEnemiesInRange();
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearDroneOwnedEntitiesForPlayer(EntityPlayer player)
	{
		OwnedEntityData[] ownedEntityClass = player.GetOwnedEntityClass("entityJunkDrone");
		for (int i = 0; i < ownedEntityClass.Length; i++)
		{
			player.RemoveOwnedEntity(ownedEntityClass[i]);
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
}
