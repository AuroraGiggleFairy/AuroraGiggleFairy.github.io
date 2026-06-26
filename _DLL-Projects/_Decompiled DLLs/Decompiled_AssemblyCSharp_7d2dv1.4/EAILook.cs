using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAILook : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewTicks;

	public EAILook()
	{
		MutexBits = 1;
	}

	public override bool CanExecute()
	{
		return manager.lookTime > 0f;
	}

	public override void Start()
	{
		waitTicks = (int)(manager.lookTime * 20f);
		manager.lookTime = 0f;
		theEntity.GetEntitySenses().Clear();
		viewTicks = 0;
		theEntity.Jumping = false;
		theEntity.moveHelper.Stop();
	}

	public override bool Continue()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		waitTicks--;
		if (waitTicks <= 0)
		{
			return false;
		}
		viewTicks--;
		if (viewTicks <= 0)
		{
			viewTicks = 40;
			Vector3 headPosition = theEntity.getHeadPosition();
			Vector3 forwardVector = theEntity.GetForwardVector();
			forwardVector = Quaternion.Euler(base.RandomFloat * 60f - 30f, base.RandomFloat * 120f - 60f, 0f) * forwardVector;
			theEntity.SetLookPosition(headPosition + forwardVector);
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
