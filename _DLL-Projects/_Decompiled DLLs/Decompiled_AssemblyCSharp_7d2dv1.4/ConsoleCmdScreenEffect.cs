using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdScreenEffect : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "ScreenEffect" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Sets a screen effect";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "ScreenEffect [name] [intensity] [fade time]\nScreenEffect clear\nScreenEffect reload";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		if (_params[0] == "clear")
		{
			for (int i = 0; i < localPlayers.Count; i++)
			{
				localPlayers[i].ScreenEffectManager.DisableScreenEffects();
			}
			return;
		}
		if (_params[0] == "reload")
		{
			for (int j = 0; j < localPlayers.Count; j++)
			{
				localPlayers[j].ScreenEffectManager.ResetEffects();
			}
			return;
		}
		float _result = 0f;
		float _result2 = 4f;
		if (_params.Count >= 2)
		{
			StringParsers.TryParseFloat(_params[1], out _result);
		}
		if (_params.Count >= 3)
		{
			StringParsers.TryParseFloat(_params[2], out _result2);
		}
		for (int k = 0; k < localPlayers.Count; k++)
		{
			localPlayers[k].ScreenEffectManager.SetScreenEffect(_params[0], _result, _result2);
		}
	}
}
