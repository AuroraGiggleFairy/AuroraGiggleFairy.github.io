using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPrefabUpdater : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public NameIdMapping idMapping = new NameIdMapping(null, Block.MAX_BLOCKS);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<(string oldName, string newName)> mappingTable = new List<(string, string)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DefaultMappingFile = "BlockUpdates.csv";

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "prefabupdater" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Update prefabs for newer game builds.\nUsage:\n   1. prefabupdater loadxml <xmlfile>\n   2. prefabupdater clearxml\n   3. prefabupdater createmapping <prefabname>\n   4. prefabupdater loadtable [nametablefile]\n   5. prefabupdater unloadtables\n   6. prefabupdater updateblocks\n\n1. Load a blocks.xml that has the information about the prefabs to be\n   updated. If you have a modded XML first load that modded XML and\n   afterwards load the XML provided with the game for legacy prefabs.\n   The xmlfile-parameter can either be relative to the game's base\n   directory or an absolute path (for pre-Alpha 17 prefabs).\n2. Unload the data loaded with loadxml.\n3. Create a block mapping file for the given prefab(s). Accepts '*' as\n   wildcard (for pre-Alpha 17 prefabs).\n4. Load a block name mapping file. File path is relative to the game\n   directory if not specified as absolute path. If no file is given the\n   default file supplied with the game is loaded (BlockUpdates.csv). \n5. Unload the data loaded with loadtable.\n6. Update the block mappings in prefabs with the block name mapping table loaded by 4.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires parameters");
		}
		else if (_params[0].EqualsCaseInsensitive("loadxml"))
		{
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("'loadxml' requires 1 argument");
				return;
			}
			if (!SdFile.Exists(_params[1]))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specified XML file does not exist");
				return;
			}
			loadLegacyBlocksXml(_params[1]);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loaded block XML data from " + _params[1]);
		}
		else if (_params[0].EqualsCaseInsensitive("clearxml"))
		{
			idMapping = new NameIdMapping(null, Block.MAX_BLOCKS);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cleared block XML data");
		}
		else if (_params[0].EqualsCaseInsensitive("createmapping"))
		{
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("'createmapping' requires 1 argument");
				return;
			}
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			int num = 0;
			foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
			{
				if (createMapping(availablePaths))
				{
					num++;
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Creating " + num + " block mappings took " + microStopwatch.ElapsedMilliseconds + " ms");
		}
		else if (_params[0].EqualsCaseInsensitive("loadtable"))
		{
			string text = ((_params.Count >= 2) ? _params[1] : (GameIO.GetGameDir("Data/Config") + "/BlockUpdates.csv"));
			if (!SdFile.Exists(text))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specified file '" + text + "' does not exist");
				return;
			}
			loadMappingTable(text);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loaded block update table from " + text);
		}
		else if (_params[0].EqualsCaseInsensitive("unloadtables"))
		{
			mappingTable.Clear();
		}
		else if (_params[0].EqualsCaseInsensitive("updateblocks"))
		{
			updateMappings();
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown subcommand '" + _params[0] + "'");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createMapping(PathAbstractions.AbstractedLocation _location)
	{
		string text = _location.FullPathNoExtension + ".blocks.nim";
		if (SdFile.Exists(text))
		{
			Log.Warning("Mapping for " + _location.Name + " already exists, skipping");
			return false;
		}
		Log.Out("Creating block mapping for " + _location.Name);
		Prefab prefab = new Prefab();
		if (!prefab.Load(_location, _applyMapping: false, _fixChildblocks: false))
		{
			Log.Error("Failed loading prefab '" + _location.Name + "'");
			return false;
		}
		NameIdMapping nameIdMapping = new NameIdMapping(text, Block.MAX_BLOCKS);
		int blockCount = prefab.GetBlockCount();
		for (int i = 0; i < blockCount; i++)
		{
			BlockValue blockNoDamage = prefab.GetBlockNoDamage(i);
			string nameForId = idMapping.GetNameForId(blockNoDamage.type);
			if (nameForId == null)
			{
				Log.Error("Creating block mapping for prefab failed: Block " + blockNoDamage.type + " used in prefab not found in loaded XMLs.");
				return false;
			}
			nameIdMapping.AddMapping(blockNoDamage.type, nameForId);
		}
		nameIdMapping.WriteToFile();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadLegacyBlocksXml(string _filename)
	{
		try
		{
			if (!SdFile.Exists(_filename))
			{
				Log.Error("Specified XML file does not exist (" + _filename + ")");
				return;
			}
			XmlDocument obj = new XmlDocument
			{
				XmlResolver = null
			};
			obj.SdLoad(_filename);
			XmlElement documentElement = obj.DocumentElement;
			if (documentElement == null || documentElement.ChildNodes.Count == 0)
			{
				throw new Exception("No element <blocks> found!");
			}
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Element && childNode.Name.Equals("block"))
				{
					XmlElement obj2 = (XmlElement)childNode;
					string attribute = obj2.GetAttribute("name");
					int id = int.Parse(obj2.GetAttribute("id"));
					idMapping.AddMapping(id, attribute, _force: true);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Loading and parsing '" + _filename + "' (" + ex.Message + ")");
			Log.Error("Loading of legacy blocks.xml aborted due to errors!");
			Log.Error(ex.StackTrace);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadMappingTable(string _file)
	{
		ByteReader byteReader = new ByteReader(SdFile.ReadAllBytes(_file));
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Block update table file does not match the expected format.");
			return;
		}
		if (betterList.buffer[0].IndexOf("old", StringComparison.OrdinalIgnoreCase) < 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid block update table file. The first column header is expected to contain 'old'.");
			return;
		}
		int num = 0;
		BetterList<string> betterList2;
		while ((betterList2 = byteReader.ReadCSV()) != null)
		{
			if (betterList2.size >= 4)
			{
				string text = betterList2.buffer[0];
				string text2 = betterList2.buffer[1];
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
				{
					mappingTable.Add((text, text2));
					num++;
				}
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Loaded {num} block mappings.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMappings()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		int num = 0;
		int num2 = 0;
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
		{
			string text = availablePaths.FullPathNoExtension + ".blocks.nim";
			if (!SdFile.Exists(text))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loading block mapping file for prefab \"" + availablePaths.Name + "\" failed: Block name to ID mapping file missing.");
				continue;
			}
			using (NameIdMapping nameIdMapping = MemoryPools.poolNameIdMapping.AllocSync(_bReset: true))
			{
				nameIdMapping.InitMapping(text, Block.MAX_BLOCKS);
				if (!nameIdMapping.LoadFromFile())
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loading block mapping file for prefab \"" + availablePaths.Name + "\" failed.");
					continue;
				}
				int num3 = nameIdMapping.ReplaceNames(mappingTable);
				if (num3 > 0)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Updated {num3} names for prefab \"{availablePaths.Name}\"");
				}
				nameIdMapping.SaveIfDirty(_async: false);
				num2 += num3;
			}
			num++;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Updating {num} block mappings took {microStopwatch.ElapsedMilliseconds} ms. Replaced a total of {num2} entries.");
	}
}
