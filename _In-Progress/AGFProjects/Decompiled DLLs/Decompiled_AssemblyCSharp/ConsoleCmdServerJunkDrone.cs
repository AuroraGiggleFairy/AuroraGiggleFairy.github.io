using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdServerJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "jds" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server junk drone commands.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return buildHelpLog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildHelpLog()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat("JunkDrone help:" + Environment.NewLine, "[Server/Host Commands]", Environment.NewLine), "jds, log man - logs out drone manager data", Environment.NewLine), "clear - clears drone data for player (ex. clear [PlayerName])", Environment.NewLine), "unstuck [playerId] - triggers teleport to player", Environment.NewLine);
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Log.Out(logDroneManager("Manager Drones"));
			return;
		}
		if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out(buildHelpLog());
			return;
		}
		if (_params[0].ContainsCaseInsensitive("save"))
		{
			DroneManager.Instance.TriggerSave();
			return;
		}
		if (_params[0].ContainsCaseInsensitive("log") && _params.Count > 1)
		{
			string a = _params[1];
			if (a.ContainsCaseInsensitive("man"))
			{
				Log.Out(logDroneManager("Manager Drones"));
			}
			if (a.ContainsCaseInsensitive("active"))
			{
				Log.Out(string.Concat("Active Drones" + Environment.NewLine, DroneManager.Instance.LogActiveDrones(), Environment.NewLine));
			}
			if (a.ContainsCaseInsensitive("unloaded"))
			{
				Log.Out(string.Concat("Unloaded Drones" + Environment.NewLine, DroneManager.Instance.LogUnloadedDrones(), Environment.NewLine));
			}
		}
		if (_params[0].ContainsCaseInsensitive("clear") && _params.Count > 1)
		{
			string b = _params[1];
			ClientInfo remoteClientInfo = _senderInfo.RemoteClientInfo;
			Log.Warning("isClientNull: " + (remoteClientInfo == null));
			List<EntityPlayer> list = GameManager.Instance.World.Players.list;
			for (int i = 0; i < list.Count; i++)
			{
				EntityPlayer entityPlayer = list[i];
				if (entityPlayer.EntityName.ContainsCaseInsensitive(b))
				{
					OwnedEntityData[] ownedEntityClass = entityPlayer.GetOwnedEntityClass("entityJunkDrone");
					for (int j = 0; j < ownedEntityClass.Length; j++)
					{
						entityPlayer.RemoveOwnedEntity(ownedEntityClass[j]);
					}
					DroneManager.Instance.ClearAllDronesForPlayer(entityPlayer);
					Log.Out("JunkDrone data cleared for {0}.", entityPlayer.EntityName);
					break;
				}
			}
			return;
		}
		if (_params[0].ContainsCaseInsensitive("unstuck"))
		{
			EntityPlayer entityPlayer2 = ((_senderInfo.RemoteClientInfo == null) ? GameManager.Instance.World.GetPrimaryPlayer() : (GameManager.Instance.World.GetEntity(_senderInfo.RemoteClientInfo.entityId) as EntityPlayer));
			if (entityPlayer2 == null)
			{
				return;
			}
			OwnedEntityData[] ownedEntityClass2 = entityPlayer2.GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass2.Length != 0)
			{
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass2[0].Id) as EntityDrone;
				if (entityDrone == null)
				{
					entityDrone = DroneManager.Instance.LoadDrone(ownedEntityClass2[0].Id, GameManager.Instance.World);
				}
				if (entityDrone != null)
				{
					entityDrone.TeleportUnstuck();
					Log.Out("drone unstuck complete");
				}
				else
				{
					Log.Warning("Server unstuck drone failed");
				}
			}
		}
		if (_params[0].ContainsCaseInsensitive("unassign"))
		{
			string playerName = _params[1];
			int result = -1;
			if (int.TryParse(_params[2], out result))
			{
				EntityPlayer entityPlayer3 = GameManager.Instance.World.Players.list.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName));
				if ((bool)entityPlayer3)
				{
					EntityDrone entityDrone2 = GameManager.Instance.World.GetEntity(result) as EntityDrone;
					if ((bool)entityDrone2)
					{
						entityDrone2.belongsPlayerId = -1;
						entityDrone2.Owner = null;
						entityPlayer3.RemoveOwnedEntity(entityDrone2);
						entityDrone2.OwnerID = null;
						Log.Out("unassigned drone {0} from player {1}", result, entityPlayer3.EntityName);
						return;
					}
				}
			}
		}
		if (_params[0].ContainsCaseInsensitive("assign"))
		{
			string playerName2 = _params[1];
			int result2 = -1;
			if (int.TryParse(_params[2], out result2))
			{
				EntityPlayer entityPlayer4 = GameManager.Instance.World.Players.list.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName2));
				if ((bool)entityPlayer4)
				{
					if (!EntityDrone.IsValidForLocalPlayer())
					{
						Log.Out("please pick up, or clear the currently deployed drone");
						return;
					}
					if (DroneManager.Instance.AssignUnloadedDrone(entityPlayer4, result2))
					{
						Log.Out("assigned unloaded drone {0} to player {1}", result2, entityPlayer4.EntityName);
					}
					else if (EntityDrone.IsValidForLocalPlayer())
					{
						EntityDrone entityDrone3 = GameManager.Instance.World.GetEntity(result2) as EntityDrone;
						if ((bool)entityDrone3)
						{
							entityDrone3.belongsPlayerId = entityPlayer4.entityId;
							entityDrone3.Owner = entityPlayer4;
							entityPlayer4.AddOwnedEntity(entityDrone3);
							entityDrone3.OwnerID = PlatformManager.InternalLocalUserIdentifier;
							Log.Warning(entityDrone3.OwnerID.ReadablePlatformUserIdentifier.ToString());
							Log.Out("assigned drone {0} to player {1}", result2, entityPlayer4.EntityName);
							return;
						}
						Log.Out("assign drone failed, id is not a drone");
					}
					else
					{
						Log.Out("assign drone failed");
					}
				}
				else
				{
					Log.Out("unknown player name: " + playerName2);
				}
			}
			else
			{
				Log.Out("assign drone parse failed");
			}
		}
		if (!_params[0].ContainsCaseInsensitive("eir"))
		{
			return;
		}
		string playerName3 = _params[1];
		int result3 = -1;
		if (int.TryParse(_params[2], out result3) && (bool)GameManager.Instance.World.Players.list.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName3)))
		{
			EntityDrone entityDrone4 = GameManager.Instance.World.GetEntity(result3) as EntityDrone;
			if ((bool)entityDrone4)
			{
				entityDrone4.DebugEnemiesInRange = !entityDrone4.DebugEnemiesInRange;
				Log.Out("JunkDrone DebugEnemiesInRange {0}", entityDrone4.DebugEnemiesInRange);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logDroneManager(string header)
	{
		string text = header + Environment.NewLine;
		List<EntityCreationData> allDronesECD = DroneManager.Instance.GetAllDronesECD();
		for (int i = 0; i < allDronesECD.Count; i++)
		{
			EntityCreationData entityCreationData = allDronesECD[i];
			text = text + $"#{i}, id {entityCreationData.id}, {EntityClass.GetEntityClassName(entityCreationData.entityClass)}, {entityCreationData.pos.ToCultureInvariantString()}, chunk {World.toChunkXZ(entityCreationData.pos)}" + Environment.NewLine;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedDrones(string header)
	{
		string text = header + Environment.NewLine;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			text += logPlayerOwnedDrones(player.Value.EntityId);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedDrones(int entityId)
	{
		string text = string.Empty;
		int num = 0;
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
		if ((bool)entityPlayer)
		{
			OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
			string text2 = string.Empty;
			foreach (OwnedEntityData ownedEntityData in ownedEntities)
			{
				if (ownedEntityData != null && EntityClass.list[ownedEntityData.ClassId].entityClassName.ContainsCaseInsensitive("entityJunkDrone"))
				{
					text2 = text2 + string.Format("entityId: {0}, classId: {1}, lastKnownPosition: {2}", ownedEntityData.Id, EntityClass.GetEntityClassName(ownedEntityData.ClassId), ownedEntityData.hasLastKnownPosition ? ownedEntityData.LastKnownPosition.ToString() : "none") + Environment.NewLine;
					num++;
				}
			}
			text += string.Format("[{0} - count({1})]" + Environment.NewLine + "{2}", entityPlayer.EntityName, num, text2);
		}
		return text;
	}
}
