using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnEntity : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "spawnentity", "se" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "se playerId entity# <count> - spawn around playerId (0 for local) entity# of count (1 to 100)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		int num = 1;
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("players:");
			foreach (KeyValuePair<int, EntityPlayer> item in world.Players.dict)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + item.Key + " - " + item.Value.EntityName);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("entity numbers:");
			{
				foreach (KeyValuePair<int, EntityClass> item2 in EntityClass.list.Dict)
				{
					if (item2.Value.userSpawnType != EntityClass.UserSpawnType.None)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + num + " - " + item2.Value.entityClassName);
						num++;
					}
				}
				return;
			}
		}
		EntityPlayer value = null;
		int result;
		bool flag = int.TryParse(_params[0], out result);
		if (flag && result == 0)
		{
			value = world.GetPrimaryPlayer();
		}
		else
		{
			if (!flag)
			{
				result = -1;
				for (int i = 0; i < world.Players.list.Count; i++)
				{
					EntityPlayer entityPlayer = world.Players.list[i];
					if (entityPlayer.EntityName.EqualsCaseInsensitive(_params[0]))
					{
						result = entityPlayer.entityId;
						break;
					}
				}
			}
			if (result != -1)
			{
				world.Players.dict.TryGetValue(result, out value);
			}
		}
		if (!value)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player '" + _params[0] + "' not found");
			return;
		}
		int result2 = -1;
		int.TryParse(_params[1], out result2);
		int num2 = 1;
		if (_params.Count >= 3)
		{
			num2 = int.Parse(_params[2]);
			num2 = Utils.FastMin(num2, 100);
		}
		num = 1;
		foreach (KeyValuePair<int, EntityClass> item3 in EntityClass.list.Dict)
		{
			if (item3.Value.userSpawnType == EntityClass.UserSpawnType.None)
			{
				continue;
			}
			if (num == result2 || item3.Value.entityClassName.Equals(_params[1]))
			{
				if (item3.Value.entityClassName == "entityJunkDrone")
				{
					if (!EntityDrone.IsValidForLocalPlayer())
					{
						return;
					}
					world.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
					num2 = 1;
				}
				for (int j = 0; j < num2; j++)
				{
					if (!world.FindRandomSpawnPointNearPlayer(value, 15, out var x, out var y, out var z, 10))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No spawn point found near player!");
						return;
					}
					Entity entity = EntityFactory.CreateEntity(item3.Key, new Vector3(x, (float)y + 0.3f, z));
					world.SpawnEntityInWorld(entity);
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Spawned " + item3.Value.entityClassName);
				return;
			}
			num++;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity '" + _params[1] + "' not found");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "spawns an entity";
	}
}
