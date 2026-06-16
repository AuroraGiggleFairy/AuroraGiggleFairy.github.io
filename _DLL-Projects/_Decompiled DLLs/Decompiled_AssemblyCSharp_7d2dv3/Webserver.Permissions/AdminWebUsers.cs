using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Webserver.Permissions;

public class AdminWebUsers : AdminSectionAbs
{
	public readonly struct WebUser
	{
		public readonly string Name;

		public readonly byte[] PasswordHash;

		public readonly PlatformUserIdentifierAbs PlatformUser;

		public readonly PlatformUserIdentifierAbs CrossPlatformUser;

		public WebUser(string _name, byte[] _passwordHash, PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser)
		{
			Name = _name;
			PasswordHash = _passwordHash;
			PlatformUser = _platformUser;
			CrossPlatformUser = _crossPlatformUser;
		}

		public WebUser(string _name, string _password, PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser)
		{
			Name = _name;
			PasswordHash = Hash(_password);
			PlatformUser = _platformUser;
			CrossPlatformUser = _crossPlatformUser;
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("user");
			xmlElement.SetAttrib("name", Name).SetAttrib("pass", Convert.ToBase64String(PasswordHash));
			PlatformUser.ToXml(xmlElement);
			CrossPlatformUser?.ToXml(xmlElement, "cross");
		}

		public static bool TryParse(XmlElement _element, out WebUser _result)
		{
			_result = default(WebUser);
			if (!_element.TryGetAttribute("name", out var _result2))
			{
				Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'name' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.TryGetAttribute("pass", out var _result3))
			{
				Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'pass' attribute: " + _element.OuterXml);
				return false;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(_element, _warnings: false);
			if (platformUserIdentifierAbs == null)
			{
				Log.Warning("[Web] [Perms] Ignoring user-entry because of missing 'platform' or 'userid' attribute: " + _element.OuterXml);
				return false;
			}
			PlatformUserIdentifierAbs crossPlatformUser = PlatformUserIdentifierAbs.FromXml(_element, _warnings: false, "cross");
			byte[] passwordHash = Convert.FromBase64String(_result3);
			_result = new WebUser(_result2, passwordHash, platformUserIdentifierAbs, crossPlatformUser);
			return true;
		}

		public bool ValidatePassword(string _password)
		{
			return Utils.ArrayEquals(Hash(_password), PasswordHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, WebUser> users = new CaseInsensitiveStringDictionary<WebUser>();

	public static AdminWebUsers Instance => GameManager.Instance.adminTools.WebUsers;

	public AdminWebUsers(AdminTools _parent)
		: base(_parent, "webusers")
	{
	}

	public override void Clear()
	{
		users.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		if (WebUser.TryParse(_childElement, out var _result))
		{
			users[_result.Name] = _result;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement parent = _root.AddXmlElement(SectionTypeName);
		foreach (var (_, webUser2) in users)
		{
			webUser2.ToXml(parent);
		}
	}

	public void AddUser(string _name, string _password, PlatformUserIdentifierAbs _userIdentifier, PlatformUserIdentifierAbs _crossPlatformIdentifier)
	{
		lock (Parent)
		{
			WebUser value = new WebUser(_name, _password, _userIdentifier, _crossPlatformIdentifier);
			users[_name] = value;
			Parent.Save();
		}
	}

	public bool RemoveUser(string _name)
	{
		lock (Parent)
		{
			bool num = users.Remove(_name);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public Dictionary<string, WebUser> GetUsers()
	{
		lock (Parent)
		{
			return users;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] Hash(string _input)
	{
		return new MD5Cng().ComputeHash(Encoding.UTF8.GetBytes(_input));
	}

	public bool TryGetUser(string _name, string _password, out WebUser _result)
	{
		lock (Parent)
		{
			if (users.TryGetValue(_name, out _result) && _result.ValidatePassword(_password))
			{
				return true;
			}
			_result = default(WebUser);
			return false;
		}
	}

	public bool HasUser(PlatformUserIdentifierAbs _platformUser, PlatformUserIdentifierAbs _crossPlatformUser, out WebUser _result)
	{
		lock (Parent)
		{
			_result = default(WebUser);
			foreach (var (_, webUser2) in users)
			{
				if (object.Equals(webUser2.PlatformUser, _platformUser) && object.Equals(webUser2.CrossPlatformUser, _crossPlatformUser))
				{
					_result = webUser2;
					return true;
				}
			}
			return false;
		}
	}
}
