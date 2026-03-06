using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class TrackingEntry
{
	public class TrackedBlock
	{
		public Vector3i WorldPos;

		public NavObject NavObject;

		public bool KeepAlive;

		public TrackedBlock(Vector3i worldPos, string NavObjectName)
		{
			WorldPos = worldPos;
			NavObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, WorldPos.ToVector3Center());
			KeepAlive = true;
		}
	}

	public float trackDistance = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<TrackedBlock> TrackedBlocks = new List<TrackedBlock>();

	public ItemClass TrackedItem;

	public BaseChallengeObjective Owner;

	public ChallengeTrackingHandler TrackingHelper;

	public string blockIndexName = "quest_wood";

	public string navObjectName = "quest_resource";

	public void AddHooks()
	{
		if (TrackingHelper != null)
		{
			TrackingHelper.AddTrackingEntry(this);
		}
		QuestEventManager.Current.BlockChange -= Current_BlockChange;
		QuestEventManager.Current.BlockChange += Current_BlockChange;
	}

	public void RemoveHooks()
	{
		if (TrackingHelper != null)
		{
			TrackingHelper.RemoveTrackingEntry(this);
		}
		QuestEventManager.Current.BlockChange -= Current_BlockChange;
		NavObjectManager instance = NavObjectManager.Instance;
		for (int num = TrackedBlocks.Count - 1; num >= 0; num--)
		{
			instance.UnRegisterNavObject(TrackedBlocks[num].NavObject);
			TrackedBlocks.RemoveAt(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockChange(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (!(blockOld.IndexName == blockIndexName))
		{
			return;
		}
		for (int i = 0; i < TrackedBlocks.Count; i++)
		{
			if (TrackedBlocks[i].WorldPos == blockPos)
			{
				NavObjectManager.Instance.UnRegisterNavObject(TrackedBlocks[i].NavObject);
				TrackedBlocks.RemoveAt(i);
				break;
			}
		}
	}

	public void StartUpdate()
	{
		if (localPlayer == null)
		{
			localPlayer = Owner.Owner.Owner.Player;
		}
		for (int i = 0; i < TrackedBlocks.Count; i++)
		{
			TrackedBlocks[i].KeepAlive = false;
		}
	}

	public void HandleTrack(Chunk c)
	{
		if (!c.IndexedBlocks.TryGetValue(blockIndexName, out var _value))
		{
			return;
		}
		foreach (Vector3i item in _value)
		{
			Vector3i vector3i = c.ToWorldPos(item);
			if (!c.GetBlock(item).ischild && Vector3.Distance(vector3i, localPlayer.position) < trackDistance)
			{
				HandleAddTrackedBlock(vector3i);
			}
		}
	}

	public void EndUpdate()
	{
		NavObjectManager instance = NavObjectManager.Instance;
		for (int num = TrackedBlocks.Count - 1; num >= 0; num--)
		{
			if (!TrackedBlocks[num].KeepAlive)
			{
				instance.UnRegisterNavObject(TrackedBlocks[num].NavObject);
				TrackedBlocks.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleAddTrackedBlock(Vector3i pos)
	{
		for (int i = 0; i < TrackedBlocks.Count; i++)
		{
			if (pos == TrackedBlocks[i].WorldPos)
			{
				TrackedBlocks[i].KeepAlive = true;
			}
		}
		TrackedBlocks.Add(new TrackedBlock(pos, navObjectName));
	}
}
