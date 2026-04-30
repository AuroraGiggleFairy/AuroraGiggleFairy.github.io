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
		return "AI commands:\nactivityclear - remove all activity areas (heat)\nanim <name> - trigger an animation (attack, attack2)\nanimmove <forward> <strafe> - set animation forward and strafe motion\nfreezepos - toggles movement\nlatency - toggles drawing\npathlines - toggles drawing editor path lines\npathgrid - force grid update\nragdoll <force> <time>\nrage <speed> <time> - make all zombies rage (0 - 2, 0 stops) (seconds)\nsendnames - toggles admin clients receiving debug name info";
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
		case "anim":
			if (_params.Count < 2)
			{
				break;
			}
			{
				foreach (Entity item in world.Entities.list)
				{
					EntityAlive entityAlive3 = item as EntityAlive;
					if (!entityAlive3 || entityAlive3.entityType == EntityType.Player)
					{
						continue;
					}
					string text2 = _params[1].ToLower();
					if (!(text2 == "attack"))
					{
						if (text2 == "attack2")
						{
							entityAlive3.StartAnimAction(3000);
						}
					}
					else
					{
						entityAlive3.StartAnimAction(0);
					}
				}
				break;
			}
		case "animmove":
		{
			float result3 = 0f;
			float result4 = 0f;
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
					EntityAlive entityAlive = item2 as EntityAlive;
					if ((bool)entityAlive && entityAlive.entityType != EntityType.Player)
					{
						entityAlive.speedForward = result3;
						entityAlive.speedStrafe = result4;
					}
				}
				break;
			}
		}
		case "freezepos":
		case "fp":
			AIDirector.DebugToggleFreezePos();
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
			float result5 = 1f;
			float result6 = 1f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out result5);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result6);
			}
			{
				foreach (Entity item3 in world.Entities.list)
				{
					EntityAlive entityAlive2 = item3 as EntityAlive;
					if ((bool)entityAlive2 && !(entityAlive2 is EntityPlayer) && !(entityAlive2 is EntityTrader))
					{
						entityAlive2.emodel.DoRagdoll(result6, EnumBodyPartHit.None, -entityAlive2.GetForwardVector() * result5, Vector3.zero, isRemote: false);
					}
				}
				break;
			}
		}
		case "rage":
		{
			float result = 1f;
			float result2 = 5f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out result);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result2);
			}
			{
				foreach (Entity item4 in world.Entities.list)
				{
					EntityHuman entityHuman = item4 as EntityHuman;
					if ((bool)entityHuman)
					{
						if (result <= 0f)
						{
							entityHuman.StopRage();
						}
						else
						{
							entityHuman.StartRage(result, result2);
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
