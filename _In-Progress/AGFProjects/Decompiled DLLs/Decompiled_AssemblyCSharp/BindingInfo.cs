using System;
using System.Collections.Generic;

public class BindingInfo
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView view;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string propertyName;

	public readonly string SourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingItem> bindingList = new List<BindingItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> cachedBindingValues = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResultValue;

	public BindingInfo(XUiView _view, string _property, string _sourceText)
	{
		view = _view;
		propertyName = _property;
		SourceText = _sourceText;
		int num = SourceText.IndexOf("{", StringComparison.Ordinal);
		while (num != -1)
		{
			int num2 = SourceText.IndexOf("}", num, StringComparison.Ordinal);
			if (num2 == -1)
			{
				break;
			}
			string text = SourceText.Substring(num, num2 - num + 1);
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
				BindingItem item = (text.StartsWith("{cvar(") ? new BindingItemCvar(this, view, text) : ((!text.StartsWith("{#")) ? ((BindingItem)new BindingItemStandard(this, view, text)) : ((BindingItem)new BindingItemNcalc(this, view, text))));
				bindingList.Add(item);
			}
			num = SourceText.IndexOf("{", num2, StringComparison.Ordinal);
		}
	}

	public void RefreshValue(bool _forceAll = false)
	{
		bool flag = cachedResultValue == null;
		for (int i = 0; i < bindingList.Count; i++)
		{
			string value = bindingList[i].GetValue(_forceAll);
			if (i < cachedBindingValues.Count)
			{
				flag |= !string.Equals(cachedBindingValues[i], value, StringComparison.Ordinal);
				cachedBindingValues[i] = value;
			}
			else
			{
				flag = true;
				cachedBindingValues.Add(value);
			}
		}
		if (flag)
		{
			string text = SourceText;
			if (bindingList.Count == 1 && text.Equals(bindingList[0].SourceText, StringComparison.Ordinal))
			{
				text = cachedBindingValues[0] ?? "";
			}
			else
			{
				for (int j = 0; j < bindingList.Count; j++)
				{
					BindingItem bindingItem = bindingList[j];
					text = text.Replace(bindingItem.SourceText, cachedBindingValues[j]);
				}
			}
			cachedResultValue = text;
		}
		try
		{
			view.ParseAttributeViewAndController(propertyName, cachedResultValue, view.Controller.Parent, _allowBindingCreation: false);
		}
		catch (Exception e)
		{
			Log.Error("[XUi] Exception parsing result of binding. Attribute: '" + propertyName + "', binding string: '" + SourceText + "', binding result: '" + cachedResultValue + "', view hierarchy: " + view.Controller.GetXuiHierarchy() + ":");
			Log.Exception(e);
		}
	}
}
