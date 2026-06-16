using System;
using UnityEngine;

public class vp_SecurityCamTurret : vp_SimpleAITurret
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_AngleBob m_AngleBob;

	public GameObject Swivel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SwivelRotation = Vector3.zero;

	public float SwivelAmp = 100f;

	public float SwivelRate = 0.5f;

	public float SwivelOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle vp_ResumeSwivelTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		m_Transform = base.transform;
		m_AngleBob = base.gameObject.AddComponent<vp_AngleBob>();
		m_AngleBob.BobAmp.y = SwivelAmp;
		m_AngleBob.BobRate.y = SwivelRate;
		m_AngleBob.YOffset = SwivelOffset;
		m_AngleBob.FadeToTarget = true;
		SwivelRotation = Swivel.transform.eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (m_Target != null && m_AngleBob.enabled)
		{
			m_AngleBob.enabled = false;
			vp_ResumeSwivelTimer.Cancel();
		}
		if (m_Target == null && !m_AngleBob.enabled && !vp_ResumeSwivelTimer.Active)
		{
			vp_Timer.In(WakeInterval * 2f, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				m_AngleBob.enabled = true;
			}, vp_ResumeSwivelTimer);
		}
		SwivelRotation.y = m_Transform.eulerAngles.y;
		Swivel.transform.eulerAngles = SwivelRotation;
	}
}
