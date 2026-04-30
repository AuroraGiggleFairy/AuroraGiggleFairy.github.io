using System;
using UnityEngine;

public class vp_Spin : MonoBehaviour
{
	public Vector3 RotationSpeed = new Vector3(0f, 90f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		m_Transform.Rotate(RotationSpeed * Time.deltaTime);
	}
}
