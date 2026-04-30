using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PrefabInstance, PrefabTriggerData> PrefabDataDict = new Dictionary<PrefabInstance, PrefabTriggerData>();

	public List<PrefabTriggerData> UpdateList = new List<PrefabTriggerData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showNavObjects;

	public bool ShowNavObjects
	{
		get
		{
			return showNavObjects;
		}
		set
		{
			showNavObjects = value;
			HandleNavObjects(showNavObjects);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleNavObjects(bool enabled)
	{
		foreach (PrefabTriggerData value in PrefabDataDict.Values)
		{
			if (enabled)
			{
				value.SetupTriggerTestNavObjects();
			}
			else
			{
				value.RemoveTriggerTestNavObjects();
			}
		}
	}

	public void AddPrefabData(PrefabInstance instance, int entityID)
	{
		PrefabTriggerData prefabTriggerData = null;
		if (!PrefabDataDict.ContainsKey(instance))
		{
			prefabTriggerData = new PrefabTriggerData(instance)
			{
				Owner = this
			};
			if (ShowNavObjects)
			{
				prefabTriggerData.SetupTriggerTestNavObjects();
			}
			PrefabDataDict.Add(instance, prefabTriggerData);
		}
		prefabTriggerData = PrefabDataDict[instance];
		prefabTriggerData.RefreshTriggers();
		prefabTriggerData.AddPlayerInArea(entityID);
	}

	public void RefreshTriggers(PrefabInstance instance, FastTags<TagGroup.Global> questTags)
	{
		PrefabTriggerData prefabTriggerData = null;
		if (!PrefabDataDict.ContainsKey(instance))
		{
			prefabTriggerData = new PrefabTriggerData(instance)
			{
				Owner = this
			};
			PrefabDataDict.Add(instance, prefabTriggerData);
		}
		else
		{
			prefabTriggerData = PrefabDataDict[instance];
			prefabTriggerData.RemoveTriggerTestNavObjects();
			prefabTriggerData.ResetData();
		}
		prefabTriggerData.ResetTriggers();
		prefabTriggerData.RefreshTriggersForQuest(questTags);
		prefabTriggerData.HandleNeedTriggers();
	}

	public void Trigger(EntityPlayer player, PrefabInstance instance, byte trigger)
	{
		if (PrefabDataDict.TryGetValue(instance, out var value))
		{
			value.Trigger(player, trigger);
		}
	}

	public void TriggerBlocks(EntityPlayer player, PrefabInstance instance, BlockTrigger trigger)
	{
		if (trigger.HasAnyTriggers() && PrefabDataDict.ContainsKey(instance))
		{
			PrefabDataDict[instance].Trigger(player, trigger);
		}
	}

	public void TriggerBlocks(EntityPlayer player, PrefabInstance instance, TriggerVolume trigger)
	{
		if (trigger.HasAnyTriggers() && PrefabDataDict.ContainsKey(instance))
		{
			PrefabDataDict[instance].Trigger(player, trigger);
		}
	}

	public void RemovePlayer(PrefabInstance instance, int entityID)
	{
		if (PrefabDataDict.ContainsKey(instance))
		{
			PrefabDataDict[instance].RemovePlayerInArea(entityID);
		}
	}

	public void RemovePrefabData(PrefabInstance instance)
	{
		if (PrefabDataDict.ContainsKey(instance))
		{
			PrefabDataDict[instance].RemoveTriggerTestNavObjects();
			PrefabDataDict.Remove(instance);
		}
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		for (int num = UpdateList.Count - 1; num >= 0; num--)
		{
			UpdateList[num].Update(deltaTime);
		}
	}

	public List<byte> GetTriggerLayers()
	{
		List<byte> list = new List<byte>();
		foreach (PrefabTriggerData value in PrefabDataDict.Values)
		{
			for (int i = 0; i < value.TriggeredLayers.Count; i++)
			{
				if (!list.Contains(value.TriggeredLayers[i]))
				{
					list.Add(value.TriggeredLayers[i]);
				}
			}
			for (int j = 0; j < value.TriggeredByLayers.Count; j++)
			{
				if (!list.Contains(value.TriggeredByLayers[j]))
				{
					list.Add(value.TriggeredByLayers[j]);
				}
			}
		}
		return list;
	}

	public void AddToUpdateList(PrefabTriggerData prefabTriggerData)
	{
		if (!UpdateList.Contains(prefabTriggerData))
		{
			UpdateList.Add(prefabTriggerData);
		}
	}

	public void RemoveFromUpdateList(PrefabTriggerData prefabTriggerData)
	{
		if (UpdateList.Contains(prefabTriggerData))
		{
			UpdateList.Remove(prefabTriggerData);
		}
	}

	public void RemoveFromUpdateList(PrefabInstance instance)
	{
		for (int num = UpdateList.Count - 1; num >= 0; num--)
		{
			if (UpdateList[num].PrefabInstance == instance)
			{
				UpdateList.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestGeneratorController getGeneratorController(Vector3i rootPos, Chunk chunk)
	{
		QuestGeneratorController result = null;
		BlockEntityData blockEntity = chunk.GetBlockEntity(rootPos);
		if (blockEntity != null && blockEntity.transform != null)
		{
			result = blockEntity.transform.GetComponent<QuestGeneratorController>();
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockSwitchController getSwitchController(Vector3i rootPos, Chunk chunk)
	{
		BlockSwitchController result = null;
		BlockEntityData blockEntity = chunk.GetBlockEntity(rootPos);
		if (blockEntity != null && blockEntity.transform != null)
		{
			result = blockEntity.transform.GetComponent<BlockSwitchController>();
		}
		return result;
	}

	public IEnumerator CheckPowerState(BlockTrigger _triggered, FastTags<TagGroup.Global> questTag, Action<bool> callback)
	{
		bool obj = false;
		if (_triggered.HasAnyTriggeredBy())
		{
			World world = GameManager.Instance.World;
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			Vector3i vector3i = _triggered.ToWorldPos();
			PrefabInstance prefabAtPosition = dynamicPrefabDecorator.GetPrefabAtPosition(vector3i);
			bool isRestorePower = questTag.Test_AnySet(QuestEventManager.restorePowerTag);
			if (prefabAtPosition != null && PrefabDataDict.TryGetValue(prefabAtPosition, out var pfd))
			{
				int triggerCount = 0;
				foreach (byte triggerIndex in _triggered.TriggeredByIndices)
				{
					bool indexTriggered = false;
					foreach (BlockTrigger trigger in pfd.Triggers)
					{
						if (!trigger.TriggersIndices.Contains(triggerIndex))
						{
							continue;
						}
						Vector3i rootPos = trigger.ToWorldPos();
						Chunk chunk;
						do
						{
							chunk = (Chunk)world.GetChunkFromWorldPos(rootPos);
							if (chunk == null)
							{
								yield return null;
							}
						}
						while (chunk == null);
						QuestGeneratorController generatorController = getGeneratorController(rootPos, chunk);
						if (generatorController != null)
						{
							if (!isRestorePower)
							{
								indexTriggered = true;
								continue;
							}
							indexTriggered = generatorController.IsRunning;
							if (!indexTriggered)
							{
								continue;
							}
							break;
						}
						BlockSwitchController switchController = getSwitchController(rootPos, chunk);
						if (switchController != null)
						{
							indexTriggered = switchController.Activated;
						}
						if (indexTriggered)
						{
							break;
						}
					}
					if (indexTriggered)
					{
						triggerCount++;
					}
				}
				obj = ((!_triggered.UseOrForMultipleTriggers) ? (triggerCount == _triggered.TriggeredByIndices.Count) : (triggerCount > 0));
			}
		}
		else
		{
			obj = true;
		}
		callback(obj);
	}
}
