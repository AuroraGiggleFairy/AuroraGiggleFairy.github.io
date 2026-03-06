using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarController : MonoBehaviour
{
	public enum DataTypes
	{
		HitDuration
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class ChangedAnimationParameters
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const float sendPeriodInSeconds = 0.05f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float m_sendDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<List<AnimParamData>> m_animationParameters = new List<List<AnimParamData>>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, int> m_animationParameterLookup = new Dictionary<int, int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_hasAnyTriggers;

		public void Add(AnimParamData apd)
		{
			int count = m_animationParameters.Count;
			List<AnimParamData> list = ((count >= 1) ? m_animationParameters[count - 1] : newPacket());
			if (m_animationParameterLookup.TryGetValue(apd.NameHash, out var value))
			{
				list.RemoveAt(value);
			}
			m_animationParameterLookup[apd.NameHash] = list.Count;
			list.Add(apd);
			if (apd.ValueType == AnimParamData.ValueTypes.Trigger)
			{
				newPacket();
				m_hasAnyTriggers = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<AnimParamData> newPacket()
		{
			List<AnimParamData> list = new List<AnimParamData>();
			m_animationParameters.Add(list);
			m_animationParameterLookup.Clear();
			return list;
		}

		public List<List<AnimParamData>> GetParameterLists()
		{
			List<List<AnimParamData>> list = new List<List<AnimParamData>>();
			m_sendDelay -= Time.deltaTime;
			if (m_hasAnyTriggers || m_sendDelay <= 0f)
			{
				while (m_animationParameters.Count > 0)
				{
					List<AnimParamData> list2 = m_animationParameters[0];
					m_animationParameters.RemoveAt(0);
					if (list2.Count != 0)
					{
						list.Add(list2);
					}
				}
				m_hasAnyTriggers = false;
				m_sendDelay = 0.05f;
			}
			return list;
		}
	}

	public enum ActionState
	{
		None,
		Start,
		Ready,
		Active
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinFloatChangeSquared = 0.0001f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int attackTag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int deathHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int digHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitStartHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int jumpHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int moveHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int stunHash;

	public static int attackHash;

	public static int attackBlendHash;

	public static int attackStartHash;

	public static int attackReadyHash;

	public static int meleeAttackSpeedHash;

	public static int beginCorpseEatHash;

	public static int endCorpseEatHash;

	public static int forwardHash;

	public static int hitBodyPartHash;

	public static int idleTimeHash;

	public static int isAimingHash;

	public static int itemUseHash;

	public static int movementStateHash;

	public static int rotationPitchHash;

	public static int strafeHash;

	public static int swimSelectHash;

	public static int turnRateHash;

	public static int weaponHoldTypeHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int walkTypeHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int walkTypeBlendHash;

	public static int isAliveHash;

	public static int isDeadHash;

	public static int isFPVHash;

	public static int isMovingHash;

	public static int isSwimHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int attackTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int deathTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int movementTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int electrocuteTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int painTriggerHash;

	public static int itemHasChangedTriggerHash;

	public static int itemThrownAwayTriggerHash;

	public static int reloadHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int dodgeBlendHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int dodgeTriggerHash;

	public static int reactionTypeHash;

	public static int reactionTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperPoseHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperTriggerHash;

	public static int jumpLandResponseHash;

	public static int forcedRootMotionHash;

	public static int preventAttackHash;

	public static int canFallHash;

	public static int isOnGroundHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int triggerAliveHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int bodyPartHitHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitDirectionHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitDamageHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int criticalHitHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int randomHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int jumpStartHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int jumpLandHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int isMaleHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int specialAttack2Hash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int rageHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int stunTypeHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int stunBodyPartHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int isCriticalHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int HitRandomValueHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int beginStunTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int endStunTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int toCrawlerTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int isElectrocutedHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int isClimbingHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int verticalSpeedHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int reviveHash;

	public static int harvestingHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int weaponFireHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int weaponPreFireCancelHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int weaponPreFireHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int weaponAmmoRemaining;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int useItemHash;

	public static int itemActionIndexHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int isCrouchingHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int reloadSpeedHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int jumpTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int inAirHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int jumpLandTriggerHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int hitRandomValueHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleSitHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleSideRightHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleSideLeftHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleBackHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleStomachHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int sleeperIdleStandHash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static int archetypeStanceHash;

	public static int yLookHash;

	public static int vehiclePoseHash;

	public static int holsteredHash;

	public const int cSleeperPoseMove = -2;

	public const int cSleeperPoseAwake = -1;

	public const int cSleeperPoseSit = 0;

	public const int cSleeperPoseSideRight = 1;

	public const int cSleeperPoseSideLeft = 2;

	public const int cSleeperPoseBack = 3;

	public const int cSleeperPoseStomach = 4;

	public const int cSleeperPoseStand = 5;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator anim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<AnimParamData>> queuedAnimParams = new List<List<AnimParamData>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public ChangedAnimationParameters changedAnimationParameters = new ChangedAnimationParameters();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float animSyncWaitTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float electrocuteTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHitBlendInTimeMax = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHitBlendOutExtraTime = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHitWeightFastTarget = 0.15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHitAgainWeightAdd = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHitAgainWeightAddWeak = 0.1f;

	public float hitWeightMax = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitDurationOut;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitLayerIndex = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitWeight = 0.001f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitWeightTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float hitWeightDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float forwardSpeedLerpMultiplier = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float strafeSpeedLerpMultiplier = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float targetSpeedForward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float targetSpeedStrafe;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPhysicsTicks = 50f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasTurnRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float turnRateFacing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float turnRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<int, string> hashNames;

	public const int cActionSpecial = 3000;

	public const int cActionEnd = 9999;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxQueuedAnimData = 10;

	public EntityAlive Entity => entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		StaticInit();
		entity = GetComponent<EntityAlive>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StaticInit()
	{
		if (!initialized)
		{
			initialized = true;
			hashNames = new Dictionary<int, string>();
			AssignAnimatorHash(ref attackHash, "Attack");
			AssignAnimatorHash(ref attackBlendHash, "AttackBlend");
			AssignAnimatorHash(ref attackStartHash, "AttackStart");
			AssignAnimatorHash(ref attackReadyHash, "AttackReady");
			AssignAnimatorHash(ref meleeAttackSpeedHash, "MeleeAttackSpeed");
			AssignAnimatorHash(ref deathHash, "Death");
			AssignAnimatorHash(ref digHash, "Dig");
			AssignAnimatorHash(ref hitStartHash, "HitStart");
			AssignAnimatorHash(ref hitHash, "Hit");
			AssignAnimatorHash(ref jumpHash, "Jump");
			AssignAnimatorHash(ref moveHash, "Move");
			AssignAnimatorHash(ref stunHash, "Stun");
			AssignAnimatorHash(ref beginCorpseEatHash, "BeginCorpseEat");
			AssignAnimatorHash(ref endCorpseEatHash, "EndCorpseEat");
			AssignAnimatorHash(ref forwardHash, "Forward");
			AssignAnimatorHash(ref hitBodyPartHash, "HitBodyPart");
			AssignAnimatorHash(ref idleTimeHash, "IdleTime");
			AssignAnimatorHash(ref isAimingHash, "IsAiming");
			AssignAnimatorHash(ref itemUseHash, "ItemUse");
			AssignAnimatorHash(ref movementStateHash, "MovementState");
			AssignAnimatorHash(ref rotationPitchHash, "RotationPitch");
			AssignAnimatorHash(ref strafeHash, "Strafe");
			AssignAnimatorHash(ref swimSelectHash, "SwimSelect");
			AssignAnimatorHash(ref turnRateHash, "TurnRate");
			AssignAnimatorHash(ref walkTypeHash, "WalkType");
			AssignAnimatorHash(ref walkTypeBlendHash, "WalkTypeBlend");
			AssignAnimatorHash(ref weaponHoldTypeHash, "WeaponHoldType");
			AssignAnimatorHash(ref isAliveHash, "IsAlive");
			AssignAnimatorHash(ref isDeadHash, "IsDead");
			AssignAnimatorHash(ref isFPVHash, "IsFPV");
			AssignAnimatorHash(ref isMovingHash, "IsMoving");
			AssignAnimatorHash(ref isSwimHash, "IsSwim");
			AssignAnimatorHash(ref attackTriggerHash, "AttackTrigger");
			AssignAnimatorHash(ref deathTriggerHash, "DeathTrigger");
			AssignAnimatorHash(ref hitTriggerHash, "HitTrigger");
			AssignAnimatorHash(ref movementTriggerHash, "MovementTrigger");
			AssignAnimatorHash(ref electrocuteTriggerHash, "ElectrocuteTrigger");
			AssignAnimatorHash(ref painTriggerHash, "PainTrigger");
			AssignAnimatorHash(ref itemHasChangedTriggerHash, "ItemHasChangedTrigger");
			AssignAnimatorHash(ref itemThrownAwayTriggerHash, "ItemThrownAwayTrigger");
			AssignAnimatorHash(ref dodgeBlendHash, "DodgeBlend");
			AssignAnimatorHash(ref dodgeTriggerHash, "DodgeTrigger");
			AssignAnimatorHash(ref reactionTriggerHash, "ReactionTrigger");
			AssignAnimatorHash(ref reactionTypeHash, "ReactionType");
			AssignAnimatorHash(ref sleeperPoseHash, "SleeperPose");
			AssignAnimatorHash(ref sleeperTriggerHash, "SleeperTrigger");
			AssignAnimatorHash(ref jumpLandResponseHash, "JumpLandResponse");
			AssignAnimatorHash(ref forcedRootMotionHash, "ForcedRootMotion");
			AssignAnimatorHash(ref preventAttackHash, "PreventAttack");
			AssignAnimatorHash(ref canFallHash, "CanFall");
			AssignAnimatorHash(ref isOnGroundHash, "IsOnGround");
			AssignAnimatorHash(ref triggerAliveHash, "TriggerAlive");
			AssignAnimatorHash(ref bodyPartHitHash, "BodyPartHit");
			AssignAnimatorHash(ref hitDirectionHash, "HitDirection");
			AssignAnimatorHash(ref criticalHitHash, "CriticalHit");
			AssignAnimatorHash(ref hitDamageHash, "HitDamage");
			AssignAnimatorHash(ref randomHash, "Random");
			AssignAnimatorHash(ref jumpStartHash, "JumpStart");
			AssignAnimatorHash(ref jumpLandHash, "JumpLand");
			AssignAnimatorHash(ref isMaleHash, "IsMale");
			AssignAnimatorHash(ref specialAttack2Hash, "SpecialAttack2");
			AssignAnimatorHash(ref rageHash, "Rage");
			AssignAnimatorHash(ref stunTypeHash, "StunType");
			AssignAnimatorHash(ref stunBodyPartHash, "StunBodyPart");
			AssignAnimatorHash(ref isCriticalHash, "isCritical");
			AssignAnimatorHash(ref HitRandomValueHash, "HitRandomValue");
			AssignAnimatorHash(ref beginStunTriggerHash, "BeginStunTrigger");
			AssignAnimatorHash(ref endStunTriggerHash, "EndStunTrigger");
			AssignAnimatorHash(ref toCrawlerTriggerHash, "ToCrawlerTrigger");
			AssignAnimatorHash(ref isElectrocutedHash, "IsElectrocuted");
			AssignAnimatorHash(ref isClimbingHash, "IsClimbing");
			AssignAnimatorHash(ref verticalSpeedHash, "VerticalSpeed");
			AssignAnimatorHash(ref reviveHash, "Revive");
			AssignAnimatorHash(ref harvestingHash, "Harvesting");
			AssignAnimatorHash(ref weaponFireHash, "WeaponFire");
			AssignAnimatorHash(ref weaponPreFireCancelHash, "WeaponPreFireCancel");
			AssignAnimatorHash(ref weaponPreFireHash, "WeaponPreFire");
			AssignAnimatorHash(ref weaponAmmoRemaining, "WeaponAmmoRemaining");
			AssignAnimatorHash(ref useItemHash, "UseItem");
			AssignAnimatorHash(ref itemActionIndexHash, "ItemActionIndex");
			AssignAnimatorHash(ref isCrouchingHash, "IsCrouching");
			AssignAnimatorHash(ref reloadHash, "Reload");
			AssignAnimatorHash(ref reloadSpeedHash, "ReloadSpeed");
			AssignAnimatorHash(ref jumpTriggerHash, "JumpTrigger");
			AssignAnimatorHash(ref inAirHash, "InAir");
			AssignAnimatorHash(ref jumpLandHash, "JumpLand");
			AssignAnimatorHash(ref hitRandomValueHash, "HitRandomValue");
			AssignAnimatorHash(ref hitTriggerHash, "HitTrigger");
			AssignAnimatorHash(ref archetypeStanceHash, "ArchetypeStance");
			AssignAnimatorHash(ref yLookHash, "YLook");
			AssignAnimatorHash(ref vehiclePoseHash, "VehiclePose");
			AssignAnimatorHash(ref sleeperIdleBackHash, "SleeperIdleBack");
			AssignAnimatorHash(ref sleeperIdleSideLeftHash, "SleeperIdleSideLeft");
			AssignAnimatorHash(ref sleeperIdleSideRightHash, "SleeperIdleSideRight");
			AssignAnimatorHash(ref sleeperIdleSitHash, "SleeperIdleSit");
			AssignAnimatorHash(ref sleeperIdleStandHash, "SleeperIdleStand");
			AssignAnimatorHash(ref sleeperIdleStomachHash, "SleeperIdleStomach");
			AssignAnimatorHash(ref holsteredHash, "Holstered");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void AssignAnimatorHash(ref int hash, string parameterName)
	{
		hash = Animator.StringToHash(parameterName);
		if (!hashNames.ContainsKey(hash))
		{
			hashNames.Add(hash, parameterName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void assignStates()
	{
	}

	public virtual Animator GetAnimator()
	{
		return anim;
	}

	public void SetAnimator(Transform _animT)
	{
		SetAnimator(_animT.GetComponent<Animator>());
	}

	public void SetAnimator(Animator _anim)
	{
		if (anim == _anim)
		{
			return;
		}
		anim = _anim;
		if (!anim)
		{
			return;
		}
		anim.logWarnings = false;
		AnimatorControllerParameter[] parameters = anim.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].nameHash == turnRateHash)
			{
				hasTurnRate = true;
			}
		}
	}

	public bool IsMoving(float forwardSpeed, float strafeSpeed)
	{
		return forwardSpeed * forwardSpeed + strafeSpeed * strafeSpeed > 0.0001f;
	}

	public virtual void NotifyAnimatorMove(Animator instigator)
	{
		entity.NotifyRootMotion(instigator);
	}

	public abstract Transform GetActiveModelRoot();

	public virtual Transform GetRightHandTransform()
	{
		return null;
	}

	public Texture2D GetTexture()
	{
		return null;
	}

	public virtual void ResetAnimations()
	{
		Animator animator = GetAnimator();
		if ((bool)animator)
		{
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			animator.enabled = true;
		}
	}

	public virtual void SetMeleeAttackSpeed(float _speed)
	{
		UpdateFloat(meleeAttackSpeedHash, _speed);
	}

	public abstract bool IsAnimationAttackPlaying();

	public abstract void StartAnimationAttack();

	public virtual void SetInAir(bool inAir)
	{
	}

	public virtual void SetAttackImpact()
	{
	}

	public virtual bool IsAttackImpact()
	{
		return true;
	}

	public virtual bool IsAnimationWithMotionRunning()
	{
		return true;
	}

	public virtual ActionState GetActionState()
	{
		return ActionState.None;
	}

	public virtual bool IsActionActive()
	{
		return false;
	}

	public virtual void StartAction(int _animType)
	{
	}

	public virtual bool IsAnimationSpecialAttackPlaying()
	{
		return false;
	}

	public virtual void StartAnimationSpecialAttack(bool _b, int _animType)
	{
	}

	public virtual bool IsAnimationSpecialAttack2Playing()
	{
		return false;
	}

	public virtual void StartAnimationSpecialAttack2()
	{
	}

	public virtual bool IsAnimationRagingPlaying()
	{
		return false;
	}

	public virtual void StartAnimationRaging()
	{
	}

	public virtual void StartAnimationFiring()
	{
	}

	public virtual bool IsAnimationHitRunning()
	{
		return false;
	}

	public virtual void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckHit(float duration)
	{
		if (hitWeight < 0.15f || duration > hitDuration || !IsAnimationHitRunning())
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHitDuration(float duration)
	{
		if (hitWeight > 0.15f)
		{
			float num = 0.2f;
			if (duration == 0f)
			{
				num = 0.1f;
			}
			if (hitWeightTarget > hitWeight)
			{
				hitWeightTarget += num;
				if (hitWeightTarget > hitWeightMax)
				{
					hitWeightTarget = hitWeightMax;
				}
			}
			hitWeight += num;
			if (hitWeight > hitWeightMax)
			{
				hitWeight = hitWeightMax;
			}
		}
		else
		{
			duration = Utils.FastMax(duration, 0.120000005f);
			hitDuration = duration;
			float num2 = Utils.FastMin(duration * 0.25f, 0.1f);
			hitDurationOut = duration - num2;
			hitWeightTarget = num2 / 0.1f;
			hitWeightTarget = Utils.FastClamp(hitWeightTarget, 0.2f, 0.8f);
			hitWeightDuration = num2 / Utils.FastMax(0.01f, hitWeightTarget - hitWeight);
			if (hitWeight == 0f)
			{
				anim.SetLayerWeight(hitLayerIndex, 0.01f);
			}
		}
	}

	public virtual bool IsAnimationHarvestingPlaying()
	{
		return false;
	}

	public virtual void StartAnimationHarvesting(float _length, bool _weaponFireTrigger)
	{
	}

	public virtual bool IsAnimationDigRunning()
	{
		return false;
	}

	public virtual void StartAnimationDodge(float _blend)
	{
	}

	public virtual bool IsAnimationToDodge()
	{
		return false;
	}

	public virtual void StartAnimationJumping()
	{
	}

	public virtual void StartAnimationJump(AnimJumpMode jumpMode)
	{
	}

	public virtual bool IsAnimationJumpRunning()
	{
		return false;
	}

	public virtual void SetSwim(bool _enable)
	{
	}

	public virtual float GetAnimationElectrocuteRemaining()
	{
		return electrocuteTime;
	}

	public virtual void StartAnimationElectrocute(float _duration)
	{
		electrocuteTime = _duration;
	}

	public virtual void Electrocute(bool enabled)
	{
	}

	public virtual void StartAnimationReloading()
	{
	}

	public void SetReloadBool(bool value)
	{
		_setBool(reloadHash, value);
	}

	public virtual void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
	}

	public virtual bool IsAnimationUsePlaying()
	{
		return false;
	}

	public virtual void StartAnimationUse()
	{
	}

	public virtual void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
	}

	public virtual void SetAiming(bool _bEnable)
	{
	}

	public virtual void SetAlive()
	{
		if (anim != null)
		{
			_setBool(isAliveHash, _value: true);
		}
	}

	public virtual void SetCrouching(bool _bEnable)
	{
	}

	public virtual void SetDrunk(float _numBeers)
	{
	}

	public virtual void SetInRightHand(Transform _transform)
	{
	}

	public virtual void SetLookPosition(Vector3 _pos)
	{
	}

	public virtual void SetVehicleAnimation(int _animHash, int _pose)
	{
	}

	public virtual int GetVehicleAnimation()
	{
		if (TryGetInt(vehiclePoseHash, out var _value))
		{
			return _value;
		}
		return -1;
	}

	public virtual void SetRagdollEnabled(bool _b)
	{
	}

	public virtual void SetWalkingSpeed(float _f)
	{
	}

	public virtual void SetWalkType(int _walkType, bool _trigger = false)
	{
		_setInt(walkTypeHash, _walkType);
		if (_walkType >= 20)
		{
			_setFloat(walkTypeBlendHash, 1f);
		}
		else if (_walkType > 0)
		{
			_setFloat(walkTypeBlendHash, 0f);
		}
		if (_trigger)
		{
			_setTrigger(movementTriggerHash);
		}
	}

	public virtual void SetHeadAngles(float _nick, float _yaw)
	{
	}

	public virtual void SetArmsAngles(float _rightArmAngle, float _leftArmAngle)
	{
	}

	public abstract void SetVisible(bool _b);

	public virtual void SetArchetypeStance(NPCInfo.StanceTypes stance)
	{
	}

	public virtual void TriggerReaction(int reaction)
	{
		if (anim != null)
		{
			_setInt(reactionTypeHash, reaction);
			_setTrigger(reactionTriggerHash);
		}
	}

	public virtual void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (anim != null)
		{
			_setInt(sleeperPoseHash, pose);
			_setTrigger(sleeperTriggerHash);
		}
	}

	public virtual void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
	}

	public virtual void CrippleLimb(BodyDamage _bodyDamage, bool restoreState)
	{
	}

	public virtual void DismemberLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (_bodyDamage.IsCrippled)
		{
			CrippleLimb(_bodyDamage, restoreState);
		}
		if (_bodyDamage.bodyPartHit != EnumBodyPartHit.None)
		{
			RemoveLimb(_bodyDamage, restoreState);
		}
	}

	public virtual void TurnIntoCrawler(bool restoreState)
	{
	}

	public virtual void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
	{
	}

	public virtual void EndStun()
	{
	}

	public virtual bool IsAnimationStunRunning()
	{
		return false;
	}

	public virtual void StartEating()
	{
	}

	public virtual void StopEating()
	{
	}

	public virtual void PlayPlayerFPRevive()
	{
	}

	public virtual bool IsAnimationPlayerFPRevivePlaying()
	{
		return false;
	}

	public bool IsRootMotionForced()
	{
		if (anim != null)
		{
			return anim.GetFloat(forcedRootMotionHash) > 0f;
		}
		return false;
	}

	public bool IsAttackPrevented()
	{
		if (anim != null)
		{
			return anim.GetFloat(preventAttackHash) > 0f;
		}
		return false;
	}

	public virtual void SetFallAndGround(bool _canFall, bool _onGnd)
	{
		_setBool(canFallHash, _canFall, _netsync: false);
		_setBool(isOnGroundHash, _onGnd, _netsync: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		if (hasTurnRate)
		{
			float y = entity.transform.eulerAngles.y;
			float num = Mathf.DeltaAngle(y, turnRateFacing) * 50f;
			if ((num > 5f && turnRate >= 0f) || (num < -5f && turnRate <= 0f))
			{
				float num2 = Utils.FastAbs(num) - Utils.FastAbs(turnRate);
				if (num2 > 0f)
				{
					turnRate = Utils.FastLerpUnclamped(turnRate, num, 0.2f);
				}
				else if (num2 < -50f)
				{
					turnRate = Utils.FastLerpUnclamped(turnRate, num, 0.05f);
				}
			}
			else
			{
				turnRate *= 0.92f;
				turnRate = Utils.FastMoveTowards(turnRate, 0f, 2f);
			}
			turnRateFacing = y;
			_setFloat(turnRateHash, turnRate, _netSync: false);
		}
		updateNetworkAnimData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		float deltaTime = Time.deltaTime;
		if (electrocuteTime > 0f)
		{
			electrocuteTime -= deltaTime;
		}
		else if (electrocuteTime <= 0f)
		{
			Electrocute(enabled: false);
			electrocuteTime = 0f;
		}
		if (hitLayerIndex < 0)
		{
			return;
		}
		if (hitWeightTarget > 0f && hitWeight == hitWeightTarget)
		{
			if (hitDuration > 999f)
			{
				if (!IsAnimationHitRunning() || entity.IsDead() || entity.emodel.IsRagdollActive)
				{
					hitWeightDuration = 0.4f;
					hitWeightTarget = 0f;
				}
			}
			else if (hitWeightTarget > 0.15f)
			{
				hitWeightDuration = (hitDurationOut + 0.2f) / (hitWeight - 0.15f);
				hitWeightTarget = 0.15f;
			}
			else
			{
				hitWeightDuration = 4f;
				hitWeightTarget = 0f;
			}
		}
		if (hitWeight != hitWeightTarget)
		{
			hitWeight = Mathf.MoveTowards(hitWeight, hitWeightTarget, deltaTime / hitWeightDuration);
			anim.SetLayerWeight(hitLayerIndex, hitWeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processAnimParamData(List<AnimParamData> animationParameterData)
	{
		for (int i = 0; i < animationParameterData.Count; i++)
		{
			int nameHash = animationParameterData[i].NameHash;
			switch (animationParameterData[i].ValueType)
			{
			case AnimParamData.ValueTypes.Bool:
				UpdateBool(nameHash, animationParameterData[i].IntValue != 0);
				break;
			case AnimParamData.ValueTypes.Trigger:
				if (animationParameterData[i].IntValue != 0)
				{
					TriggerEvent(nameHash);
				}
				else
				{
					CancelEvent(nameHash);
				}
				break;
			case AnimParamData.ValueTypes.Float:
				UpdateFloat(nameHash, animationParameterData[i].FloatValue);
				break;
			case AnimParamData.ValueTypes.Int:
				UpdateInt(nameHash, animationParameterData[i].IntValue);
				break;
			case AnimParamData.ValueTypes.DataFloat:
				SetDataFloat((DataTypes)nameHash, animationParameterData[i].FloatValue);
				break;
			}
		}
	}

	public void TriggerEvent(string _property)
	{
		_setTrigger(_property);
	}

	public void TriggerEvent(int _pid)
	{
		_setTrigger(_pid);
	}

	public void CancelEvent(string _property)
	{
		_resetTrigger(_property);
	}

	public void CancelEvent(int _pid)
	{
		_resetTrigger(_pid);
	}

	public void UpdateFloat(string _property, float _value, bool _netsync = true)
	{
		_setFloat(_property, _value, _netsync);
	}

	public void UpdateFloat(int _pid, float _value, bool _netsync = true)
	{
		_setFloat(_pid, _value, _netsync);
	}

	public void UpdateBool(string _property, bool _value, bool _netsync = true)
	{
		_setBool(_property, _value, _netsync);
	}

	public void UpdateBool(int _pid, bool _value, bool _netsync = true)
	{
		_setBool(_pid, _value, _netsync);
	}

	public void UpdateInt(string _property, int _value, bool _netsync = true)
	{
		_setInt(_property, _value, _netsync);
	}

	public void UpdateInt(int _pid, int _value, bool _netsync = true)
	{
		_setInt(_pid, _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setTrigger(string _property, bool _netsync = true)
	{
		_setTrigger(Animator.StringToHash(_property), _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setTrigger(int _pid, bool _netsync = true)
	{
		if (anim != null)
		{
			anim.SetTrigger(_pid);
			if (!entity.isEntityRemote && _netsync)
			{
				changedAnimationParameters.Add(new AnimParamData(_pid, AnimParamData.ValueTypes.Trigger, _value: true));
			}
			OnTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTrigger(int _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _resetTrigger(string _property, bool _netsync = true)
	{
		_resetTrigger(Animator.StringToHash(_property), _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		if (anim != null && anim.gameObject.activeSelf && anim.GetBool(_propertyHash))
		{
			anim.ResetTrigger(_propertyHash);
			if (!entity.isEntityRemote && _netsync)
			{
				changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, _value: false));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setFloat(string _property, float _value, bool _netsync = true)
	{
		_setFloat(Animator.StringToHash(_property), _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setBool(string _property, bool _value, bool _netsync = true)
	{
		int propertyHash = Animator.StringToHash(_property);
		_setBool(propertyHash, _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setInt(string _property, int _value, bool _netsync = true)
	{
		_setInt(Animator.StringToHash(_property), _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setFloat(int _propertyHash, float _value, bool _netSync = true)
	{
		if (!anim)
		{
			return;
		}
		if (!_netSync)
		{
			anim.SetFloat(_propertyHash, _value);
			return;
		}
		float num = anim.GetFloat(_propertyHash) - _value;
		if (num * num > 0.0001f)
		{
			anim.SetFloat(_propertyHash, _value);
			if (!entity.isEntityRemote && _netSync)
			{
				changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Float, _value));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		if (anim != null && anim.GetBool(_propertyHash) != _value)
		{
			anim.SetBool(_propertyHash, _value);
			if (_propertyHash != isFPVHash && !entity.isEntityRemote && _netsync)
			{
				changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Bool, _value));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		if (anim != null && anim.GetInteger(_propertyHash) != _value)
		{
			anim.SetInteger(_propertyHash, _value);
			if (!entity.isEntityRemote && _netsync)
			{
				changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Int, _value));
			}
		}
	}

	public virtual void SetDataFloat(DataTypes _type, float _value, bool _netsync = true)
	{
		if (_type == DataTypes.HitDuration)
		{
			InitHitDuration(_value);
		}
		if (!entity.isEntityRemote && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData((int)_type, AnimParamData.ValueTypes.DataFloat, _value));
		}
	}

	public virtual bool TryGetTrigger(string _property, out bool _value)
	{
		return TryGetTrigger(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetFloat(string _property, out float _value)
	{
		return TryGetFloat(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetBool(string _property, out bool _value)
	{
		return TryGetBool(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetInt(string _property, out int _value)
	{
		return TryGetInt(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetTrigger(int _propertyHash, out bool _value)
	{
		if (anim == null)
		{
			return _value = false;
		}
		_value = anim.GetBool(_propertyHash);
		return true;
	}

	public virtual bool TryGetFloat(int _propertyHash, out float _value)
	{
		if (anim == null)
		{
			_value = 0f;
			return false;
		}
		_value = anim.GetFloat(_propertyHash);
		return true;
	}

	public virtual bool TryGetBool(int _propertyHash, out bool _value)
	{
		if (anim == null)
		{
			return _value = false;
		}
		_value = anim.GetBool(_propertyHash);
		return true;
	}

	public virtual bool TryGetInt(int _propertyHash, out int _value)
	{
		if (anim == null)
		{
			_value = 0;
			return false;
		}
		_value = anim.GetInteger(_propertyHash);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateNetworkAnimData()
	{
		if (entity == null)
		{
			return;
		}
		if (entity.isEntityRemote)
		{
			if (queuedAnimParams.Count > 0)
			{
				do
				{
					processAnimParamData(queuedAnimParams[0]);
					queuedAnimParams.RemoveAt(0);
				}
				while (queuedAnimParams.Count > 10);
			}
			return;
		}
		foreach (List<AnimParamData> parameterList in changedAnimationParameters.GetParameterLists())
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(entity.entityId, parameterList), _onlyClientsAttachedToAnEntity: false, -1, entity.entityId, entity.entityId);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(entity.entityId, parameterList));
			}
		}
	}

	public void SyncAnimParameters(int _toEntityId)
	{
		if (!anim)
		{
			return;
		}
		Dictionary<int, AnimParamData> dictionary = new Dictionary<int, AnimParamData>();
		AnimatorControllerParameter[] parameters = anim.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			switch (animatorControllerParameter.type)
			{
			case AnimatorControllerParameterType.Bool:
			{
				bool value2 = anim.GetBool(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Bool, value2);
				break;
			}
			case AnimatorControllerParameterType.Int:
			{
				int integer = anim.GetInteger(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Int, integer);
				break;
			}
			case AnimatorControllerParameterType.Float:
			{
				float value = anim.GetFloat(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Float, value);
				break;
			}
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(entity.entityId, dictionary), _onlyClientsAttachedToAnEntity: false, _toEntityId);
	}

	public virtual string GetParameterName(int _nameHash)
	{
		AnimatorControllerParameter[] parameters = anim.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.nameHash == _nameHash)
			{
				return animatorControllerParameter.name;
			}
		}
		return "?";
	}

	public void SetAnimParameters(List<AnimParamData> animationParameterData)
	{
		queuedAnimParams.Add(animationParameterData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarController()
	{
	}
}
