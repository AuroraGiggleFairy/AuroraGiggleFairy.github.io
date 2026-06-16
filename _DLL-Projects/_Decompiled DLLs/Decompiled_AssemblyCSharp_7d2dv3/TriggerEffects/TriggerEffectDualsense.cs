namespace TriggerEffects;

[PublicizedFrom(EAccessModifier.Internal)]
public static class TriggerEffectDualsense
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void InitTriggerEffectManager()
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ConnectedUpdate(TriggerEffectManager.TriggerEffectDS currentEffectLeft, TriggerEffectManager.TriggerEffectDS currentEffectRight)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void Update(TriggerEffectManager.TriggerEffectDS left, TriggerEffectManager.TriggerEffectDS right)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetEffectToOff()
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ResetControllerIdentification()
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetControllerIdentification()
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ApplyImmediate(TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.ControllerTriggerEffect currentEffect)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ApplyEffectPS5Input(int slot, TriggerEffectManager.GamepadTrigger triggerGeneric, TriggerEffectManager.TriggerEffectDS effect)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetTriggerEffectVibration(int userID, TriggerEffectManager.GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetTriggerEffectVibrationMultiplePosition(int userID, TriggerEffectManager.GamepadTrigger trigger, byte[] amplitudes, byte frequency)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetWeaponEffect(int userID, TriggerEffectManager.GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void EnableVibration()
	{
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
	{
	}

	public static void SetLightbar(int userId, byte colorR, byte colorG, byte colorB)
	{
	}
}
