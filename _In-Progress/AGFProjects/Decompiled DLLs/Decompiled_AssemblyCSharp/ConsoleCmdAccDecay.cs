using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAccDecay : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[4] { "AccDecay", "SetAccDecay", "SetAccuracyDecay", "sad" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Accuracy Decay for guns, show/hide/reset/<Decimal value>";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "reset - apply the default settings for this command\n<Decimal Value> - Sets the decay constant for accuracy calculations using ItemActionRanged";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1)
		{
			if (float.TryParse(_params[0], out var result))
			{
				ItemActionRanged.AccuracyUpdateDecayConstant = result;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Set ItemActionRanged.DecayConstant to {result} and reset old accuracy for checking");
			}
			else if (_params[0].ToLower() == "reset")
			{
				ItemActionRanged.LogOldAccuracy = false;
				ItemActionRanged.AccuracyUpdateDecayConstant = 9.1f;
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"ItemActionRanged.DecayConstant: {ItemActionRanged.AccuracyUpdateDecayConstant}");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"ItemActionRanged.DecayConstant: {ItemActionRanged.AccuracyUpdateDecayConstant}");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"ItemActionRanged.LogOldAccuracy: {ItemActionRanged.LogOldAccuracy}");
		}
	}
}
