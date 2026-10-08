using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The body stays level and only turns left and right. It flies a straight
	/// line toward one meter above its target, ducking, climbing, or going
	/// around when something is in the way. The projectile aims at the chest.
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

		private const float TouchHeight = 1.5f;

		private const float HoverAbove = 1f;

		private const float CruiseUp = 10f;

		private const float GroundClearance = 1f;

		private const float RingRadius = 1.35f;

		private const float SearchRadius = 28f;

		private const int WindupTicks = 9;

		private static readonly FastTags<TagGroup.Global> DemonTag = FastTags<TagGroup.Global>.Parse("demon");

		private readonly List<Bounds> hits = new List<Bounds>();

		private readonly List<Entity> nearby = new List<Entity>();

		private Vector3 cruiseHome;

		private Vector3 cruisePoint;

		private int cruiseTicks;

		private bool cruiseReady;

		private int avoidSide;

		private int avoidHold;

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

		private int attackWait;

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
			Vector3 unpack = Unpack();
			if (unpack.sqrMagnitude > 0.0001f)
			{
				motion = unpack;
			}
			else
			{
				float surface = GroundAt(position.x, position.z);
				if (boundingBox.min.y < surface + 0.35f)
				{
					motion.y = Mathf.Max(motion.y, StepLength());
				}
			}

			entityCollision(motion);
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
					attackWait = rand.RandomRange(6, 14);
					return;
				}

				if (windup > 0)
				{
					return;
				}

				if (Touching(target))
				{
					Bite(target);
				}
				else
				{
					Spit();
				}

				mode = Mode.Drift;
				attackWait = rand.RandomRange(8, 27);
				return;
			}

			if (attackWait > 0)
			{
				attackWait--;
			}

			Drift(target);
			if (attackWait <= 0 && CanSee(target) && WantsShot(FlatMeters(target)))
			{
				mode = Mode.Windup;
				windup = WindupTicks;
				painCancel = false;
				motion = Vector3.zero;
			}
			else if (attackWait <= 0)
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
			Vector3 spot = lastSeenPos + Vector3.up * HoverAbove;
			Vector3 flat = lastSeenPos - position;
			flat.y = 0f;
			if (flat.sqrMagnitude > 0.25f)
			{
				SeekYaw(Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg, 0f, 12f);
			}

			if ((lastSeenPos - position).sqrMagnitude < 2.25f)
			{
				motion = Ease(Vector3.zero, StepLength());
				return;
			}

			motion = StepToward(spot);
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
			Vector3 center = target.position + Vector3.up * HoverAbove;
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
			return push.normalized * step + Vector3.up * (step * 0.45f);
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
			if (Blocked(wish))
			{
				wish = BlendedOpen(dir, side, goal, mag);
			}

			wish = ClearGround(wish, goal, mag);
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
				if (Blocked(step))
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

		private Vector3 ClearGround(Vector3 step, Vector3 goal, float mag)
		{
			if (step.sqrMagnitude < 0.0001f)
			{
				return step;
			}

			float pad = step.y < -0.2f ? 0.45f : GroundClearance;
			float floor = GroundAt(position.x + step.x, position.z + step.z) + pad;
			float destY = position.y + step.y;
			if (goal.y < floor - 0.2f && destY < floor)
			{
				step.y += floor - destY;
				if (mag > 0.001f && step.sqrMagnitude > mag * mag)
				{
					step = step.normalized * mag;
				}
			}

			return step;
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
			Bounds box = boundingBox;
			Vector3 size = box.size;
			const float floorGap = 0.45f;
			if (size.y > floorGap + 0.35f)
			{
				size.y -= floorGap;
				box.size = size;
				box.center += new Vector3(0f, floorGap * 0.5f, 0f);
			}

			box.center += step;
			hits.Clear();
			world.GetCollidingBounds(this, box, hits);
			return hits.Count > 0;
		}

		private float FlatMeters(EntityAlive target)
		{
			Vector3 flat = target.position - position;
			flat.y = 0f;
			return flat.magnitude;
		}

		private bool Touching(EntityAlive target)
		{
			if (FlatMeters(target) > TouchMeters)
			{
				return false;
			}

			return Mathf.Abs(boundingBox.center.y - target.boundingBox.center.y) <= TouchHeight;
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

		private void Bite(EntityAlive target)
		{
			int steps = 1 + (int)(rand.RandomFloat * 6f);
			if (steps > 6)
			{
				steps = 6;
			}

			DamageSourceEntity source = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, entityId);
			target.DamageEntity(source, steps * 10, false, 1f);
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
			PlayOneShot("DemonFireballThrown");
			UseHoldingItem(1, false);
			UseHoldingItem(1, true);
		}
	}
}
