using System;
using System.Collections.Generic;
using Unity.Profiling;

public class BindingsManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IBindingInstance> bindingList = new List<IBindingInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmControllerRefreshBindings = new ProfilerMarker("XC.RefreshBindings");

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerMarker pmControllerCreateBinding = new ProfilerMarker("XC.CreateBinding");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker pmReplaceCvars = new ProfilerMarker("ParseCVars");

	public void RefreshBindings()
	{
		using (pmControllerRefreshBindings.Auto())
		{
			for (int i = 0; i < bindingList.Count; i++)
			{
				bindingList[i].RefreshValue();
			}
		}
	}

	public void AddBinding(IBindingInstance _info)
	{
		if (!bindingList.Contains(_info))
		{
			bindingList.Add(_info);
		}
	}

	public static IBindingInstance CreateBinding(XUiView _view, string _attribute, string _value)
	{
		using (pmControllerCreateBinding.Auto())
		{
			BindingNcalcFunctions.RegisterNcalcFunctions();
			IBindingInstance bindingInstance;
			if (BindingInfoNcalc.IsFullNcalcBinding(_value))
			{
				bindingInstance = new BindingInfoNcalc(_view, _attribute, _value);
				bindingInstance.RefreshValue();
			}
			else
			{
				bindingInstance = new BindingInfo(_view, _attribute, _value);
			}
			return bindingInstance;
		}
	}

	public static string ReplaceCVars(string _fullText)
	{
		using (pmReplaceCvars.Auto())
		{
			for (int num = _fullText.IndexOf("{cvar(", StringComparison.Ordinal); num != -1; num = _fullText.IndexOf("{cvar(", num, StringComparison.Ordinal))
			{
				string text = _fullText.Substring(num, _fullText.IndexOf('}', num) + 1 - num);
				string text2 = "";
				int num2 = text.IndexOf('(') + 1;
				string text3 = text.Substring(num2, text.IndexOf(')') - num2);
				if (text3.IndexOf(':') >= 0)
				{
					string[] array = text3.Split(':');
					text3 = array[0];
					text2 = array[1];
				}
				_fullText = _fullText.Replace(text, XUiM_Player.GetPlayer().GetCVar(text3).ToString(text2));
			}
			return _fullText;
		}
	}
}
