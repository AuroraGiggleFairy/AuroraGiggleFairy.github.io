using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChunkReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine unloadTimed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "chunkreset", "cr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "resets the specified chunks";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. chunkreset <x1> <z1> <x2> <z2>\n  2. chunkreset [f]\n1. Rebuilds the chunks that contain the given coordinate range.\n2. Can only be executed by a player in the ingame console! Behaviour depends on whether the\n   player is currently within the bounds of a POI:\n   Within a POI: The POI is reset.\n   Not within a POI: The chunk the player is in and the eight chunks around that one are\n     rebuilt. Not deco! Does not reload POI data!\n   d - regen deco\n   f - fully regenerates chunks (may cause double entities!)\n   u, utimed, ue - Unload chunks or entities\n   nq - Enqueue a 3x3 group of chunks centred around the player to be reset when unsynced, unless they are otherwise protected from the chunk reset system.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.StartCoroutine(execute(_params, _senderInfo));
	}

	public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		if (_params.Count >= 2)
		{
			if (!int.TryParse(_params[0], out var x1))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x1 is not a valid integer");
				yield break;
			}
			if (!int.TryParse(_params[1], out var z1))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z1 is not a valid integer");
				yield break;
			}
			int x2 = x1;
			int z2 = z1;
			if (_params.Count >= 3 && !int.TryParse(_params[2], out x2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x2 is not a valid integer");
				yield break;
			}
			if (_params.Count >= 4 && !int.TryParse(_params[3], out z2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z2 is not a valid integer");
				yield break;
			}
			Vector2i chunkMin = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
			Vector2i chunkMax = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);
			if (chunkMax.x - chunkMin.x > 16384 || chunkMax.y - chunkMin.y > 16384)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("area too big");
				yield break;
			}
			chunkMin = World.toChunkXZ(chunkMin);
			chunkMax = World.toChunkXZ(chunkMax);
			HashSetLong chunks = new HashSetLong();
			for (int i = chunkMin.x; i <= chunkMax.x; i++)
			{
				for (int j = chunkMin.y; j <= chunkMax.y; j++)
				{
					chunks.Add(WorldChunkCache.MakeChunkKey(i, j));
				}
			}
			ChunkCluster cc = world.ChunkCache;
			if (world.ChunkCache.ChunkProvider is ChunkProviderGenerateWorld chunkProvider)
			{
				yield return GameManager.Instance.ResetWindowsAndLocksByChunks(chunks);
				chunkProvider.RemoveChunks(chunks);
				foreach (long item in chunks)
				{
					if (!chunkProvider.GenerateSingleChunk(cc, item, _forceRebuild: true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Failed regenerating chunk at position {WorldChunkCache.extractX(item) << 4}/{WorldChunkCache.extractZ(item) << 4}");
					}
				}
				GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reset chunks covering area {x1}/{z1} to {x2}/{z2} (chunk coordinates {chunkMin} to {chunkMax}).");
				if (!(DynamicMeshManager.Instance != null))
				{
					yield break;
				}
				foreach (long item2 in chunks)
				{
					DynamicMeshManager.Instance.AddChunk(item2, addToThread: true, primary: true, null);
				}
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Can not reset chunks on this game");
			}
		}
		else if (_params.Count <= 1 && (_senderInfo.RemoteClientInfo != null || (_senderInfo.IsLocalGame && !GameManager.IsDedicatedServer)))
		{
			Vector3 pos = ((_senderInfo.RemoteClientInfo == null) ? world.GetLocalPlayers()[0].position : world.Players.dict[_senderInfo.RemoteClientInfo.entityId].position);
			int x1 = World.toChunkXZ((int)pos.x) - 1;
			int z1 = World.toChunkXZ((int)pos.z) - 1;
			int x2 = x1 + 2;
			int z2 = z1 + 2;
			HashSetLong chunks = new HashSetLong();
			for (int k = x1; k <= x2; k++)
			{
				for (int l = z1; l <= z2; l++)
				{
					chunks.Add(WorldChunkCache.MakeChunkKey(k, l));
				}
			}
			if (_params.Count == 1)
			{
				ChunkCluster cc = world.ChunkCache;
				if (!(cc.ChunkProvider is ChunkProviderGenerateWorld chunkProvider2))
				{
					yield break;
				}
				if (_params[0] == "nq")
				{
					{
						foreach (long item3 in chunks)
						{
							chunkProvider2.RequestChunkReset(item3);
						}
						yield break;
					}
				}
				if (_params[0] == "d")
				{
					yield return GameManager.Instance.ResetWindowsAndLocksByChunks(chunks);
					foreach (long item4 in chunks)
					{
						Chunk chunkSync = cc.GetChunkSync(item4);
						if (chunkSync != null)
						{
							chunkSync.NeedsLightDecoration = true;
							chunkSync.NeedsLightCalculation = true;
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate deco around player");
				}
				else if (_params[0] == "f")
				{
					yield return GameManager.Instance.ResetWindowsAndLocksByChunks(chunks);
					foreach (long item5 in chunks)
					{
						if (!chunkProvider2.GenerateSingleChunk(cc, item5, _forceRebuild: true))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Failed regenerating chunk at position {WorldChunkCache.extractX(item5) << 4}/{WorldChunkCache.extractZ(item5) << 4}");
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate chunks around player");
				}
				else if (_params[0] == "r")
				{
					foreach (long item6 in chunks)
					{
						Chunk chunkSync2 = cc.GetChunkSync(item6);
						if (chunkSync2 != null)
						{
							chunkSync2.NeedsRegeneration = true;
						}
					}
				}
				else if (_params[0] == "u")
				{
					foreach (long item7 in chunks)
					{
						world.m_ChunkManager.RemoveChunk(item7);
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload around player");
				}
				else if (_params[0] == "utimed")
				{
					if (unloadTimed != null)
					{
						GameManager.Instance.StopCoroutine(unloadTimed);
						unloadTimed = null;
					}
					else
					{
						unloadTimed = GameManager.Instance.StartCoroutine(UnloadTimed());
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload timed at player {0}", unloadTimed != null);
				}
				else if (_params[0] == "ue")
				{
					foreach (long item8 in chunks)
					{
						Chunk chunkSync3 = cc.GetChunkSync(item8);
						if (chunkSync3 == null)
						{
							continue;
						}
						for (int m = 0; m < chunkSync3.entityLists.Length; m++)
						{
							List<Entity> list = chunkSync3.entityLists[m];
							for (int num = list.Count - 1; num >= 0; num--)
							{
								Entity entity = list[num];
								if (!entity.bWillRespawn && (!(entity.AttachedMainEntity != null) || !entity.AttachedMainEntity.bWillRespawn))
								{
									world.unloadEntity(entity, EnumRemoveEntityReason.Unloaded);
									list.RemoveAt(num);
								}
							}
						}
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("UnloadEntities around player");
				}
			}
			else
			{
				DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
				List<PrefabInstance> prefabsFromWorldPosInside;
				if (dynamicPrefabDecorator != null && (prefabsFromWorldPosInside = dynamicPrefabDecorator.GetPrefabsFromWorldPosInside(pos, FastTags<TagGroup.Global>.none)) != null)
				{
					yield return world.ResetPOIS(prefabsFromWorldPosInside, QuestEventManager.manualResetTag, -1, null, null);
				}
				else
				{
					yield return GameManager.Instance.ResetWindowsAndLocksByChunks(chunks);
					world.RebuildTerrain(chunks, Vector3i.zero, Vector3i.zero, _bStopStabilityUpdate: false, _bRegenerateChunk: true, _bFillEmptyBlocks: true);
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Reset chunks around player");
				}
			}
			if (!(DynamicMeshManager.Instance != null))
			{
				yield break;
			}
			foreach (long item9 in chunks)
			{
				DynamicMeshManager.Instance.AddChunk(item9, addToThread: true, primary: true, null);
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments, please see command help.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UnloadTimed()
	{
		int n = 0;
		while (n < 99999)
		{
			World world = GameManager.Instance.World;
			if (world == null)
			{
				break;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (!primaryPlayer)
			{
				break;
			}
			Vector3i blockPosition = primaryPlayer.GetBlockPosition();
			for (int i = -16; i <= 16; i += 16)
			{
				for (int j = -16; j <= 16; j += 16)
				{
					int x = World.toChunkXZ(blockPosition.x + j);
					int y = World.toChunkXZ(blockPosition.z + i);
					long chunkKey = WorldChunkCache.MakeChunkKey(x, y);
					world.m_ChunkManager.RemoveChunk(chunkKey);
				}
			}
			yield return new WaitForSeconds(1.5f);
			int num = n + 1;
			n = num;
		}
		unloadTimed = null;
	}
}
