using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayWhenHurt : EAIRunAway
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float lowHealthPercent = 1f;

	public EAIRunawayWhenHurt()
	{
		MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		if (!data.TryGetValue("runChance", out var value))
		{
			return;
		}
		lowHealthPercent = 0f;
		if (StringParsers.ParseFloat(value) >= base.RandomFloat)
		{
			GetData(data, "healthPer", ref lowHealthPercent);
			if (data.TryGetValue("healthPerMax", out value))
			{
				float num = StringParsers.ParseFloat(value);
				lowHealthPercent += base.RandomFloat * (num - lowHealthPercent);
			}
		}
	}

	public override bool CanExecute()
	{
		enemy = theEntity.GetRevengeTarget();
		if (!enemy)
		{
			return false;
		}
		if (lowHealthPercent < 1f && (float)theEntity.Health / (float)theEntity.GetMaxHealth() >= lowHealthPercent)
		{
			return false;
		}
		return base.CanExecute();
	}

	public override bool Continue()
	{
		return base.Continue();
	}

	public override void Update()
	{
		base.Update();
		theEntity.navigator.setMoveSpeed(theEntity.IsInWater() ? theEntity.GetMoveSpeed() : theEntity.GetMoveSpeedPanic());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetFleeFromPos()
	{
		if ((bool)enemy)
		{
			return enemy.position;
		}
		return theEntity.position;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, per {lowHealthPercent}";
	}
}
