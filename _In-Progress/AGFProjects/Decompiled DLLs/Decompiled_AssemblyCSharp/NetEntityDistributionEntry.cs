using System;
using System.Collections.Generic;
using UnityEngine;

public class NetEntityDistributionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxTrackingDistance = 46340;

	public Entity trackedEntity;

	public int updatTickCounter;

	public Vector3i encodedPos;

	public Vector3i encodedRot;

	public bool encodedOnGround;

	public Vector3 lastTrackedEntityMotion;

	public int updateCounter;

	public HashSet<EntityPlayer> trackedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int trackingDistanceThreshold;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastTrackedEntityPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdateDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldSendMotionUpdates;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sendFullUpdateAfterTicks;

	public const int cFullUpdateAfterTicks = 100;

	public int priorityLevel = 1;

	public NetEntityDistributionEntry(Entity _e, int _d, int _ticks, bool _isMotionSent)
	{
		updateCounter = 0;
		sendFullUpdateAfterTicks = 0;
		trackedPlayers = new HashSet<EntityPlayer>();
		trackedEntity = _e;
		trackingDistanceThreshold = Math.Min(46340, _d);
		updatTickCounter = _ticks;
		shouldSendMotionUpdates = _isMotionSent;
		encodedPos = EncodePos(_e.position);
		encodedRot = EncodeRot(_e.rotation);
		encodedOnGround = _e.onGround;
	}

	public override bool Equals(object _other)
	{
		if (_other is NetEntityDistributionEntry)
		{
			return ((NetEntityDistributionEntry)_other).trackedEntity.entityId == trackedEntity.entityId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return trackedEntity.entityId;
	}

	public void SendToPlayers(NetPackage _packet, int _excludePlayer, bool _inRangeOnly = false, int _range = 192)
	{
		foreach (EntityPlayer trackedPlayer in trackedPlayers)
		{
			if (trackedPlayer.entityId != _excludePlayer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(_packet, _onlyClientsAttachedToAnEntity: false, trackedPlayer.entityId, -1, _inRangeOnly ? trackedEntity.entityId : (-1), null, _range);
			}
		}
	}

	public void sendPacketToTrackedPlayersAndTrackedEntity(NetPackage _packet, int _excludePlayer, bool _inRangeOnly = false)
	{
		SendToPlayers(_packet, _excludePlayer, _inRangeOnly);
		if (trackedEntity is EntityPlayer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(_packet, _onlyClientsAttachedToAnEntity: false, trackedEntity.entityId, -1, _inRangeOnly ? trackedEntity.entityId : (-1));
		}
	}

	public void SendDestroyEntityToPlayers()
	{
		SendToPlayers(NetPackageManager.GetPackage<NetPackageEntityRemove>().Setup(trackedEntity.entityId, EnumRemoveEntityReason.Killed), -1);
	}

	public void SendUnloadEntityToPlayers()
	{
		SendToPlayers(NetPackageManager.GetPackage<NetPackageEntityRemove>().Setup(trackedEntity.entityId, EnumRemoveEntityReason.Unloaded), -1);
	}

	public void Remove(EntityPlayer _e)
	{
		if (trackedPlayers.Contains(_e))
		{
			trackedPlayers.Remove(_e);
		}
	}

	public void updatePlayerEntity(EntityPlayer _ep)
	{
		if (_ep == trackedEntity)
		{
			return;
		}
		float num = _ep.position.x - (float)(encodedPos.x / 32);
		float num2 = _ep.position.z - (float)(encodedPos.z / 32);
		if (num * num + num2 * num2 <= (float)(trackingDistanceThreshold * trackingDistanceThreshold))
		{
			if (trackedPlayers.Contains(_ep))
			{
				return;
			}
			trackedPlayers.Add(_ep);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(getSpawnPacket(), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
			EntityAlive entityAlive = trackedEntity as EntityAlive;
			if ((bool)entityAlive)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
				if (entityAlive is EntityPlayer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
				}
			}
			trackedEntity.emodel?.avatarController?.SyncAnimParameters(_ep.entityId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntitySpeeds>().Setup(trackedEntity), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
			if (shouldSendMotionUpdates)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityVelocity>().Setup(trackedEntity.entityId, trackedEntity.motion, _bAdd: false), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
			}
		}
		else if (trackedPlayers.Contains(_ep))
		{
			trackedPlayers.Remove(_ep);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityRemove>().Setup(trackedEntity.entityId, EnumRemoveEntityReason.Unloaded), _onlyClientsAttachedToAnEntity: false, _ep.entityId);
		}
	}

	public void updatePlayerEntities(List<EntityPlayer> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			EntityPlayer ep = _list[i];
			updatePlayerEntity(ep);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackage getSpawnPacket()
	{
		return NetPackageManager.GetPackage<NetPackageEntitySpawn>().Setup(new EntityCreationData(trackedEntity));
	}

	public void updatePlayerList(List<EntityPlayer> _playerList)
	{
		if (!firstUpdateDone || trackedEntity.GetDistanceSq(lastTrackedEntityPos) > 16f)
		{
			lastTrackedEntityPos = trackedEntity.position;
			firstUpdateDone = true;
			updatePlayerEntities(_playerList);
		}
		if (trackedEntity.usePhysicsMaster)
		{
			if (trackedEntity.isPhysicsMaster && updateCounter++ % updatTickCounter == 0)
			{
				NetPackageEntityPhysics netPackageEntityPhysics = trackedEntity.PhysicsMasterSetupBroadcast();
				if (netPackageEntityPhysics != null)
				{
					SendToPlayers(netPackageEntityPhysics, -1);
				}
			}
			return;
		}
		sendFullUpdateAfterTicks++;
		bool flag = priorityLevel == 0 || trackedEntity.IsAirBorne();
		int updateSteps = ((priorityLevel == 0) ? 1 : 3);
		updateCounter++;
		if (!flag)
		{
			switch (priorityLevel)
			{
			case 1:
				flag = updateCounter % updatTickCounter == 0;
				updateSteps = 3;
				break;
			case 2:
				flag = updateCounter % 6 == 0;
				updateSteps = 6;
				break;
			case 3:
				flag = updateCounter % 10 == 0;
				updateSteps = 10;
				break;
			}
		}
		if (flag)
		{
			Vector3i vector3i = EncodePos(trackedEntity.position);
			Vector3i deltaPos = vector3i - encodedPos;
			bool flag2 = Utils.FastAbs(deltaPos.x) >= 2f || Utils.FastAbs(deltaPos.y) >= 2f || Utils.FastAbs(deltaPos.z) >= 2f || encodedOnGround != trackedEntity.onGround;
			Vector3i vector3i2 = EncodeRot(trackedEntity.rotation);
			Vector3i vector3i3 = vector3i2 - encodedRot;
			bool flag3 = Utils.FastAbs(vector3i3.x) >= 2f || Utils.FastAbs(vector3i3.y) >= 2f || Utils.FastAbs(vector3i3.z) >= 2f;
			NetPackage netPackage = null;
			bool inRangeOnly = false;
			if (trackedEntity.IsMovementReplicated)
			{
				if (updatTickCounter == 1)
				{
					sendFullUpdateAfterTicks = int.MaxValue;
					inRangeOnly = true;
				}
				if (deltaPos.x < -256 || deltaPos.x >= 256 || deltaPos.y < -256 || deltaPos.y >= 256 || deltaPos.z < -256 || deltaPos.z >= 256)
				{
					sendFullUpdateAfterTicks = 0;
					netPackage = NetPackageManager.GetPackage<NetPackageEntityTeleport>().Setup(trackedEntity);
				}
				else if (deltaPos.x < -128 || deltaPos.x >= 128 || deltaPos.y < -128 || deltaPos.y >= 128 || deltaPos.z < -128 || deltaPos.z >= 128 || sendFullUpdateAfterTicks > 100)
				{
					sendFullUpdateAfterTicks = 0;
					netPackage = NetPackageManager.GetPackage<NetPackageEntityPosAndRot>().Setup(trackedEntity);
				}
				else if (flag2 && flag3)
				{
					netPackage = NetPackageManager.GetPackage<NetPackageEntityRelPosAndRot>().Setup(trackedEntity.entityId, deltaPos, vector3i2, trackedEntity.qrotation, trackedEntity.onGround, trackedEntity.IsQRotationUsed(), updateSteps);
					inRangeOnly = true;
				}
				else if (flag2)
				{
					netPackage = NetPackageManager.GetPackage<NetPackageEntityRelPosAndRot>().Setup(trackedEntity.entityId, deltaPos, vector3i2, trackedEntity.qrotation, trackedEntity.onGround, trackedEntity.IsQRotationUsed(), updateSteps);
					inRangeOnly = true;
				}
				else if (flag3)
				{
					netPackage = NetPackageManager.GetPackage<NetPackageEntityRotation>().Setup(trackedEntity.entityId, vector3i2, trackedEntity.qrotation, trackedEntity.IsQRotationUsed());
					inRangeOnly = true;
				}
			}
			if (shouldSendMotionUpdates)
			{
				float sqrMagnitude = (trackedEntity.motion - lastTrackedEntityMotion).sqrMagnitude;
				if (sqrMagnitude > 0.040000003f || (sqrMagnitude > 0f && trackedEntity.motion.Equals(Vector3.zero)))
				{
					lastTrackedEntityMotion = trackedEntity.motion;
					SendToPlayers(NetPackageManager.GetPackage<NetPackageEntityVelocity>().Setup(trackedEntity.entityId, lastTrackedEntityMotion, _bAdd: false), -1);
				}
			}
			if (netPackage != null)
			{
				SendToPlayers(netPackage, -1, inRangeOnly, trackingDistanceThreshold);
			}
			EntityAlive entityAlive = trackedEntity as EntityAlive;
			if (entityAlive != null && entityAlive.bEntityAliveFlagsChanged)
			{
				SendToPlayers(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive), trackedEntity.entityId);
				entityAlive.bEntityAliveFlagsChanged = false;
			}
			EntityPlayer entityPlayer = trackedEntity as EntityPlayer;
			if (entityPlayer != null && entityPlayer.bPlayerStatsChanged)
			{
				SendToPlayers(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), trackedEntity.entityId);
				entityAlive.bPlayerStatsChanged = false;
			}
			if (entityPlayer != null && entityPlayer.bPlayerTwitchChanged)
			{
				SendToPlayers(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityAlive), trackedEntity.entityId);
				entityAlive.bPlayerTwitchChanged = false;
			}
			if (flag2)
			{
				encodedPos = vector3i;
				encodedOnGround = trackedEntity.onGround;
			}
			if (flag3)
			{
				encodedRot = vector3i2;
			}
		}
		trackedEntity.SetAirBorne(_b: false);
	}

	public void SendFullUpdateNextTick()
	{
		sendFullUpdateAfterTicks = 100;
	}

	public static Vector3i EncodePos(Vector3 _pos)
	{
		return new Vector3i(_pos.x * 32f + 0.5f, _pos.y * 32f + 0.5f, _pos.z * 32f + 0.5f);
	}

	public static Vector3i EncodeRot(Vector3 _rot)
	{
		return new Vector3i(_rot * 256f / 360f);
	}
}
