using System.Collections.Concurrent;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DynamicMeshDebugConsoleCmd : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "Dynamic mesh debug";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GC_ENABLED = true;

	public override bool IsExecuteOnClient => false;

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params == null || _params.Count == 0)
		{
			DynamicMeshManager.Instance.AddChunk(new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition()), primary: true);
			return;
		}
		switch (_params[0].ToLower())
		{
		case "ss":
			DynamicMeshServer.ShowSender = true;
			break;
		case "dolog":
			DynamicMeshManager.DoLog = !DynamicMeshManager.DoLog;
			Log.Out("DyMesh doLog: " + DynamicMeshManager.DoLog);
			break;
		case "lognet":
			DynamicMeshManager.DoLogNet = !DynamicMeshManager.DoLogNet;
			Log.Out("DoLogNet: " + DynamicMeshManager.DoLogNet);
			break;
		case "tars":
		{
			Vector3 position = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			DynamicMeshManager.ImportVox("tars", position, 502);
			break;
		}
		case "vox":
		{
			string param = GetParam(_params, 1);
			Vector3 position2 = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			int.TryParse(GetParam(_params, 2) ?? "502", out var result);
			DynamicMeshManager.ImportVox(param, position2, result);
			break;
		}
		case "areaaround":
		case "aa":
		{
			int num4 = ((_params.Count < 2) ? 150 : int.Parse(_params[1]));
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			Vector3 vector = ((!(primaryPlayer == null)) ? primaryPlayer.position : GameManager.Instance.World.GetPlayers().FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).position);
			for (int num5 = (int)vector.x - num4; (float)num5 < vector.x + (float)num4; num5 += 16)
			{
				for (int num6 = (int)vector.z - num4; (float)num6 < vector.z + (float)num4; num6 += 16)
				{
					DynamicMeshManager.Instance.AddChunk(new Vector3i(num5, 0, num6), primary: true);
				}
			}
			Vector3 vector2 = vector;
			Log.Out("Adding chunks around  " + vector2.ToString() + " rad: " + num4);
			break;
		}
		case "air":
		{
			if (_params.Count < 1)
			{
				DynamicMeshManager.LogMsg("Specify a radius");
				break;
			}
			int num = int.Parse(GetParam(_params, 1));
			BlockValue air = BlockValue.Air;
			Vector3i vector3i = new Vector3i(GameManager.Instance.World.Players.list.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).GetPosition());
			World world = GameManager.Instance.World;
			for (int num2 = vector3i.x - num; num2 < vector3i.x + num; num2++)
			{
				for (int num3 = vector3i.z - num; num3 < vector3i.z + num; num3++)
				{
					Vector3i vector3i2 = new Vector3i(num2, vector3i.y, num3);
					if (world.GetBlock(vector3i2).type != 0)
					{
						world.SetBlockRPC(vector3i2, air);
					}
				}
			}
			break;
		}
		case "log":
		{
			string text = "Buff : " + DynamicMeshManager.Instance.BufferRegionLoadRequests.Count + "\n Server : " + SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer + " / " + SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient + "\n Thread P/S : " + DynamicMeshThread.PrimaryQueue.Count + " / " + DynamicMeshThread.SecondaryQueue.Count + "\n Packets: " + NetPackageDynamicMesh.Count + "\n ThreadDistance: " + DynamicMeshManager.ThreadDistance + "\n ObserverDistance: " + DynamicMeshManager.ObserverDistance + "\n ItemLoadDistance: " + DynamicMeshManager.ItemLoadDistance + "\n WorldChunks: " + GameManager.Instance.World.ChunkCache.chunks.list.Count + "\n ThreadNext: " + DynamicMeshThread.nextChunks.Count + "\n ThreadQueue: " + DynamicMeshThread.Queue + "\n RegionUpdates: " + DynamicMeshThread.RegionUpdates.Count + "\n RegionUpdatesDebug: " + DynamicMeshThread.RegionUpdatesDebug + "\n SyncPackets: " + DynamicMeshServer.SyncRequests.Count + " (" + DynamicMeshServer.ActiveSyncs.Count + ")";
			foreach (DynamicMeshClientConnection value in DynamicMeshServer.ClientData.Values)
			{
				text = text + "\n " + value.EntityId + ": " + value.ItemsToSend.Values.Sum([PublicizedFrom(EAccessModifier.Internal)] (ConcurrentQueue<DynamicMeshSyncRequest> d) => d.Count);
			}
			Log.Out("Info: " + text);
			break;
		}
		case "fog":
		{
			float start = float.MinValue;
			float end = float.MinValue;
			if (_params.Count >= 1)
			{
				float density = StringParsers.ParseFloat(_params[1]);
				SkyManager.SetFogDebug(density, start, end);
				Log.Out("Fog " + density);
			}
			break;
		}
		case "checkprefabs":
		case "cp":
		case "forcegen":
			DynamicMeshManager.Instance.CheckPrefabs("Console", forceRegen: true);
			break;
		case "resend":
			DynamicMeshServer.ResendPackages = !DynamicMeshServer.ResendPackages;
			Log.Out("Resending dymesh packages: " + DynamicMeshServer.ResendPackages);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(List<string> _params, int index)
	{
		int result = -9999;
		if (_params == null)
		{
			return result;
		}
		if (index >= _params.Count)
		{
			return result;
		}
		int.TryParse(_params[index], out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { info, "zd" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return info;
	}
}
