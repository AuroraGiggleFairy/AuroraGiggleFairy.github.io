using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveModifierTrackBlocks : BaseObjectiveModifier
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockIndexName = "questTracked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string navObjectName = "quest_resource";

	public float trackDistance = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<TrackedBlock> TrackedBlocks = new List<TrackedBlock>();

	public static string PropBlockIndexName = "block_index_name";

	public static string PropNavObjectName = "nav_object";

	public static string PropTrackDistance = "track_distance";

	public override void AddHooks()
	{
		base.OwnerObjective.OwnerQuest.TrackingHelper.AddTrackingEntry(this);
		QuestEventManager.Current.BlockChange += Current_BlockChange;
	}

	public override void RemoveHooks()
	{
		base.OwnerObjective.OwnerQuest.TrackingHelper.RemoveTrackingEntry(this);
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
			localPlayer = base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer;
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
			if (!c.GetBlockNoDamage(item.x, item.y, item.z).ischild)
			{
				Vector3i vector3i = c.ToWorldPos(item);
				if (Vector3.Distance(vector3i, localPlayer.position) < trackDistance)
				{
					HandleAddTrackedBlock(vector3i);
				}
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

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBlockIndexName))
		{
			blockIndexName = properties.Values[PropBlockIndexName];
		}
		if (properties.Values.ContainsKey(PropNavObjectName))
		{
			navObjectName = properties.Values[PropNavObjectName];
		}
		if (properties.Values.ContainsKey(PropTrackDistance))
		{
			trackDistance = StringParsers.ParseFloat(properties.Values[PropTrackDistance]);
		}
	}

	public override BaseObjectiveModifier Clone()
	{
		return new ObjectiveModifierTrackBlocks
		{
			blockIndexName = blockIndexName,
			navObjectName = navObjectName,
			trackDistance = trackDistance
		};
	}
}
