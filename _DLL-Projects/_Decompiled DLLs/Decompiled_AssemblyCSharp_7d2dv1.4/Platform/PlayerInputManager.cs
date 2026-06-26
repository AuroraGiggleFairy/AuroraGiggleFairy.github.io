using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;

namespace Platform;

public class PlayerInputManager
{
	public enum InputStyle
	{
		Undefined,
		Keyboard,
		PS4,
		XB1,
		Count
	}

	public enum ControllerIconStyle
	{
		Automatic,
		Xbox,
		Playstation
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InputStyle defaultInputStyle = InputStyle.Keyboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstInputDetected;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float[] inputStylesUsedMinutes = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastInputStyleSwitchTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastInputDeviceName;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputDevice lastInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputDevice newInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public BindingSourceType lastBindingSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputStyle _currentInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputStyle _currentControllerInputStyle = InputStyle.XB1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerActionsBase> actionSets = new List<PlayerActionsBase>();

	public readonly ActionSetManager ActionSetManager = new ActionSetManager();

	public InputStyle CurrentInputStyle
	{
		get
		{
			return _currentInputStyle;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == InputStyle.PS4 || value == InputStyle.XB1)
			{
				CurrentControllerInputStyle = value;
			}
			_currentInputStyle = value;
		}
	}

	public InputStyle CurrentControllerInputStyle
	{
		get
		{
			if (DeviceFlag.PS5.IsCurrent())
			{
				return InputStyle.PS4;
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				return InputStyle.XB1;
			}
			return _currentControllerInputStyle;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_currentControllerInputStyle = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ReadOnlyCollection<PlayerActionsBase> ActionSets { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsLocal PrimaryPlayer { get; }

	public event Action<InputStyle> OnLastInputStyleChanged;

	public PlayerInputManager()
	{
		Log.Out("Starting PlayerInputManager...");
		MouseBindingSource.ScaleX = (MouseBindingSource.ScaleY = (MouseBindingSource.ScaleZ = 0.2f));
		GameObject gameObject = GameObject.Find("Input");
		if (gameObject == null)
		{
			gameObject = new GameObject("Input");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		InControlManager component = gameObject.GetComponent<InControlManager>();
		if (component != null)
		{
			Log.Error("InControl already instantiated");
			return;
		}
		bool flag = GameUtils.GetLaunchArgument("noxinput") == null;
		bool enableNativeInput = GameUtils.GetLaunchArgument("disablenativeinput") == null;
		if (GameManager.IsDedicatedServer)
		{
			flag = false;
			enableNativeInput = false;
		}
		gameObject.SetActive(value: false);
		component = gameObject.AddComponent<InControlManager>();
		component.logDebugInfo = false;
		component.suspendInBackground = true;
		component.nativeInputPreventSleep = true;
		component.enableNativeInput = enableNativeInput;
		component.enableXInput = flag;
		component.nativeInputEnableXInput = flag;
		InputManager.AddCustomDeviceManagers += [PublicizedFrom(EAccessModifier.Internal)] (ref bool enableUnityInput) =>
		{
		};
		InControl.Logger.OnLogMessage += [PublicizedFrom(EAccessModifier.Internal)] (LogMessage _message) =>
		{
			switch (_message.Type)
			{
			case LogMessageType.Info:
				Log.Out(_message.Text);
				break;
			case LogMessageType.Warning:
				Log.Warning(_message.Text);
				break;
			case LogMessageType.Error:
				Log.Error(_message.Text);
				break;
			}
		};
		InControl.Logger.LogInfo("InControl (version " + InputManager.Version.ToString() + ", native module = " + component.enableNativeInput + ", XInput = " + component.enableXInput + ")");
		gameObject.SetActive(value: true);
		PlayerActionsGlobal.Init();
		if (!Submission.Enabled)
		{
			actionSets.Add(PlayerActionsGlobal.Instance);
		}
		PrimaryPlayer = new PlayerActionsLocal();
		actionSets.Add(PrimaryPlayer);
		actionSets.Add(PrimaryPlayer.VehicleActions);
		actionSets.Add(PrimaryPlayer.GUIActions);
		actionSets.Add(PrimaryPlayer.PermanentActions);
		ActionSets = new ReadOnlyCollection<PlayerActionsBase>(actionSets);
		for (int num = 0; num < actionSets.Count; num++)
		{
			PlayerActionSet actionSet = actionSets[num];
			actionSet.OnLastInputTypeChanged += [PublicizedFrom(EAccessModifier.Internal)] (BindingSourceType _type) =>
			{
				if (_type == BindingSourceType.DeviceBindingSource)
				{
					newInputDevice = actionSet.Device ?? InputManager.ActiveDevice;
				}
				else
				{
					newInputDevice = InputDevice.Null;
				}
			};
		}
		if (!GameManager.IsDedicatedServer)
		{
			ActionSetManager.Push(PrimaryPlayer);
		}
		CurrentInputStyle = defaultInputStyle;
	}

	public void Update()
	{
		newInputDevice = LastActiveInputDevice(out var bindingSourceType);
		if ((!firstInputDetected && bindingSourceType == BindingSourceType.None) || (lastInputDevice == newInputDevice && (bindingSourceType == lastBindingSource || (bindingSourceType == BindingSourceType.KeyBindingSource && lastBindingSource == BindingSourceType.MouseBindingSource) || (bindingSourceType == BindingSourceType.MouseBindingSource && lastBindingSource == BindingSourceType.KeyBindingSource))))
		{
			return;
		}
		if (!firstInputDetected)
		{
			firstInputDetected = true;
		}
		lastInputDevice = newInputDevice;
		lastBindingSource = bindingSourceType;
		InputStyle currentInputStyle = CurrentInputStyle;
		if (bindingSourceType == BindingSourceType.KeyBindingSource || bindingSourceType == BindingSourceType.MouseBindingSource)
		{
			lastInputDeviceName = null;
			CurrentInputStyle = InputStyle.Keyboard;
		}
		else if (lastInputDevice.Name == "None" || bindingSourceType == BindingSourceType.None)
		{
			lastInputDeviceName = null;
			CurrentInputStyle = defaultInputStyle;
		}
		else
		{
			string name = lastInputDevice.Name;
			if (name != lastInputDeviceName)
			{
				lastInputDeviceName = name;
				CurrentInputStyle = ((lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation2 || lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation3 || lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation4 || lastInputDevice.DeviceStyle == InputDeviceStyle.PlayStation5) ? InputStyle.PS4 : InputStyle.XB1);
			}
		}
		if (currentInputStyle != CurrentInputStyle && currentInputStyle != InputStyle.Undefined)
		{
			float unscaledTime = Time.unscaledTime;
			inputStylesUsedMinutes[(int)currentInputStyle] += (unscaledTime - lastInputStyleSwitchTime) / 60f;
			lastInputStyleSwitchTime = unscaledTime;
		}
		this.OnLastInputStyleChanged?.Invoke(CurrentInputStyle);
	}

	public void ForceInputStyleChange()
	{
		this.OnLastInputStyleChanged?.Invoke(CurrentInputStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InputDevice LastActiveInputDevice(out BindingSourceType lastBindingSource)
	{
		ulong num = 0uL;
		PlayerActionSet playerActionSet = null;
		lastBindingSource = BindingSourceType.None;
		for (int i = 0; i < ActionSets.Count; i++)
		{
			PlayerActionSet playerActionSet2 = ActionSets[i];
			if (playerActionSet2.Enabled && playerActionSet2.LastInputTypeChangedTick > num)
			{
				playerActionSet = playerActionSet2;
				num = playerActionSet2.LastInputTypeChangedTick;
			}
		}
		if (playerActionSet != null)
		{
			lastBindingSource = playerActionSet.LastInputType;
			if (playerActionSet.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				return playerActionSet.Device ?? InputManager.ActiveDevice;
			}
		}
		return InputDevice.Null;
	}

	public void ResetInputStyleUsage()
	{
		for (int i = 0; i < inputStylesUsedMinutes.Length; i++)
		{
			inputStylesUsedMinutes[i] = 0f;
			lastInputStyleSwitchTime = Time.unscaledTime;
		}
	}

	public InputStyle MostUsedInputStyle()
	{
		if (CurrentInputStyle != InputStyle.Undefined)
		{
			float unscaledTime = Time.unscaledTime;
			inputStylesUsedMinutes[(int)CurrentInputStyle] += (unscaledTime - lastInputStyleSwitchTime) / 60f;
			lastInputStyleSwitchTime = unscaledTime;
		}
		InputStyle result = InputStyle.Count;
		float num = -1f;
		for (int i = 0; i < inputStylesUsedMinutes.Length; i++)
		{
			if (inputStylesUsedMinutes[i] > num)
			{
				num = inputStylesUsedMinutes[i];
				result = (InputStyle)i;
			}
		}
		return result;
	}

	public PlayerActionsBase GetActionSetForName(string _name)
	{
		foreach (PlayerActionsBase actionSet in ActionSets)
		{
			if (actionSet.Name.EqualsCaseInsensitive(_name))
			{
				return actionSet;
			}
		}
		return null;
	}

	public void LoadActionSetsFromStrings(IList<string> actionSets)
	{
		if (ActionSets.Count != actionSets.Count)
		{
			Log.Warning($"Loading ActionSets from string array with incorrect length. Expected: {ActionSets.Count}. Actual: {actionSets?.Count}.");
			return;
		}
		for (int i = 0; i < ActionSets.Count; i++)
		{
			ActionSets[i].Load(actionSets[i]);
		}
	}

	public static InputStyle InputStyleFromSelectedIconStyle()
	{
		return (ControllerIconStyle)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerIconStyle) switch
		{
			ControllerIconStyle.Xbox => InputStyle.XB1, 
			ControllerIconStyle.Playstation => InputStyle.PS4, 
			_ => (PlatformManager.NativePlatform?.Input?.CurrentControllerInputStyle).GetValueOrDefault(), 
		};
	}
}
