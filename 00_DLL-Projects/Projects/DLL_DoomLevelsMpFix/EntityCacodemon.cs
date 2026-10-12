using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The body stays level and only turns left and right. It flies a straight
	/// line toward one meter above its target, ducking, climbing, or going
	/// around when something is in the way. A high blockage is passed underneath
	/// when the gap fits the body. The ranged line on the entity sets the shot.
	/// The projectile aims at the chest.
	/// </summary>
	[Preserve]
	public class EntityCacodemon : EntityFlying
	{
		private enum Mode
		{
			Drift,
			Windup
		}

		private const float MetersPerSpeed = 6f;

		private const float Tick = 0.05f;

		private const float AlwaysSpitMeters = 6f;

		private const float TouchMeters = 1.9f;

		private const float HoverAbove = 1f;

		private const float CruiseUp = 10f;

		private const float GroundClearance = 1f;

		private const float RingRadius = 1.35f;

		private const float SearchRadius = 28f;

		private const int ShotPauseTicks = 20;

		private const int ShotFireTick = 10;

		private static readonly FastTags<TagGroup.Global> DemonTag = FastTags<TagGroup.Global>.Parse("demon");

		private static readonly AccessTools.FieldRef<ItemActionVomit, string> WarningSound =
			AccessTools.FieldRefAccess<ItemActionVomit, string>("soundWarning");

		private readonly List<Bounds> hits = new List<Bounds>();
		private readonly List<Bounds> held = new List<Bounds>();

		private readonly List<Entity> nearby = new List<Entity>();

		private Vector3 cruiseHome;

		private Vector3 cruisePoint;

		private int cruiseTicks;

		private bool cruiseReady;

		private int avoidSide;

		private int avoidHold;

		private int detourHold;

		private int detourSide = 1;

		private bool detourDrop;

		private bool shotReady;

		private float shotCooldown = 3f;

		private float shotDuration = 3f;

		private float shotMin = 2f;

		private float shotMax = 40f;

		private string shotSound = "";

		private Vector3 steer;

		private float ringAngle;

		private int ringTargetId = -1;

		private const float HearMeters = 50f;

		private const float SearchSeconds = 60f;

		private Vector3 lastSeenPos;

		private bool hasLastSeen;

		private float searchUntil;

		private Mode mode;

		private int windup;

		private bool shotReleased;

		private int attackWait;

		private int biteWait;

		private bool painCancel;

		public override Vector3 GetLookVector()
		{
			if (lookAtPosition.sqrMagnitude < 0.01f)
			{
				return base.GetLookVector();
			}

			Vector3 to = lookAtPosition - getHeadPosition();
			if (to.sqrMagnitude < 0.0001f)
			{
				return base.GetLookVector();
			}

			return to.normalized;
		}

		public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
		{
			if (isEntityRemote || AttachedToEntity != null)
			{
				base.MoveEntityHeaded(_direction, _isDirAbsolute);
				return;
			}

			if (IsDead())
			{
				entityCollision(motion);
				motion.y -= 0.08f;
				motion.y *= 0.98f;
				motion.x *= 0.91f;
				motion.z *= 0.91f;
				return;
			}

			fallDistance = 0f;
			onGround = false;
			bool clip = HeadInCeiling();
			bool chasing = (bool)attackTarget && !attackTarget.IsDead();
			Vector3 unpack = Unpack();
			if (clip && CanDrop())
			{
				motion.y = Mathf.Min(motion.y, -Mathf.Max(StepLength(), 0.25f));
				motion.x += unpack.x * 0.35f;
				motion.z += unpack.z * 0.35f;
			}
			else if (chasing && mode != Mode.Windup)
			{
				motion.x += unpack.x * 0.35f;
				motion.z += unpack.z * 0.35f;
			}
			else if (unpack.sqrMagnitude > 0.0001f)
			{
				motion = unpack;
			}
			else if (!clip)
			{
				Gap here = Measure(position.x, boundingBox.min.y, position.z);
				if (here.HasFloor && boundingBox.min.y < here.Floor + 0.35f)
				{
					float lift = StepLength();
					if (here.HasCeiling)
					{
						float room = here.Ceiling - boundingBox.max.y;
						if (room < lift)
						{
							lift = Mathf.Max(0f, room - 0.05f);
						}
					}

					if (lift > 0f)
					{
						motion.y = Mathf.Max(motion.y, lift);
					}
				}
			}

			entityCollision(motion);
			if (clip && CanDrop() && HeadInCeiling())
			{
				SetPosition(position + Vector3.down * Mathf.Max(StepLength(), 0.2f), true);
			}
		}

		public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
		{
			int applied = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
			if (applied >= 0 && mode == Mode.Windup && rand.RandomFloat < 0.5f)
			{
				painCancel = true;
			}

			return applied;
		}

		public override void updateTasks()
		{
			if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
			{
				motion = Vector3.zero;
				return;
			}

			CheckDespawn();
			seeCache.ClearIfExpired();
			if (IsDead())
			{
				return;
			}

			EntityAlive target = Acquire();
			if (IsSleeping)
			{
				motion = Vector3.zero;
				return;
			}

			if (target == null)
			{
				mode = Mode.Drift;
				SetLookPosition(Vector3.zero);
				if (Searching())
				{
					SearchLastSeen();
				}
				else if (HasInvestigatePosition)
				{
					FlyTo(InvestigatePosition);
				}
				else if (Searches())
				{
					Cruise();
				}
				else
				{
					motion = Vector3.zero;
				}

				return;
			}

			Face(target);
			if (mode == Mode.Windup)
			{
				motion = Vector3.zero;
				windup--;
				if (painCancel || target.IsDead())
				{
					mode = Mode.Drift;
					shotReleased = false;
					attackWait = ShotGap();
					return;
				}

				if (!shotReleased && windup <= ShotFireTick && InShotRange(target))
				{
					Spit();
					shotReleased = true;
				}

				if (windup > 0)
				{
					return;
				}

				mode = Mode.Drift;
				shotReleased = false;
				attackWait = ShotGap();
				return;
			}

			if (attackWait > 0)
			{
				attackWait--;
			}

			if (biteWait > 0)
			{
				biteWait--;
			}

			Drift(target);
			if (Touching(target) && biteWait <= 0)
			{
				Bite(target);
				biteWait = BiteTicks();
			}

			if (attackWait > 0 || !CanSee(target))
			{
				return;
			}

			if (InShotRange(target) && WantsShot(FlatMeters(target)) && !Touching(target))
			{
				mode = Mode.Windup;
				windup = ShotPauseTicks;
				shotReleased = false;
				painCancel = false;
				motion = Vector3.zero;
			}
			else if (InShotRange(target))
			{
				attackWait = rand.RandomRange(8, 21);
			}
		}

		private EntityAlive Acquire()
		{
			EntityAlive revenge = GetRevengeTarget();
			if ((bool)revenge && !revenge.IsDead())
			{
				SetRevengeTarget(null);
				if (IsSleeping)
				{
					ConditionalTriggerSleeperWakeUp();
				}

				if (revenge is EntityPlayer)
				{
					Remember(revenge);
					ClearInvestigatePosition();
					SetAttackTarget(revenge, 600);
					return revenge;
				}

				SetAttackTarget(revenge, 400);
			}

			if ((bool)attackTarget && !attackTarget.IsDead() && !(attackTarget is EntityPlayer))
			{
				if (attackTargetTime < 80)
				{
					SetAttackTarget(attackTarget, 400);
				}

				return attackTarget;
			}

			if ((IsSleeping || IsSleeper) && (bool)noisePlayer && noisePlayerVolume >= sleeperNoiseToWake)
			{
				ConditionalTriggerSleeperWakeUp();
				Remember(noisePlayer);
				SetAttackTarget(noisePlayer, 600);
			}

			EntityPlayer seen = SensedPlayer();
			if ((bool)seen)
			{
				if (IsSleeping)
				{
					ConditionalTriggerSleeperWakeUp();
				}

				Remember(seen);
				searchUntil = 0f;
				ClearInvestigatePosition();
				SetAttackTarget(seen, 600);
			}
			else if (attackTarget is EntityPlayer player && !player.IsDead())
			{
				LoseSight(player);
			}
			else if (hasLastSeen && (bool)noisePlayer && !noisePlayer.IsDead() && Hears(noisePlayer))
			{
				Remember(noisePlayer);
				searchUntil = Time.time + SearchSeconds;
			}

			if ((bool)attackTarget && attackTarget.IsDead())
			{
				SetAttackTarget(null, 0);
			}

			return attackTarget;
		}

		private void Remember(EntityAlive seen)
		{
			lastSeenPos = seen.position;
			hasLastSeen = true;
		}

		private EntityPlayer SensedPlayer()
		{
			EntityPlayer seen = null;
			float bestSq = GetSeeDistance();
			bestSq *= bestSq;
			for (int i = world.Players.list.Count - 1; i >= 0; i--)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null || player.IsDead())
				{
					continue;
				}

				float distSq = (player.position - position).sqrMagnitude;
				if (distSq <= bestSq && !WorldBlocks(player))
				{
					seen = player;
					bestSq = distSq;
				}
			}

			if ((bool)seen)
			{
				return seen;
			}

			if ((bool)noisePlayer && !noisePlayer.IsDead() && Hears(noisePlayer))
			{
				return noisePlayer;
			}

			return null;
		}

		private bool Hears(EntityPlayer player)
		{
			if (noisePlayer != player || noisePlayerVolume <= 0f)
			{
				return false;
			}

			if (!hasLastSeen && attackTarget != player)
			{
				return false;
			}

			float hear = HearMeters * senseScale * player.DetectUsScale(this);
			return (player.position - position).sqrMagnitude <= hear * hear;
		}

		private bool WorldBlocks(EntityPlayer player)
		{
			Vector3 from = getHeadPosition() - Origin.position;
			Vector3 to = player.getChestPosition() - Origin.position;
			Vector3 delta = to - from;
			float dist = delta.magnitude;
			if (dist < 0.3f)
			{
				return false;
			}

			if (!Voxel.Raycast(world, new Ray(from, delta / dist), dist, false, false))
			{
				return false;
			}

			float hit = Mathf.Sqrt(Voxel.voxelRayHitInfo.hit.distanceSq);
			return hit < dist - 0.75f;
		}

		private bool Searching()
		{
			return hasLastSeen && Time.time < searchUntil;
		}

		private void LoseSight(EntityPlayer player)
		{
			if (!hasLastSeen)
			{
				Remember(player);
			}

			if (searchUntil < Time.time)
			{
				searchUntil = Time.time + SearchSeconds;
			}

			SetAttackTarget(null, 0);
		}

		private void SearchLastSeen()
		{
			FlyTo(lastSeenPos);
		}

		private void FlyTo(Vector3 point)
		{
			Vector3 flat = point - position;
			flat.y = 0f;
			if (flat.sqrMagnitude > 0.25f)
			{
				SeekYaw(Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg, 0f, 12f);
			}

			if ((point - position).sqrMagnitude < 2.25f)
			{
				motion = Ease(Vector3.zero, StepLength());
				return;
			}

			motion = StepToward(point + Vector3.up * HoverAbove);
		}

		private bool Searches()
		{
			if (IsSleeper)
			{
				return false;
			}

			EnumSpawnerSource source = GetSpawnerSource();
			return source == EnumSpawnerSource.Biome || source == EnumSpawnerSource.Dynamic;
		}

		private void Cruise()
		{
			if (!cruiseReady)
			{
				cruiseHome = position;
				cruisePoint = position;
				cruiseReady = true;
			}

			Vector3 gap = cruisePoint - position;
			gap.y = 0f;
			if (--cruiseTicks <= 0 || gap.sqrMagnitude < 4f)
			{
				cruiseTicks = rand.RandomRange(80, 160);
				float angle = rand.RandomFloat * Mathf.PI * 2f;
				float dist = 8f + rand.RandomFloat * (SearchRadius - 8f);
				float x = cruiseHome.x + Mathf.Cos(angle) * dist;
				float z = cruiseHome.z + Mathf.Sin(angle) * dist;
				cruisePoint = new Vector3(x, GroundAt(x, z) + CruiseUp, z);
			}

			motion = StepToward(cruisePoint);
			Vector3 flat = cruisePoint - position;
			flat.y = 0f;
			if (flat.sqrMagnitude > 0.25f)
			{
				SeekYaw(Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg, 0f, 12f);
			}
		}

		private void Face(EntityAlive target)
		{
			Vector3 to = target.position - position;
			float yaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
			SeekYaw(yaw, 0f, 20f);
			SetLookPosition(target.getChestPosition());
		}

		private void Drift(EntityAlive target)
		{
			if (Touching(target))
			{
				motion = Ease(Vector3.zero, StepLength());
				return;
			}

			motion = StepToward(ApproachPoint(target));
		}

		private Vector3 ApproachPoint(EntityAlive target)
		{
			Vector3 center = target.position;
			float flatDist = FlatMeters(target);
			if (flatDist > 16f)
			{
				return center;
			}

			if (target.entityId != ringTargetId)
			{
				ringTargetId = target.entityId;
				ringAngle = (Mathf.Abs(entityId) * 2.39996314f) % (Mathf.PI * 2f);
			}

			float slot = SeparateSlot(target, ringAngle);
			Vector3 radial = new Vector3(Mathf.Sin(slot), 0f, Mathf.Cos(slot));
			Vector3 slotPoint = center + radial * RingRadius;
			float blend = Mathf.InverseLerp(16f, 6f, flatDist);
			return Vector3.Lerp(center, slotPoint, blend);
		}

		private float SeparateSlot(EntityAlive target, float slot)
		{
			Gather();
			float push = 0f;
			for (int i = 0; i < nearby.Count; i++)
			{
				EntityAlive other = nearby[i] as EntityAlive;
				if (!IsDemon(other))
				{
					continue;
				}

				Vector3 rel = other.position - target.position;
				rel.y = 0f;
				if (rel.sqrMagnitude < 0.25f || rel.sqrMagnitude > 64f)
				{
					continue;
				}

				float otherAng = Mathf.Atan2(rel.x, rel.z) * Mathf.Rad2Deg;
				float delta = Mathf.DeltaAngle(otherAng, slot * Mathf.Rad2Deg);
				float gap = Mathf.Abs(delta);
				if (gap < 36f)
				{
					float sign = gap < 0.5f ? (entityId > other.entityId ? 1f : -1f) : Mathf.Sign(delta);
					push += sign * (36f - gap) * 0.015f;
				}
			}

			float desired = slot * Mathf.Rad2Deg + push;
			ringAngle = Mathf.MoveTowardsAngle(slot * Mathf.Rad2Deg, desired, 1.2f) * Mathf.Deg2Rad;
			return ringAngle;
		}

		private Vector3 Unpack()
		{
			Gather();
			Vector3 push = Vector3.zero;
			int crowded = 0;
			for (int i = 0; i < nearby.Count; i++)
			{
				EntityCacodemon other = nearby[i] as EntityCacodemon;
				if (other == null || other == this || other.IsDead())
				{
					continue;
				}

				Vector3 away = position - other.position;
				away.y = 0f;
				float dist = away.magnitude;
				if (dist > 1.05f)
				{
					continue;
				}

				if (dist < 0.05f)
				{
					float angle = (Mathf.Abs(entityId) * 2.39996314f) % (Mathf.PI * 2f);
					away = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
					dist = 1f;
				}

				push += away / dist * (1.05f - Mathf.Min(dist, 1.05f));
				crowded++;
			}

			if (crowded == 0 || push.sqrMagnitude < 0.0001f)
			{
				return Vector3.zero;
			}

			float step = StepLength();
			push.y = 0f;
			Vector3 shove = push.normalized * step;
			if (!HeadInCeiling())
			{
				Gap gap = Measure(position.x, boundingBox.min.y, position.z);
				if (!gap.HasCeiling || boundingBox.max.y < gap.Ceiling - 0.45f)
				{
					shove.y = step * 0.45f;
				}
			}

			return shove;
		}

		private float StepLength()
		{
			return Mathf.Max(0.05f, moveSpeed) * MetersPerSpeed * Tick;
		}

		private Vector3 StepToward(Vector3 goal)
		{
			float stepLen = StepLength();
			Vector3 to = goal - position;
			float dist = to.magnitude;
			if (dist < 0.2f)
			{
				return Ease(Vector3.zero, stepLen);
			}

			Vector3 dir = to / dist;
			Vector3 flat = new Vector3(dir.x, 0f, dir.z);
			if (flat.sqrMagnitude > 0.0001f)
			{
				flat.Normalize();
			}

			Vector3 side = new Vector3(-flat.z, 0f, flat.x);
			if (DemonInWay(flat))
			{
				if (avoidHold <= 0)
				{
					avoidSide = PickSide(side);
					avoidHold = 40;
				}
				else
				{
					avoidHold--;
				}

				dir = (dir + side * (avoidSide * 0.45f)).normalized;
			}
			else
			{
				avoidHold = 0;
			}

			float arrive = Mathf.Clamp01(dist / 2.4f);
			float mag = Mathf.Min(stepLen, dist) * Mathf.Lerp(0.4f, 1f, arrive);
			Vector3 wish = dir * mag;
			bool dropping = false;
			if (HeadInCeiling() && CanDrop())
			{
				Vector3 flatDrop = new Vector3(dir.x, 0f, dir.z);
				if (flatDrop.sqrMagnitude > 0.0001f)
				{
					flatDrop.Normalize();
				}

				wish = flatDrop * (mag * 0.35f);
				wish.y = -Mathf.Max(mag, 0.2f);
				dropping = true;
				detourDrop = true;
				detourHold = 20;
			}

			bool forwardOpen = dropping || (!Blocked(wish) && StepFits(wish));
			if (detourHold > 0 && !forwardOpen)
			{
				detourHold--;
				Vector3 committed = DetourStep(dir, side, mag);
				if (committed.sqrMagnitude > 0.0001f && !Blocked(committed) && StepFits(committed))
				{
					wish = committed;
				}
				else
				{
					detourHold = 0;
					wish = Vector3.zero;
				}
			}
			else
			{
				detourHold = 0;
			}

			if (!dropping && detourHold <= 0 && (Blocked(wish) || !StepFits(wish)))
			{
				Vector3 under;
				if (TryUnder(dir, mag, out under))
				{
					wish = under;
					detourDrop = true;
					detourHold = 30;
				}
				else
				{
					Vector3 slide = Slide(dir, side, mag, goal);
					if (slide.sqrMagnitude > 0.0001f)
					{
						wish = slide;
						detourDrop = false;
						detourHold = 30;
					}
					else
					{
						wish = BlendedOpen(dir, side, goal, mag);
					}
				}
			}

			wish = FitStep(wish, mag);
			return Ease(wish, stepLen);
		}

		private Vector3 BlendedOpen(Vector3 dir, Vector3 side, Vector3 goal, float mag)
		{
			Vector3 best = Vector3.zero;
			float bestDist = float.MaxValue;
			Vector3[] mix =
			{
				(dir + side * 0.55f + Vector3.up * 0.28f).normalized,
				(dir - side * 0.55f + Vector3.up * 0.28f).normalized,
				(dir + side * 0.7f).normalized,
				(dir - side * 0.7f).normalized,
				(dir + Vector3.up * 0.4f).normalized,
				(dir + Vector3.down * 0.35f).normalized,
				(dir + side * 0.4f + Vector3.down * 0.32f).normalized,
				(dir - side * 0.4f + Vector3.down * 0.32f).normalized
			};

			for (int i = 0; i < mix.Length; i++)
			{
				Vector3 step = mix[i] * mag;
				if (Blocked(step) || !StepFits(step))
				{
					continue;
				}

				float left = (goal - (position + step)).sqrMagnitude;
				if (left < bestDist)
				{
					bestDist = left;
					best = step;
				}
			}

			return best;
		}

		private Vector3 Ease(Vector3 wish, float stepLen)
		{
			if (wish.sqrMagnitude < 0.0001f)
			{
				steer = Vector3.MoveTowards(steer, Vector3.zero, stepLen * 0.12f);
				return steer;
			}

			if (steer.sqrMagnitude < 0.0001f)
			{
				steer = wish.normalized * (stepLen * 0.3f);
			}

			steer = Vector3.RotateTowards(steer, wish, 4.5f * Mathf.Deg2Rad, stepLen * 0.15f);
			if (steer.sqrMagnitude > stepLen * stepLen)
			{
				steer = steer.normalized * stepLen;
			}

			return steer;
		}

		private int PickSide(Vector3 side)
		{
			int left = 0;
			int right = 0;
			for (int i = 0; i < nearby.Count; i++)
			{
				EntityAlive other = nearby[i] as EntityAlive;
				if (!IsDemon(other))
				{
					continue;
				}

				Vector3 gap = other.position - position;
				gap.y = 0f;
				float lean = Vector3.Dot(gap, side);
				if (lean > 0.3f)
				{
					left++;
				}
				else if (lean < -0.3f)
				{
					right++;
				}
			}

			if (left < right)
			{
				return 1;
			}

			if (right < left)
			{
				return -1;
			}

			return (entityId & 1) == 0 ? 1 : -1;
		}

		private Vector3 FitStep(Vector3 step, float mag)
		{
			if (step.sqrMagnitude < 0.0001f)
			{
				return step;
			}

			float feet = boundingBox.min.y + step.y;
			Gap gap = Measure(position.x + step.x, feet, position.z + step.z);
			bool clipping = HeadInCeiling();
			bool shortRoom = gap.HasFloor && gap.HasCeiling && gap.Ceiling - gap.Floor < BodyHeight() + GroundClearance;
			if (gap.HasFloor && !clipping)
			{
				float minFeet = gap.Floor + GroundClearance;
				if (gap.HasCeiling)
				{
					float maxFeet = gap.Ceiling - BodyHeight() - 0.15f;
					if (maxFeet < gap.Floor)
					{
						minFeet = gap.Floor;
					}
					else if (minFeet > maxFeet)
					{
						minFeet = maxFeet;
					}
				}

				if (feet < minFeet)
				{
					step.y += minFeet - feet;
					feet = minFeet;
				}
			}

			if (gap.HasCeiling && GapFits(gap))
			{
				float maxFeet = gap.Ceiling - BodyHeight() - 0.15f;
				if (feet > maxFeet)
				{
					step.y -= feet - maxFeet;
					feet = boundingBox.min.y + step.y;
				}
			}

			if (!clipping && !shortRoom && gap.HasFloor && feet <= gap.Floor + GroundClearance + 0.25f && step.y < 0f)
			{
				step.y = 0f;
			}

			if (mag > 0.001f && step.sqrMagnitude > mag * mag)
			{
				step = step.normalized * mag;
			}

			return step;
		}

		private Vector3 DetourStep(Vector3 dir, Vector3 side, float mag)
		{
			Vector3 flat = new Vector3(dir.x, 0f, dir.z);
			if (flat.sqrMagnitude < 0.0001f)
			{
				return Vector3.zero;
			}

			flat.Normalize();
			if (detourDrop)
			{
				Vector3 under;
				if (TryUnder(flat, mag, out under))
				{
					return under;
				}

				return Vector3.zero;
			}

			Vector3 slide = (flat + side * detourSide).normalized * mag;
			slide.y = 0f;
			return slide;
		}

		private bool TryUnder(Vector3 dir, float mag, out Vector3 step)
		{
			step = Vector3.zero;
			if (OnFloorNow())
			{
				return false;
			}

			Vector3 flat = new Vector3(dir.x, 0f, dir.z);
			if (flat.sqrMagnitude < 0.0001f)
			{
				return false;
			}

			flat.Normalize();
			float body = BodyHeight();
			float ahead = Mathf.Max(mag, 0.35f);
			float opening;
			if (!OpeningUnder(position.x + flat.x * ahead, position.z + flat.z * ahead, boundingBox.min.y, body, out opening))
			{
				return false;
			}

			float drop = opening - boundingBox.min.y;
			if (drop >= -0.02f)
			{
				return false;
			}

			Vector3 wish = flat * mag;
			wish.y = Mathf.Clamp(drop, -mag, 0f);
			if (Blocked(wish) || !StepFits(wish))
			{
				wish = new Vector3(0f, Mathf.Clamp(drop, -mag, 0f), 0f);
				if (Blocked(wish) || !StepFits(wish))
				{
					return false;
				}
			}

			step = wish;
			return true;
		}

		private Vector3 Slide(Vector3 dir, Vector3 side, float mag, Vector3 goal)
		{
			Vector3 flat = new Vector3(dir.x, 0f, dir.z);
			if (flat.sqrMagnitude < 0.0001f)
			{
				return Vector3.zero;
			}

			flat.Normalize();
			Vector3 left = (flat + side).normalized * mag;
			Vector3 right = (flat - side).normalized * mag;
			left.y = 0f;
			right.y = 0f;
			bool leftOk = !Blocked(left) && StepFits(left);
			bool rightOk = !Blocked(right) && StepFits(right);
			if (leftOk && rightOk)
			{
				float leftDist = (goal - (position + left)).sqrMagnitude;
				float rightDist = (goal - (position + right)).sqrMagnitude;
				detourSide = leftDist <= rightDist ? 1 : -1;
				return leftDist <= rightDist ? left : right;
			}

			if (leftOk)
			{
				detourSide = 1;
				return left;
			}

			if (rightOk)
			{
				detourSide = -1;
				return right;
			}

			return Vector3.zero;
		}

		private bool OpeningUnder(float x, float z, float feet, float body, out float feetY)
		{
			feetY = feet;
			int ix = Utils.Fastfloor(x);
			int iz = Utils.Fastfloor(z);
			int top = Utils.Fastfloor(feet + body);
			bool seenSolid = false;
			for (int i = 0; i <= 14; i++)
			{
				int by = top - i;
				if (by < 1)
				{
					break;
				}

				if (Solid(ix, by, iz))
				{
					seenSolid = true;
					continue;
				}

				if (!seenSolid)
				{
					continue;
				}

				float ceiling = by + 1f;
				float floor = ceiling - 16f;
				bool hasFloor = false;
				for (int d = 1; d <= 16; d++)
				{
					int fy = by - d;
					if (fy < 0)
					{
						break;
					}

					if (Solid(ix, fy, iz))
					{
						floor = fy + 1f;
						hasFloor = true;
						break;
					}
				}

				if (ceiling - floor < body + 0.15f)
				{
					return false;
				}

				float tucked = ceiling - body - 0.15f;
				if (hasFloor)
				{
					tucked = Mathf.Max(tucked, floor + GroundClearance);
				}

				if (tucked >= feet - 0.02f)
				{
					return false;
				}

				feetY = tucked;
				return true;
			}

			return false;
		}

		private bool StepFits(Vector3 step)
		{
			if (step.sqrMagnitude < 0.0001f)
			{
				return true;
			}

			float feet = boundingBox.min.y + step.y;
			Gap gap = Measure(position.x + step.x, feet, position.z + step.z);
			if (!gap.HasFloor || !gap.HasCeiling || gap.Ceiling - gap.Floor >= BodyHeight() + 0.15f)
			{
				return true;
			}

			Gap here = Measure(position.x, boundingBox.min.y, position.z);
			float hereRoom = here.HasFloor && here.HasCeiling ? here.Ceiling - here.Floor : float.MaxValue;
			return gap.Ceiling - gap.Floor + 0.05f >= hereRoom;
		}

		private bool OnFloorNow()
		{
			Gap gap = Measure(position.x, boundingBox.min.y, position.z);
			return gap.HasFloor && boundingBox.min.y <= gap.Floor + GroundClearance + 0.25f;
		}

		private bool CanDrop()
		{
			Gap gap = Measure(position.x, boundingBox.min.y, position.z);
			if (gap.HasFloor && gap.Floor > boundingBox.min.y + 0.05f)
			{
				return true;
			}

			if (OnFloorNow())
			{
				return false;
			}

			if (!gap.HasFloor)
			{
				return true;
			}

			return boundingBox.min.y - 0.25f >= gap.Floor + GroundClearance;
		}

		private bool HeadInCeiling()
		{
			Bounds box = boundingBox;
			float y = box.max.y - 0.08f;
			float mid = box.min.y + (box.max.y - box.min.y) * 0.55f;
			if (y < mid)
			{
				return false;
			}

			float hx = box.extents.x * 0.8f;
			float hz = box.extents.z * 0.8f;
			Vector3 c = box.center;
			return SolidAt(c.x, y, c.z)
				|| SolidAt(c.x + hx, y, c.z)
				|| SolidAt(c.x - hx, y, c.z)
				|| SolidAt(c.x, y, c.z + hz)
				|| SolidAt(c.x, y, c.z - hz);
		}

		private bool SolidAt(float x, float y, float z)
		{
			return Solid(Utils.Fastfloor(x), Utils.Fastfloor(y), Utils.Fastfloor(z));
		}

		private float BodyHeight()
		{
			return Mathf.Max(0.8f, boundingBox.size.y);
		}

		private bool GapFits(Gap gap)
		{
			if (!gap.HasFloor || !gap.HasCeiling)
			{
				return true;
			}

			return gap.Ceiling - gap.Floor >= BodyHeight() + 0.15f;
		}

		private Gap Measure(float x, float feet, float z)
		{
			int ix = Utils.Fastfloor(x);
			int iz = Utils.Fastfloor(z);
			int iy = Utils.Fastfloor(feet - 0.05f);
			Gap gap = new Gap();
			for (int i = 0; i <= 16; i++)
			{
				int by = iy - i;
				if (by < 0)
				{
					break;
				}

				if (Solid(ix, by, iz))
				{
					gap.HasFloor = true;
					gap.Floor = by + 1f;
					break;
				}
			}

			int head = Utils.Fastfloor(feet + BodyHeight() - 0.05f);
			for (int i = 1; i <= 16; i++)
			{
				int by = head + i;
				if (by > 255)
				{
					break;
				}

				if (Solid(ix, by, iz))
				{
					gap.HasCeiling = true;
					gap.Ceiling = by;
					break;
				}
			}

			return gap;
		}

		private bool Solid(int x, int y, int z)
		{
			BlockValue block = world.GetBlock(x, y, z);
			if (block.isair)
			{
				return false;
			}

			Block shape = block.Block;
			return shape != null && shape.IsCollideMovement;
		}

		private struct Gap
		{
			public float Floor;

			public float Ceiling;

			public bool HasFloor;

			public bool HasCeiling;
		}

		private float GroundAt(float x, float z)
		{
			int height = world.GetHeight(Utils.Fastfloor(x), Utils.Fastfloor(z));
			if (height <= 1)
			{
				return position.y - CruiseUp;
			}

			return height;
		}

		private void Gather()
		{
			nearby.Clear();
			Bounds area = new Bounds(position, new Vector3(10f, 8f, 10f));
			world.GetEntitiesInBounds(typeof(EntityAlive), area, nearby);
		}

		private bool DemonInWay(Vector3 flatDir)
		{
			if (flatDir.sqrMagnitude < 0.0001f)
			{
				return false;
			}

			Gather();
			for (int i = 0; i < nearby.Count; i++)
			{
				EntityAlive other = nearby[i] as EntityAlive;
				if (!IsDemon(other))
				{
					continue;
				}

				Vector3 gap = other.position - position;
				float rise = Mathf.Abs(gap.y);
				gap.y = 0f;
				float dist = gap.magnitude;
				if (dist > 2.4f || dist < 0.05f || rise > 2.2f)
				{
					continue;
				}

				if (Vector3.Dot(gap / dist, flatDir) > 0.35f)
				{
					return true;
				}
			}

			return false;
		}

		private bool IsDemon(EntityAlive other)
		{
			if (other == null || other == this || other.IsDead())
			{
				return false;
			}

			EntityClass kind = EntityClass.list[other.entityClass];
			return kind != null && kind.Tags.Test_AnySet(DemonTag);
		}

		private bool Blocked(Vector3 step)
		{
			if (step.sqrMagnitude < 0.0001f)
			{
				return false;
			}

			Bounds box = ProbeBox(boundingBox);
			hits.Clear();
			world.GetCollidingBounds(this, box, hits);
			held.Clear();
			held.AddRange(hits);
			box.center += step;
			hits.Clear();
			world.GetCollidingBounds(this, box, hits);
			if (hits.Count == 0)
			{
				return false;
			}

			if (held.Count == 0)
			{
				return true;
			}

			for (int i = 0; i < hits.Count; i++)
			{
				if (!SameBlock(held, hits[i]))
				{
					return true;
				}
			}

			return false;
		}

		private static Bounds ProbeBox(Bounds box)
		{
			Vector3 size = box.size;
			const float floorGap = 0.45f;
			if (size.y > floorGap + 0.35f)
			{
				size.y -= floorGap;
				box.size = size;
				box.center += new Vector3(0f, floorGap * 0.5f, 0f);
			}

			return box;
		}

		private static bool SameBlock(List<Bounds> before, Bounds hit)
		{
			for (int i = 0; i < before.Count; i++)
			{
				if ((before[i].center - hit.center).sqrMagnitude < 0.01f && (before[i].size - hit.size).sqrMagnitude < 0.01f)
				{
					return true;
				}
			}

			return false;
		}

		private float FlatMeters(EntityAlive target)
		{
			Vector3 flat = target.position - position;
			flat.y = 0f;
			return flat.magnitude;
		}

		private bool Touching(EntityAlive target)
		{
			if (FlatMeters(target) > BiteReach())
			{
				return false;
			}

			float gap = 0f;
			if (boundingBox.min.y > target.boundingBox.max.y)
			{
				gap = boundingBox.min.y - target.boundingBox.max.y;
			}
			else if (target.boundingBox.min.y > boundingBox.max.y)
			{
				gap = target.boundingBox.min.y - boundingBox.max.y;
			}

			return gap <= 0.75f;
		}

		private bool WantsShot(float meters)
		{
			float dist = meters * 32f - 192f;
			if (dist <= 0f || meters <= AlwaysSpitMeters)
			{
				return true;
			}

			if (dist > 200f)
			{
				dist = 200f;
			}

			return rand.RandomFloat * 256f >= dist;
		}

		private ItemAction MeleeAction()
		{
			if (inventory == null || inventory.holdingItem == null)
			{
				return null;
			}

			ItemAction[] actions = inventory.holdingItem.Actions;
			if (actions == null || actions.Length < 1)
			{
				return null;
			}

			return actions[0];
		}

		private float BiteReach()
		{
			ItemAction melee = MeleeAction();
			if (melee != null && melee.Range > 0f)
			{
				return melee.Range;
			}

			return TouchMeters;
		}

		private int BiteTicks()
		{
			ItemAction melee = MeleeAction();
			float delay = melee != null ? melee.Delay : 0.2f;
			if (delay <= 0f)
			{
				delay = 0.2f;
			}

			int ticks = Mathf.CeilToInt(delay / Tick);
			return ticks < 1 ? 1 : ticks;
		}

		private void Bite(EntityAlive target)
		{
			ItemActionAttack attack = MeleeAction() as ItemActionAttack;
			if (attack == null || inventory.holdingItemItemValue == null)
			{
				return;
			}

			EntityAlive previous = MinEventContext.Other;
			MinEventContext.Other = target;
			MinEventContext.ItemValue = inventory.holdingItemItemValue;
			int damage = Mathf.RoundToInt(attack.GetDamageEntity(inventory.holdingItemItemValue, this, 0));
			MinEventContext.Other = previous;
			if (damage <= 0)
			{
				return;
			}

			DamageSourceEntity source = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, entityId);
			target.DamageEntity(source, damage, false, 1f);
		}

		private void Spit()
		{
			if (inventory == null || inventory.holdingItemData == null || inventory.holdingItemData.actionData == null || inventory.holdingItemData.actionData.Count < 2)
			{
				return;
			}

			ItemActionVomit.ItemActionDataVomit data = inventory.holdingItemData.actionData[1] as ItemActionVomit.ItemActionDataVomit;
			if (data == null)
			{
				return;
			}

			if ((bool)emodel)
			{
				data.muzzle = emodel.GetHeadTransform();
			}

			data.numWarningsPlayed = 999;
			data.bAttackStarted = true;
			data.warningTime = 0f;
			data.numVomits = 0;
			data.isActive = false;
			ReadShotLine();
			string sound = shotSound;
			if (string.IsNullOrEmpty(sound))
			{
				sound = HandWarning();
			}

			if (!string.IsNullOrEmpty(sound))
			{
				PlayOneShot(sound);
			}

			UseHoldingItem(1, false);
			UseHoldingItem(1, true);
		}

		private bool InShotRange(EntityAlive target)
		{
			ReadShotLine();
			float meters = FlatMeters(target);
			return meters >= shotMin && meters <= shotMax;
		}

		private int ShotGap()
		{
			ReadShotLine();
			float cooldown = shotCooldown + shotCooldown * 0.5f * rand.RandomFloat;
			int ticks = Mathf.CeilToInt((shotDuration + cooldown) / Tick);
			return ticks < 1 ? 1 : ticks;
		}

		private void ReadShotLine()
		{
			if (shotReady)
			{
				return;
			}

			shotReady = true;
			EntityClass kind = EntityClass.list[entityClass];
			if (kind == null || kind.Properties == null)
			{
				return;
			}

			string raw = kind.Properties.GetString("AITask");
			if (string.IsNullOrEmpty(raw))
			{
				return;
			}

			int at = raw.IndexOf("RangedAttackTarget", StringComparison.Ordinal);
			if (at < 0)
			{
				return;
			}

			int end = raw.IndexOf('|', at);
			string line = end < 0 ? raw.Substring(at) : raw.Substring(at, end - at);
			shotCooldown = Number(line, "cooldown", shotCooldown);
			shotDuration = Number(line, "duration", shotDuration);
			shotMin = Number(line, "minRange", shotMin);
			shotMax = Number(line, "maxRange", shotMax);
			shotSound = Text(line, "sndStart");
		}

		private string HandWarning()
		{
			if (inventory == null || inventory.holdingItem == null)
			{
				return "";
			}

			ItemAction[] actions = inventory.holdingItem.Actions;
			if (actions == null || actions.Length < 2)
			{
				return "";
			}

			ItemActionVomit vomit = actions[1] as ItemActionVomit;
			if (vomit == null)
			{
				return "";
			}

			return WarningSound(vomit);
		}

		private static float Number(string line, string key, float fallback)
		{
			string token = key + "=";
			int at = line.IndexOf(token, StringComparison.Ordinal);
			if (at < 0)
			{
				return fallback;
			}

			at += token.Length;
			int end = at;
			while (end < line.Length && (char.IsDigit(line[end]) || line[end] == '.' || line[end] == '-'))
			{
				end++;
			}

			float value;
			if (end > at && float.TryParse(line.Substring(at, end - at), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				return value;
			}

			return fallback;
		}

		private static string Text(string line, string key)
		{
			string token = key + "=";
			int at = line.IndexOf(token, StringComparison.Ordinal);
			if (at < 0)
			{
				return "";
			}

			at += token.Length;
			int end = at;
			while (end < line.Length && line[end] != ';' && line[end] != '|' && !char.IsWhiteSpace(line[end]))
			{
				end++;
			}

			if (end <= at)
			{
				return "";
			}

			return line.Substring(at, end - at);
		}
	}
}
