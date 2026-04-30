using System;
using System.Collections.Generic;
using NCalc;

public class BindingItemNcalc : BindingItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BindingState
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string bindingName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiController controller;

		public string CurrentValue;

		public BindingState(string _bindingName, XUiController _controller, string _initialValue)
		{
			bindingName = _bindingName;
			controller = _controller;
			CurrentValue = _initialValue;
		}

		public bool RefreshValue()
		{
			string _value = "";
			if (!controller.GetBindingValue(ref _value, bindingName))
			{
				return false;
			}
			if (string.Equals(CurrentValue, _value, StringComparison.Ordinal))
			{
				return false;
			}
			CurrentValue = _value;
			return true;
		}

		public override string ToString()
		{
			return "Binding:" + bindingName + "=" + CurrentValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BindingInfo parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView view;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, BindingState> bindings = new Dictionary<string, BindingState>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	public BindingItemNcalc(BindingInfo _parent, XUiView _view, string _sourceText)
		: base(_sourceText)
	{
		if (FieldName[0] == '#')
		{
			FieldName = FieldName.Substring(1);
		}
		else if (FieldName.EndsWith("|once", StringComparison.OrdinalIgnoreCase))
		{
			FieldName = FieldName.Substring(0, FieldName.Length - "|once".Length);
		}
		parent = _parent;
		view = _view;
		expression = new Expression(FieldName, EvaluateOptions.IgnoreCase | EvaluateOptions.NoCache | EvaluateOptions.UseDoubleForAbsFunction | EvaluateOptions.ReuseInstances)
		{
			Parameters = expressionParamDict
		};
		expression.EvaluateFunction += NCalcEvaluateFunction;
		expression.EvaluateParameter += NCalcEvaluateParameter;
		CurrentValue = EvaluateExpression();
		if (!usesIndeterministicFunctions && bindings.Count != 0)
		{
			return;
		}
		for (XUiController controller = _view.Controller; controller != null; controller = controller.Parent)
		{
			if (controller.GetType() != typeof(XUiController))
			{
				DataContext = controller;
				DataContext.AddBinding(_parent);
				break;
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (BindingType == BindingTypes.Complete && !_forceAll)
		{
			return CurrentValue;
		}
		bool flag = usesIndeterministicFunctions;
		foreach (KeyValuePair<string, BindingState> binding in bindings)
		{
			if (binding.Value.RefreshValue())
			{
				expressionParamDict[binding.Key] = binding.Value.CurrentValue;
				flag = true;
			}
		}
		if (!flag)
		{
			return CurrentValue;
		}
		CurrentValue = EvaluateExpression();
		if (BindingType == BindingTypes.Once)
		{
			BindingType = BindingTypes.Complete;
		}
		return CurrentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string EvaluateExpression()
	{
		string text = null;
		try
		{
			object obj = expression.Evaluate();
			text = ((obj == null) ? "" : ((obj is decimal) ? ((decimal)obj).ToCultureInvariantString("0.########") : ((obj is float) ? ((float)obj).ToCultureInvariantString() : ((!(obj is double)) ? obj.ToString() : ((double)obj).ToCultureInvariantString()))));
		}
		catch (ArgumentException e)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + SourceText + " --- hierarchy: " + view.Controller.GetXuiHierarchy());
			Log.Exception(e);
		}
		catch (Exception e2)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + SourceText + " --- hierarchy: " + view.Controller.GetXuiHierarchy());
			Log.Exception(e2);
			text = "";
		}
		if (text != null && text.Contains("{cvar("))
		{
			text = ParseCVars(text);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		if (findParameter(_name, out var _value))
		{
			_args.Result = _value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool findParameter(string _name, out object _value)
	{
		if (bindings.TryGetValue(_name, out var value))
		{
			_value = value.CurrentValue;
			return true;
		}
		for (XUiController controller = view.Controller; controller != null; controller = controller.Parent)
		{
			if (controller.GetType() != typeof(XUiController))
			{
				string _value2 = "";
				if (controller.GetBindingValue(ref _value2, _name))
				{
					controller.AddBinding(parent);
					value = new BindingState(_name, controller, _value2);
					bindings[_name] = value;
					expressionParamDict[_name] = _value2;
					_value = _value2;
					return true;
				}
			}
		}
		_value = null;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("localization"))
		{
			localization(_args);
		}
		else if (_name.EqualsCaseInsensitive("cvar"))
		{
			usesIndeterministicFunctions = true;
			cvar(_args);
		}
		else if (_name.EqualsCaseInsensitive("format"))
		{
			format(_args);
		}
		else if (_name.EqualsCaseInsensitive("length"))
		{
			length(_args);
		}
		else if (_name.EqualsCaseInsensitive("int"))
		{
			toInt(_args);
		}
		else if (_name.EqualsCaseInsensitive("float"))
		{
			toFloat(_args);
		}
		else if (_name.EqualsCaseInsensitive("bound"))
		{
			isBound(_args);
		}
		else if (_name.EqualsCaseInsensitive("always"))
		{
			usesIndeterministicFunctions = true;
			always(_args);
		}
		else if (_name.EqualsCaseInsensitive("defined"))
		{
			isDefined(_args);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localization(FunctionArgs _args)
	{
		_args.Result = "<ERROR>";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "localization", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
		}
		else
		{
			object obj = _args.Parameters[0].Evaluate();
			if (obj == null)
			{
				Log.Error("[XUi] Binding expression calling function 'localization': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			}
			else
			{
				_args.Result = Localization.Get(obj.ToString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cvar(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "cvar", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
			_args.Result = 1f;
			return;
		}
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			_args.Result = 1f;
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			_args.Result = 1f;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'cvar': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			_args.Result = 1f;
		}
		else
		{
			_args.Result = primaryPlayer.GetCVar(obj.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void format(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length < 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected at least {2}). Binding expression: '{3}' --- hierarchy: {4}", "format", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'format': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			return;
		}
		object[] array = new object[_args.Parameters.Length - 1];
		for (int i = 1; i < _args.Parameters.Length; i++)
		{
			array[i - 1] = _args.Parameters[i].Evaluate();
		}
		string result = string.Format(obj.ToString(), array);
		_args.Result = result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void length(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "length", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
		}
		else
		{
			object obj = _args.Parameters[0].Evaluate();
			if (obj == null)
			{
				Log.Error("[XUi] Binding expression calling function 'length': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			}
			else
			{
				_args.Result = obj.ToString().Length;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toInt(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "toInt", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
			_args.Result = 0;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'toInt': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			_args.Result = 0;
			return;
		}
		try
		{
			_args.Result = Convert.ToInt32(obj);
		}
		catch (Exception e)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}': Argument does not evaluate to a number. Binding expression: '{1}', argument '{2}' --- hierarchy: {3}", "toInt", SourceText, obj, view.Controller.GetXuiHierarchy()));
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toFloat(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "toFloat", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
			_args.Result = 0.0;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'toFloat': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			_args.Result = 0.0;
			return;
		}
		try
		{
			_args.Result = Convert.ToDouble(obj);
		}
		catch (Exception e)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}': Argument does not evaluate to a number. Binding expression: '{1}', argument '{2}' --- hierarchy: {3}", "toFloat", SourceText, obj, view.Controller.GetXuiHierarchy()));
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void isBound(FunctionArgs _args)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void always(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "always", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
		}
		else
		{
			_args.Result = _args.Parameters[0].Evaluate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void isDefined(FunctionArgs _args)
	{
		_args.Result = "<ERROR>";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", "isDefined", _args.Parameters.Length, 1, SourceText, view.Controller.GetXuiHierarchy()));
		}
		else
		{
			object obj = _args.Parameters[0].Evaluate();
			if (obj == null)
			{
				Log.Error("[XUi] Binding expression calling function 'isDefined': Can not evaluate argument. Binding expression: '" + SourceText + "' --- hierarchy: " + view.Controller.GetXuiHierarchy());
			}
			else
			{
				_args.Result = findParameter(obj.ToString(), out var _);
			}
		}
	}
}
