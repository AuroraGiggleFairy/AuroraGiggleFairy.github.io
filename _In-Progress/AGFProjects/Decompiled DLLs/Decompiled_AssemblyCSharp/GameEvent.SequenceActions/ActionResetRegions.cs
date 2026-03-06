using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionResetRegions : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ResetTypes
	{
		None,
		Full
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ResetTypes ResetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropResetType = "reset_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isComplete;

	public override ActionCompleteStates OnPerformAction()
	{
		GameManager.Instance.StartCoroutine(HandleReset());
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator HandleReset()
	{
		yield return new WaitForSeconds(1f);
		World world = GameManager.Instance.World;
		ChunkCluster cc = world.ChunkCache;
		HashSetLong hashSetLong = new HashSetLong();
		HashSetLong regeneratedChunks = new HashSetLong();
		ChunkProviderGenerateWorld chunkProvider = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
		if (ResetType != ResetTypes.Full)
		{
			yield break;
		}
		foreach (long item in chunkProvider.ResetAllChunks(ChunkProtectionLevel.None))
		{
			if (cc.ContainsChunkSync(item))
			{
				hashSetLong.Add(item);
			}
		}
		if (hashSetLong.Count <= 0)
		{
			yield break;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Regenerating {hashSetLong.Count} synced chunks.");
		foreach (long chunkKey in hashSetLong)
		{
			if (!chunkProvider.GenerateSingleChunk(cc, chunkKey, _forceRebuild: true))
			{
				yield return new WaitForEndOfFrame();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Region reset failed regenerating chunk at world XZ position: {WorldChunkCache.extractX(chunkKey) << 4}, {WorldChunkCache.extractZ(chunkKey) << 4}");
			}
			else
			{
				regeneratedChunks.Add(chunkKey);
			}
		}
		world.m_ChunkManager.ResendChunksToClients(regeneratedChunks);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Regeneration complete.");
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropResetType, ref ResetType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionResetRegions
		{
			ResetType = ResetType
		};
	}
}
