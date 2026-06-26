using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayWhenHurt : EAIRunAway
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSafeDistance = 45;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lowHealthPercent = 1f;

	public EAIRunawayWhenHurt()
	{
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		if (!data.TryGetValue("runChance", out var _value))
		{
			return;
		}
		lowHealthPercent = 0f;
		if (StringParsers.ParseFloat(_value) >= base.RandomFloat)
		{
			GetData(data, "healthPer", ref lowHealthPercent);
			if (data.TryGetValue("healthPerMax", out _value))
			{
				float num = StringParsers.ParseFloat(_value);
				lowHealthPercent += base.RandomFloat * (num - lowHealthPercent);
			}
		}
	}

	public override bool CanExecute()
	{
		if (!theEntity.GetRevengeTarget())
		{
			return false;
		}
		if (lowHealthPercent < 1f)
		{
			if ((float)theEntity.Health / (float)theEntity.GetMaxHealth() >= lowHealthPercent)
			{
				return false;
			}
			theEntity.SetRevengeTimer((60 + GetRandom(60)) * 20);
		}
		return base.CanExecute();
	}

	public override bool Continue()
	{
		EntityAlive revengeTarget = theEntity.GetRevengeTarget();
		if (!revengeTarget || theEntity.GetDistanceSq(revengeTarget) >= 2025f)
		{
			return false;
		}
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
		EntityAlive revengeTarget = theEntity.GetRevengeTarget();
		if ((bool)revengeTarget)
		{
			return revengeTarget.position;
		}
		return theEntity.position;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, per {lowHealthPercent}";
	}
}
