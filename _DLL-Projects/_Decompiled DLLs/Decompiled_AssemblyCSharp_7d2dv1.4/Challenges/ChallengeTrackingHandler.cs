using System.Collections.Generic;
using UnityEngine;

namespace Challenges;

public class ChallengeTrackingHandler
{
	public Challenge Owner;

	public EntityPlayerLocal LocalPlayer;

	public List<TrackingEntry> trackingEntries = new List<TrackingEntry>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 LastCheckedPosition = new Vector3(0f, 9999f, 0f);

	public float RefreshDistance = 5f;

	public bool NeedsRefresh;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastInTrader;

	public bool Update(float deltaTime)
	{
		if (LocalPlayer == null)
		{
			return true;
		}
		if (Owner == null || !Owner.IsActive)
		{
			return false;
		}
		if (LocalPlayer.IsInTrader != lastInTrader)
		{
			lastInTrader = LocalPlayer.IsInTrader;
			NeedsRefresh = true;
		}
		if (Vector3.Distance(LastCheckedPosition, LocalPlayer.position) > RefreshDistance || NeedsRefresh)
		{
			LastCheckedPosition = LocalPlayer.position;
			HandleTracking();
			NeedsRefresh = false;
		}
		return true;
	}

	public void AddTrackingEntry(TrackingEntry track)
	{
		if (!trackingEntries.Contains(track))
		{
			trackingEntries.Add(track);
		}
		QuestEventManager.Current.AddTrackerToBeUpdated(this);
		NeedsRefresh = true;
	}

	public void RemoveTrackingEntry(TrackingEntry track)
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
		if (!LocalPlayer.IsInTrader)
		{
			foreach (Chunk item in chunkArrayCopySync)
			{
				for (int j = 0; j < trackingEntries.Count; j++)
				{
					trackingEntries[j].HandleTrack(item);
				}
			}
		}
		for (int k = 0; k < trackingEntries.Count; k++)
		{
			trackingEntries[k].EndUpdate();
		}
	}
}
