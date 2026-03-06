using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using Force.Crc32;
using Platform.Steam;

public class AdminTools
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string XmlHeader = "\r\n\tThis file holds the settings for who is banned, whitelisted, admins and server command permissions. The\r\n\tadmin and whitelist sections can contain both individual Steam users as well as Steam groups.\r\n\r\n\tIt is recommended to modify this file only through the respective console commands, like \"admin\", or\r\n\tthe Web Dashboard.\r\n\r\n\r\n\tUSER ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tAny user entry uses two elements to identify whom it applies to.\r\n\t- platform: Identifier of the platform the User ID belongs to, i.e. \"EOS\", \"Steam\", \"XBL\", \"PSN\"\r\n\t- userid: The actual ID of the user on that platform. Examples:\r\n\t  - EOS: \"0002604bc42244e099c1bf05145fb71f\"\r\n\t  - Steam: SteamID64, e.g. \"76561198021925107\", see below\r\n\tYou can look up the IDs in the logs, e.g. whenever a user logs in the ID is logged.\r\n\r\n\tSTEAM ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tYou can find the SteamID64 of any user with one of the following pages:\r\n\thttps://steamdb.info/calculator/, https://steamid.io/lookup, https://steamid.co/\r\n\thttps://steamid.co/ instructions:\r\n\tInput the player's name in the search field. example: Kinyajuu\r\n\tIf the name doesn't work, you can also use the url of their steam page.\r\n\tYou will want the STEAM64ID. example: 76561198021925107\r\n\r\n\tSTEAM GROUP ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tYou can find the SteamID64 of any group by taking its address and adding\r\n\t  /memberslistxml/?xml=1\r\n\tto the end. You will get the XML information of the group which should have an entry\r\n\tmemberList->groupID64.\r\n\tExample: The 'Steam Universe' group has the address\r\n\t  https://steamcommunity.com/groups/steamuniverse\r\n\tSo you point your browser to\r\n\t  https://steamcommunity.com/groups/steamuniverse/memberslistxml/?xml=1\r\n\tAnd see that the groupID64 is 103582791434672565.\r\n\r\n\tPERMISSION LEVEL INSTRUCTIONS:\r\n\t===============================================================\r\n\tpermission level : 0-1000, a user may run any command equal to or above their permission level.\r\n\tUsers not given a permission level in this file will have a default permission level of 1000!\r\n\r\n\tCOMMAND PERMISSIONS INSTRUCTIONS:\r\n\t===============================================================\r\n\tcmd : This is the command name, any command not in this list will not be usable by anyone but the server.\r\n\tpermission level : 0-1000, a user may run any command equal to or above their permission level.\r\n\tCommands not specified in this file will have a default permission level of 0!\r\n\r\n\tEVERYTHING BETWEEN <!- - and - -> IS COMMENTED OUT! THE ENTRIES BELOW ARE EXAMPLES THAT ARE NOT ACTIVE!!!\r\n";

	public readonly AdminUsers Users;

	public readonly AdminWhitelist Whitelist;

	public readonly AdminBlacklist Blacklist;

	public readonly AdminCommands Commands;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XmlElement> unknownSections = new List<XmlElement>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AdminSectionAbs> modules = new Dictionary<string, AdminSectionAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FileSystemWatcher fileWatcher;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint lastHash;

	public AdminTools()
	{
		Users = new AdminUsers(this);
		Whitelist = new AdminWhitelist(this);
		Blacklist = new AdminBlacklist(this);
		Commands = new AdminCommands(this);
		registerModules();
		SdDirectory.CreateDirectory(GetFilePath());
		InitFileWatcher();
		Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerModules()
	{
		modules.Add(Users.SectionTypeName, Users);
		modules.Add(Whitelist.SectionTypeName, Whitelist);
		modules.Add(Blacklist.SectionTypeName, Blacklist);
		modules.Add(Commands.SectionTypeName, Commands);
	}

	public bool CommandAllowedFor(string[] _cmdNames, ClientInfo _clientInfo)
	{
		return Commands.GetCommandPermissionLevel(_cmdNames) >= Users.GetUserPermissionLevel(_clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitFileWatcher()
	{
		fileWatcher = new FileSystemWatcher(GetFilePath(), GetFileName());
		fileWatcher.Changed += OnFileChanged;
		fileWatcher.Created += OnFileChanged;
		fileWatcher.Deleted += OnFileChanged;
		fileWatcher.EnableRaisingEvents = true;
	}

	public void DestroyFileWatcher()
	{
		if (fileWatcher != null)
		{
			fileWatcher.Dispose();
			fileWatcher = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFileChanged(object _source, FileSystemEventArgs _e)
	{
		Log.Out("Reloading serveradmin.xml");
		Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFilePath()
	{
		return GameIO.GetSaveGameRootDir();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFileName()
	{
		return GamePrefs.GetString(EnumGamePrefs.AdminFileName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFullPath()
	{
		return GetFilePath() + "/" + GetFileName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
		try
		{
			lock (this)
			{
				if (!SdFile.Exists(GetFullPath()))
				{
					Log.Out("Permissions file '" + GetFileName() + "' not found, creating.");
					Save();
					return;
				}
				Log.Out("Loading permissions file at '" + GetFullPath() + "'");
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					using Crc32Algorithm crc32Algorithm = new Crc32Algorithm();
					using Stream stream = SdFile.OpenRead(GetFullPath());
					using (CryptoStream inStream = new CryptoStream(stream, crc32Algorithm, CryptoStreamMode.Read))
					{
						xmlDocument.Load(inStream);
					}
					uint num = crc32Algorithm.HashUint();
					if (lastHash == num)
					{
						Log.Out("Permissions file unchanged, skipping reloading");
						return;
					}
					lastHash = num;
				}
				catch (XmlException ex)
				{
					Log.Error("Failed loading permissions file: " + ex.Message);
					return;
				}
				catch (IOException ex2)
				{
					Log.Error("Failed loading permissions file: " + ex2.Message);
					return;
				}
				if (xmlDocument.DocumentElement == null)
				{
					Log.Warning("Permissions file has no root XML element.");
					return;
				}
				unknownSections.Clear();
				foreach (KeyValuePair<string, AdminSectionAbs> module in modules)
				{
					module.Deconstruct(out var _, out var value);
					value.Clear();
				}
				foreach (XmlNode childNode2 in xmlDocument.DocumentElement.ChildNodes)
				{
					if (childNode2.NodeType != XmlNodeType.Comment)
					{
						if (childNode2.NodeType != XmlNodeType.Element)
						{
							Log.Warning("Unexpected top level XML node found: " + childNode2.OuterXml);
							continue;
						}
						XmlElement childNode = (XmlElement)childNode2;
						ParseSection(childNode);
					}
				}
			}
			Log.Out("Loading permissions file done.");
		}
		catch (Exception e)
		{
			Log.Error("Exception while trying to load serveradmins.xml:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseSection(XmlElement _childNode)
	{
		string name = _childNode.Name;
		string text = ((name == "admins") ? "users" : ((!(name == "permissions")) ? name : "commands"));
		name = text;
		if (!modules.TryGetValue(name, out var value))
		{
			Log.Warning("Ignoring unknown section in permissions file: " + name);
			unknownSections.Add(_childNode);
		}
		else
		{
			value.Parse(_childNode);
		}
	}

	public static PlatformUserIdentifierAbs ParseUserIdentifier(XmlElement _lineItem)
	{
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(_lineItem, _warnings: false);
		if (platformUserIdentifierAbs != null)
		{
			return platformUserIdentifierAbs;
		}
		if (_lineItem.HasAttribute("steamID"))
		{
			string attribute = _lineItem.GetAttribute("steamID");
			try
			{
				return new UserIdentifierSteam(attribute);
			}
			catch (ArgumentException)
			{
				Log.Warning("Ignoring entry because of invalid 'steamID' attribute value: " + _lineItem.OuterXml);
				return null;
			}
		}
		Log.Warning("Ignoring entry because of missing 'platform' or 'userid' attribute: " + _lineItem.OuterXml);
		return null;
	}

	public void Save()
	{
		try
		{
			lock (this)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.CreateXmlDeclaration();
				xmlDocument.AddXmlComment("\r\n\tThis file holds the settings for who is banned, whitelisted, admins and server command permissions. The\r\n\tadmin and whitelist sections can contain both individual Steam users as well as Steam groups.\r\n\r\n\tIt is recommended to modify this file only through the respective console commands, like \"admin\", or\r\n\tthe Web Dashboard.\r\n\r\n\r\n\tUSER ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tAny user entry uses two elements to identify whom it applies to.\r\n\t- platform: Identifier of the platform the User ID belongs to, i.e. \"EOS\", \"Steam\", \"XBL\", \"PSN\"\r\n\t- userid: The actual ID of the user on that platform. Examples:\r\n\t  - EOS: \"0002604bc42244e099c1bf05145fb71f\"\r\n\t  - Steam: SteamID64, e.g. \"76561198021925107\", see below\r\n\tYou can look up the IDs in the logs, e.g. whenever a user logs in the ID is logged.\r\n\r\n\tSTEAM ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tYou can find the SteamID64 of any user with one of the following pages:\r\n\thttps://steamdb.info/calculator/, https://steamid.io/lookup, https://steamid.co/\r\n\thttps://steamid.co/ instructions:\r\n\tInput the player's name in the search field. example: Kinyajuu\r\n\tIf the name doesn't work, you can also use the url of their steam page.\r\n\tYou will want the STEAM64ID. example: 76561198021925107\r\n\r\n\tSTEAM GROUP ID INSTRUCTIONS:\r\n\t===============================================================\r\n\tYou can find the SteamID64 of any group by taking its address and adding\r\n\t  /memberslistxml/?xml=1\r\n\tto the end. You will get the XML information of the group which should have an entry\r\n\tmemberList->groupID64.\r\n\tExample: The 'Steam Universe' group has the address\r\n\t  https://steamcommunity.com/groups/steamuniverse\r\n\tSo you point your browser to\r\n\t  https://steamcommunity.com/groups/steamuniverse/memberslistxml/?xml=1\r\n\tAnd see that the groupID64 is 103582791434672565.\r\n\r\n\tPERMISSION LEVEL INSTRUCTIONS:\r\n\t===============================================================\r\n\tpermission level : 0-1000, a user may run any command equal to or above their permission level.\r\n\tUsers not given a permission level in this file will have a default permission level of 1000!\r\n\r\n\tCOMMAND PERMISSIONS INSTRUCTIONS:\r\n\t===============================================================\r\n\tcmd : This is the command name, any command not in this list will not be usable by anyone but the server.\r\n\tpermission level : 0-1000, a user may run any command equal to or above their permission level.\r\n\tCommands not specified in this file will have a default permission level of 0!\r\n\r\n\tEVERYTHING BETWEEN <!- - and - -> IS COMMENTED OUT! THE ENTRIES BELOW ARE EXAMPLES THAT ARE NOT ACTIVE!!!\r\n");
				XmlElement xmlElement = xmlDocument.AddXmlElement("adminTools");
				xmlElement.AddXmlComment(" Name in any entries is optional for display purposes only ");
				WriteSections(xmlElement);
				for (int i = 0; i < unknownSections.Count; i++)
				{
					XmlElement node = unknownSections[i];
					XmlNode newChild = xmlDocument.ImportNode(node, deep: true);
					xmlElement.AppendChild(newChild);
				}
				fileWatcher.EnableRaisingEvents = false;
				using Crc32Algorithm crc32Algorithm = new Crc32Algorithm();
				using Stream stream = SdFile.Open(GetFullPath(), FileMode.Create, FileAccess.Write, FileShare.Read);
				using (CryptoStream outStream = new CryptoStream(stream, crc32Algorithm, CryptoStreamMode.Write))
				{
					xmlDocument.Save(outStream);
				}
				lastHash = crc32Algorithm.HashUint();
				fileWatcher.EnableRaisingEvents = true;
			}
		}
		catch (Exception e)
		{
			Log.Error("Exception while trying to save serveradmins.xml:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteSections(XmlElement _root)
	{
		foreach (KeyValuePair<string, AdminSectionAbs> module in modules)
		{
			module.Deconstruct(out var _, out var value);
			value.Save(_root);
		}
	}
}
