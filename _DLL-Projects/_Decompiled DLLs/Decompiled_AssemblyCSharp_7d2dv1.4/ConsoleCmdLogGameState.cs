using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogGameState : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class HierarchyElement
	{
		public string name;

		public bool active;

		public int childCount;

		public List<HierarchyElement> children = new List<HierarchyElement>();

		public HierarchyElement(string _name, bool _active)
		{
			name = _name;
			active = _active;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, string> prefixPerLevel = new Dictionary<int, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string indentationNormal = "   |  ";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string indentationLastLevel = "   +- ";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string startEndSep = new string('*', 100);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string sectionSep = new string('-', 50);

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "loggamestate", "lgs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Log the current state of the game";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Writes information on the current state of the game (like memory usage,\nentities) to the log file. The section will use the message parameter\nin its header.\nUsage:\n   loggamestate <message> [true/false]\nMessage is a string that will be included in the header of the generated\nlog section. The optional boolean parameter specifies if this command\nshould be run on the client (true) instead of the server (false) which\nis the default.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1 || _params.Count > 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 or 2, found " + _params.Count + ".");
			return;
		}
		string text = _params[0];
		bool flag = false;
		if (_params.Count == 2)
		{
			try
			{
				flag = ConsoleHelper.ParseParamBool(_params[1]);
			}
			catch (ArgumentException)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid boolean value.");
				return;
			}
		}
		if (flag)
		{
			if (_senderInfo.RemoteClientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Second parameter may only be set to \"true\" if the command is executed from a game client.");
			}
			else
			{
				_senderInfo.RemoteClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("lgs \"" + text + "\"", _bExecute: true));
			}
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch();
		WriteHeader(text);
		WriteGeneric();
		WriteCounts();
		WriteEntities();
		WritePlayers();
		WriteThreads();
		WriteUnityObjects();
		WriteGameObjects();
		WriteFooter(text);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Wrote game state to game log file, header includes \"{text}\", took {microStopwatch.ElapsedMilliseconds} ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteGeneric()
	{
		(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements((GameManager.Instance.World != null) ? GameManager.Instance.World.worldTime : 0);
		int item = tuple.Days;
		int item2 = tuple.Hours;
		int item3 = tuple.Minutes;
		int num = (int)Time.timeSinceLevelLoad;
		int num2 = num % 60;
		int num3 = num / 60 % 60;
		int num4 = num / 3600;
		newSection("Generic information");
		printLine("System time: {0}", DateTime.Now.ToString("HH:mm:ss"));
		printLine("Game time:   Day {0}, {1:00}:{2:00}", item, item2, item3);
		printLine("Game uptime: {0:00}:{1:00}:{2:00}", num4, num3, num2);
		printLine("FPS:         {0}", GameManager.Instance.fps.Counter.ToCultureInvariantString("F1"));
		printLine("Heap:        {0} MiB / max {1} MiB", GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024, GameManager.MaxMemoryConsumption / 1024 / 1024);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteCounts()
	{
		newSection("Object counts");
		printLine("Active GameObjs:  {0}", UnityEngine.Object.FindObjectsOfType<GameObject>().Length);
		printLine("Script instances: {0}", UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Length);
		printLine("Total Objects:    {0}", UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>().Length);
		if (GameManager.Instance.World != null)
		{
			printLine("Chunks:     {0}", Chunk.InstanceCount);
			printLine("ChunkGOs:   {0}", GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount());
			printLine("Players:    {0}", GameManager.Instance.World.Players.Count);
			printLine("Zombies:    {0}", GameStats.GetInt(EnumGameStats.EnemyCount));
			printLine("Entities:   {0} in world, {1} loaded", GameManager.Instance.World.Entities.Count, Entity.InstanceCount);
			printLine("Items:      {0}", EntityItem.ItemInstanceCount);
			printLine("ChunkObs:   {0}", GameManager.Instance.World.m_ChunkManager.m_ObservedEntities.Count);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteEntities()
	{
		if (GameManager.Instance.World != null)
		{
			newSection("Entities");
			int num = 0;
			for (int num2 = GameManager.Instance.World.Entities.list.Count - 1; num2 >= 0; num2--)
			{
				Entity entity = GameManager.Instance.World.Entities.list[num2];
				printLine("{0,3}. id={1}, name={2}, pos={3}, lifetime={4}, remote={5}, dead={6}", ++num, entity.entityId, entity.ToString(), entity.GetPosition().ToCultureInvariantString(), entity.lifetime.ToCultureInvariantString("F1"), entity.isEntityRemote, entity.IsDead());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WritePlayers()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		newSection("Players");
		int num = 0;
		foreach (KeyValuePair<int, EntityPlayer> item in GameManager.Instance.World.Players.dict)
		{
			string text = "<unknown>";
			PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = null;
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(item.Key);
			if (clientInfo != null)
			{
				text = clientInfo.ip;
				platformUserIdentifierAbs = clientInfo.PlatformId;
				platformUserIdentifierAbs2 = clientInfo.CrossplatformId;
			}
			printLine("{0,3}. id={1}, {2}, pos={3}, remote={4}, pltfmid={5}, crossid={6}, ip={7}, ping={8}", ++num, item.Value.entityId, item.Value.EntityName, item.Value.position.ToCultureInvariantString(), item.Value.isEntityRemote, platformUserIdentifierAbs?.CombinedString ?? "<unknown>", platformUserIdentifierAbs2?.CombinedString ?? "<unknown>", text, item.Value.pingToServer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteThreads()
	{
		newSection("Threads");
		int num = 0;
		foreach (KeyValuePair<string, ThreadManager.ThreadInfo> activeThread in ThreadManager.ActiveThreads)
		{
			printLine("{0,3}. {1}", ++num, activeThread.Key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteUnityObjects()
	{
		SortedDictionary<string, int> sortedDictionary = new SortedDictionary<string, int>();
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
		for (int i = 0; i < array.Length; i++)
		{
			string fullName = array[i].GetType().FullName;
			if (sortedDictionary.ContainsKey(fullName))
			{
				sortedDictionary[fullName]++;
			}
			else
			{
				sortedDictionary[fullName] = 1;
			}
		}
		newSection("Unity objects by type");
		foreach (KeyValuePair<string, int> item in sortedDictionary)
		{
			printLine("{0,5} * {1}", item.Value, item.Key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteGameObjects()
	{
		newSection("GameObjects");
		HierarchyElement hierarchyElement = new HierarchyElement("<top>", _active: true);
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.transform.parent == null)
			{
				hierarchyElement.childCount += TraverseScene(hierarchyElement, gameObject);
			}
		}
		PrintGameObjectHierarchy(hierarchyElement, 0, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int TraverseScene(HierarchyElement _parent, GameObject _go)
	{
		HierarchyElement hierarchyElement = new HierarchyElement(_go.name, _go.activeInHierarchy);
		_parent.children.Add(hierarchyElement);
		foreach (Transform item in _go.transform)
		{
			hierarchyElement.childCount += TraverseScene(hierarchyElement, item.gameObject);
		}
		return hierarchyElement.childCount + 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrintGameObjectHierarchy(HierarchyElement _he, int _indentation, string _prefix)
	{
		if (_he.active)
		{
			printLine("{0}{1} (children={2})", _prefix, _he.name, _he.childCount);
			if (!prefixPerLevel.ContainsKey(_indentation + 1))
			{
				string value = ((_indentation != 0) ? (indentationNormal + _prefix) : (_prefix + indentationLastLevel));
				prefixPerLevel[_indentation + 1] = value;
			}
			for (int i = 0; i < _he.children.Count; i++)
			{
				HierarchyElement he = _he.children[i];
				PrintGameObjectHierarchy(he, _indentation + 1, prefixPerLevel[_indentation + 1]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteHeader(string _message)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine(startEndSep);
		Console.Out.WriteLine("WRITING GAME STATE: " + _message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteFooter(string _message)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine("END OF GAME STATE: " + _message);
		Console.Out.WriteLine(startEndSep);
		Console.Out.WriteLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newSection(string _sectionName)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine(sectionSep);
		Console.Out.WriteLine(_sectionName + ":");
		Console.Out.WriteLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void print(string _format, params object[] _args)
	{
		Console.Out.Write(string.Format(_format, _args));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printLine(string _format, params object[] _args)
	{
		Console.Out.WriteLine(string.Format(_format, _args));
	}
}
