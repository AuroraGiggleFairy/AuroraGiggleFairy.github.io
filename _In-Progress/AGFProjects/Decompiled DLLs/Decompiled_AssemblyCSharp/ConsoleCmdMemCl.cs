using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMemCl : ConsoleCmdMem
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "memcl" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Prints memory information on client and calls garbage collector";
	}
}
