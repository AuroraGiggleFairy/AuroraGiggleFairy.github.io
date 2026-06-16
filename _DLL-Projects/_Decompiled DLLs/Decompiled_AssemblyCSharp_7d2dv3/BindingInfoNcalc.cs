using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;
using Unity.Profiling;

public class BindingInfoNcalc : IBindingInstance, IBindingNcalc
{
	public abstract class VariableStateAbs
	{
		public abstract object CurrentValue { get; }

		public abstract bool RefreshValue();

		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateAbs()
		{
		}
	}

	public abstract class VariableStateBinding : VariableStateAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly XUiController Controller;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string BindingName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Dictionary<string, object> ExpressionParamDict;

		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateBinding(string _name, XUiController _controller, Dictionary<string, object> _expressionParamDict)
		{
			BindingName = _name;
			Controller = _controller;
			ExpressionParamDict = _expressionParamDict;
		}
	}

	public class VariableStateParamBinding : VariableStateBinding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiBindingDelegate bindingDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public object currentValue;

		public override object CurrentValue => currentValue;

		public VariableStateParamBinding(string _name, XUiController _controller, object _initialValue, XuiBindingDelegate _bindingDelegate, Dictionary<string, object> _expressionParamDict)
			: base(_name, _controller, _expressionParamDict)
		{
			currentValue = _initialValue;
			bindingDelegate = _bindingDelegate;
			ExpressionParamDict[BindingName] = _initialValue;
		}

		public override bool RefreshValue()
		{
			object obj = bindingDelegate(Controller);
			if (obj == null)
			{
				Log.Warning("[XUi] Binding '" + BindingName + "' on controller '" + Controller.GetType().FullName + "' returned null, should always return appropriate non-null value of same data type. Hierarchy: " + Controller.GetXuiHierarchy());
				return false;
			}
			if (obj.Equals(CurrentValue))
			{
				return false;
			}
			currentValue = obj;
			ExpressionParamDict[BindingName] = obj;
			return true;
		}

		public override string ToString()
		{
			return $"ParamBinding: {BindingName}='{currentValue}'";
		}
	}

	public class VariableStateLegacyBinding : VariableStateBinding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string currentValue;

		public override object CurrentValue => currentValue;

		public VariableStateLegacyBinding(string _name, XUiController _controller, string _initialValue, Dictionary<string, object> _expressionParamDict)
			: base(_name, _controller, _expressionParamDict)
		{
			currentValue = _initialValue;
			ExpressionParamDict[BindingName] = _initialValue;
		}

		public override bool RefreshValue()
		{
			string _value = "";
			if (!Controller.GetBindingValue(ref _value, BindingName))
			{
				Log.Error("[XUi] Refreshing binding failed: Controller's GetBindingValue no longer returns true! (Binding: " + BindingName + ", hierarchy: " + Controller.GetXuiHierarchy() + ")");
				return false;
			}
			if (string.Equals(currentValue, _value, StringComparison.Ordinal))
			{
				return false;
			}
			currentValue = _value;
			ExpressionParamDict[BindingName] = _value;
			return true;
		}

		public override string ToString()
		{
			return "LegacyBinding: " + BindingName + "='" + currentValue + "'";
		}
	}

	public class VariableStateCVar : VariableStateAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string cVarName;

		[PublicizedFrom(EAccessModifier.Private)]
		public float currentValue;

		public override object CurrentValue => currentValue;

		public VariableStateCVar(string _name)
		{
			cVarName = _name;
			currentValue = 0f;
		}

		public override bool RefreshValue()
		{
			GameManager instance = GameManager.Instance;
			if (instance == null)
			{
				return false;
			}
			EntityPlayer entityPlayer = instance.World?.GetPrimaryPlayer();
			if (entityPlayer == null)
			{
				return false;
			}
			float cVar = entityPlayer.GetCVar(cVarName);
			if (cVar.Equals(currentValue))
			{
				return false;
			}
			currentValue = cVar;
			return true;
		}

		public override string ToString()
		{
			return $"cVar: {cVarName}={currentValue}";
		}
	}

	public abstract class VariableStateSimpleLookupAbs : VariableStateAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public object currentValue;

		public abstract string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public override object CurrentValue => currentValue;

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract object getCurrentValue();

		public override bool RefreshValue()
		{
			object obj = getCurrentValue();
			if (obj == null)
			{
				Log.Warning("[XUi] " + VarType + " '" + VarName + "' returned null.");
				return false;
			}
			if (obj.Equals(CurrentValue))
			{
				return false;
			}
			currentValue = obj;
			return true;
		}

		public override string ToString()
		{
			return $"{VarType}: {VarName}={currentValue}";
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateSimpleLookupAbs()
		{
		}
	}

	public class VariableStateGamePref : VariableStateSimpleLookupAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumGamePrefs pref;

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GamePref";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return pref.ToStringCached();
			}
		}

		public VariableStateGamePref(EnumGamePrefs _pref)
		{
			pref = _pref;
			currentValue = getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return GamePrefs.GetObject(pref);
		}
	}

	public class VariableStateGameStat : VariableStateSimpleLookupAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumGameStats stat;

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameStat";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return stat.ToStringCached();
			}
		}

		public VariableStateGameStat(EnumGameStats _stat)
		{
			stat = _stat;
			currentValue = getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return GameStats.GetObject(stat);
		}
	}

	public class VariableStateGameInfoInt : VariableStateSimpleLookupAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoInt name;

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoInt";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return name.ToStringCached();
			}
		}

		public VariableStateGameInfoInt(GameInfoInt _name)
		{
			name = _name;
			currentValue = getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(name);
		}
	}

	public class VariableStateGameInfoBool : VariableStateSimpleLookupAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoBool name;

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoBool";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return name.ToStringCached();
			}
		}

		public VariableStateGameInfoBool(GameInfoBool _name)
		{
			name = _name;
			currentValue = getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(name);
		}
	}

	public class VariableStateGameInfoString : VariableStateSimpleLookupAbs
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoString name;

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoString";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return name.ToStringCached();
			}
		}

		public VariableStateGameInfoString(GameInfoString _name)
		{
			name = _name;
			currentValue = getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IXUiElement targetViewOrTween;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string attributeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string sourceString;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IXUiElement targetParsingElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XuiParsingDelegate targetParsingDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<VariableStateAbs> variables = new List<VariableStateAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool singleIdentifierBinding;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public int boundToControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public object currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex fullBinding = new Regex("^\\{%.*\\}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex singleIdentifierExpression = new Regex("^\\s*([a-zA-Z]\\w*)\\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmCtor = new ProfilerMarker("BindingInfoNcalc.ctor");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueComplete = new ProfilerMarker("BindingInfoNcalc.RefreshValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueCompleteSourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshBindings = new ProfilerMarker("Refresh Bindings");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmNcalcEval = new ProfilerMarker("NCalc.Evaluate");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmApplyAttribute = new ProfilerMarker("Apply Attribute");

	public string SourceText => sourceString;

	public IXUiElement TargetElement => targetViewOrTween;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInitializing { get; }

	public static bool IsFullNcalcBinding(string _input)
	{
		if (!fullBinding.IsMatch(_input))
		{
			return false;
		}
		int num = 0;
		for (int i = 2; i < _input.Length - 1; i++)
		{
			if (_input[i] == '{')
			{
				num++;
			}
			else if (_input[i] == '}')
			{
				num--;
			}
			if (num < 0)
			{
				return false;
			}
		}
		if (num > 0)
		{
			return false;
		}
		return true;
	}

	public BindingInfoNcalc(IXUiElement _targetViewOrTween, string _attribute, string _sourceString)
	{
		targetViewOrTween = _targetViewOrTween;
		attributeName = _attribute;
		sourceString = _sourceString;
		pmRefreshValueCompleteSourceText = new ProfilerMarker(_sourceString);
		int num = 1;
		if (_sourceString[1] == '%' || _sourceString[1] == '#')
		{
			num++;
		}
		string input = _sourceString.Substring(num, _sourceString.Length - num - 1);
		Match match = singleIdentifierExpression.Match(input);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			singleIdentifierBinding = true;
			if (!FindParameter(value, out currentValue))
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning("[XUi] Binding name '" + value + "' not found! Hierarchy: " + xuiHierarchy());
				}
				return;
			}
		}
		else
		{
			expression = new Expression(input, EvaluateOptions.NoCache | EvaluateOptions.UseDoubleForAbsFunction | EvaluateOptions.ReuseInstances)
			{
				Parameters = expressionParamDict
			};
			expression.EvaluateFunction += nCalcEvaluateFunction;
			expression.EvaluateParameter += nCalcEvaluateParameter;
			IsInitializing = true;
			currentValue = evaluateExpression();
			IsInitializing = false;
		}
		if (currentValue == null)
		{
			expression = null;
			Log.Error("[XUi] Failed evaluating binding expression, result is null. Binding " + attributeName + "=\"" + sourceString + "\", view hierarchy: " + xuiHierarchy());
			return;
		}
		BindToController();
		Type type = currentValue.GetType();
		ParsingMethodCache.ParsingMethodData _parsingDelegate2;
		if (ParsingMethodCache.Instance.TryGetParsingDelegate(targetViewOrTween, attributeName, out var _parsingDelegate))
		{
			if (_parsingDelegate.TryGetDelegateForSourceType(type, out targetParsingDelegate))
			{
				targetParsingElement = targetViewOrTween;
				return;
			}
			Log.Error("[XUi] Failed mapping binding to parser on view, no conversion possible. Binding " + attributeName + "=\"" + sourceString + "\", binding result type " + type.FullName + ", parser native type " + _parsingDelegate.NativeParseType.FullName + ", view hierarchy: " + xuiHierarchy());
			expression = null;
		}
		else if (targetViewOrTween is XUiTweenAbs)
		{
			Log.Error("[XUi] Failed mapping binding to parser on XUiTween, no parse found for attribute name '" + attributeName + "', view hierarchy: " + xuiHierarchy());
			expression = null;
		}
		else if (ParsingMethodCache.Instance.TryGetParsingDelegate(targetViewOrTween.Controller, attributeName, out _parsingDelegate2))
		{
			if (_parsingDelegate2.TryGetDelegateForSourceType(type, out targetParsingDelegate))
			{
				targetParsingElement = targetViewOrTween.Controller;
				return;
			}
			Log.Error("[XUi] Failed mapping binding to parser on controller, no conversion possible. Binding " + attributeName + "=\"" + sourceString + "\", binding result type " + type.FullName + ", parser native type " + _parsingDelegate2.NativeParseType.FullName + ", view hierarchy: " + xuiHierarchy());
			expression = null;
		}
		else
		{
			targetParsingElement = targetViewOrTween.Controller;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void BindToController()
		{
			if (usesIndeterministicFunctions || boundToControllers == 0)
			{
				for (XUiController xUiController = targetViewOrTween.Controller; xUiController != null; xUiController = xUiController.Parent)
				{
					if (xUiController.GetType() != typeof(XUiController))
					{
						xUiController.Bindings.AddBinding(this);
						break;
					}
				}
			}
		}
	}

	public void RefreshValue()
	{
		if ((singleIdentifierBinding && variables.Count == 0) || (expression == null && !singleIdentifierBinding))
		{
			return;
		}
		using (pmRefreshValueComplete.Auto())
		{
			using (pmRefreshValueCompleteSourceText.Auto())
			{
				bool flag = usesIndeterministicFunctions;
				using (pmRefreshBindings.Auto())
				{
					for (int i = 0; i < variables.Count; i++)
					{
						flag |= variables[i].RefreshValue();
					}
				}
				if (singleIdentifierBinding)
				{
					currentValue = variables[0].CurrentValue;
				}
				else
				{
					using (pmNcalcEval.Auto())
					{
						if (flag)
						{
							currentValue = evaluateExpression();
						}
					}
				}
				try
				{
					object obj = currentValue;
					if (obj is string text && text.Contains("{cvar("))
					{
						obj = BindingsManager.ReplaceCVars(text);
					}
					using (pmApplyAttribute.Auto())
					{
						if (targetParsingDelegate == null)
						{
							((XUiController)targetParsingElement).CustomAttributes[attributeName] = obj;
						}
						else
						{
							targetParsingDelegate(targetParsingElement, obj);
						}
					}
				}
				catch (Exception e)
				{
					Log.Error($"[XUi] Exception parsing result of binding. Binding {attributeName}=\"{sourceString}\", binding result: '{currentValue}' (type: {currentValue.GetType().FullName}), view hierarchy: {xuiHierarchy()}:");
					Log.Exception(e);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object evaluateExpression()
	{
		try
		{
			return expression.Evaluate();
		}
		catch (Exception e)
		{
			Log.Error("[XUi] Binding expression can not be evaluated. Binding " + attributeName + "=\"" + sourceString + "\" --- hierarchy: " + xuiHierarchy());
			Log.Exception(e);
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		if (FindParameter(_name, out var _value))
		{
			_args.Result = _value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		BindingNcalcFunctions.EvaluateFunc(this, _name, _args);
	}

	public bool FindParameter(string _name, out object _value)
	{
		if (expressionParamDict.TryGetValue(_name, out _value))
		{
			return true;
		}
		for (XUiController xUiController = targetViewOrTween.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				object obj = null;
				VariableStateAbs variableStateAbs = null;
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xUiController, _name, out var _bindingDelegate))
				{
					obj = _bindingDelegate(xUiController);
					variableStateAbs = new VariableStateParamBinding(_name, xUiController, obj, _bindingDelegate, expressionParamDict);
				}
				else
				{
					string _value2 = "";
					if (xUiController.GetBindingValue(ref _value2, _name))
					{
						variableStateAbs = new VariableStateLegacyBinding(_name, xUiController, _value2, expressionParamDict);
						obj = _value2;
					}
				}
				if (variableStateAbs != null)
				{
					xUiController.Bindings.AddBinding(this);
					boundToControllers++;
					variables.Add(variableStateAbs);
					_value = obj;
					return true;
				}
			}
		}
		if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning("[XUi] Binding name '" + _name + "' not found! Hierarchy: " + xuiHierarchy());
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiHierarchy()
	{
		return targetViewOrTween.GetXuiHierarchy();
	}

	public void SetIndeterministic()
	{
		usesIndeterministicFunctions = true;
	}

	public bool RegisterVariable(VariableStateAbs _var)
	{
		variables.Add(_var);
		return true;
	}
}
