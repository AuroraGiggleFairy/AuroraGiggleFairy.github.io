using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NavObject
{
	public enum TrackTypes
	{
		None,
		Transform,
		Position,
		Entity
	}

	public static Vector3 InvalidPos = new Vector3(-99999f, -99999f, -99999f);

	public List<NavObjectClass> NavObjectClassList = new List<NavObjectClass>();

	public NavObjectClass NavObjectClass;

	public TrackTypes TrackType;

	public bool IsActive = true;

	public bool ForceDisabled;

	public int EntityID;

	public Entity OwnerEntity;

	public string name;

	public bool usingLocalizationId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform trackedTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 trackedPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity trackedEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasOnScreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextKey;

	public int Key;

	public float ExtraData;

	public bool IsTracked;

	public bool hiddenOnCompass = true;

	public string HiddenDisplayName;

	public bool hiddenOnMap;

	public string OverrideSpriteName = "";

	public bool UseOverrideColor;

	public bool UseOverrideFontColor;

	public Color OverrideColor;

	public string DisplayName
	{
		get
		{
			if (usingLocalizationId)
			{
				if (string.IsNullOrEmpty(localizedName))
				{
					localizedName = Localization.Get(name);
				}
				return localizedName;
			}
			return name;
		}
	}

	public Transform TrackedTransform
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return trackedTransform;
		}
		set
		{
			trackedTransform = value;
			TrackType = TrackTypes.Transform;
		}
	}

	public Vector3 TrackedPosition
	{
		get
		{
			return trackedPosition;
		}
		set
		{
			trackedPosition = value;
			TrackType = TrackTypes.Position;
		}
	}

	public Entity TrackedEntity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return trackedEntity;
		}
		set
		{
			trackedEntity = value;
			EntityID = (trackedEntity ? trackedEntity.entityId : (-1));
			TrackType = TrackTypes.Entity;
			SetupEntityOptions();
		}
	}

	public NavObjectMapSettings CurrentMapSettings
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return NavObjectClass?.GetMapSettings(IsActive);
		}
	}

	public NavObjectCompassSettings CurrentCompassSettings
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return NavObjectClass?.GetCompassSettings(IsActive);
		}
	}

	public NavObjectScreenSettings CurrentScreenSettings
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return NavObjectClass?.GetOnScreenSettings(IsActive);
		}
	}

	public bool HasOnScreen => hasOnScreen;

	public Vector3 Rotation
	{
		get
		{
			if (CurrentMapSettings.UseRotation && TrackType == TrackTypes.Entity)
			{
				if (trackedEntity.AttachedToEntity != null)
				{
					return trackedEntity.AttachedToEntity.rotation;
				}
				return trackedEntity.rotation;
			}
			return Vector3.zero;
		}
	}

	public bool IsTrackedTransform(Transform transform)
	{
		if (TrackType == TrackTypes.Transform)
		{
			return trackedTransform == transform;
		}
		return false;
	}

	public bool IsTrackedPosition(Vector3 position)
	{
		if (TrackType == TrackTypes.Position)
		{
			return trackedPosition == position;
		}
		return false;
	}

	public bool IsTrackedEntity(Entity entity)
	{
		if (TrackType == TrackTypes.Entity)
		{
			return trackedEntity == entity;
		}
		return false;
	}

	public bool IsValidPlayer(EntityPlayerLocal player, NavObjectClass navObjectClass)
	{
		if (ForceDisabled)
		{
			return false;
		}
		bool flag = true;
		if (TrackType == TrackTypes.Entity)
		{
			flag = IsValidEntity(player, TrackedEntity, navObjectClass);
		}
		switch (navObjectClass.RequirementType)
		{
		case NavObjectClass.RequirementTypes.QuestBounds:
		case NavObjectClass.RequirementTypes.Tracking:
			return flag;
		case NavObjectClass.RequirementTypes.CVar:
			return player.GetCVar(navObjectClass.RequirementName) > 0f && flag;
		case NavObjectClass.RequirementTypes.NoTag:
			return !NavObjectManager.Instance.HasNavObjectTag(navObjectClass.RequirementName) && flag;
		case NavObjectClass.RequirementTypes.IsOwner:
			return OwnerEntity == player && flag;
		case NavObjectClass.RequirementTypes.MinimumTreasureRadius:
		{
			float extraData = ExtraData;
			extraData = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, player);
			extraData = Mathf.Clamp(extraData, 0f, extraData);
			return extraData == 0f;
		}
		default:
			return flag;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidEntity(EntityPlayerLocal player, Entity entity, NavObjectClass navObjectClass)
	{
		if (entity == null)
		{
			return true;
		}
		if (player == null)
		{
			return true;
		}
		EntityAlive entityAlive = entity as EntityAlive;
		if ((bool)entityAlive)
		{
			if (navObjectClass.RequirementType == NavObjectClass.RequirementTypes.None)
			{
				if (entityAlive.IsAlive())
				{
					return !entityAlive.IsSleeperPassive;
				}
				return false;
			}
			if (!entityAlive.IsAlive() || entityAlive.IsSleeperPassive)
			{
				return false;
			}
			switch (navObjectClass.RequirementType)
			{
			case NavObjectClass.RequirementTypes.QuestBounds:
				if (player.QuestJournal.ActiveQuest != null && entityAlive.IsSleeper)
				{
					Vector3 position = entity.position;
					position.y = position.z;
					if (player.ZombieCompassBounds.Contains(position))
					{
						return true;
					}
				}
				return false;
			case NavObjectClass.RequirementTypes.CVar:
				return entityAlive.GetCVar(navObjectClass.RequirementName) > 0f;
			case NavObjectClass.RequirementTypes.Tracking:
				return EffectManager.GetValue(PassiveEffects.Tracking, null, 0f, player, null, entity.EntityTags) > 0f;
			case NavObjectClass.RequirementTypes.IsAlly:
				if (entity as EntityPlayer != null && (entity as EntityPlayer).IsFriendOfLocalPlayer && entity != player)
				{
					return !(entity as EntityPlayer).IsSpectator;
				}
				return false;
			case NavObjectClass.RequirementTypes.InParty:
				if (player.Party != null && player.Party.MemberList.Contains(entity as EntityPlayer) && entity != player && !(entity as EntityPlayer).IsSpectator)
				{
					if (!(player.AttachedToEntity == null))
					{
						return player.AttachedToEntity != entity.AttachedToEntity;
					}
					return true;
				}
				return false;
			case NavObjectClass.RequirementTypes.IsPlayer:
				return entity == player;
			case NavObjectClass.RequirementTypes.IsVehicleOwner:
				if (!(entity as EntityVehicle != null) || !(entity as EntityVehicle).HasOwnedEntity(player.entityId))
				{
					if (entity as EntityTurret != null)
					{
						return (entity as EntityTurret).belongsPlayerId == player.entityId;
					}
					return false;
				}
				return true;
			case NavObjectClass.RequirementTypes.NoActiveQuests:
				if (entity as EntityNPC == null)
				{
					return true;
				}
				return player.QuestJournal.FindReadyForTurnInQuestByGiver(entity.entityId) == null;
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf:
				if (entity.spawnById == player.entityId)
				{
					return !string.IsNullOrEmpty(entity.spawnByName);
				}
				return false;
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedOther:
				if (entity.spawnById > 0 && entity.spawnById != player.entityId)
				{
					return !string.IsNullOrEmpty(entity.spawnByName);
				}
				return false;
			}
		}
		else
		{
			switch (navObjectClass.RequirementType)
			{
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf:
				return entity.spawnById == player.entityId;
			case NavObjectClass.RequirementTypes.IsTwitchSpawnedOther:
				if (entity.spawnById > 0)
				{
					return entity.spawnById != player.entityId;
				}
				return false;
			}
		}
		return true;
	}

	public void AddNavObjectClass(NavObjectClass navClass)
	{
		if (!NavObjectClassList.Contains(navClass))
		{
			NavObjectClassList.Insert(0, navClass);
		}
	}

	public bool RemoveNavObjectClass(NavObjectClass navClass)
	{
		NavObjectClassList.Remove(navClass);
		if (NavObjectClassList.Count == 0)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupEntityOptions()
	{
		if (TrackType == TrackTypes.Entity && NavObjectClass != null && NavObjectClass.RequirementType == NavObjectClass.RequirementTypes.Tracking)
		{
			OverrideSpriteName = ((TrackedEntity.GetTrackerIcon() == null) ? "" : TrackedEntity.GetTrackerIcon());
		}
		else
		{
			OverrideSpriteName = "";
		}
	}

	public bool IsValid()
	{
		if (TrackType == TrackTypes.Transform && TrackedTransform == null)
		{
			TrackType = TrackTypes.None;
		}
		else if (TrackType == TrackTypes.Entity && TrackedEntity == null)
		{
			TrackType = TrackTypes.None;
		}
		return TrackType != TrackTypes.None;
	}

	public Vector3 GetPosition()
	{
		return TrackType switch
		{
			TrackTypes.Position => trackedPosition - Origin.position, 
			TrackTypes.Transform => trackedTransform.position, 
			TrackTypes.Entity => trackedEntity.position - Origin.position, 
			_ => InvalidPos, 
		};
	}

	public float GetMaxDistance(NavObjectSettings settings, EntityPlayer player)
	{
		if (TrackType == TrackTypes.Entity && NavObjectClass.RequirementType == NavObjectClass.RequirementTypes.Tracking && settings.MaxDistance == -1f)
		{
			return EffectManager.GetValue(PassiveEffects.TrackDistance, null, 0f, player);
		}
		return settings.MaxDistance;
	}

	public string GetSpriteName(NavObjectSettings settings)
	{
		if (!NavObjectClass.UseOverrideIcon)
		{
			return settings.SpriteName;
		}
		return OverrideSpriteName;
	}

	public NavObject(string className)
	{
		Key = nextKey++;
		SetupNavObjectClass(className);
	}

	public void Reset(string className)
	{
		UseOverrideColor = false;
		OverrideColor = Color.white;
		SetupNavObjectClass(className);
		trackedPosition = InvalidPos;
		trackedTransform = null;
		trackedEntity = null;
		OwnerEntity = null;
		name = "";
		ForceDisabled = false;
		usingLocalizationId = false;
		TrackType = TrackTypes.None;
	}

	public void SetupNavObjectClass(string className)
	{
		NavObjectClassList.Clear();
		hasOnScreen = false;
		if (className.Contains(","))
		{
			string[] array = className.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(array[i]);
				if (navObjectClass != null)
				{
					if (navObjectClass.OnScreenSettings != null || navObjectClass.InactiveOnScreenSettings != null)
					{
						hasOnScreen = true;
					}
					NavObjectClassList.Add(navObjectClass);
				}
			}
		}
		else
		{
			NavObjectClass navObjectClass2 = NavObjectClass.GetNavObjectClass(className);
			if (navObjectClass2.OnScreenSettings != null || navObjectClass2.InactiveOnScreenSettings != null)
			{
				hasOnScreen = true;
			}
			NavObjectClassList.Add(navObjectClass2);
		}
		NavObjectClass = NavObjectClassList[0];
	}

	public void HandleActiveNavClass(EntityPlayerLocal localPlayer)
	{
		if (NavObjectClassList == null || NavObjectClassList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < NavObjectClassList.Count; i++)
		{
			if (IsValidPlayer(localPlayer, NavObjectClassList[i]))
			{
				if (NavObjectClass != NavObjectClassList[i])
				{
					NavObjectClass = NavObjectClassList[i];
					SetupEntityOptions();
				}
				return;
			}
		}
		NavObjectClass = null;
	}

	public virtual float GetCompassIconScale(float _distance)
	{
		float t = 1f - _distance / CurrentCompassSettings.MaxScaleDistance;
		return Mathf.Lerp(CurrentCompassSettings.MinCompassIconScale, CurrentCompassSettings.MaxCompassIconScale, t);
	}

	public override string ToString()
	{
		string text = "";
		if (TrackType == TrackTypes.Transform)
		{
			text = ((TrackedTransform != null) ? TrackedTransform.name : "none");
		}
		else if (TrackType == TrackTypes.Entity)
		{
			text = ((TrackedEntity != null) ? TrackedEntity.GetDebugName() : "none");
		}
		return string.Format("{0} #{1}, {2}, {3}, {4}, {5}, {6}", name, NavObjectClassList.Count, (NavObjectClass != null) ? NavObjectClass.NavObjectClassName : "null", NavObjectClass?.RequirementType, TrackType, text, GetPosition());
	}
}
