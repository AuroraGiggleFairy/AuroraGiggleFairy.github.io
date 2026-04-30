using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTrees : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "trees" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches trees on/off";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "trees - toggles\ntrees <value> - (off, on)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			DecoManager.Instance.IsHidden = !DecoManager.Instance.IsHidden;
		}
		else if (_params[0] == "on")
		{
			DecoManager.Instance.IsHidden = false;
		}
		else
		{
			if (!(_params[0] == "off"))
			{
				if (int.TryParse(_params[0], out var result))
				{
					DecoManager.Instance.SetChunkDistance(result);
					DecoManager.Instance.OnWorldUnloaded();
					IChunkProvider chunkProvider = GameManager.Instance.World.ChunkClusters[0].ChunkProvider;
					ThreadManager.RunCoroutineSync(DecoManager.Instance.OnWorldLoaded(chunkProvider.GetWorldSize().x, chunkProvider.GetWorldSize().y, GameManager.Instance.World, chunkProvider));
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting to deco chunk distance " + result + " =" + 128 * result + "m");
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
				}
				return;
			}
			DecoManager.Instance.IsHidden = true;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Trees set to " + ((!DecoManager.Instance.IsHidden) ? "on" : "off"));
	}
}
