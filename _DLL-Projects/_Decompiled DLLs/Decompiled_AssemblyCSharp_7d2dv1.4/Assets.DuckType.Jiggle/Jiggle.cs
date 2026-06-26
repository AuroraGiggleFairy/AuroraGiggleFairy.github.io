using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.DuckType.Jiggle;

public class Jiggle : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float TORQUE_FACTOR = 0.001f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_Initialised;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion m_RestLocalRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion m_LastWorldRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion m_Torque = Quaternion.identity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_LastCenterOfMassWorld;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_NoisePhase;

	public bool UpdateWithPhysics;

	public bool UseCenterOfMass = true;

	public Vector3 CenterOfMass = new Vector3(1f, 0f, 0f);

	public float CenterOfMassInertia = 1f;

	public bool AddWind;

	public Vector3 WindDirection = new Vector3(1f, 0f, 0f);

	public float WindStrength = 1f;

	public bool AddNoise;

	public float NoiseStrength = 1f;

	public float NoiseScale = 1f;

	public float NoiseSpeed = 1f;

	public float RotationInertia = 1f;

	public float Gravity;

	public float SpringStrength = 0.4f;

	public float Dampening = 0.4f;

	public bool BlendToOriginalRotation;

	public bool Hinge;

	public float HingeAngle;

	public bool UseAngleLimit;

	public float AngleLimit = 180f;

	public bool UseSoftLimit;

	public float SoftLimitInfluence = 0.5f;

	public float SoftLimitStrength = 0.5f;

	public bool ShowViewportGizmos = true;

	public float GizmoScale = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmos()
	{
		if (ShowViewportGizmos)
		{
			Vector3 vector = base.transform.localToWorldMatrix.MultiplyPoint3x4(CenterOfMass);
			Gizmos.color = Color.green;
			if (UseCenterOfMass)
			{
				Gizmos.DrawSphere(vector, CenterOfMassInertia * 5f * GizmoScale);
				Gizmos.DrawLine(base.transform.position, vector);
			}
			if (Hinge)
			{
				DrawGizmosArc(base.transform.position, base.transform.position + GetRestRotationWorld() * CenterOfMass * 11f * GizmoScale, GetHingeNormalWorld(), 360f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmosSelected()
	{
		if (!ShowViewportGizmos || !(UseAngleLimit & (AngleLimit > 0f)))
		{
			return;
		}
		float num = 10f * GizmoScale;
		Vector3 vector = GetRestRotationWorld() * CenterOfMass;
		List<Vector3> list;
		if (Hinge)
		{
			Vector3 vector2 = Vector3.Cross(vector, GetHingeNormalWorld());
			list = new List<Vector3>
			{
				vector2,
				-vector2
			};
		}
		else
		{
			list = vector.GetOrthogonalVectors(12);
		}
		foreach (Vector3 item in list)
		{
			Gizmos.color = Color.red;
			Vector3 vector3 = ((AngleLimit < 90f) ? Vector3.RotateTowards(item, base.transform.rotation * CenterOfMass, (90f - AngleLimit) * (MathF.PI / 180f), 1f) : Vector3.RotateTowards(item, base.transform.rotation * -CenterOfMass, (AngleLimit - 90f) * (MathF.PI / 180f), 1f));
			vector3 *= num;
			Gizmos.DrawRay(base.transform.position, vector3);
			if (UseSoftLimit)
			{
				Gizmos.color = Color.yellow;
				Vector3 startPoint = base.transform.position + vector3;
				DrawGizmosArc(base.transform.position, startPoint, Vector3.Cross(vector3, vector), AngleLimit * SoftLimitInfluence);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_Initialised = true;
		m_RestLocalRotation = base.transform.localRotation;
		m_LastWorldRotation = base.transform.rotation;
		m_LastCenterOfMassWorld = base.transform.localToWorldMatrix.MultiplyPoint3x4(CenterOfMass);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		JiggleScheduler.Update(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		JiggleScheduler.Register(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		JiggleScheduler.Deregister(this);
	}

	public void ScheduledUpdate(float deltaTime)
	{
		Quaternion quaternion = ((!(RotationInertia > 0f)) ? base.transform.rotation : Quaternion.SlerpUnclamped(base.transform.rotation, m_LastWorldRotation, RotationInertia));
		if (UseCenterOfMass && CenterOfMassInertia > 0f)
		{
			Quaternion source = Quaternion.FromToRotation(quaternion * CenterOfMass, m_LastCenterOfMassWorld - base.transform.position);
			Debug.DrawLine(m_LastCenterOfMassWorld, base.transform.position);
			quaternion = source.Scale(CenterOfMassInertia) * quaternion;
		}
		quaternion *= m_Torque.Scale(deltaTime / 0.001f);
		Quaternion quaternion2 = ((base.transform.parent != null) ? base.transform.parent.rotation : Quaternion.identity) * m_RestLocalRotation;
		if (BlendToOriginalRotation)
		{
			quaternion2 = base.transform.rotation;
		}
		float num = Quaternion.Angle(quaternion, quaternion2);
		if (UseAngleLimit && num > AngleLimit)
		{
			quaternion = Quaternion.Slerp(quaternion2, quaternion, AngleLimit / num);
		}
		if (Hinge)
		{
			Vector3 vector = quaternion * CenterOfMass;
			Vector3 hingeNormalWorld = GetHingeNormalWorld();
			Vector3 toDirection = Vector3.Cross(hingeNormalWorld, Vector3.Cross(vector, hingeNormalWorld));
			quaternion = Quaternion.FromToRotation(vector, toDirection) * quaternion;
		}
		base.transform.rotation = quaternion;
		if (SpringStrength > 0f)
		{
			Quaternion source2 = base.transform.rotation.FromToRotation(quaternion2);
			source2 = source2.Scale(0.001f * SpringStrength * 250f * deltaTime);
			m_Torque = m_Torque.Append(source2);
		}
		if (UseCenterOfMass)
		{
			if (Gravity > 0f)
			{
				Quaternion closestRotationFromTo = GetClosestRotationFromTo(base.transform.rotation * CenterOfMass, Vector3.down);
				m_Torque = m_Torque.Append(closestRotationFromTo.Scale(0.001f * Gravity * 50f * deltaTime));
			}
			if (AddWind)
			{
				Quaternion closestRotationFromTo2 = GetClosestRotationFromTo(base.transform.rotation * CenterOfMass, WindDirection);
				m_Torque = m_Torque.Append(closestRotationFromTo2.Scale(0.001f * WindStrength * 50f * deltaTime));
			}
			if (AddNoise)
			{
				Vector3 noiseVector = GetNoiseVector(base.transform.localToWorldMatrix.MultiplyPoint3x4(CenterOfMass), NoiseScale * 10f, m_NoisePhase += deltaTime * NoiseSpeed);
				Quaternion closestRotationFromTo3 = GetClosestRotationFromTo(base.transform.rotation * CenterOfMass, noiseVector);
				m_Torque = m_Torque.Append(closestRotationFromTo3.Scale(0.001f * NoiseStrength * 50f * deltaTime));
			}
		}
		if (UseSoftLimit && UseAngleLimit && AngleLimit > 0f && SoftLimitStrength > 0f)
		{
			num = Quaternion.Angle(quaternion, quaternion2);
			float num2 = AngleLimit * (1f - SoftLimitInfluence);
			if (num > num2)
			{
				float num3 = Mathf.Min((num - num2) / (AngleLimit - num2), 1f);
				Quaternion source3 = base.transform.rotation.FromToRotation(quaternion2);
				m_Torque = m_Torque.Append(source3.Scale(0.001f * num3 * SoftLimitStrength * 250f * deltaTime));
			}
		}
		m_Torque = m_Torque.Scale((1f - Dampening * 10f * deltaTime).Clamp01());
		m_LastCenterOfMassWorld = base.transform.localToWorldMatrix.MultiplyPoint3x4(CenterOfMass);
		m_LastWorldRotation = base.transform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetHingeNormalWorld()
	{
		Vector3 vector = ((Mathf.Abs(CenterOfMass.normalized.y) != 1f) ? (Quaternion.AngleAxis(HingeAngle, CenterOfMass) * Vector3.Cross(CenterOfMass, Vector3.up)) : (Quaternion.AngleAxis(HingeAngle, CenterOfMass) * Vector3.Cross(CenterOfMass, Vector3.right)));
		return GetRestRotationWorld() * vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion GetRestRotationWorld()
	{
		if (m_Initialised)
		{
			if (!(base.transform.parent != null))
			{
				return m_RestLocalRotation;
			}
			return base.transform.parent.rotation * m_RestLocalRotation;
		}
		return base.transform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetNoiseVector(Vector3 pos, float scale, float phase)
	{
		pos /= scale;
		return new Vector3(Mathf.PerlinNoise(pos.x, pos.y + phase) - 0.5f, Mathf.PerlinNoise(pos.y, pos.z + phase) - 0.5f, Mathf.PerlinNoise(pos.z, pos.x + phase) - 0.5f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion GetClosestRotationFromTo(Vector3 from, Vector3 to)
	{
		Quaternion target = Quaternion.FromToRotation(from, to) * base.transform.rotation;
		return base.transform.rotation.FromToRotation(target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawGizmosArc(Vector3 center, Vector3 startPoint, Vector3 normal, float degrees)
	{
		int num = (int)Mathf.Ceil(degrees / 20f);
		Quaternion quaternion = Quaternion.AngleAxis(degrees / (float)num, normal);
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = quaternion * (startPoint - center) + center;
			Gizmos.DrawRay(startPoint, vector - startPoint);
			startPoint = vector;
		}
	}
}
