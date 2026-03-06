using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TriggerEffects;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

[UnityEngine.Scripting.Preserve]
public class TriggerEffectManager : IDisposable
{
	public enum GamepadTrigger
	{
		LeftTrigger,
		RightTrigger
	}

	public enum EffectDualsense
	{
		Off,
		WeaponSingle,
		WeaponMultipoint,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope,
		FeedbackMultipoint,
		VibrationMultipoint
	}

	public struct TriggerEffectDS
	{
		public EffectDualsense Effect;

		public byte Position;

		public byte EndPosition;

		public byte Frequency;

		public byte AmplitudeEndStrength;

		public byte Strength;

		public byte[] Strengths;
	}

	public enum EffectXbox
	{
		Off,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope
	}

	public struct TriggerEffectXB
	{
		public EffectXbox Effect;

		public float Strength;

		[FormerlySerializedAs("endStrength")]
		[FormerlySerializedAs("Amplitude")]
		public float EndStrength;

		public float StartPosition;

		public float EndPosition;
	}

	public struct ControllerTriggerEffect
	{
		public TriggerEffectDS DualsenseEffect;

		public TriggerEffectXB XboxTriggerEffect;
	}

	public static readonly Dictionary<string, TriggerEffectDS> ControllerTriggerEffectsDS = new Dictionary<string, TriggerEffectDS>();

	public static readonly Dictionary<string, TriggerEffectXB> ControllerTriggerEffectsXb = new Dictionary<string, TriggerEffectXB>();

	[PublicizedFrom(EAccessModifier.Internal)]
	public static readonly TriggerEffectDS NoneEffectDs = new TriggerEffectDS
	{
		Effect = EffectDualsense.Off,
		AmplitudeEndStrength = 0,
		Frequency = 0,
		Position = 0,
		EndPosition = 0,
		Strength = 0,
		Strengths = Array.Empty<byte>()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TriggerEffectXB NoneEffectXb = new TriggerEffectXB
	{
		Effect = EffectXbox.Off,
		Strength = 0f
	};

	public static readonly ControllerTriggerEffect NoneEffect = new ControllerTriggerEffect
	{
		DualsenseEffect = NoneEffectDs,
		XboxTriggerEffect = NoneEffectXb
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] _controllersConnected = new bool[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float _currentControllerVibrationStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _stateChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerTriggerEffect _currentEffectLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerTriggerEffect _currentEffectRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthSubtle = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStandard = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStrong = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDualSenseRumbleStrengthMultiplier = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float audioRumbleStrength = 1f;

	public AudioGamepadRumbleSource[] vibrationAudioSources = new AudioGamepadRumbleSource[5];

	[PublicizedFrom(EAccessModifier.Protected)]
	public static LightbarGradients lightbarGradients;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLeftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetRightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _inUI;

	public bool Enabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return _enabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_enabled = value;
			_stateChanged = true;
		}
	}

	public bool inUI
	{
		get
		{
			return _inUI;
		}
		set
		{
			if (_inUI != value)
			{
				_inUI = value;
				_stateChanged = true;
			}
		}
	}

	public TriggerEffectManager()
	{
		PollSetting();
		_stateChanged = false;
		try
		{
			TriggerEffectDualsensePC.InitTriggerEffectManager(ref _controllersConnected);
		}
		catch (DllNotFoundException arg)
		{
			Log.Warning($"[TriggerEffectManager] Failed to load ControllerExt, disabling. Details: {arg}");
			GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, _value: false);
			return;
		}
		TriggerEffectDualsense.InitTriggerEffectManager();
		for (int i = 0; i < vibrationAudioSources.Length; i++)
		{
			vibrationAudioSources[i] = new AudioGamepadRumbleSource();
		}
		UpdateControllerVibrationStrength();
		EnableVibration();
		InitializeLightbarGradient();
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
	}

	public void EnableVibration()
	{
		TriggerEffectDualsensePC.EnableVibration();
		TriggerEffectDualsense.EnableVibration();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeLightbarGradient()
	{
		lightbarGradients = Resources.Load<LightbarGradients>("Data/LightBarGradients");
	}

	public static void SetEnabled(bool _enabled)
	{
		GameManager.Instance.triggerEffectManager.Enabled = _enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte FloatToByte(float value)
	{
		return (byte)(Mathf.Clamp01(value) * 255f);
	}

	public void Update()
	{
		if (GameManager.Instance.World != null && audioRumbleStrength > 0f && !GameManager.Instance.IsPaused() && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			int num = 0;
			int num2 = 0;
			targetLeftAudioStrength = 0f;
			targetRightAudioStrength = 0f;
			Transform transform = ((GameManager.Instance.World == null) ? LocalPlayerUI.primaryUI.uiCamera.transform : GameManager.Instance.World.GetPrimaryPlayer()?.cameraTransform);
			AudioGamepadRumbleSource[] array = vibrationAudioSources;
			foreach (AudioGamepadRumbleSource audioGamepadRumbleSource in array)
			{
				if (!(audioGamepadRumbleSource.audioSrc != null))
				{
					continue;
				}
				if (audioGamepadRumbleSource.audioSrc.isPlaying)
				{
					float num3 = audioGamepadRumbleSource.GetSample(0);
					if (audioGamepadRumbleSource.locationBased)
					{
						float num4 = 1f - Vector3.Distance(transform.position, audioGamepadRumbleSource.audioSrc.transform.position) / audioGamepadRumbleSource.audioSrc.maxDistance;
						if (num4 < 0.9f)
						{
							continue;
						}
						num3 *= num4;
					}
					num3 *= audioGamepadRumbleSource.strengthMultiplier * audioGamepadRumbleSource.audioSrc.pitch * audioRumbleStrength;
					if (num3 > 0f)
					{
						num2++;
						targetRightAudioStrength += num3;
					}
					else if (num3 < 0f)
					{
						num++;
						targetLeftAudioStrength += num3;
					}
				}
				else
				{
					audioGamepadRumbleSource.Clear();
				}
			}
			if (num > 0 || num2 > 0)
			{
				if (num > 0)
				{
					targetLeftAudioStrength /= num;
				}
				if (num2 > 0)
				{
					targetRightAudioStrength /= num2;
				}
				leftAudioStrength = Mathf.Lerp(leftAudioStrength, targetLeftAudioStrength, Time.deltaTime * 15f);
				rightAudioStrength = Mathf.Lerp(rightAudioStrength, targetRightAudioStrength, Time.deltaTime * 15f);
				InputManager.ActiveDevice.Vibrate(0f - leftAudioStrength, rightAudioStrength);
				SetGamepadVibration(0f - leftAudioStrength + rightAudioStrength);
				SetDualSenseVibration(FloatToByte((0f - leftAudioStrength) * 0.25f), FloatToByte(rightAudioStrength * 0.25f));
			}
			else
			{
				SetDualSenseVibration(0, 0);
				InputManager.ActiveDevice.StopVibration();
				SetGamepadVibration(0f);
			}
		}
		TriggerEffectDualsensePC.PCConnectedUpdate(ref _controllersConnected, _currentEffectLeft.DualsenseEffect, _currentEffectRight.DualsenseEffect);
		TriggerEffectDualsense.ConnectedUpdate(_currentEffectLeft.DualsenseEffect, _currentEffectRight.DualsenseEffect);
		if (Enabled && !inUI)
		{
			TriggerEffectDualsensePC.PCTriggerUpdate(_stateChanged, _currentEffectLeft.DualsenseEffect, _currentEffectRight.DualsenseEffect);
			TriggerEffectDualsense.Update(_currentEffectLeft.DualsenseEffect, _currentEffectRight.DualsenseEffect);
		}
		else if (_stateChanged && (!Enabled || inUI))
		{
			SetGamepadTriggerEffectOff(GamepadTrigger.RightTrigger);
			SetGamepadTriggerEffectOff(GamepadTrigger.LeftTrigger);
			TriggerEffectDualsensePC.DualsensePCSetEffectToOff();
			TriggerEffectDualsense.SetEffectToOff();
			_stateChanged = true;
		}
		_triggerSetLeft = false;
		_triggerSetRight = false;
		_stateChanged = false;
	}

	public void SetGamepadTriggerEffectOff(GamepadTrigger trigger)
	{
		switch (trigger)
		{
		case GamepadTrigger.LeftTrigger:
			_currentEffectLeft = NoneEffect;
			break;
		case GamepadTrigger.RightTrigger:
			_currentEffectRight = NoneEffect;
			break;
		default:
			throw new ArgumentOutOfRangeException("trigger", trigger, null);
		}
		_stateChanged = true;
	}

	public void SetWeaponEffect(int userID, GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
	{
		if (Enabled)
		{
			TriggerEffectDualsensePC.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
			TriggerEffectDualsense.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
			_stateChanged = true;
		}
	}

	public void ResetControllerIdentification()
	{
		TriggerEffectDualsensePC.ResetControllerIdentification();
		TriggerEffectDualsense.ResetControllerIdentification();
		_stateChanged = true;
	}

	public void SetControllerIdentification()
	{
		TriggerEffectDualsensePC.SetControllerIdentification();
		TriggerEffectDualsense.SetControllerIdentification();
		_stateChanged = true;
	}

	public void SetTriggerEffectVibration(int userID, GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
	{
		if (Enabled)
		{
			TriggerEffectDualsensePC.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
			TriggerEffectDualsense.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
			_stateChanged = true;
		}
	}

	public void SetTriggerEffectVibrationMultiplePosition(int userID, GamepadTrigger trigger, byte[] amplitudes, byte frequency)
	{
		if (Enabled)
		{
			TriggerEffectDualsensePC.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
			TriggerEffectDualsense.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
			_stateChanged = true;
		}
	}

	public void Shutdown()
	{
		StopGamepadVibration();
		LibShutdown();
		TriggerEffectDualsense.ResetControllerIdentification();
		TriggerEffectDualsensePC.ResetControllerIdentification();
		for (int i = 1; i < 5; i++)
		{
			_controllersConnected[i - 1] = false;
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= OnLastInputStyleChanged;
		Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LibShutdown()
	{
		TriggerEffectDualsensePC.LibShutdown();
	}

	public void Dispose()
	{
		Shutdown();
	}

	public void PollSetting()
	{
		Enabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects);
	}

	public void SetGamepadVibration(float strength)
	{
		_currentControllerVibrationStrength = strength;
		_stateChanged = true;
	}

	public void StopGamepadVibration()
	{
		_currentControllerVibrationStrength = 0f;
		_stateChanged = true;
		SetDualSenseVibration(0, 0);
		InputManager.ActiveDevice.StopVibration();
	}

	public void SetTriggerEffect(ControllerTriggerEffect effect)
	{
		if (Enabled)
		{
			SetTriggerEffect(GamepadTrigger.LeftTrigger, effect);
			SetTriggerEffect(GamepadTrigger.RightTrigger, effect);
		}
	}

	public void SetTriggerEffect(GamepadTrigger trigger, ControllerTriggerEffect effect, bool asap = false)
	{
		if (!Enabled)
		{
			return;
		}
		_stateChanged = true;
		switch (trigger)
		{
		case GamepadTrigger.LeftTrigger:
			_currentEffectLeft.DualsenseEffect = effect.DualsenseEffect;
			if (asap)
			{
				TriggerEffectDualsensePC.ApplyImmediate(trigger, _currentEffectLeft);
			}
			_triggerSetLeft = true;
			break;
		case GamepadTrigger.RightTrigger:
			_currentEffectRight.DualsenseEffect = effect.DualsenseEffect;
			if (asap)
			{
				TriggerEffectDualsensePC.ApplyImmediate(trigger, _currentEffectRight);
			}
			_triggerSetRight = true;
			break;
		}
	}

	public static ControllerTriggerEffect GetTriggerEffect((string, string) triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return NoneEffect;
		}
		ControllerTriggerEffect result = new ControllerTriggerEffect
		{
			DualsenseEffect = NoneEffectDs,
			XboxTriggerEffect = NoneEffectXb
		};
		if (ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out var value))
		{
			result.DualsenseEffect = value;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		if (ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out var value2))
		{
			result.XboxTriggerEffect = value2;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return result;
	}

	public static ControllerTriggerEffect GetTriggerEffect(string dualsenseTrigger, string impulseTrigger)
	{
		if (string.IsNullOrEmpty(dualsenseTrigger) || string.IsNullOrEmpty(impulseTrigger) || (dualsenseTrigger.Contains("NoEffect") && impulseTrigger.Contains("NoEffect")) || (dualsenseTrigger.Contains("NoneEffect") && impulseTrigger.Contains("NoneEffect")))
		{
			return NoneEffect;
		}
		ControllerTriggerEffect result = new ControllerTriggerEffect
		{
			DualsenseEffect = NoneEffectDs,
			XboxTriggerEffect = NoneEffectXb
		};
		if (ControllerTriggerEffectsDS.TryGetValue(dualsenseTrigger, out var value))
		{
			result.DualsenseEffect = value;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + dualsenseTrigger);
		}
		if (ControllerTriggerEffectsXb.TryGetValue(impulseTrigger, out var value2))
		{
			result.XboxTriggerEffect = value2;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + impulseTrigger);
		}
		return result;
	}

	public static (TriggerEffectDS, TriggerEffectXB) GetTriggerEffectAsTuple((string, string) triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return (NoneEffect.DualsenseEffect, NoneEffect.XboxTriggerEffect);
		}
		(TriggerEffectDS, TriggerEffectXB) result = (NoneEffect.DualsenseEffect, NoneEffect.XboxTriggerEffect);
		if (ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out var value))
		{
			result.Item1 = value;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		if (ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out var value2))
		{
			result.Item2 = value2;
		}
		else
		{
			Debug.LogWarning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return result;
	}

	public static bool SettingDefaultValue()
	{
		if (Application.platform != RuntimePlatform.PS5 && Application.platform != RuntimePlatform.GameCoreXboxSeries)
		{
			return Application.platform == RuntimePlatform.WindowsEditor;
		}
		return true;
	}

	public void SetAudioRumbleSource(AudioSource _audioSource, float _strengthMultiplier, bool _locationBased)
	{
		AudioGamepadRumbleSource audioGamepadRumbleSource = null;
		float num = float.MaxValue;
		AudioGamepadRumbleSource[] array = vibrationAudioSources;
		foreach (AudioGamepadRumbleSource audioGamepadRumbleSource2 in array)
		{
			if (audioGamepadRumbleSource2.audioSrc != null)
			{
				if (audioGamepadRumbleSource2.timeAdded < num)
				{
					audioGamepadRumbleSource = audioGamepadRumbleSource2;
					num = audioGamepadRumbleSource2.timeAdded;
				}
				continue;
			}
			audioGamepadRumbleSource2.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
			return;
		}
		audioGamepadRumbleSource?.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
	{
		TriggerEffectDualsense.SetDualSenseVibration(_smallMotor, _largeMotor);
		TriggerEffectDualsensePC.SetDualSenseVibration(_smallMotor, _largeMotor);
	}

	public static void UpdateControllerVibrationStrength()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength))
		{
		case 0:
			audioRumbleStrength = 0f;
			break;
		case 1:
			audioRumbleStrength = 0.5f;
			break;
		case 2:
			audioRumbleStrength = 1f;
			break;
		case 3:
			audioRumbleStrength = 2f;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style == PlayerInputManager.InputStyle.Keyboard)
		{
			StopGamepadVibration();
		}
	}

	public static void UpdateDualSenseLightFromWeather(WeatherManager.BiomeWeather weather)
	{
		if (!(lightbarGradients == null) && weather != null)
		{
			float num = GameManager.Instance.World.GetWorldTime() % 24000;
			float num2 = SkyManager.GetDawnTime() * 1000f;
			float num3 = SkyManager.GetDuskTime() * 1000f;
			bool flag = num < num2 || num > num3;
			float time;
			if (flag)
			{
				float num4 = 24000f - num3;
				float num5 = num4 + num2;
				time = ((!(num >= num3) || !(num < 24000f)) ? (num4 / num5 + num / (num2 + num4)) : ((num - num3) / num5));
			}
			else
			{
				float num6 = num3 - num2;
				time = (num - num2) / num6;
			}
			Color dualSenseLightbarColor;
			if (SkyManager.BloodMoonVisiblePercent() != 1f)
			{
				dualSenseLightbarColor = ((weather.rainParam.value >= 0.5f || (weather.biomeDefinition != null && (weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.Wasteland || weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.burnt_forest))) ? ((!flag) ? lightbarGradients.cloudDayGradient.Evaluate(time) : lightbarGradients.cloudNightGradient.Evaluate(time)) : ((!flag) ? lightbarGradients.dayGradient.Evaluate(time) : lightbarGradients.nightGradient.Evaluate(time)));
			}
			else
			{
				float time2 = (1f + Mathf.Sin(Time.time)) / 2f;
				dualSenseLightbarColor = lightbarGradients.bloodmoonGradient.Evaluate(time2);
			}
			SetDualSenseLightbarColor(dualSenseLightbarColor);
		}
	}

	public static void SetDualSenseLightbarColor(Color color)
	{
		for (int i = 1; i < 5; i++)
		{
			TriggerEffectDualsense.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
			TriggerEffectDualsensePC.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
		}
	}

	public static void SetMainMenuLightbarColor()
	{
		SetDualSenseLightbarColor(lightbarGradients.mainMenuColor);
	}
}
