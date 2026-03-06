using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

public class AdminBlacklist(AdminTools _parent) : AdminSectionAbs(_parent, "blacklist")
{
	public readonly struct BannedUser(string _name, PlatformUserIdentifierAbs _userIdentifier, DateTime _banUntil, string _banReason)
	{
		public readonly string Name = _name;

		public readonly PlatformUserIdentifierAbs UserIdentifier = _userIdentifier;

		public readonly DateTime BannedUntil = _banUntil;

		public readonly string BanReason = _banReason ?? string.Empty;

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("blacklisted");
			UserIdentifier.ToXml(xmlElement);
			if (Name != null)
			{
				xmlElement.SetAttrib("name", Name);
			}
			xmlElement.SetAttrib("unbandate", BannedUntil.ToCultureInvariantString());
			xmlElement.SetAttrib("reason", BanReason);
		}

		public static bool TryParse(XmlElement _element, out BannedUser _result)
		{
			_result = default(BannedUser);
			string text = _element.GetAttribute("name");
			if (text.Length == 0)
			{
				text = null;
			}
			if (!_element.HasAttribute("unbandate"))
			{
				Log.Warning("Ignoring blacklist-entry because of missing 'unbandate' attribute: " + _element.OuterXml);
				return false;
			}
			if (!StringParsers.TryParseDateTime(_element.GetAttribute("unbandate"), out var _result2) && !DateTime.TryParse(_element.GetAttribute("unbandate"), out _result2))
			{
				Log.Warning("Ignoring blacklist-entry because of invalid value for 'unbandate' attribute: " + _element.OuterXml);
				return false;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = AdminTools.ParseUserIdentifier(_element);
			if (platformUserIdentifierAbs == null)
			{
				return false;
			}
			string attribute = _element.GetAttribute("reason");
			_result = new BannedUser(text, platformUserIdentifierAbs, _result2, attribute);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, BannedUser> bannedUsers = new Dictionary<PlatformUserIdentifierAbs, BannedUser>();

	public override void Clear()
	{
		bannedUsers.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		if (BannedUser.TryParse(_childElement, out var _result))
		{
			bannedUsers[_result.UserIdentifier] = _result;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement("blacklist");
		xmlElement.AddXmlComment(" <blacklisted platform=\"\" userid=\"\" name=\"\" unbandate=\"\" reason=\"\" /> ");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, BannedUser> bannedUser in bannedUsers)
		{
			bannedUser.Value.ToXml(xmlElement);
		}
	}

	public void AddBan(string _name, PlatformUserIdentifierAbs _identifier, DateTime _banUntil, string _banReason)
	{
		lock (Parent)
		{
			BannedUser value = new BannedUser(_name, _identifier, _banUntil, _banReason);
			bannedUsers[_identifier] = value;
			if (_banUntil > DateTime.Now)
			{
				Parent.Users.RemoveUser(_identifier, _save: false);
			}
			Parent.Save();
		}
	}

	public bool RemoveBan(PlatformUserIdentifierAbs _identifier)
	{
		lock (Parent)
		{
			bool num = bannedUsers.Remove(_identifier);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public bool IsBanned(PlatformUserIdentifierAbs _identifier, out DateTime _bannedUntil, out string _reason)
	{
		lock (Parent)
		{
			if (bannedUsers.ContainsKey(_identifier))
			{
				BannedUser bannedUser = bannedUsers[_identifier];
				if (bannedUser.BannedUntil > DateTime.Now)
				{
					_bannedUntil = bannedUser.BannedUntil;
					_reason = bannedUser.BanReason;
					return true;
				}
			}
			_bannedUntil = DateTime.Now;
			_reason = string.Empty;
			return false;
		}
	}

	public List<BannedUser> GetBanned()
	{
		lock (Parent)
		{
			return bannedUsers.Values.Where([PublicizedFrom(EAccessModifier.Internal)] (BannedUser _b) => _b.BannedUntil > DateTime.Now).ToList();
		}
	}
}
