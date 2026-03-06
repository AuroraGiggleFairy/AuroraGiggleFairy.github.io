using System.Collections.Generic;
using System.Xml;

public class AdminWhitelist(AdminTools _parent) : AdminSectionAbs(_parent, "whitelist")
{
	public readonly struct WhitelistUser(string _name, PlatformUserIdentifierAbs _userIdentifier)
	{
		public readonly string Name = _name;

		public readonly PlatformUserIdentifierAbs UserIdentifier = _userIdentifier;

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("user");
			UserIdentifier.ToXml(xmlElement);
			if (Name != null)
			{
				xmlElement.SetAttrib("name", Name);
			}
		}

		public static bool TryParse(XmlElement _element, out WhitelistUser _result)
		{
			_result = default(WhitelistUser);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = AdminTools.ParseUserIdentifier(_element);
			if (platformUserIdentifierAbs == null)
			{
				return false;
			}
			_result = new WhitelistUser(text, platformUserIdentifierAbs);
			return true;
		}
	}

	public readonly struct WhitelistGroup(string _name, string _steamIdGroup)
	{
		public readonly string Name = _name;

		public readonly string SteamIdGroup = _steamIdGroup;

		public void ToXml(XmlElement _parent)
		{
			XmlElement element = _parent.AddXmlElement("group");
			element.SetAttrib("steamID", SteamIdGroup);
			if (Name != null)
			{
				element.SetAttrib("name", Name);
			}
		}

		public static bool TryParse(XmlElement _element, out WhitelistGroup _result)
		{
			_result = default(WhitelistGroup);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("steamID"))
			{
				Log.Warning("Ignoring whitelist-entry because of missing 'steamID' attribute: " + _element.OuterXml);
				return false;
			}
			string attribute = _element.GetAttribute("steamID");
			_result = new WhitelistGroup(text, attribute);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, WhitelistUser> whitelistedUsers = new Dictionary<PlatformUserIdentifierAbs, WhitelistUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, WhitelistGroup> whitelistedGroups = new Dictionary<string, WhitelistGroup>();

	public override void Clear()
	{
		whitelistedUsers.Clear();
		whitelistedGroups.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		WhitelistUser _result2;
		if (_childElement.Name == "group")
		{
			if (WhitelistGroup.TryParse(_childElement, out var _result))
			{
				whitelistedGroups[_result.SteamIdGroup] = _result;
			}
		}
		else if (WhitelistUser.TryParse(_childElement, out _result2))
		{
			whitelistedUsers[_result2.UserIdentifier] = _result2;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(SectionTypeName);
		xmlElement.AddXmlComment(" ONLY PUT ITEMS IN WHITELIST IF YOU WANT WHITELIST ONLY ENABLED!!! ");
		xmlElement.AddXmlComment(" If there are any items in the whitelist, the whitelist only mode is enabled ");
		xmlElement.AddXmlComment(" Nobody can join that ISN'T in the whitelist or admins once whitelist only mode is enabled ");
		xmlElement.AddXmlComment(" Name is optional for display purposes only ");
		xmlElement.AddXmlComment(" <user platform=\"\" userid=\"\" name=\"\" /> ");
		xmlElement.AddXmlComment(" <group steamID=\"\" name=\"\" /> ");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, WhitelistUser> whitelistedUser in whitelistedUsers)
		{
			whitelistedUser.Value.ToXml(xmlElement);
		}
		foreach (KeyValuePair<string, WhitelistGroup> whitelistedGroup in whitelistedGroups)
		{
			whitelistedGroup.Value.ToXml(xmlElement);
		}
	}

	public void AddUser(string _name, PlatformUserIdentifierAbs _identifier)
	{
		lock (Parent)
		{
			WhitelistUser value = new WhitelistUser(_name, _identifier);
			whitelistedUsers[_identifier] = value;
			Parent.Save();
		}
	}

	public bool RemoveUser(PlatformUserIdentifierAbs _identifier)
	{
		lock (Parent)
		{
			bool num = whitelistedUsers.Remove(_identifier);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public Dictionary<PlatformUserIdentifierAbs, WhitelistUser> GetUsers()
	{
		lock (Parent)
		{
			return whitelistedUsers;
		}
	}

	public void AddGroup(string _name, string _steamId)
	{
		lock (Parent)
		{
			WhitelistGroup value = new WhitelistGroup(_name, _steamId);
			whitelistedGroups[_steamId] = value;
			Parent.Save();
		}
	}

	public bool RemoveGroup(string _steamId)
	{
		lock (Parent)
		{
			bool num = whitelistedGroups.Remove(_steamId);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public Dictionary<string, WhitelistGroup> GetGroups()
	{
		lock (Parent)
		{
			return whitelistedGroups;
		}
	}

	public bool IsWhitelisted(ClientInfo _clientInfo)
	{
		lock (Parent)
		{
			if (whitelistedUsers.ContainsKey(_clientInfo.PlatformId) || whitelistedUsers.ContainsKey(_clientInfo.CrossplatformId))
			{
				return true;
			}
			foreach (KeyValuePair<string, int> groupMembership in _clientInfo.groupMemberships)
			{
				if (whitelistedGroups.ContainsKey(groupMembership.Key))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsWhiteListEnabled()
	{
		lock (Parent)
		{
			return whitelistedUsers.Count > 0 || whitelistedGroups.Count > 0;
		}
	}
}
