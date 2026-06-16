using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdExhausted : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "exhausted" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			float num = 0f;
			if (_params.Count > 0)
			{
				num = StringParsers.ParseFloat(_params[0]);
			}
			primaryPlayer.Stats.Stamina.Value = num * primaryPlayer.Stats.Stamina.Max;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Makes the player exhausted.";
	}
}
