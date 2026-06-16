using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowSpecular : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "showspecular", "spec" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		Polarizer.SetDebugView((Polarizer.GetDebugView() != Polarizer.ViewEnums.Specular) ? Polarizer.ViewEnums.Specular : Polarizer.ViewEnums.None);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("showspecular " + ((Polarizer.GetDebugView() == Polarizer.ViewEnums.Specular) ? "on" : "off"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enables/disables display of specular values in gBuffer";
	}
}
