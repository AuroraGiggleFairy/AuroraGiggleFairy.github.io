using System.Collections.Generic;
using System.Xml;

namespace Webserver.Permissions;

public class AdminApiTokens : AdminSectionAbs
{
	public readonly struct ApiToken(string _name, string _secret, int _permissionLevel)
	{
		public readonly string Name = _name;

		public readonly string Secret = _secret;

		public readonly int PermissionLevel = _permissionLevel;

		public void ToXml(XmlElement _parent)
		{
			XmlElement element = _parent.AddXmlElement("token").SetAttrib("name", Name).SetAttrib("secret", Secret);
			int permissionLevel = PermissionLevel;
			element.SetAttrib("permission_level", permissionLevel.ToString());
		}

		public static bool TryParse(XmlElement _element, out ApiToken _result)
		{
			_result = default(ApiToken);
			if (!_element.TryGetAttribute("name", out var _result2))
			{
				Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'name' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.TryGetAttribute("secret", out var _result3))
			{
				Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'secret' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.TryGetAttribute("permission_level", out var _result4))
			{
				Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_result4, out var result))
			{
				Log.Warning("[Web] [Perms] Ignoring apitoken-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			_result = new ApiToken(_result2, _result3, result);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, ApiToken> tokens = new CaseInsensitiveStringDictionary<ApiToken>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool commandlineChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandlineTokenName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandlineTokenSecret;

	public static AdminApiTokens Instance => GameManager.Instance.adminTools.ApiTokens;

	public AdminApiTokens(AdminTools _parent)
		: base(_parent, "apitokens")
	{
	}

	public override void Clear()
	{
		tokens.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		if (ApiToken.TryParse(_childElement, out var _result))
		{
			tokens[_result.Name] = _result;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(SectionTypeName);
		xmlElement.AddXmlComment(" <token name=\"adminuser1\" secret=\"supersecrettoken\" permission_level=\"0\" /> ");
		foreach (var (_, apiToken2) in tokens)
		{
			apiToken2.ToXml(xmlElement);
		}
	}

	public void AddToken(string _name, string _secret, int _permissionLevel)
	{
		lock (Parent)
		{
			ApiToken value = new ApiToken(_name, _secret, _permissionLevel);
			tokens[_name] = value;
			Parent.Save();
		}
	}

	public bool RemoveToken(string _name)
	{
		lock (Parent)
		{
			bool num = tokens.Remove(_name);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public Dictionary<string, ApiToken> GetTokens()
	{
		lock (Parent)
		{
			return tokens;
		}
	}

	public int GetPermissionLevel(string _name, string _secret)
	{
		lock (Parent)
		{
			if (tokens.TryGetValue(_name, out var value) && value.Secret == _secret)
			{
				return value.PermissionLevel;
			}
			if (IsCommandlineToken(_name, _secret))
			{
				return 0;
			}
			return int.MaxValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCommandlineToken(string _name, string _secret)
	{
		if (!commandlineChecked)
		{
			commandlineTokenName = GameUtils.GetLaunchArgument("webapitokenname");
			commandlineTokenSecret = GameUtils.GetLaunchArgument("webapitokensecret");
			commandlineChecked = true;
		}
		if (string.IsNullOrEmpty(commandlineTokenName) || string.IsNullOrEmpty(commandlineTokenSecret))
		{
			return false;
		}
		if (_name == commandlineTokenName)
		{
			return _secret == commandlineTokenSecret;
		}
		return false;
	}
}
