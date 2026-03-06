using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;

public static class InputUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<PlayerActionSet, bool> previousState = new Dictionary<PlayerActionSet, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool? isMac;

	public static bool IsMac
	{
		get
		{
			if (!isMac.HasValue)
			{
				RuntimePlatform platform = Application.platform;
				isMac = platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer;
			}
			return isMac.Value;
		}
	}

	public static bool ControlKeyPressed
	{
		get
		{
			if (!IsMac)
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					return Input.GetKey(KeyCode.RightControl);
				}
				return true;
			}
			if (!Input.GetKey(KeyCode.LeftMeta))
			{
				return Input.GetKey(KeyCode.RightMeta);
			}
			return true;
		}
	}

	public static bool ShiftKeyPressed
	{
		get
		{
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				return Input.GetKey(KeyCode.RightShift);
			}
			return true;
		}
	}

	public static bool AltKeyPressed
	{
		get
		{
			if (!Input.GetKey(KeyCode.LeftAlt))
			{
				return Input.GetKey(KeyCode.RightAlt);
			}
			return true;
		}
	}

	public static void EnableAllPlayerActions(bool _enable)
	{
		if (ActionSetManager.DebugLevel != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("EnableAllPlayerActions: " + _enable);
		}
		if (!_enable)
		{
			previousState.Clear();
			{
				foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
				{
					if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
					{
						Log.Out($"PAS: {actionSet} IsInDict {previousState.ContainsKey(actionSet)}");
					}
					if (!previousState.ContainsKey(actionSet))
					{
						if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
						{
							Log.Out($"Disabling: {actionSet} was {actionSet.Enabled}");
						}
						previousState.Add(actionSet, actionSet.Enabled);
						actionSet.Enabled = false;
					}
				}
				return;
			}
		}
		foreach (PlayerActionsBase actionSet2 in PlatformManager.NativePlatform.Input.ActionSets)
		{
			if (previousState.ContainsKey(actionSet2))
			{
				if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
				{
					Log.Out($"PrevState contains: {actionSet2} was {previousState[actionSet2]}");
				}
				actionSet2.Enabled = previousState[actionSet2];
			}
			else
			{
				if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
				{
					Log.Out($"PrevState does not contain: {actionSet2}");
				}
				actionSet2.Enabled = true;
			}
		}
	}
}
