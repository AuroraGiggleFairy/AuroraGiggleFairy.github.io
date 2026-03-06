using System;
using System.Collections.Generic;
using System.Xml;

public class AdminUsers(AdminTools _parent) : AdminSectionAbs(_parent, "users")
{
	public readonly struct UserPermission(string _name, PlatformUserIdentifierAbs _userIdentifier, int _permissionLevel)
	{
		public readonly string Name = _name;

		public readonly PlatformUserIdentifierAbs UserIdentifier = _userIdentifier;

		public readonly int PermissionLevel = _permissionLevel;

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("user");
			UserIdentifier.ToXml(xmlElement);
			if (Name != null)
			{
				xmlElement.SetAttrib("name", Name);
			}
			int permissionLevel = PermissionLevel;
			xmlElement.SetAttrib("permission_level", permissionLevel.ToString());
		}

		public static bool TryParse(XmlElement _element, out UserPermission _result)
		{
			_result = default(UserPermission);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("permission_level"))
			{
				Log.Warning("Ignoring admin-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_element.GetAttribute("permission_level"), out var result))
			{
				Log.Warning("Ignoring admin-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = AdminTools.ParseUserIdentifier(_element);
			if (platformUserIdentifierAbs == null)
			{
				return false;
			}
			_result = new UserPermission(text, platformUserIdentifierAbs, result);
			return true;
		}
	}

	public readonly struct GroupPermission(string _name, string _steamIdGroup, int _permissionLevelNormal, int _permissionLevelMods)
	{
		public readonly string Name = _name;

		public readonly string SteamIdGroup = _steamIdGroup;

		public readonly int PermissionLevelNormal = _permissionLevelNormal;

		public readonly int PermissionLevelMods = _permissionLevelMods;

		public void ToXml(XmlElement _parent)
		{
			XmlElement element = _parent.AddXmlElement("group");
			element.SetAttrib("steamID", SteamIdGroup);
			if (Name != null)
			{
				element.SetAttrib("name", Name);
			}
			int permissionLevelNormal = PermissionLevelNormal;
			element.SetAttrib("permission_level_default", permissionLevelNormal.ToString());
			permissionLevelNormal = PermissionLevelMods;
			element.SetAttrib("permission_level_mod", permissionLevelNormal.ToString());
		}

		public static bool TryParse(XmlElement _element, out GroupPermission _result)
		{
			_result = default(GroupPermission);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("steamID"))
			{
				Log.Warning("Ignoring admin-entry because of missing 'steamID' attribute: " + _element.OuterXml);
				return false;
			}
			string attribute = _element.GetAttribute("steamID");
			if (!_element.HasAttribute("permission_level_default"))
			{
				Log.Warning("Ignoring admin-entry because of missing 'permission_level_default' attribute on group: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_element.GetAttribute("permission_level_default"), out var result))
			{
				Log.Warning("Ignoring admin-entry because of invalid (non-numeric) value for 'permission_level_default' attribute on group: " + _element.OuterXml);
				return false;
			}
			if (!_element.HasAttribute("permission_level_mod"))
			{
				Log.Warning("Ignoring admin-entry because of missing 'permission_level_mod' attribute on group: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_element.GetAttribute("permission_level_mod"), out var result2))
			{
				Log.Warning("Ignoring admin-entry because of invalid (non-numeric) value for 'permission_level_mod' attribute on group: " + _element.OuterXml);
				return false;
			}
			_result = new GroupPermission(text, attribute, result, result2);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, UserPermission> userPermissions = new Dictionary<PlatformUserIdentifierAbs, UserPermission>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, GroupPermission> groupPermissions = new Dictionary<string, GroupPermission>();

	public override void Clear()
	{
		userPermissions.Clear();
		groupPermissions.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		UserPermission _result2;
		if (_childElement.Name == "group")
		{
			if (GroupPermission.TryParse(_childElement, out var _result))
			{
				groupPermissions[_result.SteamIdGroup] = _result;
			}
		}
		else if (UserPermission.TryParse(_childElement, out _result2))
		{
			userPermissions[_result2.UserIdentifier] = _result2;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(SectionTypeName);
		xmlElement.AddXmlComment(" <user platform=\"Steam\" userid=\"76561198021925107\" name=\"Hint on who this user is\" permission_level=\"0\" /> ");
		xmlElement.AddXmlComment(" <group steamID=\"103582791434672565\" name=\"Steam Universe\" permission_level_default=\"1000\" permission_level_mod=\"0\" /> ");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, UserPermission> userPermission in userPermissions)
		{
			userPermission.Value.ToXml(xmlElement);
		}
		foreach (KeyValuePair<string, GroupPermission> groupPermission in groupPermissions)
		{
			groupPermission.Value.ToXml(xmlElement);
		}
	}

	public void AddUser(string _name, PlatformUserIdentifierAbs _identifier, int _permissionLevel)
	{
		lock (Parent)
		{
			UserPermission value = new UserPermission(_name, _identifier, _permissionLevel);
			userPermissions[_identifier] = value;
			Parent.Save();
		}
	}

	public bool RemoveUser(PlatformUserIdentifierAbs _identifier, bool _save = true)
	{
		lock (Parent)
		{
			bool num = userPermissions.Remove(_identifier);
			if (num && _save)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public bool HasEntry(ClientInfo _clientInfo)
	{
		lock (Parent)
		{
			return userPermissions.ContainsKey(_clientInfo.PlatformId) || userPermissions.ContainsKey(_clientInfo.CrossplatformId);
		}
	}

	public Dictionary<PlatformUserIdentifierAbs, UserPermission> GetUsers()
	{
		lock (Parent)
		{
			return userPermissions;
		}
	}

	public void AddGroup(string _name, string _steamId, int _permissionLevelDefault, int _permissionLevelMod)
	{
		lock (Parent)
		{
			GroupPermission value = new GroupPermission(_name, _steamId, _permissionLevelDefault, _permissionLevelMod);
			groupPermissions[_steamId] = value;
			Parent.Save();
		}
	}

	public bool RemoveGroup(string _steamId)
	{
		lock (Parent)
		{
			bool num = groupPermissions.Remove(_steamId);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public Dictionary<string, GroupPermission> GetGroups()
	{
		lock (Parent)
		{
			return groupPermissions;
		}
	}

	public int GetUserPermissionLevel(PlatformUserIdentifierAbs _userId)
	{
		lock (Parent)
		{
			if (userPermissions.TryGetValue(_userId, out var value))
			{
				return value.PermissionLevel;
			}
			return 1000;
		}
	}

	public int GetUserPermissionLevel(ClientInfo _clientInfo)
	{
		lock (Parent)
		{
			int num = 1000;
			if (userPermissions.TryGetValue(_clientInfo.PlatformId, out var value))
			{
				num = value.PermissionLevel;
			}
			if (_clientInfo.CrossplatformId != null && userPermissions.TryGetValue(_clientInfo.CrossplatformId, out var value2))
			{
				num = Math.Min(num, value2.PermissionLevel);
			}
			if (groupPermissions.Count > 0 && _clientInfo.groupMemberships.Count > 0)
			{
				int num2 = int.MaxValue;
				foreach (KeyValuePair<string, int> groupMembership in _clientInfo.groupMemberships)
				{
					if (groupPermissions.TryGetValue(groupMembership.Key, out var value3))
					{
						num2 = Math.Min(num2, (groupMembership.Value == 2) ? value3.PermissionLevelMods : value3.PermissionLevelNormal);
					}
				}
				num = Math.Min(num, num2);
			}
			return num;
		}
	}
}
