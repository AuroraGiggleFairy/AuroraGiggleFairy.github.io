using System;
using UnityEngine;

public class vp_Remover : MonoBehaviour
{
	public float LifeTime = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_DestroyTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		vp_Timer.In(Mathf.Max(LifeTime, 0.1f), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			vp_Utility.Destroy(base.gameObject);
		}, m_DestroyTimer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		m_DestroyTimer.Cancel();
	}
}
