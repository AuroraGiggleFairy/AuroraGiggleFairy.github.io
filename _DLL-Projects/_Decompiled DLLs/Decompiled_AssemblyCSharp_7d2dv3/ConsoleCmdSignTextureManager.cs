using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSignTextureManager : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureManager.SignTextureQuality previousQuality = SignTextureManager.SignTextureQuality.Medium;

	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "signtexman", "stm" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Allows enabling/disabling the Sign Texture Manager and configuring various baking settings.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "No params: Toggles the enabled state of SignTextureManager. When enabled (default), signs are baked to textures when close to the camera. When disabled, all signs are rendered dynamically (infinite resolution).\nTwo parameter modes: either (string), or (int, optional int, optional bool).\nSingle string parameter:\n   `stm l` (log) will log debug info.\n   `stm ld` (log disk) will do the same and also save the log to the application directory.\nFirst param (integer): Sets sign texture quality level. 0-4 inclusive: Lowest-Ultra. 5: Infinite (no textures).\nSecond param (integer, optional after first param): Sets tile size. Intended to be a neat power of two in the range of 64-1024.\nThird param (bool, optional after second param): Sets debug visualization of in-progress bakes on/off.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World == null)
		{
			Log.Error("Cannot process signtexman/stm command when world is not loaded and active.");
			return;
		}
		SignTextureManager instance = SignTextureManager.Instance;
		if (_params.Count == 0)
		{
			SignTextureManager.SignTextureQuality currentQuality = instance.CurrentQuality;
			if (currentQuality != SignTextureManager.SignTextureQuality.Infinite)
			{
				previousQuality = currentQuality;
				instance.SetQuality(SignTextureManager.SignTextureQuality.Infinite);
				Log.Out("[SignTextureManager] Sign texture manager has been disabled (Quality = SignTextureQuality.Infinite).");
			}
			else
			{
				instance.SetQuality(previousQuality);
				Log.Out($"[SignTextureManager] Sign texture manager has been enabled (Quality = {previousQuality}).");
			}
			return;
		}
		if (_params[0].IndexOf('l', StringComparison.InvariantCultureIgnoreCase) == 0)
		{
			instance.LogDebugInfo(_params[0].IndexOf('d', StringComparison.InvariantCultureIgnoreCase) == 1);
			return;
		}
		if (!int.TryParse(_params[0], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to parse parameter 0 as integer.");
			return;
		}
		SignTextureManager.SignTextureQuality signTextureQuality = (SignTextureManager.SignTextureQuality)result;
		instance.SetQuality(signTextureQuality);
		Log.Out($"[SignTextureManager] Set quality level to {signTextureQuality}.");
		if (_params.Count < 2)
		{
			return;
		}
		if (!int.TryParse(_params[1], out var result2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to parse parameter 1 as integer.");
			return;
		}
		instance.SetTileSize(result2);
		Log.Out($"[SignTextureManager] Set tile size to {result2}.");
		if (_params.Count >= 3)
		{
			if (!bool.TryParse(_params[2], out var result3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to parse parameter 2 as bool.");
				return;
			}
			instance.SetShowProgress(result3);
			Log.Out($"[SignTextureManager] Set show progress to {result3}.");
		}
	}
}
