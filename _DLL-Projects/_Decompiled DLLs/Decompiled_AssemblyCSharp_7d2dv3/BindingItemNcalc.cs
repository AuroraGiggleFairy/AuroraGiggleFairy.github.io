using System;
using System.Collections.Generic;
using NCalc;
using Unity.Profiling;

public class BindingItemNcalc : BindingItem, IBindingNcalc
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BindingState
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string bindingName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiController controller;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiBindingDelegate bindingDelegate;

		public string CurrentValue;

		public BindingState(string _bindingName, XUiController _controller, string _initialValue, XuiBindingDelegate _bindingDelegate)
		{
			bindingName = _bindingName;
			controller = _controller;
			CurrentValue = _initialValue;
			bindingDelegate = _bindingDelegate;
		}

		public bool RefreshValue()
		{
			string _value = "";
			if (bindingDelegate == null)
			{
				if (!controller.GetBindingValue(ref _value, bindingName))
				{
					Log.Error("[XUi] Refreshing binding failed: Controller's GetBindingValue no longer returns true! (Binding: " + bindingName + ", hierarchy: " + controller.GetXuiHierarchy() + ")");
					return false;
				}
			}
			else
			{
				object obj = bindingDelegate(controller);
				if (obj == null)
				{
					Log.Warning("[XUi] Binding '" + bindingName + "' returned null, should always return appropriate non-null value of same data type. Hierarchy: " + controller.GetXuiHierarchy());
					obj = "";
				}
				_value = obj.ToString();
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

	public readonly BindingInfo Parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, BindingState> bindings = new Dictionary<string, BindingState>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmNcalcEval = new ProfilerMarker("NCalc.Evaluate");

	public new string SourceText => base.SourceText;

	public IXUiElement TargetElement => Parent.View;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInitializing { get; }

	public BindingItemNcalc(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		if (fieldName[0] == '#')
		{
			fieldName = fieldName.Substring(1);
		}
		Parent = _parent;
		IsInitializing = true;
		expression = new Expression(fieldName, EvaluateOptions.IgnoreCase | EvaluateOptions.NoCache | EvaluateOptions.UseDoubleForAbsFunction | EvaluateOptions.ReuseInstances)
		{
			Parameters = expressionParamDict
		};
		expression.EvaluateFunction += nCalcEvaluateFunction;
		expression.EvaluateParameter += nCalcEvaluateParameter;
		currentValue = evaluateExpression();
		IsInitializing = false;
		if (!usesIndeterministicFunctions && bindings.Count != 0)
		{
			return;
		}
		for (XUiController xUiController = Parent.View.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				xUiController.Bindings.AddBinding(_parent);
				break;
			}
		}
	}

	public override string GetValue()
	{
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
			return currentValue;
		}
		using (pmNcalcEval.Auto())
		{
			currentValue = evaluateExpression();
		}
		return currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string evaluateExpression()
	{
		string result = null;
		try
		{
			object obj = expression.Evaluate();
			string text = ((obj == null) ? "" : ((obj is decimal value) ? value.ToCultureInvariantString("0.########") : ((obj is float value2) ? value2.ToCultureInvariantString() : ((!(obj is double value3)) ? obj.ToString() : value3.ToCultureInvariantString()))));
			result = text;
		}
		catch (ArgumentException e)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + SourceText + " --- hierarchy: " + Parent.View.GetXuiHierarchy());
			Log.Exception(e);
		}
		catch (Exception e2)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + SourceText + " --- hierarchy: " + Parent.View.GetXuiHierarchy());
			Log.Exception(e2);
			result = "";
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		if (FindParameter(_name, out var _value))
		{
			_args.Result = _value;
		}
	}

	public bool FindParameter(string _name, out object _value)
	{
		if (bindings.TryGetValue(_name, out var value))
		{
			_value = value.CurrentValue;
			return true;
		}
		for (XUiController xUiController = Parent.View.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				string _value2 = "";
				if (xUiController.GetBindingValue(ref _value2, _name))
				{
					xUiController.Bindings.AddBinding(Parent);
					value = new BindingState(_name, xUiController, _value2, null);
					bindings[_name] = value;
					expressionParamDict[_name] = _value2;
					_value = _value2;
					return true;
				}
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xUiController, _name, out var _bindingDelegate))
				{
					xUiController.Bindings.AddBinding(Parent);
					_value2 = _bindingDelegate(xUiController).ToString();
					value = new BindingState(_name, xUiController, _value2, _bindingDelegate);
					bindings[_name] = value;
					expressionParamDict[_name] = _value2;
					_value = _bindingDelegate(xUiController);
					return true;
				}
			}
		}
		_value = null;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		BindingNcalcFunctions.EvaluateFunc(this, _name, _args);
	}

	public void SetIndeterministic()
	{
		usesIndeterministicFunctions = true;
	}

	public bool RegisterVariable(BindingInfoNcalc.VariableStateAbs _var)
	{
		return false;
	}
}
