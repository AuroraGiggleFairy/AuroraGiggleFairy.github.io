using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class PrefabTriggerData
{
	public Dictionary<int, List<BlockTrigger>> TriggeredByDictionary = new Dictionary<int, List<BlockTrigger>>();

	public Dictionary<int, List<SleeperVolume>> TriggeredByVolumes = new Dictionary<int, List<SleeperVolume>>();

	public PrefabInstance PrefabInstance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	public List<int> PlayersInArea = new List<int>();

	public List<byte> TriggeredLayers;

	public List<byte> TriggeredByLayers;

	public List<BlockTrigger> Triggers = new List<BlockTrigger>();

	public TriggerManager Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public float needsTriggerTimer = -1f;

	public bool NeedsTriggerUpdate
	{
		get
		{
			return needsTriggerTimer != -1f;
		}
		set
		{
			if (Owner == null)
			{
				Owner = world.triggerManager;
			}
			if (value)
			{
				Owner.AddToUpdateList(this);
				needsTriggerTimer = 3f;
			}
			else
			{
				Owner.RemoveFromUpdateList(this);
				needsTriggerTimer = -1f;
			}
		}
	}

	public PrefabTriggerData(PrefabInstance instance)
	{
		PrefabInstance = instance;
		world = GameManager.Instance.World;
		SetupData();
	}

	public void ResetData()
	{
		if (TriggeredLayers != null)
		{
			TriggeredLayers.Clear();
		}
		if (TriggeredByLayers != null)
		{
			TriggeredByLayers.Clear();
		}
		TriggeredByDictionary.Clear();
		TriggeredByVolumes.Clear();
		Triggers.Clear();
		SetupData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupData()
	{
		bool flag = GameManager.Instance.World.IsEditor();
		HashSetLong occupiedChunks = PrefabInstance.GetOccupiedChunks();
		Vector3i boundingBoxSize = PrefabInstance.boundingBoxSize;
		Vector3i boundingBoxPosition = PrefabInstance.boundingBoxPosition;
		foreach (long item2 in occupiedChunks)
		{
			Chunk chunkSync = world.ChunkCache.GetChunkSync(item2);
			if (chunkSync == null)
			{
				continue;
			}
			foreach (BlockTrigger item3 in chunkSync.GetBlockTriggers().list)
			{
				Vector3i vector3i = item3.ToWorldPos();
				if (boundingBoxPosition.x > vector3i.x || boundingBoxPosition.y > vector3i.y || boundingBoxPosition.z > vector3i.z || boundingBoxPosition.x + boundingBoxSize.x <= vector3i.x || boundingBoxPosition.y + boundingBoxSize.y <= vector3i.y || boundingBoxPosition.z + boundingBoxSize.z <= vector3i.z)
				{
					continue;
				}
				foreach (byte triggeredByIndex in item3.TriggeredByIndices)
				{
					if (!TriggeredByDictionary.TryGetValue(triggeredByIndex, out var value))
					{
						value = new List<BlockTrigger>();
						TriggeredByDictionary[triggeredByIndex] = value;
					}
					value.Add(item3);
					if (flag)
					{
						if (TriggeredByLayers == null)
						{
							TriggeredByLayers = new List<byte>();
						}
						if (!TriggeredByLayers.Contains(triggeredByIndex))
						{
							TriggeredByLayers.Add(triggeredByIndex);
						}
					}
				}
				foreach (byte triggersIndex in item3.TriggersIndices)
				{
					if (TriggeredLayers == null)
					{
						TriggeredLayers = new List<byte>();
					}
					if (!TriggeredLayers.Contains(triggersIndex))
					{
						TriggeredLayers.Add(triggersIndex);
					}
				}
				Triggers.Add(item3);
				item3.TriggerDataOwner = this;
			}
		}
		List<SleeperVolume> sleeperVolumes = PrefabInstance.sleeperVolumes;
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			SleeperVolume sleeperVolume = sleeperVolumes[i];
			for (int j = 0; j < sleeperVolume.TriggeredByIndices.Count; j++)
			{
				AddTriggeredBy(sleeperVolume);
				if (flag)
				{
					if (TriggeredByLayers == null)
					{
						TriggeredByLayers = new List<byte>();
					}
					byte item = sleeperVolume.TriggeredByIndices[j];
					if (!TriggeredByLayers.Contains(item))
					{
						TriggeredByLayers.Add(item);
					}
				}
			}
		}
		RefreshTriggers();
	}

	public void Update(float deltaTime)
	{
		if (needsTriggerTimer != -1f)
		{
			needsTriggerTimer -= deltaTime;
			if (needsTriggerTimer <= 0f)
			{
				HandleNeedTriggers();
				NeedsTriggerUpdate = false;
			}
		}
	}

	public void HandleNeedTriggers()
	{
		for (int i = 0; i < Triggers.Count; i++)
		{
			if (Triggers[i].NeedsTriggered == BlockTrigger.TriggeredStates.NeedsTriggered)
			{
				Trigger(null, Triggers[i]);
				Triggers[i].NeedsTriggered = BlockTrigger.TriggeredStates.HasTriggered;
			}
		}
	}

	public void RefreshTriggers()
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < Triggers.Count; i++)
			{
				Triggers[i].Refresh(FastTags<TagGroup.Global>.none);
			}
		}
	}

	public void RefreshTriggersForQuest(FastTags<TagGroup.Global> questTags)
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < Triggers.Count; i++)
			{
				Triggers[i].Refresh(questTags);
			}
		}
	}

	public void ResetTriggers()
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < Triggers.Count; i++)
			{
				Triggers[i].NeedsTriggered = BlockTrigger.TriggeredStates.NotTriggered;
			}
		}
	}

	public void AddPlayerInArea(int entityID)
	{
		if (!PlayersInArea.Contains(entityID))
		{
			PlayersInArea.Add(entityID);
		}
	}

	public void RemovePlayerInArea(int entityID)
	{
		if (PlayersInArea.Contains(entityID))
		{
			PlayersInArea.Remove(entityID);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddTriggeredBy(SleeperVolume triggeredVolume)
	{
		for (int i = 0; i < triggeredVolume.TriggeredByIndices.Count; i++)
		{
			byte key = triggeredVolume.TriggeredByIndices[i];
			if (!TriggeredByVolumes.ContainsKey(key))
			{
				TriggeredByVolumes.Add(key, new List<SleeperVolume>());
			}
			TriggeredByVolumes[key].Add(triggeredVolume);
		}
	}

	public void Trigger(EntityPlayer player, byte index)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		if (TriggeredByDictionary.TryGetValue(index, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].OnTriggered(player, world, index, list);
			}
		}
		if (TriggeredByVolumes.TryGetValue(index, out var value2))
		{
			foreach (SleeperVolume item in value2)
			{
				item.OnTriggered(player, world, index);
			}
		}
		if (list.Count > 0)
		{
			UpdateBlocks(list);
		}
	}

	public void Trigger(EntityPlayer player, BlockTrigger trigger)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		foreach (byte triggersIndex in trigger.TriggersIndices)
		{
			if (TriggeredByDictionary.TryGetValue(triggersIndex, out var value))
			{
				foreach (BlockTrigger item in value)
				{
					item.OnTriggered(player, world, triggersIndex, list, trigger);
				}
			}
			if (!(player != null) || !TriggeredByVolumes.ContainsKey(triggersIndex))
			{
				continue;
			}
			foreach (SleeperVolume item2 in TriggeredByVolumes[triggersIndex])
			{
				item2.OnTriggered(player, world, triggersIndex);
			}
		}
		if (list.Count > 0)
		{
			UpdateBlocks(list);
		}
	}

	public void Trigger(EntityPlayer player, TriggerVolume trigger)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			int num = trigger.TriggersIndices[i];
			if (TriggeredByDictionary.ContainsKey(num))
			{
				for (int j = 0; j < TriggeredByDictionary[num].Count; j++)
				{
					TriggeredByDictionary[num][j].OnTriggered(player, world, num, list);
				}
			}
			if (TriggeredByVolumes.ContainsKey(num))
			{
				for (int k = 0; k < TriggeredByVolumes[num].Count; k++)
				{
					TriggeredByVolumes[num][k].OnTriggered(player, world, num);
				}
			}
		}
		if (list.Count > 0)
		{
			UpdateBlocks(list);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		if (GameManager.Instance.World != null && blockChanges != null)
		{
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
		}
	}

	public void SetupTriggerTestNavObjects()
	{
		RemoveTriggerTestNavObjects();
		for (int i = 0; i < Triggers.Count; i++)
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject("editor_block_trigger", Triggers[i].ToWorldPos().ToVector3Center());
			navObject.name = Triggers[i].TriggerDisplay();
			navObject.OverrideColor = ((Triggers[i].TriggeredByIndices.Count > 0) ? Color.blue : Color.red);
		}
	}

	public void RemoveTriggerTestNavObjects()
	{
		for (int i = 0; i < Triggers.Count; i++)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(Triggers[i].ToWorldPos().ToVector3Center(), "editor_block_trigger");
		}
	}
}
