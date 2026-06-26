using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class ActionSetManager
{
	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerActionSet> PlayerActions = new List<PlayerActionSet>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EDebugLevel debug;

	public bool Empty => PlayerActions.Count <= 0;

	public PlayerActionSet Top => PlayerActions[PlayerActions.Count - 1];

	public static EDebugLevel DebugLevel => debug;

	[PublicizedFrom(EAccessModifier.Private)]
	static ActionSetManager()
	{
		debug = EDebugLevel.Off;
		string launchArgument = GameUtils.GetLaunchArgument("debuginput");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				debug = EDebugLevel.Verbose;
			}
			else
			{
				debug = EDebugLevel.Normal;
			}
		}
	}

	public void Insert(PlayerActionSet _playerAction, int _index, string _windowName = null)
	{
		if (debug != EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Insert ({2} - {0}):{1}", _playerAction.GetType().FullName, (debug == EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "", _windowName);
		}
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Insert - Inserting a null input onto stack.");
		}
		if (!Empty)
		{
			Top.Enabled = false;
		}
		_playerAction.Enabled = false;
		PlayerActions.Insert(_index, _playerAction);
		Top.Enabled = true;
		if (debug != EDebugLevel.Off)
		{
			LogActionSets();
		}
	}

	public void Remove(PlayerActionSet _playerAction, int _minIndex, string _windowName = null)
	{
		if (debug != EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Remove ({2} - {0}):{1}", _playerAction.GetType().FullName, (debug == EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "", _windowName);
		}
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Remove - Trying to remove a null input from the stack.");
		}
		if (Empty)
		{
			Log.Warning("LocalPlayerInput::Remove - Removing input from an empty stack.");
			return;
		}
		Top.Enabled = false;
		_playerAction.Enabled = false;
		int num = -1;
		for (int num2 = PlayerActions.Count - 1; num2 >= _minIndex; num2--)
		{
			if (PlayerActions[num2] == _playerAction)
			{
				num = num2;
				break;
			}
		}
		if (num >= 0)
		{
			PlayerActions.RemoveAt(num);
		}
		else
		{
			Log.Warning($"LocalPlayerInput::Remove - Failed to find action set of type '{_playerAction.GetType().FullName}' with a min index of {_minIndex} to remove.");
		}
		if (!Empty)
		{
			Top.Enabled = true;
		}
		if (debug != EDebugLevel.Off)
		{
			LogActionSets();
		}
	}

	public void Push(PlayerActionSet _playerAction)
	{
		if (_playerAction == null)
		{
			Log.Warning("LocalPlayerInput::Push - Pushing a null input onto stack.");
		}
		PushInternal(_playerAction);
	}

	public void Push(GUIWindow _window)
	{
		if (_window?.GetActionSet() == null)
		{
			Log.Warning("LocalPlayerInput::Push - Pushing a null input onto stack.");
		}
		PushInternal(_window?.GetActionSet(), _window?.Id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PushInternal(PlayerActionSet _playerAction, string _windowName = null)
	{
		if (debug != EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Push ({2} - {0}):{1}", _playerAction.GetType().FullName, (debug == EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "", _windowName);
		}
		if (!Empty)
		{
			Top.Enabled = false;
		}
		PlayerActions.Add(_playerAction);
		Top.Enabled = true;
		if (debug != EDebugLevel.Off)
		{
			LogActionSets();
		}
	}

	public void Pop(GUIWindow _window = null)
	{
		if (debug != EDebugLevel.Off)
		{
			Log.Out("LocalPlayerInput.Pop ({1}):{0}", (debug == EDebugLevel.Verbose) ? ("\n" + StackTraceUtility.ExtractStackTrace()) : "", _window?.Id);
		}
		if (Empty)
		{
			Log.Warning("LocalPlayerInput::Pop - Popping input from an empty stack.");
			return;
		}
		int index = PlayerActions.Count - 1;
		if (_window != null)
		{
			PlayerActionsBase actionSet = _window.GetActionSet();
			if (actionSet != null && actionSet != PlayerActions[index])
			{
				Log.Warning("LocalPlayerInput::Pop - Tried to pop a different action set from what belongs to window " + _window.Id);
				return;
			}
		}
		Top.Enabled = false;
		PlayerActions.RemoveAt(index);
		if (!Empty)
		{
			Top.Enabled = true;
		}
		if (debug != EDebugLevel.Off)
		{
			LogActionSets();
		}
	}

	public void LogActionSets()
	{
		string text = "";
		for (int i = 0; i < PlayerActions.Count; i++)
		{
			text = text + PlayerActions[i].GetType().Name + " (" + PlayerActions[i].Enabled + "), ";
		}
		string text2 = "";
		if ((PlatformManager.NativePlatform?.Input?.ActionSets?.Count).GetValueOrDefault() > 0)
		{
			for (int j = 0; j < PlatformManager.NativePlatform.Input.ActionSets.Count; j++)
			{
				text2 += $"{PlatformManager.NativePlatform.Input.ActionSets[j].GetType().Name} ({PlatformManager.NativePlatform.Input.ActionSets[j].Enabled}), ";
			}
		}
		Log.Out("ActionSets: Stack: {0} --- All: {1}", text, text2);
	}

	public void Reset()
	{
		while (!Empty)
		{
			Pop();
		}
	}
}
