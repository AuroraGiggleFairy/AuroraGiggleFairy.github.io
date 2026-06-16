using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideoSimplified : XUiC_OptionsVideoBase
{
	public enum UpscalerMode
	{
		FSR3,
		Scale
	}

	public enum GraphicsMode
	{
		ConsolePerformance,
		ConsoleQuality,
		ConsoleCustom
	}

	[XuiBindComponent("OptionConsoleUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionUpscaler;

	[XuiBindComponent("ConsoleUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboUpscaler;

	[XuiBindComponent("OptionQualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionQualityPreset;

	[XuiBindComponent("QualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboQualityPreset;

	public static string ID = "";

	public static event Action OnSettingsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public void initUpscalerModeOptions()
	{
		if (optionUpscaler == null || comboUpscaler == null)
		{
			return;
		}
		optionUpscaler.GetSettingValue = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			int num = GamePrefUpscalerModeIndex();
			if (num == -1)
			{
				Log.Out($"Upscaler mode \"{GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode)}\" is unsupported on this platform; defaulting to \"{2}\".");
				num = 0;
				GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 2);
			}
			comboUpscaler.SelectedIndex = num;
		};
		optionUpscaler.DiscardChanges = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			comboUpscaler.SelectedIndex = GamePrefUpscalerModeIndex();
		};
		optionUpscaler.ApplyChanges = [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, SelectionToUpscalerMode());
		};
		optionUpscaler.IsChangedDelegate = [PublicizedFrom(EAccessModifier.Private)] () => comboUpscaler.SelectedIndex != GamePrefUpscalerModeIndex();
		[PublicizedFrom(EAccessModifier.Internal)]
		static int GamePrefUpscalerModeIndex()
		{
			return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode) switch
			{
				2 => 0, 
				4 => 1, 
				_ => -1, 
			};
		}
		[PublicizedFrom(EAccessModifier.Private)]
		int SelectionToUpscalerMode()
		{
			return comboUpscaler.SelectedIndex switch
			{
				0 => 2, 
				1 => 4, 
				_ => GameOptionsPlatforms.DefaultUpscalerMode, 
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initQualityPresetBasedOptions()
	{
		optionQualityPreset.MapGamePrefToListIndex = [PublicizedFrom(EAccessModifier.Internal)] (int _prefValue) => (GameOptionsPlatforms.GfxPreset)_prefValue switch
		{
			GameOptionsPlatforms.GfxPreset.ConsolePerformance => 0, 
			GameOptionsPlatforms.GfxPreset.ConsoleQuality => 1, 
			_ => 2, 
		};
		optionQualityPreset.MapListIndexToGamePref = [PublicizedFrom(EAccessModifier.Internal)] (int _index) => (GraphicsMode)_index switch
		{
			GraphicsMode.ConsolePerformance => 6, 
			GraphicsMode.ConsoleQuality => 8, 
			_ => 5, 
		};
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		initUpscalerModeOptions();
		initQualityPresetBasedOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateCustomModeVisibility();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCustomModeVisibility()
	{
		comboQualityPreset.MaxIndex = Math.Max(comboQualityPreset.SelectedIndex, 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		updateCustomModeVisibility();
		GameOptionsManager.SetGraphicsQuality();
		GameOptionsManager.ApplyAllOptions(xui.playerUI);
		XUiC_OptionsVideoSimplified.OnSettingsChanged?.Invoke();
	}
}
