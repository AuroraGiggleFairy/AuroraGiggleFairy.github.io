using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAILook : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lookAtTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int turnTicks;

	public EAILook()
	{
		MutexBits = 1;
	}

	public override bool CanExecute()
	{
		if (manager.lookTime > 0f)
		{
			return !theEntity.Jumping;
		}
		return false;
	}

	public override void Start()
	{
		waitTicks = (int)(manager.lookTime * 20f);
		manager.lookTime = 0f;
		theEntity.GetEntitySenses().Clear();
		lookAtTicks = 0;
		turnTicks = 0;
		theEntity.moveHelper.Stop();
	}

	public override bool Continue()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (theEntity.IsAlert)
		{
			waitTicks--;
			lookAtTicks -= 2;
			if (--turnTicks <= 0)
			{
				turnTicks = 14;
				theEntity.SeekYaw(theEntity.rotation.y + (base.RandomFloat * 120f - 60f), 0f, 35f);
			}
		}
		if (--waitTicks <= 0)
		{
			return false;
		}
		if (--lookAtTicks <= 0)
		{
			lookAtTicks = 40;
			Vector3 headPosition = theEntity.getHeadPosition();
			Vector3 vector = theEntity.GetForwardVector() * 20f;
			vector = Quaternion.Euler(base.RandomFloat * 60f - 30f, base.RandomFloat * 120f - 60f, 0f) * vector;
			theEntity.SetLookPosition(headPosition + vector);
		}
		return true;
	}

	public override void Reset()
	{
		theEntity.SetLookPosition(Vector3.zero);
	}

	public override string ToString()
	{
		return $"{base.ToString()}, wait {((float)waitTicks / 20f).ToCultureInvariantString()}";
	}
}
