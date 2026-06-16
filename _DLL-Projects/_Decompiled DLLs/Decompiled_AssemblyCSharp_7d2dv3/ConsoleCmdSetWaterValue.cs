using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetWaterValue : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "setwatervalue", "swv" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Sets the water value for all flow-permitting blocks within the current selection area, specified in the range of 0 (empty) to 1 (full).";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "'swv [0.0 - 1.0]' Sets water value between empty (0.0) and full (1.0) for all flow-permitting blocks within the current selection bounds. \nE.g. 'swv 0.5' will set all affected blocks to be half-full. \nBlocks which do not permit flow are unchanged.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1 || !float.TryParse(_params[0], out var result) || result < 0f || result > 1f)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		BlockToolSelection instance = BlockToolSelection.Instance;
		if (!instance.SelectionActive)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No selection active. Running this command requires an active selection box.");
		}
		else
		{
			BlockTools.CubeWaterRPC(GameManager.Instance, instance.SelectionStart, instance.SelectionEnd, new WaterValue((int)(result * 19500f)));
		}
	}
}
