using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NCalc;

public static class XmlPatchConditionEvaluator
{
	public static XElement FindActiveConditionalBranchElement(XmlFile _xmlFile, XElement _conditionalElement)
	{
		foreach (XElement item in _conditionalElement.Elements())
		{
			string localName = item.Name.LocalName;
			if (localName.EqualsCaseInsensitive("if"))
			{
				string value = (item.Attribute("cond") ?? throw new XmlPatchException(item, "Conditional", "Patch child 'if'-element does not have an 'cond' attribute")).Value;
				if (Evaluate(_xmlFile, item, value))
				{
					return item;
				}
				continue;
			}
			if (localName.EqualsCaseInsensitive("else"))
			{
				return item;
			}
			throw new XmlPatchException(item, "Conditional", "Unexpected child element '" + localName + "' in conditional patch block");
		}
		return null;
	}

	public static bool Evaluate(XmlFile _xmlFile, XElement _xmlElement, string _expression)
	{
		Expression expression = new Expression(_expression, EvaluateOptions.IgnoreCase | EvaluateOptions.NoCache | EvaluateOptions.UseDoubleForAbsFunction | EvaluateOptions.AllowNullParameter);
		expression.Parameters["xml"] = _xmlFile;
		expression.EvaluateFunction += NCalcEvaluateFunction;
		object obj;
		try
		{
			obj = expression.Evaluate();
		}
		catch (Exception innerException)
		{
			throw new XmlPatchException(_xmlElement, "Evaluate", "Error evaluating conditional expression: " + _expression, innerException);
		}
		if (obj is bool)
		{
			return (bool)obj;
		}
		throw new XmlPatchException(_xmlElement, "Evaluate", $"Conditional expression did not evaluate to a boolean value: {_expression} == {obj}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void NCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("mod_loaded"))
		{
			mod_loaded(_args);
		}
		else if (_name.EqualsCaseInsensitive("mod_version"))
		{
			mod_version(_args);
		}
		else if (_name.EqualsCaseInsensitive("game_version"))
		{
			game_version(_args);
		}
		else if (_name.EqualsCaseInsensitive("version"))
		{
			version(_args);
		}
		else if (_name.EqualsCaseInsensitive("serverinfo"))
		{
			serverinfo(_args);
		}
		else if (_name.EqualsCaseInsensitive("gamepref"))
		{
			gamepref(_args);
		}
		else if (_name.EqualsCaseInsensitive("event"))
		{
			eventActive(_args);
		}
		else if (_name.EqualsCaseInsensitive("time_minutes"))
		{
			time_minutes(_args);
		}
		else if (_name.EqualsCaseInsensitive("game_loaded"))
		{
			game_loaded(_args);
		}
		else if (_name.EqualsCaseInsensitive("xpath"))
		{
			xpath(_args);
		}
		else if (_name.EqualsCaseInsensitive("has_entitlement"))
		{
			has_entitlement(_args);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mod_loaded(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "mod_loaded", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function mod_loaded: Expected string argument");
		}
		_args.Result = ModManager.ModLoaded(text.Trim());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mod_version(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "mod_version", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function mod_version: Expected string argument");
		}
		_args.Result = ModManager.GetMod(text.Trim(), _onlyLoaded: true)?.Version ?? new Version(0, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void game_version(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "game_version", _args.Parameters.Length, 0));
		}
		_args.Result = Constants.cVersionInformation.Version;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void version(FunctionArgs _args)
	{
		if (_args.Parameters.Length < 2)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected at least {2})", "version", _args.Parameters.Length, 2));
		}
		if (!(_args.Parameters[0].Evaluate() is int major))
		{
			throw new ArgumentException("Calling function version: Expected int arguments");
		}
		if (!(_args.Parameters[1].Evaluate() is int minor))
		{
			throw new ArgumentException("Calling function version: Expected int arguments");
		}
		int build = 0;
		if (_args.Parameters.Length >= 3)
		{
			if (!(_args.Parameters[2].Evaluate() is int num))
			{
				throw new ArgumentException("Calling function version: Expected int arguments");
			}
			build = num;
		}
		int revision = 0;
		if (_args.Parameters.Length >= 4)
		{
			if (!(_args.Parameters[3].Evaluate() is int num2))
			{
				throw new ArgumentException("Calling function version: Expected int arguments");
			}
			revision = num2;
		}
		_args.Result = _args.Parameters.Length switch
		{
			2 => new Version(major, minor), 
			3 => new Version(major, minor, build), 
			_ => new Version(major, minor, build, revision), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamepref(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "gamepref", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function gamepref: Expected string argument");
		}
		if (!EnumUtils.TryParse<EnumGamePrefs>(text, out var _result, _ignoreCase: true))
		{
			throw new ArgumentException("Calling function gamepref: Unknown GamePref name '" + text + "'");
		}
		_args.Result = GamePrefs.GetObject(_result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void eventActive(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "eventActive", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function eventActive: Expected string argument");
		}
		if (!EventsFromXml.Events.TryGetValue(text, out var value))
		{
			throw new ArgumentException("Calling function eventActive: Unknown event name '" + text + "'");
		}
		_args.Result = value.Active && !GamePrefs.GetBool(EnumGamePrefs.OptionsDisableXmlEvents);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfo(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "serverinfo", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function serverinfo: Expected string argument");
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			throw new Exception("'serverinfo' conditional function can only be executed with a game loaded!");
		}
		if (EnumUtils.TryParse<GameInfoBool>(text, out var _result, _ignoreCase: true))
		{
			_args.Result = gameServerInfo.GetValue(_result);
			return;
		}
		if (EnumUtils.TryParse<GameInfoInt>(text, out var _result2, _ignoreCase: true))
		{
			_args.Result = gameServerInfo.GetValue(_result2);
			return;
		}
		if (EnumUtils.TryParse<GameInfoString>(text, out var _result3, _ignoreCase: true))
		{
			_args.Result = gameServerInfo.GetValue(_result3);
			return;
		}
		throw new ArgumentException("Calling function serverinfo: Unknown ServerInfo name '" + text + "'");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void time_minutes(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "time_minutes", _args.Parameters.Length, 0));
		}
		_args.Result = DateTime.Now.Minute;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void game_loaded(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "game_loaded", _args.Parameters.Length, 0));
		}
		_args.Result = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode != ProtocolManager.NetworkType.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void xpath(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "xpath", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function xpath: Expected string argument");
		}
		if (!_args.Parameters[0].Parameters.TryGetValue("xml", out var value) || !(value is XmlFile xmlFile))
		{
			throw new ArgumentException("Calling function xpath: XML file reference not found");
		}
		Log.Out("Xpath conditional on file: " + xmlFile.Filename);
		List<XObject> list = new List<XObject>();
		if (!xmlFile.GetXpathResultsInList(text, list))
		{
			_args.Result = null;
			return;
		}
		Log.Out($"Xpath matches: {list.Count}");
		if (list.Count > 1)
		{
			_args.Result = "More than one match";
			return;
		}
		_args.Result = list[0].ToString();
		Log.Out($"Match: {_args.Result}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void has_entitlement(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "has_entitlement", _args.Parameters.Length, 1));
		}
		if (!(_args.Parameters[0].Evaluate() is string text))
		{
			throw new ArgumentException("Calling function has_entitlement: Expected string argument");
		}
		if (!EnumUtils.TryParse<EntitlementSetEnum>(text, out var _result, _ignoreCase: true))
		{
			throw new ArgumentException("Calling function has_entitlement: Unknown EntitlementSetEnum name '" + text + "'");
		}
		_args.Result = EntitlementManager.Instance.HasEntitlement(_result);
	}
}
