using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnEntityAt : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "spawnentityat", "sea" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns an entity at a give position";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. spawnentityat\n  2. spawnentityat <entityidx> <x> <y> <z>\n  3. spawnentityat <entityidx> <x> <y> <z> <count>\n  4. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ>\n  5. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ> <stepX> <stepY> <stepZ>\n  6. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ> <stepX> <stepY> <stepZ> <spawnerType>\n1. Lists the known entity class names\n2. Spawns the entity with the given class name at the given coordinates\n3. As 2. but spawns <count> instances of that entity type\n4. As 3. but also specifies the rotation of the spawned entities\n5. As 4. but also specify the step distance between entities\n6. As 5. but also specify the spawner source of the entity (Dynamic, StaticSpawner, Biome)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity class names:");
			int num = 1;
			{
				foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
				{
					if (item.Value.userSpawnType != EntityClass.UserSpawnType.None)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + num + " - " + item.Value.entityClassName);
						num++;
					}
				}
				return;
			}
		}
		if (_params.Count != 4 && _params.Count != 5 && _params.Count != 8 && _params.Count != 11 && _params.Count != 12)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal number of parameters");
			return;
		}
		string b = _params[0];
		if (!StringParsers.TryParseFloat(_params[1], out var _result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x coordinate is not a valid float");
			return;
		}
		if (!StringParsers.TryParseFloat(_params[2], out var _result2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y coordinate is not a valid float");
			return;
		}
		if (!StringParsers.TryParseFloat(_params[3], out var _result3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z coordinate is not a valid float");
			return;
		}
		Vector3 transformPos = new Vector3(_result, _result2, _result3);
		int result = 1;
		if (_params.Count > 4 && !int.TryParse(_params[4], out result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("count is not a valid integer");
			return;
		}
		Vector3 rotation = Vector3.zero;
		if (_params.Count > 5)
		{
			if (!StringParsers.TryParseFloat(_params[5], out var _result4))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotX is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[6], out var _result5))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotY is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[7], out var _result6))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotZ is not a valid float");
				return;
			}
			rotation = new Vector3(_result4, _result5, _result6);
		}
		Vector3 zero = Vector3.zero;
		if (_params.Count > 8)
		{
			if (!StringParsers.TryParseFloat(_params[8], out zero.x))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotX is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[9], out zero.y))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotY is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[10], out zero.z))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotZ is not a valid float");
				return;
			}
		}
		EnumSpawnerSource spawnerSource = EnumSpawnerSource.Unknown;
		if (_params.Count > 11)
		{
			try
			{
				spawnerSource = EnumUtils.Parse<EnumSpawnerSource>(_params[11]);
			}
			catch (ArgumentException)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("spawnerType is not a valid value");
				return;
			}
		}
		foreach (KeyValuePair<int, EntityClass> item2 in EntityClass.list.Dict)
		{
			if (item2.Value.userSpawnType == EntityClass.UserSpawnType.None || !item2.Value.entityClassName.EqualsCaseInsensitive(b))
			{
				continue;
			}
			if (item2.Value.entityClassName == "entityJunkDrone")
			{
				if (!EntityDrone.IsValidForLocalPlayer())
				{
					return;
				}
				GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
			}
			transformPos -= zero * ((float)(result - 1) * 0.5f);
			for (int i = 0; i < result; i++)
			{
				Entity entity = EntityFactory.CreateEntity(item2.Key, transformPos, rotation);
				entity.SetSpawnerSource(spawnerSource);
				GameManager.Instance.World.SpawnEntityInWorld(entity);
				transformPos += zero;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Spawned " + result + " " + item2.Value.entityClassName);
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity class name '" + _params[0] + "' unknown or not allowed to be instantiated");
	}
}
