using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAI : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "ai" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "AI commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "AI commands:\nactivityclear - remove all activity areas (heat)\nlatency - toggles drawing\npathlines - toggles drawing editor path lines\npathgrid - force grid update\nragdoll <force> <time>\nrage <speed> <time> - make all zombies rage (0 - 2, 0 stops) (seconds)\nsendnames - toggles admin clients receiving debug name info";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		World world = GameManager.Instance.World;
		string text = _params[0].ToLower();
		switch (text)
		{
		case "activityclear":
		case "ac":
			GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>().Clear();
			break;
		case "l":
		case "latency":
		{
			int num = world.GetPrimaryPlayerId();
			if (_senderInfo.RemoteClientInfo != null)
			{
				num = _senderInfo.RemoteClientInfo.entityId;
			}
			if (num != -1)
			{
				AIDirector.DebugToggleSendLatency(num);
			}
			break;
		}
		case "pathlines":
			GameManager.Instance.DebugAILines = !GameManager.Instance.DebugAILines;
			break;
		case "pathgrid":
			if ((bool)AstarManager.Instance)
			{
				AstarManager.Instance.OriginChanged();
			}
			break;
		case "ragdoll":
		{
			float result = 1f;
			float result2 = 1f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out result);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result2);
			}
			{
				foreach (Entity item in world.Entities.list)
				{
					EntityAlive entityAlive = item as EntityAlive;
					if ((bool)entityAlive && !(entityAlive is EntityPlayer) && !(entityAlive is EntityTrader))
					{
						entityAlive.emodel.DoRagdoll(result2, EnumBodyPartHit.None, -entityAlive.GetForwardVector() * result, Vector3.zero, isRemote: false);
					}
				}
				break;
			}
		}
		case "rage":
		{
			float result3 = 1f;
			float result4 = 5f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out result3);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result4);
			}
			{
				foreach (Entity item2 in world.Entities.list)
				{
					EntityHuman entityHuman = item2 as EntityHuman;
					if ((bool)entityHuman)
					{
						if (result3 <= 0f)
						{
							entityHuman.StopRage();
						}
						else
						{
							entityHuman.StartRage(result3, result4);
						}
					}
				}
				break;
			}
		}
		case "sendnames":
			if (_senderInfo.RemoteClientInfo != null)
			{
				AIDirector.DebugToggleSendNameInfo(_senderInfo.RemoteClientInfo.entityId);
			}
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text + ".");
			break;
		}
	}
}
