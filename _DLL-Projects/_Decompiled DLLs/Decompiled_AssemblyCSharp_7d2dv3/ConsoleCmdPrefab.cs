using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPrefab : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly MicroStopwatch processedSW = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int processedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int stopCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PathAbstractions.AbstractedLocation> prefabsToConvert = new List<PathAbstractions.AbstractedLocation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PathAbstractions.AbstractedLocation> prefabsToMerge = new List<PathAbstractions.AbstractedLocation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PathAbstractions.AbstractedLocation> prefabsToThumbnail = new List<PathAbstractions.AbstractedLocation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PrefabStatsFilename = GameIO.GetDeviceLocalUserGameDataDir() + "/_prefabstats.csv";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "prefab" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Prefab commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return ("\r\n\t\t\t|Usage:\r\n\t\t\t|  1. " + PrimaryCommand + " load [prefab name]\r\n            |      Load given prefab. If no prefab is given show prefab explorer window.\r\n\t\t\t|  2. " + PrimaryCommand + " save\r\n            |      Save current prefab.\r\n\t\t\t|  3. " + PrimaryCommand + " clear\r\n            |      Clear prefab editor contents.\r\n\t\t\t|  4. " + PrimaryCommand + " thumbnail [name]\r\n            |      Update prefab thumbnail of given prefab. If no prefab is given applies to the loaded prefab.\r\n\t\t\t|  5. " + PrimaryCommand + " thumbnail bulk\r\n            |      Create thumbnails for all prefabs that do not have one yet.\r\n\t\t\t|  6. " + PrimaryCommand + " stats\r\n            |      Create CSV file with basic stats of all prefabs.\r\n\t\t\t|  7. " + PrimaryCommand + " convert <name>\r\n            |      Load, simplify, combine and export named prefab.\r\n\t\t\t|  8. " + PrimaryCommand + " bulk [count]\r\n            |      Do \"convert\" on all prefabs (optionally limited to first \"count\" prefabs).\r\n\t\t\t|  9. " + PrimaryCommand + " bulkins\r\n            |      Update inside information (.ins) for all prefabs.\r\n\t\t\t|  10. " + PrimaryCommand + " density <needle> <set>\r\n            |      Change density of all non-air blocks in current pefab that have <needle> density to <set>.\r\n\t\t\t|  11. " + PrimaryCommand + " simplify\r\n            |      Simplify current prefab for imposter generation.\r\n\t\t\t|  12. " + PrimaryCommand + " simplify1\r\n            |      Simplify current prefab for imposter generation, keep more complex blocks.\r\n\t\t\t|  13. " + PrimaryCommand + " combine\r\n            |      Combine meshes of current prefab into one.\r\n\t\t\t|  14. " + PrimaryCommand + " export\r\n            |      Export combined meshes as imposter for current prefab.\r\n\t\t\t").Unindent();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!GameManager.Instance.IsEditMode())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command has to be run while in Prefab Editor!");
			return;
		}
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		switch (_params[0])
		{
		case "load":
			if (_params.Count < 2)
			{
				LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetLocalPlayers()[0]).windowManager.Open(XUiC_PrefabList.ID, _bModal: true);
			}
			else
			{
				PrefabEditModeManager.Instance.LoadVoxelPrefab(PathAbstractions.PrefabsSearchPaths.GetLocation(_params[1]));
			}
			break;
		case "save":
			if (PrefabEditModeManager.Instance.VoxelPrefab == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No prefab loaded");
				break;
			}
			PrefabEditModeManager.Instance.SaveVoxelPrefab();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Saved prefab {PrefabEditModeManager.Instance.VoxelPrefab.location} with size {PrefabEditModeManager.Instance.VoxelPrefab.size}");
			break;
		case "simplify":
			if (PrefabEditModeManager.Instance.VoxelPrefab == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No prefab loaded");
			}
			else
			{
				PrefabHelpers.SimplifyPrefab();
			}
			break;
		case "simplify1":
			if (PrefabEditModeManager.Instance.VoxelPrefab == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No prefab loaded");
			}
			else
			{
				PrefabHelpers.SimplifyPrefab(_bOnlySimplify1: true);
			}
			break;
		case "combine":
			if (PrefabEditModeManager.Instance.VoxelPrefab == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No prefab loaded");
				break;
			}
			PrefabHelpers.combine(_bCombineSliced: true);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Combined");
			break;
		case "export":
			if (PrefabEditModeManager.Instance.VoxelPrefab == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No prefab loaded");
				break;
			}
			PrefabHelpers.export();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Exported");
			break;
		case "convert":
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Please specify prefab to load");
				break;
			}
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PathAbstractions.PrefabsSearchPaths.GetLocation(_params[1]));
			PrefabHelpers.convert(PrefabHelpers.Cleanup);
			break;
		case "clear":
		{
			PrefabEditModeManager.Instance.ClearImposterPrefab();
			ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
			foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
			{
				GameManager.Instance.World.m_ChunkManager.RemoveChunk(item.Key);
			}
			chunkCache.Clear();
			break;
		}
		case "bulk":
			prefabsToConvert.Clear();
			prefabsToConvert.AddRange(PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList());
			processedCount = 0;
			processedSW.ResetAndRestart();
			stopCount = int.MaxValue;
			if (_params.Count >= 2)
			{
				stopCount = int.Parse(_params[1]);
			}
			convertBulk();
			break;
		case "bulkins":
			prefabsToConvert.Clear();
			prefabsToConvert.AddRange(PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList());
			convertBulkInsideOutside();
			break;
		case "density":
		{
			if (_params.Count < 3)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Two arguments expected: Original density and replace value");
				break;
			}
			int densityMatch = int.Parse(_params[1]);
			int densitySet = int.Parse(_params[2]);
			PrefabHelpers.DensityChange(densityMatch, densitySet);
			break;
		}
		case "thumbnail":
			prefabsToThumbnail.Clear();
			if (_params.Count == 2 && _params[1] == "bulk")
			{
				foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
				{
					if (!SdFile.Exists(availablePaths.FullPathNoExtension + ".jpg"))
					{
						prefabsToThumbnail.Add(availablePaths);
					}
				}
				thumbnailBulk();
			}
			else if (_params.Count == 2)
			{
				if (PrefabEditModeManager.Instance.VoxelPrefab == null || PrefabEditModeManager.Instance.VoxelPrefab.PrefabName != _params[1])
				{
					PrefabEditModeManager.Instance.LoadVoxelPrefab(PathAbstractions.PrefabsSearchPaths.GetLocation(_params[1]), _bBulk: true, _bIgnoreExcludeImposterCheck: true);
					if (PrefabEditModeManager.Instance.VoxelPrefab != null)
					{
						ThreadManager.StartCoroutine(thumbnailWaitForAllChunksBuilt(PrefabEditModeManager.Instance.VoxelPrefab.location));
					}
				}
			}
			else if (PrefabEditModeManager.Instance.VoxelPrefab != null)
			{
				ThreadManager.StartCoroutine(thumbnailWaitForAllChunksBuilt(PrefabEditModeManager.Instance.VoxelPrefab.location, 3f));
			}
			break;
		case "stats":
			SdFile.WriteAllText(PrefabStatsFilename, "Prefab,TotalVerts,TotalTris,LightsVolumePortion,SizeX,SizeY,SizeZ,Volume,LightsVolume\n", Encoding.UTF8);
			PrefabHelpers.IteratePrefabs(_ignoreExcludeImposterCheck: true, null, getPrefabStats, null, null, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Log.Out("Prefab stats written to: " + PrefabStatsFilename);
			});
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void getPrefabStats(PathAbstractions.AbstractedLocation _path, Prefab _prefab)
	{
		WorldStats worldStats = (_prefab.RenderingCostStats = WorldStats.CaptureWorldStats());
		_prefab.SaveXMLData(_path);
		SdFile.AppendAllText(PrefabStatsFilename, $"{_path.Name},{worldStats.TotalVertices},{worldStats.TotalTriangles},{worldStats.LightsVolume / (float)_prefab.size.Volume()},{_prefab.size.x},{_prefab.size.y},{_prefab.size.z},{_prefab.size.Volume()},{worldStats.LightsVolume}\n");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void thumbnailBulk()
	{
		if (prefabsToThumbnail.Count == 0)
		{
			return;
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.AbstractedLocation.None;
		while (prefabsToThumbnail.Count != 0)
		{
			location = prefabsToThumbnail[0];
			prefabsToThumbnail.RemoveAt(0);
			if (PrefabEditModeManager.Instance.LoadVoxelPrefab(location, _bBulk: true, _bIgnoreExcludeImposterCheck: true))
			{
				break;
			}
		}
		GameManager.Instance.World.GetLocalPlayers()[0].SetPosition(new Vector3(0f, (float)PrefabEditModeManager.Instance.VoxelPrefab.size.y * 2f / 3f, -PrefabEditModeManager.Instance.VoxelPrefab.size.z));
		ThreadManager.StartCoroutine(thumbnailWaitForAllChunksBuilt(location));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator thumbnailWaitForAllChunksBuilt(PathAbstractions.AbstractedLocation _location, float _delay = 0f)
	{
		if (_delay > 0f)
		{
			yield return new WaitForSeconds(_delay);
		}
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c) && !c.IsEmpty())
			{
				while (c.NeedsRegeneration || c.NeedsCopying)
				{
					yield return new WaitForSeconds(1f);
				}
			}
		}
		GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.File, _location.FullPathNoExtension, 0.1f, _b4to3: true, 280, 210);
		if (prefabsToThumbnail.Count > 0)
		{
			thumbnailBulk();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void mergeBulk()
	{
		if (prefabsToMerge.Count == 0)
		{
			return;
		}
		while (prefabsToMerge.Count != 0)
		{
			PathAbstractions.AbstractedLocation location = prefabsToMerge[0];
			prefabsToMerge.RemoveAt(0);
			if (PrefabEditModeManager.Instance.LoadVoxelPrefab(location, _bBulk: true, _bIgnoreExcludeImposterCheck: true))
			{
				PrefabHelpers.mergePrefab(_bRebuildMesh: false);
				PrefabEditModeManager.Instance.SaveVoxelPrefab();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Saved prefab {PrefabEditModeManager.Instance.VoxelPrefab.location} with size {PrefabEditModeManager.Instance.VoxelPrefab.size}");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void convertBulk()
	{
		if (prefabsToConvert.Count == 0 || processedCount >= stopCount)
		{
			PrefabHelpers.Cleanup();
			Log.Out("-- Prefab bulk {0}, done in {1}! --", processedCount, (float)processedSW.ElapsedMilliseconds * 0.001f);
			return;
		}
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
		while (prefabsToConvert.Count != 0)
		{
			abstractedLocation = prefabsToConvert[0];
			prefabsToConvert.RemoveAt(0);
			if (PrefabEditModeManager.Instance.LoadVoxelPrefab(abstractedLocation, _bBulk: true))
			{
				break;
			}
		}
		processedCount++;
		Log.Out("Prefab #{0}, {1}", processedCount, abstractedLocation);
		PrefabHelpers.convert(convertBulk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void convertBulkInsideOutside()
	{
		if (prefabsToConvert.Count == 0)
		{
			PrefabHelpers.Cleanup();
			return;
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.AbstractedLocation.None;
		while (prefabsToConvert.Count != 0)
		{
			location = prefabsToConvert[0];
			prefabsToConvert.RemoveAt(0);
			if (PrefabEditModeManager.Instance.LoadVoxelPrefab(location, _bBulk: true) && !PrefabEditModeManager.Instance.VoxelPrefab.bExcludePOICulling)
			{
				break;
			}
		}
		Log.Out("Processing " + location);
		PrefabHelpers.convertInsideOutside(convertBulkInsideOutside);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void restore()
	{
		World world = GameManager.Instance.World;
		EntityPlayerLocal entityPlayerLocal = world.GetLocalPlayers()[0];
		Chunk chunkSync = world.ChunkCache.GetChunkSync(World.toChunkXZ(entityPlayerLocal.GetBlockPosition().x), World.toChunkXZ(entityPlayerLocal.GetBlockPosition().z));
		if (chunkSync != null)
		{
			chunkSync.RestoreCulledBlocks(world);
			chunkSync.NeedsRegeneration = true;
		}
	}
}
