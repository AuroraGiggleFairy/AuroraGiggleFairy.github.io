using System;

public abstract class BindingItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum BindingTypes
	{
		Always,
		Once,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static readonly char cvarFormatSplitChar = ':';

	[PublicizedFrom(EAccessModifier.Protected)]
	public static readonly char[] cvarFormatSplitCharArray = new char[1] { cvarFormatSplitChar };

	[PublicizedFrom(EAccessModifier.Protected)]
	public string FieldName;

	public readonly string SourceText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController DataContext;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BindingTypes BindingType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string CurrentValue = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BindingItem(string _sourceText)
	{
		SourceText = _sourceText;
		FieldName = _sourceText.Substring(1, _sourceText.Length - 2);
	}

	public abstract string GetValue(bool _forceAll = false);

	[PublicizedFrom(EAccessModifier.Protected)]
	public string ParseCVars(string _fullText)
	{
		for (int num = _fullText.IndexOf("{cvar(", StringComparison.Ordinal); num != -1; num = _fullText.IndexOf("{cvar(", num, StringComparison.Ordinal))
		{
			string text = _fullText.Substring(num, _fullText.IndexOf('}', num) + 1 - num);
			string text2 = "";
			int num2 = text.IndexOf('(') + 1;
			string text3 = text.Substring(num2, text.IndexOf(')') - num2);
			if (text3.IndexOf(cvarFormatSplitChar) >= 0)
			{
				string[] array = text3.Split(cvarFormatSplitCharArray);
				text3 = array[0];
				text2 = array[1];
			}
			_fullText = _fullText.Replace(text, XUiM_Player.GetPlayer().GetCVar(text3).ToString(text2));
		}
		return _fullText;
	}
}
