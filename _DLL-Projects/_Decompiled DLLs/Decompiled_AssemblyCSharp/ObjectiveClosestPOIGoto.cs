using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveClosestPOIGoto : ObjectiveGoto
{
	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveRallyPointHeadTo");
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		icon = "ui_game_symbol_quest";
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = 1)
	{
		return GetPosition(ownerNPC, player, usedPOILocations) != Vector3.zero;
	}

	public override void SetPosition(Vector3 position, Vector3 size)
	{
		FinalizePoint(position, size);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		int traderId = ((ownerNPC == null) ? (-1) : ownerNPC.entityId);
		int playerId = ((entityPlayer == null) ? (-1) : entityPlayer.entityId);
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.Position = position;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		EntityAlive entityAlive = ((ownerNPC == null) ? ((EntityAlive)base.OwnerQuest.OwnerJournal.OwnerPlayer) : ((EntityAlive)ownerNPC));
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string questKey = "";
			if (base.OwnerQuest != null && base.OwnerQuest.QuestClass != null)
			{
				questKey = base.OwnerQuest.QuestClass.UniqueKey;
			}
			PrefabInstance closestPOIToWorldPos = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(base.OwnerQuest.QuestTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, ignoreCurrentPOI: false, BiomeFilterTypes.SameBiome, "", questKey);
			if (closestPOIToWorldPos == null)
			{
				return Vector3.zero;
			}
			Vector2 vector = new Vector2((float)closestPOIToWorldPos.boundingBoxPosition.x + (float)closestPOIToWorldPos.boundingBoxSize.x / 2f, (float)closestPOIToWorldPos.boundingBoxPosition.z + (float)closestPOIToWorldPos.boundingBoxSize.z / 2f);
			if (vector.x == -0.1f && vector.y == -0.1f)
			{
				Log.Error("ObjectiveClosestPOIGoto: No POI found.");
				return Vector3.zero;
			}
			int num = (int)vector.x;
			int num2 = (int)entityAlive.position.y;
			int num3 = (int)vector.y;
			if (GameManager.Instance.World.IsPositionInBounds(position))
			{
				FinalizePoint(new Vector3(closestPOIToWorldPos.boundingBoxPosition.x, closestPOIToWorldPos.boundingBoxPosition.y, closestPOIToWorldPos.boundingBoxPosition.z), new Vector3(closestPOIToWorldPos.boundingBoxSize.x, closestPOIToWorldPos.boundingBoxSize.y, closestPOIToWorldPos.boundingBoxSize.z));
				position = new Vector3(num, num2, num3);
				base.OwnerQuest.Position = position;
				return position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(traderId, playerId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.Closest, base.OwnerQuest.QuestClass.DifficultyTier));
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_WaitingForServer()
	{
		if (positionSet)
		{
			base.CurrentValue = 2;
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveClosestPOIGoto objectiveClosestPOIGoto = new ObjectiveClosestPOIGoto();
		CopyValues(objectiveClosestPOIGoto);
		return objectiveClosestPOIGoto;
	}
}
