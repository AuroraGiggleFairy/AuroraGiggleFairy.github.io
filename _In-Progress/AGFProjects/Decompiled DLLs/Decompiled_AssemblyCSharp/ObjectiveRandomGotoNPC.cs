using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRandomGotoNPC : ObjectiveRandomGoto
{
	public override bool NeedsNPCSetPosition => true;

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return GetPosition(ownerNPC) != Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 GetPosition(EntityNPC ownerNPC)
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.Location))
		{
			base.OwnerQuest.Position = position;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.TreasurePoint))
		{
			positionSet = true;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, base.OwnerQuest.Position);
			base.CurrentValue = 2;
			return position;
		}
		float _result = 50f;
		float num = 50f;
		float num2 = 50f;
		if (Value != null && Value != "")
		{
			if (StringParsers.TryParseFloat(Value, out _result))
			{
				num2 = _result;
			}
			else if (Value.Contains("-"))
			{
				string[] array = Value.Split('-');
				num = StringParsers.ParseFloat(array[0]);
				num2 = StringParsers.ParseFloat(array[1]);
				_result = GameManager.Instance.World.GetGameRandom().RandomFloat * (num2 - num) + num;
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Vector3i vector3i = ObjectiveRandomGoto.CalculateRandomPoint(ownerNPC.entityId, _result, base.OwnerQuest.ID, canBeWithinPOI: false, biomeFilterType, biomeFilter);
			if (!GameManager.Instance.World.CheckForLevelNearbyHeights(vector3i.x, vector3i.z, 5) || GameManager.Instance.World.GetWaterAt(vector3i.x, vector3i.z))
			{
				return Vector3.zero;
			}
			World world = GameManager.Instance.World;
			if (vector3i.y > 0 && world.IsPositionInBounds(vector3i) && !world.IsPositionWithinPOI(vector3i, 5))
			{
				FinalizePoint(vector3i.x, vector3i.y, vector3i.z);
				return position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(ownerNPC.entityId, _result, 1, base.OwnerQuest.QuestCode));
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public override BaseObjective Clone()
	{
		ObjectiveRandomGotoNPC objectiveRandomGotoNPC = new ObjectiveRandomGotoNPC();
		CopyValues(objectiveRandomGotoNPC);
		objectiveRandomGotoNPC.position = position;
		objectiveRandomGotoNPC.positionSet = positionSet;
		objectiveRandomGotoNPC.completionDistance = completionDistance;
		objectiveRandomGotoNPC.biomeFilter = biomeFilter;
		objectiveRandomGotoNPC.biomeFilterType = biomeFilterType;
		return objectiveRandomGotoNPC;
	}
}
