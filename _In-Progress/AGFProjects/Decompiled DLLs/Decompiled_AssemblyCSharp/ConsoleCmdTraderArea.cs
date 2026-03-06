using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTraderArea : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "traderarea" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "...";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		bool flag = true;
		if (_params.Count > 0)
		{
			flag = StringParsers.ParseBool(_params[0]);
			for (int i = 0; i < GameManager.Instance.World.TraderAreas.Count; i++)
			{
				GameManager.Instance.World.TraderAreas[i].SetClosed(GameManager.Instance.World, flag, null);
			}
		}
		else
		{
			for (int j = 0; j < GameManager.Instance.World.TraderAreas.Count; j++)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"TraderArea: Position: {GameManager.Instance.World.TraderAreas[j].Position} - IsClosed: {GameManager.Instance.World.TraderAreas[j].IsClosed}");
			}
		}
	}
}
