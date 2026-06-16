using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NCalc;
using UnityEngine;
using UnityEngine.Scripting;

[XuiBindingNcalcFunction]
[Preserve]
public static class BindingNcalcFunctions
{
	public delegate void CustomNcalcFunctionDelegate(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments);

	[PublicizedFrom(EAccessModifier.Private)]
	public class FunctionDefinition
	{
		public readonly CustomNcalcFunctionDelegate Delegate;

		public readonly int ExpectedArgumentCount;

		public readonly object ErrorResult;

		public FunctionDefinition(CustomNcalcFunctionDelegate _delegate, int _expectedArgumentCount, object _errorResult)
		{
			Delegate = _delegate;
			ExpectedArgumentCount = _expectedArgumentCount;
			ErrorResult = _errorResult;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedFalse = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedTrue = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ObjectArrayLengthCached = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Stack<object[]> objectArrays = new Stack<object[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, FunctionDefinition> ncalcFunctions = new Dictionary<string, FunctionDefinition>();

	[XuiBindingNcalcFunction("<ERROR>", 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void localization(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = Localization.Get(_evaluatedArguments[0].ToString());
	}

	[XuiBindingNcalcFunction(1f, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void cvar(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateCVar(text)))
		{
			_bindingInstance.SetIndeterministic();
		}
		if (!(GameManager.Instance == null) && GameManager.Instance.World != null)
		{
			EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (!(primaryPlayer == null))
			{
				_args.Result = primaryPlayer.GetCVar(text);
			}
		}
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamepref(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a non-empty string.", null, "gamepref");
			return;
		}
		if (!EnumUtils.TryParseIgnoreCase<EnumGamePrefs>(text, out var _result))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a valid GamePref.", null, "gamepref");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGamePref(_result)))
		{
			_bindingInstance.SetIndeterministic();
		}
		_args.Result = GamePrefs.GetObject(_result);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamestat(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a non-empty string.", null, "gamestat");
			return;
		}
		if (!EnumUtils.TryParseIgnoreCase<EnumGameStats>(text, out var _result))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a valid GameStat.", null, "gamestat");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameStat(_result)))
		{
			_bindingInstance.SetIndeterministic();
		}
		_args.Result = GameStats.GetObject(_result);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfoint(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a non-empty string.", null, "serverinfoint");
			return;
		}
		if (!EnumUtils.TryParseIgnoreCase<GameInfoInt>(text, out var _result))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a valid GameInfoInt.", null, "serverinfoint");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoInt(_result)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfoint");
		}
		else
		{
			_args.Result = gameServerInfo.GetValue(_result);
		}
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfobool(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a non-empty string.", null, "serverinfobool");
			return;
		}
		if (!EnumUtils.TryParseIgnoreCase<GameInfoBool>(text, out var _result))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a valid GameInfoBool.", null, "serverinfobool");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoBool(_result)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfobool");
		}
		else
		{
			_args.Result = gameServerInfo.GetValue(_result);
		}
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfostring(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a non-empty string.", null, "serverinfostring");
			return;
		}
		if (!EnumUtils.TryParseIgnoreCase<GameInfoString>(text, out var _result))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' is not a valid GameInfoString.", null, "serverinfostring");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoString(_result)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfostring");
		}
		else
		{
			_args.Result = gameServerInfo.GetValue(_result);
		}
	}

	[XuiBindingNcalcFunction(null, 2, "dictvalue")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void dictValue(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_bindingInstance.SetIndeterministic();
		if (!(_evaluatedArguments[0] is IDictionary dictionary))
		{
			LogFunctionError(_bindingInstance, $"Argument 0 '{_evaluatedArguments[0]}' does not evaluate to a dictionary.", null, "dictValue");
			return;
		}
		object obj = _evaluatedArguments[1];
		if (obj == null)
		{
			LogFunctionError(_bindingInstance, "Argument 1 is null.", null, "dictValue");
		}
		else if (!dictionary.Contains(obj))
		{
			_args.Result = "";
		}
		else
		{
			_args.Result = dictionary[obj];
		}
	}

	[XuiBindingNcalcFunction("", -1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void format(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (_args.Parameters.Length < 1)
		{
			LogFunctionError(_bindingInstance, $"Invalid number of arguments ({_args.Parameters.Length}, expected at least 1).", null, "format");
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			LogFunctionError(_bindingInstance, "Can not evaluate argument.", null, "format");
			return;
		}
		object[] objectArray = GetObjectArray(_args.Parameters.Length - 1);
		for (int i = 1; i < _args.Parameters.Length; i++)
		{
			objectArray[i - 1] = _args.Parameters[i].Evaluate();
		}
		string result = string.Format(obj.ToString(), objectArray);
		ReturnObjectArray(objectArray);
		_args.Result = result;
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void length(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = _evaluatedArguments[0].ToString().Length;
	}

	[XuiBindingNcalcFunction(0, 1, "int")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toInt(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = Convert.ToInt32(_evaluatedArguments[0]);
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to an integer.", e, "toInt");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "roundtoint")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void roundToInt(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "v", out var _value, "roundToInt"))
		{
			_args.Result = Mathf.RoundToInt(_value);
		}
	}

	[XuiBindingNcalcFunction(0.0, 1, "float")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toFloat(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = Convert.ToDouble(_evaluatedArguments[0]);
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a number.", e, "toFloat");
		}
	}

	[XuiBindingNcalcFunction("", 1, "str")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = _evaluatedArguments[0]?.ToString() ?? "";
	}

	[XuiBindingNcalcFunction("", 2, "itos")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void intToString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "i", out var _value, "intToString"))
		{
			_args.Result = _value.ToString(_evaluatedArguments[1].ToString());
		}
	}

	[XuiBindingNcalcFunction("", 2, "ftos")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void floatToString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "f", out var _value, "floatToString"))
		{
			_args.Result = _value.ToString(_evaluatedArguments[1].ToString());
		}
	}

	[XuiBindingNcalcFunction(false, 1, "bound")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void isBound(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		throw new NotImplementedException();
	}

	[XuiBindingNcalcFunction("", 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void always(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_bindingInstance.SetIndeterministic();
		_args.Result = _evaluatedArguments[0];
	}

	[XuiBindingNcalcFunction(false, 1, "defined")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void isDefined(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = (_bindingInstance.FindParameter(_evaluatedArguments[0].ToString(), out var _) ? boxedTrue : boxedFalse);
	}

	[XuiBindingNcalcFunction(0, 2, "c_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (!tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out var _value, "color32_i"))
		{
			return;
		}
		if (_value < 0 || _value > 3)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[1]}' outside allowed range (0-3).", null, "color32_i");
		}
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0])[_value];
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Color.", e, "color32_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_r")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_r(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).r;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Color.", e, "color32_r");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_g")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_g(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).g;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Color.", e, "color32_g");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_b")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_b(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).b;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Color.", e, "color32_b");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_a")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_a(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).a;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Color.", e, "color32_a");
		}
	}

	[XuiBindingNcalcFunction(null, 4, "color")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 0, "r", out var _value, "color32") && tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 1, "g", out var _value2, "color32") && tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 2, "b", out var _value3, "color32") && tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 3, "a", out var _value4, "color32"))
		{
			_args.Result = new Color32(_value, _value2, _value3, _value4);
		}
	}

	[XuiBindingNcalcFunction(0, 2, "v2i_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (!tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out var _value, "v2i_i"))
		{
			return;
		}
		if (_value < 0 || _value > 1)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[1]}' outside allowed range (0-1).", null, "v2i_i");
		}
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0])[_value];
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector2i.", e, "v2i_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v2i_x")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_x(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0]).x;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector2i.", e, "v2i_x");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v2i_y")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_y(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0]).y;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector2i.", e, "v2i_y");
		}
	}

	[XuiBindingNcalcFunction(null, 2, "v2i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector2i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "x", out var _value, "vector2i") && tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "y", out var _value2, "vector2i"))
		{
			_args.Result = new Vector2i(_value, _value2);
		}
	}

	[XuiBindingNcalcFunction(0, 2, "v3i_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (!tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out var _value, "v3i_i"))
		{
			return;
		}
		if (_value < 0 || _value > 2)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[1]}' outside allowed range (0-2).", null, "v3i_i");
		}
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0])[_value];
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector3i.", e, "v3i_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_x")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_x(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).x;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector3i.", e, "v3i_x");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_y")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_y(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).y;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector3i.", e, "v3i_y");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_z")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_z(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).z;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument '{_evaluatedArguments[0]}' does not evaluate to a Vector3i.", e, "v3i_z");
		}
	}

	[XuiBindingNcalcFunction(null, 3, "v3i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector3i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "x", out var _value, "vector3i") && tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "y", out var _value2, "vector3i") && tryArgAsInt(_bindingInstance, _evaluatedArguments, 2, "z", out var _value3, "vector3i"))
		{
			_args.Result = new Vector3i(_value, _value2, _value3);
		}
	}

	[XuiBindingNcalcFunction(null, 2, "v2")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector2(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "x", out var _value, "vector2") && tryArgAsFloat(_bindingInstance, _evaluatedArguments, 1, "y", out var _value2, "vector2"))
		{
			_args.Result = new Vector2(_value, _value2);
		}
	}

	[XuiBindingNcalcFunction(null, 3, "v3")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector3(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "x", out var _value, "vector3") && tryArgAsFloat(_bindingInstance, _evaluatedArguments, 1, "y", out var _value2, "vector3") && tryArgAsFloat(_bindingInstance, _evaluatedArguments, 2, "z", out var _value3, "vector3"))
		{
			_args.Result = new Vector3(_value, _value2, _value3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsByteClamped(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out byte _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		try
		{
			_value = (byte)Mathf.Clamp((int)obj, 0, 255);
			return true;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument {_argName}='{obj}' (type {obj.GetType().FullName}) does not evaluate to an integer.", e, _funcName);
			_value = 0;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsInt(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out int _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		try
		{
			_value = Convert.ToInt32(obj);
			return true;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument {_argName}='{obj}' (type {obj.GetType().FullName}) does not evaluate to an integer.", e, _funcName);
			_value = 0;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsFloat(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out float _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		try
		{
			_value = Convert.ToSingle(obj);
			return true;
		}
		catch (Exception e)
		{
			LogFunctionError(_bindingInstance, $"Argument {_argName}='{obj}' (type {obj.GetType().FullName}) does not evaluate to a number.", e, _funcName);
			_value = 0f;
			return false;
		}
	}

	public static void LogFunctionError(IBindingNcalc _bindingInstance, string _message, Exception _e = null, [CallerMemberName] string _funcName = null)
	{
		Log.Error("[XUi] Binding expression calling function '" + _funcName + "': " + _message + " Binding expression: '" + _bindingInstance.SourceText + "' --- hierarchy: " + _bindingInstance.TargetElement.GetXuiHierarchy());
		if (_e != null)
		{
			Log.Exception(_e);
		}
	}

	public static void EvaluateFunc(IBindingNcalc _bindingInstance, string _name, FunctionArgs _args)
	{
		if (!ncalcFunctions.TryGetValue(_name, out var value))
		{
			return;
		}
		object[] array = null;
		_args.Result = null;
		if (value.ExpectedArgumentCount >= 0)
		{
			if (_args.Parameters.Length != value.ExpectedArgumentCount)
			{
				LogFunctionError(_bindingInstance, $"Invalid number of arguments ({_args.Parameters.Length}, expected {value.ExpectedArgumentCount}).", null, _name);
				if (value.ErrorResult != null)
				{
					_args.Result = value.ErrorResult;
				}
				return;
			}
			if (_args.Parameters.Length != 0)
			{
				array = GetObjectArray(_args.Parameters.Length);
				for (int i = 0; i < _args.Parameters.Length; i++)
				{
					object obj = _args.Parameters[i].Evaluate();
					if (obj == null)
					{
						LogFunctionError(_bindingInstance, $"Can not evaluate argument at index {i}.", null, _name);
						if (value.ErrorResult != null)
						{
							_args.Result = value.ErrorResult;
						}
						return;
					}
					array[i] = obj;
				}
			}
		}
		value.Delegate(_bindingInstance, _args, array);
		if (_args.Result == null && value.ErrorResult != null)
		{
			_args.Result = value.ErrorResult;
		}
		ReturnObjectArray(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object[] GetObjectArray(int _length)
	{
		if (_length > 20)
		{
			return new object[_length];
		}
		if (objectArrays.Count == 0)
		{
			return new object[_length];
		}
		return objectArrays.Pop();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReturnObjectArray(object[] _arr)
	{
		if (_arr != null && _arr.Length == 20)
		{
			objectArrays.Push(_arr);
		}
	}

	public static void RegisterNcalcFunctions()
	{
		if (ncalcFunctions.Count <= 0)
		{
			for (int i = 0; i < 10; i++)
			{
				objectArrays.Push(new object[20]);
			}
			ReflectionHelpers.FindTypesWithAttribute<XuiBindingNcalcFunctionAttribute>(TypeFoundCallback, _allowAbstract: true);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void MethodFoundCallback(MethodInfo _method, bool _hasMultiple, XuiBindingNcalcFunctionAttribute _methodAttribute)
		{
			if (!ReflectionHelpers.MethodCompatibleWithDelegate<CustomNcalcFunctionDelegate>(_method, _openDelegate: true))
			{
				Log.Error("[XUi] Binding NCalc function method " + _method.DeclaringType.FullName + "." + _method.Name + " not compatible with delegate signature");
			}
			else
			{
				string text = _methodAttribute.FunctionName ?? _method.Name.ToLowerInvariant();
				CustomNcalcFunctionDelegate customNcalcFunctionDelegate = (CustomNcalcFunctionDelegate)_method.CreateDelegate(typeof(CustomNcalcFunctionDelegate));
				if (ncalcFunctions.ContainsKey(text))
				{
					Log.Warning("[XUi] Binding NCalc function method " + _method.DeclaringType.FullName + "." + _method.Name + " overriding previously defined method for NCalc function '" + text + "'");
				}
				ncalcFunctions[text] = new FunctionDefinition(customNcalcFunctionDelegate, _methodAttribute.ExpectedArgumentCount, _methodAttribute.ErrorResult);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void TypeFoundCallback(Type _type, bool _hasMultiple, XuiBindingNcalcFunctionAttribute _classAttribute)
		{
			ReflectionHelpers.GetMethodsWithAttribute<XuiBindingNcalcFunctionAttribute>(_type, MethodFoundCallback);
		}
	}
}
