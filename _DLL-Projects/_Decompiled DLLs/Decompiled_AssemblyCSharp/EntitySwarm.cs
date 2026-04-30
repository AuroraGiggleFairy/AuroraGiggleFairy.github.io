using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySwarm : EntityVulture
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dissipateDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
		targetAttackHealthPercent = 1f;
		ignoreTargetAttached = true;
		wanderHeightRange.x = 1f;
		wanderHeightRange.y = 8f;
		dissipateDelay = 24f;
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (!IsDead())
		{
			dissipateDelay -= 0.05f;
			if (dissipateDelay <= 2f && state != State.Home)
			{
				StartHome(position + new Vector3(0f, 50f, 0f));
			}
			if (dissipateDelay <= 0f)
			{
				Kill(DamageResponse.New(_fatal: true));
			}
		}
	}
}
