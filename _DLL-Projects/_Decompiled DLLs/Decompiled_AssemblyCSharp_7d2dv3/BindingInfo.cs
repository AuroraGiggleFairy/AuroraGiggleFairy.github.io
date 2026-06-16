using System;
using System.Collections.Generic;
using Unity.Profiling;

public class BindingInfo : IBindingInstance
{
	public readonly XUiView View;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string attributeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string sourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool parsersDetected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XuiParsingDelegate parsingDelegateView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XuiParsingDelegate parsingDelegateController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingItem> bindingList = new List<BindingItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> cachedBindingValues = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResultValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmCtor = new ProfilerMarker("BindingInfo.ctor");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueComplete = new ProfilerMarker("BindingInfo.RefreshValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueCompleteSourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmGetBindingValues = new ProfilerMarker("GetBindingValues");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmBuildAttributeValue = new ProfilerMarker("BuildAttributeValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmParseAttribute = new ProfilerMarker("ParseAttribute");

	public BindingInfo(XUiView _view, string _attribute, string _sourceText)
	{
		View = _view;
		attributeName = _attribute;
		sourceText = _sourceText;
		pmRefreshValueCompleteSourceText = new ProfilerMarker(sourceText);
		int num = sourceText.IndexOf("{", StringComparison.Ordinal);
		while (num != -1)
		{
			int num2 = sourceText.IndexOf("}", num, StringComparison.Ordinal);
			if (num2 == -1)
			{
				break;
			}
			string text = sourceText.Substring(num, num2 - num + 1);
			bool flag = false;
			for (int i = 0; i < bindingList.Count; i++)
			{
				if (bindingList[i].SourceText == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BindingItem item;
				if (text.StartsWith("{cvar("))
				{
					item = new BindingItemCvar(this, text);
				}
				else if (text.StartsWith("{#"))
				{
					item = new BindingItemNcalc(this, text);
				}
				else
				{
					if (text.StartsWith("{%"))
					{
						throw new Exception("[XUi] Using new NCalc binding expression for partial attribute evaluation. Attribute: '" + attributeName + "', full attribute value: '" + sourceText + "', binding string: '" + text + "', view hierarchy: " + View.GetXuiHierarchy() + ":");
					}
					item = new BindingItemStandard(this, text);
				}
				bindingList.Add(item);
			}
			num = sourceText.IndexOf("{", num2, StringComparison.Ordinal);
		}
	}

	public void RefreshValue()
	{
		using (pmRefreshValueComplete.Auto())
		{
			using (pmRefreshValueCompleteSourceText.Auto())
			{
				bool flag = cachedResultValue == null;
				using (pmGetBindingValues.Auto())
				{
					for (int i = 0; i < bindingList.Count; i++)
					{
						string text = bindingList[i].GetValue() ?? "";
						if (i < cachedBindingValues.Count)
						{
							flag |= !string.Equals(cachedBindingValues[i], text, StringComparison.Ordinal);
							cachedBindingValues[i] = text;
						}
						else
						{
							flag = true;
							cachedBindingValues.Add(text);
						}
					}
				}
				using (pmBuildAttributeValue.Auto())
				{
					if (flag)
					{
						string text2 = sourceText;
						if (bindingList.Count == 1 && text2.Equals(bindingList[0].SourceText, StringComparison.Ordinal))
						{
							text2 = cachedBindingValues[0] ?? "";
						}
						else
						{
							for (int j = 0; j < bindingList.Count; j++)
							{
								BindingItem bindingItem = bindingList[j];
								text2 = text2.Replace(bindingItem.SourceText, cachedBindingValues[j]);
							}
						}
						cachedResultValue = text2;
					}
				}
				using (pmParseAttribute.Auto())
				{
					string text3 = cachedResultValue;
					if (text3.Contains("{cvar("))
					{
						text3 = BindingsManager.ReplaceCVars(text3);
					}
					try
					{
						if (!parsersDetected)
						{
							if (ParsingMethodCache.Instance.TryGetParsingDelegate(View, attributeName, out var _parsingDelegate) && _parsingDelegate.TryGetDelegateForSourceType(typeof(string), out parsingDelegateView))
							{
								parsingDelegateView(View, text3);
							}
							if (parsingDelegateView == null && View.Controller != null && ParsingMethodCache.Instance.TryGetParsingDelegate(View.Controller, attributeName, out _parsingDelegate) && _parsingDelegate.TryGetDelegateForSourceType(typeof(string), out parsingDelegateController))
							{
								parsingDelegateController(View.Controller, text3);
							}
							if (parsingDelegateView == null && parsingDelegateController == null)
							{
								View.ParseAttributeViewAndController(attributeName, text3);
							}
							parsersDetected = true;
						}
						else if (parsingDelegateView != null)
						{
							parsingDelegateView(View, text3);
						}
						else if (parsingDelegateController != null)
						{
							parsingDelegateController(View.Controller, text3);
						}
						else
						{
							View.ParseAttributeViewAndController(attributeName, text3);
						}
					}
					catch (Exception e)
					{
						Log.Error("[XUi] Exception parsing result of binding. Attribute: '" + attributeName + "', binding string: '" + sourceText + "', binding result: '" + text3 + "', view hierarchy: " + View.GetXuiHierarchy() + ":");
						Log.Exception(e);
					}
				}
			}
		}
	}
}
