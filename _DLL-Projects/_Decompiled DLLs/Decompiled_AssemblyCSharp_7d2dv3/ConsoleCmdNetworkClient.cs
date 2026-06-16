using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNetworkClient : ConsoleCmdNetworkServer
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "networkclient", "netc" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Client side network commands";
	}
}
