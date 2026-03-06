using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using Audio;
using Platform;
using UniLinq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class DynamicMeshConsoleCmd : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "Dynamic mesh";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GC_ENABLED = true;

	public override bool AllowedInMainMenu => true;

	public override bool IsExecuteOnClient => true;

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params == null || _params.Count == 0)
		{
			DynamicMeshManager.Instance.AddChunk(new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition()), primary: true);
			return;
		}
		string text = _params[0].ToLower();
		if (text == "block")
		{
			Vector3i vector3i = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
			vector3i.y += 3;
			int num = int.Parse(_params[1]);
			int num2 = int.Parse(_params[2]);
			int num3 = int.Parse(_params[3]);
			List<BlockChangeInfo> list = new List<BlockChangeInfo>();
			BlockValue blockValue = Block.GetBlockValue("concreteBlock", _caseInsensitive: true);
			for (int i = vector3i.y; i < vector3i.y + num2; i++)
			{
				for (int j = vector3i.x - num; j < vector3i.x + num; j++)
				{
					for (int k = vector3i.z - num3; k < vector3i.z + num3; k++)
					{
						BlockChangeInfo item = new BlockChangeInfo(new Vector3i(j, i, k), blockValue, 0);
						list.Add(item);
					}
				}
			}
			GameManager.Instance.SetBlocksRPC(list);
		}
		if (text == "pause")
		{
			DynamicMeshThread.Paused = !DynamicMeshThread.Paused;
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Dynamic Paused: " + DynamicMeshThread.Paused);
			}
		}
		else if (text == "fog")
		{
			float start = float.MinValue;
			float end = float.MinValue;
			if (_params.Count >= 1)
			{
				float density = StringParsers.ParseFloat(_params[1]);
				SkyManager.SetFogDebug(density, start, end);
				Log.Out("Fog " + density);
			}
		}
		else if (text == "orphans")
		{
			Log.Out("Running orphan checks...");
			DynamicMeshManager.Instance.ForceOrphanChecks();
		}
		else if (text == "dolog")
		{
			DynamicMeshManager.DoLog = !DynamicMeshManager.DoLog;
			Log.Out("Dolog: " + DynamicMeshManager.DoLog);
		}
		else if (text == "qef")
		{
			DynamicMeshVoxel.QefToFile("C:\\Users\\D\\Documents\\Qubicle 3.0\\Tars.qef", "C:\\Users\\D\\Documents\\Qubicle 3.0\\", "tars");
		}
		else if (text == "tars")
		{
			DynamicMeshManager.ImportVox("tars", GameManager.Instance.World.GetPrimaryPlayer().position, 502);
		}
		else if (text == "vox")
		{
			string param = GetParam(_params, 1);
			int.TryParse(GetParam(_params, 2) ?? "502", out var result);
			DynamicMeshManager.ImportVox(param, GameManager.Instance.World.GetPrimaryPlayer().position, result);
		}
		else if (text == "lognet")
		{
			DynamicMeshManager.DoLogNet = !DynamicMeshManager.DoLogNet;
			Log.Out("DoLogNet: " + DynamicMeshManager.DoLogNet);
		}
		else if (text == "settings")
		{
			DynamicMeshSettings.LogSettings();
		}
		else if (text == "imp")
		{
			DynamicMeshSettings.UseImposterValues = !DynamicMeshSettings.UseImposterValues;
			Log.Out("Use Imposter Values: " + DynamicMeshSettings.UseImposterValues);
		}
		else if (text == "useimpostervalues")
		{
			DynamicMeshSettings.UseImposterValues = !DynamicMeshSettings.UseImposterValues;
			DynamicMeshBlockSwap.Init();
			Log.Out("Use Imposter Values: " + DynamicMeshSettings.UseImposterValues);
		}
		else if (text == "playerareaonly" || text == "pao")
		{
			DynamicMeshSettings.OnlyPlayerAreas = !DynamicMeshSettings.OnlyPlayerAreas;
			DynamicMeshSettings.Validate();
			Log.Out("Player Area Only: " + DynamicMeshSettings.OnlyPlayerAreas);
		}
		else if (text == "playerareabuffer" || text == "pab")
		{
			DynamicMeshSettings.PlayerAreaChunkBuffer = Math.Max(1, GetParamAsInt(_params, 1));
			Log.Out("Player Area Buffer: " + DynamicMeshSettings.PlayerAreaChunkBuffer);
		}
		else if (text == "newworldregen")
		{
			DynamicMeshSettings.NewWorldFullRegen = !DynamicMeshSettings.NewWorldFullRegen;
			Log.Out("World full regen: " + DynamicMeshSettings.NewWorldFullRegen);
		}
		else
		{
			if (text == "loadregion")
			{
				return;
			}
			if (text == "regenall")
			{
				foreach (KeyValuePair<long, DynamicMeshItem> item2 in DynamicMeshManager.Instance.ItemsDictionary)
				{
					if (item2.Value.FileExists())
					{
						DynamicMeshThread.RequestSecondaryQueue(item2.Value);
					}
				}
				return;
			}
			if (text == "regenregion")
			{
				int paramAsInt = GetParamAsInt(_params, 1);
				int paramAsInt2 = GetParamAsInt(_params, 2);
				DynamicMeshThread.AddRegionUpdateData(paramAsInt, paramAsInt2, isUrgent: true);
				return;
			}
			if (text == "setmaxregion")
			{
				DynamicMeshSettings.MaxViewDistance = GetParamAsInt(_params, 1);
				DynamicMeshManager.LogMsg("New limit: " + DynamicMeshSettings.MaxViewDistance);
				return;
			}
			if (text == "setmaxitem")
			{
				DynamicMeshRegion.ItemLoadIndex = GetParamAsInt(_params, 1);
				DynamicMeshRegion.ItemUnloadIndex = DynamicMeshRegion.ItemLoadIndex + 1;
				DynamicMeshManager.LogMsg("New limit: " + DynamicMeshRegion.ItemLoadIndex);
				return;
			}
			if (text == "clearlod")
			{
				GameManager.Instance.prefabLODManager.Cleanup();
				return;
			}
			if (text == "lod")
			{
				DynamicMeshManager.DisableLOD = !DynamicMeshManager.DisableLOD;
				foreach (PrefabLODManager.PrefabGameObject value in GameManager.Instance.prefabLODManager.displayedPrefabs.Values)
				{
					if (value.go != null)
					{
						value.go.SetActive(!DynamicMeshManager.DisableLOD);
					}
				}
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Disable LOD: " + DynamicMeshManager.DisableLOD);
				}
				return;
			}
			if (text == "index")
			{
				int paramAsInt3 = GetParamAsInt(_params, 1);
				int paramAsInt4 = GetParamAsInt(_params, 2);
				bool flag = DynamicMeshRegion.IsInBuffer(paramAsInt3, paramAsInt4);
				Log.Out("Is " + paramAsInt3 + "," + paramAsInt4 + " in buffer: " + flag);
				return;
			}
			if (text == "meshlock")
			{
				DynamicMeshThread.LockMeshesAfterGenerating = !DynamicMeshThread.LockMeshesAfterGenerating;
				Log.Out("Lock Mesh: " + DynamicMeshThread.LockMeshesAfterGenerating);
				return;
			}
			if (text == "regioncount")
			{
				int paramAsInt5 = GetParamAsInt(_params, 1);
				DynamicMeshSettings.MaxRegionMeshData = (DynamicMeshManager.Instance.AvailableRegionLoadRequests = paramAsInt5);
				Log.Out("MaxRegionMeshData: " + paramAsInt5);
				return;
			}
			if (text == "update")
			{
				int paramAsInt6 = GetParamAsInt(_params, 1);
				int paramAsInt7 = GetParamAsInt(_params, 2);
				DynamicMeshManager.ChunkChanged(new Vector3i(paramAsInt6, 0, paramAsInt7), DynamicMeshManager.player.entityId, 1);
				Log.Out("added " + paramAsInt6 + "," + paramAsInt7);
				return;
			}
			if (text == "chunkname")
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				DynamicMeshItem itemOrNull = DynamicMeshManager.Instance.GetItemOrNull(new Vector3i(primaryPlayer.position));
				DynamicMeshRegion dynamicMeshRegion = itemOrNull?.GetRegion();
				string text2 = itemOrNull?.ChunkObject?.name ?? "missing";
				string text3 = dynamicMeshRegion?.RegionObject?.name ?? "missing";
				Log.Out("Chunk " + text2 + "  Region:  " + text3);
				return;
			}
			if (text == "traders")
			{
				GameManager.Instance.World.GetPrimaryPlayer();
				{
					foreach (PrefabInstance item3 in (from d in GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs()
						where d.prefab.HasAnyQuestTag(QuestEventManager.traderTag)
						select d).ToList())
					{
						Vector3i boundingBoxPosition = item3.boundingBoxPosition;
						Log.Out("Trader at " + boundingBoxPosition.ToString());
						Waypoint waypoint = new Waypoint();
						waypoint.pos = World.worldToBlockPos(item3.boundingBoxPosition.ToVector3() + item3.boundingBoxSize.ToVector3() / 2f);
						waypoint.icon = "ui_game_symbol_safe";
						waypoint.name.Update("Trader", PlatformManager.MultiPlatform.User.PlatformUserId);
						waypoint.ownerId = null;
						waypoint.lastKnownPositionEntityId = -1;
						EntityPlayer primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
						if (!primaryPlayer2.Waypoints.ContainsWaypoint(waypoint))
						{
							primaryPlayer2.Waypoints.Collection.Add(waypoint);
							if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
							{
								MapObjectWaypoint mo = new MapObjectWaypoint(waypoint);
								GameManager.Instance.World.ObjectOnMapAdd(mo);
							}
						}
					}
					return;
				}
			}
			if (text == "cloth")
			{
				foreach (XmlData item4 in Manager.audioData.Values.Where([PublicizedFrom(EAccessModifier.Internal)] (XmlData d) => d.audioClipMap.Any([PublicizedFrom(EAccessModifier.Internal)] (ClipSourceMap e) => e.clipName.ContainsCaseInsensitive("hitmetal"))).ToList())
				{
					for (int num4 = 0; num4 < item4.audioClipMap.Count; num4++)
					{
						ClipSourceMap clipSourceMap = item4.audioClipMap[num4];
						Log.Out("Updating " + clipSourceMap.clipName);
						clipSourceMap.clipName = clipSourceMap.clipName.Replace("hitmetal", "hitcloth");
						item4.audioClipMap[num4] = clipSourceMap;
					}
				}
				return;
			}
			if (text == "ms")
			{
				int paramAsInt8 = GetParamAsInt(_params, 1);
				if (paramAsInt8 > 0)
				{
					DynamicMeshFile.ReadMeshMax = paramAsInt8;
				}
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("New max MS: " + DynamicMeshFile.ReadMeshMax);
				}
				return;
			}
			if (text == "listblocks")
			{
				Block[] list2 = Block.list;
				foreach (Block block in list2)
				{
					if (block != null && DynamicMeshManager.DoLog)
					{
						DynamicMeshManager.LogMsg("Block " + block.GetBlockName() + "  Id: " + block.blockID);
					}
				}
				return;
			}
			if (text == "nopro")
			{
				DynamicMeshThread.NoProcessing = !DynamicMeshThread.NoProcessing;
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Dynamic No Processing: " + DynamicMeshThread.NoProcessing);
				}
				return;
			}
			if (text == "meshthreads" || text == "threads")
			{
				int paramAsInt9 = GetParamAsInt(_params, 1);
				DynamicMeshThread.BuilderManager.SetNewLimit(paramAsInt9);
				return;
			}
			if (text == "activesyncs")
			{
				DynamicMeshServer.MaxActiveSyncs = GetParamAsInt(_params, 1);
				Log.Out("Max active syncs: " + DynamicMeshServer.MaxActiveSyncs);
				return;
			}
			if (text == "guiy")
			{
				DynamicMeshManager.GuiY = GetParamAsInt(_params, 1);
				Log.Out("GUI y: " + DynamicMeshManager.GuiY);
				return;
			}
			if (text == "imax")
			{
				int paramAsInt10 = GetParamAsInt(_params, 1);
				DynamicMeshThread.ChunkDataQueue.MaxAllowedItems = paramAsInt10;
				Log.Out("Max Item: " + DynamicMeshThread.ChunkDataQueue.MaxAllowedItems);
				Log.Out("Max Region: " + DynamicMeshThread.ChunkDataQueue.MaxAllowedItems);
				return;
			}
			if (text == "loadqueues")
			{
				DynamicMeshThread.SetNextChunksFromQueues();
				return;
			}
			if (text == "farreach" || text == "fr")
			{
				if (Constants.cDigAndBuildDistance != 50f)
				{
					Constants.cDigAndBuildDistance = 50f;
					Constants.cBuildIntervall = 0.2f;
					Constants.cCollectItemDistance = 50f;
				}
				else
				{
					Constants.cDigAndBuildDistance = 5f;
					Constants.cBuildIntervall = 0.5f;
					Constants.cCollectItemDistance = 3.5f;
				}
				Log.Out("Reach distance: " + Constants.cDigAndBuildDistance);
				return;
			}
			if (text == "reorder")
			{
				try
				{
					DynamicMeshManager.Instance.ReorderGameObjects();
					return;
				}
				catch (Exception ex)
				{
					Log.Error("Reorder error: " + ex.Message);
					return;
				}
			}
			if (text == "scope")
			{
				DynamicMeshManager.DisableScopeTexture = !DynamicMeshManager.DisableScopeTexture;
				return;
			}
			if (text == "kit")
			{
				AddKit();
				return;
			}
			if (text == "info")
			{
				Vector3 position = GameManager.Instance.World.GetPrimaryPlayer().position;
				string x = GetParam(_params, 1) ?? DynamicMeshUnity.GetChunkPositionFromWorldPosition(position.x).ToString();
				string z = GetParam(_params, 2) ?? DynamicMeshUnity.GetChunkPositionFromWorldPosition(position.z).ToString();
				DynamicMeshRegion dynamicMeshRegion2 = DynamicMeshRegion.Regions.Values.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshRegion d) => d.WorldPosition.x.ToString() == x && d.WorldPosition.z.ToString() == z);
				if (dynamicMeshRegion2 == null)
				{
					return;
				}
				DynamicMeshManager.LogMsg(dynamicMeshRegion2.ToDebugLocation() + " UnloadedItems");
				foreach (DynamicMeshItem item5 in from d in dynamicMeshRegion2.UnloadedItems
					orderby d.WorldPosition.x, d.WorldPosition.z
					select d)
				{
					DynamicMeshManager.LogMsg("Item " + item5.ToDebugLocation() + "  state: " + item5.State);
				}
				DynamicMeshManager.LogMsg(dynamicMeshRegion2.ToDebugLocation() + " LoadedItems");
				{
					foreach (DynamicMeshItem item6 in from d in dynamicMeshRegion2.LoadedItems
						orderby d.WorldPosition.x, d.WorldPosition.z
						select d)
					{
						DynamicMeshManager.LogMsg("Item " + item6.ToDebugLocation() + " state: " + item6.State.ToString() + "  chunk: " + ((item6.ChunkObject == null) ? "null" : item6.ChunkObject.activeSelf.ToString()));
						if (item6.ChunkObject != null)
						{
							item6.ChunkObject.SetActive(value: true);
						}
					}
					return;
				}
			}
			if (text == "tnt")
			{
				if (_params.Count < 1)
				{
					DynamicMeshManager.LogMsg("Specify a radius");
					return;
				}
				int num6 = int.Parse(GetParam(_params, 1));
				BlockValue blockValue2 = Block.GetBlockValue("cntBarrelOilSingle00");
				Vector3i vector3i2 = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
				World world = GameManager.Instance.World;
				for (int num7 = vector3i2.x - num6; num7 < vector3i2.x + num6; num7++)
				{
					for (int num8 = vector3i2.z - num6; num8 < vector3i2.z + num6; num8++)
					{
						if (num7 % 2 != 0 && num8 % 2 != 0)
						{
							Vector3i vector3i3 = new Vector3i(num7, vector3i2.y, num8);
							if (world.GetBlock(vector3i3).isair)
							{
								world.SetBlockRPC(vector3i3, blockValue2);
							}
						}
					}
				}
				return;
			}
			if (text == "air")
			{
				if (_params.Count < 1)
				{
					DynamicMeshManager.LogMsg("Specify a radius");
					return;
				}
				int num9 = int.Parse(GetParam(_params, 1));
				BlockValue air = BlockValue.Air;
				Vector3i vector3i4 = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
				World world2 = GameManager.Instance.World;
				for (int num10 = vector3i4.x - num9; num10 < vector3i4.x + num9; num10++)
				{
					for (int num11 = vector3i4.z - num9; num11 < vector3i4.z + num9; num11++)
					{
						Vector3i vector3i5 = new Vector3i(num10, vector3i4.y, num11);
						if (world2.GetBlock(vector3i5).type != 0)
						{
							world2.SetBlockRPC(vector3i5, air);
						}
					}
				}
				return;
			}
			if (text == "checkprefabs" || text == "cp" || text == "forcegen")
			{
				if (_params.Count > 1 && _params[1] == "all")
				{
					DebugAll();
				}
				DynamicMeshManager.Instance.CheckPrefabs("Console", forceRegen: true);
				return;
			}
			if (text == "sc")
			{
				foreach (DynamicMeshItem value2 in DynamicMeshManager.Instance.ItemsDictionary.Values)
				{
					if (value2.ChunkObject != null)
					{
						value2.SetVisible(active: true, "show chunk cmd");
					}
					else if (DynamicMeshManager.DoLog)
					{
						Vector3i boundingBoxPosition = value2.WorldPosition;
						DynamicMeshManager.LogMsg("Item chunk null " + boundingBoxPosition.ToString());
					}
				}
				return;
			}
			if (text == "showhide")
			{
				DynamicMeshManager.DebugReport = true;
				DynamicMeshManager.Instance.ShowOrHidePrefabs();
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("ShowHide run");
				}
				DynamicMeshManager.DebugReport = false;
				return;
			}
			if (text == "copter")
			{
				int paramAsInt11 = GetParamAsInt(_params, 1);
				DynamicProperties dynamicProperties = Vehicle.PropertyMap["vehicleGyrocopter".ToLower()];
				dynamicProperties.Values["velocityMax"] = "9, " + paramAsInt11;
				Log.Out("Max: " + dynamicProperties.Values["velocityMax"]);
				return;
			}
			if (text == "gctoggle")
			{
				if (GC_ENABLED)
				{
					GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
				}
				else
				{
					GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
					GC.Collect();
				}
				GC_ENABLED = !GC_ENABLED;
				Log.Out("GC: " + GC_ENABLED);
				return;
			}
			if (text == "gc")
			{
				if (GetParam(_params, 1).EqualsCaseInsensitive("all"))
				{
					Log.Out("collecting...");
					GarbageCollector.CollectIncremental(2147483647uL);
				}
				else
				{
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect();
				}
				Log.Out("gc done");
				return;
			}
			if (text == "toggle")
			{
				bool flag2 = !GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
				GamePrefs.Set(EnumGamePrefs.DynamicMeshEnabled, flag2);
				DynamicMeshManager.EnabledChanged(flag2);
				Log.Out("DM changed. Enabled: " + flag2);
				return;
			}
			if (text == "deco")
			{
				DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
				Vector3 playerPos = GameManager.Instance.World.GetPrimaryPlayer().position;
				{
					foreach (PrefabInstance item7 in from d in dynamicPrefabDecorator.GetDynamicPrefabs()
						orderby Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3()))
						select d)
					{
						Vector3i boundingBoxPosition = item7.boundingBoxPosition;
						DynamicMeshManager.LogMsg("Deco Prefab at " + boundingBoxPosition.ToString() + " name: " + item7.name);
					}
					return;
				}
			}
			if (text == "resend")
			{
				DynamicMeshServer.ResendPackages = !DynamicMeshServer.ResendPackages;
				Log.Out("Resending dymesh packages: " + DynamicMeshServer.ResendPackages);
				return;
			}
			if (text == "restart")
			{
				DynamicMeshManager.OnWorldUnload();
				DynamicMeshManager.Init();
				return;
			}
			if (text == "wipe")
			{
				string param2 = GetParam(_params, 1);
				if (param2 == null || param2 == "1")
				{
					if (DynamicMeshManager.DoLog)
					{
						DynamicMeshManager.LogMsg("Clear mesh pool");
					}
					DynamicMeshFile.VoxelMeshPool = new ConcurrentQueue<VoxelMesh>();
				}
				if (param2 == null || param2 == "2")
				{
					if (DynamicMeshManager.DoLog)
					{
						DynamicMeshManager.LogMsg("Clear unsed assets");
					}
					Resources.UnloadUnusedAssets();
				}
				return;
			}
			if (text == "pool")
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Items in pool: " + DynamicMeshFile.VoxelMeshPool.Count);
				}
				long num12 = 0L;
				foreach (VoxelMesh item8 in DynamicMeshFile.VoxelMeshPool)
				{
					if (item8 == null)
					{
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg("Null item in pool");
						}
						continue;
					}
					if (item8.CollIndices.Items != null)
					{
						num12 += 4 * item8.CollIndices.Items.Length;
					}
					if (item8.CollVertices.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector3)) * item8.CollVertices.Items.Length;
					}
					if (item8.Indices.Items != null)
					{
						num12 += 4 * item8.Indices.Items.Length;
					}
					if (item8.Normals.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector3)) * item8.Normals.Items.Length;
					}
					if (item8.ColorVertices.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Color)) * item8.ColorVertices.Items.Length;
					}
					if (item8.Tangents.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector4)) * item8.Tangents.Items.Length;
					}
					if (item8.Uvs.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector2)) * item8.Uvs.Items.Length;
					}
					if (item8.UvsCrack.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector2)) * item8.UvsCrack.Items.Length;
					}
					if (item8.Vertices.Items != null)
					{
						num12 += Marshal.SizeOf(typeof(Vector3)) * item8.Vertices.Items.Length;
					}
				}
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Total bytes: " + num12 + " = " + (double)num12 / 1024.0 / 1024.0 + "MB  or " + num12 + "  bytes. v3: " + Marshal.SizeOf(typeof(Vector3)));
				}
				return;
			}
			if (text == "font")
			{
				int paramAsInt12 = GetParamAsInt(_params, 1);
				Log.Out("Font size from " + DynamicMeshManager.DebugStyle.fontSize + " to " + paramAsInt12);
				DynamicMeshManager.DebugStyle.fontSize = paramAsInt12;
				return;
			}
			if (text == "meshsize")
			{
				long num13 = 0L;
				long num14 = 0L;
				foreach (Transform item9 in DynamicMeshManager.Parent.transform)
				{
					if (item9.gameObject.name.StartsWith("C", StringComparison.OrdinalIgnoreCase))
					{
						num14 += DynamicMeshUnity.GetMeshSize(item9.gameObject);
					}
					else
					{
						num13 += DynamicMeshUnity.GetMeshSize(item9.gameObject);
					}
				}
				Log.Out("Total chunk mesh: " + num14 / 1024 / 1024 + "MB");
				Log.Out("Total region mesh: " + num13 / 1024 / 1024 + "MB");
				return;
			}
			if (text == "maxdata")
			{
				DynamicMeshSettings.MaxDyMeshData = GetParamAsInt(_params, 1);
				Log.Out($"MaxItems: {DynamicMeshSettings.MaxDyMeshData}");
				return;
			}
			if (text == "enabled")
			{
				DynamicMeshManager.CONTENT_ENABLED = !DynamicMeshManager.CONTENT_ENABLED;
				if (DynamicMeshManager.Instance != null)
				{
					DynamicMeshManager.Instance.Awake();
				}
				Log.Out("Dynamic Mesh enabled: " + DynamicMeshManager.CONTENT_ENABLED);
				return;
			}
			if (text == "gocheck")
			{
				DynamicMeshManager.Instance.CheckGameObjects();
				return;
			}
			if (text == "debugreleases")
			{
				DynamicMeshManager.DebugReleases = !DynamicMeshManager.DebugReleases;
				Log.Out("Debug releases: " + DynamicMeshManager.DebugReleases);
				return;
			}
			if (text == "freequeues" || text == "fq")
			{
				DynamicMeshThread.ChunkDataQueue.FreeMemory();
				return;
			}
			if (text == "showchunks" || text == "sc")
			{
				foreach (DynamicMeshItem value3 in DynamicMeshManager.Instance.ItemsDictionary.Values)
				{
					if (value3.ChunkObject != null && !value3.ChunkObject.activeSelf)
					{
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg("Showing hidden chunk: " + value3.ToDebugLocation());
						}
						value3.SetVisible(active: true, "forced");
					}
				}
				return;
			}
			if (text == "hidechunks" || text == "hc")
			{
				foreach (DynamicMeshItem value4 in DynamicMeshManager.Instance.ItemsDictionary.Values)
				{
					if (value4.ChunkObject != null && value4.ChunkObject.activeSelf)
					{
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg("Showing hidden chunk: " + value4.ToDebugLocation());
						}
						value4.SetVisible(active: false, "forced");
					}
				}
				return;
			}
			if (text == "ur")
			{
				int paramAsInt13 = GetParamAsInt(_params, 1);
				int paramAsInt14 = GetParamAsInt(_params, 2);
				DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(paramAsInt13, paramAsInt14);
				DynamicMeshManager.Instance.UpdateDynamicPrefabDecoratorRegion(region);
				return;
			}
			if (text == "checkregions" || text == "cr" || text == "crr")
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Region count: " + DynamicMeshRegion.Regions.Count);
				}
				string filter = GetParam(_params, 1) ?? "";
				if (filter.ToLower() == "this")
				{
					Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(GameManager.Instance.World.GetPrimaryPlayer().position);
					filter = regionPositionFromWorldPosition.x + "," + regionPositionFromWorldPosition.z;
				}
				_ = GameManager.Instance.World.GetPrimaryPlayer().position;
				int num15 = 0;
				int num16 = 0;
				int num17 = 0;
				int num18 = 0;
				int num19 = 0;
				int num20 = 0;
				ICollection<DynamicMeshRegion> values = DynamicMeshRegion.Regions.Values;
				int num21 = 0;
				int num22 = 0;
				int num23 = 0;
				int num24 = 0;
				long num25 = 0L;
				foreach (DynamicMeshRegion item10 in from d in values
					orderby d.WorldPosition.ToString()
					where filter == "" || d.ToDebugLocation().Contains(filter)
					select d)
				{
					int num26 = ((!(item10.RegionObject == null)) ? item10.RegionObjects : 0);
					int num27 = ((!(item10.RegionObject == null)) ? item10.Vertices : 0);
					int num28 = ((!(item10.RegionObject == null)) ? item10.Triangles : 0);
					num15 += num26;
					num17 += num28;
					num16 += num27;
					if (item10.RegionObject != null)
					{
						num25 += Profiler.GetRuntimeMemorySizeLong(item10.RegionObject.GetComponent<MeshFilter>().mesh);
						if (item10.RegionObject.GetComponent<MeshRenderer>().isVisible)
						{
							num18 += num26;
							num20 += num28;
							num19 += num27;
							num22++;
						}
						if (item10.RegionObject.activeSelf)
						{
							num21++;
						}
					}
					DynamicMeshManager.LogMsg("Region " + item10.ToDebugLocation() + " state: " + item10.State.ToString() + " | Index: ?   xi: " + item10.xIndex + "," + item10.zIndex + " | UnloadedItems: " + item10.UnloadedItems.Count + "  LoadedItems: " + item10.LoadedItems.Count + " Chunks: " + item10.LoadedChunks.Count + " Visible: " + item10.LoadedItems.Where([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => d.ChunkObject != null && d.ChunkObject.activeSelf).Count() + " Distance: " + item10.DistanceToPlayer() + "  RegionVisible: " + ((item10.RegionObject == null) ? "null" : item10.RegionObject.activeSelf.ToString()) + "  RegionObjects: " + num26 + "  Triangles: " + num28 + "  Vertices: " + num27);
				}
				foreach (DynamicMeshItem value5 in DynamicMeshManager.Instance.ItemsDictionary.Values)
				{
					int num29 = ((!(value5.ChunkObject == null)) ? value5.Vertices : 0);
					int num30 = ((!(value5.ChunkObject == null)) ? value5.Triangles : 0);
					num17 += num30;
					num16 += num29;
					if (value5.ChunkObject != null)
					{
						if (value5.ChunkObject.GetComponent<MeshRenderer>().isVisible)
						{
							num20 += num30;
							num19 += num29;
							num24++;
						}
						if (value5.ChunkObject.activeSelf)
						{
							num23++;
						}
					}
				}
				DynamicMeshManager.LogMsg("Total Objects: " + num15 + "   Total Tris: " + num17 + "    Total Verts: " + num16);
				DynamicMeshManager.LogMsg("Visible Objects: " + num18 + "   Total Tris: " + num20 + "    Total Verts: " + num19);
				DynamicMeshManager.LogMsg("Total Active Regions: " + num21 + "    Active Items: " + num23);
				DynamicMeshManager.LogMsg("Total Rendered Regions: " + num22 + "    Rendered Items: " + num24);
				return;
			}
			if (text == "findchunk")
			{
				int x2 = GetParamAsInt(_params, 1);
				int z2 = GetParamAsInt(_params, 2);
				string msg = "chunk not found";
				foreach (KeyValuePair<long, DynamicMeshRegion> region2 in DynamicMeshRegion.Regions)
				{
					if (region2.Value.LoadedChunks.Any([PublicizedFrom(EAccessModifier.Internal)] (Vector3i d) => d.x == x2 && d.z == z2))
					{
						msg = "Chunk found in region " + region2.Value.ToDebugLocation();
						break;
					}
				}
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg(msg);
				}
				return;
			}
			if (text == "meshinfo")
			{
				int paramAsInt15 = GetParamAsInt(_params, 2);
				int paramAsInt16 = GetParamAsInt(_params, 3);
				DynamicMeshItem itemFromWorldPosition = DynamicMeshManager.Instance.GetItemFromWorldPosition(paramAsInt15, paramAsInt16);
				if (itemFromWorldPosition == null)
				{
					if (DynamicMeshManager.DoLog)
					{
						DynamicMeshManager.LogMsg("Mesh not found at " + paramAsInt15 + "," + paramAsInt16);
					}
				}
				else if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Mesh " + paramAsInt15 + "," + paramAsInt16 + " GO Vis: " + ((itemFromWorldPosition.ChunkObject == null) ? "null" : (itemFromWorldPosition.ChunkObject.activeSelf + " tris: " + itemFromWorldPosition.Triangles + " verts: " + itemFromWorldPosition.Vertices)));
				}
				return;
			}
			if (text == "showpos")
			{
				DynamicMeshManager.DebugItemPositions = !DynamicMeshManager.DebugItemPositions;
				return;
			}
			if (text == "dr")
			{
				DynamicMeshManager.DebugReport = true;
				return;
			}
			switch (text)
			{
			case "dx":
				DynamicMeshManager.DebugX = int.Parse(_params[1]);
				break;
			case "dz":
				DynamicMeshManager.DebugZ = int.Parse(_params[1]);
				break;
			case "dr":
				DynamicMeshManager.DebugReport = true;
				break;
			case "itemload":
			{
				int x3 = int.Parse(_params[1]);
				int z3 = int.Parse(_params[2]);
				DynamicMeshItem itemOrNull2 = DynamicMeshManager.Instance.GetItemOrNull(new Vector3i(x3, 0, z3));
				if (itemOrNull2 != null)
				{
					DynamicMeshManager.Instance.AddItemLoadRequest(itemOrNull2, urgent: true);
				}
				break;
			}
			case "area":
			{
				int num34 = int.Parse(_params[1]);
				int num35 = int.Parse(_params[2]);
				int num36 = int.Parse(_params[3]);
				int num37 = int.Parse(_params[4]);
				for (int num38 = num34; num38 < num36; num38 += 16)
				{
					for (int num39 = num35; num39 < num37; num39 += 16)
					{
						DynamicMeshManager.Instance.AddChunk(new Vector3i(num38, 0, num39), primary: true);
					}
				}
				break;
			}
			case "areaaround":
			case "aa":
			{
				int num31 = ((_params.Count < 2) ? 150 : int.Parse(_params[1]));
				Vector3 position5 = GameManager.Instance.World.GetPrimaryPlayer().position;
				for (int num32 = (int)position5.x - num31; (float)num32 < position5.x + (float)num31; num32 += 16)
				{
					for (int num33 = (int)position5.z - num31; (float)num33 < position5.z + (float)num31; num33 += 16)
					{
						DynamicMeshManager.Instance.AddChunk(new Vector3i(num32, 0, num33), primary: true);
					}
				}
				break;
			}
			case "refreshall":
				DynamicMeshManager.Instance.RefreshAll();
				break;
			case "debug":
			case "dd":
				DynamicMeshManager.ShowDebug = !DynamicMeshManager.ShowDebug;
				break;
			case "stop":
				DynamicMeshThread.PrimaryQueue.Clear();
				DynamicMeshThread.SecondaryQueue.Clear();
				break;
			case "reload":
				DynamicMeshManager.Parent.AddComponent<DynamicMeshManager>();
				break;
			case "clear":
				DynamicMeshManager.Instance.ClearPrefabs();
				break;
			case "max":
				DynamicMeshSettings.MaxViewDistance = int.Parse(GetParam(_params, 1));
				PrefabLODManager.lodPoiDistance = DynamicMeshSettings.MaxViewDistance;
				Log.Out("Max Dynamic Mesh: " + DynamicMeshSettings.MaxViewDistance);
				break;
			case "autosend":
				DynamicMeshServer.AutoSend = !DynamicMeshServer.AutoSend;
				Log.Out("Autosend: " + DynamicMeshServer.AutoSend);
				break;
			case "all":
			{
				Constants.cDigAndBuildDistance = 50f;
				Constants.cBuildIntervall = 0.2f;
				Constants.cCollectItemDistance = 50f;
				AddKit();
				GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, _value: true);
				GameManager.Instance.World.GetPrimaryPlayer().GodModeSpeedModifier = 15f;
				float start2 = float.MinValue;
				float end2 = float.MinValue;
				SkyManager.SetFogDebug(0.06f, start2, end2);
				DynamicMeshManager.ShowGui = true;
				break;
			}
			case "down":
			{
				Vector3 position6 = GameManager.Instance.World.GetPrimaryPlayer().position;
				if (_params.Count == 3)
				{
					position6.x = int.Parse(GetParam(_params, 1));
					position6.z = int.Parse(GetParam(_params, 2));
				}
				Vector3i regionPositionFromWorldPosition2 = DynamicMeshUnity.GetRegionPositionFromWorldPosition(position6);
				{
					foreach (DynamicMeshRegion value6 in DynamicMeshRegion.Regions.Values)
					{
						if (value6.WorldPosition.x == regionPositionFromWorldPosition2.x && value6.WorldPosition.z == regionPositionFromWorldPosition2.z && value6.RegionObject != null)
						{
							value6.RegionObject.transform.position += new Vector3(0f, -3f, 0f);
						}
					}
					break;
				}
			}
			case "previewclear":
				ChunkPreviewManager.Instance?.ClearAll();
				break;
			case "up":
			{
				Vector3 position4 = GameManager.Instance.World.GetPrimaryPlayer().position;
				if (_params.Count == 3)
				{
					position4.x = int.Parse(GetParam(_params, 1));
					position4.z = int.Parse(GetParam(_params, 2));
				}
				DynamicMeshUnity.GetRegionPositionFromWorldPosition(position4);
				foreach (DynamicMeshRegion value7 in DynamicMeshRegion.Regions.Values)
				{
					if (value7.RegionObject != null)
					{
						value7.RegionObject.transform.position += new Vector3(0f, 6f, 0f);
					}
				}
				Log.Out("Regions up");
				break;
			}
			case "upp":
			{
				Vector3 position3 = GameManager.Instance.World.GetPrimaryPlayer().position;
				if (_params.Count == 3)
				{
					position3.x = int.Parse(GetParam(_params, 1));
					position3.z = int.Parse(GetParam(_params, 2));
				}
				DynamicMeshUnity.GetRegionPositionFromWorldPosition(position3);
				foreach (KeyValuePair<long, DynamicMeshItem> item11 in DynamicMeshManager.Instance.ItemsDictionary)
				{
					if (item11.Value.ChunkObject != null)
					{
						item11.Value.ChunkObject.transform.position += new Vector3(0f, 10f, 0f);
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg(item11.Value.ToDebugLocation() + " chunk new pos: " + item11.Value.ChunkObject.transform.position.ToString());
						}
					}
				}
				Log.Out("Items up");
				break;
			}
			case "downn":
			{
				Vector3 position2 = GameManager.Instance.World.GetPrimaryPlayer().position;
				if (_params.Count == 3)
				{
					position2.x = int.Parse(GetParam(_params, 1));
					position2.z = int.Parse(GetParam(_params, 2));
				}
				DynamicMeshUnity.GetRegionPositionFromWorldPosition(position2);
				{
					foreach (KeyValuePair<long, DynamicMeshItem> item12 in DynamicMeshManager.Instance.ItemsDictionary)
					{
						if (item12.Value.ChunkObject != null)
						{
							item12.Value.ChunkObject.transform.position -= new Vector3(0f, 10f, 0f);
							if (DynamicMeshManager.DoLog)
							{
								DynamicMeshManager.LogMsg(item12.Value.ToDebugLocation() + " chunk new pos: " + item12.Value.ChunkObject.transform.position.ToString());
							}
						}
					}
					break;
				}
			}
			case "gui":
				DynamicMeshManager.ShowGui = !DynamicMeshManager.ShowGui;
				break;
			case "white":
				DynamicMeshManager.DebugStyle.normal.textColor = ((DynamicMeshManager.DebugStyle.normal.textColor == Color.magenta) ? Color.white : Color.magenta);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddKit()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("keystoneBlock").type), 1);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("drinkJarBoiledWater").type), 1);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("meleeToolTorch").type), 1);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("foodCanChili").type), 1);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("medicalFirstAidBandage").type), 1);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("noteDuke01").type), 1);
		ItemValue itemValue = new ItemValue(ItemClass.GetItem("gunExplosivesT3RocketLauncher").type, 6, 6, _bCreateDefaultModItems: true, null, 99f);
		if (primaryPlayer.inventory.GetItemCount(itemValue) == 0)
		{
			if (!primaryPlayer.inventory.AddItem(new ItemStack(itemValue, 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunExplosivesT3RocketLauncher").type, 6, 6, _bCreateDefaultModItems: true, new string[1] { "" }, 99f), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunRifleT3SniperRifle").type, 6, 6, _bCreateDefaultModItems: true, new string[1] { "modGunScopeLarge" }), 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunRifleT3SniperRifle").type, 6, 6, _bCreateDefaultModItems: true, new string[1] { "modGunScopeLarge" }), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunToolDiggerAdmin").type, 6, 6, _bCreateDefaultModItems: true), 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunToolDiggerAdmin").type, 6, 6, _bCreateDefaultModItems: true), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("concreteBlock").type), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("concreteBlock").type), 5000));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDirt").type), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDirt").type), 5000));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDestroyedStone").type), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDestroyedStone").type), 5000));
			}
			primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("ammoRocketHE").type), 2000));
			primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("ammo762mmBulletBall").type), 2000));
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
		return new string[2] { info, "zz" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return info;
	}

	public static void DebugAll()
	{
		DynamicMeshThread.BuilderManager.SetNewLimit(Application.isEditor ? 8 : 16);
		DynamicMeshThread.ChunkDataQueue.MaxAllowedItems = 11111;
		Constants.cDigAndBuildDistance = 50f;
		Constants.cBuildIntervall = 0.2f;
		Constants.cCollectItemDistance = 50f;
		AddKit();
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, _value: true);
		GameManager.Instance.World.GetPrimaryPlayer().GodModeSpeedModifier = 15f;
		float start = float.MinValue;
		float end = float.MinValue;
		SkyManager.SetFogDebug(0.06f, start, end);
		DynamicMeshManager.ShowGui = true;
	}
}
