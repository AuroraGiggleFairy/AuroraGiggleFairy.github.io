using System;
using UnityEngine;

namespace TriggerEffects;

public static class TriggerEffectDualsensePC
{
	public enum APIState
	{
		UnInit,
		OK,
		Error
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static APIState _apiState
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetTriggerEffectVibration(int userID, TriggerEffectManager.GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtSetTriggerEffectVibration(userID, (int)trigger, position, amplitude, frequency);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ResetControllerIdentification()
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtResetLightbar(1);
			ControllerExt.ControllerExtResetLightbar(2);
			ControllerExt.ControllerExtResetLightbar(3);
			ControllerExt.ControllerExtResetLightbar(4);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetControllerIdentification()
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtSetLightbar(1, 180, 180, 0);
			ControllerExt.ControllerExtSetLightbar(2, 80, 80, 0);
			ControllerExt.ControllerExtSetLightbar(3, 0, 180, 80);
			ControllerExt.ControllerExtSetLightbar(4, 0, 80, 80);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetLightbar(int userId, byte colorR, byte colorG, byte colorB)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtSetLightbar(userId, colorR, colorG, colorB);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetWeaponEffect(int userID, TriggerEffectManager.GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtSetTriggerEffectWeapon(userID, (int)trigger, startPosition, endPosition, strength);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetTriggerEffectVibrationMultiplePosition(int userID, TriggerEffectManager.GamepadTrigger trigger, byte[] amplitudes, byte frequency)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			ControllerExt.ControllerExtSetTriggerEffectMultiVibration(userID, (int)trigger, amplitudes, amplitudes.Length, frequency);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void LibShutdown()
	{
		try
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || (Application.platform == RuntimePlatform.WindowsPlayer && _apiState == APIState.OK))
			{
				for (int i = 1; i < 5; i++)
				{
					ControllerExt.ControllerExtResetLightbar(i);
					ControllerExt.ControllerExtSetTriggerEffectOff(i, 0);
					ControllerExt.ControllerExtSetTriggerEffectOff(i, 1);
					ControllerExt.ControllerExtClosePad(i);
				}
				ControllerExt.ControllerExtShutdown();
				_apiState = APIState.UnInit;
			}
		}
		catch (Exception arg)
		{
			Debug.Log($"[ControllerExt] Shutdown failed {arg}");
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void InitTriggerEffectManager(ref bool[] controllersConnected)
	{
		if (Application.platform != RuntimePlatform.WindowsEditor && (Application.platform != RuntimePlatform.WindowsPlayer || _apiState != APIState.UnInit))
		{
			return;
		}
		Application.quitting += LibShutdown;
		try
		{
			if (!ControllerExt.ControllerExtInit() && Application.platform == RuntimePlatform.WindowsEditor)
			{
				if (ControllerExt.GetErrorString(out var error))
				{
					Debug.LogWarning("TriggerEffectManager: ControllerExtInit failed: " + error);
				}
				ControllerExt.ControllerExtUpdate();
				for (int i = 1; i < 5; i++)
				{
					ControllerExt.ControllerExtClosePad(i);
					controllersConnected[i - 1] = false;
				}
			}
			ControllerExt.ControllerExtUpdate();
			for (int j = 1; j < 5; j++)
			{
				if (ControllerExt.ControllerExtOpenPad(j))
				{
					controllersConnected[j - 1] = true;
				}
			}
			ControllerExt.ControllerExtUpdate();
			_apiState = APIState.OK;
		}
		catch (Exception arg)
		{
			Log.Error($"[ControllerExt] Failed to load Library, disabiling: {arg}");
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void PCTriggerUpdate(bool stateChanged, TriggerEffectManager.TriggerEffectDS currentEffectLeft, TriggerEffectManager.TriggerEffectDS currentEffectRight)
	{
		if ((Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer) || _apiState != APIState.OK)
		{
			return;
		}
		for (int i = 1; i < 5; i++)
		{
			if (GameManager.Instance.IsPaused())
			{
				ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.NoneEffectDs);
				ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffectDs);
				continue;
			}
			if (!ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, currentEffectLeft) && stateChanged && ControllerExt.GetErrorString(out var error))
			{
				Debug.LogWarning($"Controller {i} ControllerExtTriggerEffectApply returned false ({error})");
			}
			if (!ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, currentEffectRight) && stateChanged && ControllerExt.GetErrorString(out var error2))
			{
				Debug.LogWarning($"Controller {i} ControllerExtTriggerEffectApply returned false ({error2})");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void PCConnectedUpdate(ref bool[] controllersConnected, TriggerEffectManager.TriggerEffectDS currentEffectLeft, TriggerEffectManager.TriggerEffectDS currentEffectRight)
	{
		if ((Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer) || _apiState != APIState.OK)
		{
			return;
		}
		ControllerExt.ControllerExtUpdate();
		for (int i = 1; i < 5; i++)
		{
			if (controllersConnected[i - 1] && !ControllerExt.ControllerExtIsConnected(i))
			{
				ControllerExt.ControllerExtClosePad(i);
				controllersConnected[i - 1] = false;
			}
			else if (!controllersConnected[i - 1] && ControllerExt.ControllerExtOpenPad(i))
			{
				controllersConnected[i - 1] = true;
				if (GameManager.Instance.IsPaused())
				{
					ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.NoneEffectDs);
					ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffectDs);
				}
				else
				{
					ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, currentEffectLeft);
					ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, currentEffectRight);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ApplyEffectDualsenseOnPC(int userId, TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.TriggerEffectDS effect)
	{
		bool result = true;
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			result = false;
			switch (effect.Effect)
			{
			case TriggerEffectManager.EffectDualsense.Off:
				result = ControllerExt.ControllerExtSetTriggerEffectOff(userId, (int)trigger);
				break;
			case TriggerEffectManager.EffectDualsense.WeaponSingle:
				result = ControllerExt.ControllerExtSetTriggerEffectWeapon(userId, (int)trigger, effect.Position, effect.EndPosition, effect.Strength);
				break;
			case TriggerEffectManager.EffectDualsense.FeedbackSingle:
				result = ControllerExt.ControllerExtSetTriggerEffectFeedback(userId, (int)trigger, effect.Position, effect.Strength);
				break;
			case TriggerEffectManager.EffectDualsense.VibrationSingle:
				result = ControllerExt.ControllerExtSetTriggerEffectVibration(userId, (int)trigger, effect.Position, effect.AmplitudeEndStrength, effect.Frequency);
				break;
			case TriggerEffectManager.EffectDualsense.FeedbackSlope:
				result = ControllerExt.ControllerExtSetTriggerEffectSlopeFeedback(userId, (int)trigger, effect.Position, effect.EndPosition, effect.Strength, effect.AmplitudeEndStrength);
				break;
			case TriggerEffectManager.EffectDualsense.FeedbackMultipoint:
				result = ControllerExt.ControllerExtSetTriggerEffectMultiFeedback(userId, (int)trigger, effect.Strengths, effect.Strengths.Length);
				break;
			case TriggerEffectManager.EffectDualsense.VibrationMultipoint:
				result = ControllerExt.ControllerExtSetTriggerEffectMultiVibration(userId, (int)trigger, effect.Strengths, effect.Strengths.Length, effect.Frequency);
				break;
			case TriggerEffectManager.EffectDualsense.WeaponMultipoint:
			case TriggerEffectManager.EffectDualsense.VibrationSlope:
				throw new NotSupportedException();
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ApplyImmediate(TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.ControllerTriggerEffect currentEffect)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			for (int i = 1; i < 5; i++)
			{
				ApplyEffectDualsenseOnPC(i, trigger, currentEffect.DualsenseEffect);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void DualsensePCSetEffectToOff()
	{
		if ((Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer) || _apiState != APIState.OK)
		{
			return;
		}
		for (int i = 1; i < 5; i++)
		{
			if (!ControllerExt.ControllerExtSetTriggerEffectOff(i, 0) && ControllerExt.GetErrorString(out var error))
			{
				Debug.LogWarning($"Controller {i} ControllerExtTriggerEffectApply returned false {error}");
			}
			if (!ControllerExt.ControllerExtSetTriggerEffectOff(i, 1) && ControllerExt.GetErrorString(out error))
			{
				Debug.LogWarning($"Controller {i} ControllerExtTriggerEffectApply returned false {error}");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void EnableVibration()
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			for (int i = 1; i < 5; i++)
			{
				ControllerExt.ControllerExtEnableCompatibleVibration(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
	{
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && _apiState == APIState.OK)
		{
			for (int i = 1; i < 5; i++)
			{
				ControllerExt.ControllerExtSetMotorValue(i, _smallMotor, _largeMotor);
			}
		}
	}
}
