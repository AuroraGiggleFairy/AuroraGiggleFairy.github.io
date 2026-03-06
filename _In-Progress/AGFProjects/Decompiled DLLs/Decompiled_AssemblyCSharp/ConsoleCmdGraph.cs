using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGraph : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "graph" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Draws graphs on screen";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Graph commands:\n# - 0 removes all graphs, 1+ sets graphs height\ncvar <name> <count> <max> - show graph of a cvar, line count (0 hides), max graph value (default is 1)\ndr <count> - show dynamic res graph, line count (0 hides)\nfps <count> <fps max> - show fps graph, line count (0 hides), max graph value\npe <name> <count> <max> - show graph of a passive effect (healthmax..), line count (0 hides), max graph value (default is 1)\nspf <count> <spf max> - show seconds per frame graph, line count (0 hides), max graph value\nstat <name> <count> <max> - show graph of a stat (health, stamina..) with line count (0 hides), max graph value (default is 1)\ntex <name> <count> - show texture graph (mem or stream), line count (0 hides)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
		}
		else
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			GameRenderManager renderManager = GameManager.Instance.World.GetPrimaryPlayer().renderManager;
			GameGraphManager graphManager = renderManager.graphManager;
			string text = _params[0].ToLower();
			if (char.IsDigit(text[0]))
			{
				int num = int.Parse(text);
				if (num == 0)
				{
					graphManager.RemoveAll();
				}
				else
				{
					graphManager.SetHeight(num);
				}
				return;
			}
			switch (text)
			{
			case "dr":
			{
				int result5 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out result5);
				}
				graphManager.Add("Dynamic Res", renderManager.DynamicResolutionUpdateGraph, result5, 1f, 0.5f);
				return;
			}
			case "fps":
			{
				int result6 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out result6);
				}
				int result7 = 100;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result7);
				}
				graphManager.Add("FPS", renderManager.FPSUpdateGraph, result6, result7, 60f);
				return;
			}
			case "spf":
			{
				int result2 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out result2);
				}
				int result3 = 100;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result3);
				}
				int result4 = 17;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out result4);
				}
				graphManager.Add("SPF", renderManager.SPFUpdateGraph, result2, result3, result4);
				return;
			}
			case "tex":
			{
				if (_params.Count < 2)
				{
					break;
				}
				string text2 = _params[1].ToLower();
				int result = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result);
				}
				if (!(text2 == "mem"))
				{
					if (text2 == "stream")
					{
						GameGraphManager.Graph.Callback callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
						{
							value = Texture.streamingTextureCount;
							return true;
						};
						graphManager.Add("TStream Textures", callback, result, 3000f);
						callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
						{
							value = Texture.streamingTextureLoadingCount;
							return true;
						};
						graphManager.Add("TStream Loading", callback, result, 100f);
						callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
						{
							value = Texture.streamingRendererCount;
							return true;
						};
						graphManager.Add("TStream Renderers", callback, result, 25000f);
					}
				}
				else
				{
					GameGraphManager.Graph.Callback callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
					{
						value = Texture.currentTextureMemory / 1048576;
						return true;
					};
					graphManager.Add("Tex Current", callback, result, 12288f, 6144f);
					callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
					{
						value = Texture.desiredTextureMemory / 1048576;
						return true;
					};
					graphManager.Add("Tex Desired", callback, result, 12288f, 6144f);
					callback = [PublicizedFrom(EAccessModifier.Internal)] (ref float value) =>
					{
						value = Texture.totalTextureMemory / 1048576;
						return true;
					};
					graphManager.Add("Tex Total", callback, result, 12288f, 6144f);
				}
				return;
			}
			}
			switch (text)
			{
			case "cvar":
			{
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No cvar name");
					break;
				}
				string text4 = _params[1];
				int result13 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result13);
				}
				int result14 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out result14);
				}
				graphManager.AddCVar("CVar " + text4, result13, text4, result14);
				break;
			}
			case "pe":
			{
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No pe name");
					break;
				}
				Enum.TryParse<PassiveEffects>(_params[1], ignoreCase: true, out var result10);
				int result11 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result11);
				}
				int result12 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out result12);
				}
				graphManager.AddPassiveEffect("PE " + result10.ToStringCached(), result11, result10, result12);
				break;
			}
			case "stat":
			{
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No stat name");
					break;
				}
				string text3 = _params[1];
				int result8 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result8);
				}
				int result9 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out result9);
				}
				graphManager.AddStat("Stat " + text3, result8, text3, result9);
				break;
			}
			default:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + _params[0]);
				break;
			}
		}
	}
}
