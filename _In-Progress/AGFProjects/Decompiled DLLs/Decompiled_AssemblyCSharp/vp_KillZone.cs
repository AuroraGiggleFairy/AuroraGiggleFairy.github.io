using System;
using UnityEngine;

public class vp_KillZone : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_DamageHandler m_TargetDamageHandler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Respawner m_TargetRespawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer == 29 || col.gameObject.layer == 26)
		{
			return;
		}
		m_TargetDamageHandler = vp_DamageHandler.GetDamageHandlerOfCollider(col);
		if (!(m_TargetDamageHandler == null) && !(m_TargetDamageHandler.CurrentHealth <= 0f))
		{
			m_TargetRespawner = vp_Respawner.GetRespawnerOfCollider(col);
			if (!(m_TargetRespawner != null) || !(Time.time < m_TargetRespawner.LastRespawnTime + 1f))
			{
				m_TargetDamageHandler.Damage(new vp_DamageInfo(m_TargetDamageHandler.CurrentHealth, m_TargetDamageHandler.Transform));
			}
		}
	}
}
