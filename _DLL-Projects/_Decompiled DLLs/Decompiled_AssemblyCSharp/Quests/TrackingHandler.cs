using System.Collections.Generic;
using UnityEngine;

namespace Quests;

public class TrackingHandler
{
	public int QuestCode;

	public EntityPlayerLocal LocalPlayer;

	public List<ObjectiveModifierTrackBlocks> trackingEntries = new List<ObjectiveModifierTrackBlocks>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 LastCheckedPosition = new Vector3(0f, 9999f, 0f);

	public float RefreshDistance = 5f;

	public bool NeedsRefresh;

	public bool Update(float deltaTime)
	{
		if (LocalPlayer == null)
		{
			return true;
		}
		Quest quest = LocalPlayer.QuestJournal.FindActiveQuest(QuestCode);
		if (quest == null || quest.OwnerJournal == null || quest.OwnerJournal.OwnerPlayer == null)
		{
			return false;
		}
		if (Vector3.Distance(LastCheckedPosition, LocalPlayer.position) > RefreshDistance || NeedsRefresh)
		{
			LastCheckedPosition = LocalPlayer.position;
			HandleTracking();
			NeedsRefresh = false;
		}
		return true;
	}

	public void AddTrackingEntry(ObjectiveModifierTrackBlocks track)
	{
		if (!trackingEntries.Contains(track))
		{
			trackingEntries.Add(track);
			NeedsRefresh = true;
		}
		QuestEventManager.Current.AddTrackerToBeUpdated(this);
	}

	public void RemoveTrackingEntry(ObjectiveModifierTrackBlocks track)
	{
		if (trackingEntries.Contains(track))
		{
			trackingEntries.Remove(track);
			NeedsRefresh = true;
		}
		if (trackingEntries.Count == 0)
		{
			QuestEventManager.Current.RemoveTrackerToBeUpdated(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTracking()
	{
		_ = NavObjectManager.Instance;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < trackingEntries.Count; i++)
		{
			trackingEntries[i].StartUpdate();
		}
		foreach (Chunk item in chunkArrayCopySync)
		{
			for (int j = 0; j < trackingEntries.Count; j++)
			{
				trackingEntries[j].HandleTrack(item);
			}
		}
		for (int k = 0; k < trackingEntries.Count; k++)
		{
			trackingEntries[k].EndUpdate();
		}
	}
}
