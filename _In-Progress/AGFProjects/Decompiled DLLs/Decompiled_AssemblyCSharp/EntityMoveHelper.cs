using System;
using System.Collections.Generic;
using GamePath;
using UnityEngine;

public class EntityMoveHelper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct DestroyData(int _offsetX, int _offsetZ, int _stepX, int _stepZ)
	{
		public int offsetX = _offsetX;

		public int offsetZ = _offsetZ;

		public int stepX = _stepX;

		public int stepZ = _stepZ;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDoneXZDistSq = 0.0009f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckBlockedDist = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckBlockedRadius = 0.125f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckSidestepDist = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckSidestepRadius = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTempMoveDist = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cYawNextDist = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoveDirectDist = 0.65f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoveSlowDist = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigXZDistSq = 0.010000001f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigDiagonalXZDistSq = 2.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigAngleCos = 0.86f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cJumpUpXZDistSq = 0.16000001f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLadderXZDistSq = 0.10890001f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUnreachJumpMin = 1.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	public bool IsActive;

	public bool CanBreakBlocks;

	public Vector3 JumpToPos;

	public EntityAlive BlockedEntity;

	public int BlockedFlags;

	public int BlockedFlagsAfterCrouch;

	public float BlockedTime;

	public WorldRayHitInfo HitInfo = new WorldRayHitInfo();

	public WorldRayHitInfo HitInfo2 = new WorldRayHitInfo();

	public float DamageScale;

	public bool IsUnreachableAbove;

	public bool IsUnreachableSide;

	public bool IsUnreachableSideJump;

	public Vector3 UnreachablePos;

	public float SideStepAngle;

	public float UnreachablePercent;

	public bool IsDestroyAreaTryUnreachable;

	public bool IsDestroyArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public DamageResponse damageResponse = DamageResponse.New(_fatal: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 moveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveToDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moveToTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moveToFailCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveToDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 focusPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int focusTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int obstacleCheckTickDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasNextPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 nextMoveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 tempMoveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTempMove;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blockedDistSq;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blockedEntityDistSq;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDigging;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int expiryTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClimb;

	[PublicizedFrom(EAccessModifier.Private)]
	public float jumpYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public int swimStrokeDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ccRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ccHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] checkEdgeXs = new float[3] { 0f, -0.25f, 0.25f };

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigMovedDist = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 digStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digForTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digActionTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool digAttacked;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digForwardCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDestroyRefreshAfter = 25;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDestroyOtherAIDist = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DestroyData[] destroyData = new DestroyData[7]
	{
		new DestroyData(-1, 1, 1, 0),
		new DestroyData(1, 1, 0, -1),
		new DestroyData(1, -1, -1, 0),
		new DestroyData(-1, -1, 0, 1),
		new DestroyData(-1, 1, 1, 0),
		new DestroyData(1, 1, 0, -1),
		new DestroyData(1, -1, -1, 0)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int destroyRefreshTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 destroyPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Entity> entityTempList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] blockOpenOffsets = new int[8] { -1, 0, 1, 0, 0, 1, 0, -1 };

	public EntityMoveHelper(EntityAlive _entity)
	{
		entity = _entity;
		random = _entity.rand;
		moveToPos = _entity.position;
	}

	public void SetMoveTo(Vector3 _pos, bool _canBreakBlocks)
	{
		moveToPos = _pos;
		moveSpeed = entity.GetMoveSpeedAggro();
		focusTicks = 0;
		isTempMove = false;
		CanBreakBlocks = _canBreakBlocks;
		isClimb = false;
		IsActive = true;
		expiryTicks = 10;
		ResetStuckCheck();
	}

	public void SetMoveTo(PathEntity path, float _speed, bool _canBreakBlocks)
	{
		PathPoint currentPoint = path.CurrentPoint;
		Vector3 vector = moveToPos;
		moveToPos = currentPoint.AdjustedPositionForEntity(entity);
		CanBreakBlocks = _canBreakBlocks;
		bool flag = true;
		if (IsActive)
		{
			if ((moveToPos - vector).sqrMagnitude < 0.010000001f)
			{
				flag = false;
			}
		}
		else
		{
			moveToDir = entity.rotation.y;
		}
		if (flag)
		{
			focusTicks = 0;
			isTempMove = false;
			ResetStuckCheck();
		}
		hasNextPos = false;
		PathPoint nextPoint = path.NextPoint;
		if (nextPoint != null)
		{
			hasNextPos = true;
			nextMoveToPos = nextPoint.AdjustedPositionForEntity(entity);
		}
		moveSpeed = _speed;
		isClimb = false;
		expiryTicks = 40;
		IsActive = true;
	}

	public void Stop()
	{
		StopMove();
		entity.getNavigator().clearPath();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopMove()
	{
		IsActive = false;
		if (!entity.Jumping || entity.isSwimming)
		{
			entity.SetMoveForward(0f);
			entity.SetRotationAndStopTurning(entity.rotation);
		}
		expiryTicks = 0;
		ClearBlocked();
		BlockedEntity = null;
	}

	public void SetFocusPos(Vector3 _pos)
	{
		focusPos = _pos;
		focusTicks = 5;
	}

	public void UpdateMoveHelper()
	{
		destroyRefreshTicks--;
		if (!IsActive)
		{
			return;
		}
		if (--expiryTicks <= 0)
		{
			StopMove();
			return;
		}
		ccHeight = entity.m_characterController.GetHeight();
		ccRadius = entity.m_characterController.GetRadius();
		Vector3 position = entity.position;
		Vector3 vector = moveToPos;
		if (isTempMove)
		{
			if (BlockedFlags == 0)
			{
				isTempMove = false;
				ResetStuckCheck();
			}
			else
			{
				vector = tempMoveToPos;
			}
		}
		bool jumping = entity.Jumping;
		bool flag = jumping || entity.isSwimming;
		bool flag2 = jumping && !entity.isSwimming;
		float num = vector.x - position.x;
		float num2 = vector.z - position.z;
		float num3 = num * num + num2 * num2;
		float num4 = vector.y - (position.y + 0.05f);
		bool flag3 = entity.IsInElevator();
		isClimb = false;
		if (flag3 && entity.bCanClimbLadders && num3 < 0.10890001f && num4 > 0.1f && !jumping)
		{
			isClimb = true;
		}
		else if (num3 <= 0.0009f && Utils.FastAbs(num4) < 0.25f && !isTempMove)
		{
			StopMove();
			return;
		}
		AvatarController avatarController = entity.emodel.avatarController;
		if (avatarController.IsRootMotionForced())
		{
			entity.SetMoveForwardWithModifiers(moveSpeed, 1f, _climb: false);
			ResetStuckCheck();
			ClearTempMove();
			ClearBlocked();
			return;
		}
		if ((!flag && !isDigging && !avatarController.IsAnimationWithMotionRunning()) || entity.sleepingOrWakingUp || !entity.bodyDamage.CurrentStun.CanMove() || entity.emodel.IsRagdollActive)
		{
			entity.SetMoveForward(0f);
			ResetStuckCheck();
			ClearBlocked();
			return;
		}
		float num5 = moveToPos.x - position.x;
		float num6 = moveToPos.z - position.z;
		float num7 = num5 * num5 + num6 * num6;
		float num8 = moveToPos.y - (position.y + 0.05f);
		if (num8 < -1.1f && num7 <= 0.010000001f && !flag2 && entity.onGround)
		{
			DigStart(20);
		}
		if (isDigging)
		{
			DigUpdate();
			return;
		}
		float num9 = Mathf.Atan2(num5, num6) * 57.29578f;
		if (flag2)
		{
			moveToDir = num9;
		}
		else
		{
			moveToDir = Mathf.MoveTowardsAngle(moveToDir, num9, 13f);
		}
		entity.emodel.ClearLookAt();
		if (hasNextPos || num7 >= 0.0225f)
		{
			float num10 = 9999f;
			if (flag2)
			{
				num10 = jumpYaw;
			}
			else
			{
				float num11 = num5;
				float num12 = num6;
				if (hasNextPos && num7 <= 2.25f)
				{
					float t = Mathf.Sqrt(num7) / 1.5f;
					num11 = Utils.FastLerp(nextMoveToPos.x, moveToPos.x, t) - position.x;
					num12 = Utils.FastLerp(nextMoveToPos.z, moveToPos.z, t) - position.z;
				}
				if (focusTicks > 0)
				{
					focusTicks--;
					num11 = focusPos.x - position.x;
					num12 = focusPos.z - position.z;
				}
				if (num11 * num11 + num12 * num12 > 0.0001f)
				{
					num10 = Mathf.Atan2(num11, num12) * 57.29578f;
				}
			}
			if (num10 < 9000f)
			{
				entity.SeekYaw(num10, 0f, 30f);
			}
		}
		float num13 = Utils.FastAbs(Utils.DeltaAngle(num9, moveToDir));
		float num14 = 1f;
		if (IsUnreachableAbove && !entity.IsRunning)
		{
			num14 = 1.3f;
		}
		float num15 = num13 - 15f;
		if (num15 > 0f)
		{
			num14 *= 1f - Utils.FastMin(num15 / 30f, 0.8f);
		}
		if (num14 > 0.5f)
		{
			if (BlockedTime > 0.1f)
			{
				num14 = 0.5f;
			}
			if (focusTicks > 0)
			{
				num14 = 0.45f;
			}
		}
		if (flag3 && !entity.onGround)
		{
			num14 = 0.5f;
		}
		if (entity.hasBeenAttackedTime > 0 && entity.painResistPercent < 1f)
		{
			num14 = 0.1f;
		}
		if (!hasNextPos && !isTempMove && !jumping && num3 < 0.36f && num14 > 0.1f)
		{
			float num16 = num14 * Mathf.Sqrt(num3) / 0.6f;
			if (num16 < 0.1f)
			{
				num16 = 0.1f;
			}
			num14 = num16;
		}
		bool isBreakingBlocks = entity.IsBreakingBlocks;
		if (isBreakingBlocks)
		{
			num14 = 0.03f;
		}
		entity.SetMoveForwardWithModifiers(moveSpeed, num14, isClimb);
		if (num14 > 0f)
		{
			float x = num;
			float z = num2;
			float minMotion = 0.02f * num14;
			float maxMotion = 1f;
			if (!isTempMove)
			{
				if (entity.entityType == EntityType.Bandit)
				{
					entity.AddMotion(moveToDir, entity.speedForward * num14 * 40f * 0.02f);
				}
				if (SideStepAngle != 0f)
				{
					float f = (moveToDir + SideStepAngle) * (MathF.PI / 180f);
					x = Mathf.Sin(f);
					z = Mathf.Cos(f);
					minMotion = 0.025f;
					maxMotion = 0.06f;
					moveToPos = Vector3.MoveTowards(moveToPos, position, 0.010000001f);
				}
				else if (num3 > 0.42249995f)
				{
					float f2 = moveToDir * (MathF.PI / 180f);
					x = Mathf.Sin(f2);
					z = Mathf.Cos(f2);
				}
			}
			entity.MakeMotionMoveToward(x, z, minMotion, maxMotion);
			if (flag3)
			{
				Vector3 normalized = new Vector3(num, num4, num2).normalized;
				float num17 = Mathf.Pow(moveSpeed, 0.4f);
				if (num4 > 0.1f)
				{
					num17 *= 0.7f;
				}
				else if (num4 < -0.1f)
				{
					num17 *= 1.4f;
				}
				normalized *= num17 * 0.1f;
				entity.motion = normalized;
			}
		}
		if (flag2)
		{
			return;
		}
		if (entity.isSwimming && entity.swimStrokeRate.x > 0f)
		{
			swimStrokeDelayTicks--;
			if (swimStrokeDelayTicks <= 0)
			{
				swimStrokeDelayTicks = (int)(20f / random.RandomRange(entity.swimStrokeRate.x, entity.swimStrokeRate.y));
				StartSwimStroke();
				swimStrokeDelayTicks += 3;
			}
		}
		if (isBreakingBlocks || num13 > 60f || num14 == 0f)
		{
			moveToTicks = 0;
		}
		else if (++moveToTicks > 6)
		{
			moveToTicks = 0;
			float num18 = Mathf.Sqrt(num * num + num4 * num4 + num2 * num2);
			float num19 = moveToDistance - num18;
			if (num19 < 0.021f)
			{
				if (num19 < -0.01f)
				{
					moveToDistance = num18;
				}
				if (++moveToFailCnt >= 3 && !AIDirector.debugFreezePos)
				{
					bool flag4 = num8 < -1.1f && num7 <= 0.64000005f;
					if (flag4 && entity.onGround && random.RandomFloat < 0.6f)
					{
						DigStart(80);
						return;
					}
					CheckAreaBlocked();
					if (BlockedFlags > 0)
					{
						if (random.RandomFloat < 0.7f)
						{
							DamageScale = 6f;
							obstacleCheckTickDelay = 40;
						}
						else
						{
							StartJump(calcYaw: false, 0.5f + random.RandomFloat * 0.4f, 1.3f);
						}
					}
					else
					{
						if (flag4)
						{
							return;
						}
						if (random.RandomFloat > 0.5f)
						{
							if (entity.Attack(_isReleased: false))
							{
								entity.Attack(_isReleased: true);
							}
						}
						else
						{
							StartJump(calcYaw: false, 0.7f + random.RandomFloat * 0.8f, 1.4f);
						}
					}
					return;
				}
			}
			else
			{
				moveToDistance = num18;
				if (num19 >= 0.07f)
				{
					moveToFailCnt = 0;
				}
			}
		}
		if (!entity.onGround && !entity.isSwimming && !flag3 && !isClimb && (num8 < -0.5f || num8 > 0.5f))
		{
			BlockedTime = 0f;
			BlockedEntity = null;
		}
		else if (--obstacleCheckTickDelay <= 0)
		{
			obstacleCheckTickDelay = 4;
			BlockedEntity = null;
			BlockedFlags = 0;
			BlockedFlagsAfterCrouch = 0;
			blockedDistSq = float.MaxValue;
			if (isClimb)
			{
				CheckBlockedUp(position);
				BlockedFlagsAfterCrouch = BlockedFlags;
			}
			else if (num13 < 10f)
			{
				CheckEntityBlocked(position, moveToPos);
				CheckWorldBlocked();
				BlockedFlagsAfterCrouch = ((entity.crouchType == 0) ? BlockedFlags : (BlockedFlags & -3));
				if (BlockedFlagsAfterCrouch > 0)
				{
					obstacleCheckTickDelay = 12;
					ResetStuckCheck();
				}
				SideStepAngle = 0f;
				if (!IsUnreachableAbove && hasNextPos && (BlockedFlagsAfterCrouch > 0 || (bool)BlockedEntity))
				{
					SideStepAngle = CalcObstacleSideStep();
					if (SideStepAngle != 0f)
					{
						isTempMove = false;
						BlockedEntity = null;
						ClearBlocked();
					}
				}
				if ((bool)BlockedEntity)
				{
					if (BlockedFlagsAfterCrouch == 0 || blockedEntityDistSq < blockedDistSq)
					{
						moveToTicks = 0;
						if (random.RandomFloat < 0.1f)
						{
							if (BlockedEntity.moveHelper != null && BlockedEntity.moveHelper.BlockedFlags > 0)
							{
								StartJump(calcYaw: false, 0.7f, BlockedEntity.height * 0.8f);
							}
						}
						else
						{
							Push(BlockedEntity);
						}
					}
				}
				else if ((BlockedFlags > 0 || !hasNextPos) && num8 < -1.5f && num7 >= 2.25f && entity.onGround)
				{
					float num20 = Mathf.Sqrt(num7 + num8 * num8) + 0.001f;
					if (num8 / num20 < -0.86f)
					{
						DigStart(160);
					}
				}
			}
		}
		if (BlockedFlagsAfterCrouch > 0)
		{
			BlockedTime += 0.05f;
		}
		else
		{
			BlockedTime = 0f;
		}
		if (!entity.CanEntityJump() || isClimb || flag)
		{
			return;
		}
		float num21 = 0f;
		float heightDiff = 0.9f;
		if (BlockedTime > 0.1f && BlockedFlags == 1)
		{
			num21 = 0.5f + random.RandomFloat * 0.3f;
		}
		else if (num8 > 0.9f && num7 <= 0.16000001f && random.RandomFloat < 0.1f)
		{
			num21 = 0.05f + random.RandomFloat * 0.2f;
			heightDiff = 1f;
		}
		if (IsUnreachableSideJump && num13 < 25f)
		{
			PathEntity path = entity.navigator.getPath();
			if (path == null || path.NodeCountRemaining() <= 1)
			{
				Vector3 vector2 = entity.position + entity.GetForwardVector() * 0.2f;
				vector2.y += 0.4f;
				if (!Physics.Raycast(vector2 - Origin.position, Vector3.down, out var hitInfo, 3.4f, 1082195968) || hitInfo.distance > 2.2f)
				{
					num21 = entity.jumpMaxDistance;
					heightDiff = UnreachablePos.y - entity.position.y;
				}
			}
		}
		if (!(num21 > 0f))
		{
			return;
		}
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y + 2.35f), Utils.Fastfloor(position.z));
		BlockValue block = entity.world.GetBlock(vector3i);
		if (!block.Block.IsMovementBlocked(entity.world, vector3i, block, BlockFace.None))
		{
			StartJump(calcYaw: true, num21, heightDiff);
			if (IsUnreachableSideJump)
			{
				UnreachablePercent += 0.1f;
				IsDestroyAreaTryUnreachable = true;
			}
		}
		IsUnreachableSideJump = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckWorldBlocked()
	{
		BlockedFlags = 0;
		DamageScale = 1f;
		Vector3 headPosition = entity.getHeadPosition();
		headPosition.x = entity.position.x * 0.5f + headPosition.x * 0.5f;
		headPosition.z = entity.position.z * 0.5f + headPosition.z * 0.5f;
		headPosition.y = entity.position.y;
		Vector3 endPos = moveToPos;
		if (ccHeight < 1f)
		{
			float num = Utils.FastMax(ccHeight, 0.7f);
			headPosition.y += num - 0.125f;
			endPos.y = headPosition.y + 1f;
			headPosition.y += 0.3f;
			CheckBlocked(headPosition, endPos, 1, checkSlope: false, HitInfo2);
			headPosition.y -= 0.3f;
			endPos.y = headPosition.y;
			CheckBlocked(headPosition, endPos, 0, checkSlope: true, HitInfo);
			if (BlockedFlags == 2)
			{
				HitInfo.CopyFrom(HitInfo2);
				if (entity.crouchType != 0 || entity.physicsHeight < 1f)
				{
					isTempMove = false;
				}
			}
			else if (BlockedFlags == 3)
			{
				SelectBestHit();
			}
			return;
		}
		float num2 = Utils.FastClamp(ccHeight, 1.225f, 1.5f);
		headPosition.y += num2;
		endPos.y = Utils.FastMax(headPosition.y, moveToPos.y + 0.125f + 0.3f);
		CheckBlocked(headPosition, endPos, 1, checkSlope: false, HitInfo2);
		Vector3 pos = headPosition;
		pos.y = entity.position.y + entity.stepHeight + 0.125f;
		endPos.y = pos.y;
		CheckBlocked(pos, endPos, 0, checkSlope: true, HitInfo);
		if ((BlockedFlags & 2) > 0)
		{
			if (BlockedFlags == 2)
			{
				HitInfo.CopyFrom(HitInfo2);
				if (entity.crouchType != 0)
				{
					isTempMove = false;
				}
			}
			else
			{
				SelectBestHit();
			}
		}
		else
		{
			if (BlockedFlags <= 0)
			{
				return;
			}
			endPos.y = headPosition.y + 1f;
			CheckBlocked(headPosition, endPos, 2, checkSlope: false, HitInfo2);
			if ((BlockedFlags & 4) > 0 && (HitInfo.hit.blockPos.x != Utils.Fastfloor(entity.position.x) || HitInfo.hit.blockPos.z != Utils.Fastfloor(entity.position.z)))
			{
				SelectBestHit();
				BlockValue blockValue = HitInfo.hit.blockValue;
				float num3 = blockValue.Block.MaxDamage - blockValue.damage;
				BlockValue blockValue2 = HitInfo2.hit.blockValue;
				if ((float)(blockValue2.Block.MaxDamage - blockValue2.damage) < num3 * 0.7f)
				{
					HitInfo.CopyFrom(HitInfo2);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectBestHit()
	{
		BlockValue blockValue = HitInfo.hit.blockValue;
		float num = blockValue.Block.MaxDamage - blockValue.damage;
		BlockValue blockValue2 = HitInfo2.hit.blockValue;
		if ((float)(blockValue2.Block.MaxDamage - blockValue2.damage) < num * 0.7f)
		{
			HitInfo.CopyFrom(HitInfo2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBlocked(Vector3 pos, Vector3 endPos, int baseY, bool checkSlope, WorldRayHitInfo hitInfo)
	{
		endPos.y -= 0.01f;
		Vector3 vector = endPos - pos;
		float num = vector.magnitude + 0.001f;
		vector *= 1f / num;
		Ray ray = new Ray(pos - vector * 0.375f, vector);
		if (num > ccRadius + 0.35f)
		{
			num = ccRadius + 0.35f;
			if (isTempMove)
			{
				num += 0.4f;
			}
		}
		if (vector.y >= 0.2f)
		{
			num += 0.21f;
		}
		if (!Voxel.Raycast(entity.world, ray, num - 0.125f + 0.375f, 1082195968, 128, 0.125f))
		{
			return;
		}
		if (checkSlope && BlockedFlags == 0 && Voxel.phyxRaycastHit.normal.y > 0.643f)
		{
			Vector2 vector2 = default(Vector2);
			vector2.x = Voxel.phyxRaycastHit.normal.x;
			vector2.y = Voxel.phyxRaycastHit.normal.z;
			vector2.Normalize();
			Vector2 vector3 = default(Vector2);
			vector3.x = vector.x;
			vector3.y = vector.z;
			vector3.Normalize();
			if (vector3.x * vector2.x + vector3.y * vector2.y < -0.7f)
			{
				return;
			}
		}
		if (!(Voxel.voxelRayHitInfo.hit.blockValue.Block is BlockDamage))
		{
			hitInfo.CopyFrom(Voxel.voxelRayHitInfo);
			BlockedFlags |= 1 << baseY;
			Vector3 vector4 = pos - hitInfo.hit.pos;
			float sqrMagnitude = vector4.sqrMagnitude;
			if (sqrMagnitude < blockedDistSq)
			{
				blockedDistSq = sqrMagnitude;
				float num2 = 1f / Mathf.Sqrt(sqrMagnitude);
				float num3 = ccRadius + 0.4f;
				tempMoveToPos = vector4 * (num2 * num3) + hitInfo.hit.pos;
				tempMoveToPos.y = Mathf.MoveTowards(tempMoveToPos.y, moveToPos.y, 1f);
				isTempMove = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBlockedUp(Vector3 pos)
	{
		BlockedFlags = 0;
		Vector3 headPosition = entity.getHeadPosition();
		headPosition.x = pos.x;
		headPosition.z = pos.z;
		headPosition.y -= 0.625f;
		if (Voxel.Raycast(ray: new Ray(headPosition, Vector3.up), _world: entity.world, distance: 1f, _layerMask: 1082195968, _hitMask: 128, _sphereRadius: 0.125f) && !(Voxel.voxelRayHitInfo.hit.blockValue.Block is BlockDamage))
		{
			HitInfo.CopyFrom(Voxel.voxelRayHitInfo);
			BlockedFlags = 4;
			float sqrMagnitude = (pos - HitInfo.hit.pos).sqrMagnitude;
			if (sqrMagnitude < blockedDistSq)
			{
				blockedDistSq = sqrMagnitude;
				obstacleCheckTickDelay = 12;
				ResetStuckCheck();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckAreaBlocked()
	{
		BlockedFlags = 0;
		Vector3 headPosition = entity.getHeadPosition();
		headPosition.y = entity.position.y;
		Vector3 vector = moveToPos - headPosition;
		float f = Mathf.Atan2(vector.x, vector.z);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		vector.Normalize();
		Vector3 vector2 = headPosition + vector * 0.575f;
		for (float num3 = ccHeight - 0.125f; num3 > 0.225f; num3 -= 0.25f)
		{
			for (int i = 0; i < 3; i++)
			{
				float num4 = checkEdgeXs[i];
				float num5 = num4 * num2;
				float num6 = num4 * (0f - num);
				Vector3 pos = headPosition;
				pos.x += num5;
				pos.y += num3;
				pos.z += num6;
				Vector3 endPos = vector2;
				endPos.x += num5;
				endPos.y += num3;
				endPos.z += num6;
				CheckBlocked(pos, endPos, 0, checkSlope: false, HitInfo);
				if (BlockedFlags > 0)
				{
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcObstacleSideStep()
	{
		Vector3 headPosition = entity.getHeadPosition();
		headPosition.y = entity.position.y;
		Vector3 vector = moveToPos - headPosition;
		if (vector.y >= 0.6f)
		{
			return 0f;
		}
		float num = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
		if (num <= ccRadius + 0.05f)
		{
			return 0f;
		}
		Vector2 vector2 = new Vector2(vector.x / num, vector.z / num);
		headPosition.x -= vector2.x * 0.2f;
		headPosition.z -= vector2.y * 0.2f;
		float angleRad = Mathf.Atan2(vector2.x, vector2.y);
		if (CalcObstacleSideStepArc(headPosition, angleRad, 8f, 20f, 10f) == 0f && CalcObstacleSideStepArc(headPosition, angleRad, -8f, -20f, -10f) == 0f)
		{
			return 0f;
		}
		float num2 = CalcObstacleSideStepArc(headPosition, angleRad, -48f, -20f, 11f);
		float num3 = CalcObstacleSideStepArc(headPosition, angleRad, 48f, 20f, -11f);
		if (Utils.FastAbs(num2) < num3)
		{
			if (num2 <= -48f)
			{
				return 0f;
			}
			if (num2 == 0f)
			{
				num2 = -20f;
			}
			return num2 - 50f;
		}
		if (num3 >= 48f)
		{
			return 0f;
		}
		if (num3 == 0f)
		{
			num3 = 20f;
		}
		return num3 + 50f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcObstacleSideStepArc(Vector3 startPos, float angleRad, float dirMin, float dirMax, float dirStep)
	{
		float num = ccRadius + 0.45f;
		Vector3 vector = startPos;
		Vector3 direction = default(Vector3);
		direction.y = 0f;
		float num2 = dirMin;
		int num3 = (int)Utils.FastAbs((dirMax - dirMin) / dirStep) + 1;
		for (int i = 0; i < num3; i++)
		{
			float num4 = num2 * (MathF.PI / 180f);
			float f = angleRad + num4;
			direction.x = Mathf.Sin(f);
			direction.z = Mathf.Cos(f);
			float maxDistance = num / Mathf.Cos(num4);
			for (float num5 = ccHeight - 0.1f; num5 > 0.3f; num5 -= 0.9f)
			{
				vector.y = startPos.y + num5;
				if (Physics.SphereCast(vector - Origin.position, 0.1f, direction, out var _, maxDistance, 1082720256))
				{
					return num2;
				}
			}
			num2 += dirStep;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckEntityBlocked(Vector3 pos, Vector3 endPos)
	{
		Vector3 direction = endPos - pos;
		pos.y += 0.7f;
		if (!Physics.SphereCast(pos - Origin.position, 0.15f, direction, out var hitInfo, 0.8f, 524288))
		{
			return;
		}
		Transform transform = hitInfo.transform;
		if (!transform)
		{
			return;
		}
		Transform transform2 = transform.parent.Find("GameObject");
		if (!transform2)
		{
			return;
		}
		EntityAlive component = transform2.GetComponent<EntityAlive>();
		if ((bool)component && component != entity)
		{
			float sqrMagnitude = (entity.position - component.position).sqrMagnitude;
			float num = ccRadius + component.m_characterController.GetRadius() + 0.16f + 0.25f;
			if (sqrMagnitude < num * num)
			{
				BlockedEntity = component;
				blockedEntityDistSq = sqrMagnitude;
			}
		}
	}

	public void StartJump(bool calcYaw, float distance = 0f, float heightDiff = 0f)
	{
		if (!entity.Jumping && (entity.onGround || entity.IsInElevator()) && !entity.Electrocuted)
		{
			JumpToPos = moveToPos;
			if (!calcYaw)
			{
				jumpYaw = entity.rotation.y;
			}
			else
			{
				float y = moveToPos.x - entity.position.x;
				float x = moveToPos.z - entity.position.z;
				jumpYaw = Mathf.Atan2(y, x) * 57.29578f;
			}
			entity.Jumping = true;
			entity.SetJumpDistance(distance, heightDiff);
			ClearBlocked();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSwimStroke()
	{
		if (!entity.Jumping)
		{
			JumpToPos = moveToPos;
			float y = moveToPos.x - entity.position.x;
			float x = moveToPos.z - entity.position.z;
			jumpYaw = Mathf.Atan2(y, x) * 57.29578f;
			entity.Jumping = true;
			entity.SetSwimValues(swimStrokeDelayTicks, moveToPos - entity.position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Push(EntityAlive blockerEntity)
	{
		Vector3 normalized = (blockerEntity.position - entity.position).normalized;
		damageResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, normalized);
		float massKg = EntityClass.list[entity.entityClass].MassKg;
		damageResponse.StunDuration = 0f;
		damageResponse.Strength = (int)(massKg * 0.05f);
		blockerEntity.DoRagdoll(damageResponse);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackPush(EntityAlive blockerEntity)
	{
		Vector3 normalized = (blockerEntity.position - entity.position).normalized;
		damageResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, normalized);
		if (entity.inventory.holdingItemData.actionData[0] is ItemActionAttackData itemActionAttackData)
		{
			itemActionAttackData.hitDelegate = GetAttackHitInfo;
			if (entity.Attack(_isReleased: false))
			{
				entity.Attack(_isReleased: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetAttackHitInfo(out float damageMpy)
	{
		if ((bool)BlockedEntity)
		{
			float massKg = EntityClass.list[entity.entityClass].MassKg;
			if (random.RandomFloat < 0.3f)
			{
				damageResponse.StunDuration = 0.5f;
				damageResponse.Strength = (int)(massKg * 0.4f);
			}
			else
			{
				damageResponse.StunDuration = 0f;
				damageResponse.Strength = (int)(massKg * 0.2f);
			}
			BlockedEntity.DoRagdoll(damageResponse);
		}
		damageMpy = 0f;
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigStart(int forTicks)
	{
		digStartPos = entity.position;
		if (isDigging)
		{
			digForTicks = Utils.FastMax(digForTicks, forTicks);
		}
		else if (CanBreakBlocks)
		{
			digForTicks = forTicks;
			digTicks = 0f;
			digActionTicks = 18f;
			digAttacked = false;
			digForwardCount = 0f;
			AvatarController avatarController = entity.emodel.avatarController;
			avatarController.CancelEvent("EndTrigger");
			avatarController.TriggerEvent("DigStartTrigger");
			isDigging = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigUpdate()
	{
		if ((digForTicks -= 1f) <= 0f)
		{
			DigStop();
			return;
		}
		entity.SetMoveForward(0f);
		if (entity.world.IsDark())
		{
			expiryTicks = 5;
		}
		digTicks += 1f;
		if (digTicks < digActionTicks)
		{
			return;
		}
		if (!entity.emodel.avatarController.IsAnimationDigRunning())
		{
			isDigging = false;
			return;
		}
		if ((entity.position - digStartPos).sqrMagnitude >= 0.25f)
		{
			DigStop();
			return;
		}
		if (!digAttacked)
		{
			entity.emodel.avatarController.TriggerEvent("DigTrigger");
			digTicks = 0f;
			digActionTicks = 4f;
			digAttacked = true;
			return;
		}
		digActionTicks = 14f;
		digAttacked = false;
		Vector3 position = entity.position;
		position.y += 0.6f;
		Vector3 direction;
		float distance;
		if (digForwardCount > 0f)
		{
			digForwardCount -= 1f;
			direction = entity.GetForwardVector();
			distance = 1.1f;
			entity.SeekYaw(entity.rotation.y + (random.RandomFloat * 2f - 1f) * 120f, 0f, 120f);
		}
		else
		{
			position.x += (random.RandomFloat - 0.5f) * 0.3f;
			position.z += (random.RandomFloat - 0.5f) * 0.3f;
			direction = moveToPos - position;
			distance = 1.4000001f;
		}
		if (Voxel.Raycast(ray: new Ray(position, direction), _world: entity.world, distance: distance, _layerMask: 1082195968, _hitMask: 128, _sphereRadius: 0.15f))
		{
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			DamageMultiplier damageMultiplier = new DamageMultiplier();
			List<string> buffActions = null;
			ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
			{
				hardnessScale = 1f
			};
			float num = 1f;
			if (entity.inventory.holdingItem.Actions[0] is ItemActionAttack itemActionAttack)
			{
				num = itemActionAttack.GetDamageBlock(entity.inventory.holdingItemData.actionData[0].invData.itemValue, BlockValue.Air);
			}
			ItemActionAttack.Hit(voxelRayHitInfo, entity.entityId, EnumDamageTypes.Bashing, num, num, 1f, 1f, 0f, 0.05f, "organic", damageMultiplier, buffActions, attackDetails);
		}
		else if (digForwardCount == 0f)
		{
			digForwardCount = 2f;
		}
		else
		{
			digForwardCount = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigStop()
	{
		if (isDigging)
		{
			isDigging = false;
			entity.emodel.avatarController.TriggerEvent("EndTrigger");
		}
	}

	public float CalcBlockedDistanceSq()
	{
		Vector3 pos = HitInfo.hit.pos;
		Vector3 position = entity.position;
		float num = pos.x - position.x;
		float num2 = pos.z - position.z;
		return num * num + num2 * num2;
	}

	public void ClearBlocked()
	{
		BlockedFlags = 0;
		BlockedFlagsAfterCrouch = 0;
		BlockedTime = 0f;
	}

	public void ClearTempMove()
	{
		isTempMove = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetStuckCheck()
	{
		SideStepAngle = 0f;
		moveToTicks = 0;
		moveToFailCnt = 0;
		if (isTempMove)
		{
			moveToDistance = CalcTempMoveDist();
		}
		else
		{
			moveToDistance = CalcMoveDist();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcMoveDist()
	{
		Vector3 position = entity.position;
		float num = moveToPos.x - position.x;
		float num2 = moveToPos.z - position.z;
		float num3 = moveToPos.y - position.y;
		return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcTempMoveDist()
	{
		Vector3 position = entity.position;
		float num = tempMoveToPos.x - position.x;
		float num2 = tempMoveToPos.z - position.z;
		float num3 = tempMoveToPos.y - position.y;
		return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalcBlockCenterXZ(Vector3 pos)
	{
		pos.x = (float)Utils.Fastfloor(pos.x) + 0.5f;
		pos.z = (float)Utils.Fastfloor(pos.z) + 0.5f;
		return pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalcBlockCenter(Vector3 pos)
	{
		pos.x = (float)Utils.Fastfloor(pos.x) + 0.5f;
		pos.y = (float)Utils.Fastfloor(pos.y) + 0.5f;
		pos.z = (float)Utils.Fastfloor(pos.z) + 0.5f;
		return pos;
	}

	public void CalcIfUnreachablePos()
	{
		IsUnreachableSideJump = false;
		if (entity.Jumping)
		{
			return;
		}
		IsUnreachableAbove = false;
		IsUnreachableSide = false;
		PathEntity path = entity.navigator.getPath();
		if (path == null)
		{
			return;
		}
		Vector3 toPos = path.toPos;
		Vector3 rawEndPos = path.rawEndPos;
		float num = rawEndPos.x - toPos.x;
		float num2 = rawEndPos.z - toPos.z;
		float num3 = num * num + num2 * num2;
		float num4 = toPos.y - rawEndPos.y;
		if (num4 > ccHeight + 0.7f && num3 < 25f)
		{
			IsUnreachableAbove = true;
			UnreachablePos = rawEndPos;
		}
		if (!(num4 >= -1.5f) || !(num3 >= 1.44f))
		{
			return;
		}
		IsUnreachableSide = true;
		UnreachablePos = rawEndPos;
		float jumpMaxDistance = entity.jumpMaxDistance;
		if (jumpMaxDistance > 0f && num4 < 0.5f + jumpMaxDistance * 0.5f)
		{
			jumpMaxDistance += 3.4f;
			if (num3 <= jumpMaxDistance * jumpMaxDistance)
			{
				IsUnreachableSideJump = true;
			}
		}
	}

	public bool IsMoveToAbove()
	{
		if (moveToPos.y - entity.position.y > 1.9f)
		{
			return true;
		}
		return false;
	}

	public bool FindExistingDestroyPos(ref Vector3 destroyPos)
	{
		if (GetExistingDestroyPos(ref destroyPos))
		{
			return true;
		}
		entity.world.GetEntitiesAround(EntityFlags.AISmelling, destroyPos, 20f, entityTempList);
		int count = entityTempList.Count;
		if (count > 1)
		{
			int num = random.RandomRange(count);
			for (int i = 0; i < count; i++)
			{
				EntityAlive entityAlive = (EntityAlive)entityTempList[(i + num) % count];
				if (entityAlive != entity && entityAlive.moveHelper != null && entityAlive.moveHelper.GetExistingDestroyPos(ref destroyPos))
				{
					entityTempList.Clear();
					return true;
				}
			}
			entityTempList.Clear();
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetExistingDestroyPos(ref Vector3 destroyPos)
	{
		if (destroyRefreshTicks > 0 && destroyPosition.y > 0f)
		{
			ChunkCluster chunkCache = entity.world.ChunkCache;
			Vector3i vector3i = World.worldToBlockPos(destroyPosition);
			BlockValue block = chunkCache.GetBlock(vector3i);
			Block block2 = block.Block;
			if (block2.IsMovementBlocked(entity.world, vector3i, block, BlockFace.None) && block2.StabilitySupport)
			{
				destroyPos = destroyPosition;
				return true;
			}
			destroyPosition.y = 0f;
		}
		return false;
	}

	public bool FindDestroyPos(ref Vector3 destroyPos, int destroyRadius, bool isLookFar)
	{
		destroyPosition.y = 0f;
		if (SearchForDestroyPos(ref destroyPos, destroyRadius, isLookFar))
		{
			destroyRefreshTicks = 500;
			destroyPosition = destroyPos;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SearchForDestroyPos(ref Vector3 destroyPos, int destroyRadius, bool isLookFar)
	{
		int num = int.MaxValue;
		Vector3i vector3i = Vector3i.zero;
		World world = entity.world;
		_ = world.ChunkCache;
		Vector3i vector3i2 = World.worldToBlockPos(destroyPos);
		int i = 0;
		int num2 = 1;
		if (isLookFar)
		{
			i = random.RandomRange(destroyRadius / 2, destroyRadius);
			num2 = -1;
			vector3i2.y -= 2;
		}
		int num3 = random.RandomRange(0, 4);
		int num4 = 0;
		vector3i2.y = Utils.FastMax(2, vector3i2.y);
		BlockValue[] array = new BlockValue[7];
		IChunk _chunk = null;
		Vector3i vector3i3 = default(Vector3i);
		for (; i >= 0 && i <= destroyRadius; i += num2)
		{
			int num5 = i * 2;
			for (int j = 0; j < 4; j++)
			{
				DestroyData destroyData = EntityMoveHelper.destroyData[j + num3];
				int num6 = destroyData.offsetX * i;
				int num7 = destroyData.offsetZ * i;
				vector3i3.x = vector3i2.x + num6;
				vector3i3.z = vector3i2.z + num7;
				int num8 = 0;
				do
				{
					world.GetChunkFromWorldPos(vector3i3.x, vector3i3.z, ref _chunk);
					if (_chunk != null)
					{
						_chunk.GetBlockColumn(World.toBlockXZ(vector3i3.x), vector3i2.y + -2, World.toBlockXZ(vector3i3.z), array);
						for (int k = -2; k <= 2; k++)
						{
							int num9 = k - -2;
							BlockValue blockValue = array[num9 + 1];
							if (blockValue.isair)
							{
								continue;
							}
							Block block = blockValue.Block;
							if (!block.StabilitySupport)
							{
								continue;
							}
							BlockValue blockValue2 = array[num9];
							if (blockValue2.isair)
							{
								continue;
							}
							Block block2 = blockValue2.Block;
							if (!block2.StabilitySupport)
							{
								continue;
							}
							vector3i3.y = vector3i2.y + k;
							int num10 = 0;
							int num11 = 0;
							int num12 = block2.MaxDamagePlusDowngrades - (blockValue2.damage & -128);
							if (block2.shape.IsTerrain())
							{
								num12 *= 51;
								num10++;
							}
							if (block.shape.IsTerrain())
							{
								num12 *= 2;
								num10++;
							}
							if (num10 == 0)
							{
								BlockValue blockValue3 = array[num9 + 2];
								if (!blockValue3.isair && blockValue3.Block.StabilitySupport)
								{
									num11++;
									num12 /= 2;
									if (num9 < 4)
									{
										BlockValue blockValue4 = array[num9 + 3];
										if (!blockValue4.isair)
										{
											num11++;
											num12 /= 4;
										}
									}
								}
							}
							if (num12 < num && (num4 == 0 || num10 < 2) && IsABlockSideOpen(vector3i3, ref _chunk))
							{
								num4 += num11;
								num = num12;
								vector3i = vector3i3;
							}
						}
					}
					vector3i3.x += destroyData.stepX;
					vector3i3.z += destroyData.stepZ;
				}
				while (++num8 < num5);
				if (i == 0)
				{
					break;
				}
			}
			if (num4 >= 2 && i >= 5)
			{
				break;
			}
		}
		if (num > 999999)
		{
			return false;
		}
		destroyPos = vector3i.ToVector3CenterXZ();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsABlockSideOpen(Vector3i _checkPos, ref IChunk _chunk)
	{
		World world = entity.world;
		Vector3i blockPos = _checkPos;
		for (int i = 0; i < 8; i += 2)
		{
			blockPos.x = _checkPos.x + blockOpenOffsets[i];
			blockPos.z = _checkPos.z + blockOpenOffsets[i + 1];
			world.GetChunkFromWorldPos(blockPos.x, blockPos.z, ref _chunk);
			if (_chunk != null)
			{
				BlockValue blockNoDamage = _chunk.GetBlockNoDamage(World.toBlockXZ(blockPos.x), blockPos.y, World.toBlockXZ(blockPos.z));
				if (!blockNoDamage.Block.IsMovementBlocked(world, blockPos, blockNoDamage, BlockFace.None))
				{
					return true;
				}
			}
		}
		return false;
	}
}
