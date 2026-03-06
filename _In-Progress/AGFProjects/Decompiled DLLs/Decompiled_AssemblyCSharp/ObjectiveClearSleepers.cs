using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveClearSleepers : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum GotoStates
	{
		NoPosition,
		TryRefresh,
		TryComplete,
		Completed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Private)]
	public string locationVariable = "gotolocation";

	public Dictionary<Vector3, MapObjectSleeperVolume> SleeperMapObjectList = new Dictionary<Vector3, MapObjectSleeperVolume>();

	public Dictionary<Vector3, NavObject> SleeperNavObjectList = new Dictionary<Vector3, NavObject>();

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

	public override bool RequiresZombies => true;

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.clearTag);
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveClearAreas_keyword");
		SetupIcon();
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
		StatusText = "";
	}

	public override void AddHooks()
	{
		GetPosition();
		Vector3 pos = Vector3.zero;
		Vector3 pos2 = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		base.OwnerQuest.GetPositionData(out pos2, Quest.PositionDataTypes.POISize);
		QuestEventManager.Current.SleepersCleared += Current_SleepersCleared;
		QuestEventManager.Current.SleeperVolumePositionAdd += Current_SleeperVolumePositionAdd;
		QuestEventManager.Current.SleeperVolumePositionRemove += Current_SleeperVolumePositionRemove;
		QuestEventManager.Current.SubscribeToUpdateEvent(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, pos);
		SetupZombieCompassBounds(pos, pos2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleeperVolumePositionAdd(Vector3 position)
	{
		if (NavObjectName == "")
		{
			if (!SleeperMapObjectList.ContainsKey(position))
			{
				MapObjectSleeperVolume mapObjectSleeperVolume = new MapObjectSleeperVolume(position);
				GameManager.Instance.World.ObjectOnMapAdd(mapObjectSleeperVolume);
				SleeperMapObjectList.Add(position, mapObjectSleeperVolume);
			}
		}
		else if (!SleeperNavObjectList.ContainsKey(position))
		{
			NavObject value = NavObjectManager.Instance.RegisterNavObject(NavObjectName, position);
			SleeperNavObjectList.Add(position, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleeperVolumePositionRemove(Vector3 position)
	{
		if (NavObjectName == "")
		{
			if (SleeperMapObjectList.ContainsKey(position))
			{
				MapObject mapObject = SleeperMapObjectList[position];
				GameManager.Instance.World.ObjectOnMapRemove(mapObject.type, (int)mapObject.key);
				SleeperMapObjectList.Remove(position);
			}
		}
		else if (SleeperNavObjectList.ContainsKey(position))
		{
			NavObject navObject = SleeperNavObjectList[position];
			NavObjectManager.Instance.UnRegisterNavObject(navObject);
			SleeperNavObjectList.Remove(position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveSleeperVolumeMapObjects()
	{
		if (NavObjectName == "")
		{
			GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SleeperVolume);
			SleeperMapObjectList.Clear();
		}
		else
		{
			NavObjectManager.Instance.UnRegisterNavObjectByClass(NavObjectName);
			SleeperNavObjectList.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupZombieCompassBounds(Vector3 poiPos, Vector3 poiSize)
	{
		base.OwnerQuest.OwnerJournal.OwnerPlayer.ZombieCompassBounds = new Rect(poiPos.x, poiPos.z, poiSize.x, poiSize.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleepersCleared(Vector3 prefabPos)
	{
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		if (pos.x == prefabPos.x && pos.z == prefabPos.z && base.OwnerQuest.CheckRequirements())
		{
			base.Complete = true;
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.SleepersCleared -= Current_SleepersCleared;
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		QuestEventManager.Current.UnSubscribeToUpdateEvent(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, pos);
		if (base.OwnerQuest.OwnerJournal.ActiveQuest == base.OwnerQuest)
		{
			base.OwnerQuest.OwnerJournal.OwnerPlayer.ZombieCompassBounds = default(Rect);
		}
		RemoveSleeperVolumeMapObjects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupIcon()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.Position = position;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(float offset, float x, float y, float z)
	{
		distanceOffset = offset;
		position = new Vector3(x, y, z);
		base.OwnerQuest.DataVariables.Add(locationVariable, $"{offset.ToCultureInvariantString()},{x.ToCultureInvariantString()},{y.ToCultureInvariantString()},{z.ToCultureInvariantString()}");
		base.OwnerQuest.Position = position;
		base.CurrentValue = 1;
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveClearSleepers objectiveClearSleepers = new ObjectiveClearSleepers();
		CopyValues(objectiveClearSleepers);
		return objectiveClearSleepers;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		FinalizePoint(distanceOffset, pos.x, pos.y, pos.z);
		return true;
	}
}
