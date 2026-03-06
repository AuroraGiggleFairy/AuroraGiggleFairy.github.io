using System.Collections.Generic;
using System.Xml;

public class AdminCommands(AdminTools _parent) : AdminSectionAbs(_parent, "commands")
{
	public readonly struct CommandPermission(string _cmd, int _permissionLevel)
	{
		public readonly string Command = _cmd;

		public readonly int PermissionLevel = _permissionLevel;

		public void ToXml(XmlElement _parent)
		{
			XmlElement element = _parent.AddXmlElement("permission").SetAttrib("cmd", Command);
			int permissionLevel = PermissionLevel;
			element.SetAttrib("permission_level", permissionLevel.ToString());
		}

		public static bool TryParse(XmlElement _element, out CommandPermission _result)
		{
			_result = default(CommandPermission);
			string attribute = _element.GetAttribute("cmd");
			if (string.IsNullOrEmpty(attribute))
			{
				Log.Warning("Ignoring permission-entry because of missing or empty 'cmd' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.HasAttribute("permission_level"))
			{
				Log.Warning("Ignoring permission-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_element.GetAttribute("permission_level"), out var result))
			{
				Log.Warning("Ignoring permission-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			_result = new CommandPermission(attribute, result);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, CommandPermission> commands = new Dictionary<string, CommandPermission>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CommandPermission defaultCommandPermission = new CommandPermission("", 0);

	public override void Clear()
	{
		commands.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		if (CommandPermission.TryParse(_childElement, out var _result))
		{
			commands[_result.Command] = _result;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(SectionTypeName);
		xmlElement.AddXmlComment(" <permission cmd=\"dm\" permission_level=\"0\" /> ");
		xmlElement.AddXmlComment(" <permission cmd=\"kick\" permission_level=\"1\" /> ");
		xmlElement.AddXmlComment(" <permission cmd=\"say\" permission_level=\"1000\" /> ");
		foreach (KeyValuePair<string, CommandPermission> command in commands)
		{
			command.Value.ToXml(xmlElement);
		}
	}

	public void AddCommand(string _cmd, int _permissionLevel, bool _save = true)
	{
		lock (Parent)
		{
			CommandPermission value = new CommandPermission(_cmd, _permissionLevel);
			commands[_cmd] = value;
			if (_save)
			{
				Parent.Save();
			}
		}
	}

	public bool RemoveCommand(string[] _cmds)
	{
		lock (Parent)
		{
			bool flag = false;
			foreach (string key in _cmds)
			{
				flag |= commands.Remove(key);
			}
			if (flag)
			{
				Parent.Save();
			}
			return flag;
		}
	}

	public bool IsPermissionDefined(string[] _cmds)
	{
		lock (Parent)
		{
			foreach (string key in _cmds)
			{
				if (commands.ContainsKey(key))
				{
					return true;
				}
			}
			return false;
		}
	}

	public Dictionary<string, CommandPermission> GetCommands()
	{
		lock (Parent)
		{
			return commands;
		}
	}

	public int GetCommandPermissionLevel(string[] _cmdNames)
	{
		lock (Parent)
		{
			return GetAdminToolsCommandPermission(_cmdNames).PermissionLevel;
		}
	}

	public CommandPermission GetAdminToolsCommandPermission(string[] _cmdNames)
	{
		lock (Parent)
		{
			foreach (string text in _cmdNames)
			{
				if (!string.IsNullOrEmpty(text) && commands.ContainsKey(text))
				{
					return commands[text];
				}
			}
			return defaultCommandPermission;
		}
	}
}
