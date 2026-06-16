using System;
using UnityEngine;

public class vp_Billboard : MonoBehaviour
{
	public Transform m_CameraTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Transform = base.transform;
		if (m_CameraTransform == null)
		{
			m_CameraTransform = Camera.main.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (m_CameraTransform != null)
		{
			m_Transform.localEulerAngles = m_CameraTransform.eulerAngles;
		}
		m_Transform.localEulerAngles = (Vector2)m_Transform.localEulerAngles;
	}
}
