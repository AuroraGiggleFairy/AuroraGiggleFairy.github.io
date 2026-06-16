using System;
using UnityEngine;

[RequireComponent(typeof(vp_Shooter))]
public class vp_SimpleAITurret : MonoBehaviour
{
	public float ViewRange = 10f;

	public float AimSpeed = 50f;

	public float WakeInterval = 2f;

	public float FireAngle = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Shooter m_Shooter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Target;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Shooter = GetComponent<vp_Shooter>();
		m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (!m_Timer.Active)
		{
			vp_Timer.In(WakeInterval, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				if (m_Target == null)
				{
					m_Target = ScanForLocalPlayer();
				}
				else
				{
					m_Target = null;
				}
			}, m_Timer);
		}
		if (m_Target != null)
		{
			AttackTarget();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Transform ScanForLocalPlayer()
	{
		Collider[] array = Physics.OverlapSphere(m_Transform.position, ViewRange, 1073741824);
		foreach (Collider collider in array)
		{
			Physics.Linecast(m_Transform.position, collider.transform.position + Vector3.up, out var hitInfo);
			if (!(hitInfo.collider != null) || !(hitInfo.collider != collider))
			{
				return collider.transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AttackTarget()
	{
		Quaternion to = Quaternion.LookRotation(m_Target.GetComponent<Collider>().bounds.center - m_Transform.position);
		m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, to, Time.deltaTime * AimSpeed);
		if (Mathf.Abs(vp_3DUtility.LookAtAngleHorizontal(m_Transform.position, m_Transform.forward, m_Target.position)) < FireAngle)
		{
			m_Shooter.TryFire();
		}
	}
}
