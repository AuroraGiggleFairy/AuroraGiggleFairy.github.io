using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimatorDoorState : StateMachineBehaviour
{
	[Range(0f, 1f)]
	public float collideOffPercent;

	[Range(0f, 1f)]
	public float collideOnPercent = 0.99f;

	public bool disableColliderOnObstacleDetection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int openHash = Animator.StringToHash("Open");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int isOpenHash = Animator.StringToHash("IsOpen");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCollisionRules[] rules;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Collider[] colliders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int colliderState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpenAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool matchedEnterState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopNext;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!matchDoorState(animator, stateInfo))
		{
			return;
		}
		matchedEnterState = true;
		colliders = animator.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		rules = new EntityCollisionRules[colliders.Length];
		for (int num = colliders.Length - 1; num >= 0; num--)
		{
			Collider collider = colliders[num];
			rules[num] = collider.GetComponent<EntityCollisionRules>();
			if (!isOpenAnim)
			{
				EntityCollisionRules entityCollisionRules = rules[num];
				if ((bool)entityCollisionRules && entityCollisionRules.IsAnimPush)
				{
					collider.enabled = false;
				}
			}
		}
		colliderState = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool matchDoorState(Animator animator, AnimatorStateInfo stateInfo)
	{
		bool num = animator.GetBool(isOpenHash);
		isOpenAnim = stateInfo.shortNameHash == openHash;
		return num == isOpenAnim;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!matchedEnterState || !matchDoorState(animator, stateInfo))
		{
			return;
		}
		if (stopNext)
		{
			stopNext = false;
			matchedEnterState = false;
			colliderState = 1;
			EnableColliders(_on: true);
			animator.enabled = false;
		}
		else
		{
			if (animator.IsInTransition(layerIndex))
			{
				return;
			}
			float normalizedTime = stateInfo.normalizedTime;
			if (normalizedTime >= collideOnPercent)
			{
				if (colliderState != 1)
				{
					if (CheckForObstacles())
					{
						return;
					}
					colliderState = 1;
					EnableColliders(_on: true);
				}
				else if (disableColliderOnObstacleDetection && CheckForObstacles())
				{
					colliderState = 0;
					EnableColliders(_on: false);
				}
				if (normalizedTime >= 0.99f)
				{
					stopNext = true;
				}
			}
			else if (normalizedTime < 1f && normalizedTime >= collideOffPercent)
			{
				if (colliderState != 0)
				{
					colliderState = 0;
					EnableColliders(_on: false);
				}
				if (!isOpenAnim)
				{
					PushPlayers(normalizedTime);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableColliders(bool _on)
	{
		if (colliders == null)
		{
			return;
		}
		for (int num = colliders.Length - 1; num >= 0; num--)
		{
			EntityCollisionRules entityCollisionRules = rules[num];
			if ((bool)entityCollisionRules)
			{
				if (entityCollisionRules.IsStatic)
				{
					continue;
				}
				if (entityCollisionRules.IsAnimPush)
				{
					colliders[num].enabled = !_on;
					continue;
				}
			}
			colliders[num].enabled = _on;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForObstacles()
	{
		if (colliders == null)
		{
			return false;
		}
		Ray ray = new Ray(Vector3.zero, Vector3.up);
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Vector3 origin = list[num].position - Origin.position;
			origin.y += 0.35f;
			ray.origin = origin;
			for (int num2 = colliders.Length - 1; num2 >= 0; num2--)
			{
				EntityCollisionRules entityCollisionRules = rules[num2];
				if (!entityCollisionRules || (!entityCollisionRules.IsStatic && !entityCollisionRules.IsAnimPush))
				{
					Collider obj = colliders[num2];
					bool enabled = obj.enabled;
					obj.enabled = true;
					RaycastHit hitInfo;
					bool flag = obj.Raycast(ray, out hitInfo, 0.9f);
					obj.enabled = enabled;
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PushPlayers(float _normalizedTime)
	{
		if (colliders == null || _normalizedTime < 0.5f)
		{
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			EntityPlayer entityPlayer = list[num];
			Vector3 vector = entityPlayer.position - Origin.position;
			vector.y += 0.1f;
			float radius = entityPlayer.m_characterController.GetRadius();
			radius *= radius;
			for (int num2 = colliders.Length - 1; num2 >= 0; num2--)
			{
				EntityCollisionRules entityCollisionRules = rules[num2];
				if ((bool)entityCollisionRules && entityCollisionRules.IsAnimPush)
				{
					Collider collider = colliders[num2];
					Vector3 vector2 = collider.ClosestPoint(vector);
					Vector3 vector3 = vector - vector2;
					vector3.y = 0f;
					float sqrMagnitude = vector3.sqrMagnitude;
					if (sqrMagnitude < radius)
					{
						float num3 = 0.002f;
						if (sqrMagnitude == 0f)
						{
							vector3 = collider.transform.forward * -1f;
							if (_normalizedTime >= 0.94f)
							{
								num3 *= 7f;
							}
						}
						vector3 = vector3.normalized * num3;
						entityPlayer.PhysicsPush(vector3, Vector3.zero, affectLocalPlayerController: true);
					}
				}
			}
		}
	}

	[Conditional("DEBUG_DOOR")]
	public void LogDoor(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Door {_format}";
		Log.Warning(_format, _args);
	}
}
