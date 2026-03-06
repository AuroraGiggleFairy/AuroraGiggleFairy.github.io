using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class GameObjectAnimalAnimation : AvatarController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum State
	{
		None,
		Attack,
		Idle,
		Jump,
		Pain,
		Run,
		Swim,
		Walk
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimIdle1 = "Idle1";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimIdle2 = "Idle2";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimAttack1 = "Attack1";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimAttack2 = "Attack2";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimPain = "Pain";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimJump = "Jump";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimDeath = "Death";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimRun = "Run";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimWalk = "Walk";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAnimSwim = "Swim";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform figureT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public new Animation anim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationState attack1AS;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationState attack2AS;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool visInit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_bVisible;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDead;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAlwaysWalk;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastAbsMotionX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastAbsMotionZ;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastAbsMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stepSoundCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public State state;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		parentT = EModelBase.FindModel(base.transform);
		for (int num = parentT.childCount - 1; num >= 0; num--)
		{
			figureT = parentT.GetChild(num);
			if (figureT.gameObject.activeSelf)
			{
				break;
			}
		}
		anim = figureT.GetComponent<Animation>();
		if ((bool)anim["Idle1"])
		{
			anim.Play("Idle1");
		}
		attack1AS = anim["Attack1"];
		attack2AS = anim["Attack2"];
	}

	public void SetAlwaysWalk(bool _b)
	{
		bAlwaysWalk = _b;
	}

	public override bool IsAnimationAttackPlaying()
	{
		if (!(attack1AS != null) || !attack1AS.enabled)
		{
			if (attack2AS != null)
			{
				return attack2AS.enabled;
			}
			return false;
		}
		return true;
	}

	public override void StartAnimationAttack()
	{
		state = State.Attack;
		if (!(attack1AS != null))
		{
			return;
		}
		if (attack2AS != null)
		{
			if (entity.rand.RandomFloat > 0.5f)
			{
				anim.Play("Attack1");
			}
			else
			{
				anim.Play("Attack2");
			}
		}
		else
		{
			anim.Play("Attack1");
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!isDead)
		{
			state = State.Pain;
			if ((bool)anim["Pain"])
			{
				anim.Play("Pain");
			}
		}
	}

	public override void StartAnimationJumping()
	{
		if (!entity.IsSwimming() && anim["Jump"] != null)
		{
			state = State.Jump;
			anim.CrossFade("Jump", 0.2f);
		}
	}

	public override void SetVisible(bool _b)
	{
		if (m_bVisible == _b && visInit)
		{
			return;
		}
		m_bVisible = _b;
		visInit = true;
		Transform transform = parentT;
		if ((bool)transform)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = _b;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		if (!m_bVisible || entity == null)
		{
			return;
		}
		if (entity.IsDead())
		{
			if (!isDead)
			{
				isDead = true;
				anim.Stop();
				if ((bool)anim["Death"])
				{
					anim.CrossFade("Death", 0.5f);
				}
			}
		}
		else
		{
			if (entity.Jumping || (!(attack1AS == null) && attack1AS.enabled) || (!(attack2AS == null) && attack2AS.enabled) || (!(anim["Death"] == null) && anim["Death"].enabled) || (!(anim["Pain"] == null) && anim["Pain"].enabled))
			{
				return;
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = lastAbsMotion;
			num = Mathf.Abs(entity.position.x - entity.lastTickPos[0].x) * 6f;
			num2 = Mathf.Abs(entity.position.z - entity.lastTickPos[0].z) * 6f;
			if (!entity.isEntityRemote)
			{
				if (Mathf.Abs(num - lastAbsMotionX) > 0.01f || Mathf.Abs(num2 - lastAbsMotionZ) > 0.01f)
				{
					num3 = Mathf.Sqrt(num * num + num2 * num2);
					lastAbsMotionX = num;
					lastAbsMotionZ = num2;
					lastAbsMotion = num3;
				}
			}
			else if (num > lastAbsMotionX || num2 > lastAbsMotionZ)
			{
				num3 = Mathf.Sqrt(num * num + num2 * num2);
				lastAbsMotionX = num;
				lastAbsMotionZ = num2;
				lastAbsMotion = num3;
			}
			else
			{
				lastAbsMotionX *= 0.9f;
				lastAbsMotionZ *= 0.9f;
				lastAbsMotion *= 0.9f;
			}
			if (bAlwaysWalk || num3 > 0.15f)
			{
				if (entity.IsSwimming() && anim["Swim"] != null)
				{
					state = State.Swim;
					if (!anim["Swim"].enabled)
					{
						anim.Play("Swim");
					}
					anim["Swim"].speed = Mathf.Clamp01(num3 * 2f);
					return;
				}
				if (num3 >= 1f)
				{
					if (state != State.Run)
					{
						state = State.Run;
						AnimationState animationState = anim["Run"];
						if (!animationState.enabled)
						{
							anim.CrossFade("Run", 0.5f);
						}
						animationState.speed = Utils.FastMin(num3, 1.5f);
					}
				}
				else if (state != State.Run)
				{
					state = State.Walk;
					AnimationState animationState2 = anim["Walk"];
					if (!animationState2.enabled)
					{
						anim.CrossFade("Walk", 0.5f);
					}
					animationState2.speed = num3 * 2f;
				}
				if (stepSoundCounter <= 0f)
				{
					stepSoundCounter = 0.3f;
				}
				return;
			}
			state = State.Idle;
			if (anim["Idle2"] != null)
			{
				if (!anim["Idle1"].enabled && !anim["Idle2"].enabled)
				{
					if (entity.rand.RandomFloat > 0.5f)
					{
						anim.CrossFade("Idle1", 0.5f);
					}
					else
					{
						anim.CrossFade("Idle2", 0.5f);
					}
				}
			}
			else if (!anim["Idle1"].enabled)
			{
				anim.CrossFade("Idle1", 0.5f);
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return figureT;
	}
}
