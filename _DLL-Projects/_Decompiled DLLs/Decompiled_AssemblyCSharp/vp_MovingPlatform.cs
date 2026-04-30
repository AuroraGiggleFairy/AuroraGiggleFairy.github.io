using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class vp_MovingPlatform : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class WaypointComparer : IComparer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		int IComparer.Compare(object x, object y)
		{
			return new CaseInsensitiveComparer().Compare(((Transform)x).name, ((Transform)y).name);
		}
	}

	public enum PathMoveType
	{
		PingPong,
		Loop,
		Target
	}

	public enum Direction
	{
		Forward,
		Backwards,
		Direct
	}

	public enum MovementInterpolationMode
	{
		EaseInOut,
		EaseIn,
		EaseOut,
		EaseOut2,
		Slerp,
		Lerp
	}

	public enum RotateInterpolationMode
	{
		SyncToMovement,
		EaseOut,
		CustomEaseOut,
		CustomRotate
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	public PathMoveType PathType;

	public GameObject PathWaypoints;

	public Direction PathDirection;

	public int MoveAutoStartTarget = 1000;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Transform> m_Waypoints = new List<Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_NextWaypoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentTargetPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentTargetAngle = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_TargetedWayPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_TravelDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_OriginalAngle = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_CurrentWaypoint;

	public float MoveSpeed = 0.1f;

	public float MoveReturnDelay;

	public float MoveCooldown;

	public MovementInterpolationMode MoveInterpolationMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Moving;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NextAllowedMoveTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MoveTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_ReturnDelayTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PrevPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve m_EaseInOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float RotationEaseAmount = 0.1f;

	public Vector3 RotationSpeed = Vector3.zero;

	public RotateInterpolationMode RotationInterpolationMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PrevAngle = Vector3.zero;

	public AudioClip SoundStart;

	public AudioClip SoundStop;

	public AudioClip SoundMove;

	public AudioClip SoundWaypoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public bool PhysicsSnapPlayerToTopOnIntersect = true;

	public float m_PhysicsPushForce = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_RigidBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider m_Collider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider m_PlayerCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_PlayerToPush;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_PhysicsCurrentMoveVelocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_PhysicsCurrentRotationVelocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Collider, vp_PlayerEventHandler> m_KnownPlayers = new Dictionary<Collider, vp_PlayerEventHandler>();

	public int TargetedWaypoint => m_TargetedWayPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_Transform = base.transform;
		m_Collider = GetComponentInChildren<Collider>();
		m_RigidBody = GetComponent<Rigidbody>();
		m_RigidBody.useGravity = false;
		m_RigidBody.isKinematic = true;
		m_NextWaypoint = 0;
		m_Audio = GetComponent<AudioSource>();
		m_Audio.loop = true;
		m_Audio.clip = SoundMove;
		if (PathWaypoints == null)
		{
			return;
		}
		base.gameObject.layer = 28;
		foreach (Transform item in PathWaypoints.transform)
		{
			if (vp_Utility.IsActive(item.gameObject))
			{
				m_Waypoints.Add(item);
				item.gameObject.layer = 28;
			}
			if (item.GetComponent<Renderer>() != null)
			{
				item.GetComponent<Renderer>().enabled = false;
			}
			if (item.GetComponent<Collider>() != null)
			{
				item.GetComponent<Collider>().enabled = false;
			}
		}
		IComparer comparer = new WaypointComparer();
		m_Waypoints.Sort(comparer.Compare);
		if (m_Waypoints.Count > 0)
		{
			m_CurrentTargetPosition = m_Waypoints[m_NextWaypoint].position;
			m_CurrentTargetAngle = m_Waypoints[m_NextWaypoint].eulerAngles;
			m_Transform.position = m_CurrentTargetPosition;
			m_Transform.eulerAngles = m_CurrentTargetAngle;
			if (MoveAutoStartTarget > m_Waypoints.Count - 1)
			{
				MoveAutoStartTarget = m_Waypoints.Count - 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		UpdatePath();
		UpdateMovement();
		UpdateRotation();
		UpdateVelocity();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdatePath()
	{
		if (m_Waypoints.Count < 2 || !(GetDistanceLeft() < 0.01f) || !(Time.time >= m_NextAllowedMoveTime))
		{
			return;
		}
		switch (PathType)
		{
		case PathMoveType.Target:
			if (m_NextWaypoint == m_TargetedWayPoint)
			{
				if (m_Moving)
				{
					OnStop();
				}
				else if (m_NextWaypoint != 0)
				{
					OnArriveAtDestination();
				}
				break;
			}
			if (m_Moving)
			{
				if (m_PhysicsCurrentMoveVelocity == 0f)
				{
					OnStart();
				}
				else
				{
					OnArriveAtWaypoint();
				}
			}
			GoToNextWaypoint();
			break;
		case PathMoveType.Loop:
			OnArriveAtWaypoint();
			GoToNextWaypoint();
			break;
		case PathMoveType.PingPong:
			if (PathDirection == Direction.Backwards)
			{
				if (m_NextWaypoint == 0)
				{
					PathDirection = Direction.Forward;
				}
			}
			else if (m_NextWaypoint == m_Waypoints.Count - 1)
			{
				PathDirection = Direction.Backwards;
			}
			OnArriveAtWaypoint();
			GoToNextWaypoint();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnStart()
	{
		if (SoundStart != null)
		{
			m_Audio.PlayOneShot(SoundStart);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnArriveAtWaypoint()
	{
		if (SoundWaypoint != null)
		{
			m_Audio.PlayOneShot(SoundWaypoint);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnArriveAtDestination()
	{
		if (MoveReturnDelay > 0f && !m_ReturnDelayTimer.Active)
		{
			vp_Timer.In(MoveReturnDelay, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				GoTo(0);
			}, m_ReturnDelayTimer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnStop()
	{
		m_Audio.Stop();
		if (SoundStop != null)
		{
			m_Audio.PlayOneShot(SoundStop);
		}
		m_Transform.position = m_CurrentTargetPosition;
		m_Transform.eulerAngles = m_CurrentTargetAngle;
		m_Moving = false;
		if (m_NextWaypoint == 0)
		{
			m_NextAllowedMoveTime = Time.time + MoveCooldown;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateMovement()
	{
		if (m_Waypoints.Count >= 2)
		{
			switch (MoveInterpolationMode)
			{
			case MovementInterpolationMode.EaseInOut:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_Transform.position, m_CurrentTargetPosition, m_EaseInOutCurve.Evaluate(m_MoveTime)));
				break;
			case MovementInterpolationMode.EaseIn:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.MoveTowards(m_Transform.position, m_CurrentTargetPosition, m_MoveTime));
				break;
			case MovementInterpolationMode.EaseOut:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_Transform.position, m_CurrentTargetPosition, m_LinearCurve.Evaluate(m_MoveTime)));
				break;
			case MovementInterpolationMode.EaseOut2:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_Transform.position, m_CurrentTargetPosition, MoveSpeed * 0.25f));
				break;
			case MovementInterpolationMode.Lerp:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.MoveTowards(m_Transform.position, m_CurrentTargetPosition, MoveSpeed));
				break;
			case MovementInterpolationMode.Slerp:
				m_Transform.position = vp_MathUtility.NaNSafeVector3(Vector3.Slerp(m_Transform.position, m_CurrentTargetPosition, m_LinearCurve.Evaluate(m_MoveTime)));
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateRotation()
	{
		switch (RotationInterpolationMode)
		{
		case RotateInterpolationMode.SyncToMovement:
			if (m_Moving)
			{
				m_Transform.eulerAngles = vp_MathUtility.NaNSafeVector3(new Vector3(Mathf.LerpAngle(m_OriginalAngle.x, m_CurrentTargetAngle.x, 1f - GetDistanceLeft() / m_TravelDistance), Mathf.LerpAngle(m_OriginalAngle.y, m_CurrentTargetAngle.y, 1f - GetDistanceLeft() / m_TravelDistance), Mathf.LerpAngle(m_OriginalAngle.z, m_CurrentTargetAngle.z, 1f - GetDistanceLeft() / m_TravelDistance)));
			}
			break;
		case RotateInterpolationMode.EaseOut:
			m_Transform.eulerAngles = vp_MathUtility.NaNSafeVector3(new Vector3(Mathf.LerpAngle(m_Transform.eulerAngles.x, m_CurrentTargetAngle.x, m_LinearCurve.Evaluate(m_MoveTime)), Mathf.LerpAngle(m_Transform.eulerAngles.y, m_CurrentTargetAngle.y, m_LinearCurve.Evaluate(m_MoveTime)), Mathf.LerpAngle(m_Transform.eulerAngles.z, m_CurrentTargetAngle.z, m_LinearCurve.Evaluate(m_MoveTime))));
			break;
		case RotateInterpolationMode.CustomEaseOut:
			m_Transform.eulerAngles = vp_MathUtility.NaNSafeVector3(new Vector3(Mathf.LerpAngle(m_Transform.eulerAngles.x, m_CurrentTargetAngle.x, RotationEaseAmount), Mathf.LerpAngle(m_Transform.eulerAngles.y, m_CurrentTargetAngle.y, RotationEaseAmount), Mathf.LerpAngle(m_Transform.eulerAngles.z, m_CurrentTargetAngle.z, RotationEaseAmount)));
			break;
		case RotateInterpolationMode.CustomRotate:
			m_Transform.Rotate(RotationSpeed);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateVelocity()
	{
		m_MoveTime += MoveSpeed * 0.01f * vp_TimeUtility.AdjustedTimeScale;
		m_PhysicsCurrentMoveVelocity = (m_Transform.position - m_PrevPos).magnitude;
		m_PhysicsCurrentRotationVelocity = (m_Transform.eulerAngles - m_PrevAngle).magnitude;
		m_PrevPos = m_Transform.position;
		m_PrevAngle = m_Transform.eulerAngles;
	}

	public void GoTo(int targetWayPoint)
	{
		if (Time.time < m_NextAllowedMoveTime || PathType != PathMoveType.Target)
		{
			return;
		}
		m_TargetedWayPoint = GetValidWaypoint(targetWayPoint);
		if (targetWayPoint > m_NextWaypoint)
		{
			if (PathDirection != Direction.Direct)
			{
				PathDirection = Direction.Forward;
			}
		}
		else if (PathDirection != Direction.Direct)
		{
			PathDirection = Direction.Backwards;
		}
		m_Moving = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float GetDistanceLeft()
	{
		if (m_Waypoints.Count < 2)
		{
			return 0f;
		}
		return Vector3.Distance(m_Transform.position, m_Waypoints[m_NextWaypoint].position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void GoToNextWaypoint()
	{
		if (m_Waypoints.Count >= 2)
		{
			m_MoveTime = 0f;
			if (!m_Audio.isPlaying)
			{
				m_Audio.Play();
			}
			m_CurrentWaypoint = m_NextWaypoint;
			switch (PathDirection)
			{
			case Direction.Forward:
				m_NextWaypoint = GetValidWaypoint(m_NextWaypoint + 1);
				break;
			case Direction.Backwards:
				m_NextWaypoint = GetValidWaypoint(m_NextWaypoint - 1);
				break;
			case Direction.Direct:
				m_NextWaypoint = m_TargetedWayPoint;
				break;
			}
			m_OriginalAngle = m_CurrentTargetAngle;
			m_CurrentTargetPosition = m_Waypoints[m_NextWaypoint].position;
			m_CurrentTargetAngle = m_Waypoints[m_NextWaypoint].eulerAngles;
			m_TravelDistance = GetDistanceLeft();
			m_Moving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetValidWaypoint(int wayPoint)
	{
		if (wayPoint < 0)
		{
			return m_Waypoints.Count - 1;
		}
		if (wayPoint > m_Waypoints.Count - 1)
		{
			return 0;
		}
		return wayPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnTriggerEnter(Collider col)
	{
		if (GetPlayer(col))
		{
			TryPushPlayer();
			TryAutoStart();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnTriggerStay(Collider col)
	{
		if (PhysicsSnapPlayerToTopOnIntersect && GetPlayer(col))
		{
			TrySnapPlayerToTop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool GetPlayer(Collider col)
	{
		if (!m_KnownPlayers.ContainsKey(col))
		{
			if (col.gameObject.layer != 30)
			{
				return false;
			}
			vp_PlayerEventHandler component = col.transform.root.GetComponent<vp_PlayerEventHandler>();
			if (component == null)
			{
				return false;
			}
			m_KnownPlayers.Add(col, component);
		}
		if (!m_KnownPlayers.TryGetValue(col, out m_PlayerToPush))
		{
			return false;
		}
		m_PlayerCollider = col;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TryPushPlayer()
	{
		if (!(m_PlayerToPush == null) && m_PlayerToPush.Platform != null && !(m_PlayerToPush.Position.Get().y > m_Collider.bounds.max.y) && !(m_PlayerToPush.Platform.Get() == m_Transform))
		{
			float num = m_PhysicsCurrentMoveVelocity;
			if (num == 0f)
			{
				num = m_PhysicsCurrentRotationVelocity * 0.1f;
			}
			if (num > 0f)
			{
				m_PlayerToPush.ForceImpact.Send(vp_3DUtility.HorizontalVector(-(m_Transform.position - m_PlayerCollider.bounds.center).normalized * num * m_PhysicsPushForce));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TrySnapPlayerToTop()
	{
		if (!(m_PlayerToPush == null) && m_PlayerToPush.Platform != null && !(m_PlayerToPush.Position.Get().y > m_Collider.bounds.max.y) && !(m_PlayerToPush.Platform.Get() == m_Transform) && RotationSpeed.x == 0f && RotationSpeed.z == 0f && m_CurrentTargetAngle.x == 0f && m_CurrentTargetAngle.z == 0f && !(m_Collider.bounds.max.x < m_PlayerCollider.bounds.max.x) && !(m_Collider.bounds.max.z < m_PlayerCollider.bounds.max.z) && !(m_Collider.bounds.min.x > m_PlayerCollider.bounds.min.x) && !(m_Collider.bounds.min.z > m_PlayerCollider.bounds.min.z))
		{
			Vector3 o = m_PlayerToPush.Position.Get();
			o.y = m_Collider.bounds.max.y - 0.1f;
			m_PlayerToPush.Position.Set(o);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TryAutoStart()
	{
		if (MoveAutoStartTarget != 0 && !(m_PhysicsCurrentMoveVelocity > 0f) && !m_Moving)
		{
			GoTo(MoveAutoStartTarget);
		}
	}
}
