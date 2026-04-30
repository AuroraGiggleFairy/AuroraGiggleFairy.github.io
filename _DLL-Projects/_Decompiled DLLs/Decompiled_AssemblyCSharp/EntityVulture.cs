using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityVulture : EntityFlying
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum State
	{
		Attack,
		AttackReposition,
		AttackStop,
		Home,
		Stun,
		WanderStart,
		Wander
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFlyingMinimumSpeed = 0.02f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTargetDistanceClose = 0.9f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTargetDistanceMax = 80f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTargetAttackOffsetY = -0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cVomitMinRange = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAttackDelay = 18;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBattleFatigueMin = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBattleFatigueMax = 60f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBattleFatigueCooldownMin = 80f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBattleFatigueCooldownMax = 180f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int moveUpdateDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float motionReverseScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 waypoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCircling;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 circleCenter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float circleReverseScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float glidingCurrentPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float glidingPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float accel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 wanderHeightRange = new Vector2(10f, 30f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive currentTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float targetAttackHealthPercent = 0.8f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ignoreTargetAttached;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int targetSwitchDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int homeCheckDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int homeSeekDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int wanderChangeDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int checkBlockedDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float battleDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float battleFatigueSeconds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBattleFatigued;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attackDelay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attackCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attack2Delay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAttack2On;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EAISetNearestEntityAsTargetSorter sorter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> list = new List<Entity>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public State state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stateMaxTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Bounds> collBB = new List<Bounds>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			component.center = new Vector3(0f, 0.35f, 0f);
			component.size = new Vector3(0.4f, 0.4f, 0.4f);
		}
		base.Awake();
		state = State.WanderStart;
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		Init();
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
		Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Init()
	{
		if (navigator != null)
		{
			navigator.setCanDrown(_b: true);
		}
		battleFatigueSeconds = rand.RandomRange(30f, 60f);
	}

	public override void SetSleeper()
	{
		base.SetSleeper();
		sorter = new EAISetNearestEntityAsTargetSorter(this);
		setHomeArea(new Vector3i(position), (int)sleeperSightRange + 1);
		battleFatigueSeconds = float.MaxValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
		{
			aiManager.UpdateDebugName();
		}
		else
		{
			if (GameStats.GetInt(EnumGameStats.GameState) == 2)
			{
				return;
			}
			CheckDespawn();
			GetEntitySenses().ClearIfExpired();
			if (IsSleeperPassive)
			{
				return;
			}
			if (IsSleeping)
			{
				float seeDistance = GetSeeDistance();
				world.GetEntitiesInBounds(typeof(EntityPlayer), BoundsUtils.ExpandBounds(boundingBox, seeDistance, seeDistance, seeDistance), list);
				list.Sort(sorter);
				EntityPlayer entityPlayer = null;
				float num = float.MaxValue;
				if (noisePlayer != null && noisePlayerVolume >= sleeperNoiseToWake)
				{
					entityPlayer = noisePlayer;
					num = noisePlayerDistance;
				}
				for (int i = 0; i < list.Count; i++)
				{
					EntityPlayer entityPlayer2 = (EntityPlayer)list[i];
					if (CanSee(entityPlayer2))
					{
						float distance = GetDistance(entityPlayer2);
						if (GetSleeperDisturbedLevel(distance, entityPlayer2.Stealth.lightLevel) >= 2 && distance < num)
						{
							entityPlayer = entityPlayer2;
							num = distance;
						}
					}
				}
				list.Clear();
				if (entityPlayer == null)
				{
					return;
				}
				ConditionalTriggerSleeperWakeUp();
				SetAttackTarget(entityPlayer, 1200);
			}
			bool flag = Buffs.HasBuff("buffShocked");
			if (flag)
			{
				SetState(State.Stun);
			}
			else
			{
				EntityAlive revengeTarget = GetRevengeTarget();
				if ((bool)revengeTarget)
				{
					battleDuration = 0f;
					isBattleFatigued = false;
					SetRevengeTarget(null);
					if (revengeTarget != attackTarget && (!attackTarget || rand.RandomFloat < 0.5f))
					{
						SetAttackTarget(revengeTarget, 1200);
					}
				}
				if (attackTarget != currentTarget)
				{
					currentTarget = attackTarget;
					if ((bool)currentTarget)
					{
						SetState(State.Attack);
						waypoint = position;
						moveUpdateDelay = 0;
						homeCheckDelay = 400;
					}
					else
					{
						SetState(State.AttackStop);
					}
				}
			}
			float sqrMagnitude = (waypoint - position).sqrMagnitude;
			stateTime += 0.05f;
			switch (state)
			{
			case State.Attack:
				battleDuration += 0.05f;
				break;
			case State.AttackReposition:
				if (sqrMagnitude < 2.25f || stateTime >= stateMaxTime)
				{
					SetState(State.Attack);
					motion *= -0.2f;
					motion.y = 0f;
				}
				break;
			case State.AttackStop:
				ClearTarget();
				SetState(State.WanderStart);
				break;
			case State.Home:
				if (sqrMagnitude < 4f || stateTime > 30f)
				{
					SetState(State.WanderStart);
				}
				else if (--homeSeekDelay <= 0)
				{
					homeSeekDelay = 40;
					int minXZ = 10;
					if (stateTime > 20f)
					{
						minXZ = -20;
					}
					int num2 = getMaximumHomeDistance();
					Vector3 vector2 = RandomPositionGenerator.CalcTowards(this, minXZ, 30, num2 / 2, getHomePosition().position.ToVector3());
					if (!vector2.Equals(Vector3.zero))
					{
						waypoint = vector2;
						AdjustWaypoint();
					}
				}
				break;
			case State.Stun:
			{
				Animator componentInChildren = ModelTransform.GetComponentInChildren<Animator>();
				if (flag)
				{
					motion = rand.RandomOnUnitSphere * -0.075f;
					motion.y += -0.060000002f;
					if ((bool)componentInChildren)
					{
						componentInChildren.enabled = false;
					}
					return;
				}
				if ((bool)componentInChildren)
				{
					componentInChildren.enabled = true;
				}
				SetState(State.WanderStart);
				break;
			}
			case State.WanderStart:
			{
				homeCheckDelay = 60;
				if (!isWithinHomeDistanceCurrentPosition())
				{
					StartHome(getHomePosition().position.ToVector3());
					break;
				}
				SetState(State.Wander);
				isCircling = !IsSleeper && rand.RandomFloat < 0.4f;
				float num3 = position.y;
				if (Physics.Raycast(position - Origin.position, Vector3.down, out var hitInfo, 999f, 65536))
				{
					float num4 = rand.RandomRange(wanderHeightRange.x, wanderHeightRange.y);
					if (IsSleeper)
					{
						num4 *= 0.4f;
					}
					num3 += 0f - hitInfo.distance + num4;
				}
				else
				{
					isCircling = false;
				}
				bool flag2 = false;
				EntityPlayer entityPlayer4 = null;
				if (!isBattleFatigued)
				{
					entityPlayer4 = world.GetClosestPlayerSeen(this, 80f, 1f);
					if ((bool)entityPlayer4 && GetDistanceSq(entityPlayer4) > 400f)
					{
						flag2 = true;
					}
				}
				if (isCircling)
				{
					wanderChangeDelay = 120;
					Vector3 right = base.transform.right;
					right.y = 0f;
					circleReverseScale = 1f;
					if (rand.RandomFloat < 0.5f)
					{
						circleReverseScale = -1f;
						right.x = 0f - right.x;
						right.z = 0f - right.z;
					}
					circleCenter = position + right * (3f + rand.RandomFloat * 7f);
					circleCenter.y = num3;
					if (flag2)
					{
						circleCenter.x = circleCenter.x * 0.6f + entityPlayer4.position.x * 0.4f;
						circleCenter.z = circleCenter.z * 0.6f + entityPlayer4.position.z * 0.4f;
					}
				}
				else
				{
					wanderChangeDelay = 400;
					waypoint = position;
					waypoint.x += rand.RandomFloat * 16f - 8f;
					waypoint.y = num3;
					waypoint.z += rand.RandomFloat * 16f - 8f;
					if (flag2)
					{
						waypoint.x = waypoint.x * 0.6f + entityPlayer4.position.x * 0.4f;
						waypoint.z = waypoint.z * 0.6f + entityPlayer4.position.z * 0.4f;
					}
					AdjustWaypoint();
				}
				break;
			}
			case State.Wander:
				if (isBattleFatigued)
				{
					battleDuration -= 0.05f;
					if (battleDuration <= 0f)
					{
						isBattleFatigued = false;
					}
				}
				if (--wanderChangeDelay <= 0)
				{
					SetState(State.WanderStart);
				}
				if (isCircling)
				{
					Vector3 vector = circleCenter - position;
					float x = vector.x;
					vector.x = (0f - vector.z) * circleReverseScale;
					vector.z = x * circleReverseScale;
					vector.y = 0f;
					waypoint = position + vector;
				}
				else if (sqrMagnitude < 1f)
				{
					SetState(State.WanderStart);
				}
				if (--targetSwitchDelay > 0)
				{
					break;
				}
				targetSwitchDelay = 40;
				if (IsSleeper || !(rand.RandomFloat < 0.5f))
				{
					EntityPlayer entityPlayer3 = FindTarget();
					if ((bool)entityPlayer3)
					{
						SetAttackTarget(entityPlayer3, 1200);
					}
				}
				break;
			}
			if (state != State.Home && --homeCheckDelay <= 0)
			{
				homeCheckDelay = 60;
				if (!isWithinHomeDistanceCurrentPosition())
				{
					SetState(State.AttackStop);
				}
			}
			if (--moveUpdateDelay <= 0)
			{
				moveUpdateDelay = 4 + rand.RandomRange(5);
				if ((bool)currentTarget && state == State.Attack)
				{
					waypoint = currentTarget.getHeadPosition();
					waypoint.y += -0.1f;
					if ((bool)currentTarget.AttachedToEntity)
					{
						waypoint += currentTarget.GetVelocityPerSecond() * 0.3f;
					}
					else
					{
						waypoint += currentTarget.GetVelocityPerSecond() * 0.1f;
					}
					Vector3 vector3 = waypoint - position;
					vector3.y = 0f;
					vector3.Normalize();
					waypoint += vector3 * -0.6f;
				}
				if (!IsCourseTraversable(waypoint, out var _))
				{
					waypoint.y += 2f;
					if (state == State.Attack)
					{
						if (rand.RandomFloat < 0.1f)
						{
							StartAttackReposition();
						}
					}
					else if (state != State.Home && state != State.AttackReposition)
					{
						SetState(State.WanderStart);
					}
				}
			}
			Vector3 vector4 = waypoint - position;
			float magnitude = vector4.magnitude;
			Vector3 vector5 = vector4 * (1f / magnitude);
			glidingPercent = 0f;
			if (vector5.y > 0.57f)
			{
				accel = 0.35f;
			}
			else if (vector5.y < -0.34f)
			{
				accel = 0.95f;
				glidingPercent = 1f;
			}
			else
			{
				accel = 0.55f;
				if (state == State.Home || state == State.Wander)
				{
					accel = 0.8f;
					if (isCircling)
					{
						glidingPercent = 1f;
					}
				}
			}
			if (attackDelay > 0)
			{
				glidingPercent = 0f;
			}
			if ((bool)currentTarget && (bool)currentTarget.AttachedToEntity && !ignoreTargetAttached)
			{
				if (IsBloodMoon && accel > 0.5f)
				{
					accel = 2.5f;
				}
				accel *= moveSpeedAggro;
			}
			else
			{
				accel *= moveSpeed;
			}
			motion = motion * 0.9f + vector5 * (accel * 0.1f);
			if ((bool)emodel.avatarController)
			{
				glidingCurrentPercent = Mathf.MoveTowards(glidingCurrentPercent, glidingPercent, 0.060000002f);
				emodel.avatarController.UpdateFloat("Gliding", glidingCurrentPercent);
			}
			if (attackDelay > 0)
			{
				attackDelay--;
			}
			if (attack2Delay > 0)
			{
				attack2Delay--;
			}
			float num5 = Mathf.Atan2(motion.x * motionReverseScale, motion.z * motionReverseScale) * 57.29578f;
			if ((bool)currentTarget && --targetSwitchDelay <= 0)
			{
				targetSwitchDelay = 60;
				if (state != State.AttackStop)
				{
					EntityPlayer entityPlayer5 = FindTarget();
					if ((bool)entityPlayer5 && entityPlayer5 != attackTarget)
					{
						SetAttackTarget(entityPlayer5, 400);
					}
				}
				float num6 = (currentTarget.AttachedToEntity ? 0.1f : 0.25f);
				if (state != State.AttackReposition && rand.RandomFloat < num6)
				{
					StartAttackReposition();
				}
			}
			if ((bool)currentTarget)
			{
				Vector3 headPosition = currentTarget.getHeadPosition();
				headPosition += currentTarget.GetVelocityPerSecond() * 0.1f;
				Vector3 vector6 = headPosition - position;
				float sqrMagnitude2 = vector6.sqrMagnitude;
				if ((sqrMagnitude2 > 6400f && !IsBloodMoon) || currentTarget.IsDead())
				{
					SetState(State.AttackStop);
				}
				else if (state != State.AttackReposition)
				{
					if (sqrMagnitude2 < 4f)
					{
						num5 = Mathf.Atan2(vector6.x, vector6.z) * 57.29578f;
					}
					if (attackDelay <= 0 && !isAttack2On)
					{
						if (sqrMagnitude2 < 0.80999994f && position.y >= currentTarget.position.y && position.y < headPosition.y + 0.1f)
						{
							AttackAndAdjust(isBlock: false);
						}
						else if (checkBlockedDelay > 0)
						{
							checkBlockedDelay--;
						}
						else
						{
							checkBlockedDelay = 6;
							Vector3 normalized = vector6.normalized;
							if (Voxel.Raycast(ray: new Ray(position + new Vector3(0f, 0.22f, 0f) - normalized * 0.13f, normalized), _world: world, distance: 0.83f, _layerMask: 1082195968, _hitMask: 128, _sphereRadius: 0.13f))
							{
								AttackAndAdjust(isBlock: true);
							}
						}
					}
					bool flag3 = false;
					ItemActionVomit.ItemActionDataVomit itemActionDataVomit = inventory.holdingItemData.actionData[1] as ItemActionVomit.ItemActionDataVomit;
					if (itemActionDataVomit != null && attack2Delay <= 0 && sqrMagnitude2 >= 9f)
					{
						float range = ((ItemActionRanged)inventory.holdingItem.Actions[1]).GetRange(itemActionDataVomit);
						if (sqrMagnitude2 < range * range && Utils.FastAbs(Utils.DeltaAngle(num5, rotation.y)) < 20f && Utils.FastAbs(Vector3.SignedAngle(vector6, base.transform.forward, Vector3.right)) < 25f)
						{
							flag3 = true;
						}
					}
					if (!isAttack2On && flag3)
					{
						isAttack2On = true;
						itemActionDataVomit.muzzle = emodel.GetHeadTransform();
						itemActionDataVomit.numWarningsPlayed = 999;
					}
					if (isAttack2On)
					{
						if (!flag3)
						{
							isAttack2On = false;
						}
						else
						{
							motion *= 0.7f;
							SetLookPosition(headPosition);
							UseHoldingItem(1, _isReleased: false);
							if (!itemActionDataVomit.isActive)
							{
								isAttack2On = false;
							}
						}
						if (!isAttack2On)
						{
							if (itemActionDataVomit.numVomits > 0)
							{
								StartAttackReposition();
							}
							UseHoldingItem(1, _isReleased: true);
							attack2Delay = 60;
							SetLookPosition(Vector3.zero);
						}
					}
				}
			}
			float magnitude2 = motion.magnitude;
			if (magnitude2 < 0.02f)
			{
				motion *= 1f / magnitude2 * 0.02f;
			}
			SeekYaw(num5, 0f, 20f);
			aiManager.UpdateDebugName();
		}
	}

	public override string MakeDebugNameInfo()
	{
		return string.Format("\n{0} {1}\nWaypoint {2}\nTarget {3}, AtkDelay {4}, BtlTime {5}\nSpeed {6}, Motion {7}, Accel {8}", state.ToStringCached(), stateTime.ToCultureInvariantString("0.00"), waypoint.ToCultureInvariantString(), currentTarget ? currentTarget.name : "", attackDelay, battleDuration.ToCultureInvariantString("0.00"), motion.magnitude.ToCultureInvariantString("0.000"), motion.ToCultureInvariantString("0.000"), accel.ToCultureInvariantString("0.000"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetState(State newState)
	{
		state = newState;
		stateTime = 0f;
		motionReverseScale = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AdjustWaypoint()
	{
		int num = 255;
		Vector3i pos = new Vector3i(waypoint);
		while (!world.GetBlock(pos).isair && --num >= 0)
		{
			waypoint.y += 1f;
			pos.y++;
		}
		waypoint.y = Mathf.Min(waypoint.y, 250f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartAttackReposition()
	{
		if (!IsBloodMoon && battleDuration >= battleFatigueSeconds)
		{
			ClearTarget();
			battleDuration = rand.RandomRange(80f, 180f);
			isBattleFatigued = true;
			SetState(State.Wander);
			return;
		}
		SetState(State.AttackReposition);
		stateMaxTime = rand.RandomRange(0.8f, 5f);
		attackCount = 0;
		waypoint = position;
		waypoint.x += rand.RandomFloat * 8f - 4f;
		waypoint.y += rand.RandomFloat * 4f + 3f;
		waypoint.z += rand.RandomFloat * 8f - 4f;
		moveUpdateDelay = 0;
		motion = -motion;
		if (rand.RandomFloat < 0.5f)
		{
			motionReverseScale = -1f;
			motion.y = 0.2f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StartHome(Vector3 _homePos)
	{
		SetState(State.Home);
		homeSeekDelay = 0;
		waypoint = _homePos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTarget()
	{
		SetAttackTarget(null, 0);
		SetRevengeTarget(null);
		currentTarget = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer FindTarget()
	{
		EntityPlayer entityPlayer;
		if (IsBloodMoon)
		{
			entityPlayer = world.GetClosestPlayerSeen(this, -1f, 0f);
			if (!entityPlayer)
			{
				entityPlayer = world.GetClosestPlayer(this, -1f, _isDead: false);
			}
			return entityPlayer;
		}
		float lightMin = 26f;
		entityPlayer = world.GetClosestPlayerSeen(this, 80f, lightMin);
		if (!entityPlayer || entityPlayer.inWaterPercent >= 0.6f)
		{
			entityPlayer = noisePlayer;
		}
		if ((bool)entityPlayer)
		{
			if (isBattleFatigued)
			{
				return null;
			}
			float num = (float)entityPlayer.Health / entityPlayer.Stats.Health.ModifiedMax;
			if (IsSleeper || num <= targetAttackHealthPercent)
			{
				return entityPlayer;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _v, Vector3 _fallMotion)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	public override float GetEyeHeight()
	{
		return 0.3f;
	}

	public override Vector3 GetLookVector()
	{
		if (lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return lookAtPosition - getHeadPosition();
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		Entity entity = world.GetEntity(_sourceEntityId);
		if ((bool)entity && entity.entityClass == entityClass)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackAndAdjust(bool isBlock)
	{
		if (UseHoldingItem(0, _isReleased: false))
		{
			UseHoldingItem(0, _isReleased: true);
			attackDelay = 18;
			isCircling = false;
			if ((bool)currentTarget.AttachedToEntity)
			{
				motion *= 0.7f;
			}
			else
			{
				motion *= 0.6f;
			}
			if (++attackCount >= 5 || rand.RandomFloat < 0.25f)
			{
				StartAttackReposition();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCourseTraversable(Vector3 _pos, out float _distance)
	{
		float num = _pos.x - position.x;
		float num2 = _pos.y - position.y;
		float num3 = _pos.z - position.z;
		_distance = Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
		if (_distance < 1.5f)
		{
			return true;
		}
		num /= _distance;
		num2 /= _distance;
		num3 /= _distance;
		Bounds aabb = boundingBox;
		collBB.Clear();
		for (int i = 1; (float)i < _distance - 1f; i++)
		{
			aabb.center += new Vector3(num, num2, num3);
			world.GetCollidingBounds(this, aabb, collBB);
			if (collBB.Count > 0)
			{
				return false;
			}
		}
		return true;
	}
}
