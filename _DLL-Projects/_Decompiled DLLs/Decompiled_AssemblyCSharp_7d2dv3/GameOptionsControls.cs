using System.Collections.Generic;
using InControl;
using Platform;

public static class GameOptionsControls
{
	public const string cActionSetSavePrefix = "ActionSet_";

	public const string ActionSetSaveModeFlagPref = "ActionSetsSaved";

	public const string LegacyControlsPref = "Controls";

	public static IEnumerable<(PlayerActionsBase, string)> ActionSetPrefs
	{
		get
		{
			foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
			{
				yield return (actionSet, "ActionSet_" + actionSet.Name);
			}
		}
	}

	public static void Save()
	{
		foreach (var actionSetPref in ActionSetPrefs)
		{
			var (playerActionsBase, _) = actionSetPref;
			SdPlayerPrefs.SetString(actionSetPref.Item2, playerActionsBase.Save());
		}
		SdPlayerPrefs.SetString("Controls", Export());
		SdPlayerPrefs.SetInt("ActionSetsSaved", 0);
		ApplyAllowControllerOption();
	}

	public static void Load()
	{
		bool flag = SdPlayerPrefs.HasKey("ActionSetsSaved");
		if (!flag && SdPlayerPrefs.HasKey("Controls"))
		{
			string text = SdPlayerPrefs.GetString("Controls", string.Empty);
			PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(text.Split(';'));
			Save();
			ApplyAllowControllerOption();
			RestoreNonBindableControllerActionsToDefaults();
			Log.Out("Legacy controls data converted");
			return;
		}
		if (flag)
		{
			foreach (var (playerActionsBase, key) in ActionSetPrefs)
			{
				if (!string.IsNullOrEmpty(SdPlayerPrefs.GetString(key, string.Empty)))
				{
					playerActionsBase.Load(SdPlayerPrefs.GetString(key));
				}
				else
				{
					Log.Warning("Loading controls: No data for action set " + playerActionsBase.Name);
				}
			}
		}
		ApplyAllowControllerOption();
		RestoreNonBindableControllerActionsToDefaults();
	}

	public static string Export()
	{
		string[] array = new string[PlatformManager.NativePlatform.Input.ActionSets.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PlatformManager.NativePlatform.Input.ActionSets[i].Save();
		}
		return string.Join(";", array);
	}

	public static void Import(string importString)
	{
		PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(importString.Split(';'));
		ApplyAllowControllerOption();
		RestoreNonBindableControllerActionsToDefaults();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyAllowControllerOption()
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsAllowController);
		for (int i = 0; i < PlatformManager.NativePlatform.Input.ActionSets.Count; i++)
		{
			PlatformManager.NativePlatform.Input.ActionSets[i].Device = (flag ? null : InputDevice.Null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RestoreNonBindableControllerActionsToDefaults()
	{
		PlatformManager.NativePlatform.Input.GetActionSetForName("gui").ResetControllerBindings();
		PlatformManager.NativePlatform.Input.GetActionSetForName("permanent").ResetControllerBindings();
	}
}
