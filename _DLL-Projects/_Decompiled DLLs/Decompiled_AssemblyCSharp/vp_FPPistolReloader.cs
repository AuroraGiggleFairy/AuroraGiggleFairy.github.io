using System;
using UnityEngine;

public class vp_FPPistolReloader : vp_FPWeaponReloader
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Reload()
	{
		if (m_Weapon.gameObject != base.gameObject || m_Timer.Active)
		{
			return;
		}
		base.OnStart_Reload();
		vp_Timer.In(0.4f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (vp_Utility.IsActive(m_Weapon.gameObject) && m_Weapon.StateEnabled("Reload"))
			{
				m_Weapon.AddForce2(new Vector3(0f, 0.05f, 0f), new Vector3(0f, 0f, 0f));
				vp_Timer.In(0.15f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					if (vp_Utility.IsActive(m_Weapon.gameObject) && m_Weapon.StateEnabled("Reload"))
					{
						m_Weapon.SetState("Reload", enabled: false);
						m_Weapon.SetState("Reload2");
						m_Weapon.RotationOffset.z = 0f;
						m_Weapon.Refresh();
						vp_Timer.In(0.35f, [PublicizedFrom(EAccessModifier.Private)] () =>
						{
							if (vp_Utility.IsActive(m_Weapon.gameObject) && m_Weapon.StateEnabled("Reload2"))
							{
								m_Weapon.AddForce2(new Vector3(0f, 0f, -0.05f), new Vector3(5f, 0f, 0f));
								vp_Timer.In(0.1f, [PublicizedFrom(EAccessModifier.Private)] () =>
								{
									m_Weapon.SetState("Reload2", enabled: false);
								});
							}
						});
					}
				});
			}
		}, m_Timer);
	}
}
